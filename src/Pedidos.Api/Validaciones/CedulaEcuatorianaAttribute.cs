using System.ComponentModel.DataAnnotations;

namespace Pedidos.Api.Validaciones;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class CedulaEcuatorianaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var cedula = value as string;

        // 1. La validación se detiene en el primer error para indicar exactamente qué corregir.
        if (string.IsNullOrEmpty(cedula))
        {
            return new ValidationResult("La cédula es obligatoria.");
        }

        // 2. Solo se aceptan dígitos del 0 al 9, sin recortar espacios ni aceptar guiones.
        if (cedula.Any(caracter => caracter < '0' || caracter > '9'))
        {
            return new ValidationResult("La cédula solo debe contener números (sin guiones ni espacios).");
        }

        // 3 y 4. Se comprueba la longitud antes de acceder a las posiciones de la cédula.
        if (cedula.Length < 10)
        {
            return new ValidationResult($"La cédula está incompleta: debe tener 10 dígitos (tiene {cedula.Length}).");
        }

        if (cedula.Length > 10)
        {
            return new ValidationResult($"La cédula tiene demasiados dígitos: debe tener 10 (tiene {cedula.Length}).");
        }

        // 5. Los dos primeros dígitos deben representar una provincia 01–24 o el código 30.
        var provincia = (cedula[0] - '0') * 10 + cedula[1] - '0';
        if ((provincia < 1 || provincia > 24) && provincia != 30)
        {
            return new ValidationResult($"La cédula no es válida: los dos primeros dígitos ({cedula[..2]}) no corresponden a una provincia del Ecuador.");
        }

        // 6. Para una cédula de persona natural, el tercer dígito debe estar entre 0 y 5.
        if (cedula[2] - '0' >= 6)
        {
            return new ValidationResult("La cédula no es válida: el tercer dígito debe ser menor que 6.");
        }

        // 7. Módulo 10: multiplicar los nueve primeros dígitos por 2,1,2,1,2,1,2,1,2.
        // Si un producto supera 9, restar 9; luego sumar los nueve resultados.
        var suma = 0;
        for (var indice = 0; indice < 9; indice++)
        {
            var producto = (cedula[indice] - '0') * (indice % 2 == 0 ? 2 : 1);
            suma += producto > 9 ? producto - 9 : producto;
        }

        // El complemento al siguiente múltiplo de 10 es el verificador (0 si ya es múltiplo).
        // Solo se acepta cuando coincide con el décimo dígito recibido.
        var verificador = (10 - suma % 10) % 10;
        if (verificador != cedula[9] - '0')
        {
            return new ValidationResult("La cédula no es válida: el dígito verificador no coincide. Verifique que sea una cédula real.");
        }

        return ValidationResult.Success;
    }
}
