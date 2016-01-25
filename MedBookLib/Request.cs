using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace MedBookLib
{
    public static class Request
    {
        public static HttpWebRequest POSTRequest(string uri, CookieContainer cookieContainer,
            Dictionary<string, string> dataDictionary)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);
            var boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.CookieContainer = cookieContainer;
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/534.30 (KHTML, like Gecko) Chrome/12.0.742.113 Safari/534.30";

            var byteArray =
                Encoding.Default.GetBytes(PostMultiString.WriteMultipartForm(boundary, dataDictionary));
            request.ContentLength = byteArray.Length;
            request.GetRequestStream().Write(byteArray, 0, byteArray.Length);

            return request;
        }

        public static HttpWebRequest POSTRequest(string uri, CookieContainer cookieContainer,
            NameValueCollection dataDictionary)
        {
            var postString = PostMultiString.ConstructQueryString(dataDictionary);
            var postBytes = Encoding.ASCII.GetBytes(postString);

            var webRequest = (HttpWebRequest) WebRequest.Create(uri);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = postBytes.Length;
            webRequest.CookieContainer = cookieContainer;

            var postStream = webRequest.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();

            return webRequest;
        }

        public static HttpWebRequest GETRequest(string uri, CookieContainer cookieContainer = null)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);

            if (cookieContainer != null)
                request.CookieContainer = cookieContainer;

            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/534.30 (KHTML, like Gecko) Chrome/12.0.742.113 Safari/534.30";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Language", "en");
            request.KeepAlive = true;
            request.AllowAutoRedirect = false;
            request.Method = "GET";
            request.AllowAutoRedirect = true;

            return request;
        }
    }
}