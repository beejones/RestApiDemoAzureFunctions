using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Text;
using System.Net.Http.Formatting;

namespace RestApiDemoAzureFunctions
{
    public static class AddClaims
    {
        [FunctionName("AddClaims")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Validate credentials
            if (!AddClaims.ValidateCredentials(req, log))
            {
                return req.CreateResponse(HttpStatusCode.Forbidden, "Validation of authorization header failed");
            }
            // parse query parameter
            string givenName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "givenName", true) == 0)
                .Value;
            string surName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "surName", true) == 0)
                .Value;

            // Get request body
            dynamic body = await req.Content.ReadAsAsync<object>();

            if (body != null)
            {
                givenName = givenName ?? body.givenName;
                surName = surName ?? body.surName;
            }

            // Do the validation of the claims. E.g. check the name in an external database
            if (string.Compare("ronny", givenName, true) == 0)
            {
                log.Info($"Validation passed for {givenName} {surName}");

                // Return additional claims
                var responseContent = new ResponseContent
                {
                    version = "1.0.0",
                    status = (int)HttpStatusCode.OK,
                    city = "Brussels",
                    profession = "engineer"
                };
                return req.CreateResponse<ResponseContent>(
                          HttpStatusCode.OK,
                          responseContent,
                          new JsonMediaTypeFormatter(),
                          "application/json");
            }

            string message = $"The user {givenName} {surName} does not belong to the community";
            log.Info(message);
            return req.CreateResponse<ResponseContent>(
                        HttpStatusCode.Conflict,
                        new ResponseContent
                        {
                            version = "1.0.0",
                            status = (int)HttpStatusCode.Conflict,
                            userMessage = message
                        },
                        new JsonMediaTypeFormatter(),
                        "application/json");
        }

        /// <summary>
        /// Validate the basic authentication credentials
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static bool ValidateCredentials(HttpRequestMessage req, TraceWriter log)
        {
            var authorization = req.Headers.Authorization;

            if (authorization == null)
            {
                log.Info("Authorization failed - no Authorization header found.");
                return false;
            }
            else
                if (string.IsNullOrWhiteSpace(authorization.Parameter))
            {
                log.Info("Authorization failed - no Authorization header found.");
                return false;
            }

            string creds = authorization.Parameter;
            try
            {
                creds = Encoding.UTF8.GetString(Convert.FromBase64String(creds));
            }
            catch (FormatException)
            {
                log.Info("Authorization failed - Credentials badly formatted.");
                return false;
            }

            // Extract credentials and compare with fixed demo values
            int separator = creds.IndexOf(':');
            string clientId = creds.Substring(0, separator);
            string clientSecret = creds.Substring(separator + 1);

            return string.Compare("12345678", clientId, false) == 0 && string.Compare("abcdefg", clientSecret, false) == 0;
        }
    }

    /// <summary>
    /// Class to model responses
    /// </summary>
    public class ResponseContent
    {
        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Gets or sets the http status
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// Gets or sets the user message
        /// </summary>
        public string userMessage { get; set; }

        /// <summary>
        /// Gets or sets the city claim
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// Gets or sets the profession claim
        /// </summary>
        public string profession { get; set; }
    }
}