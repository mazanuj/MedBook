namespace MedBookLib
{
    public static class MultiFormData
    {
        public static string GetMultiFormData(string key, string value, string boundary)
        {
            var output = "--" + boundary + "\r\n";
            output += "Content-Disposition: form-data; name=\"" + key + "\"\r\n\r\n";
            output += value + "\r\n";
            return output;
        }
    }
}