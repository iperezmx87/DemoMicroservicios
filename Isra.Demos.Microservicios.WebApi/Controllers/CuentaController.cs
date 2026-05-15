using Isra.Demos.Microservicios.WebApi.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Isra.Demos.Microservicios.WebApi.Controllers
{
    /// <summary>
    /// Api de cuentas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CuentaController : ControllerBase
    {
        private readonly ISaldoRepositorio _saldoRepositorio;
        private readonly IEstadoCuentaRepositorio _estadoCuentaRepositorio;
        private readonly IGeneradorEstadoCuentaPdfService _generadorEstadoCuentaPdfService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="saldoRepositorio"></param>
        /// <param name="estadoCuentaRepositorio"></param>
        /// <param name="generadorEstadoCuentaPdfService"></param>
        public CuentaController(ISaldoRepositorio saldoRepositorio,
            IEstadoCuentaRepositorio estadoCuentaRepositorio,
            IGeneradorEstadoCuentaPdfService generadorEstadoCuentaPdfService)
        {
            _saldoRepositorio = saldoRepositorio;
            _estadoCuentaRepositorio = estadoCuentaRepositorio;
            _generadorEstadoCuentaPdfService = generadorEstadoCuentaPdfService;
        }

        /// <summary>
        /// Obtener el saldo actualizado de una cuenta después de una transacción, utilizando el ID de la cuenta. Este endpoint se utiliza para consultar el saldo actual de una cuenta específica después de que se haya realizado una transacción, proporcionando información actualizada al cliente.
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        [HttpGet("{cuentaId}/saldo")]
        public async Task<IActionResult> GetSaldo(Guid cuentaId)
        {
            var saldo = await _saldoRepositorio.GetSaldoActualAsync(cuentaId);
            return Ok(saldo);
        }

        /// <summary>
        /// Genera el estado de cuenta de una cuenta específica utilizando su ID. Este endpoint se utiliza para obtener un resumen
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        [HttpGet("{cuentaId}/estado-cuenta")]
        public async Task<IActionResult> GetEstadoCuenta(Guid cuentaId)
        {
            var estadoCuenta = await _estadoCuentaRepositorio.ObtenerEstadoCuentaAsync(cuentaId);

            if (estadoCuenta.Saldo == 0.00m)
                return NotFound("No se encontraron transacciones para esta cuenta.");

            return Ok(estadoCuenta);
        }

        /// <summary>
        /// Genera el pdf del estado de cuenta
        /// </summary>
        /// <param name="cuentaId"></param>
        /// <returns></returns>
        [HttpGet("{cuentaId}/estado-cuenta-pdf")]
        public async Task<IActionResult> CrearEstadoCuentaPDF(Guid cuentaId)
        {
            var estadoPdf = await _generadorEstadoCuentaPdfService.GenerarEstadoCuentaPdf(cuentaId);

            // 3. Retornar el archivo
            return File(estadoPdf, "application/pdf", $"EstadoCuenta_{cuentaId}.pdf");
        }
    }
}
