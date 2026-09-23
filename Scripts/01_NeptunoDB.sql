USE master;
GO

IF DB_ID('NeptunoDB') IS NOT NULL
BEGIN
    ALTER DATABASE NeptunoDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE NeptunoDB;
END
GO

CREATE DATABASE NeptunoDB;
GO

USE NeptunoDB;
GO

CREATE TABLE Categorias
(
    IdCategoria     INT IDENTITY(1,1)   NOT NULL,
    NombreCategoria NVARCHAR(50)        NOT NULL,
    Descripcion     NVARCHAR(500)       NULL,
    CONSTRAINT PK_Categorias PRIMARY KEY (IdCategoria)
);
GO

CREATE TABLE Proveedores
(
    IdProveedor     INT IDENTITY(1,1)   NOT NULL,
    NombreCompania  NVARCHAR(60)        NOT NULL,
    NombreContacto  NVARCHAR(60)        NULL,
    CargoContacto   NVARCHAR(60)        NULL,
    Direccion       NVARCHAR(120)       NULL,
    Ciudad          NVARCHAR(50)        NULL,
    Region          NVARCHAR(50)        NULL,
    CodPostal       NVARCHAR(15)        NULL,
    Pais            NVARCHAR(50)        NULL,
    Telefono        NVARCHAR(30)        NULL,
    Fax             NVARCHAR(30)        NULL,
    CONSTRAINT PK_Proveedores PRIMARY KEY (IdProveedor)
);
GO

CREATE TABLE Productos
(
    IdProducto          INT IDENTITY(1,1)   NOT NULL,
    NombreProducto      NVARCHAR(80)        NOT NULL,
    IdProveedor         INT                 NULL,
    IdCategoria         INT                 NULL,
    CantidadPorUnidad   NVARCHAR(40)        NULL,
    PrecioUnidad        DECIMAL(10,2)       NOT NULL CONSTRAINT DF_Productos_Precio    DEFAULT (0),
    UnidadesEnExistencia SMALLINT           NOT NULL CONSTRAINT DF_Productos_Stock     DEFAULT (0),
    UnidadesEnPedido    SMALLINT            NOT NULL CONSTRAINT DF_Productos_Pedido    DEFAULT (0),
    NivelNuevoPedido    SMALLINT            NOT NULL CONSTRAINT DF_Productos_Nivel     DEFAULT (0),
    Suspendido          BIT                 NOT NULL CONSTRAINT DF_Productos_Susp      DEFAULT (0),
    CONSTRAINT PK_Productos          PRIMARY KEY (IdProducto),
    CONSTRAINT FK_Productos_Proveedores FOREIGN KEY (IdProveedor) REFERENCES Proveedores (IdProveedor),
    CONSTRAINT FK_Productos_Categorias  FOREIGN KEY (IdCategoria) REFERENCES Categorias  (IdCategoria),
    CONSTRAINT CK_Productos_Precio      CHECK (PrecioUnidad >= 0)
);
GO

CREATE TABLE Clientes
(
    IdCliente       NCHAR(5)        NOT NULL,
    NombreCompania  NVARCHAR(60)    NOT NULL,
    NombreContacto  NVARCHAR(60)    NULL,
    Ciudad          NVARCHAR(50)    NULL,
    Pais            NVARCHAR(50)    NULL,
    Telefono        NVARCHAR(30)    NULL,
    CONSTRAINT PK_Clientes PRIMARY KEY (IdCliente)
);
GO

CREATE TABLE Empleados
(
    IdEmpleado  INT IDENTITY(1,1)   NOT NULL,
    Apellidos   NVARCHAR(40)        NOT NULL,
    Nombre      NVARCHAR(40)        NOT NULL,
    Cargo       NVARCHAR(60)        NULL,
    CONSTRAINT PK_Empleados PRIMARY KEY (IdEmpleado)
);
GO

CREATE TABLE Pedidos
(
    IdPedido        INT IDENTITY(1,1)   NOT NULL,
    IdCliente       NCHAR(5)            NULL,
    IdEmpleado      INT                 NULL,
    FechaPedido     DATETIME            NULL,
    FechaEntrega    DATETIME            NULL,
    FechaEnvio      DATETIME            NULL,
    Cargo           DECIMAL(10,2)       NOT NULL CONSTRAINT DF_Pedidos_Cargo DEFAULT (0),
    Destinatario    NVARCHAR(60)        NULL,
    DireccionDestinatario NVARCHAR(120) NULL,
    CiudadDestinatario    NVARCHAR(50)  NULL,
    PaisDestinatario      NVARCHAR(50)  NULL,
    CONSTRAINT PK_Pedidos            PRIMARY KEY (IdPedido),
    CONSTRAINT FK_Pedidos_Clientes   FOREIGN KEY (IdCliente)  REFERENCES Clientes  (IdCliente),
    CONSTRAINT FK_Pedidos_Empleados  FOREIGN KEY (IdEmpleado) REFERENCES Empleados (IdEmpleado)
);
GO

CREATE TABLE DetallesPedidos
(
    IdPedido        INT             NOT NULL,
    IdProducto      INT             NOT NULL,
    PrecioUnidad    DECIMAL(10,2)   NOT NULL CONSTRAINT DF_Detalles_Precio    DEFAULT (0),
    Cantidad        SMALLINT        NOT NULL CONSTRAINT DF_Detalles_Cantidad  DEFAULT (1),
    Descuento       DECIMAL(4,2)    NOT NULL CONSTRAINT DF_Detalles_Descuento DEFAULT (0),
    CONSTRAINT PK_DetallesPedidos        PRIMARY KEY (IdPedido, IdProducto),
    CONSTRAINT FK_Detalles_Pedidos       FOREIGN KEY (IdPedido)   REFERENCES Pedidos   (IdPedido) ON DELETE CASCADE,
    CONSTRAINT FK_Detalles_Productos     FOREIGN KEY (IdProducto) REFERENCES Productos (IdProducto),
    CONSTRAINT CK_Detalles_Cantidad      CHECK (Cantidad > 0),
    CONSTRAINT CK_Detalles_Descuento     CHECK (Descuento >= 0 AND Descuento <= 1)
);
GO

CREATE INDEX IX_Proveedores_NombreContacto ON Proveedores (NombreContacto);
CREATE INDEX IX_Proveedores_Ciudad         ON Proveedores (Ciudad);
CREATE INDEX IX_Pedidos_FechaPedido        ON Pedidos     (FechaPedido);
GO

INSERT INTO Categorias (NombreCategoria, Descripcion) VALUES
 (N'Bebidas',         N'Gaseosas, cafes, tes y cervezas'),
 (N'Condimentos',     N'Salsas, aderezos y especias'),
 (N'Reposteria',      N'Postres, dulces y panes dulces'),
 (N'Lacteos',         N'Quesos, leches y yogures'),
 (N'Granos/Cereales', N'Panes, galletas, pastas y cereales'),
 (N'Carnes',          N'Carnes rojas, aves y embutidos'),
 (N'Frutas/Verduras', N'Frutas secas, verduras y legumbres'),
 (N'Pescados',        N'Pescados y mariscos');
GO

INSERT INTO Proveedores (NombreCompania, NombreContacto, CargoContacto, Direccion, Ciudad, Region, CodPostal, Pais, Telefono, Fax) VALUES
 (N'Distribuidora Andina S.A.', N'Carlos Mendoza',  N'Gerente de Ventas',      N'Av. Arequipa 1520',    N'Lima',      N'Lima',        N'15046',   N'Peru',     N'(01) 445-2210', N'(01) 445-2211'),
 (N'Alimentos del Sur EIRL',    N'Ana Torres',      N'Representante',          N'Jr. Puno 340',         N'Arequipa',  N'Arequipa',    N'04001',   N'Peru',     N'(054) 22-1188', NULL),
 (N'Lacteos Cajamarca SAC',     N'Luis Ramirez',    N'Jefe de Logistica',      N'Av. Hoyos Rubio 890',  N'Cajamarca', N'Cajamarca',   N'06001',   N'Peru',     N'(076) 36-4420', N'(076) 36-4421'),
 (N'Comercial Pacifico SRL',    N'Maria Fernandez', N'Gerente de Cuentas',     N'Calle Los Cedros 210', N'Lima',      N'Lima',        N'15074',   N'Peru',     N'(01) 610-9080', NULL),
 (N'Exportadora El Trigal',     N'Jorge Salazar',   N'Administrador',          N'Av. Grau 1030',        N'Trujillo',  N'La Libertad', N'13001',   N'Peru',     N'(044) 24-7755', N'(044) 24-7756'),
 (N'Pesquera Costa Azul S.A.',  N'Rosa Villanueva', N'Jefa Comercial',         N'Muelle Norte S/N',     N'Chimbote',  N'Ancash',      N'02801',   N'Peru',     N'(043) 32-1190', NULL),
 (N'Global Foods Chile Ltda.',  N'Andres Soto',     N'Ejecutivo de Ventas',    N'Av. Providencia 2450', N'Santiago',  N'RM',          N'7500000', N'Chile',    N'(2) 2345-6677', N'(2) 2345-6678'),
 (N'Sabores de Colombia SAS',   N'Diana Restrepo',  N'Coordinadora Comercial', N'Carrera 43A 18-95',    N'Medellin',  N'Antioquia',   N'050021',  N'Colombia', N'(4) 444-2020',  NULL),
 (N'Import Andes Bolivia',      N'Ana Torres',      N'Encargada de Compras',   N'Av. Ballivian 1200',   N'La Paz',    N'La Paz',      N'0000',    N'Bolivia',  N'(2) 279-3311',  NULL),
 (N'Frutos del Valle SAC',      N'Pedro Chavez',    N'Supervisor de Ventas',   N'Av. Los Incas 455',    N'Lima',      N'Lima',        N'15082',   N'Peru',     N'(01) 355-7712', N'(01) 355-7713');
GO

INSERT INTO Productos (NombreProducto, IdProveedor, IdCategoria, CantidadPorUnidad, PrecioUnidad, UnidadesEnExistencia, UnidadesEnPedido, NivelNuevoPedido, Suspendido) VALUES
 (N'Cafe tostado molido 500g',     1, 1, N'24 bolsas x 500 g',   18.50, 120,  0, 25, 0),
 (N'Te verde en filtrantes',       1, 1, N'40 cajas x 25 u',      9.90,  80, 20, 20, 0),
 (N'Gaseosa cola 1.5L',            4, 1, N'12 botellas',          6.50, 300,  0, 50, 0),
 (N'Cerveza artesanal IPA',        7, 1, N'24 botellas x 330 ml',15.00,  45, 24, 15, 0),
 (N'Salsa de aji amarillo',        2, 2, N'12 frascos x 220 g',   7.80, 150,  0, 30, 0),
 (N'Mostaza Dijon',                8, 2, N'12 frascos x 200 g',  11.20,  60,  0, 20, 0),
 (N'Aceite de oliva extra virgen', 7, 2, N'6 botellas x 500 ml', 32.90,  40, 12, 10, 0),
 (N'Chocolate de taza 250g',       5, 3, N'20 tabletas',         12.40,  95,  0, 25, 0),
 (N'Galletas de vainilla',         5, 3, N'30 paquetes',          4.30, 220,  0, 40, 0),
 (N'Mermelada de fresa',           8, 3, N'12 frascos x 330 g',   9.60,  70,  0, 20, 0),
 (N'Queso fresco 500g',            3, 4, N'10 unidades',         14.00,  55, 30, 20, 0),
 (N'Leche evaporada 400g',         3, 4, N'48 latas',             3.90, 400,  0, 80, 0),
 (N'Yogurt natural 1L',            3, 4, N'12 botellas',          8.20,  90,  0, 25, 0),
 (N'Manjar blanco 500g',           3, 4, N'12 frascos',          11.50,  35,  0, 15, 1),
 (N'Arroz extra 5kg',              5, 5, N'10 sacos',            24.00, 180,  0, 40, 0),
 (N'Fideos spaghetti 500g',        5, 5, N'40 paquetes',          3.50, 260,  0, 50, 0),
 (N'Quinua perlada 1kg',          10, 5, N'20 bolsas',           16.80, 110, 25, 30, 0),
 (N'Avena tradicional 1kg',        2, 5, N'24 bolsas',            7.20, 140,  0, 30, 0),
 (N'Jamon ingles 250g',            4, 6, N'20 paquetes',         13.90,  65,  0, 20, 0),
 (N'Pollo entero congelado',       4, 6, N'12 unidades',         22.50,  50, 12, 15, 0),
 (N'Lomo fino de res 1kg',         4, 6, N'10 unidades',         48.00,  28,  0, 10, 0),
 (N'Aceitunas negras 500g',       10, 7, N'12 frascos',          10.40,  85,  0, 25, 0),
 (N'Pasas rubias 250g',           10, 7, N'24 bolsas',            6.90, 130,  0, 30, 0),
 (N'Filete de merluza congelado',  6, 8, N'10 kg',               27.60,  42, 20, 15, 0),
 (N'Conchas de abanico',           6, 8, N'5 kg',                56.00,  18,  0, 10, 0),
 (N'Atun en conserva 170g',        6, 8, N'48 latas',             5.40, 310,  0, 60, 0);
GO

INSERT INTO Clientes (IdCliente, NombreCompania, NombreContacto, Ciudad, Pais, Telefono) VALUES
 (N'ALFKI', N'Bodega Alfa',             N'Miguel Rojas',   N'Lima',     N'Peru',     N'(01) 421-3344'),
 (N'BODMI', N'Minimarket El Sol',       N'Elena Quispe',   N'Lima',     N'Peru',     N'(01) 478-9012'),
 (N'CENTR', N'Supermercados Centro',    N'Raul Gutierrez', N'Arequipa', N'Peru',     N'(054) 28-3311'),
 (N'DELIC', N'Delicias del Norte',      N'Sandra Paredes', N'Trujillo', N'Peru',     N'(044) 29-6677'),
 (N'ELMER', N'Comercial El Mercado',    N'Victor Huaman',  N'Cusco',    N'Peru',     N'(084) 23-4455'),
 (N'FRUTA', N'Frutera La Colmena',      N'Nataly Diaz',    N'Piura',    N'Peru',     N'(073) 30-1122'),
 (N'GOURM', N'Gourmet Express',         N'Ricardo Leon',   N'Santiago', N'Chile',    N'(2) 2777-8899'),
 (N'HORIZ', N'Distribuidora Horizonte', N'Claudia Meza',   N'Medellin', N'Colombia', N'(4) 322-1100'),
 (N'INKAS', N'Market Inkas',            N'Jose Ccahuana',  N'Lima',     N'Peru',     N'(01) 500-7788');
GO

INSERT INTO Empleados (Apellidos, Nombre, Cargo) VALUES
 (N'Ramos Vega',    N'Patricia', N'Representante de Ventas'),
 (N'Linares Soto',  N'Fernando', N'Gerente de Ventas'),
 (N'Cordova Pinto', N'Lucia',    N'Representante de Ventas'),
 (N'Aguilar Rios',  N'Marco',    N'Coordinador Logistico'),
 (N'Beltran Nunez', N'Sofia',    N'Representante de Ventas');
GO

INSERT INTO Pedidos (IdCliente, IdEmpleado, FechaPedido, FechaEntrega, FechaEnvio, Cargo, Destinatario, DireccionDestinatario, CiudadDestinatario, PaisDestinatario) VALUES
 (N'ALFKI', 1, '2026-01-12', '2026-01-19', '2026-01-14',  35.20, N'Bodega Alfa',             N'Av. Brasil 1200',       N'Lima',     N'Peru'),
 (N'BODMI', 3, '2026-01-23', '2026-02-02', '2026-01-26',  18.75, N'Minimarket El Sol',       N'Jr. Ica 455',           N'Lima',     N'Peru'),
 (N'CENTR', 2, '2026-02-05', '2026-02-15', '2026-02-08',  62.40, N'Supermercados Centro',    N'Av. Ejercito 780',      N'Arequipa', N'Peru'),
 (N'DELIC', 1, '2026-02-17', '2026-02-27', '2026-02-20',  44.00, N'Delicias del Norte',      N'Av. Espana 320',        N'Trujillo', N'Peru'),
 (N'ELMER', 5, '2026-02-28', '2026-03-10', '2026-03-03',  27.90, N'Comercial El Mercado',    N'Calle Marquez 145',     N'Cusco',    N'Peru'),
 (N'ALFKI', 3, '2026-03-08', '2026-03-18', '2026-03-11',  31.60, N'Bodega Alfa',             N'Av. Brasil 1200',       N'Lima',     N'Peru'),
 (N'FRUTA', 4, '2026-03-19', '2026-03-29', '2026-03-23',  55.10, N'Frutera La Colmena',      N'Av. Sanchez Cerro 900', N'Piura',    N'Peru'),
 (N'GOURM', 2, '2026-03-30', '2026-04-12', '2026-04-03', 120.00, N'Gourmet Express',         N'Av. Vitacura 3100',     N'Santiago', N'Chile'),
 (N'INKAS', 1, '2026-04-09', '2026-04-19', '2026-04-13',  22.30, N'Market Inkas',            N'Av. Colonial 2450',     N'Lima',     N'Peru'),
 (N'HORIZ', 5, '2026-04-21', '2026-05-04', '2026-04-25',  98.70, N'Distribuidora Horizonte', N'Calle 30 #45-12',       N'Medellin', N'Colombia'),
 (N'BODMI', 3, '2026-05-06', '2026-05-16', '2026-05-09',  16.40, N'Minimarket El Sol',       N'Jr. Ica 455',           N'Lima',     N'Peru'),
 (N'CENTR', 2, '2026-05-18', '2026-05-28', '2026-05-21',  71.25, N'Supermercados Centro',    N'Av. Ejercito 780',      N'Arequipa', N'Peru'),
 (N'ALFKI', 1, '2026-06-02', '2026-06-12', '2026-06-05',  29.80, N'Bodega Alfa',             N'Av. Brasil 1200',       N'Lima',     N'Peru'),
 (N'DELIC', 4, '2026-06-15', '2026-06-25', '2026-06-18',  47.50, N'Delicias del Norte',      N'Av. Espana 320',        N'Trujillo', N'Peru'),
 (N'ELMER', 5, '2026-06-27', '2026-07-08', '2026-07-01',  33.90, N'Comercial El Mercado',    N'Calle Marquez 145',     N'Cusco',    N'Peru'),
 (N'FRUTA', 3, '2026-07-07', '2026-07-17', '2026-07-10',  52.00, N'Frutera La Colmena',      N'Av. Sanchez Cerro 900', N'Piura',    N'Peru'),
 (N'INKAS', 2, '2026-07-20', '2026-07-30', '2026-07-23',  24.60, N'Market Inkas',            N'Av. Colonial 2450',     N'Lima',     N'Peru'),
 (N'GOURM', 1, '2026-08-03', '2026-08-14', '2026-08-06', 115.40, N'Gourmet Express',         N'Av. Vitacura 3100',     N'Santiago', N'Chile'),
 (N'HORIZ', 4, '2026-08-14', '2026-08-26', '2026-08-18', 102.30, N'Distribuidora Horizonte', N'Calle 30 #45-12',       N'Medellin', N'Colombia'),
 (N'ALFKI', 5, '2026-08-25', '2026-09-04', '2026-08-28',  30.10, N'Bodega Alfa',             N'Av. Brasil 1200',       N'Lima',     N'Peru');
GO

INSERT INTO DetallesPedidos (IdPedido, IdProducto, PrecioUnidad, Cantidad, Descuento) VALUES
 ( 1,  1, 18.50, 10, 0.00), ( 1,  9,  4.30, 24, 0.05), ( 1, 12,  3.90, 48, 0.00),
 ( 2,  3,  6.50, 36, 0.00), ( 2, 16,  3.50, 20, 0.00),
 ( 3, 15, 24.00, 15, 0.10), ( 3, 21, 48.00,  6, 0.00), ( 3, 11, 14.00, 12, 0.05),
 ( 4,  5,  7.80, 24, 0.00), ( 4, 18,  7.20, 18, 0.00), ( 4, 23,  6.90, 20, 0.05),
 ( 5,  8, 12.40, 12, 0.00), ( 5, 10,  9.60, 12, 0.00),
 ( 6,  2,  9.90, 30, 0.05), ( 6, 13,  8.20, 12, 0.00),
 ( 7, 24, 27.60, 10, 0.00), ( 7, 26,  5.40, 48, 0.10), ( 7, 22, 10.40, 12, 0.00),
 ( 8,  7, 32.90, 12, 0.00), ( 8,  4, 15.00, 24, 0.05), ( 8, 25, 56.00,  5, 0.00),
 ( 9, 12,  3.90, 60, 0.00), ( 9, 16,  3.50, 40, 0.05),
 (10,  6, 11.20, 18, 0.00), (10, 17, 16.80, 25, 0.10), (10, 19, 13.90, 15, 0.00),
 (11,  9,  4.30, 30, 0.00), (11,  3,  6.50, 24, 0.00),
 (12, 15, 24.00, 20, 0.10), (12, 20, 22.50, 10, 0.00), (12, 13,  8.20, 18, 0.05),
 (13,  1, 18.50, 12, 0.00), (13, 11, 14.00, 10, 0.00),
 (14, 21, 48.00,  8, 0.05), (14, 26,  5.40, 36, 0.00), (14, 18,  7.20, 24, 0.00),
 (15,  8, 12.40, 15, 0.00), (15, 23,  6.90, 24, 0.05),
 (16, 24, 27.60, 12, 0.00), (16, 22, 10.40, 18, 0.00),
 (17, 16,  3.50, 50, 0.05), (17, 12,  3.90, 48, 0.00),
 (18,  7, 32.90, 15, 0.10), (18,  4, 15.00, 36, 0.05), (18, 25, 56.00,  6, 0.00),
 (19, 17, 16.80, 30, 0.00), (19,  6, 11.20, 20, 0.00), (19, 10,  9.60, 18, 0.05),
 (20,  1, 18.50, 15, 0.00), (20,  2,  9.90, 20, 0.00), (20, 19, 13.90, 12, 0.00);
GO

PRINT '>>> NeptunoDB creada correctamente con datos de prueba.';
GO
