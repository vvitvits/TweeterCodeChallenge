﻿using System;
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
    public class Controller : IDisposable
    {
        private string _consumerKey;
        private string _consumerSecret;
        private WebClient _webClient;

        public Controller(string consumerKey, string consumerSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _webClient = new WebClient();
        }

        public void Dispose()
        {
            _webClient?.Dispose();
        }

        private string GetAccessToken() {
            var BearerToken = HttpContext.Current.Server.UrlEncode(_consumerKey) + ":" + HttpContext.Current.Server.UrlEncode(_consumerSecret);
            var BearerTokenb64 = Convert.ToBase64String(Encoding.Default.GetBytes(BearerToken));
            _webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
            _webClient.Headers.Add("Authorization", "Basic " + BearerTokenb64);
            var tokenPayload = _webClient.UploadString("https://api.twitter.com/oauth2/token", "grant_type=client_credentials");
            var rgx = new Regex("\"access_token\"\\s*:\\s*\"([^\"]*)\"");
            // you can store this accessToken and just do the next bit if you want
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

        public List<Tweet> GetTweets(int count, string filter = null)
        {
            var accessToken = GetAccessToken();
            _webClient.Headers.Clear();
            _webClient.Headers.Add("Authorization", "Bearer " + accessToken);

            var TwitterApiUrl =$"https://api.twitter.com/1.1/statuses/user_timeline.json?tweet_mode=extended&screen_name=@salesforce&count={count}";


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
