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

        #region [SYS_TOKEN]アクセス用定数

        public abstract class TokenId
        {
            public const string Token = "T02";
            public const string Interval = "T03";
        }

        public abstract class Procedure
        {
            public const string GetToken = "Proc_GET_Token";
            public const string UpdateToken = "Proc_UPD_Token";
        }

        public abstract class TableColumn
        {
            public const string TokenValue = "TOKEN_VALUE";
            public const string TokenSaveDate = "TOKEN_SAVE_DATE";
        }
        public abstract class Parameter
        {
            public const string TokenId = "@pTokenId";
            public const string TokenValue = "@pTokenValue";
            public const string UpdateUserId = "@pUpdateUserId";
        }

        #endregion [SYS_TOKEN]アクセス用定数

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
            var savedToken = GetAccessToken(TokenId:TokenId.Token);

            if (isValidToken(token: savedToken)) return savedToken[TableColumn.TokenValue].ToString();
            else
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

                            // Save Token
                            var retToken = convertData.access_token;
                            UpdateAccessToken(
                                TokenId:TokenId.Token, TokenValue:retToken);

                            // Return Token
                            return retToken;

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

        }

        #endregion Request - POST

        #region トークン関係 (Zoom-API 同時アクセス対応) 2023.06.13 追加

        /// <summary>
        /// アクセストークンを取得します。
        /// </summary>
        /// <param name="TokenId">トークンＩＤ</param>
        /// <returns>パラメータ値</returns>
        /// <remarks>
        /// </remarks>
        private DataRow GetAccessToken(
            string TokenId
            )
        {
            DataTable returnDataTable = new DataTable();

            // DB接続情報を web.config より取得
            using (SqlConnection sqlConnection
                = new SqlConnection(ConfigurationManager.ConnectionStrings[DBName].ConnectionString))
            using (SqlCommand command = new SqlCommand(Procedure.GetToken, sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    // 各パラメータ設定

                    // 戻り値
                    SqlParameter returnValue = command.Parameters.Add(GetSysParaParaRet, SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    // ＩＤ
                    SqlParameter pTokenId = command.Parameters.Add(Parameter.TokenId, SqlDbType.NVarChar, 4);
                    pTokenId.Direction = ParameterDirection.Input;
                    pTokenId.Value = TokenId;

                    // DB Search
                    sqlConnection.Open();

                    SqlDataReader sqlReader = command.ExecuteReader();
                    returnDataTable.Load(sqlReader, LoadOption.OverwriteChanges);
                    sqlReader.Close();

                    // 正常終了
                    return returnDataTable.Rows[0];
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
            // 引数が取得できない場合、そのまま返す
            if (string.IsNullOrEmpty(TokenId))
            {
                return;
            }

            // DB接続情報を web.config より取得
            using (SqlConnection sqlConnection
                = new SqlConnection(ConfigurationManager.ConnectionStrings[DBName].ConnectionString))
            using (SqlCommand command = new SqlCommand(Procedure.UpdateToken, sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    // 各パラメータ設定

                    // 戻り値
                    SqlParameter returnValue = command.Parameters.Add(GetSysParaParaRet, SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    // ＩＤ
                    SqlParameter pTokenId = command.Parameters.Add(Parameter.TokenId, SqlDbType.NVarChar, 4);
                    pTokenId.Direction = ParameterDirection.Input;
                    pTokenId.Value = TokenId;

                    // 値
                    SqlParameter pTokenValue = command.Parameters.Add(Parameter.TokenValue, SqlDbType.NVarChar, -1);
                    pTokenValue.Direction = ParameterDirection.Input;
                    pTokenValue.Value = TokenValue;

                    // 更新者
                    SqlParameter pUpdateUser = command.Parameters.Add(Parameter.UpdateUserId, SqlDbType.NVarChar, 256);
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

        /// <summary>
        /// トークンを再取得が必要かどうかをチェックする
        /// </summary>
        /// <returns>
        /// 再取得の必要がなければTrue、必要であればFalseを返す
        /// </returns>
        private bool isValidToken(DataRow token)
        {
            // 前回トークン取得日時
            var savedDate = DateTime.Parse(token[TableColumn.TokenSaveDate].ToString());

            // 取得間隔(秒)、2023.06.13現在の最大は3600(1時間有効)
            var interval = int.Parse(
                GetAccessToken(TokenId: TokenId.Interval)[TableColumn.TokenValue].ToString()
                );

            // 前回トークン取得日時に、任意に設定した取得間隔を足す
            var exp = savedDate.AddSeconds(interval);

            if (DateTime.Now < exp) return true;
            else return false;
        }

        #endregion トークン関係       

    }
}
