using Isra.Demos.Microservicios.UsuariosCuentas.Modelo;
using Isra.Demos.Microservicios.UsuariosCuentas.Repositorio;

namespace Isra.Demos.Microservicios.UsuariosCuentas.Servicio
{
    /// <summary>
    /// Servicio para la gestión de cuentas de usuario. Esta clase implementa la interfaz ICuentaServicio y proporciona métodos para crear, actualizar, eliminar y obtener cuentas de usuario. La implementación específica de estos métodos dependerá de la lógica de negocio y los requisitos del sistema, así como del tipo de almacenamiento utilizado (por ejemplo, base de datos relacional, NoSQL, etc.). En esta clase se pueden incluir validaciones adicionales, manejo de errores y cualquier otra lógica necesaria para garantizar el correcto funcionamiento del servicio de cuentas de usuario.
    /// </summary>
    public class CuentaServicio
        : ICuentaServicio
    {
        private readonly ICuentaRepositorio _cuentaRepositorio;

        /// <summary>
        /// Constructor de la clase CuentaServicio, que recibe una instancia de ICuentaRepositorio a través de la inyección de dependencias. Esta instancia se utiliza para interactuar con el repositorio de cuentas de usuario y realizar las operaciones necesarias para manejar las solicitudes relacionadas con las cuentas de usuario. Al utilizar la inyección de dependencias, se facilita la gestión de las dependencias y se promueve un diseño más modular y mantenible del código.
        /// </summary>
        /// <param name="cuentaRepositorio"></param>
        public CuentaServicio(ICuentaRepositorio cuentaRepositorio)
        {
            _cuentaRepositorio = cuentaRepositorio;
        }

        /// <summary>
        /// creando una nueva cuenta de usuario. Este método recibe un objeto CuentaUsuario que contiene la información de la cuenta a crear, y devuelve un valor booleano que indica si la creación de la cuenta fue exitosa o no. La implementación de este método puede incluir validaciones adicionales, manejo de errores y cualquier otra lógica necesaria para garantizar el correcto funcionamiento del proceso de creación de cuentas de usuario.
        /// </summary>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public async Task<bool> CrearCuentaAsync(CuentaUsuario cuenta)
        {
            // Validar que el usuario sea único
            if (await _cuentaRepositorio.ExisteUsuarioAsync(cuenta.Usuario))
            {
                throw new InvalidOperationException($"El usuario '{cuenta.Usuario}' ya existe. Por favor elija uno diferente.");
            }

            // Hashear la contraseña antes de guardarla
            cuenta.Secreto = BCrypt.Net.BCrypt.HashPassword(cuenta.Secreto);

            // almacena primero la cuenta del usuario
            return await _cuentaRepositorio.CrearCuentaAsync(cuenta);
        }

        /// <summary>
        /// Validar credenciales de usuario
        /// </summary>
        public async Task<CuentaUsuario> ValidarCredencialesAsync(string usuario, string secreto)
        {
            var cuenta = await _cuentaRepositorio.ObtenerCuentaPorUsuarioAsync(usuario);

            if (cuenta != null && BCrypt.Net.BCrypt.Verify(secreto, cuenta.Secreto))
            {
                return cuenta;
            }

            return null;
        }
    }
}
