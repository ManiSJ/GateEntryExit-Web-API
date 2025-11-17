using System.Net;
using System.Text;

namespace GateEntryExit.Middlewares
{
    public class HttpContextMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Enable buffering so the body can be read muliple times
            context.Request.EnableBuffering();

            string requestBody;
            using (var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // rewind
            }

            // --- Capture Response Body ---
            var originalResponseBodyStream = context.Response.Body;
            await using var tempResponseBodyStream = new MemoryStream();
            context.Response.Body = tempResponseBodyStream;

            //var anyService = context.RequestServices.GetRequiredService<IanyService>();

            await _next.Invoke(context);

            // --- Read Response Body ---
            tempResponseBodyStream.Seek(0, SeekOrigin.Begin);
            string responseBOdy = await new StreamReader(tempResponseBodyStream).ReadToEndAsync();
            if(context.Response.StatusCode == (int)HttpStatusCode.OK)
            {

            }
            else
            {

            }

            // --- Copy back to original response stream ---
            tempResponseBodyStream.Seek(0, SeekOrigin.Begin);
            await tempResponseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
        }
    }
}
