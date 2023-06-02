using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace OIDC.Format
{
    public class O365OIDCFormat
    {
        #region パラメータ
        /// <summary>
        /// Path パラメータ
        /// </summary>
        public class Parameters
        {
            
        }
        #endregion パラメータ

        #region Request
        /// <summary>
        /// Requet Format
        /// </summary>
        [DataContract]
        public class Request
        {
            
        }
        #endregion Request

        #region Response
        /// <summary>
        /// Response Format
        /// </summary>
        [DataContract]
        public class Response
        {
            [DataMember(Name = "exception")]
            public string Exception { get; set; }

            [DataMember(Name = "response_status")]
            public byte? ResponseStatus { get; set; }

            [DataMember(Name = "payload")]
            public PayloadInfo Payload { get; set; }

            [DataContract]
            public class PayloadInfo
            {
                [DataMember(Name = "aio")]
                public string Aio { get; set; }

                [DataMember(Name = "aud")]
                public string Application_ID { get; set; }

                [DataMember(Name = "email")]
                public string Email { get; set; }

                [DataMember(Name = "exp")]
                public long? Expires { get; set; }

                [DataMember(Name = "family_name")]
                public string FamilyName { get; set; }

                [DataMember(Name = "given_name")]
                public string GivenName { get; set; }

                [DataMember(Name = "iat")]
                public long? Issued_At { get; set; }

                [DataMember(Name = "iss")]
                public string Issuer { get; set; }

                [DataMember(Name = "name")]
                public string Name { get; set; }

                [DataMember(Name = "nonce")]
                public string Nonce { get; set; }

                [DataMember(Name = "nbf")]
                public long? NotBefore { get; set; }

                [DataMember(Name = "oid")]
                public string OID { get; set; }

                [DataMember(Name = "picture")]
                public string Picture { get; set; }

                [DataMember(Name = "preferred_username")]
                public string Preferred_UserName { get; set; }

                [DataMember(Name = "sub")]
                public string Sub { get; set; }

                [DataMember(Name = "tid")]
                public string TenantID { get; set; }
                
                [DataMember(Name = "ver")]
                public string Version { get; set; }



                // 参考"https://stackoverflow.com/questions/14671507/how-to-get-the-property-that-has-a-datamemberattribute-with-a-specified-name/14671540#14671540"より
                public object getDataMemberByName(string name)
                {
                    return (typeof(PayloadInfo).GetProperties().FirstOrDefault(propertyInfo => propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false)
                                         .OfType<DataMemberAttribute>()
                                         .Any(dataMember => dataMember.Name == name))).GetValue(this);
                }
            }

        }
        #endregion Response
    }
}