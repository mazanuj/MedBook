using System.Net;

namespace MedBookLib
{
    public static class Cookies
    {
        public static CookieContainer GetCookiesContainer(string path, out string cookieResp, out HttpWebRequest request)
        {
            var cookieContainer = new CookieContainer();

            request = Request.GETRequest(path);
            request.CookieContainer = cookieContainer;

            var response = Response.GetResponse(request);
            cookieResp = Response.GetResponseString(response);

            var cookieCollection = response.Cookies;
            cookieContainer.Add(cookieCollection);

            return cookieContainer;
        }
    }
}