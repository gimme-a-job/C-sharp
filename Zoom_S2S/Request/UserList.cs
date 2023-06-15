using Zoom_Cooperation.Control;
using Zoom_Cooperation.Format.Users;

namespace Zoom_Cooperation.Request
{
    /// <summary>
    /// users/List Userts(Get) API
    /// </summary>
    public class UserList
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
        /// コンストラクタ(Server-to-Server OAuth)
        /// </summary>
        /// <param name="argBaseUrl">基本URL(共通)</param>
        /// <param name="argS2sUrl">認証URL(S2S)</param>
        /// <param name="argAcctId">AccountID(S2S)</param>
        /// <param name="argCltId">ClientID(S2S)</param>
        /// <param name="argCltScrt">ClientSecret(S2S)</param>
        /// <param name="argQParam">Query パラメータ</param>
        public UserList(string argBaseUrl, string argS2sUrl, string argAcctId, string argCltId, string argCltScrt, UserListFormat.QueryParameters argQParam)
        {
            ApiUrl = $"{argBaseUrl}/users?{nameof(argQParam.status)}={argQParam.status}&{nameof(argQParam.page_size)}={argQParam.page_size}&{nameof(argQParam.page_number)}={argQParam.page_number}";
            S2sUrl = $"{argS2sUrl}?grant_type=account_credentials&account_id={argAcctId}";
            S2sCltId = argCltId;
            S2sCltScrt = argCltScrt;
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="argRetStatus">結果</param>
        /// <returns></returns>
        public UserListFormat.Response Execute(out RestApiControl.RESPONSE argRetStatus)
        {
            argRetStatus = RestApiControl.RESPONSE.NG;

            using (RestApiControl api = new RestApiControl())
            {
                // 初期設定
                api.SetSetting(ApiUrl, RestApiControl.METHOD.Get);

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
                var res = api.SendRequest<UserListFormat.Requst, UserListFormat.Response>(null);
                // 送信結果のステータス
                argRetStatus = api.ResponseStatus;

                return res;
            }
        }
    }
}
