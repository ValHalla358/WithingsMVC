using AsyncOAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WithingsTest.Controllers
{
    public class WithingsController : Controller
    {
        private string consumerSecret= "1f933dcc400ef51f63ffc40b8eeeb429cbbdc53198cf7c4249662c70f403b";
        
        private string consumerKey="8ecb6d91d1ee66d5c3696c0c803197ed9e8cdff332ff40d5f964430ce9d1";
        
        // GET: Withings
        public ActionResult Index()
        {
            return View();
        }
        //public static WithingsClient Create(string consumerKey, string consumerSecret)
        //{
        //    var client = OAuthUtility.CreateOAuthClient("consumerKey", "consumerSecret", new AccessToken("accessToken", "accessTokenSecret"));
        //}

        public async Task<ActionResult> RequestTokenFlow()
        {
            //setting authorizer value to the keyvaluepair and IEnumberable key and secret
             
            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            //Createing a KeyvaluePair for the callBack URL used in step one

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("oauth_callback", Uri.EscapeUriString("http://localhost:49932/CallBack/AccessTokenFlow")));


            // get request token - once url reads http://localhost:49932/Withings/RequestTokenFlow Controller begins with action result HERE

            var tokenResponse = await authorizer.GetRequestToken("https://oauth.withings.com/account/request_token",parameters);
            //Summary - Sends consumerKey and consumerSecret to withings oauth site with parameters of oauth callback valued above

            //setting value of the Token to the requestToken


            var requestToken = tokenResponse.Token;
            //Oauth is hashed and decoded via AsynchOauth and stored as tokenReponse.Token in requestToken variable 
            //adding data to session
            //Store the products to a session

            Session["requestToken"] = requestToken;

         

            //requestUrl buildAuthroizeUrl is putting together a callback url

            var requestUrl = authorizer.BuildAuthorizeUrl("https://oauth.withings.com/account/authorize", requestToken);
            //creating a request url with consumerKey/Secret + withings oauth + tokenResponse.Token

            //Binding View to go to callback URL defined in first step

            ViewBag.RequestUrl = requestUrl;
        
    
        
            return View();
        }
   }
}