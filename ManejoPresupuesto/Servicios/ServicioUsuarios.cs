using System.Security.Claims;

namespace ManejoPresupuesto.Servicios
{
    public interface IServicioUsuarios
    {
        int ObtenerUsuariosId();
    }
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly HttpContext httpContext;
        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor) 
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public int ObtenerUsuariosId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                // Solamente si el usuario está autenticado
                var idClaim = httpContext.User.Claims
                    .Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault(); // Un Claim es una información acerca de un usuario (email, nombre, id, entre otros)
                
                var id = int.Parse(idClaim.Value);

                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado");
            }
        }
    }
}
