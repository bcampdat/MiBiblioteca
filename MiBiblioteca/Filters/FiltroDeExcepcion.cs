using Microsoft.AspNetCore.Mvc.Filters;

namespace MiBiblioteca.Filters
{
    public class FiltroDeExcepcion : ExceptionFilterAttribute
    {
        private readonly ILogger<FiltroDeExcepcion> logger;
        private readonly IWebHostEnvironment env;

        public FiltroDeExcepcion(ILogger<FiltroDeExcepcion> logger, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);
            var IP = context.HttpContext.Connection.RemoteIpAddress.ToString();
            var ruta = context.HttpContext.Request.Path.ToString();
            var path = $@"{env.ContentRootPath}\wwwroot\logErrores.txt";
            var metodo = context.HttpContext.Request.Method;
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine($@"{IP} - {DateTime.Now} - {ruta} - {metodo}");
            }

            base.OnException(context);
        }
    }

}
