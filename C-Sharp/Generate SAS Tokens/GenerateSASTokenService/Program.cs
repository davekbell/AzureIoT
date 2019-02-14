using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GenerateSASTokenService
{

    class Program
    {
        static void Main(string[] args)
        {

            string uriToUse = "{Target URI e.g. {iot hub name}.azure-devices.net/devices";
            string keyToUse = "{Device or Policy Key}";
            string policyToUse = "If Device then null else Policy name e.g. registryRead";
            int tokenLifeSpan = /*{tokenLifeSpan of token in seconds}*/

            string sasToken = GenerateSasToken(uriToUse,keyToUse,policyToUse, tokenLifeSpan);
            Console.WriteLine(sasToken);

        }


        public static string GenerateSasToken(string resourceUri, string key, string policyName, int expiryInSeconds = 3600)
        {
            TimeSpan fromEpochStart = DateTime.UtcNow - new DateTime(1970, 1, 1);
            string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + expiryInSeconds);

            string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;

            HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(key));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            string token = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}", WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry);

            if (!String.IsNullOrEmpty(policyName))
            {
                token += "&skn=" + policyName;
            }

            return token;
        }
    }
}