using MiBiblioteca.DTOs;
using MiBiblioteca.Models;
using MiBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiStore.Services;

namespace MiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LibrosController : ControllerBase
    {
        private readonly MiBibliotecaContext context;
        private readonly IGestorImagenes gestorImagenes;

        public LibrosController(MiBibliotecaContext context, IGestorImagenes gestorImagenes)
        {
            this.context = context;
            this.gestorImagenes = gestorImagenes;
        }

        //Desde la ruta /Libros
        [HttpGet("/Libros")]
        public async Task<List<Libro>> GetLibros()
        {
            return await context.Libros.ToListAsync();
        }
        // Desde la ruta /api/Libros
        [HttpGet]
        public async Task<List<Libro>> GetLibrosAPI()
        {
            return await context.Libros.ToListAsync();
        }
        // Devolver error 404 si no existe el ID del Libro
        [HttpGet("{isbn}")]
        public async Task<ActionResult<Libro>> GetLibrosError(int id)
        {
            var libro = await context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }
            return Ok(libro);
        }
        // Devolver los libros según lo que contenga el título
        [HttpGet("contiene/{texto}")]
        public async Task<ActionResult<Libro>> GetLibrosPorContenido(string texto)
        {
            var libroTitulo = await context.Libros.Where(x => x.Titulo.Contains(texto)).ToListAsync();

            if (libroTitulo == null)
            {
                return NotFound();
            }
            return Ok(libroTitulo);
        }
        // Ordenar los libros según su dirección
        [HttpGet("ordenadosportitulo/{direccion}")]
        public async Task<ActionResult<Libro>> GetLibrosOrdenDireccion(bool direccion)
        {
            var libroDireccion = new List<Libro>();
            if (direccion)
            {
                libroDireccion = await context.Libros.OrderBy(x => x.Titulo).ToListAsync();
            }
            else if (direccion == false)
            {
                libroDireccion = await context.Libros.OrderByDescending(x => x.Titulo).ToListAsync();
            }
            else
            {
                return BadRequest("Valor de consulta incorrecto");
            }
            return Ok(libroDireccion);
        }

        [HttpGet("precio/entre")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibroPrecios([FromQuery] decimal min, [FromQuery] decimal max)
        {
            var libroPrecio = await context.Libros.Where(x => x.Precio > min && x.Precio < max).ToListAsync();
            return Ok(libroPrecio);
        }

        [HttpGet("desdehasta/{desde}/{hasta}")]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibroDesdeHasta(int desde, int hasta)
        {
            if (desde < 1)
            {
                return BadRequest("El min. no puede ser 0.");
            }
            if (hasta < desde)
            {
                return BadRequest("El max. no puede ser inferior que el min.");
            }
            var librosDesdeHasta = await context.Libros.Skip(desde - 1).Take(hasta - desde).ToListAsync();
            return Ok(librosDesdeHasta);
        }

        [HttpGet("venta")]
        public async Task<ActionResult<List<DTOVentaLibro>>> GetLibroPrecios()
        {
            var libroVentas = await context.Libros.Select(x => new DTOVentaLibro
            {
                TituloLibro = x.Titulo,
                PrecioLibro = x.Precio
            }).ToListAsync();
            return Ok(libroVentas);
        }

        [HttpGet("librosagrupadospordescatalogado")]
        public async Task<ActionResult> GetLibrosAgrupadosPorDescatalogado()
        {
            var libros = await context.Libros.GroupBy(g => g.Descatalogados)
                .Select(x => new
                {
                    Descatalogado = x.Key,
                    Total = x.Count(),
                    Libros = x.ToList()
                }).ToListAsync();

            return Ok(libros);
        }


        [HttpPost]
        public async Task<ActionResult> PostLibro(Libro libro)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.IdAutor == libro.AutorId);
            if (!existeAutor)
            {
                return BadRequest("Autor inexistente");
            }

            var existeEditorial = await context.Editoriales.AnyAsync(x => x.IdEditorial == libro.EditorialId);
            if (!existeEditorial)
            {
                return BadRequest("Editorial inexistente");
            }

            var newLibro = new Libro()
            {
                Isbn = libro.Isbn,
                Titulo = libro.Titulo,
                Precio = libro.Precio,
                Paginas = libro.Paginas,
                Descatalogados = libro.Descatalogados,
                FotoPortadaUrl = libro.FotoPortadaUrl,
                AutorId = libro.AutorId,
                EditorialId = libro.EditorialId,
            };

            await context.AddAsync(newLibro);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPut]
        public async Task<ActionResult> PutLibro([FromBody] Libro libro)
        {
            var libroUpdate = await context.Libros.AsTracking().FirstOrDefaultAsync(x => x.Isbn == libro.Isbn);
            if (libroUpdate == null)
            {
                return NotFound();
            }

            var existeAutor = await context.Autores.AnyAsync(x => x.IdAutor == libro.AutorId);
            if (!existeAutor)
            {
                return BadRequest("Autor inexistente");
            }

            var existeEditorial = await context.Editoriales.AnyAsync(x => x.IdEditorial == libro.EditorialId);
            if (!existeEditorial)
            {
                return BadRequest("Editorial inexistente");
            }

            libroUpdate.Titulo = libro.Titulo;
            libroUpdate.Precio = libro.Precio;
            libroUpdate.Paginas = libro.Paginas;
            libroUpdate.Descatalogados = libro.Descatalogados;
            libroUpdate.FotoPortadaUrl = libro.FotoPortadaUrl;
            libroUpdate.AutorId = libro.AutorId;
            libroUpdate.EditorialId = libro.EditorialId;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteLibro(string isbn)
        {
            var libro = await context.Libros.FirstOrDefaultAsync(x => x.Isbn == isbn);

            if (libro is null)
            {
                return NotFound();
            }

            context.Remove(libro);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult> GetFiltroMultiple([FromQuery] DTOLibrosConsulta filtroLibros)
        {
            var librosQueryable = context.Libros.AsQueryable();

            if (filtroLibros.Precio is not null)
            {
                librosQueryable = librosQueryable.Where(x => x.Precio > filtroLibros.Precio);
            }

            if (filtroLibros.Descatalogado)
            {
                librosQueryable = librosQueryable.Where(x => x.Descatalogados == true);
            }

            var libros = await librosQueryable.ToListAsync();

            return Ok(libros);
        }

        [HttpPost("foto")]
        public async Task<ActionResult> PostLibros([FromForm] DTOLibroAgregar libro)
        {
            Libro newLibro = new Libro
            {
                Isbn = libro.Isbn,
                Titulo = libro.Titulo,
                Precio = libro.Precio,
                Paginas = libro.Paginas,
                Descatalogados = false,
                AutorId = libro.AutorId,
                EditorialId = libro.EditorialId,
                FotoPortadaUrl = ""
            };

            if (libro.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await libro.Foto.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // La extensión la necesitamos para guardar el archivo
                    var extension = Path.GetExtension(libro.Foto.FileName);
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivos instancia el servicio y cuando se deja de usar se destruye
                    newLibro.FotoPortadaUrl = await gestorImagenes.GuardarArchivo(contenido, extension, "imagenes",
                        libro.Foto.ContentType);
                }
            }

            await context.AddAsync(newLibro);
            await context.SaveChangesAsync();
            return Ok(newLibro);
        }

        [HttpPut("foto")]
        public async Task<ActionResult> PutLibros([FromForm] DTOLibroAgregar libro)
        {
            var libroUpdate = await context.Libros.FindAsync(libro.Isbn);
            if (libroUpdate is null)
            {
                return NotFound();
            }
            libroUpdate.Titulo = libro.Titulo;
            libroUpdate.Precio = libro.Precio;
            libroUpdate.Paginas = libro.Paginas;
            libroUpdate.EditorialId = libro.EditorialId;
            libroUpdate.AutorId = libro.AutorId;

            if (libro.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await libro.Foto.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // La extensión la necesitamos para guardar el archivo
                    var extension = Path.GetExtension(libro.Foto.FileName);
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivos instancia el servicio y cuando se deja de usar se destruye
                    libroUpdate.FotoPortadaUrl = await gestorImagenes.EditarArchivo(contenido, extension, "imagenes", libroUpdate.FotoPortadaUrl,
                        libro.Foto.ContentType);
                }
            }

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("foto/{isbn}")]
        public async Task<ActionResult> DeleteLibros([FromRoute] string isbn)
        {
            var libro = await context.Libros.FindAsync(isbn);
            if (libro == null)
            {
                return NotFound();
            }

            await gestorImagenes.BorrarArchivo(libro.FotoPortadaUrl, "imagenes");
            context.Remove(libro);
            await context.SaveChangesAsync();
            return Ok();
        }


    }
}
