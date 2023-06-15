using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Zoom_Cooperation.Format
{
    class S2STokenFormat
    {
        #region Response
        /// <summary>
        /// Response Format
        /// </summary>
        [DataContract]
        public class Response
        {
            [DataMember]
            public string access_token { get; set; }
            [DataMember]
            public string token_type { get; set; }
            [DataMember]
            public int expires_in { get; set; }
            [DataMember]
            public string scope { get; set; }
                        
        }
        #endregion Response
    }
}
