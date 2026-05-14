using Isra.Demos.Microservicios.CuentaMovimientos.Modelo;
using Isra.Demos.Microservicios.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Isra.Demos.Microservicios.CuentaMovimientos.Controllers
{
    /// <summary>
    /// Controlador para operaciones de cuentas bancarias
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly ICuentaBancariaService _cuentaService;

        /// <summary>
        /// Constructor del controlador, inyecta el servicio de cuentas bancarias
        /// </summary>
        /// <param name="cuentaService"></param>
        public CuentasController(ICuentaBancariaService cuentaService)
        {
            _cuentaService = cuentaService;
        }

        /// <summary>
        /// Depositar dinero en una cuenta
        /// </summary>
        [HttpPost("{cuentaId:guid}/depositar")]
        public async Task<ActionResult<object>> Depositar(Guid cuentaId, [FromBody] OperacionMonetariaRequest request)
        {
            try
            {
                await _cuentaService.DepositarAsync(cuentaId, request.Monto, request.Propietario);
                var cuenta = await _cuentaService.ObtenerCuentaAsync(cuentaId);
                return Ok(new
                {
                    mensaje = "Depósito realizado exitosamente",
                    cuentaId,
                    saldoActual = cuenta.Saldo,
                    cuenta.Propietario
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Retirar dinero de una cuenta
        /// </summary>
        [HttpPost("{cuentaId:guid}/retirar")]
        public async Task<ActionResult<object>> Retirar(Guid cuentaId, [FromBody] OperacionMonetariaRequest request)
        {
            try
            {
                await _cuentaService.RetirarAsync(cuentaId, request.Monto, request.Propietario);
                var cuenta = await _cuentaService.ObtenerCuentaAsync(cuentaId);
                return Ok(new
                {
                    mensaje = "Retiro realizado exitosamente",
                    cuentaId,
                    saldoActual = cuenta.Saldo,
                    cuenta.Propietario
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
