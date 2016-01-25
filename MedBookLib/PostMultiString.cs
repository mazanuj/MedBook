using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace MedBookLib
{
    internal static class PostMultiString
    {
        public static string WriteMultipartForm(string boundary, Dictionary<string, string> dataDictionary)
        {
            var sPostMultiString = string.Empty;
            if (dataDictionary != null)
                sPostMultiString = dataDictionary.Aggregate(sPostMultiString,
                    (current, pair) => current + MultiFormData.GetMultiFormData(pair.Key, pair.Value, boundary));

            sPostMultiString += "--" + boundary + "--\r\n\r\n";
            return sPostMultiString;
        }

        public static string ConstructQueryString(NameValueCollection parameters)
        {
            var sb = new StringBuilder();

            foreach (string name in parameters)
                sb.Append(string.Concat(name, "=", System.Web.HttpUtility.UrlEncode(parameters[name]), "&"));

            return sb.Length > 0 ? sb.ToString(0, sb.Length - 1) : string.Empty;
        }

        public static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16*1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}