// A simple middleware to capture http requests
public class RequestFilter
    {
        private readonly RequestDelegate next;
        public RequestFilter(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // Call error handler class when an error has occurred.
                ErrorHandler.LogExceptionFile(context, ex,true);
            }
        }
        
    }
