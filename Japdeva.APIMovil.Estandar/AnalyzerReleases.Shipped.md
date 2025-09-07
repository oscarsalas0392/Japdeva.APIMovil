; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
JAPDEVA001 | Style | Error | LimitarLineasCodigo
JAPDEVA002 | Style | Error | DetectarUsingsNoUtilizados
JAPDEVA003 | Style | Error | DetectarVariablesNoUtilizadas
JAPDEVA004 | Style | Error | DetectarMetodosNoUtilizados
JAPDEVA005 | Style | Error | DetectarValoresQuemados
JAPDEVA006 | Style | Error | ValidarVisibilidadConstantes
JAPDEVA007 | Style | Error | ValidarNomenclaturaConstantes
JAPDEVA008 | Style | Error | ValidarNomenclaturaVariables
JAPDEVA010 | Style | Error | ValidarNomenclaturaPropiedades
JAPDEVA011 | Style | Error | ValidarNomenclaturaAtributosPrivados
JAPDEVA012 | Style | Error | ValidarNomenclaturaMetodos
JAPDEVA013 | Style | Error | ValidarNomenclaturaMetodosAsincronicos
JAPDEVA014 | Style | Error | ValidarNomenclaturaClasesServices
JAPDEVA015 | Style | Error | ValidarNomenclaturaInterfaces
JAPDEVA016 | Style | Error | ValidarImplementacionInterfazServices
JAPDEVA017 | Style | Error | ValidarOrdenUsings
JAPDEVA018 | Style | Error | ValidarNomenclaturaClasesMiddlewares
JAPDEVA019 | Style | Error | ValidarNomenclaturaClasesBackgroundServices
JAPDEVA020 | Style | Error | ValidarNomenclaturaExtensions
JAPDEVA021 | Style | Error | ValidarClasesExtensionsEstaticas
JAPDEVA023 | Style | Error | ValidarNomenclaturaClasesRepositories
JAPDEVA024 | Style | Error | ValidarNomenclaturaClasesModels
JAPDEVA029 | Style | Error | ValidarInterfazRepository
JAPDEVA030 | Style | Error | ValidarUbicacionInterfaces
JAPDEVA031 | Style | Error | ValidarNombreInterfazClase (Interfaz)
JAPDEVA032 | Style | Error | ValidarNombreInterfazClase (Clase)
JAPDEVA033 | Style | Error | ValidarUbicacionClaseConInterfaz
JAPDEVA034 | Style | Error | DetectarValoresQuemadosEnArrays
JAPDEVA035 | Style | Error | ValidarNomenclaturaParametros
JAPDEVA036 | Style | Error | ValidarEspaciosParametros (Declaracion)
JAPDEVA037 | Style | Error | ValidarEspaciosParametros (Llamada)
JAPDEVA038 | Style | Error | DetectarCamposNoUtilizados
JAPDEVA039 | Style | Warning | DetectarUsingsRedundantes
JAPDEVA040 | Maintainability | Warning | DetectarCodigoComentado
JAPDEVA041 | Style | Warning | ValidarEspaciosComas (Sin espacio)
JAPDEVA042 | Style | Warning | ValidarEspaciosComas (Espacios excesivos)
JAPDEVA043 | Style | Warning | ValidarUsoVar
JAPDEVA044 | Documentation | Warning | ValidarDocumentacionXML (Clase)
JAPDEVA045 | Documentation | Warning | ValidarDocumentacionXML (Interfaz)
JAPDEVA046 | Documentation | Warning | ValidarDocumentacionXML (Método)
JAPDEVA047 | Documentation | Warning | ValidarDocumentacionXML (Propiedad)
JAPDEVA048 | Documentation | Warning | ValidarDocumentacionXML (Constructor)
JAPDEVA049 | Quality | Error | ProhibirWarnings
JAPDEVA050 | Maintainability | Warning | LimitarComplejidadCiclomatica
JAPDEVA051 | Style | Warning | DetectarEspaciosInnecesarios (Líneas vacías al inicio)
JAPDEVA052 | Style | Warning | DetectarEspaciosInnecesarios (Espacios finales de línea)
JAPDEVA053 | Style | Warning | DetectarEspaciosInnecesarios (Espacios excesivos)
JAPDEVA054 | Design | Warning | ValidarUbicacionInterfaz (Interfaz debe estar en misma carpeta que implementación)
JAPDEVA055 | Design | Warning | ValidarVisibilidadMetodos (Método debe ser público para pruebas unitarias)
JAPDEVA056 | Maintainability | Warning | ValidarInicializadoresComplejos (Inicializadores complejos dificultan debugging)
JAPDEVA057 | Reliability | Error | ValidarReturnEnCatch (Return en bloque catch oculta errores)
JAPDEVA058 | Reliability | Warning | ValidarEstructuraTryCatchFinally (Método debe tener estructura try-catch)
JAPDEVA059 | Reliability | Warning | ValidarEstructuraTryCatchFinally (Método debe tener bloque finally)
JAPDEVA060 | Estilo | Warning | ValidarUsoThis (Uso obligatorio de 'this.' para miembros de instancia)

## Release 1.1

### Re-enabled Rules

Rule ID | Category | Notes
--------|----------|-------
JAPDEVA017 | Style | ValidarOrdenUsings - Reescrito desde cero con lógica mejorada para validar orden: System.*, Microsoft.*, terceros, Japdeva.*