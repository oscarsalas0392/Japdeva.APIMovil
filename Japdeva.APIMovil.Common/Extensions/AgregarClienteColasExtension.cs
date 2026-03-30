using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientMapperService;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para registrar el cliente gRPC de Colas en el contenedor de dependencias.
    /// </summary>
    public static class AgregarClienteColasExtension
    {
        /// <summary>
        /// Registra <see cref="IColasGrpcClientService"/> como Singleton en el contenedor de DI.
        /// El canal gRPC subyacente es thread-safe y reutilizable; por eso se registra como Singleton.
        /// Requiere que las variables de entorno COLAS_URL y GRPC_CLAVE_INTERNA estén configuradas.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web.</param>
        /// <returns>El mismo builder para encadenar llamadas.</returns>
        public static WebApplicationBuilder AgregarClienteColasGrpc(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.Services.AddSingleton<IColasGrpcClientMapperService, ColasGrpcClientMapperService>();
                builder.Services.AddSingleton<IColasGrpcClientService, ColasGrpcClientService>();
                builder.Services.AddSingleton<IColaRpcService, ColaRpcService>();
                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
