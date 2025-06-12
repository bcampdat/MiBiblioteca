namespace MiBiblioteca.Middlewares
{
    public class BloquearPostMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment env;

        public BloquearPostMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            this.next = next;
            this.env = env;
        }

        // Invoke o InvokeAsync
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var IP = httpContext.Connection.RemoteIpAddress.ToString();
            //if (IP == "::1" && httpContext.Request.Method == "POST") // Bloquearía las peticiones de una IP
            //{
            //    httpContext.Response.StatusCode = 400;
            //    httpContext.Response.ContentType = "text/plain";
            //    await httpContext.Response.WriteAsync("No se permiten peticiones POST");
            //    return;
            //}  comentado para permitir post durante el curso
            var ruta = httpContext.Request.Path.ToString();

            var path = $@"{env.ContentRootPath}\wwwroot\log.txt";
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine($@"{IP} - {ruta} - {DateTime.Now} - {httpContext.Request.Method}" );
            }

            await next(httpContext);
        }
    }


}


