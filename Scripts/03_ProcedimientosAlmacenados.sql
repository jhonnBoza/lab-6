USE NeptunoDB;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE usp_Categorias_Listar
    @IncluirInactivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  IdCategoria,
            NombreCategoria,
            Descripcion,
            Activo
    FROM    Categorias
    WHERE   Activo = 1 OR @IncluirInactivos = 1
    ORDER BY Activo DESC, NombreCategoria;
END
GO

CREATE OR ALTER PROCEDURE usp_Categorias_Obtener
    @IdCategoria INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  IdCategoria,
            NombreCategoria,
            Descripcion,
            Activo
    FROM    Categorias
    WHERE   IdCategoria = @IdCategoria;
END
GO

CREATE OR ALTER PROCEDURE usp_Categorias_Insertar
    @NombreCategoria NVARCHAR(50),
    @Descripcion     NVARCHAR(500) = NULL,
    @NuevoId         INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Categorias
               WHERE NombreCategoria = @NombreCategoria AND Activo = 1)
    BEGIN
        RAISERROR (N'Ya existe una categoria activa con ese nombre.', 16, 1);
        RETURN;
    END

    INSERT INTO Categorias (NombreCategoria, Descripcion, Activo)
    VALUES (@NombreCategoria, @Descripcion, 1);

    SET @NuevoId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE usp_Categorias_Actualizar
    @IdCategoria     INT,
    @NombreCategoria NVARCHAR(50),
    @Descripcion     NVARCHAR(500) = NULL,
    @FilasAfectadas  INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Categorias WHERE IdCategoria = @IdCategoria)
    BEGIN
        RAISERROR (N'La categoria indicada no existe.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Categorias
               WHERE NombreCategoria = @NombreCategoria
                 AND IdCategoria <> @IdCategoria
                 AND Activo = 1)
    BEGIN
        RAISERROR (N'Ya existe otra categoria activa con ese nombre.', 16, 1);
        RETURN;
    END

    UPDATE  Categorias
    SET     NombreCategoria = @NombreCategoria,
            Descripcion     = @Descripcion
    WHERE   IdCategoria     = @IdCategoria;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Categorias_Eliminar
    @IdCategoria    INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Categorias WHERE IdCategoria = @IdCategoria)
    BEGIN
        RAISERROR (N'La categoria indicada no existe.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Productos
               WHERE IdCategoria = @IdCategoria AND Activo = 1)
    BEGIN
        RAISERROR (N'No se puede dar de baja: la categoria tiene productos activos.', 16, 1);
        RETURN;
    END

    UPDATE  Categorias
    SET     Activo      = 0
    WHERE   IdCategoria = @IdCategoria
      AND   Activo      = 1;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Categorias_Restaurar
    @IdCategoria    INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Categorias c
               WHERE c.Activo = 1
                 AND c.IdCategoria <> @IdCategoria
                 AND c.NombreCategoria = (SELECT NombreCategoria FROM Categorias
                                          WHERE IdCategoria = @IdCategoria))
    BEGIN
        RAISERROR (N'No se puede restaurar: ya hay otra categoria activa con ese nombre.', 16, 1);
        RETURN;
    END

    UPDATE  Categorias
    SET     Activo      = 1
    WHERE   IdCategoria = @IdCategoria
      AND   Activo      = 0;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Listar
    @IncluirInactivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  IdProveedor, NombreCompania, NombreContacto, CargoContacto,
            Direccion, Ciudad, Region, CodPostal, Pais, Telefono, Fax, Activo
    FROM    Proveedores
    WHERE   Activo = 1 OR @IncluirInactivos = 1
    ORDER BY Activo DESC, NombreCompania;
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Obtener
    @IdProveedor INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  IdProveedor, NombreCompania, NombreContacto, CargoContacto,
            Direccion, Ciudad, Region, CodPostal, Pais, Telefono, Fax, Activo
    FROM    Proveedores
    WHERE   IdProveedor = @IdProveedor;
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Buscar
    @NombreContacto   NVARCHAR(60) = NULL,
    @Ciudad           NVARCHAR(50) = NULL,
    @IncluirInactivos BIT          = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  IdProveedor, NombreCompania, NombreContacto, CargoContacto,
            Direccion, Ciudad, Region, CodPostal, Pais, Telefono, Fax, Activo
    FROM    Proveedores
    WHERE   (Activo = 1 OR @IncluirInactivos = 1)
      AND   (@NombreContacto IS NULL OR LTRIM(RTRIM(@NombreContacto)) = N''
             OR NombreContacto LIKE N'%' + @NombreContacto + N'%')
      AND   (@Ciudad IS NULL OR LTRIM(RTRIM(@Ciudad)) = N''
             OR Ciudad LIKE N'%' + @Ciudad + N'%')
    ORDER BY Activo DESC, NombreCompania;
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Insertar
    @NombreCompania NVARCHAR(60),
    @NombreContacto NVARCHAR(60)  = NULL,
    @CargoContacto  NVARCHAR(60)  = NULL,
    @Direccion      NVARCHAR(120) = NULL,
    @Ciudad         NVARCHAR(50)  = NULL,
    @Region         NVARCHAR(50)  = NULL,
    @CodPostal      NVARCHAR(15)  = NULL,
    @Pais           NVARCHAR(50)  = NULL,
    @Telefono       NVARCHAR(30)  = NULL,
    @Fax            NVARCHAR(30)  = NULL,
    @NuevoId        INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Proveedores (NombreCompania, NombreContacto, CargoContacto, Direccion,
                             Ciudad, Region, CodPostal, Pais, Telefono, Fax, Activo)
    VALUES (@NombreCompania, @NombreContacto, @CargoContacto, @Direccion,
            @Ciudad, @Region, @CodPostal, @Pais, @Telefono, @Fax, 1);

    SET @NuevoId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Actualizar
    @IdProveedor    INT,
    @NombreCompania NVARCHAR(60),
    @NombreContacto NVARCHAR(60)  = NULL,
    @CargoContacto  NVARCHAR(60)  = NULL,
    @Direccion      NVARCHAR(120) = NULL,
    @Ciudad         NVARCHAR(50)  = NULL,
    @Region         NVARCHAR(50)  = NULL,
    @CodPostal      NVARCHAR(15)  = NULL,
    @Pais           NVARCHAR(50)  = NULL,
    @Telefono       NVARCHAR(30)  = NULL,
    @Fax            NVARCHAR(30)  = NULL,
    @FilasAfectadas INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Proveedores WHERE IdProveedor = @IdProveedor)
    BEGIN
        RAISERROR (N'El proveedor indicado no existe.', 16, 1);
        RETURN;
    END

    UPDATE  Proveedores
    SET     NombreCompania = @NombreCompania,
            NombreContacto = @NombreContacto,
            CargoContacto  = @CargoContacto,
            Direccion      = @Direccion,
            Ciudad         = @Ciudad,
            Region         = @Region,
            CodPostal      = @CodPostal,
            Pais           = @Pais,
            Telefono       = @Telefono,
            Fax            = @Fax
    WHERE   IdProveedor    = @IdProveedor;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Eliminar
    @IdProveedor    INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Proveedores WHERE IdProveedor = @IdProveedor)
    BEGIN
        RAISERROR (N'El proveedor indicado no existe.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Productos
               WHERE IdProveedor = @IdProveedor AND Activo = 1)
    BEGIN
        RAISERROR (N'No se puede dar de baja: el proveedor tiene productos activos.', 16, 1);
        RETURN;
    END

    UPDATE  Proveedores
    SET     Activo      = 0
    WHERE   IdProveedor = @IdProveedor
      AND   Activo      = 1;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Proveedores_Restaurar
    @IdProveedor    INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE  Proveedores
    SET     Activo      = 1
    WHERE   IdProveedor = @IdProveedor
      AND   Activo      = 0;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Productos_Listar
    @IncluirInactivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.IdProducto,
            p.NombreProducto,
            p.IdProveedor,
            pr.NombreCompania    AS NombreProveedor,
            p.IdCategoria,
            c.NombreCategoria,
            p.CantidadPorUnidad,
            p.PrecioUnidad,
            p.UnidadesEnExistencia,
            p.UnidadesEnPedido,
            p.NivelNuevoPedido,
            p.Suspendido,
            p.Activo
    FROM        Productos   p
    LEFT JOIN   Proveedores pr ON pr.IdProveedor = p.IdProveedor
    LEFT JOIN   Categorias  c  ON c.IdCategoria  = p.IdCategoria
    WHERE       p.Activo = 1 OR @IncluirInactivos = 1
    ORDER BY p.Activo DESC, p.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE usp_Productos_Obtener
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  p.IdProducto,
            p.NombreProducto,
            p.IdProveedor,
            pr.NombreCompania    AS NombreProveedor,
            p.IdCategoria,
            c.NombreCategoria,
            p.CantidadPorUnidad,
            p.PrecioUnidad,
            p.UnidadesEnExistencia,
            p.UnidadesEnPedido,
            p.NivelNuevoPedido,
            p.Suspendido,
            p.Activo
    FROM        Productos   p
    LEFT JOIN   Proveedores pr ON pr.IdProveedor = p.IdProveedor
    LEFT JOIN   Categorias  c  ON c.IdCategoria  = p.IdCategoria
    WHERE   p.IdProducto = @IdProducto;
END
GO

CREATE OR ALTER PROCEDURE usp_Productos_Insertar
    @NombreProducto       NVARCHAR(80),
    @IdProveedor          INT           = NULL,
    @IdCategoria          INT           = NULL,
    @CantidadPorUnidad    NVARCHAR(40)  = NULL,
    @PrecioUnidad         DECIMAL(10,2) = 0,
    @UnidadesEnExistencia SMALLINT      = 0,
    @UnidadesEnPedido     SMALLINT      = 0,
    @NivelNuevoPedido     SMALLINT      = 0,
    @Suspendido           BIT           = 0,
    @NuevoId              INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PrecioUnidad < 0
    BEGIN
        RAISERROR (N'El precio unitario no puede ser negativo.', 16, 1);
        RETURN;
    END

    IF @IdCategoria IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Categorias WHERE IdCategoria = @IdCategoria AND Activo = 1)
    BEGIN
        RAISERROR (N'La categoria indicada no existe o esta dada de baja.', 16, 1);
        RETURN;
    END

    IF @IdProveedor IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Proveedores WHERE IdProveedor = @IdProveedor AND Activo = 1)
    BEGIN
        RAISERROR (N'El proveedor indicado no existe o esta dado de baja.', 16, 1);
        RETURN;
    END

    INSERT INTO Productos (NombreProducto, IdProveedor, IdCategoria, CantidadPorUnidad,
                           PrecioUnidad, UnidadesEnExistencia, UnidadesEnPedido,
                           NivelNuevoPedido, Suspendido, Activo)
    VALUES (@NombreProducto, @IdProveedor, @IdCategoria, @CantidadPorUnidad,
            @PrecioUnidad, @UnidadesEnExistencia, @UnidadesEnPedido,
            @NivelNuevoPedido, @Suspendido, 1);

    SET @NuevoId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE usp_Productos_Actualizar
    @IdProducto           INT,
    @NombreProducto       NVARCHAR(80),
    @IdProveedor          INT           = NULL,
    @IdCategoria          INT           = NULL,
    @CantidadPorUnidad    NVARCHAR(40)  = NULL,
    @PrecioUnidad         DECIMAL(10,2) = 0,
    @UnidadesEnExistencia SMALLINT      = 0,
    @UnidadesEnPedido     SMALLINT      = 0,
    @NivelNuevoPedido     SMALLINT      = 0,
    @Suspendido           BIT           = 0,
    @FilasAfectadas       INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto)
    BEGIN
        RAISERROR (N'El producto indicado no existe.', 16, 1);
        RETURN;
    END

    IF @PrecioUnidad < 0
    BEGIN
        RAISERROR (N'El precio unitario no puede ser negativo.', 16, 1);
        RETURN;
    END

    UPDATE  Productos
    SET     NombreProducto       = @NombreProducto,
            IdProveedor          = @IdProveedor,
            IdCategoria          = @IdCategoria,
            CantidadPorUnidad    = @CantidadPorUnidad,
            PrecioUnidad         = @PrecioUnidad,
            UnidadesEnExistencia = @UnidadesEnExistencia,
            UnidadesEnPedido     = @UnidadesEnPedido,
            NivelNuevoPedido     = @NivelNuevoPedido,
            Suspendido           = @Suspendido
    WHERE   IdProducto           = @IdProducto;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Productos_Eliminar
    @IdProducto     INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto)
    BEGIN
        RAISERROR (N'El producto indicado no existe.', 16, 1);
        RETURN;
    END

    UPDATE  Productos
    SET     Activo     = 0
    WHERE   IdProducto = @IdProducto
      AND   Activo     = 1;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Productos_Restaurar
    @IdProducto     INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Productos p
               INNER JOIN Categorias c ON c.IdCategoria = p.IdCategoria
               WHERE p.IdProducto = @IdProducto AND c.Activo = 0)
    BEGIN
        RAISERROR (N'No se puede restaurar: la categoria del producto esta dada de baja.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Productos p
               INNER JOIN Proveedores pr ON pr.IdProveedor = p.IdProveedor
               WHERE p.IdProducto = @IdProducto AND pr.Activo = 0)
    BEGIN
        RAISERROR (N'No se puede restaurar: el proveedor del producto esta dado de baja.', 16, 1);
        RETURN;
    END

    UPDATE  Productos
    SET     Activo     = 1
    WHERE   IdProducto = @IdProducto
      AND   Activo     = 0;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Pedidos_Listar
    @IncluirInactivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ped.IdPedido,
            ped.IdCliente,
            cli.NombreCompania AS NombreCliente,
            ped.IdEmpleado,
            emp.Apellidos + N', ' + emp.Nombre AS NombreEmpleado,
            ped.FechaPedido,
            ped.FechaEntrega,
            ped.FechaEnvio,
            ped.Cargo,
            ped.Destinatario,
            ped.DireccionDestinatario,
            ped.CiudadDestinatario,
            ped.PaisDestinatario,
            ISNULL(det.Total, 0) AS TotalPedido,
            ped.Activo
    FROM        Pedidos   ped
    LEFT JOIN   Clientes  cli ON cli.IdCliente  = ped.IdCliente
    LEFT JOIN   Empleados emp ON emp.IdEmpleado = ped.IdEmpleado
    OUTER APPLY (SELECT SUM(d.PrecioUnidad * d.Cantidad * (1 - d.Descuento)) AS Total
                 FROM   DetallesPedidos d
                 WHERE  d.IdPedido = ped.IdPedido) det
    WHERE       ped.Activo = 1 OR @IncluirInactivos = 1
    ORDER BY ped.Activo DESC, ped.IdPedido DESC;
END
GO

CREATE OR ALTER PROCEDURE usp_Pedidos_Obtener
    @IdPedido INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ped.IdPedido,
            ped.IdCliente,
            cli.NombreCompania AS NombreCliente,
            ped.IdEmpleado,
            emp.Apellidos + N', ' + emp.Nombre AS NombreEmpleado,
            ped.FechaPedido,
            ped.FechaEntrega,
            ped.FechaEnvio,
            ped.Cargo,
            ped.Destinatario,
            ped.DireccionDestinatario,
            ped.CiudadDestinatario,
            ped.PaisDestinatario,
            ISNULL(det.Total, 0) AS TotalPedido,
            ped.Activo
    FROM        Pedidos   ped
    LEFT JOIN   Clientes  cli ON cli.IdCliente  = ped.IdCliente
    LEFT JOIN   Empleados emp ON emp.IdEmpleado = ped.IdEmpleado
    OUTER APPLY (SELECT SUM(d.PrecioUnidad * d.Cantidad * (1 - d.Descuento)) AS Total
                 FROM   DetallesPedidos d
                 WHERE  d.IdPedido = ped.IdPedido) det
    WHERE   ped.IdPedido = @IdPedido;
END
GO

CREATE OR ALTER PROCEDURE usp_Pedidos_Insertar
    @IdCliente             NCHAR(5)      = NULL,
    @IdEmpleado            INT           = NULL,
    @FechaPedido           DATETIME      = NULL,
    @FechaEntrega          DATETIME      = NULL,
    @FechaEnvio            DATETIME      = NULL,
    @Cargo                 DECIMAL(10,2) = 0,
    @Destinatario          NVARCHAR(60)  = NULL,
    @DireccionDestinatario NVARCHAR(120) = NULL,
    @CiudadDestinatario    NVARCHAR(50)  = NULL,
    @PaisDestinatario      NVARCHAR(50)  = NULL,
    @NuevoId               INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaEntrega IS NOT NULL AND @FechaPedido IS NOT NULL AND @FechaEntrega < @FechaPedido
    BEGIN
        RAISERROR (N'La fecha de entrega no puede ser anterior a la fecha del pedido.', 16, 1);
        RETURN;
    END

    INSERT INTO Pedidos (IdCliente, IdEmpleado, FechaPedido, FechaEntrega, FechaEnvio,
                         Cargo, Destinatario, DireccionDestinatario,
                         CiudadDestinatario, PaisDestinatario, Activo)
    VALUES (@IdCliente, @IdEmpleado, @FechaPedido, @FechaEntrega, @FechaEnvio,
            @Cargo, @Destinatario, @DireccionDestinatario,
            @CiudadDestinatario, @PaisDestinatario, 1);

    SET @NuevoId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE usp_Pedidos_Actualizar
    @IdPedido              INT,
    @IdCliente             NCHAR(5)      = NULL,
    @IdEmpleado            INT           = NULL,
    @FechaPedido           DATETIME      = NULL,
    @FechaEntrega          DATETIME      = NULL,
    @FechaEnvio            DATETIME      = NULL,
    @Cargo                 DECIMAL(10,2) = 0,
    @Destinatario          NVARCHAR(60)  = NULL,
    @DireccionDestinatario NVARCHAR(120) = NULL,
    @CiudadDestinatario    NVARCHAR(50)  = NULL,
    @PaisDestinatario      NVARCHAR(50)  = NULL,
    @FilasAfectadas        INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE IdPedido = @IdPedido)
    BEGIN
        RAISERROR (N'El pedido indicado no existe.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE IdPedido = @IdPedido AND Activo = 1)
    BEGIN
        RAISERROR (N'El pedido esta dado de baja: restaurelo antes de modificarlo.', 16, 1);
        RETURN;
    END

    IF @FechaEntrega IS NOT NULL AND @FechaPedido IS NOT NULL AND @FechaEntrega < @FechaPedido
    BEGIN
        RAISERROR (N'La fecha de entrega no puede ser anterior a la fecha del pedido.', 16, 1);
        RETURN;
    END

    UPDATE  Pedidos
    SET     IdCliente             = @IdCliente,
            IdEmpleado            = @IdEmpleado,
            FechaPedido           = @FechaPedido,
            FechaEntrega          = @FechaEntrega,
            FechaEnvio            = @FechaEnvio,
            Cargo                 = @Cargo,
            Destinatario          = @Destinatario,
            DireccionDestinatario = @DireccionDestinatario,
            CiudadDestinatario    = @CiudadDestinatario,
            PaisDestinatario      = @PaisDestinatario
    WHERE   IdPedido              = @IdPedido;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Pedidos_Eliminar
    @IdPedido       INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE IdPedido = @IdPedido)
    BEGIN
        RAISERROR (N'El pedido indicado no existe.', 16, 1);
        RETURN;
    END

    UPDATE  Pedidos
    SET     Activo   = 0
    WHERE   IdPedido = @IdPedido
      AND   Activo   = 1;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Pedidos_Restaurar
    @IdPedido       INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE  Pedidos
    SET     Activo   = 1
    WHERE   IdPedido = @IdPedido
      AND   Activo   = 0;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_DetallesPedidos_ListarPorPedido
    @IdPedido INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  d.IdPedido,
            d.IdProducto,
            p.NombreProducto,
            d.PrecioUnidad,
            d.Cantidad,
            d.Descuento,
            CAST(d.PrecioUnidad * d.Cantidad * (1 - d.Descuento) AS DECIMAL(12,2)) AS Subtotal
    FROM        DetallesPedidos d
    INNER JOIN  Productos       p ON p.IdProducto = d.IdProducto
    WHERE   d.IdPedido = @IdPedido
    ORDER BY p.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE usp_DetallesPedidos_Insertar
    @IdPedido       INT,
    @IdProducto     INT,
    @PrecioUnidad   DECIMAL(10,2),
    @Cantidad       SMALLINT,
    @Descuento      DECIMAL(4,2) = 0,
    @FilasAfectadas INT          = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE IdPedido = @IdPedido AND Activo = 1)
    BEGIN
        RAISERROR (N'No se pueden agregar lineas a un pedido dado de baja.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto AND Activo = 1)
    BEGIN
        RAISERROR (N'El producto indicado esta dado de baja.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM DetallesPedidos
               WHERE IdPedido = @IdPedido AND IdProducto = @IdProducto)
    BEGIN
        RAISERROR (N'El producto ya fue agregado a este pedido.', 16, 1);
        RETURN;
    END

    INSERT INTO DetallesPedidos (IdPedido, IdProducto, PrecioUnidad, Cantidad, Descuento)
    VALUES (@IdPedido, @IdProducto, @PrecioUnidad, @Cantidad, @Descuento);

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_DetallesPedidos_Actualizar
    @IdPedido       INT,
    @IdProducto     INT,
    @PrecioUnidad   DECIMAL(10,2),
    @Cantidad       SMALLINT,
    @Descuento      DECIMAL(4,2) = 0,
    @FilasAfectadas INT          = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE IdPedido = @IdPedido AND Activo = 1)
    BEGIN
        RAISERROR (N'No se puede modificar el detalle de un pedido dado de baja.', 16, 1);
        RETURN;
    END

    UPDATE  DetallesPedidos
    SET     PrecioUnidad = @PrecioUnidad,
            Cantidad     = @Cantidad,
            Descuento    = @Descuento
    WHERE   IdPedido     = @IdPedido
      AND   IdProducto   = @IdProducto;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_DetallesPedidos_Eliminar
    @IdPedido       INT,
    @IdProducto     INT,
    @FilasAfectadas INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE IdPedido = @IdPedido AND Activo = 1)
    BEGIN
        RAISERROR (N'No se puede modificar el detalle de un pedido dado de baja.', 16, 1);
        RETURN;
    END

    DELETE FROM DetallesPedidos
    WHERE IdPedido = @IdPedido AND IdProducto = @IdProducto;

    SET @FilasAfectadas = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE usp_Reporte_DetallesPedidosPorFechas
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio > @FechaFin
    BEGIN
        RAISERROR (N'La fecha inicial no puede ser mayor que la fecha final.', 16, 1);
        RETURN;
    END

    SELECT  ped.IdPedido,
            ped.FechaPedido,
            ped.IdCliente,
            cli.NombreCompania                  AS NombreCliente,
            det.IdProducto,
            pro.NombreProducto,
            cat.NombreCategoria,
            det.PrecioUnidad,
            det.Cantidad,
            det.Descuento,
            CAST(det.PrecioUnidad * det.Cantidad * (1 - det.Descuento) AS DECIMAL(12,2)) AS Subtotal
    FROM        DetallesPedidos det
    INNER JOIN  Pedidos         ped ON ped.IdPedido   = det.IdPedido
    INNER JOIN  Productos       pro ON pro.IdProducto = det.IdProducto
    LEFT  JOIN  Categorias      cat ON cat.IdCategoria = pro.IdCategoria
    LEFT  JOIN  Clientes        cli ON cli.IdCliente   = ped.IdCliente
    WHERE   ped.Activo = 1
      AND   ped.FechaPedido >= @FechaInicio
      AND   ped.FechaPedido <  DATEADD(DAY, 1, @FechaFin)
    ORDER BY ped.FechaPedido, ped.IdPedido, pro.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE usp_Reporte_ResumenPorCategoria
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio > @FechaFin
    BEGIN
        RAISERROR (N'La fecha inicial no puede ser mayor que la fecha final.', 16, 1);
        RETURN;
    END

    SELECT  ISNULL(cat.NombreCategoria, N'(Sin categoria)') AS NombreCategoria,
            COUNT(DISTINCT ped.IdPedido)                    AS Pedidos,
            SUM(det.Cantidad)                               AS UnidadesVendidas,
            CAST(SUM(det.PrecioUnidad * det.Cantidad * (1 - det.Descuento)) AS DECIMAL(12,2)) AS Importe
    FROM        DetallesPedidos det
    INNER JOIN  Pedidos         ped ON ped.IdPedido   = det.IdPedido
    INNER JOIN  Productos       pro ON pro.IdProducto = det.IdProducto
    LEFT  JOIN  Categorias      cat ON cat.IdCategoria = pro.IdCategoria
    WHERE   ped.Activo = 1
      AND   ped.FechaPedido >= @FechaInicio
      AND   ped.FechaPedido <  DATEADD(DAY, 1, @FechaFin)
    GROUP BY cat.NombreCategoria
    ORDER BY Importe DESC;
END
GO

CREATE OR ALTER PROCEDURE usp_Reporte_PedidosDadosDeBaja
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio > @FechaFin
    BEGIN
        RAISERROR (N'La fecha inicial no puede ser mayor que la fecha final.', 16, 1);
        RETURN;
    END

    SELECT  ped.IdPedido,
            ped.FechaPedido,
            ped.IdCliente,
            cli.NombreCompania        AS NombreCliente,
            COUNT(det.IdProducto)     AS Lineas,
            CAST(ISNULL(SUM(det.PrecioUnidad * det.Cantidad * (1 - det.Descuento)), 0)
                 AS DECIMAL(12,2))    AS Importe
    FROM        Pedidos         ped
    LEFT  JOIN  DetallesPedidos det ON det.IdPedido  = ped.IdPedido
    LEFT  JOIN  Clientes        cli ON cli.IdCliente = ped.IdCliente
    WHERE   ped.Activo = 0
      AND   ped.FechaPedido >= @FechaInicio
      AND   ped.FechaPedido <  DATEADD(DAY, 1, @FechaFin)
    GROUP BY ped.IdPedido, ped.FechaPedido, ped.IdCliente, cli.NombreCompania
    ORDER BY ped.FechaPedido, ped.IdPedido;
END
GO

CREATE OR ALTER PROCEDURE usp_Clientes_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdCliente, NombreCompania, NombreContacto, Ciudad, Pais, Telefono
    FROM   Clientes
    ORDER BY NombreCompania;
END
GO

CREATE OR ALTER PROCEDURE usp_Empleados_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdEmpleado,
           Apellidos,
           Nombre,
           Apellidos + N', ' + Nombre AS NombreCompleto,
           Cargo
    FROM   Empleados
    ORDER BY Apellidos, Nombre;
END
GO

PRINT '>>> Procedimientos almacenados creados correctamente.';
GO
