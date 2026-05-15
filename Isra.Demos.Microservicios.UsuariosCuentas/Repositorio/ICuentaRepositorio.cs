using Isra.Demos.Microservicios.UsuariosCuentas.Modelo;

namespace Isra.Demos.Microservicios.UsuariosCuentas.Repositorio
{
    /// <summary>
    /// Interfaz para el repositorio de cuentas de usuario.
    /// </summary>
    public interface ICuentaRepositorio
    {
        //Task<Cuenta> ObtenerCuentaPorIdAsync(Guid id);

        /// <summary>
        /// Crea una nueva cuenta de usuario.
        /// </summary>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        Task<bool> CrearCuentaAsync(CuentaUsuario cuenta);

        //Task ActualizarCuentaAsync(Cuenta cuenta);
        //Task EliminarCuentaAsync(Guid id);

        /// <summary>
        /// Verifica si existe un usuario con el mismo nombre.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        Task<bool> ExisteUsuarioAsync(string usuario);
    }
}
