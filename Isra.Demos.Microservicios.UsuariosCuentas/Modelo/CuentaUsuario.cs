namespace Isra.Demos.Microservicios.UsuariosCuentas.Modelo
{
    /// <summary>
    /// Cuenta de usuario que representa la información de la cuenta asociada a un usuario.
    /// Cuenta bancaria
    /// </summary>
    public class CuentaUsuario
    {
        /// <summary>
        /// Id del registro
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id de la cuenta bancaria asociada al usuario
        /// </summary>
        public Guid IdCuenta { get; set; }

        /// <summary>
        /// Propietario de la cuenta bancaria asociada al usuario
        /// </summary>
        public string Propietario { get; set; }

        /// <summary>
        /// Fecha y hora de creación del registro de la cuenta de usuario
        /// </summary>
        public DateTimeOffset FechaHoraCreacion { get; set; }

        /// <summary>
        /// Fecha y hora de modificación del registro de la cuenta de usuario
        /// </summary>
        public DateTimeOffset FechaHoraModificacion { get; set; }

        /// <summary>
        /// Estatus de la cuenta, puede ser inactiva, activa, bloqueada, etc. dependiendo de la lógica de negocio.
        /// </summary>
        public byte Estatus { get; set; }

        /// <summary>
        /// Usuario para acceso a portal de la cuenta bancaria asociada al usuario. Este campo es opcional y puede ser utilizado para almacenar el nombre de usuario o identificador de acceso a la cuenta bancaria, dependiendo de la lógica de negocio y los requisitos del sistema.
        /// </summary>
        public string Usuario { get; set; }

        /// <summary>
        /// Contraseña para acceso a portal de la cuenta bancaria asociada al usuario. Este campo es opcional y puede ser utilizado para almacenar la contraseña o clave de acceso a la cuenta bancaria, dependiendo de la lógica de negocio y los requisitos del sistema. Es importante considerar las mejores prácticas de seguridad al manejar esta información, como el uso de cifrado o hashing para proteger la contraseña.
        /// </summary>
        public string Secreto { get; set; }
    }
}
