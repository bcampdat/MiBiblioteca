using MiBiblioteca.DTOs;
using MiBiblioteca.Models;
using MiBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using ServiceStack.Text;
using System.Text;

namespace MiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AuthController : ControllerBase
    {
        private readonly MiBibliotecaContext context;
        private readonly HashService hashService;
        private readonly TokenService tokenService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IWebHostEnvironment env;

        public AuthController(MiBibliotecaContext context,
            HashService hashService, TokenService tokenService, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            this.context = context;
            this.hashService = hashService;
            this.tokenService = tokenService;
            this.httpContextAccessor = httpContextAccessor;
            this.env = env;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return BadRequest();
            }

            var resultadoHash = hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                var response = tokenService.GenerarToken(usuario);
                return Ok(response);
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost("linkchangepassword")]
        public async Task<ActionResult> LinkChangePassword([FromBody] string email)
        {
            var usuarioDB = await context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no registrado");
            }

            // Creamos un string aleatorio 
            Guid miGuid = Guid.NewGuid();
            string textoEnlace = Convert.ToBase64String(miGuid.ToByteArray());
            // Eliminar caracteres que pueden causar problemas
            textoEnlace = textoEnlace.Replace("=", "").Replace("+", "").Replace("/", "").Replace("?", "").Replace("&", "").Replace("!", "").Replace("¡", "");
            usuarioDB.EnlaceCambioPass = textoEnlace;
            usuarioDB.FechaEnvioEnlace = DateTime.Now;
            await context.SaveChangesAsync();
            var ruta = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/changepassword/{textoEnlace}";
            return Ok(ruta);
        }

        //[HttpGet("/changepassword/{textoEnlace}")]
        //public async Task<ActionResult> LinkChangePasswordHash(string textoEnlace)
        //{
        //    var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.EnlaceCambioPass == textoEnlace);
        //    if (usuarioDB == null)
        //    {
        //        return Unauthorized("Operación no autorizada");
        //    }

        //    var fechaCaducidad = usuarioDB.FechaEnvioEnlace.Value.AddMinutes(1);

        //    if (fechaCaducidad < DateTime.Now)
        //    {
        //        return Unauthorized("Enlace caducado");
        //    }

        //    return Ok("Enlace correcto");
        //}

        [HttpGet("/changepassword/{textoEnlace}")]
        public async Task<ActionResult> LinkChangePasswordHash(string textoEnlace)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.EnlaceCambioPass == textoEnlace);

            StringBuilder htmlComienzo = new StringBuilder();
            string bootstrap = "<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css' integrity='sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3' crossorigin='anonymous'>";
            StringBuilder htmlRespuesta = new StringBuilder();

            if (usuarioDB == null)
            {
                htmlRespuesta.AppendLine(bootstrap);
                htmlRespuesta.AppendLine("<div class='px-4 py-5 my-5 text-center'>");
                htmlRespuesta.AppendLine("<img class='d-block mx-auto mb-4 img-fluid' src='https://img.freepik.com/vector-gratis/vector-degradado-logotipo-colorido-pajaro_343694-1365.jpg?size=626&ext=jpg' alt='Logo'>");
                htmlRespuesta.AppendLine("<div class='col-lg-6 mx-auto'>");
                htmlRespuesta.AppendLine("<p class='display-5 mb-4'>Operación no autorizada</p>");
                htmlRespuesta.AppendLine("<div class='d-grid gap-2 d-sm-flex justify-content-sm-center'>");
                htmlRespuesta.AppendLine("</div></div></div>");
                return Content(htmlRespuesta.ToString(), "text/html", Encoding.UTF8);
            }
            var fechaCaducidad = usuarioDB.FechaEnvioEnlace.Value.AddMinutes(2);
            if (fechaCaducidad < DateTime.Now)
            {
                htmlRespuesta.AppendLine(bootstrap);
                htmlRespuesta.AppendLine("<div class='px-4 py-5 my-5 text-center'>");
                htmlRespuesta.AppendLine("<img class='d-block mx-auto mb-4 img-fluid' src='https://img.freepik.com/vector-gratis/vector-degradado-logotipo-colorido-pajaro_343694-1365.jpg?size=626&ext=jpg' alt='Logo'>");
                htmlRespuesta.AppendLine("<div class='col-lg-6 mx-auto'>");
                htmlRespuesta.AppendLine("<p class='display-5 mb-4'>Enlace caducado o ya utilizado</p>");
                htmlRespuesta.AppendLine("<div class='d-grid gap-2 d-sm-flex justify-content-sm-center'>");
                htmlRespuesta.AppendLine("</div></div></div>");
                return Content(htmlRespuesta.ToString(), "text/html", Encoding.UTF8);
            }

            var path = Path.Combine(env.WebRootPath, "changepassword.html");
            htmlRespuesta.Append(System.IO.File.ReadAllText(path));

            return Content(htmlRespuesta.ToString(), "text/html", Encoding.UTF8);
        }


        [AllowAnonymous]
        [HttpPost("/resetpassword")]
        public async Task<ActionResult> ResetPassword([FromBody] DTORessetPassword infoUsuario)
        {
            var usuarioDB = await context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.EnlaceCambioPass == infoUsuario.Link);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            if (usuarioDB.FechaEnvioEnlace.Value.AddMinutes(2) < DateTime.Now)
            {
                return Unauthorized("Enlace caducado");
            }

            var resultadoHash = hashService.Hash(infoUsuario.Password);
            usuarioDB.Password = resultadoHash.Hash;
            usuarioDB.Salt = resultadoHash.Salt;
            usuarioDB.EnlaceCambioPass = null;
            usuarioDB.FechaEnvioEnlace = null;

            await context.SaveChangesAsync();

            return Ok("Password cambiado con exito");
        }

    }
}
