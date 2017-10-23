using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;

namespace TweetController
{
    public static class Controller
    {
        public static string ToShortDescriptiveString(System.DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return string.Empty;
            }
            else if (date.Year == System.DateTime.Today.Year)
            {
                if (date.DayOfYear == System.DateTime.Today.DayOfYear)
                {
                    return string.Format("Today {0:h:mm tt}", date);
                }
                else if (date.DayOfYear == System.DateTime.Today.DayOfYear - 1)
                {
                    return string.Format("Yesterday {0:h:mm tt}", date);
                }
                return string.Format("{0:MMM %d, h:mm tt}", date);
            }
            return string.Format("{0:MM/dd/yy h:mm tt}", date);
        }
        public static List<Tweet> GetTweets(string filter = null)
        {
            var ConsumerKey = Properties.Settings.Default.twitterConsumerKey;
            var ConsumerSecret = Properties.Settings.Default.twitterConsumerSecret;
            var BearerToken = HttpContext.Current.Server.UrlEncode(ConsumerKey) + ":" + HttpContext.Current.Server.UrlEncode(ConsumerSecret);
            var BearerTokenb64 = Convert.ToBase64String(Encoding.Default.GetBytes(BearerToken));
            using (var WebClient = new WebClient())
            {
                WebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
                WebClient.Headers.Add("Authorization", "Basic " + BearerTokenb64);
                var tokenPayload = WebClient.UploadString("https://api.twitter.com/oauth2/token", "grant_type=client_credentials");
                var rgx = new Regex("\"access_token\"\\s*:\\s*\"([^\"]*)\"");
                // you can store this accessToken and just do the next bit if you want
                var accessToken = rgx.Match(tokenPayload).Groups[1].Value;
                WebClient.Headers.Clear();
                WebClient.Headers.Add("Authorization", "Bearer " + accessToken);

                const string TwitterApiUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json?tweet_mode=extended&screen_name=@salesforce&count=10";
  
                const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";

                var json = WebClient.DownloadString(TwitterApiUrl);
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
                            TweetDate = ToShortDescriptiveString(DateTime.ParseExact(tweet["created_at"], format, System.Globalization.CultureInfo.InvariantCulture)),
                            MediaUrl = (tweet["entities"].ContainsKey("media") && tweet["entities"]["media"][0]["type"]=="photo") ? tweet["entities"]["media"][0]["media_url"] : ""
                        });
                    }
                }
                return result;
            }
        }

    }
}
