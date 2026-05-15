using Dapper;
using Isra.Demos.Microservicios.Modelo;
using Isra.Demos.Microservicios.UsuariosCuentas.Modelo;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Isra.Demos.Microservicios.UsuariosCuentas.Repositorio
{
    /// <summary>
    /// Repositorio para la gestión de cuentas de usuario. Esta clase implementa la interfaz ICuentaRepositorio y proporciona métodos para crear, actualizar, eliminar y obtener cuentas de usuario. La implementación específica de estos métodos dependerá de la lógica de negocio y los requisitos del sistema, así como del tipo de almacenamiento utilizado (por ejemplo, base de datos relacional, NoSQL, etc.). En esta clase se pueden incluir validaciones adicionales, manejo de errores y cualquier otra lógica necesaria para garantizar el correcto funcionamiento del repositorio de cuentas de usuario.
    /// </summary>
    public class CuentaRepositorio : ICuentaRepositorio
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor de la clase CuentaRepositorio, que inicializa la cadena de conexión a la base de datos utilizando una constante definida en la clase Constantes. Esta cadena de conexión es fundamental para establecer la comunicación entre la aplicación y la base de datos SQL Server, permitiendo así el almacenamiento y recuperación de información relacionada con las cuentas bancarias y otros datos relevantes para el funcionamiento del sistema. Al utilizar una cadena de conexión local, se facilita el desarrollo y las pruebas de la aplicación en un entorno controlado, aunque en un entorno de producción se recomendaría utilizar una configuración más robusta y segura para la conexión a la base de datos.
        /// </summary>
        public CuentaRepositorio()
        {
            _connectionString = Constantes.SqlServerBancoCuentasConnectionString;
        }

        /// <summary>
        /// Crear la cuenta de usuario en la base de datos. Este método es responsable de insertar una nueva cuenta de usuario en la base de datos utilizando la información proporcionada en el objeto Cuenta. La implementación específica de este método dependerá de la lógica de negocio y los requisitos del sistema, así como del tipo de almacenamiento utilizado (por ejemplo, base de datos relacional, NoSQL, etc.). En esta implementación se pueden incluir validaciones adicionales, manejo de errores y cualquier otra lógica necesaria para garantizar el correcto funcionamiento del proceso de creación de cuentas de usuario.
        /// </summary>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CrearCuentaAsync(CuentaUsuario cuenta)
        {
            using var cnn = new SqlConnection(_connectionString);

            await cnn.OpenAsync();

            await cnn.ExecuteAsync(@"
            INSERT INTO [dbo].[TblCuentasUsuario]
            ([Id]
            ,[IdCuenta]
            ,[Propietario]
            ,[FechaHoraCreacion]
            ,[FechaHoraModificacion]
            ,[Estatus]
            ,[Usuario]
            ,[Secreto]) VALUES (@Id, @IdCuenta, @Propietario, @FechaHoraCreacion, @FechaHoraModificacion, @Estatus, @Usuario, @Secreto)",
            new
            {
                Id = cuenta.Id,
                IdCuenta = cuenta.IdCuenta,
                Propietario = cuenta.Propietario,
                FechaHoraCreacion = cuenta.FechaHoraCreacion,
                FechaHoraModificacion = cuenta.FechaHoraModificacion,
                Estatus = cuenta.Estatus,
                Usuario = cuenta.Usuario,
                Secreto = cuenta.Secreto
            }, commandType: CommandType.Text);

            return true;
        }

        /// <summary>
        /// Verifica si existe el nombre de usuario
        /// </summary>
        public async Task<bool> ExisteUsuarioAsync(string usuario)
        {
            using var cnn = new SqlConnection(_connectionString);
            await cnn.OpenAsync();
            var count = await cnn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM [dbo].[TblCuentasUsuario] WHERE Usuario = @Usuario", 
                new { Usuario = usuario }
            );
            return count > 0;
        }

        /// <summary>
        /// Obtiene una cuenta de usuario por sus credenciales
        /// </summary>
        public async Task<CuentaUsuario> ObtenerCuentaPorCredencialesAsync(string usuario, string secreto)
        {
            using var cnn = new SqlConnection(_connectionString);
            await cnn.OpenAsync();
            return await cnn.QueryFirstOrDefaultAsync<CuentaUsuario>(
                "SELECT * FROM [dbo].[TblCuentasUsuario] WHERE Usuario = @Usuario AND Secreto = @Secreto AND Estatus = 1",
                new { Usuario = usuario, Secreto = secreto }
            );
        }
    }
}
