using MiBiblioteca.Models;
using MiBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MiBiblioteca.DTOs.DTOAutoresLibros;

namespace MiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AutoresController : ControllerBase
    {
        private readonly MiBibliotecaContext context;
        private readonly OperacionesService operacionesService;
        public AutoresController(MiBibliotecaContext context, OperacionesService operacionesService)
        {
            this.context = context;
            this.operacionesService = operacionesService;
        }
       //Desde la ruta /Autores
       [HttpGet("/Autores")]
        public async Task<List<Autore>> GetAutores()
        {
            return await context.Autores.ToListAsync();
        }
        // Desde la ruta /api/Autores
        [HttpGet]
        public async Task<ActionResult<List<Autore>>> GetAutoresAPI()
        {
            var puedeHacerOperacion = await operacionesService.PuedeRealizarOperacion();
            if (!puedeHacerOperacion)
            {
                return BadRequest("No han pasado 30 segundos desde tu última operación");
            }
            await operacionesService.AddOperacion("Consultar", "Autores");
            return await context.Autores.ToListAsync();
        }
        // Devolver error 404 si no existe el ID del Autor
        [HttpGet("{id}")]
        public async Task<ActionResult<Autore>> GetAutoresError(int id)
        {
            var autor = await context.Editoriales.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }
            return Ok(autor);
        }


        //[HttpGet("autoreslibrosdto")]
        //public async Task<ActionResult> GetEditorialesLibrosDTO()
        //{
        //    var autoresLibros = await (from x in context.Autores
        //                               select new DTOAutorLibro
        //                               {
        //                                   IdAutor = x.IdAutor,
        //                                   Nombre = x.Nombre,
        //                                   TotalLibros = x.Libros.Count(),
        //                                   PromedioPrecio = x.Libros.Average(y => y.Precio),
        //                                   Libros = x.Libros.Select(y => new DTOLibroItem
        //                                   {
        //                                       Isbn = y.Isbn,
        //                                       Titulo = y.Titulo,
        //                                       Precio = y.Precio
        //                                   }).ToList(),
        //                               }).ToListAsync();

        //    return Ok(autoresLibros);
        //}

        [HttpGet("autoreslibrosdto/{idAutor:int}")]
        public async Task<ActionResult> GetEditorialesLibrosDTOPorId(int idAutor)
        {
            var autoresLibros = await (from x in context.Autores
                                       select new DTOAutorLibro
                                       {
                                           IdAutor = x.IdAutor,
                                           Nombre = x.Nombre,
                                           TotalLibros = x.Libros.Count(),
                                           PromedioPrecio = x.Libros.Average(y => y.Precio),
                                           Libros = x.Libros.Select(y => new DTOLibroItem
                                           {
                                               Isbn = y.Isbn,
                                               Titulo = y.Titulo,
                                               Precio = y.Precio
                                           }).ToList(),
                                       }).FirstOrDefaultAsync(y => y.IdAutor == idAutor);

            return Ok(autoresLibros);
        }


        [HttpPost]
        public async Task<ActionResult> PostAutor(Autore autor)
        {

            var newAutor = new Autore()
            {
                Nombre = autor.Nombre
            };

            await context.AddAsync(newAutor);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPut]
        public async Task<ActionResult> PutAutor([FromBody] Autore autor)
        {
            var autorUpdate = await context.Autores.AsTracking().FirstOrDefaultAsync(x => x.IdAutor == autor.IdAutor);
            if (autorUpdate == null)
            {
                return NotFound();
            }
            autorUpdate.Nombre = autor.Nombre;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteAutor(int id)
        {
            var hayLibros = await context.Libros.AnyAsync(x => x.AutorId == id);
            if (hayLibros)
            {
                return BadRequest("Hay libros relacionados");
            }
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.IdAutor == id);

            if (autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("sql/{id:int}")]
        public async Task<ActionResult> EliminarAutorSQL(int id)
        {
            var autor = await context.Autores
                      .FromSqlInterpolated($"SELECT * FROM Autores WHERE IdAutor = {id}")
                      .FirstOrDefaultAsync();

            if (autor is null)
            {
                return NotFound();
            }

            var libros = await context.Libros
                        .FromSqlInterpolated($"SELECT * FROM Libros WHERE AutorId = {id}")
                        .FirstOrDefaultAsync();
            if (libros is not null)
            {
                return BadRequest("Hay libros asociados");
            }

            await context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Autores WHERE IdAutor = {id}");
            return Ok();
        }
    }
}
