using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Catalogo.Api.DTOs;

public class ProductoPatchDto : IValidatableObject
{
    private string? _nombre;
    private string? _sku;
    private decimal? _precio;
    private string? _estado;

    [StringLength(150, ErrorMessage = "El nombre del producto no puede superar los 150 caracteres.")]
    public string? Nombre
    {
        get => _nombre;
        set { _nombre = value; NombreEnviado = true; }
    }

    [StringLength(30, ErrorMessage = "El SKU no puede superar los 30 caracteres.")]
    public string? SKU
    {
        get => _sku;
        set { _sku = value; SkuEnviado = true; }
    }

    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "El precio debe ser mayor que 0.")]
    public decimal? Precio
    {
        get => _precio;
        set { _precio = value; PrecioEnviado = true; }
    }

    [RegularExpression("^(ACTIVO|INACTIVO)$", ErrorMessage = "El estado debe ser ACTIVO o INACTIVO.")]
    public string? Estado
    {
        get => _estado;
        set { _estado = value; EstadoEnviado = true; }
    }

    // Distinguen un campo omitido de uno enviado explícitamente como null.
    [JsonIgnore] public bool NombreEnviado { get; private set; }
    [JsonIgnore] public bool SkuEnviado { get; private set; }
    [JsonIgnore] public bool PrecioEnviado { get; private set; }
    [JsonIgnore] public bool EstadoEnviado { get; private set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!NombreEnviado && !SkuEnviado && !PrecioEnviado && !EstadoEnviado)
        {
            yield return new ValidationResult(
                "Debe enviar al menos uno de estos campos: nombre, sku, precio, estado.");
        }
        if (NombreEnviado && string.IsNullOrWhiteSpace(Nombre))
        {
            yield return new ValidationResult("El nombre del producto es obligatorio.", [nameof(Nombre)]);
        }
        if (SkuEnviado && string.IsNullOrWhiteSpace(SKU))
        {
            yield return new ValidationResult("El SKU es obligatorio.", [nameof(SKU)]);
        }
        if (PrecioEnviado && Precio is null)
        {
            yield return new ValidationResult("El precio es obligatorio.", [nameof(Precio)]);
        }
        if (EstadoEnviado && string.IsNullOrWhiteSpace(Estado))
        {
            yield return new ValidationResult("El estado debe ser ACTIVO o INACTIVO.", [nameof(Estado)]);
        }
    }
}
