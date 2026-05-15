using Isra.Demos.Microservicios.UsuariosCuentas.Modelo;
using Isra.Demos.Microservicios.UsuariosCuentas.Servicio;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Isra.Demos.Microservicios.UsuariosCuentas.Controllers
{
    /// <summary>
    /// Controller de cuentas usuario. Esta clase es responsable de manejar las solicitudes HTTP relacionadas con las cuentas de usuario, como la creación, actualización, eliminación y obtención de cuentas de usuario. La implementación específica de los métodos en este controller dependerá de la lógica de negocio y los requisitos del sistema, así como del tipo de almacenamiento utilizado (por ejemplo, base de datos relacional, NoSQL, etc.). En esta clase se pueden incluir validaciones adicionales, manejo de errores y cualquier otra lógica necesaria para garantizar el correcto funcionamiento del controller de cuentas de usuario.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CuentaUsuarioController : ControllerBase
    {
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IConfiguration _config;
        //private readonly ICuentaRepositorio _cuentaRepositorio;

        ///// <summary>
        ///// Constructor de la clase CuentaUsuarioController, que recibe una instancia de ICuentaRepositorio a través de la inyección de dependencias. Esta instancia se utiliza para interactuar con el repositorio de cuentas de usuario y realizar las operaciones necesarias para manejar las solicitudes HTTP relacionadas con las cuentas de usuario. Al utilizar la inyección de dependencias, se facilita la gestión de las dependencias y se promueve un diseño más modular y mantenible del código.
        ///// </summary>
        ///<param name="cuentaServicio"></param>
        ///<param name="config"></param>
        public CuentaUsuarioController(ICuentaServicio cuentaServicio, IConfiguration config)
        {
            _cuentaServicio = cuentaServicio;
            _config = config;
        }

        /// <summary>
        /// Creando la nueva cuenta de usuario
        /// </summary>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CrearCuentaAsync([FromBody] CuentaUsuario cuenta)
        {
            if (cuenta == null)
            {
                return BadRequest();
            }

            // datos complementarios
            cuenta.Id = Guid.NewGuid();
            cuenta.IdCuenta = Guid.NewGuid();
            cuenta.FechaHoraCreacion = DateTimeOffset.UtcNow;
            cuenta.FechaHoraModificacion = DateTimeOffset.UtcNow;
            cuenta.Estatus = 1; // activo

            try
            {
                var resultado = await _cuentaServicio.CrearCuentaAsync(cuenta);

                if (!resultado)
                {
                    return StatusCode(500);
                }

                return Ok(new
                {
                    Success = true,
                    Mensaje = "Cuenta creada exitosamente",
                    cuenta.IdCuenta
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Iniciar sesión
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Secreto))
            {
                return BadRequest(new { Success = false, Mensaje = "Datos de inicio de sesión inválidos" });
            }

            var cuenta = await _cuentaServicio.ValidarCredencialesAsync(request.Usuario, request.Secreto);

            if (cuenta == null)
            {
                return Unauthorized(new { Success = false, Mensaje = "Credenciales incorrectas" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, cuenta.Id.ToString()),
                    new Claim(ClaimTypes.Name, cuenta.Usuario),
                    new Claim("IdCuenta", cuenta.IdCuenta.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            });
        }
    }
}
