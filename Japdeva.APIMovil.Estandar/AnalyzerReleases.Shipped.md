# Release notes

## Release 1.3

### New Rules

| Rule ID | Category | Severity | Notes |
|---------|----------|----------|-------|
| JAPDEVA081 | Design | Error | Métodos en servicios deben seguir estructura estándar de logging |

## Release 1.2

### New Rules

| Rule ID | Category | Severity | Notes |
|---------|----------|----------|-------|
| JAPDEVA080 | Design | Error | Interfaz y clase no pueden estar en el mismo archivo |

## Release 1.1

### New Rules

| Rule ID | Category | Severity | Notes |
|---------|----------|----------|-------|
| JAPDEVA079 | Design | Error | Clases estáticas no permitidas en carpeta Services |

## Release 1.0

### New Rules

| Rule ID | Category | Severity | Notes |
|---------|----------|----------|-------|
| JAPDEVA078 | Design | Error | Los middlewares deben implementar su interfaz correspondiente |

## Descripción de Reglas

### JAPDEVA081: Métodos en servicios deben seguir estructura estándar de logging

Esta regla verifica que todos los métodos públicos en servicios sigan la estructura estándar de logging. Esto garantiza:

- **Trazabilidad consistente**: Todos los métodos tienen logging uniforme
- **Debugging facilitado**: Estructura predecible para análisis de logs
- **Mantenibilidad**: Patrón estándar fácil de seguir y modificar
- **Observabilidad**: Visibilidad completa del flujo de ejecución

#### Estructura Requerida:

**✅ Correcto:**
```csharp
public string MiMetodo(string traceId, string parametro)
{
    string nombreMetodo = this.ObtenerNombreMetodo();
    
    try
    {
        this._logger.Inicio(traceId, nombreMetodo);
        
        // Lógica del método aquí
        
        return resultado;
    }
    catch (Exception ex)
    {
        this._logger.Error(traceId, nombreMetodo, ex);
        throw new InvalidOperationException("Mensaje de error", ex);
    }
    finally
    {
        this._logger.Fin(traceId, nombreMetodo);
    }
}
```

**❌ Incorrecto:**
```csharp
public string MiMetodo(string traceId, string parametro)
{
    // ❌ Falta estructura de logging completa
    // Lógica del método
    return resultado;
}
```

#### Elementos Obligatorios:
1. `string nombreMetodo = this.ObtenerNombreMetodo();`
2. Bloque `try-catch-finally`
3. `this._logger.Inicio(traceId, nombreMetodo);` en try
4. `this._logger.Error(traceId, nombreMetodo, ex);` en catch
5. `this._logger.Fin(traceId, nombreMetodo);` en finally

### JAPDEVA080: Interfaz y clase no pueden estar en el mismo archivo

Esta regla prohíbe definir interfaces y clases en el mismo archivo. Esto garantiza:

- **Principio de Responsabilidad Única**: Cada archivo tiene una sola responsabilidad
- **Organización del código**: Facilita la navegación y mantenimiento
- **Estándares de la industria**: Sigue las mejores prácticas de .NET
- **Claridad arquitectónica**: Separación clara entre contratos e implementaciones

#### Ejemplos

**✅ Correcto:**
```csharp
// IEncriptarService.cs
namespace Japdeva.APIMovil.Common.Services.EncriptarService;

public interface IEncriptarService
{
    string Encriptar(string traceId, string textoPlano);
}

// EncriptarService.cs (archivo separado)
namespace Japdeva.APIMovil.Common.Services.EncriptarService;

public class EncriptarService : IEncriptarService
{
    public string Encriptar(string traceId, string textoPlano)
    {
        // Implementación
    }
}
```

**❌ Incorrecto:**
```csharp
// EncriptarService.cs
namespace Japdeva.APIMovil.Common.Services.EncriptarService;

public interface IEncriptarService // ❌ Interfaz y clase en mismo archivo
{
    string Encriptar(string traceId, string textoPlano);
}

public class EncriptarService : IEncriptarService // ❌ Interfaz y clase en mismo archivo
{
    public string Encriptar(string traceId, string textoPlano)
    {
        // Implementación
    }
}
```

### JAPDEVA079: Clases estáticas no permitidas en carpeta Services

Esta regla prohíbe la creación de clases estáticas dentro de la carpeta Services. Esto garantiza:

- **Inyección de dependencias**: Mantiene el patrón de DI en toda la capa de servicios
- **Testabilidad**: Las clases estáticas son difíciles de mockear en pruebas unitarias
- **Principios SOLID**: Cumple con el principio de inversión de dependencias
- **Mantenibilidad**: Facilita futuras modificaciones y extensiones

#### Ejemplos

**✅ Correcto:**
```csharp
namespace Japdeva.APIMovil.Common.Services.EncriptarService;

public interface IEncriptarService
{
    string Encriptar(string traceId, string textoPlano);
}

public class EncriptarService : IEncriptarService
{
    public string Encriptar(string traceId, string textoPlano)
    {
        // Implementación del servicio
    }
}
```

**❌ Incorrecto:**
```csharp
namespace Japdeva.APIMovil.Common.Services.EncriptarService;

public static class EncriptarHelperService // Clase estática en Services
{
    public static string Encriptar(string textoPlano)
    {
        // Implementación estática
    }
}
```

### JAPDEVA078: Los middlewares deben implementar su interfaz correspondiente

Esta regla verifica que todos los middlewares en el proyecto implementen su interfaz correspondiente. Esto garantiza:

- **Consistencia arquitectónica**: Todos los middlewares siguen el mismo patrón
- **Testabilidad**: Las interfaces facilitan la creación de mocks para pruebas unitarias
- **Principios SOLID**: Cumple con el principio de inversión de dependencias
- **Mantenibilidad**: Facilita futuras modificaciones y extensiones

#### Ejemplos

**✅ Correcto:**
```csharp
namespace Japdeva.APIMovil.Common.Middlewares.RecibirTraceIdMiddleware;

public interface IRecibirTraceIdMiddleware
{
    Task InvokeAsync(HttpContext context);
}

public class RecibirTraceIdMiddleware : IRecibirTraceIdMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Implementación del middleware
    }
}
```

**❌ Incorrecto:**
```csharp
namespace Japdeva.APIMovil.Common.Middlewares.RecibirTraceIdMiddleware;

public class RecibirTraceIdMiddleware // No implementa interfaz
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Implementación del middleware
    }
}
```

#### Cómo corregir

1. Crear la interfaz correspondiente `I{NombreMiddleware}` en el mismo directorio
2. Agregar el método `Task InvokeAsync(HttpContext context)` a la interfaz
3. Hacer que la clase middleware implemente la interfaz

#### Configuración

Esta regla está habilitada por defecto y no puede deshabilitarse ya que es fundamental para la arquitectura del proyecto.