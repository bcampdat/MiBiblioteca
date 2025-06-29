﻿using MiBiblioteca.DTOs;
using MiBiblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace MiBiblioteca.Services
{

    public class TareaProgramadaService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IWebHostEnvironment env;
        private readonly string nombreArchivo = "Archivo.txt";
        private Timer timer;

        public TareaProgramadaService(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            this.serviceProvider = serviceProvider;
            this.env = env;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(EscribirDatos, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            Escribir("Proceso iniciado");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            Escribir("Proceso finalizado");
            return Task.CompletedTask; // Parar la depuración desde el icono de IIS para que se ejecute el StopAsync
        }

        private async void EscribirDatos(object state)
        {
            using (var scope = serviceProvider.CreateScope())
            // Necesitamos delimitar un alcance scoped. Los servicios IHostedService son singleton y el ApplicationDBContext Scoped. A continuación "abrimos" un scoped con using para poder
            // utilizar el ApplicationDbContext en este servicio Singleton
            {
                var context = scope.ServiceProvider.GetRequiredService<MiBibliotecaContext>();
                //var libros = await context.Libros.Select(x => x.Titulo).ToListAsync();
                //foreach (var libro in libros)
                //{
                //    Escribir(libro);
                //}

                //var totalLibros = await context.Libros.CountAsync();
                //Escribir($"Total de libros: {totalLibros}");
                var libros = await context.Libros.Select(x => x.Titulo).ToListAsync();
                var totalLibros = await context.Libros.CountAsync();
                Escribir($"Total de libros: {totalLibros}");
            }
        }
        private void Escribir(string mensaje)
        {
            var ruta = $@"{env.ContentRootPath}\wwwroot\{nombreArchivo}";
            using (StreamWriter writer = new StreamWriter(ruta, append: true))
            {
                writer.WriteLine(mensaje);
            }
        }
    }   
}
