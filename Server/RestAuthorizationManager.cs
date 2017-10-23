using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Web;

namespace Server
{
    public class RestAuthorizationManager : System.ServiceModel.ServiceAuthorizationManager
    {
        /// <summary>  
        /// Method source sample taken from here: http://bit.ly/1hUa1LR  
        /// </summary>  
        protected override bool CheckAccessCore(System.ServiceModel.OperationContext operationContext)
        {
            if (WebOperationContext.Current.IncomingRequest.Method == "OPTIONS") {
                return true;
            }
            //Extract the Authorization header, and parse out the credentials converting the Base64 string:  
            var authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
            if ((authHeader != null) && (authHeader != string.Empty) && authHeader.Length>6) //Todo: refactor into a method
            {
                var svcCredentials = System.Text.ASCIIEncoding.ASCII
                    .GetString(Convert.FromBase64String(authHeader.Substring(6)))
                    .Split(':');
                var user = new
                {
                    Name = svcCredentials[0],
                    Password = svcCredentials[1]
                };
                if ((user.Name == "testuser" && user.Password == "testpassword"))
                {
                    //User is authrized and originating call will proceed  
                    return true;
                }
                else
                {
                    //not authorized  
                    FailAuthentication();
                }
            }
            else
            {
                FailAuthentication();
            }
            return false;
        }

        private void FailAuthentication() {
              //No authorization header was provided, so challenge the client to provide before proceeding:  
                WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"MyWCFService\"");
                //Throw an exception with the associated HTTP status code equivalent to HTTP status 401  
                throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}