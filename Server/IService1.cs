using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using TweetController;

namespace Server
{
    [ServiceContract]
    public interface IService1
    {

        [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle= WebMessageBodyStyle.Bare, UriTemplate = "GetTweets?filter={filter}")]
        List<Tweet> GetTweets(string filter=null);

        [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "Login")]
        LoginResponse Login();
    }

}
