using AsyncOAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            

            var client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessTokens);


            //string withingsDateApiUrl = "&date=";

            //string withingsStartDateApiUrl = "&startdateymd=";

            //string withingsEndDateApiUrl = "&enddateymd=";
            //DateTime date = DateTime.Now;
            //string dateFormat = date.ToString("yyyy-MM-dd");
            //string startDateFormat = "2017-03-10";

            //string endDateFormat = "2017-03-21";

            // string dateFormat = "2017-03-13";

            //string oauthenticator = "&"+consumerSecret+"&"+accessToken;
            var oAuth_params = OAuthUtility.BuildBasicParameters(consumerKey, consumerSecret, "https://wbsapi.withings.net", HttpMethod.Get, accessTokens)
                .Where(p => p.Key != "oauth_signature")
                .OrderBy(p => p.Key);
           

            string requestUri = $"https://wbsapi.withings.net/measure?action=getmeas&userid={userId}&";

            requestUri += string.Join("&", oAuth_params.Select(kvp => kvp.Key + "=" + kvp.Value));

            var signature = OAuthUtility.BuildBasicParameters(consumerKey, consumerSecret, requestUri, HttpMethod.Get, accessTokens)
                .First(p => p.Key == "oauth_signature").Value;

            string json = await client.GetStringAsync(requestUri + "&oauth_signature=" + signature);

            var o = JObject.Parse(json);

            int updateTime = (int)o["body"]["updatetime"];

            ViewBag.measureGroups = o["body"]["measuregrps"].Select(no => new
            {
                GroupId = no["grpid"],
                Attrib = no["attrib"]
            });
          
            ViewBag.serializedResult = "JsonData";
            return View("AccessTokenFlow");
           
        }
    }   
}