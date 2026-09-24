# Captura las vistas del Lab 06 via UI Automation + screenshot de ventana.
# Uso: powershell -ExecutionPolicy Bypass -File Scripts\CapturarVistas.ps1

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

$capturas = Join-Path $repoRoot 'Capturas'
$evidencias = Join-Path $repoRoot 'Evidencias'
New-Item -ItemType Directory -Force -Path $capturas, $evidencias | Out-Null

$exe = Join-Path $repoRoot 'Lab06_DAEA\bin\Debug\net10.0-windows\Lab06_DAEA.exe'
if (-not (Test-Path $exe)) {
    Write-Host 'Compilando...'
    dotnet build (Join-Path $repoRoot 'Lab06_DAEA.slnx') -v q | Out-Null
}

Get-Process Lab06_DAEA -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

$proc = Start-Process -FilePath $exe -PassThru
$deadline = (Get-Date).AddSeconds(45)
$window = $null

while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 400
    $condName = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        'Sistema de mantenimiento')
    $window = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst(
        [System.Windows.Automation.TreeScope]::Children, $condName)
    if ($null -ne $window -and $window.Current.NativeWindowHandle -ne 0) { break }
}

if ($null -eq $window) {
    throw 'No se encontro la ventana Sistema de mantenimiento. Revisa si Lab06 arranca.'
}

# Traer al frente
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32 {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
  [StructLayout(LayoutKind.Sequential)]
  public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@

$hwnd = [IntPtr]$window.Current.NativeWindowHandle
[Win32]::ShowWindow($hwnd, 9) | Out-Null  # SW_RESTORE
[Win32]::SetForegroundWindow($hwnd) | Out-Null
Start-Sleep -Seconds 1

function Capture-Window([string]$path) {
    $r = New-Object Win32+RECT
    [Win32]::GetWindowRect($hwnd, [ref]$r) | Out-Null
    $w = [Math]::Max(1, $r.Right - $r.Left)
    $h = [Math]::Max(1, $r.Bottom - $r.Top)
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.Left, $r.Top, 0, 0, (New-Object System.Drawing.Size($w, $h)))
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "OK  $path"
}

function Find-ByName([string]$name, [string]$controlType = $null) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $name)
    if ($controlType) {
        $typeId = [System.Windows.Automation.ControlType]::$controlType
        $typeCond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty, $typeId)
        $and = New-Object System.Windows.Automation.AndCondition($cond, $typeCond)
        return $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $and)
    }
    return $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $cond)
}

function Click-Element($el) {
    if ($null -eq $el) { throw 'Elemento no encontrado' }
    try {
        $invoke = $el.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        $invoke.Invoke()
    } catch {
        try {
            $sel = $el.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            $sel.Select()
        } catch {
            $rect = $el.Current.BoundingRectangle
            $x = [int]($rect.X + $rect.Width / 2)
            $y = [int]($rect.Y + $rect.Height / 2)
            [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point($x, $y)
            Add-Type -TypeDefinition @"
using System.Runtime.InteropServices;
public static class Mouse {
  [DllImport("user32.dll")] public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
  public const int LEFTDOWN = 0x02;
  public const int LEFTUP = 0x04;
  public static void Click() { mouse_event(LEFTDOWN, 0, 0, 0, 0); mouse_event(LEFTUP, 0, 0, 0, 0); }
}
"@ -ErrorAction SilentlyContinue
            [Mouse]::Click()
        }
    }
    Start-Sleep -Milliseconds 700
}

function Select-Tab([string]$name) {
    $tab = Find-ByName $name 'TabItem'
    if ($null -eq $tab) { $tab = Find-ByName $name }
    Click-Element $tab
    Start-Sleep -Milliseconds 900
}

function Set-EditValue([string]$automationNameOrNearby, [string]$value) {
    # Busca Edit controls y setea el primero vacio o el que coincida
    $edits = $window.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        (New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Edit)))
    foreach ($e in $edits) {
        $n = $e.Current.Name
        $aid = $e.Current.AutomationId
        if ($n -match $automationNameOrNearby -or $aid -match $automationNameOrNearby) {
            $vp = $e.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
            $vp.SetValue($value)
            Start-Sleep -Milliseconds 300
            return
        }
    }
}

function Toggle-Checkbox([string]$name, [bool]$check) {
    $cb = Find-ByName $name 'CheckBox'
    if ($null -eq $cb) { $cb = Find-ByName $name }
    if ($null -eq $cb) { Write-Host "WARN checkbox $name no encontrado"; return }
    $tp = $cb.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    $isOn = $tp.Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On
    if ($check -ne $isOn) { $tp.Toggle(); Start-Sleep -Milliseconds 500 }
}

# --- Capturas principales (5 vistas) ---
Select-Tab 'Productos'
Start-Sleep -Seconds 1
Capture-Window (Join-Path $capturas '01-productos.png')

Select-Tab 'Categorias'
Start-Sleep -Seconds 1
Capture-Window (Join-Path $capturas '02-categorias.png')

Select-Tab 'Proveedores'
Start-Sleep -Seconds 1
Capture-Window (Join-Path $capturas '03-proveedores.png')

Select-Tab 'Pedidos'
Start-Sleep -Seconds 1
Capture-Window (Join-Path $capturas '04-pedidos.png')

Select-Tab 'Reportes'
Start-Sleep -Seconds 1
# Intentar generar reporte
$gen = Find-ByName 'Generar' 
if ($null -eq $gen) { $gen = Find-ByName 'Generar reporte' }
if ($null -ne $gen) { Click-Element $gen; Start-Sleep -Seconds 1 }
Capture-Window (Join-Path $capturas '05-reportes.png')

# --- Evidencias extra ---
Select-Tab 'Productos'
Toggle-Checkbox 'Mostrar dados de baja' $true
Start-Sleep -Seconds 1
Capture-Window (Join-Path $evidencias '01-productos-con-dados-de-baja.png')
Toggle-Checkbox 'Mostrar dados de baja' $false

Select-Tab 'Proveedores'
# filtros: intentar setear ciudad Lima
$edits = $window.FindAll(
    [System.Windows.Automation.TreeScope]::Descendants,
    (New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Edit)))
# Suele haber 2 edits: contacto y ciudad — llenar el segundo con Lima
$idx = 0
foreach ($e in $edits) {
    if (-not $e.Current.IsEnabled) { continue }
    try {
        $vp = $e.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($idx -eq 1) { $vp.SetValue('Lima') }
        $idx++
    } catch {}
}
$buscar = Find-ByName 'Buscar'
if ($null -ne $buscar) { Click-Element $buscar; Start-Sleep -Seconds 1 }
Capture-Window (Join-Path $evidencias '02-proveedores-busqueda-lima.png')

# limpiar y mostrar inactivos
foreach ($e in $edits) {
    if (-not $e.Current.IsEnabled) { continue }
    try {
        $vp = $e.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        $vp.SetValue('')
    } catch {}
}
$limpiar = Find-ByName 'Limpiar'
if ($null -ne $limpiar) { Click-Element $limpiar }
Toggle-Checkbox 'Mostrar dados de baja' $true
Start-Sleep -Seconds 1
Capture-Window (Join-Path $evidencias '03-proveedores-con-dados-de-baja.png')
Toggle-Checkbox 'Mostrar dados de baja' $false

Select-Tab 'Categorias'
Toggle-Checkbox 'Mostrar dados de baja' $true
Start-Sleep -Seconds 1
Capture-Window (Join-Path $evidencias '04-categorias-con-dados-de-baja.png')
Toggle-Checkbox 'Mostrar dados de baja' $false

Select-Tab 'Pedidos'
Toggle-Checkbox 'Mostrar dados de baja' $true
Start-Sleep -Seconds 1
Capture-Window (Join-Path $evidencias '05-pedidos-con-dados-de-baja.png')

Select-Tab 'Reportes'
$ultimos = Find-ByName 'Ultimos 30 dias'
if ($null -eq $ultimos) { $ultimos = Find-ByName 'Últimos 30 días' }
if ($null -eq $ultimos) { $ultimos = Find-ByName 'Ultimos 30' }
if ($null -ne $ultimos) { Click-Element $ultimos; Start-Sleep -Seconds 1 }
else {
    $gen2 = Find-ByName 'Generar'
    if ($null -ne $gen2) { Click-Element $gen2; Start-Sleep -Seconds 1 }
}
Capture-Window (Join-Path $evidencias '06-reportes-ultimos-30.png')

# Vista final productos limpia
Select-Tab 'Productos'
Start-Sleep -Milliseconds 800
Capture-Window (Join-Path $evidencias '07-productos-listado.png')

Write-Host ''
Write-Host 'Capturas listas en:'
Write-Host "  $capturas"
Write-Host "  $evidencias"
Write-Host 'Deja la app abierta para revision manual.'
