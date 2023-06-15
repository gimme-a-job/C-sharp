using System.Net;
using Zoom_Cooperation.Control;
using Zoom_Cooperation.Format.Meeting;

namespace Zoom_Cooperation.Request
{
    /// <summary>
    /// meetings/{meetingId} : Delete Meeting(DELETE) API
    /// </summary>
    public class DeleteMeeting
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
        /// <param name="argQParam">Query パラメータ</param>
        public DeleteMeeting(string argBaseUrl, string argS2sUrl, string argAcctId, string argCltId, string argCltScrt, DeleteMeetingFormat.Parameters argParam, DeleteMeetingFormat.QueryParameter argQParam = null)
        {
            ApiUrl = $"{argBaseUrl}/meetings/{argParam.p_meetingId}";
            if (argQParam != null)
            {
                string query = "";
                if (string.IsNullOrEmpty(argQParam.occurrence_id) == false)
                {
                    query = $"{nameof(argQParam.occurrence_id)}={argQParam.occurrence_id}";
                }
                if (argQParam.schedule_for_reminder != null)
                {
                    if (query != "")
                    {
                        query += "&";
                    }
                    query += $"{nameof(argQParam.schedule_for_reminder)}={argQParam.schedule_for_reminder}";
                }
                if (string.IsNullOrEmpty(argQParam.cancel_meeting_reminder) == false)
                {
                    if (query != "")
                    {
                        query += "&";
                    }
                    query += $"{nameof(argQParam.cancel_meeting_reminder)}={argQParam.cancel_meeting_reminder}";
                }
                if (query != "")
                {
                    ApiUrl += "?" + query;
                }
            }
            S2sUrl = $"{argS2sUrl}?grant_type=account_credentials&account_id={argAcctId}";
            S2sCltId = argCltId;
            S2sCltScrt = argCltScrt;
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <returns></returns>
        public DeleteMeetingFormat.Response Execute()
        {
            using (RestApiControl api = new RestApiControl())
            {
                // 初期設定
                api.SetSetting(ApiUrl, RestApiControl.METHOD.Delete);

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
                var res = api.SendRequest<DeleteMeetingFormat.Requst, DeleteMeetingFormat.Response>(null);
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
