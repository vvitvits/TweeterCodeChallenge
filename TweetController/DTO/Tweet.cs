using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TweetController
{
    [DataContract(Namespace = "")]
    public class Tweet
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string UserScreenName { get; set; }

        [DataMember]
        public string ProfileImageUrl { get; set; }

        [DataMember]
        public String Content { get; set; }

        [DataMember]
        public string TweetDate { get; set; }

        [DataMember]
        public int RetweetCount { get; set; }

        [DataMember]
        public string MediaUrl { get; set; }
    }
}
