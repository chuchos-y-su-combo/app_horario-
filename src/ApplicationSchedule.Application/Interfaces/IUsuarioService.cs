using ApplicationSchedule.Application.DTOs.Usuarios;

namespace ApplicationSchedule.Application.Interfaces;

public interface IUsuarioService
{
    Task<List<UsuarioResponse>> ObtenerTodosAsync();

    Task<UsuarioResponse?> ObtenerPorIdAsync(string idUsuario);

    Task<UsuarioResponse> CrearAsync(CrearUsuarioRequest request);

    Task<bool> ActualizarAsync(string idUsuario, ActualizarUsuarioRequest request);

    Task<bool> CambiarPasswordAsync(string idUsuario, CambiarPasswordRequest request);

    Task<bool> EliminarAsync(string idUsuario);
}