namespace Isra.Demos.Microservicios.UsuariosCuentas.Modelo
{
    /// <summary>
    /// Peticion de login. Esta clase representa la estructura de los datos que se esperan recibir en una solicitud de inicio de sesión. Contiene dos propiedades: "Usuario", que representa el nombre de usuario o correo electrónico del usuario que intenta iniciar sesión, y "Secreto", que representa la contraseña o clave secreta asociada a esa cuenta de usuario. Esta clase se utiliza para deserializar los datos enviados en el cuerpo de la solicitud HTTP y facilitar el proceso de autenticación del usuario en el sistema.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Nombre de usuario o correo electrónico del usuario que intenta iniciar sesión.
        /// </summary>
        public string Usuario { get; set; }

        /// <summary>
        /// clave secreta o contraseña asociada a la cuenta de usuario que se está intentando autenticar.
        /// </summary>
        public string Secreto { get; set; }
    }
}
