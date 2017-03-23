using AsyncOAuth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WithingsTest.Controllers
{
    public class CallBackController : Controller
    {
        private string consumerSecret = "1f933dcc400ef51f63ffc40b8eeeb429cbbdc53198cf7c4249662c70f403b";

        private string consumerKey = "8ecb6d91d1ee66d5c3696c0c803197ed9e8cdff332ff40d5f964430ce9d1";

       
        // GET: CallBack
        public ActionResult Details(string verifier, string Token)
        {

            return View();


        }

        public async Task<ActionResult> AccessTokenFlow()
        {
            //seting the authorizer varable with consumerKey/Secret as in Withings. Will become variable for later injection
            //grab value in URL to place in varables

            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            var accessToken = Request.QueryString["oauth_token"].ToString();
            var oAuthVerifier = Request.QueryString["oauth_verifier"].ToString();
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("oauth_token", accessToken));



            //  List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            //   parameters.Add(new KeyValuePair<string, string>("oauth_verifier", oAuthVerifier));

            var requestToken = Session["requestToken"] as RequestToken;

            //send them out as access_tokens to get access granted by Withings 
            var accessTokenResponse = await authorizer.GetAccessToken("https://oauth.withings.com/account/access_token", requestToken, oAuthVerifier);
            var accessTokens = accessTokenResponse.Token;

            string userId = Request.QueryString["userid"]; //todo: Find out how to assign the real user id from OAuth call
         

            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, new AccessToken("accessToken", "accessTokenSecret"));
            string requestUri = "https://wbsapi.withings.net/v2/measure?action=getactivity/userid= " + userId;
            string json = await client.GetStringAsync(requestUri);
            ViewBag.json = "JsonData";
            return View("AccessTokenFlow");  
        }
    }   
}