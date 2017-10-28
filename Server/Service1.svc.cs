using System.Collections.Generic;
using TweetController;

namespace Server
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public List<Tweet> GetTweets(string filter = null)
        {
            var ConsumerKey = Properties.Settings.Default.twitterConsumerKey;
            var ConsumerSecret = Properties.Settings.Default.twitterConsumerSecret;
            using (var webClient = new TweetController.WebClient())
            {
                var tweetController = new TweetController.Controller(ConsumerKey, ConsumerSecret, webClient);
                return tweetController.GetTweets("@salesforce", 10, filter);
            }
        }

        public LoginResponse Login()
        {
            return new LoginResponse() { Success = true };
        }
    }
}
