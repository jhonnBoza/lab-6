# Laboratorio 06 — Class Library, DataSet y async/await sobre NeptunoDB

**Curso:** Desarrollo de Aplicaciones Empresariales Avanzado — Semana 06  
**Tema:** Class Library & DataSet  
**Continuación de:** Lab 05 (ExecuteNonQuery + eliminación lógica)

Aplicación WPF (.NET 10) sobre **NeptunoDB**. Respecto del Lab 05 se agregan tres
cambios de arquitectura:

1. **Class Library** `Lab06_DAEA.Datos`: modelos y repositorios fuera del ejecutable WPF.
2. **Modo desconectado (DataSet)** en listados, búsquedas y reportes vía `SqlDataAdapter.Fill`.
3. **async/await de punta a punta** (sin `.Result` / `.Wait()`) para no congelar la UI.

Las escrituras siguen usando **`ExecuteNonQuery`** (alta / edición / baja lógica).

---

## 1. Contenido de la entrega

| Ruta | Descripción |
|------|-------------|
| `Scripts/01_NeptunoDB.sql` | Crea la base, tablas, índices y datos de prueba. |
| `Scripts/02_EliminacionLogica.sql` | Columna `Activo` en Productos, Categorías, Proveedores y Pedidos. |
| `Scripts/03_ProcedimientosAlmacenados.sql` | CRUD con baja lógica, búsqueda de proveedores y reportes por fechas. |
| `Lab06_DAEA.Datos/` | **Class Library**: modelos + capa Repository/Data. |
| `Lab06_DAEA/` | Proyecto WPF de inicio (UI + `App.config` con la cadena de conexión). |
| `Lab06_DAEA.slnx` | Solución con ambos proyectos y la referencia correcta. |
| `Capturas/` | Capturas de las cinco vistas en ejecución. |
| `Evidencias/` | Capturas adicionales (baja lógica, búsqueda, reportes). |
| `GLAB-S06-EAREVALO-2026-2.docx` | Enunciado del laboratorio. |

**Repositorio:** https://github.com/jhonnBoza/lab-6.git

---

## 2. Cómo ejecutar

### 2.1 Base de datos

Ejecutar los tres scripts **en orden**:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Scripts/01_NeptunoDB.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Scripts/02_EliminacionLogica.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Scripts/03_ProcedimientosAlmacenados.sql
```

### 2.2 Cadena de conexión (gotcha de App.config)

Está en **`Lab06_DAEA/App.config`** (proyecto de inicio), **no** en la Class Library.

`ConfigurationManager` solo lee el `.config` del ejecutable. Si la cadena estuviera
solo en `Lab06_DAEA.Datos`, en runtime fallaría aunque compile.

| Motor | Data Source |
|-------|-------------|
| LocalDB | `(localdb)\MSSQLLocalDB` |
| SQL Server Express | `.\SQLEXPRESS` |
| Instancia local | `.` |

### 2.3 Aplicación

Abrir `Lab06_DAEA.slnx` en Visual Studio (Windows) y ejecutar F5, o:

```bash
dotnet run --project Lab06_DAEA
```

Requisitos: Windows + .NET 10 SDK (WPF).

---

## 3. Arquitectura

```
Lab06_DAEA (WPF, WinExe)          Lab06_DAEA.Datos (Class Library)
├── App.config  ← cadena aquí     ├── Modelos/Entidades.cs
├── Vistas/*                      └── Datos/
└── ProjectReference ──────────────►    ConexionBD, AccesoDatos
                                        *Repositorio (async)
```

Referencia de proyectos: `Lab06_DAEA.csproj` → `Lab06_DAEA.Datos.csproj`.

---

## 4. Criterio del escenario desconectado

**Cuándo se usa DataSet (desconectado):**

- Listados de productos, categorías, proveedores y pedidos
- Búsqueda de proveedores por `nombreContacto` / `ciudad`
- Catálogos (clientes, empleados)
- Detalle de un pedido
- Reportes por intervalo de fechas

Implementación: `SqlDataAdapter.Fill(DataSet)`. La conexión se abre solo mientras
se copia el resultado a memoria; después la UI trabaja sobre el `DataSet`/`DataTable`
ya cerrado (modo desconectado clásico de ADO.NET).

**Cuándo se usa modo conectado (`ExecuteNonQueryAsync`):**

- Insertar, actualizar y eliminar (baja lógica) de productos, categorías, proveedores y pedidos
- Mutaciones del detalle de pedido

Motivo: el enunciado exige escritura con `ExecuteNonQuery`, y una mutación puntual
no necesita mantener un DataSet en memoria.

---

## 5. async/await (sin bloquear la UI)

En el Lab 05 las cargas eran síncronas y podían congelar la ventana. Aquí:

- Repositorios expuestos como `*Async` (`ListarAsync`, `InsertarAsync`, …)
- Eventos de UI: `private async void AlGuardar(...)` con `await` hasta la capa de datos
- Arranque: `await ConexionBD.ProbarAsync()` (sin `.Result` ni `.Wait()`)

---

## 6. Requisitos del enunciado y dónde se cumplen

| Requisito | Dónde |
|-----------|--------|
| Campo `Activo` + eliminación lógica | `Scripts/02_*.sql` y `usp_*_Eliminar` |
| CRUD productos/categorías/proveedores/pedidos con `ExecuteNonQuery` | Repositorios → `Ejecutar*Async` |
| Búsqueda proveedores por contacto/ciudad (`Activo = 1`) | `ProveedorRepositorio.BuscarAsync` |
| Reportes por fechas (excluye `Activo = 0`) | `ReporteRepositorio` + vista Reportes |
| Separar modelos y acceso a datos en Class Library | `Lab06_DAEA.Datos` |
| Referencias de proyectos correctas | `Lab06_DAEA.csproj` → ProjectReference |
| Modo desconectado donde corresponde | `AccesoDatos.LlenarDataSetAsync` / `ListarAsync` |
| Gotcha App.config en proyecto de inicio | `Lab06_DAEA/App.config` |
| Corregir bloqueo UI (async/await) | Vistas + `App.OnStartup` |

---

## 7. Capturas de las vistas

### Capturas/

| Archivo | Vista |
|---------|-------|
| `01-productos.png` | Mantenimiento de productos |
| `02-categorias.png` | Mantenimiento de categorías |
| `03-proveedores.png` | Mantenimiento de proveedores |
| `04-pedidos.png` | Mantenimiento de pedidos |
| `05-reportes.png` | Reportes por intervalo de fechas |

### Evidencias/

| Archivo | Qué muestra |
|---------|-------------|
| `01-productos-con-dados-de-baja.png` | Listado con `Mostrar dados de baja` |
| `02-proveedores-busqueda-lima.png` | Búsqueda por ciudad = Lima |
| `03-proveedores-con-dados-de-baja.png` | Proveedores incluyendo inactivos |
| `04-categorias-con-dados-de-baja.png` | Categorías incluyendo inactivas |
| `05-pedidos-con-dados-de-baja.png` | Pedidos incluyendo dados de baja |
| `06-reportes-ultimos-30.png` | Reporte últimos 30 días |
| `07-productos-listado.png` | Listado de productos activos |

Para regenerarlas: `powershell -ExecutionPolicy Bypass -File Scripts\CapturarVistas.ps1`

---

## 8. Observaciones y conclusiones

- Separar la Class Library obliga a pensar en el **proyecto de inicio** como dueño
  de la configuración: el gotcha de `App.config` es el síntoma típico.
- El modo desconectado encaja en consultas de solo lectura; mezclarlo con
  `ExecuteNonQuery` en escrituras cumple el enunciado sin forzar DataAdapter en mutaciones.
- `async/await` de punta a punta evita el antipatrón `.Result`/`.Wait()` que
  bloquea el hilo de UI de WPF.
