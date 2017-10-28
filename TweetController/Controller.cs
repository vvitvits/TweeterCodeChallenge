using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace TweetController
{
    public class Controller 
    {
        private string _consumerKey;
        private string _consumerSecret;
        private IWebClient _webClient;
        private const string AuthUrl = "https://api.twitter.com/oauth2/token";
        private const string ApiUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json";

        public Controller(string consumerKey, string consumerSecret, IWebClient webClient)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _webClient = webClient;
        }

        private string GetAccessToken() {
            var BearerToken = HttpContext.Current.Server.UrlEncode(_consumerKey) + ":" + HttpContext.Current.Server.UrlEncode(_consumerSecret);
            var BearerTokenb64 = Convert.ToBase64String(Encoding.Default.GetBytes(BearerToken));
            _webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
            _webClient.Headers.Add("Authorization", "Basic " + BearerTokenb64);
            var tokenPayload = _webClient.UploadString(AuthUrl, "grant_type=client_credentials");
            var rgx = new Regex("\"access_token\"\\s*:\\s*\"([^\"]*)\"");
            // We could cache access tolken to avoid extra call
            return rgx.Match(tokenPayload).Groups[1].Value;
        }

        private string GetTweetDate(dynamic tweet) {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return HelperMethods.ToShortDescriptiveString(DateTime.ParseExact(tweet["created_at"], format, System.Globalization.CultureInfo.InvariantCulture));
        }

        private string GetMediaUrl(dynamic tweet) {
            if (tweet["entities"].ContainsKey("media") && tweet["entities"]["media"][0]["type"] == "photo") {
                return tweet["entities"]["media"][0]["media_url"];
            }
            return null;
        }

        public List<Tweet> GetTweets(string handle, int count, string filter = null)
        {
            var accessToken = GetAccessToken();
            _webClient.Headers.Clear();
            _webClient.Headers.Add("Authorization", "Bearer " + accessToken);

            var TwitterApiUrl =$"{ApiUrl}?tweet_mode=extended&screen_name={handle}&count={count}";


            var json = _webClient.DownloadString(TwitterApiUrl);
            dynamic tweets = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(json);
            var result = new List<Tweet>();
            foreach (dynamic tweet in tweets)
            {
                var TweetContent = tweet["full_text"] as string;
                //Check filter
                if (string.IsNullOrEmpty(filter) || TweetContent.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Add(new Tweet()
                    {
                        UserName = tweet["user"]["name"],
                        UserScreenName = "@" + tweet["user"]["screen_name"],
                        ProfileImageUrl = tweet["user"]["profile_image_url"],
                        Content = TweetContent,
                        RetweetCount = tweet["retweet_count"],
                        TweetDate = GetTweetDate(tweet),
                        MediaUrl = GetMediaUrl(tweet)
                    });
                }
            }
            return result;
        }
    }
}
