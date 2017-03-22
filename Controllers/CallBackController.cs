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

        // RNGCryptoServiceProvider is thread safe in .NET 3.5 and above
        // .NET 3.0 and below will need locking to protect access
        private static readonly RNGCryptoServiceProvider random =
            new RNGCryptoServiceProvider();
        private int length;

        public /*virtual*/ string GenerateNonce(int length)
        {
            var data = new byte[length];
            random.GetNonZeroBytes(data);
            string nonceData = Convert.ToBase64String(data);
            return nonceData;

        }


        public class OAuthParameters
        {
            public string RedirectUrl { get; set; }

            public string ClientId { get; set; }
            public string ClientSecret { get; set; }

            protected string NormalizeParameters(SortedDictionary<string, string> parameters)
            {
                StringBuilder sb = new StringBuilder();

                var i = 0;
                foreach (var parameter in parameters)
                {
                    if (i > 0)
                        sb.Append("&");

                    sb.AppendFormat("{0}={1}", parameter.Key, parameter.Value);

                    i++;
                }

                return sb.ToString();
            }

            private string GenerateBase(string nonce, string timeStamp, Uri url)
            {
                var parameters = new SortedDictionary<string, string>
        {
            {"oauth_consumer_key", ClientId},
            {"oauth_signature_method", "HMAC-SHA1"},
            {"oauth_timestamp", timeStamp},
            {"oauth_nonce", nonce},
            {"oauth_version", "1.0"}
        };

                var sb = new StringBuilder();
                sb.Append("GET");
                sb.Append("&" + Uri.EscapeDataString(url.AbsoluteUri));
                sb.Append("&" + Uri.EscapeDataString(NormalizeParameters(parameters)));
                return sb.ToString();
            }

            public string GenerateSignature(string nonce, string timeStamp, Uri url)
            {
                var signatureBase = GenerateBase(nonce, timeStamp, url);
                var signatureKey = string.Format("{0}&{1}", ClientId, "");
                var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signatureKey));
                return Convert.ToBase64String(hmac.ComputeHash(new ASCIIEncoding().GetBytes(signatureBase)));
            }
        }
        public string GetAuthorizationUrl(string Url)
        {
            var sb = new StringBuilder();
            var nonce = Guid.NewGuid().ToString();
            var timeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString();
            var signature = parameters.GenerateSignature(nonce, timeStamp, new Uri(Url));

            sb.Append(GenerateQueryStringOperator(sb.ToString()) + "oauth_consumer_key=" + Uri.EscapeDataString(parameters.ClientId));
            sb.Append("&oauth_nonce=" + Uri.EscapeDataString(nonce));
            sb.Append("&oauth_timestamp=" + Uri.EscapeDataString(timeStamp));
            sb.Append("&oauth_signature_method=" + Uri.EscapeDataString("HMAC-SHA1"));
            sb.Append("&oauth_version=" + Uri.EscapeDataString("1.0"));
            sb.Append("&oauth_signature=" + Uri.EscapeDataString(signature));

            return Url + sb.ToString();
        }

        private string GenerateQueryStringOperator(string currentUrl)
        {
            if (currentUrl.Contains("?"))
                return "&";
            else
                return "?";
        }

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

            ViewBag.status = "";
            ViewBag.date = "";

            int userId = Int32.Parse(Request.QueryString["userid"]); //todo: Find out how to assign the real user id from OAuth call

            string nonceData = GenerateNonce(length);

            string json = await GetActivityAsync(ViewBag.Response, userId, consumerKey, nonceData);


            return View(json);
        }

        public async Task<string> GetActivityAsync(string dlc_content, int userId, string oauthConsumerKey, string nonceData)
        {
           
            string url = $"https://wbsapi.withings.net/v2/measure?action=getactivity&userId={userId}&oauth_consumer_key={oauthConsumerKey}&oauth_nonce={nonceData}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());

            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Method = "Get";


            using (WebResponse response = await request.GetResponseAsync())
            {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string responseBody = streamReader.ReadToEnd();

                    return responseBody;
                }
            }
        }

       


    }   
}