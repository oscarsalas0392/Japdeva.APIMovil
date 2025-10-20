namespace Japdeva.APIMovil.Colas.Constants
{
    /// <summary>
    /// Constantes para paginación en el sistema de colas.
    /// </summary>
    public static class PaginacionConstants
    {
        /// <summary>
        /// Página inicial para consultas paginadas.
        /// </summary>
        public const int PAGINA_INICIAL = 1;
    }

    /// <summary>
    /// Constantes para estados en el sistema de colas.
    /// </summary>
    public static class EstadoConstants
    {
        /// <summary>
        /// Indica que un registro está activo.
        /// </summary>
        public const bool ACTIVO = true;

        /// <summary>
        /// Indica que un registro está inactivo.
        /// </summary>
        public const bool INACTIVO = false;

        /// <summary>
        /// Indica que un mensaje está en proceso.
        /// </summary>
        public const bool EN_PROCESO = true;

        /// <summary>
        /// Indica que un mensaje no está en proceso.
        /// </summary>
        public const bool NO_EN_PROCESO = false;
    }
}