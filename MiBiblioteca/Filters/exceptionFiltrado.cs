using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MiBiblioteca.Filters
{
    public class exceptionFiltrado : ExceptionFilterAttribute
    {
        private readonly ILogger<exceptionFiltrado> logger;
        private readonly IWebHostEnvironment env;

        public exceptionFiltrado(ILogger<exceptionFiltrado> logger, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ocurrió un error inesperado.";

            switch (exception)
            {
                case ArgumentNullException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Uno o más argumentos son nulos.";
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Acceso no autorizado.";
                    break;
                case InvalidOperationException:
                    statusCode = HttpStatusCode.Conflict;
                    message = "Operación inválida.";
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = "Recurso no encontrado.";
                    break;
                case NotImplementedException:
                    statusCode = HttpStatusCode.NotImplemented;
                    message = "Operación no implementada.";
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "Ocurrió un error inesperado.";
                    break;
            }

            logger.LogError(exception, exception.Message);

            var path = $@"{env.ContentRootPath}\wwwroot\errores.txt";
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine($"{DateTime.UtcNow}: {exception.Message}");
            }

            context.Result = new ContentResult
            {
                Content = message,
                ContentType = "text/plain",
                StatusCode = (int)statusCode
            };

            base.OnException(context);
        }
    }
}
