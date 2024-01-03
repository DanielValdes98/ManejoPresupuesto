using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "La longitud del campo {0} debe estar entre {2} y {1}")]
        [Display(Name = "Nombre del tipo cuenta")]
        [PrimeraLetraMayuscula]
        [Remote(action:"verificarExisteTipoCuenta", controller:"TiposCuentas")]
        //[RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "No se permiten espacios en blanco al principio o al final")]
        public string Nombre { get; set; }
        public int Usuarioid { get; set; }
        public int Orden { get; set; }

        /*
        // Pruebas de otras validaciones por defecto
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electronico valido")] 
        public string Email { get; set; }

        [Range(minimum:18, maximum:100, ErrorMessage = "El valor de {0} debe estar entre {1} y {2}")]
        public int Edad { get; set; }

        [Url(ErrorMessage = "El campo debe ser una URL valida")]
        public string URL { get; set; }

        [CreditCard(ErrorMessage = "La tarjeta de credito no es valida")]
        [Display(Name = "Tarjeta de crédito")]
        public string TarjetaDeCredito { get; set; }
        */
    }
}
