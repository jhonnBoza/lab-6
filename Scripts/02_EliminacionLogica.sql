USE NeptunoDB;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH('dbo.Categorias', 'Activo') IS NULL
    ALTER TABLE Categorias
        ADD Activo BIT NOT NULL CONSTRAINT DF_Categorias_Activo DEFAULT (1);
GO

IF COL_LENGTH('dbo.Proveedores', 'Activo') IS NULL
    ALTER TABLE Proveedores
        ADD Activo BIT NOT NULL CONSTRAINT DF_Proveedores_Activo DEFAULT (1);
GO

IF COL_LENGTH('dbo.Productos', 'Activo') IS NULL
    ALTER TABLE Productos
        ADD Activo BIT NOT NULL CONSTRAINT DF_Productos_Activo DEFAULT (1);
GO

IF COL_LENGTH('dbo.Pedidos', 'Activo') IS NULL
    ALTER TABLE Pedidos
        ADD Activo BIT NOT NULL CONSTRAINT DF_Pedidos_Activo DEFAULT (1);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Categorias_Activo')
    CREATE INDEX IX_Categorias_Activo  ON Categorias  (IdCategoria) WHERE Activo = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Proveedores_Activo')
    CREATE INDEX IX_Proveedores_Activo ON Proveedores (IdProveedor) WHERE Activo = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Productos_Activo')
    CREATE INDEX IX_Productos_Activo   ON Productos   (IdProducto)  WHERE Activo = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedidos_Activo')
    CREATE INDEX IX_Pedidos_Activo     ON Pedidos     (FechaPedido) WHERE Activo = 1;
GO

IF NOT EXISTS (SELECT 1 FROM Categorias WHERE NombreCategoria = N'Snacks')
    INSERT INTO Categorias (NombreCategoria, Descripcion, Activo)
    VALUES (N'Snacks', N'Piqueos y frutos secos (linea descontinuada)', 0);
GO

IF NOT EXISTS (SELECT 1 FROM Proveedores WHERE NombreCompania = N'Importadora Oriente SRL')
    INSERT INTO Proveedores (NombreCompania, NombreContacto, CargoContacto, Direccion,
                             Ciudad, Region, CodPostal, Pais, Telefono, Fax, Activo)
    VALUES (N'Importadora Oriente SRL', N'Hugo Paredes', N'Gerente General',
            N'Av. Javier Prado 3200', N'Lima', N'Lima', N'15036', N'Peru',
            N'(01) 622-4400', NULL, 0);
GO

IF NOT EXISTS (SELECT 1 FROM Productos WHERE NombreProducto = N'Galletas integrales 200g')
    INSERT INTO Productos (NombreProducto, IdProveedor, IdCategoria, CantidadPorUnidad,
                           PrecioUnidad, UnidadesEnExistencia, UnidadesEnPedido,
                           NivelNuevoPedido, Suspendido, Activo)
    VALUES (N'Galletas integrales 200g', 5, 3, N'24 paquetes', 5.10, 0, 0, 0, 1, 0);
GO

IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE Destinatario = N'Bodega Alfa'
                                       AND FechaPedido = '2026-09-01')
BEGIN
    INSERT INTO Pedidos (IdCliente, IdEmpleado, FechaPedido, FechaEntrega, FechaEnvio,
                         Cargo, Destinatario, DireccionDestinatario,
                         CiudadDestinatario, PaisDestinatario, Activo)
    VALUES (N'ALFKI', 2, '2026-09-01', '2026-09-11', NULL, 41.00, N'Bodega Alfa',
            N'Av. Brasil 1200', N'Lima', N'Peru', 0);

    DECLARE @IdPedidoAnulado INT = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO DetallesPedidos (IdPedido, IdProducto, PrecioUnidad, Cantidad, Descuento)
    VALUES (@IdPedidoAnulado,  1, 18.50, 20, 0.00),
           (@IdPedidoAnulado, 15, 24.00, 10, 0.05);
END
GO

SELECT 'Categorias'  AS Tabla, COUNT(*) AS Total,
       SUM(CAST(Activo AS INT)) AS Activos,
       COUNT(*) - SUM(CAST(Activo AS INT)) AS DadosDeBaja FROM Categorias
UNION ALL
SELECT 'Proveedores', COUNT(*), SUM(CAST(Activo AS INT)),
       COUNT(*) - SUM(CAST(Activo AS INT)) FROM Proveedores
UNION ALL
SELECT 'Productos',   COUNT(*), SUM(CAST(Activo AS INT)),
       COUNT(*) - SUM(CAST(Activo AS INT)) FROM Productos
UNION ALL
SELECT 'Pedidos',     COUNT(*), SUM(CAST(Activo AS INT)),
       COUNT(*) - SUM(CAST(Activo AS INT)) FROM Pedidos;
GO

PRINT '>>> Campo Activo agregado. La eliminacion logica ya puede usarse.';
GO
