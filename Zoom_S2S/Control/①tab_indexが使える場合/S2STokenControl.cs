using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Json;
using Zoom_Cooperation.Format;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Zoom_Cooperation.Control
{
    class S2STokenControl : IDisposable
    {
        #region パラメータ
        /// <summary>
        /// HttpClient
        /// </summary>
        private HttpClient m_httpClient = null;
        /// <summary>
        /// メッセージ
        /// </summary>
        private HttpRequestMessage m_httpMsg = null;

        /// <summary>
        /// ステータスコード
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; set; }

        /// <summary>
        /// 戻り
        /// </summary>
        public RESPONSE ResponseStatus { get; set; }

        /// <summary>
        /// エラーの詳細
        /// </summary>
        public Format.CommonFormat.ErrorResponse ErrorResponseDetail { get; set; }

        public enum RESPONSE
        {
            OK,
            NG
        }
        #endregion パラメータ

        #region 要求種別
        public enum METHOD
        {
            Get,
            Post,
            Patch,
            Delete
        }
        #endregion 要求種別

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public S2STokenControl()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }
        #endregion コンストラクタ

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (m_httpMsg != null)
                {
                    try
                    {
                        m_httpMsg.Dispose();
                    }
                    catch { }
                    m_httpMsg = null;
                }

                if (m_httpClient != null)
                {
                    try
                    {
                        m_httpClient.Dispose();
                    }
                    catch { }
                    m_httpClient = null;
                }
            }
            catch (Exception)
            { }
        }
        #endregion Dispose

        #region 初期設定
        /// <summary>
        /// 初期設定
        /// </summary>
        public void SetSetting(string argUri, METHOD argMethod, string argClientId, string argClientSecret)
        {
            m_httpClient = new HttpClient();
            m_httpMsg = new HttpRequestMessage();

            // URL設定

            // 同時アクセス対応 2023.06.13 追加
            argUri += $"&token_index={getCount()}";

            m_httpMsg.RequestUri = new Uri(argUri);
            switch (argMethod)
            {
                case METHOD.Get:
                    m_httpMsg.Method = HttpMethod.Get;
                    break;
                case METHOD.Post:
                    m_httpMsg.Method = HttpMethod.Post;
                    break;
                case METHOD.Patch:
                    m_httpMsg.Method = new HttpMethod("PATCH");
                    break;
                case METHOD.Delete:
                    m_httpMsg.Method = HttpMethod.Delete;
                    break;
                default:
                    throw new Exception("指定のMethodが不正です");
            }

            // JSONのみ処理可能(未指定だとXMLでレスポンスがくる場合がある)
            AddHeader("Accept", "application/json");
            // Base64形式でエンコード
            var client = argClientId + ":" + argClientSecret;
            var basenc = Convert.ToBase64String(Encoding.UTF8.GetBytes(client));
            AddHeader("Authorization", "Basic "+basenc);
        }

        private int getCount()
        {
            var cnt = 0;

            var dt = GetAccessToken(
                TokenId: "T02"
                );

            int.TryParse(dt.Rows[0]["TOKEN_VALUE"].ToString(), out cnt);

            UpdateAccessToken(
                TokenId: "T02",
                TokenValue: getNextCount(cnt: cnt).ToString()
                );

            return cnt;
        }

        private int getNextCount(int cnt)
        {
            cnt++;

            if (cnt > getMaxCount()) cnt = 0;

            return cnt;
            
        }

        private int getMaxCount()
        {
            var dt = GetAccessToken(
                TokenId:"T03"
                );

            var max = 0;
            int.TryParse(dt.Rows[0]["TOKEN_VALUE"].ToString(), out max);

            return max;
        }

        #region トークン取得

        /// <summary>
        /// アクセストークンを取得します。
        /// </summary>
        /// <param name="TokenId">トークンＩＤ</param>
        /// <returns>パラメータ値</returns>
        /// <remarks>
        /// </remarks>
        private DataTable GetAccessToken(
            string TokenId
            )
        {
            DataTable returnDataTable = new DataTable();

            // 戻り値用
            string returnString = string.Empty;

            // DB接続情報を web.config より取得
            using (SqlConnection sqlConnection
                = new SqlConnection(ConfigurationManager.ConnectionStrings[DBName].ConnectionString))
            using (SqlCommand command = new SqlCommand("Proc_GET_Token", sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    // 各パラメータ設定

                    // 戻り値
                    SqlParameter returnValue = command.Parameters.Add(GetSysParaParaRet, SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    // ＩＤ
                    SqlParameter pTokenId = command.Parameters.Add("@pTokenId", SqlDbType.NVarChar, 4);
                    pTokenId.Direction = ParameterDirection.Input;
                    pTokenId.Value = TokenId;

                    // DB Search
                    sqlConnection.Open();

                    SqlDataReader sqlReader = command.ExecuteReader();
                    returnDataTable.Load(sqlReader, LoadOption.OverwriteChanges);
                    sqlReader.Close();

                    // 正常終了
                    return returnDataTable;
                }
                catch (SqlException sqlexception)
                {
                    // 異常終了
                    throw sqlexception;
                }
                finally
                {
                }
            }
        }

        /// <summary>
        /// トークンを取得します。
        /// </summary>
        /// <param name="TokenId">トークンＩＤ</param>
        /// <returns>パラメータ値</returns>
        /// <remarks>
        /// 列情報: PARAM_NAME, PARAM_VALUE
        /// </remarks>
        private void UpdateAccessToken(
            string TokenId,
            string TokenValue
            )
        {
            DataTable returnDataTable = new DataTable();

            // 戻り値用
            string returnString = string.Empty;

            // 引数が取得できない場合、そのまま返す
            if (string.IsNullOrEmpty(TokenId))
            {
                return;
            }

            // DB接続情報を web.config より取得
            using (SqlConnection sqlConnection
                = new SqlConnection(ConfigurationManager.ConnectionStrings[DBName].ConnectionString))
            using (SqlCommand command = new SqlCommand("Proc_UPD_Token", sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    // 各パラメータ設定

                    // 戻り値
                    SqlParameter returnValue = command.Parameters.Add(GetSysParaParaRet, SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    // ＩＤ
                    SqlParameter pTokenId = command.Parameters.Add("@pTokenId", SqlDbType.NVarChar, 4);
                    pTokenId.Direction = ParameterDirection.Input;
                    pTokenId.Value = TokenId;

                    // 値
                    SqlParameter pTokenValue = command.Parameters.Add("@pTokenValue", SqlDbType.NVarChar, 3);
                    pTokenValue.Direction = ParameterDirection.Input;
                    pTokenValue.Value = TokenValue;

                    // 更新者
                    SqlParameter pUpdateUser = command.Parameters.Add("@pUpdateUserId", SqlDbType.NVarChar, 256);
                    pUpdateUser.Direction = ParameterDirection.Input;
                    pUpdateUser.Value = "Rsrv_Rgstr";

                    // DB Search
                    sqlConnection.Open();

                    command.ExecuteNonQuery();

                }
                catch (SqlException sqlexception)
                {
                    // 異常終了
                    throw sqlexception;
                }
                finally
                {
                }
            }
        }

        #endregion トークン取得

        #endregion 初期設定

        #region ヘッダ制御
        /// <summary>
        /// ヘッダー追加
        /// </summary>
        /// <param name="argKey"></param>
        /// <param name="argValue"></param>
        public void AddHeader(string argKey, string argValue)
        {
            m_httpMsg.Headers.Add(argKey, argValue);
        }
        #endregion ヘッダ制御

        #region Request - Send

        public string GetAuthorizeToken()
        {
            // Posting.  
            Console.WriteLine("Post Request Start");

            // Initialization.  
            ResponseStatus = RESPONSE.NG;
            ErrorResponseDetail = null;
            
            // HTTP POST
            var result = m_httpClient.SendAsync(m_httpMsg);
            var responseResult = result.Result;
            Console.WriteLine($"Request -> Send OK");

            // ステータスコード
            ResponseStatusCode = responseResult.StatusCode;

            // Responseをテキスト形式で取得
            var responseContent = responseResult.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseContent);

            // Verification  
            using (var msRes = new MemoryStream(Encoding.UTF8.GetBytes(responseContent)))
            {
                if (responseResult.IsSuccessStatusCode)
                {
                    // Reading Response.
                    ResponseStatus = RESPONSE.OK;
                    if (responseContent.Length > 0)
                    {
                        var ser = new DataContractJsonSerializer(typeof(S2STokenFormat.Response));
                        var convertData = (S2STokenFormat.Response)ser.ReadObject(msRes);
                        msRes.Close();
                        return convertData.access_token;
                    }
                    else
                    {
                        msRes.Close();
                        return default(string);
                    }
                }
                else
                {
                    var ser = new DataContractJsonSerializer(typeof(Format.CommonFormat.ErrorResponse));
                    ErrorResponseDetail = (Format.CommonFormat.ErrorResponse)ser.ReadObject(msRes);
                    msRes.Close();
                    return default(string);
                }
            }
            
        }

        #endregion Request - POST

    }
}
