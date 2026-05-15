using Isra.Demos.Microservicios.UsuariosCuentas.Modelo;

namespace Isra.Demos.Microservicios.UsuariosCuentas.Servicio
{
    /// <summary>
    /// Servicio de cuentas de usuario. Esta interfaz define los métodos que deben ser implementados por cualquier clase que se encargue de gestionar las cuentas de usuario en el sistema. En este caso, se incluye un método para crear una cuenta de usuario de forma asíncrona, lo que permite realizar operaciones de manera eficiente sin bloquear el hilo principal de ejecución. La implementación específica de este servicio dependerá de la lógica de negocio y los requisitos del sistema, así como del tipo de almacenamiento utilizado (por ejemplo, base de datos relacional, NoSQL, etc.). En esta interfaz se pueden incluir otros métodos relacionados con la gestión de cuentas de usuario, como la actualización, eliminación y obtención de cuentas, según sea necesario para el funcionamiento del sistema.
    /// </summary>
    public interface ICuentaServicio
    {
        /// <summary>
        /// crear la cuenta nueva
        /// </summary>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        Task<bool> CrearCuentaAsync(CuentaUsuario cuenta);

        /// <summary>
        /// Validar las credenciales de un usuario
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="secreto"></param>
        /// <returns></returns>
        Task<CuentaUsuario> ValidarCredencialesAsync(string usuario, string secreto);
    }
}
