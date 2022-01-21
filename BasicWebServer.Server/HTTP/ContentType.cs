namespace BasicWebServer.Server.HTTP
{
    public class ContentType
    {
        public const string PlainText = "text/plain; charset=UTF-8";
        
        public const string Html = "text/html; charset=UTF-8";

        // a constant for the form content type, as we will need it to parse the form
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";
    }
}
