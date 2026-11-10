namespace Inventario.Api.Clients;

public class CatalogoNoDisponibleException : Exception
{
    public CatalogoNoDisponibleException(string mensaje, Exception? excepcionInterna = null)
        : base(mensaje, excepcionInterna)
    {
    }
}
