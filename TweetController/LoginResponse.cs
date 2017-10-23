
using System.Runtime.Serialization;

namespace TweetController
{
    [DataContract(Namespace = "")]
    public class LoginResponse
    {
        [DataMember]
        public bool Success;

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
