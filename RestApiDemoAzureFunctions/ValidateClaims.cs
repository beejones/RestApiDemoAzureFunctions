using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Formatting;

namespace RestApiDemoAzureFunctions
{
    public static class ValidateClaims
    {
        [FunctionName("ValidateClaims")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

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
            // For test purposes we just check the givenname
            if (string.Compare("ronny", givenName, true) == 0)
            {
                log.Info($"Validation passed for {givenName} {surName}");
                return req.CreateResponse(HttpStatusCode.OK, "Validated");
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
    }
}
