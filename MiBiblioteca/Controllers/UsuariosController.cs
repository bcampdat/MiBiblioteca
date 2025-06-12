using MiBiblioteca.DTOs;
using MiBiblioteca.Models;
using MiBiblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
  {
      private readonly MiBibliotecaContext context;
      private readonly HashService hashService;

      public UsuariosController(MiBibliotecaContext context, HashService hashService)
      {
          this.context = context;
          this.hashService = hashService;
      }

        [AllowAnonymous]
        [HttpPost("hash/nuevousuario")]
      public async Task<ActionResult> RegisterUsuarioHash([FromBody] DTOUsuario usuario)
      {
          var resultadoHash = hashService.Hash(usuario.Password);
          var newUsuario = new Usuario
          {
              Email = usuario.Email,
              Password = resultadoHash.Hash,
              Salt = resultadoHash.Salt
          };

          await context.Usuarios.AddAsync(newUsuario);
          await context.SaveChangesAsync();

          return Ok(newUsuario);
      }

     
      [HttpPost("hash/checkusuario")]
      public async Task<ActionResult> CheckUsuarioHash([FromBody] DTOUsuario usuario)
      {
          var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
          if (usuarioDB == null)
          {
              return Unauthorized();
          }

          var resultadoHash = hashService.Hash(usuario.Password, usuarioDB.Salt);
          if (usuarioDB.Password == resultadoHash.Hash)
          {
              return Ok();
          }
          else
          {
              return Unauthorized();
          }

      }

        [HttpPost("hash/changepassword")]
        public async Task<ActionResult> ChangePasswordUsuarioHash([FromBody] DTOUsuarioChangePassword usuario)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            if (usuario.NewPassword == null)
            {
                return Unauthorized();
            }

            var resultadoHash = hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                var resultadoHashNewPassword = hashService.Hash(usuario.NewPassword, usuarioDB.Salt);
                usuarioDB.Password = resultadoHashNewPassword.Hash;
                context.Usuarios.Update(usuarioDB);
                await context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                return Unauthorized();
            }
        }


    }

}
