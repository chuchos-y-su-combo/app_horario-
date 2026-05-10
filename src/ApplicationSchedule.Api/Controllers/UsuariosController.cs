using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationSchedule.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponse>>> ObtenerTodos()
    {
        List<UsuarioResponse> usuarios = await _usuarioService.ObtenerTodosAsync();

        return Ok(usuarios);
    }

    [HttpGet("{idUsuario}")]
    public async Task<ActionResult<UsuarioResponse>> ObtenerPorId(string idUsuario)
    {
        UsuarioResponse? usuario = await _usuarioService.ObtenerPorIdAsync(idUsuario);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado."
            });
        }

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Crear(CrearUsuarioRequest request)
    {
        try
        {
            UsuarioResponse usuarioCreado = await _usuarioService.CrearAsync(request);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { idUsuario = usuarioCreado.IdUsuario },
                usuarioCreado
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("{idUsuario}")]
    public async Task<IActionResult> Actualizar(string idUsuario, ActualizarUsuarioRequest request)
    {
        try
        {
            bool actualizado = await _usuarioService.ActualizarAsync(idUsuario, request);

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPatch("{idUsuario}/password")]
    public async Task<IActionResult> CambiarPassword(string idUsuario, CambiarPasswordRequest request)
    {
        bool actualizado = await _usuarioService.CambiarPasswordAsync(idUsuario, request);

        if (!actualizado)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado."
            });
        }

        return NoContent();
    }

    [HttpDelete("{idUsuario}")]
    public async Task<IActionResult> Eliminar(string idUsuario)
    {
        bool eliminado = await _usuarioService.EliminarAsync(idUsuario);

        if (!eliminado)
        {
            return NotFound(new
            {
                mensaje = "Usuario no encontrado."
            });
        }

        return NoContent();
    }
}