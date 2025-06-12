using MiBiblioteca.DTOs;
using MiBiblioteca.Models;
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
    public class EditorialesController : ControllerBase
    {
        private readonly MiBibliotecaContext context;

        // Inyección de dependencia.
        // Como nuestro controller depende de MiBibliotecaContext para poder desempeñar sus funciones, lo podemos inyectar en el constructor
        // La inyección de dependencia trae al constructor de la clase todas las dependencias que necesita y las pasa a variables privadas
        // de la clase

        public EditorialesController(MiBibliotecaContext context)
        {
            this.context = context;
        }
        // Desde la ruta /Editoriales
        [HttpGet("/Editoriales")]
        public async Task<List<Editoriale>> GetEditoriales()
        {
            return await context.Editoriales.ToListAsync();
        }
        // Desde la ruta /api/Editoriales
        [HttpGet]
        public async Task<List<Editoriale>> GetEditorialesAPI()
        {
            return await context.Editoriales.ToListAsync();
        }
        // Devolver error 404 si no existe el ID de la Editorial
        [HttpGet("{id}")]
        public async Task<ActionResult<Editoriale>> GetEditorialesError(int id)
        {
            var editorial = await context.Editoriales.FindAsync(id);
            if (editorial == null)
            {
                return NotFound();
            }
            return Ok(editorial);
        }

        [HttpGet("editorialeslibros")]
        public async Task<ActionResult> GetEditorialesLibros()
        {
            var editorialesLibros = await context.Editoriales.Include(x => x.Libros).ToListAsync();
            return Ok(editorialesLibros);
        }

        [HttpPost]
        public async Task<ActionResult> PostEditorial(Editoriale editorial)
        {
            var newEditorial = new Editoriale()
            {
                Nombre = editorial.Nombre
            };

            await context.AddAsync(newEditorial);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPost("dto")]
        public async Task<ActionResult> PostEditorialDTO(DTOAltaEditorial editorial)
        {
            var newEditorial = new Editoriale()
            {
                Nombre = editorial.NombreEditorial
            };

            await context.AddAsync(newEditorial);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPost("varios")]
        public async Task<ActionResult> PostEditoriales(Editoriale[] editoriales)
        {
            List<Editoriale> varias = new();
            foreach (var x in editoriales)
            {
                varias.Add(new Editoriale
                {
                    Nombre = x.Nombre
                });
            }
            await context.AddRangeAsync(varias);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("variosdto")]
        public async Task<ActionResult> PostEditorialesDTO(DTOAltaEditorial[] editoriales)
        {
            List<Editoriale> varias = new();
            foreach (var x in editoriales)
            {
                varias.Add(new Editoriale
                {
                    Nombre = x.NombreEditorial
                });
            }
            await context.AddRangeAsync(varias);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> PutEditorial([FromBody] Editoriale editorial)
        {
            var editorialUpdate = await context.Editoriales.AsTracking().FirstOrDefaultAsync(x => x.IdEditorial == editorial.IdEditorial);
            if (editorialUpdate == null)
            {
                return NotFound();
            }
            editorialUpdate.Nombre = editorial.Nombre;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("dto")]
        public async Task<ActionResult> PutEditorialDTO([FromBody] DTOAltaEditorial editorial)
        {
            var editorialUpdate = await context.Editoriales.AsTracking().FirstOrDefaultAsync(x => x.IdEditorial == editorial.IdEditorial);
            if (editorialUpdate == null)
            {
                return NotFound();
            }
            editorialUpdate.Nombre = editorial.NombreEditorial;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteEditorial(int id)
        {
            var hayLibros = await context.Libros.AnyAsync(x => x.EditorialId == id);
            if (hayLibros)
            {
                return BadRequest("Hay libros relacionados");
            }
            var editorial = await context.Editoriales.FindAsync(id);

            if (editorial is null)
            {
                return NotFound();
            }

            context.Remove(editorial);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("sql")]
        public async Task<ActionResult> ModificarEditoralSQL(DTOAltaEditorial editorial)
        {
            await context.Database.
                ExecuteSqlInterpolatedAsync($"UPDATE Editoriales SET Nombre = {editorial.NombreEditorial} WHERE IdEditorial={editorial.IdEditorial}");
            return Ok();
        }

    }
}
