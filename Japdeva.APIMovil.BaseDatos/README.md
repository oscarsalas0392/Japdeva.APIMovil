# Japdeva.APIMovil.BaseDatos

## Descripción

**Japdeva.APIMovil.BaseDatos** es una aplicación de consola (.NET 10) diseñada para automatizar la creación, configuración e inicialización de bases de datos PostgreSQL para los microservicios del ecosistema **Japdeva.APIMovil**.

El proyecto orquesta la ejecución de scripts SQL en un orden específico, permitiendo:
- Crear nuevas bases de datos
- Crear estructura de tablas
- Crear índices para optimización
- Insertar datos iniciales

## Características Principales

✅ **Ejecución ordenada de scripts SQL** - Ejecuta scripts respetando el orden: BaseDatos → Tablas → Índices → Datos

✅ **Configuración centralizada** - Usa `appsettings.json` para definir conexión y microservicios

✅ **Gestión de múltiples microservicios** - Soporta ejecución para uno o varios microservicios

✅ **Validación preventiva** - Valida la estructura de archivos antes de ejecutar cualquier script

✅ **Interacción interactiva** - Solicita confirmación del usuario y opciones de recreación de bases de datos

✅ **Manejo de errores robusto** - Proporciona mensajes descriptivos y reintentos en caso de fallos

## Estructura del Proyecto

```
Japdeva.APIMovil.BaseDatos/
├── Models/
│   ├── ConfiguracionModel.cs          # Modelo de configuración general
│   └── BaseDatosModel.cs              # Modelo de cada base de datos
├── Services/
│   ├── EjecutarScriptsService/        # Orquesta la ejecución de scripts
│   │   ├── IEjecutarScriptsService.cs
│   │   └── EjecutarScriptsService.cs
│   └── BaseDatosService/              # Ejecuta comandos contra PostgreSQL
│       ├── IBaseDatosService.cs
│       └── BaseDatosService.cs
├── Extensions/
│   └── BaseDatosServiciosExtension.cs # Registro de inyección de dependencias
├── BasesDatos/                        # Carpeta con scripts SQL organizados
│   ├── Colas/
│   ├── Usuarios/
│   ├── Parametros/
│   ├── Reclamos/
│   └── EnvioCorreos/
├── Program.cs                         # Punto de entrada
├── appsettings.json                   # Configuración de conexión
└── README.md                          # Este archivo
```

## Requisitos Previos

### Software Requerido

- **.NET 10 SDK** - [Descargar](https://dotnet.microsoft.com/download)
- **PostgreSQL 12+** - [Descargar](https://www.postgresql.org/download/)
- **PowerShell 5.0+** o **Terminal/Bash**

### Dependencias NuGet

El proyecto utiliza los siguientes paquetes:

```
Microsoft.Extensions.Configuration.Json (v10.0.5)
Microsoft.Extensions.Hosting (v10.0.5)
Npgsql (v10.0.2)
```

### Configuración de PostgreSQL

Asegúrate de que:
1. El servidor PostgreSQL esté **ejecutándose**
2. El usuario `postgres` esté disponible y tenga contraseña
3. El servidor sea accesible en `localhost:5432` (configuración por defecto)

**Para verificar la conexión:**

```bash
psql -h localhost -U postgres -c "SELECT version();"
```

## Configuración (appsettings.json)

```json
{
  "ConfiguracionConexion": {
    "Servidor": "localhost",           # Host del servidor PostgreSQL
    "Puerto": 5432,                    # Puerto PostgreSQL (default: 5432)
    "UsuarioPostgres": "postgres",     # Usuario PostgreSQL
    "RecrearTodasBasesDatos": false,   # Recrear todas las BDs (no recomendado)
    "OrdenCarpetas": [
      "BaseDatos",                     # Primero: crear bases de datos
      "Tablas",                        # Segundo: crear tablas
      "Indices",                       # Tercero: crear índices
      "Datos"                          # Cuarto: insertar datos iniciales
    ],
    "BaseDatos": [
      {
        "Nombre": "Colas",
        "RecrearBaseDatos": false,     # ¿Recrear esta BD específica?
        "Ejecutar": true               # ¿Ejecutar scripts para esta BD?
      },
      {
        "Nombre": "Usuarios",
        "RecrearBaseDatos": false,
        "Ejecutar": true
      },
      {
        "Nombre": "Parametros",
        "RecrearBaseDatos": false,
        "Ejecutar": true
      },
      {
        "Nombre": "Reclamos",
        "RecrearBaseDatos": false,
        "Ejecutar": true
      },
      {
        "Nombre": "EnvioCorreos",
        "RecrearBaseDatos": false,
        "Ejecutar": true
      }
    ]
  }
}
```

### Parámetros de Configuración

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `Servidor` | string | Dirección del host PostgreSQL |
| `Puerto` | int | Puerto de conexión (default: 5432) |
| `UsuarioPostgres` | string | Usuario de PostgreSQL |
| `RecrearTodasBasesDatos` | bool | Si es `true`, recrea todas las BDs (cuidado: ¡destruye datos!) |
| `OrdenCarpetas` | array | Orden de ejecución de carpetas de scripts |
| `BaseDatos[].Nombre` | string | Nombre de la base de datos |
| `BaseDatos[].Ejecutar` | bool | Si es `false`, omite esta BD |
| `BaseDatos[].RecrearBaseDatos` | bool | Si es `true`, recrea esta BD específica |

## Estructura de Scripts SQL

Los scripts deben organizarse en carpetas por microservicio y por tipo:

```
BasesDatos/
├── Colas/
│   ├── BaseDatos/
│   │   └── 1_crear_basedatos.sql
│   ├── Tablas/
│   │   ├── 1_tabla_colas.sql
│   │   └── orden.json
│   ├── Indices/
│   │   ├── 1_indice_colas.sql
│   │   └── orden.json
│   └── Datos/
│       ├── 1_datos_iniciales.sql
│       └── orden.json
├── Usuarios/
│   ├── BaseDatos/
│   │   └── 1_crear_basedatos.sql
│   └── ...
└── ...
```

### Archivo `orden.json`

Cada carpeta (excepto BaseDatos) **debe contener** un archivo `orden.json` que especifique el orden de ejecución:

```json
{
  "scripts": [
    "1_tabla_usuarios.sql",
    "2_tabla_roles.sql",
    "3_tabla_permisos.sql"
  ]
}
```

### Validaciones de Scripts

El servicio valida automáticamente:

✅ Existencia de carpetas por microservicio

✅ Existencia de archivos `orden.json` en las carpetas correctas

✅ Patrones SQL válidos (CREATE DATABASE, CREATE TABLE, CREATE INDEX, INSERT)

✅ Scripts sin duplicados

## Uso

### 1. Instalación de Dependencias

```bash
dotnet restore
```

### 2. Ejecución Básica

**Ejecutar todos los microservicios:**

```bash
dotnet run
```

O desde la carpeta del proyecto:

```bash
cd Japdeva.APIMovil.BaseDatos
dotnet run
```

### 3. Ejecución Avanzada

#### Ejecutar solo un microservicio específico:

```bash
dotnet run -- --microservicio Usuarios
```

#### Ejecutar solo una carpeta específica:

```bash
dotnet run -- --carpeta Tablas
```

#### Ejecutar un microservicio y carpeta específicos:

```bash
dotnet run -- --microservicio Usuarios --carpeta Tablas
```

### 4. Interacción Interactiva

Durante la ejecución, el programa solicitará:

1. **Contraseña de PostgreSQL** - Escribe la contraseña del usuario `postgres`

2. **Opción de recreación de bases de datos:**
   - `0` - No recrear (usar bases de datos existentes)
   - `1` - Recrear todas las bases de datos (borra datos existentes)
   - `2` - Seleccionar cuáles recrear (ejemplo: `1,3,5`)

3. **Confirmación** - Antes de ejecutar, muestra un resumen y pide confirmación

## Ejemplos de Uso

### Ejemplo 1: Inicialización Completa

```bash
dotnet run
```

**Flujo:**
- Solicita contraseña PostgreSQL
- Valida todos los scripts
- Muestra resumen de bases de datos a procesar
- Pide confirmación del usuario
- Ejecuta scripts en orden para todos los microservicios

### Ejemplo 2: Crear Solo Tablas para Usuarios

```bash
dotnet run -- --microservicio Usuarios --carpeta Tablas
```

**Flujo:**
- Valida scripts de `BasesDatos/Usuarios/Tablas/`
- Solicita contraseña
- Ejecuta solo los scripts de creación de tablas para Usuarios

### Ejemplo 3: Recrear Solo Datos de Parametros

```bash
dotnet run -- --microservicio Parametros --carpeta Datos
```

## Solución de Problemas

### ❌ Error: "No se pudo leer la sección 'ConfiguracionConexion'"

**Causa:** El archivo `appsettings.json` no existe o está mal formado.

**Solución:**
```bash
# Verificar que appsettings.json existe
dir appsettings.json

# Validar JSON
(Get-Content appsettings.json) | ConvertFrom-Json
```

### ❌ Error: "No se puede conectar a PostgreSQL"

**Causa:** PostgreSQL no está ejecutándose o credenciales incorrectas.

**Solución:**
```bash
# Verificar que PostgreSQL está ejecutándose
psql -h localhost -U postgres -c "SELECT version();"

# Si falla, inicia PostgreSQL (Windows):
pg_ctl -D "C:\Program Files\PostgreSQL\15\data" start
```

### ❌ Error: "Archivo de orden (orden.json) no encontrado"

**Causa:** Falta `orden.json` en una carpeta de scripts.

**Solución:**
```json
// Crear orden.json en la carpeta con contenido:
{
  "scripts": [
    "nombre_del_script.sql"
  ]
}
```

### ❌ Error: "Script contiene CREATE TABLE sin IF NOT EXISTS"

**Causa:** Un script CREATE TABLE no tiene la cláusula `IF NOT EXISTS`.

**Solución:**
```sql
-- ❌ Incorrecto:
CREATE TABLE usuarios (id INT PRIMARY KEY);

-- ✅ Correcto:
CREATE TABLE IF NOT EXISTS usuarios (id INT PRIMARY KEY);
```

### ❌ Error: "Microservicio 'NombreErrado' no encontrado"

**Causa:** El nombre del microservicio no existe en `appsettings.json`.

**Solución:**
```bash
# Verificar nombres disponibles en appsettings.json
# Nombres válidos: Colas, Usuarios, Parametros, Reclamos, EnvioCorreos

dotnet run -- --microservicio Usuarios  # ✅ Correcto
```

## Desarrollo

### Estructura del Código

```
EjecutarScriptsService
├── EjecutarAsync()           # Método principal orquestador
├── ValidarArchivos()         # Valida estructura de archivos
├── ObtenerArgumento()        # Extrae argumentos CLI
├── EscribirEncabezado()      # Muestra información de conexión
├── SolicitarDatosUsuario()   # Interactúa con usuario
├── ProcesarMicroservicios()  # Ejecuta scripts por microservicio
└── ...
```

### Para Agregar Nuevos Microservicios

1. **Agregar en `appsettings.json`:**

```json
{
  "Nombre": "NuevoServicio",
  "RecrearBaseDatos": false,
  "Ejecutar": true
}
```

2. **Crear estructura de carpetas:**

```
BasesDatos/NuevoServicio/
├── BaseDatos/
│   └── 1_crear_basedatos.sql
├── Tablas/
│   ├── 1_tabla.sql
│   └── orden.json
├── Indices/
│   ├── 1_indice.sql
│   └── orden.json
└── Datos/
    ├── 1_datos.sql
    └── orden.json
```

3. **Agregar scripts SQL** en cada carpeta

## Contribuciones

Para reportar problemas o sugerir mejoras, contacta al equipo de desarrollo.

## Licencia

Este proyecto es parte de **Japdeva.APIMovil** - Todos los derechos reservados.

---

**Versión:** 1.0  
**Última actualización:** 2024  
**Autor:** Equipo de Desarrollo - Japdeva
