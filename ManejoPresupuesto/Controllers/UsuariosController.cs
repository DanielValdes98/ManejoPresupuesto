using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;

        public UsuariosController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager) 
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario()
            {
                Email = modelo.Email
            };

            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);

            if(resultado.Succeeded) 
            {
                // Aun si el usuario cierra el navegador, va a seguir autenticado en la aplicacion
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transacciones");
            }

            // Si no, retorna los errores a nivel del modelo
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modelo);
            }

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // ESTO SE USA EN EL SERVICIO DE USUARIOS
            //if (User.Identity.IsAuthenticated)
            //{
            //    // Solamente si el usuario está autenticado
            //    var claims = User.Claims.ToList(); // Un Claim es una información acerca de un usuario (email, nombre, id, entre otros)
            //    var usuarioIdReal = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            //    var id = usuarioIdReal.Value;
            //}
            //else
            //{
            //    // Esto es si el usuario NO está autenticado
            //}

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if(!ModelState.IsValid) 
            {
                return View(modelo);
            }

            var resultado = await signInManager.PasswordSignInAsync(modelo.Email, modelo.Password,
                modelo.Recuerdame, lockoutOnFailure: false); // lockoutOnFailure: true -> Es cuando el usuario intenta varias veces el login, se le impide el acesso a la cuenta

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto.");
                return View(modelo);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Transacciones");
        }
    }
}
