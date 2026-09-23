namespace Lab06_DAEA.Datos.Modelos;

public abstract class EntidadConEstado
{
    public bool Activo { get; set; } = true;

    public string EstadoTexto => Activo ? "Activo" : "Dado de baja";
}

public class Categoria : EntidadConEstado
{
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public override string ToString() => NombreCategoria;
}

public class Proveedor : EntidadConEstado
{
    public int IdProveedor { get; set; }
    public string NombreCompania { get; set; } = string.Empty;
    public string? NombreContacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Region { get; set; }
    public string? CodPostal { get; set; }
    public string? Pais { get; set; }
    public string? Telefono { get; set; }
    public string? Fax { get; set; }

    public override string ToString() => NombreCompania;
}

public class Producto : EntidadConEstado
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int? IdProveedor { get; set; }
    public string? NombreProveedor { get; set; }
    public int? IdCategoria { get; set; }
    public string? NombreCategoria { get; set; }
    public string? CantidadPorUnidad { get; set; }
    public decimal PrecioUnidad { get; set; }
    public short UnidadesEnExistencia { get; set; }
    public short UnidadesEnPedido { get; set; }
    public short NivelNuevoPedido { get; set; }
    public bool Suspendido { get; set; }

    public override string ToString() => NombreProducto;
}

public class Pedido : EntidadConEstado
{
    public int IdPedido { get; set; }
    public string? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int? IdEmpleado { get; set; }
    public string? NombreEmpleado { get; set; }
    public DateTime? FechaPedido { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public decimal Cargo { get; set; }
    public string? Destinatario { get; set; }
    public string? DireccionDestinatario { get; set; }
    public string? CiudadDestinatario { get; set; }
    public string? PaisDestinatario { get; set; }
    public decimal TotalPedido { get; set; }
}

public class DetallePedido
{
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public decimal PrecioUnidad { get; set; }
    public short Cantidad { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}

public class Cliente
{
    public string IdCliente { get; set; } = string.Empty;
    public string NombreCompania { get; set; } = string.Empty;
    public string? NombreContacto { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public string? Telefono { get; set; }

    public override string ToString() => NombreCompania;
}

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string Apellidos { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cargo { get; set; }

    public override string ToString() => NombreCompleto;
}

public class ReporteDetallePedido
{
    public int IdPedido { get; set; }
    public DateTime? FechaPedido { get; set; }
    public string? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public string? NombreCategoria { get; set; }
    public decimal PrecioUnidad { get; set; }
    public short Cantidad { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}

public class ResumenCategoria
{
    public string NombreCategoria { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public int UnidadesVendidas { get; set; }
    public decimal Importe { get; set; }
}

public class PedidoDadoDeBaja
{
    public int IdPedido { get; set; }
    public DateTime? FechaPedido { get; set; }
    public string? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int Lineas { get; set; }
    public decimal Importe { get; set; }
}
