using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationSchedule.Application.Security;

namespace ApplicationSchedule.Api.Controllers;
//[Authorize] //quitar el comentario en caso de que se quiera que solo los de token creen los usuarios
[Authorize(Roles = RolesSistema.Administrador)]
[ApiController]
[Route("api/usuarios")]
/// <summary>
/// Controlador REST para la gestión de usuarios del sistema.
/// Responsable de exponer los endpoints CRUD para la entidad Usuario.
/// Pertenece a la capa API y delega la lógica a <see cref="IUsuarioService"/>.
/// </summary>
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    /// <summary>
    /// Crea una nueva instancia de <see cref="UsuariosController"/>.
    /// </summary>
    /// <param name="usuarioService">Servicio de usuarios inyectado desde la capa de aplicación.</param>
    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios registrados.
    /// </summary>
    /// <returns>Listado de <see cref="UsuarioResponse"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponse>>> ObtenerTodos()
    {
        List<UsuarioResponse> usuarios = await _usuarioService.ObtenerTodosAsync();

        return Ok(usuarios);
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario.</param>
    /// <returns><see cref="UsuarioResponse"/> si existe; <see cref="Microsoft.AspNetCore.Mvc.NotFoundResult"/> si no.</returns>
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

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos necesarios para crear el usuario.</param>
    /// <returns>Recurso creado con <see cref="UsuarioResponse"/> y ubicación del nuevo recurso.</returns>
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

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario a actualizar.</param>
    /// <param name="request">Datos con los campos a actualizar.</param>
    /// <returns>204 No Content si se actualiza; 404 si no existe; 400 en caso de validación.</returns>
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

    /// <summary>
    /// Cambia la contraseña de un usuario.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario.</param>
    /// <param name="request">Datos con la contraseña actual y la nueva.</param>
    /// <returns>204 No Content si se cambió; 404 si no existe.</returns>
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

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    /// <param name="idUsuario">Identificador del usuario a eliminar.</param>
    /// <returns>204 No Content si se eliminó; 404 si no existe.</returns>
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