namespace BasicWebServer.Server.HTTP
{
    public class TextResponse : ContentResponse
    {
        // the second parameter in the constructor --> this is the name of a pre-render action,
        // which will use the request and modify the response before it is returned to the browser.
        public TextResponse(string text)
            : base(text, ContentType.PlainText)
        {
        }
    }
}
