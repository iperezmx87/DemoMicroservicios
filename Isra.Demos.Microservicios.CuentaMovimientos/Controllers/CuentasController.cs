using Isra.Demos.Banking.CurrentAccount.Modelo;
using Isra.Demos.Banking.CurrentAccount.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isra.Demos.Banking.CurrentAccount.Controllers
{
    /// <summary>
    /// Controlador para operaciones de cuentas bancarias
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
                await _cuentaService.DepositarAsync(cuentaId, request.Monto);
                var cuenta = await _cuentaService.ObtenerCuentaAsync(cuentaId);
                return Ok(new
                {
                    mensaje = "Depósito realizado exitosamente",
                    cuentaId,
                    saldoActual = cuenta.Saldo
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidDataException ide)
            {
                return BadRequest(new { mensaje = ide.Message });
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
                await _cuentaService.RetirarAsync(cuentaId, request.Monto);
                var cuenta = await _cuentaService.ObtenerCuentaAsync(cuentaId);
                return Ok(new
                {
                    mensaje = "Retiro realizado exitosamente",
                    cuentaId,
                    saldoActual = cuenta.Saldo
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidDataException ide)
            {
                return BadRequest(new { mensaje = ide.Message });
            }
        }

        /// <summary>
        /// Transferir dinero entre dos cuentas
        /// </summary>
        [HttpPost("transferir")]
        public async Task<ActionResult<object>> Transferir([FromBody] TransferenciaRequest request)
        {
            try
            {
                await _cuentaService.TransferirAsync(
                    request.CuentaOrigenId,
                    request.CuentaDestinoId,
                    request.Monto
                );

                var cuentaOrigen = await _cuentaService.ObtenerCuentaAsync(request.CuentaOrigenId);

                return Ok(new
                {
                    mensaje = "Transferencia realizada exitosamente",
                    cuentaOrigenId = request.CuentaOrigenId,
                    saldoActualOrigen = cuentaOrigen.Saldo,
                    cuentaDestinoId = request.CuentaDestinoId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidDataException ide)
            {
                return BadRequest(new { mensaje = ide.Message });
            }
        }
    }
}
