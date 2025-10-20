using System.Diagnostics;
using System.Reflection;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Extensiones para obtener información de contexto de ejecución
    /// como namespace, clase y método actual.
    /// </summary>
    public static class ContextoEjecucionExtension
    {
        private const string CONTEXTO_DESCONOCIDO = "Desconocido";
        private const int FRAME_NIVEL_LLAMADOR = 1;
        /// <summary>
        /// Obtiene el nombre completo del método actual incluyendo namespace, clase y método.
        /// </summary>
        /// <param name="objeto">El objeto desde el cual se llama la extensión.</param>
        /// <returns>String con formato "Namespace.Clase.Método".</returns>
        public static string ObtenerNombreMetodo(this object objeto)
        {
            try
            {
                StackFrame? frame = new StackFrame(FRAME_NIVEL_LLAMADOR);

                MethodBase? metodo = frame.GetMethod();

                if (metodo is not null)
                {
                    Type? tipo = metodo.DeclaringType;
                    if (tipo is not null)
                    {
                        return $"{tipo.Namespace}.{tipo.Name}.{metodo.Name}";
                    }
                }

                return CONTEXTO_DESCONOCIDO;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
        }
    }
}
