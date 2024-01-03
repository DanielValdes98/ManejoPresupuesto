using Azure.Core;
using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public IRepositorioTiposCuentas RepositorioTiposCuentas { get; }

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioid);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.Usuarioid = servicioUsuarios.ObtenerUsuariosId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.Usuarioid);

            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre),
                    $"El nombre {tipoCuenta.Nombre} ya existe.");

                return View(tipoCuenta);
            }

            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioid);
            
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioid);           

            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioid);

            if (tipoCuenta is null)
            {
                return RedirectToAction ("NoEncontrado", "Home");
            }

            return View(tipoCuenta);

        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioid);

            if (tipoCuenta is null)
            {
                return RedirectToAction ("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");

        }

        // Validaciones personalizados con JavaScript - Remote:
        [HttpGet]
        public async Task<IActionResult> verificarExisteTipoCuenta(string nombre)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioid);

            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuariosId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioid);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            // Verificar si los ids de tipos cuentas le pertenezcan al usuario, es una validacion:
            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid(); // Prohibe
            }

            var tiposCuentasOrdenados = ids.Select((valor,indice) =>
                        new TipoCuenta() { Id = valor, Orden = indice +1 }).AsEnumerable();

            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }
    }
}
