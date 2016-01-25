using System.IO;
using System.Net;
using System.Text;

namespace MedBookLib
{
    public static class Response
    {
        public static string GetResponseString(HttpWebRequest request)
        {
            using (var response = (HttpWebResponse) request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) return null;
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }

        public static string GetResponseString(HttpWebResponse response)
        {
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) return null;
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }

        public static byte[] GetResponseBytes(HttpWebRequest request)
        {
            using (var response = (HttpWebResponse) request.GetResponse())
            using (var responseStream = response.GetResponseStream())
                return responseStream == null ? null : PostMultiString.ReadFully(responseStream);
        }

        public static CookieCollection GetResponseCookies(HttpWebRequest request)
        {
            using (var response = (HttpWebResponse) request.GetResponse())
                return response.Cookies;
        }

        public static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            return (HttpWebResponse) request.GetResponse();
        }
    }
}