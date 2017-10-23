using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using TweetController;

namespace Server
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public List<Tweet> GetTweets(string filter = null)
        {
            return TweetController.Controller.GetTweets(filter);
            //return new List<Tweet>()
            //{
            //    new Tweet() { UserName="User1", UserScreenName="@user1", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 1", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User2", UserScreenName="@user11", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 2", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User3", UserScreenName="@user111", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 3", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User4", UserScreenName="@user112", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 4", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User5", UserScreenName="@user13", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 5", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User6", UserScreenName="@user1454", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 6", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User7", UserScreenName="@user153", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 7", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User8", UserScreenName="@user122", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 8", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User9", UserScreenName="@user1423", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 9", TweetDate=DateTime.Now },
            //    new Tweet() { UserName="User10", UserScreenName="@user1473", ProfileImageUrl="https://photos.app.goo.gl/GUGSJyJ9umz3xrm12", Content="Some Tweet 10", TweetDate=DateTime.Now }
            //};
        }

        public LoginResponse Login()
        {
            return new LoginResponse() {Success = true };
        }
    }
}
