using MiBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace MiBiblioteca.Services
{
    public class OperacionesService
    {
        private readonly MiBibliotecaContext context;
        private readonly IHttpContextAccessor accessor;

        public OperacionesService(MiBibliotecaContext context, IHttpContextAccessor accessor)
        {
            this.context = context;
            this.accessor = accessor;
        }

        public async Task AddOperacion(string operacion, string controller)
        {
            Operacione nuevaOperacion = new Operacione()
            {
                FechaAccion = DateTime.Now,
                Operacion = operacion,
                Controller = controller,
                Ip = accessor.HttpContext.Connection.RemoteIpAddress.ToString()
            };

            await context.Operaciones.AddAsync(nuevaOperacion);
            await context.SaveChangesAsync();

            await Task.FromResult(0);
        }

        public async Task<bool> PuedeRealizarOperacion()
        {
            var ip = accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var ultimaOperacion = await context.Operaciones
                .Where(x => x.Ip == ip)
                .OrderByDescending(x => x.FechaAccion)
                .FirstOrDefaultAsync();

            if (ultimaOperacion == null)
            {
                return true;
            }

            var tiempoTranscurrido = (DateTime.Now - ultimaOperacion.FechaAccion).TotalSeconds;
            return tiempoTranscurrido >= 30;
        }      
    }
}
