using System.Net;
using Zoom_Cooperation.Control;
using Zoom_Cooperation.Format.Meeting;

namespace Zoom_Cooperation.Request
{
    /// <summary>
    /// users/{userId}/meetings : Create Meeting(POST) API
    /// </summary>
    public class CreateMeeting
    {
        /// <summary>
        /// APIのURL(共通)
        /// </summary>
        private string ApiUrl { get; set; }

        /// <summary>
        /// Server-to-Server認証用のURL
        /// </summary>
        private string S2sUrl { get; set; }
        /// <summary>
        /// Server-to-Server認証用のClientID
        /// </summary>
        private string S2sCltId { get; set; }
        /// <summary>
        /// Server-to-Server認証用のClientSecret
        /// </summary>
        private string S2sCltScrt { get; set; }

        /// <summary>
        /// 要求に対するレスポンス結果
        /// </summary>
        public RestApiControl.RESPONSE Response { get; set; }
        /// <summary>
        /// 要求に対するレスポンスコード
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; set; }
        /// <summary>
        /// エラーの詳細
        /// </summary>
        public Format.CommonFormat.ErrorResponse ErrorResponseDetail { get; set; }

        /// <summary>
        /// コンストラクタ(Server-to-Server OAuth)
        /// </summary>
        /// <param name="argBaseUrl">基本URL(共通)</param>
        /// <param name="argS2sUrl">認証URL(S2S)</param>
        /// <param name="argAcctId">AccountID(S2S)</param>
        /// <param name="argCltId">ClientID(S2S)</param>
        /// <param name="argCltScrt">ClientSecret(S2S)</param>
        /// <param name="argParam">パラメータ</param>
        public CreateMeeting(string argBaseUrl, string argS2sUrl, string argAcctId, string argCltId, string argCltScrt, CreateMeetingFormat.Parameters argParam)
        {
            ApiUrl = $"{argBaseUrl}/users/{argParam.p_userId}/meetings";
            S2sUrl = $"{argS2sUrl}?grant_type=account_credentials&account_id={argAcctId}";
            S2sCltId = argCltId;
            S2sCltScrt = argCltScrt;
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="argRequest">リクエスト</param>
        /// <returns></returns>
        public CreateMeetingFormat.Response Execute(CreateMeetingFormat.Requst argRequest)
        {
            using (RestApiControl api = new RestApiControl())
            {
                // 初期設定
                api.SetSetting(ApiUrl, RestApiControl.METHOD.Post);               

                // S2Sトークン作成
                using (S2STokenControl s2s = new S2STokenControl())
                {
                    // 初期設定                 
                    s2s.SetSetting(S2sUrl, S2STokenControl.METHOD.Post, S2sCltId, S2sCltScrt);

                    // Server-to-Serverトークン作成
                    string s2sToken = $"Bearer {s2s.GetAuthorizeToken()}";
                    api.AddHeader("Authorization", s2sToken);
                }

                // 送信
                var res = api.SendRequest<CreateMeetingFormat.Requst, CreateMeetingFormat.Response>(argRequest);
                // 送信結果のステータス
                Response = api.ResponseStatus;
                ResponseStatusCode = api.ResponseStatusCode;
                if (Response == RestApiControl.RESPONSE.NG)
                {
                    ErrorResponseDetail = api.ErrorResponseDetail;
                }
                return res;
            }
        }
    }
}
