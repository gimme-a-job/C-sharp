using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using Microsoft.IdentityModel.Tokens;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using Org.BouncyCastle.Crypto.Parameters;
using System.Runtime.Serialization.Json;
using OIDC.Format;
using System.Text.RegularExpressions;
using System.Reflection;
using Org.BouncyCastle.OpenSsl;

using Microsoft.IdentityModel.JsonWebTokens;


public partial class Default
    {
        #region 定数

        #region ログ出力用定数
        /* for logging */
        /// <summary>
        /// 画面ID
        /// </summary>
        private const string ScreenId = "Default";

        /// <summary>
        /// 操作ログ出力(備考1)
        /// </summary>
        private const string LogRemarks1_LoginSuccess = "CM_TOPへログインしました。";
        private const string LogRemarks1_OIDCError = "レスポンスヘッダにエラーが含まれています。";
        private const string LogRemarks1_StateError = "不正なリクエストを検知しました。：S";
        private const string LogRemarks1_NonceError = "不正なリクエストを検知しました。：N";
        private const string LogRemarks1_PubKeyIsNullOrEmpty = "公開鍵の取得に失敗しました。";
        private const string LogRemarks1_InvalidSignature = "署名の検証に失敗しました。";
        private const string LogRemarks1_PayloadIsNull = "Payloadが取得されていません。";
        private const string LogRemarks1_DecodeError = "認証情報の取得に失敗しました。";
        private const string LogRemarks1_ResponceLengthError = "Response Length is 0 or Less.";
        private const string LogRemarks1_PythonResIsNull = "Python内で例外が発生した可能性があります。";
        private const string LogRemarks1_LstUserIdIsNull = "ユーザーIDの取得に失敗しました。";
        private const string LogRemarks1_UserIdCnt0 = "未登録、またはドメイン未参加のユーザです。";
        private const string LogRemarks1_UserIdCnt2orMore = "{0}人のユーザが該当しています。";

        /// <summary>
        /// ログに出力する文字列
        /// </summary>
        private const string LogRemarks2_LoginId = "ログインID:{0}";
        private const string LogRemarks2_ErrorDescription = "error_description:{0}";
        private const string LogRemarks2_ResponseState = "response_state:{0}";
        private const string LogRemarks2_ResponseNonce = "response_nonce:{0}";
        private const string LogRemarks2_IdToken = "id_token:{0}";
        private const string LogRemarks2_PublicKey = "public_key:{0}";
        private const string LogRemarks2_xAmznOidcData = "x-amzn-oidc-data:{0}";
        private const string LogRemarks2_PythonException = "python_exception:{0}";
        private const string LogRemarks2_SearchKey = "search_key:{0}";
        #endregion ログ出力用定数

        /* データベース */
        /// <summary>
        /// 任意のカラムをキーにしてアカウントIDを取得するプロシージャ
        /// </summary>
        private const string ProcedureGetUserByDsgCol = "PR_CM_GET_UserByDsgCol";
        /// <summary>
        /// 入力したメールアドレスに一致するアカウントIDを取得するプロシージャ
        /// </summary>
        private const string ProcedureGetUserInfoIdByMail = "PR_MN_GET_UserInfoByMail";

        /// パラメータ(指定のカラム)
        /// </summary>
        private const string ParameterColName = "@pCOL_NAME";
        /// パラメータ(任意の値)
        /// </summary>
        private const string ParameterValue = "@pValue";
        /// <summary>
        /// パラメータ(メールアドレス)
        /// </summary>
        private const string ParameterMailAddress = "@pMAIL_ADDRESS";

        /* 画面制御 */
        /// <summary>
        /// トップ
        /// </summary>

        /// <summary>
        /// ログイン
        /// </summary>

        /// <summary>
        /// ログアウト
        /// </summary>

        /* Web.config */
        /// <summary>
        /// Default.aspxアクセス時の認証モード
        /// </summary>
        private const string AuthMode = "Single_Sign-On_AuthenticationMode";
        /// <summary>
        /// 署名の検証をするかどうか
        /// </summary>
        private const string isVerifySig = "Single_Sign-On_VerifySignature";


        /// <summary>
        /// 列挙型
        /// </summary>
        private enum AUTH_MODE
        {
            /// <summary>
            /// 設定無し
            /// </summary>
            NONE = 0,
            /// <summary>
            /// AD認証
            /// </summary>
            AD = 1,
            /// <summary>
            /// OIDC(Microsoftアカウント)
            /// </summary>
            OIDC = 2,
            /// <summary>
            /// ALB-OIDC(ALB経由、Microsoftアカウント)
            /// </summary>
            ALBOIDC = 3
        }

        /// <summary>
        /// JWTのデコードに使う言語を選択するモード
        /// </summary>
        private enum JWT_MODE
        {
            /// <summary>
            /// C#
            /// </summary>
            Csharp = 1,
            /// <summary>
            /// Python
            /// </summary>
            Python = 2
        }

        #region Python関係
        /// <summary>
        /// Pythonの呼び方を選択するモード
        /// </summary>
        private enum PY_MODE
        {
            /// <summary>
            /// IronPython - CreateRuntime()
            /// </summary>
            IronRuntime = 1,
            /// <summary>
            /// IronPython - CreateEngine()
            /// </summary>
            IronEngine = 2,
            /// <summary>
            /// Process Start
            /// </summary>
            ProcessStart = 3
        }

        /// <summary>
        /// .py内で例外が発生したかどうかをチェック
        /// </summary>
        /// </summary>
        private enum ResponseStatus
        {
            NG = 0,
            OK = 1
        }
        #endregion Python関係

        /// <summary>
        /// デバッグかどうかの判定用
        /// </summary>
        private const bool isDebug =
#if DEBUG
            true;
#else
false;
#endif

        #endregion 定数

        protected void Page_Load(object sender, EventArgs e)
        {
            // 認証モード
            var mode = AUTH_MODE.NONE;
            try { mode = (AUTH_MODE)Enum.Parse(typeof(AUTH_MODE), ConfigurationManager.AppSettings[AuthMode]); }
            #region 認証モード取得時のエラー処理
            catch (ArgumentNullException ex)
            {
                // Web.configから設定値が取得出来ない場合は初期値のままにする

                // イベントビューアにエラーログ出力

            }
            catch (ArgumentException ex)
            {
                // Web.configの設定値がおかしい場合は初期値のままにする

                // イベントビューアにエラーログ出力

            }
            catch (Exception ex)
            {
                // イベントビューアにエラーログ出力


                // システムエラー画面に遷移する
                //Response.Redirect(エラーページ);
            }
            #endregion 認証モード取得時のエラー処理

            string strUserId = "";
            string strArea_Id = string.Empty;
            string strUserName = string.Empty;
            Boolean isLogined = false;
            String strMessage = null;

            #region ログインチェック
            // ログインチェック

            try
            {
                #region 未ログイン
                // ログイン済みでないなら
                if (string.IsNullOrEmpty(
                    "セッションにユーザIDがセットされているかチェック"
                    ))
                {

                    switch (mode)
                    {
                        #region AD認証
                        // AD認証
                        case AUTH_MODE.AD:

                            strUserId = Page.User.Identity.Name;

                            if (!(string.IsNullOrEmpty(strUserId)))
                            {

                                // ユーザの存在有無
                                strUserName =
                                    "ADからユーザIDを取得してセッションにセット"
                                    ;

                                if ((string.IsNullOrEmpty(strUserName)) == true)
                                {
                                    isLogined = false;
                                }
                                else
                                {
                                    // 各種初期設定を行う
                                    SetSetting();
                                }
                            }
                            else
                            {
                                isLogined = false;
                            }
                            break;
                        #endregion AD認証


                        #region OIDC(Microsoftアカウント)
                        // MicrosoftアカウントでのOIDC
                        case AUTH_MODE.OIDC:

                            #region ヘッダにOIDCのエラーがあるかチェック
                            // OIDCエラーチェック
                            var err = Request.Params.GetValues("error");

                            #region OIDCエラー検出
                            // OIDCエラーなら
                            if (err != null)
                            {
                                var error_description = Request.Params.GetValues("error_description");

                                // エラーメッセージの設定
                                strMessage = $"error: {err[0]}<br>" +
                                    $"error_description: {error_description[0]}";
                                isLogined = false;

                                // イベントビューアにエラーログ出力
                                //f{エラーログを吐くメソッド } (
                                //    "セッションのユーザID",
                                //    ScreenId,
                                //    ScreenId + "/" + MethodBase.GetCurrentMethod().Name,
                                //    new Exception(LogRemarks1_OIDCError + ":" + err[0])
                                //    );

                                // ログ出力
                                // f{DBへのログ出力用メソッド}(
                                // DateTime.Now,
                                // Enum.GetName(typeof(AUTH_MODE), mode) + "Login",
                                // "セッションからユーザIDを取得",
                                // ScreenId + "/" + MethodBase.GetCurrentMethod().Name,
                                // LogRemarks1_OIDCError + ":" + err[0],
                                // string.Format(LogRemarks2_ErrorDescription, error_description[0]),
                                // "セッションからユーザIDを取得"
                                // );

                            }
                            #endregion OIDCエラー検出

                            #region OIDCエラー未検出(正常時)
                            // OIDCエラーが出てないなら
                            else
                            {
                                // OIDCのIDトークンを取得しているかチェック
                                var id_token = Request.Params.GetValues("id_token");

                                #region ログイン済み
                                // IDトークン取得できたなら
                                if (id_token != null)
                                {
                                    #region stateチェック
                                    // stateチェック
                                    var state = Request.Params.GetValues("state");

                                    #region stateが正しい
                                    // 正しいstateなら
                                    //if (state[0].Equals(Session["Session_State"]))
                                    if (state[0].Equals(GetValueOrDefault(Request.Cookies["Cookie_State"])))
                                    {
                                        var token = id_token[0];
                                        var jsonWebToken = new JsonWebToken(token);

                                        #region nonceチェック
                                        // nonceチェック
                                        var nonce = jsonWebToken.Claims.First(claim => claim.Type == "nonce").Value;

                                        #region nonceが正しい
                                        // nonceも正しいなら
                                        //if (nonce.Equals(Session["Session_Nonce"]))
                                        if (nonce.Equals(GetValueOrDefault(Request.Cookies["Cookie_Nonce"])))
                                        {
                                            #region テスト用(Email固定)
                                            //var email = jwtSecurityToken.Claims.First(claim => claim.Type == "email").Value;

                                            //// メールアドレスでMST_USERを検索して、見つかった数によって処理を分岐
                                            //SetUserByMail(email);
                                            #endregion テスト用(Email固定)

                                            var keyName = "(どこからかクレーム名の初期値を取得する)";

                                            var searchKey = jsonWebToken.Claims.First(claim => claim.Type == keyName).Value;

                                            // 指定したカラムに対し任意の値でMST_USERを検索して、見つかった数によって処理を分岐
                                            SetUserByDsgCol(keyValue: searchKey, domainUserOnly: true);
                                        }
                                        #endregion nonceが正しい

                                        #region nonceが不正
                                        // stateは正しいがnonceがおかしいなら
                                        else
                                        {
                                            // エラーメッセージの設定
                                            strMessage = LogRemarks1_NonceError;
                                            isLogined = false;

                                            // イベントビューアにエラーログ出力


                                            // DBにログ出力

                                        }
                                        #endregion nonceが不正

                                        #endregion nonceチェック
                                    }
                                    #endregion stateが正しい

                                    #region stateが不正
                                    // stateが正しくない、おかしい
                                    else
                                    {
                                        // エラーメッセージの設定
                                        strMessage = LogRemarks1_StateError;
                                        isLogined = false;

                                        // イベントビューアにエラーログ出力


                                        // DBにログ出力

                                    }
                                    #endregion stateが不正

                                    #endregion stateチェック

                                }
                                #endregion ログイン済み

                                #region 未ログインのためOffice365のログイン画面へ
                                // IDトークン取得なし→未ログインなのでログイン画面に飛ばす
                                else
                                {
                                    // Microsoftアカウントのログイン画面に飛ばす
                                    isLogined = false;
                                    RedirectToO365();
                                    return;

                                }
                                #endregion 未ログインのためOffice365のログイン画面へ
                            }
                            #endregion OIDCエラー未検出(正常時)

                            #endregion ヘッダにOIDCのエラーがあるかチェック

                            break;

                        #endregion OIDC(Microsoftアカウント)


                        #region ALB-OIDC(ALB経由、Microsoftアカウント)

                        case AUTH_MODE.ALBOIDC:

                            #region ヘッダにALBのレスポンスがあるかチェック
                            var encoded_jwt = Request.Headers.GetValues("x-amzn-oidc-data");

                            #region ログイン済み
                            if (encoded_jwt != null)
                            {

                                var decoder = JWT_MODE.Csharp;
                                //var decoder = JWT_MODE.Python;

                                switch (decoder)
                                {
                                    #region C#でやるなら
                                    case JWT_MODE.Csharp:


                                        #region 検証ありかどうかをWeb.configから取得(初期値：検証する)

                                        bool verify;
                                        // Web.configからの値の取得に失敗した場合は、True：検証する
                                        if (!Boolean.TryParse(ConfigurationManager.AppSettings[isVerifySig], out verify)) verify = true;

                                        var jsonWebToken = getJsonWebToken(
                                        rawToken: encoded_jwt[0],
                                        isLogined: ref isLogined,
                                        strMessage: ref strMessage,
                                        verify: verify
                                        );

                                        #endregion

                                        if (jsonWebToken != null)
                                        {
                                            #region テスト用(Email固定)
                                            //var email = payload.Claims.First(claim => claim.Type == "email").Value;

                                            //// メールアドレスでMST_USERを検索して、見つかった数によって処理を分岐
                                            //SetUserByMail(email);
                                            #endregion テスト用(Email固定)

                                            var keyName = "(どこからかクレーム名の設定値を取得する)";

                                            var searchKey = jsonWebToken.Claims.First(claim => claim.Type == keyName).Value;

                                            // 指定したカラムに対し任意の値でMST_USERを検索して、見つかった数によって処理を分岐
                                            SetUserByDsgCol(keyValue: searchKey, domainUserOnly: true);
                                        }
                                        else
                                        {
                                            isLogined = false;
                                            strMessage += LogRemarks1_PayloadIsNull + "<br>";

                                            // エラーログ出力


                                            // ログ出力

                                        }

                                        break;
                                    #endregion C#

                                    #region Pythonを組み込んだ場合
                                    case JWT_MODE.Python:

                                        var py_mode = PY_MODE.ProcessStart;

                                        #region (没)IronPython用.pyファイルのパス
                                        var path_mod_iron = @"D:\～\Py_Module\IronPython\module_jwt.py";
                                        #endregion

                                        // Pythonのインストール先
                                        var path_python = @"C:\Program Files\Python311";

                                        switch (py_mode)
                                        {
                                            #region (没)にほんごのさいと(https://t19488sns.com/csharp_project-add-python/1508/) No module named 'base64' で動かない → unsupported operand type(s) for |: 'type' and 'type' で動かない
                                            case PY_MODE.IronRuntime:


                                                ScriptRuntime py = IronPython.Hosting.Python.CreateRuntime();

                                                //ココで実行
                                                Console.WriteLine("[Pythonファイル実行]");
                                                dynamic script = py.UseFile(path_mod_iron);

                                                var payloadRT = script.getPayload(encoded_jwt[0]);

                                                break;
                                            #endregion


                                            #region (没)IronPythonの公式みたいなのとか(https://documentation.help/IronPython/getting_started.html) unsupported operand type(s) for |: 'type' and 'type' で動かない
                                            case PY_MODE.IronEngine:
                                                var engine = IronPython.Hosting.Python.CreateEngine();

                                                var searchPaths = engine.GetSearchPaths();
                                                searchPaths.Add(path_python + @"\Lib");
                                                engine.SetSearchPaths(searchPaths);

                                                var scope = engine.CreateScope();
                                                var source = engine.CreateScriptSourceFromFile(path_mod_iron);
                                                source.Execute(scope);

                                                var getPayload = scope.GetVariable<Func<string, object>>("getPayload");
                                                var payloadEN = getPayload(encoded_jwt[0]);

                                                break;
                                            #endregion IronPythonとか


                                            #region processstartだか("https://www.fenet.jp/dotnet/column/language/8209/")
                                            case PY_MODE.ProcessStart:

                                                //下記のPythonスクリプトへのファイルパスを記述する
                                                string path_module = @"D:\～\Py_Module\ProcessStart\module_jwt.py";
                                                if (!System.IO.File.Exists(path_module))
                                                {
                                                    isLogined = false;

                                                    // その他記述
                                                    throw new FileNotFoundException(path_module + "が見つかりません。");

                                                    //return;
                                                }


                                                var pyProcess = new Process
                                                {
                                                    //StartInfo = new ProcessStartInfo("python.exe")
                                                    StartInfo = new ProcessStartInfo(@path_python + @"\python.exe")
                                                    {
                                                        UseShellExecute = false,
                                                        RedirectStandardOutput = true,
                                                        Arguments = path_module + " " + Regex.Unescape(encoded_jwt[0]) // Pythonへの引数は、"\\"を"\"に置き換えてエスケープの解除してから渡す(重要)
                                                    }
                                                };

                                                pyProcess.Start();
                                                StreamReader stream = pyProcess.StandardOutput;
                                                string py_res = stream.ReadLine();
                                                pyProcess.WaitForExit();
                                                pyProcess.Close();

                                                Console.WriteLine("Value received from script: " + py_res);

                                                if (!String.IsNullOrEmpty(py_res))
                                                {
                                                    using (var msRes = new MemoryStream(Encoding.UTF8.GetBytes(py_res)))
                                                    {
                                                        if (py_res.Length > 0)
                                                        {
                                                            var ser = new DataContractJsonSerializer(typeof(O365OIDCFormat.Response));
                                                            var convertData = (O365OIDCFormat.Response)ser.ReadObject(msRes);
                                                            msRes.Close();

                                                            switch (convertData.ResponseStatus)
                                                            {
                                                                case (byte?)ResponseStatus.OK:

                                                                    #region テスト用(email固定)
                                                                    //var email = convertData.payload.email; // テスト用
                                                                    //SetUserByMail(email); // テスト用
                                                                    #endregion テスト用(email固定)

                                                                    // クレームおよびDatamemberプロパティの名前を取得
                                                                    var keyName = "(使うクレーム名の設定値を取得する)";

                                                                    // ペイロードから値の取得
                                                                    var searchKey = convertData.Payload.getDataMemberByName(keyName);

                                                                    bool domainOnly = true;

                                                                    // 指定したカラムに対し任意の値でMST_USERを検索して、見つかった数によって処理を分岐
                                                                    SetUserByDsgCol(searchKey.ToString(), domainOnly);

                                                                    break;

                                                                case (byte?)ResponseStatus.NG:
                                                                default:
                                                                    strMessage = LogRemarks1_DecodeError + "<br><br>"
                                                                        + string.Format(LogRemarks2_PythonException, (object)convertData.Exception) + "<br>";
                                                                    isLogined = false;

                                                                    // イベントビューアにエラーログ出力
                                                                    

                                                                    // DBにログ出力
                                                                    

                                                                    break;

                                                            }
                                                        }
                                                        else
                                                        {
                                                            msRes.Close();
                                                            strMessage = LogRemarks1_DecodeError + "<br><br>";
                                                            isLogined = false;

                                                            // イベントビューアにエラーログ出力
                                                            

                                                            // DBにログ出力
                                                            
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    strMessage = LogRemarks1_DecodeError + "<br><br>";
                                                    isLogined = false;

                                                    // イベントビューアにエラーログ出力
                                                    

                                                    // DBにログ出力
                                                    ;
                                                }

                                                break;

                                                #endregion


                                        }


                                        break;
                                    #endregion Python

                                    default:
                                        // とりあえず何もしない
                                        isLogined = false;
                                        break;

                                }

                            }
                            #endregion ログイン済み

                            #region 未ログインのためOffice365(ALB経由)のログイン画面へ
                            // ALBのヘッダ取得なし→未ログインなのでログイン画面に飛ばす
                            else
                            {
                                isLogined = false;

#pragma warning disable CS0162 // 到達できないコードが検出されました
                                if (!isDebug) RedirectToALBO365(); // リリース時はオン
#pragma warning restore CS0162 // 到達できないコードが検出されました

                            }
                            #endregion 未ログインのためOffice365(ALB経由)のログイン画面へ

                            #endregion ヘッダにALBのレスポンスがあるかチェック

                            break;

                        #endregion ALB-OIDC(ALB経由、Microsoftアカウント)


                        #region その他
                        // 現状ここには来ない予定
                        default:
                            // 何もしない
                            isLogined = false;
                            break;
                            #endregion その他

                    }

                }
                #endregion 未ログイン

                #region 既にログイン済み
                // ログイン済み
                else
                {
                    if (isDebug) Session.Clear(); // テスト時
#pragma warning disable CS0162 // 到達できないコードが検出されました
                    else isLogined = true; // 暫定本番
#pragma warning restore CS0162 // 到達できないコードが検出されました

                }

                #endregion #region 既にログイン済み

            }
            catch (Exception ex)
            {
                // イベントビューアにエラーログ出力

                // システムエラー画面に遷移する
                //Response.Redirect(システムエラー画面);
            }

            #endregion ログインチェック

            // エラーメッセージがあれば表示
            ShowMessage(strMessage);

            if (isLogined == true)
            {
                // トップ画面へ遷移
                //Response.Redirect(トップ画面);
                return;
            }

            #region ローカル関数

            /// <summary>
            /// 任意のカラムをキーにMST_USERを検索して、見つかった数によって処理を分岐
            /// </summary>
            void SetUserByDsgCol(string keyValue, bool domainUserOnly = true)
            {
                try
                {

                    // SYS_PARAMETERからカラム名を取得
                    var keyColumn = "MAIL_ADDRESS"; // テスト用
                    //var keyColumn = 
                    //    // 任意の値をどこからか取得する
                    //    ;

                    // ユーザの存在有無
                    var lstUserId = getUserIdByDsgCol(column: keyColumn, value: keyValue);

                    if (lstUserId == null)
                    {
                        // ここに来ることが有り得るか不明
                        // エラーメッセージの設定
                        strMessage = LogRemarks1_LstUserIdIsNull;
                        isLogined = false;

                        // イベントビューアにエラーログ出力

                        // DBにログ出力
                    }
                    else
                    {
                        // ユーザIDはドメインが付いているもののみ残す
                        if (domainUserOnly) lstUserId = lstUserId.Where(userId => userId.Contains(@"\")).ToList();

                        // ユニークか分からないためチェックを入れる
                        switch (lstUserId.Count)
                        {
                            // MST_USERに該当がない
                            case 0:
                                // エラーメッセージの設定
                                strMessage = LogRemarks1_UserIdCnt0;
                                isLogined = false;

                                // イベントビューアにエラーログ出力

                                // DBにログ出力

                                break;

                            // 正常時
                            case 1:
                                strUserId = lstUserId[0];

                                // 各種初期設定を行う
                                SetSetting();


                                // DBにログ出力

                                break;

                            // 任意の値で検索してMST_USERに重複がある場合
                            default:
                                // エラーメッセージの設定
                                strMessage = string.Format(LogRemarks1_UserIdCnt2orMore, lstUserId.Count);
                                isLogined = false;

                                // イベントビューアにエラーログ出力

                                // DBにログ出力

                                break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    // イベントビューアにエラーログ出力


                    // システムエラー画面に遷移する
                    //Response.Redirect(システムエラー画面);
                }
            }

            /// <summary>
            /// メールアドレスでMST_USERを検索して、見つかった数によって処理を分岐
            /// </summary>
            void SetUserByMail(string email)
            {
                // ユーザの存在有無
                var lstUserId = getUserIdByMail(email);

                if (lstUserId == null)
                {
                    // ここに来ることが有り得るか不明
                    // エラーメッセージの設定
                    strMessage = "不明なエラーが発生しました。";
                    isLogined = false;
                }
                else
                {
                    // 現時点ではMST_USERのMAIL_ADDRESSカラムがユニークではないためチェックを入れる
                    switch (lstUserId.Count)
                    {
                        // MST_USERに登録がない
                        case 0:
                            // エラーメッセージの設定
                            strMessage = LogRemarks1_UserIdCnt0;
                            isLogined = false;
                            break;

                        // 正常時
                        case 1:
                            strUserId = lstUserId[0];

                            // 各種初期設定を行う
                            SetSetting();

                            // DBにログ出力

                            break;

                        // MST_USERにメールアドレスの重複がある場合
                        default:
                            // エラーメッセージの設定
                            strMessage = lstUserId.Count + "人のユーザが該当しています。";
                            isLogined = false;
                            break;
                    }
                }
            }

            /// <summary>
            /// 各種初期設定を行う
            /// </summary>
            void SetSetting()
            {

            }

            /// <summary>
            /// Microsoftアカウントのログイン画面に飛ばす
            /// </summary>
            void RedirectToO365()
            {
                var strURL = "";
                var url =
                    //isDebug? 
                    "https://login.microsoftonline.com"
                    //: "どこからか設定値を取得する"
                    ;
                var tenantId =
                    //isDebug? 
                    "(テナントIDをいれる)"
                    //: "どこからか設定値を取得する"
                    ;
                var clientId =
                    //isDebug? 
                    "(クライアントIDをいれる）"
                    //: "どこからか設定値を取得する"
                    ;
                var resType = "id_token%20token";
                var redirectURL =
                    Request.Url.AbsoluteUri
                    ;
                var resMode = "form_post";
                var scope = "openid+profile+email";
                var state = System.Guid.NewGuid().ToString(); ;
                var nonce = System.Guid.NewGuid().ToString(); ;
                strURL = $"{url}/{tenantId}/oauth2/v2.0/authorize?" +
                    $"client_id={clientId}" +
                    $"&response_type={resType}" +
                    $"&redirect_uri={redirectURL}" +
                    $"&response_mode={resMode}" +
                    $"&scope={scope}" +
                    $"&state={state}" +
                    $"&nonce={nonce}";

                // (httpとhttpsが混在するからか)Sessionで上手くいかないので取り敢えずCookieを使ってます、適宜変更してください
                #region state
                // f{セッションをセットするメソッド}("Session_State", state, Session);
                //Session["Session_State"] = state;
                Response.Cookies["Cookie_State"].Value = state;
                #endregion state

                #region nonce
                // f{セッションをセットするメソッド}("Session_Nonce", nonce, Session);
                //Session["Session_Nonce"] = nonce;
                Response.Cookies["Cookie_Nonce"].Value = nonce;
                #endregion nonce

                Response.Redirect(strURL, false);
            }

            /// <summary>
            /// (ALB経由) Microsoftアカウントのログイン画面に飛ばす
            /// </summary>
            void RedirectToALBO365()
            {
                try
                {
                    var strURL = "";
                    string url = "https://{DNSorCNAME}/oauth2/idpresponse";
                    strURL = $"{url}";

                    Response.Redirect(strURL, false);

                }
                catch (Exception ex)
                {
                    // イベントビューアにエラーログ出力


                    // システムエラー画面に遷移する
                    //Response.Redirect(システムエラー画面);
                }
            }

            /// <summary>
            /// エラーメッセージがあれば表示する
            /// </summary>
            void ShowMessage(string msg)
            {
                try
                {
                    // エラーメッセージがあれば表示
                    if (msg != null)
                    {
                        var lblId = "Label_Message";

                        // 初期文字を残すかどうか
                        //this.Controls.Clear();

                        Label lblMsg = new Label();
                        lblMsg.ID = lblId;
                        lblMsg.Text = "<br>" + msg;
                        lblMsg.ForeColor = System.Drawing.Color.Red;
                        this.Controls.Add(lblMsg);

                        // ラベルではなくJSのalertとして表示する場合(こちらは現状上手く動かない)
                        //string jscript = null;
                        //ClientScriptManager cs = Page.ClientScript;
                        //jscript = "\nalert (\"" + msg + "\");\n";
                        //cs.RegisterStartupScript(this.GetType(), "messages", jscript, true);
                        //ScriptManager.RegisterStartupScript(FindControl(lblId), FindControl(lblId).GetType(), "messages", jscript, true);

                    }

                }
                catch (Exception ex)
                {
                    // イベントビューアにエラーログ出力


                    // システムエラー画面に遷移する
                    //Response.Redirect(システムエラー画面);
                }

            }

            /// <summary>
            /// Cookieの値を取得する
            /// </summary>
            /// <returns>nullかCookieの値を返す</returns>
            string GetValueOrDefault(HttpCookie cookie)
            {
                if (cookie == null)
                {
                    return "";
                }
                return cookie.Value;
            }

            #endregion ローカル関数


        }


        #region Jwtデコード

        /// <summary>
        /// JWTトークンからJsonWebTokenを返す
        /// </summary>
        /// <param name="rawToken">JWTトークン</param>
        /// <param name="isLogined">ログイン済みかどうか</param>
        /// <param name="strMessage">エラーメッセージ格納</param>
        /// <param name="verify">署名の検証を行うかどうか</param>
        /// <returns></returns>
        private JsonWebToken getJsonWebToken(string rawToken, ref bool isLogined, ref string strMessage, bool verify = true)
        {
            JsonWebToken jsonWebToken = null;

            try
            {
                jsonWebToken = new JsonWebToken(PadTrim(rawToken));

                #region 検証ありの場合
                // 検証フラグが立っているなら。
                if (verify)
                {

                    #region Step 1: Get the key id from JWT headers (the kid field)

                    var kid = jsonWebToken.Kid;

                    #endregion Step 1: Get the key id from JWT headers (the kid field)


                    #region Step 2: Get the public key from regional endpoint
                    var pub_key = "";
                    var region = "ap-northeast-1"
                    var url = $"https://public-keys.auth.elb.{region}.amazonaws.com/{kid}";

                    //"https://araramistudio.jimdo.com/2020/11/26/c-%E3%81%A7https%E3%81%AEweb%E3%83%9A%E3%83%BC%E3%82%B8%E3%82%92%E3%83%80%E3%82%A6%E3%83%B3%E3%83%AD%E3%83%BC%E3%83%89%E3%81%99%E3%82%8B/"より
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (WebClient webClient = new WebClient())
                    {
                        using (System.IO.Stream stream = webClient.OpenRead(url))
                        {
                            // "https://vdlz.xyz/Csharp/Porpose/WebTool/WebClient/OpenRead.html"より
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                pub_key = sr.ReadToEnd();
                            }
                        }
                    }

                    #region 試験用に使う公開鍵を設定(AWSで使用している暗号鍵が分からないため、AWS無しの環境では自前のトークンに合わせた公開鍵が必要)
                    // 取得したpub_keyはテスト時に使えないため
                    pub_key = isDebug ? "-----BEGIN PUBLIC KEY-----\n" +
                        "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEEVs/o5+uQbTjL3chynL4wXgUg2R9\n" +
                        "q9UU8I5mEovUf86QZ7kOBIjJwqnzD1omageEHWwHdBO6B+dFabmdT9POxg==\n" +
                        "-----END PUBLIC KEY-----\n" : pub_key;

                    #endregion テスト用公開鍵の設定                                                           

                    #endregion Step 2: Get the public key from regional endpoint


                    #region Step 3: Get the payload（payloadの検証のみ）

                    #region 公開鍵が取得されている場合
                    if (!String.IsNullOrEmpty(pub_key))
                    {

                        #region 検証成功
                        // トークンの形式によっては失敗の可能性あり
                        //if (SignatureValidator(
                        //    argRawToken: rawToken, 
                        //    argPubKey: pub_key
                        //    ))
                        #region 有料ライブラリを使える場合
                        ChilkatJwt chilkatJwt = new ChilkatJwt(argToken: token, argPubKey: pub_key, isDecode: false);
                        if (chilkatJwt.sigVerified)
                        #endregion Chilkat
                        {
                            isLogined = true;
                            return jsonWebToken;
                        }
                        #endregion 検証成功

                        #region 検証失敗
                        else
                        {
                            isLogined = false;
                            strMessage = LogRemarks1_InvalidSignature + "<br>";

                            // イベントビューアにエラーログ出力


                            // DBにログ出力

                        }
                        #endregion 検証失敗

                    }
                    #endregion 公開鍵が取得されている場合

                    #region 公開鍵が空文字
                    else
                    {
                        isLogined = false;
                        strMessage = LogRemarks1_PubKeyIsNullOrEmpty + "<br>";

                        // イベントビューアにエラーログ出力

                        // DBにログ出力

                    }
                    #endregion #region 公開鍵が空文字

                    #endregion Step 3: Get the payload（payloadの検証のみ）

                }
                #endregion 検証ありの場合

                #region 検証なしの場合
                // 検証しない場合
                else
                {
                    isLogined = true;
                    return jsonWebToken;
                }
                #endregion 検証なしの場合


            }
            catch (Exception ex)
            {
                // イベントビューアにエラーログ出力


                // DBにログ出力


            }

            isLogined = false;
            return null;

        }

        private class ChilkatJwt
        {
            #region パラメータ

            private Chilkat.PublicKey pubKey = new Chilkat.PublicKey();
            public bool keyLoaded { get; private set; }

            private Chilkat.Jwt jwt = new Chilkat.Jwt();
            private string token { get; }

            public bool sigVerified { get; private set; }

            private int leeway { get; }
            public bool bTimeValid { get; private set; }

            public string payload { get; private set; }
            public bool payloadJsonLoaded { get; private set; }
            public string payloadJson { get; private set; }

            public string joseHeader { get; private set; }
            public bool headerJsonLoaded { get; private set; }
            public string headerJson { get; private set; }

            #endregion パラメータ

            #region コンストラクタ

            // "https://www.example-code.com/csharp/jwt_ecc_verify.asp"より
            public ChilkatJwt(string argToken, string argPubKey, bool isDecode = true, int argLeeway = 60)
            {
                // Demonstrates how to verify an JWT using an ECC public key.

                // This example requires the Chilkat API to have been previously unlocked.
                // See Global Unlock Sample for sample code.

                setKey(argPubKey);

                this.token = argToken;

                this.leeway = argLeeway;

                // First verify the signature.
                VerifySignature();

                // Let's see if the time constraints, if any, are valid.                
                // If the current system time is before the "nbf" time, or after the "exp" time,
                // then IsTimeValid will return false/0.
                // Also, we'll allow a leeway of 60 seconds to account for any clock skew.
                // Note: If the token has no "nbf" or "exp" claim fields, then IsTimeValid is always true.
                TimeValidation();

                // Now let's recover the original claims JSON (the payload).
                // We can recover the original JOSE header in the same way:
                // We can format for human viewing by loading it into Chilkat's JSON object
                // and emit.
                if (isDecode) Decode();

            }

            #endregion コンストラクタ

            #region 関数
            private void setKey(string argPubKey)
            {
                bool success = this.pubKey.LoadBase64(argPubKey); // 2023.05.18 追加
                this.keyLoaded = success; // 2023.05.19 追加
            }

            private void VerifySignature()
            {
                // First verify the signature.
                this.sigVerified = this.jwt.VerifyJwtPk(this.token, this.pubKey);
                Debug.WriteLine("verified: " + Convert.ToString(this.sigVerified));
            }

            private void TimeValidation()
            {
                // Let's see if the time constraints, if any, are valid.                
                // If the current system time is before the "nbf" time, or after the "exp" time,
                // then IsTimeValid will return false/0.
                // Also, we'll allow a leeway of 60 seconds to account for any clock skew.
                // Note: If the token has no "nbf" or "exp" claim fields, then IsTimeValid is always true.
                this.bTimeValid = this.jwt.IsTimeValid(this.token, this.leeway);
                Debug.WriteLine("time constraints valid: " + Convert.ToString(this.bTimeValid));
            }

            private void Decode()
            {
                // Now let's recover the original claims JSON (the payload).
                getPayload();
                // The payload will likely be in compact form:

                // We can format for human viewing by loading it into Chilkat's JSON object
                // and emit.
                GetJsonFrmPayload();

                // We can recover the original JOSE header in the same way:
                getJoseHeader();
                // The payload will likely be in compact form:

                // We can format for human viewing by loading it into Chilkat's JSON object
                // and emit.
                GetJsonFrmHeader();
            }

            private void getPayload()
            {
                // Now let's recover the original claims JSON (the payload).
                this.payload = this.jwt.GetPayload(this.token);
                // The payload will likely be in compact form:
                Debug.WriteLine(this.payload);
            }

            private void GetJsonFrmPayload()
            {
                // We can format for human viewing by loading it into Chilkat's JSON object
                // and emit.
                Chilkat.JsonObject json = new Chilkat.JsonObject();
                bool success = json.Load(payload);
                this.payloadJsonLoaded = success; // 2023.05.19 追加
                json.EmitCompact = false;
                this.payloadJson = json.Emit(); // 2023.05.19 追加
                Debug.WriteLine(this.payloadJson);
            }


            private void getJoseHeader()
            {
                // We can recover the original JOSE header in the same way:
                this.joseHeader = this.jwt.GetHeader(this.token);
                // The payload will likely be in compact form:
                Debug.WriteLine(this.joseHeader);
            }

            private void GetJsonFrmHeader()
            {
                // We can format for human viewing by loading it into Chilkat's JSON object
                // and emit.
                Chilkat.JsonObject json = new Chilkat.JsonObject();
                bool success = json.Load(this.joseHeader);
                this.headerJsonLoaded = success; // 2023.05.19 追加
                json.EmitCompact = false;
                this.headerJson = json.Emit(); // 2023.05.19 追加
                Debug.WriteLine(this.headerJson);
            }


            #endregion 関数

        }


        #endregion jwtデコード

        #region jwt書式              

        #region 余計な文字を省く
        // 参考"https://stackoverflow.com/questions/4140723/how-to-remove-new-line-characters-from-a-string"
        private string PadTrim(string token, bool rmHeader = true, bool rmPayload = true, bool rmSignature = true)
        {
            var sep = '.';
            var token_S = token.Split(sep);

            if (rmHeader) Formatter(ref token_S[0]);
            if (rmPayload) Formatter(ref token_S[1]);
            if (rmSignature) Formatter(ref token_S[2]);

            var result = string.Join(sep.ToString(), token_S);

            return result;


            void Formatter(ref string @string)
            {
                @string = RemoveSpecialCharacters(Regex.Replace(@string, @"\\a|\\b|\\f|\\n|\\r|\\t|\\v|\\0", String.Empty));
            }

        }
        #endregion 余計な文字を省く

        // 参考"https://stackoverflow.com/questions/1120198/most-efficient-way-to-remove-special-characters-from-string"
        private string RemoveSpecialCharacters(string str)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in str)
            {
                //if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '.' || c == '_' || c == '/' || (c >= '+' && c <= '-')) // (没)'+'と'/'も受け付けない模様
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '.' || c == '_' || c == '-') // (採用版)'+'と'/'も受け付けない模様
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        #endregion jwt書式

        #region 公開鍵やRSA周り

        /// <summary>
        /// 署名が有効かどうかをチェックする
        /// </summary>
        /// <param name="argRawToken">JWTトークン(取得時のままを想定)</param>
        /// <param name="argPubKey">ログイン済みかどうか</param>
        /// <param name="bTimeValid">ValidateLifeTimeを行うかどうか</param>
        /// <returns>署名が有効かどうか</returns>
        private bool SignatureValidator(string argRawToken, string argPubKey, bool bTimeValid = true)
        {
            ECDsa ecdsa = LoadPublicKey(Convert.FromBase64String(getTrimmedPubKey(argPubKey)));

            // 参考"https://github.com/awslabs/aws-alb-identity-aspnetcore"
            var validationParams = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new ECDsaSecurityKey(ecdsa),
                //IssuerSigningKey = ConvertPemToSecurityKey(pub_key), // こちらでも良い
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = bTimeValid,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            var tokenValidationResult = new JsonWebTokenHandler().ValidateToken(argRawToken, validationParams);

            #region 検証失敗の場合
            if (!tokenValidationResult.IsValid)
            {
                // イベントビューアにエラーログ出力

                // DBへログ出力
            }
            #endregion 検証失敗の場合

            return tokenValidationResult.IsValid;
        }

        // https://stackoverflow.com/questions/59211413/validate-jwt-es256-token-with-public-key-in-c-sharp
        private ECDsa LoadPublicKey(byte[] key)
        {
            byte[] pubKeyX = key.Skip(27).Take(32).ToArray();
            byte[] pubKeyY = key.Skip(59).Take(32).ToArray();
            return ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = pubKeyX,
                    Y = pubKeyY
                }
            });
        }

        /// <summary>
        /// 取得した公開鍵を、扱い易い様にトリムする
        /// </summary>
        /// <param name="argRawPubKey">JWTトークン(取得時のままを想定)</param>
        /// <returns>トリムした公開鍵を返す</returns>
        private string getTrimmedPubKey(string argRawPubKey)
        {
            var begin = @"-----BEGIN PUBLIC KEY-----";
            var end = @"-----END PUBLIC KEY-----";

            return argRawPubKey = Regex.Replace(argRawPubKey, $@"\n|{begin}|{end}", String.Empty); // 本番用
        }

        private static ECDsaSecurityKey ConvertPemToSecurityKey(string pem)
        {
            using (TextReader publicKeyTextReader = new StringReader(pem))
            {
                var ec = (ECPublicKeyParameters)new PemReader(publicKeyTextReader).ReadObject();
                var ecpar = new ECParameters
                {
                    Curve = ECCurve.NamedCurves.nistP256,
                    Q = new ECPoint
                    {
                        X = ec.Q.XCoord.GetEncoded(),
                        Y = ec.Q.YCoord.GetEncoded()
                    }
                };

                return new ECDsaSecurityKey(ECDsa.Create(ecpar));
            }
        }

        #endregion

        #region ユーザIDの取得

        /// <summary>
        /// 指定したカラムからユーザIDを取得する
        /// </summary>
        /// <returns>(複数該当する可能性があるため)コレクション型で返す</returns>
        private List<string> getUserIdByDsgCol(string column, string value)
        {
            var strret = new List<string>();

            using (SqlConnection connection
               = new SqlConnection(
                   // 接続文字列
                   ))
            {
                using (SqlCommand command = new SqlCommand(ProcedureGetUserByDsgCol, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        // パラメータの設定
                        // 指定のカラム
                        SqlParameter parameterColName = command.Parameters.Add(ParameterColName, SqlDbType.NVarChar, 64); // 適宜数字は変更願います
                        parameterColName.Value = column;
                        // 任意の値
                        SqlParameter parameterValue = command.Parameters.Add(ParameterValue, SqlDbType.NVarChar, 256); // 適宜数字は変更願います
                        parameterValue.Value = value;

                        // DB検索
                        connection.Open();

                        SqlDataReader sqlReader = command.ExecuteReader();
                        if (sqlReader.HasRows)
                        {
                            while (sqlReader.Read())
                            {
                                strret.Add(sqlReader["EMPLOYEE_UID"].ToString());
                            }
                        }
                        connection.Close();
                        return strret;

                    }
                    catch (SqlException sqlexception)
                    {
                        throw sqlexception;
                    }
                }
            }
        }

        /// <summary>
        /// メールアドレスからユーザIDを取得する
        /// </summary>
        /// <returns>(複数該当する可能性があるため)コレクション型で返す</returns>
        private List<string> getUserIdByMail(string email)
        {
            var strret = new List<string>();

            using (SqlConnection connection
               = new SqlConnection(
                   // 接続文字列
                   ))
            {
                using (SqlCommand command = new SqlCommand(ProcedureGetUserInfoIdByMail, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        // パラメータの設定
                        // メールアドレス
                        SqlParameter parameterMail = command.Parameters.Add(ParameterMailAddress, SqlDbType.NVarChar, 128);
                        parameterMail.Value = email;

                        // DB検索
                        connection.Open();

                        SqlDataReader sqlReader = command.ExecuteReader();
                        if (sqlReader.HasRows)
                        {
                            while (sqlReader.Read())
                            {
                                strret.Add(sqlReader["EMPLOYEE_UID"].ToString());
                            }
                        }
                        connection.Close();
                        return strret;

                    }
                    catch (SqlException sqlexception)
                    {
                        throw sqlexception;
                    }
                }
            }
        }

        #endregion ユーザIDの取得

        #region SQL



        #endregion SQL

    }