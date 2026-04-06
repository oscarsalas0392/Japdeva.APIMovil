CREATE TABLE IF NOT EXISTS "Tbl_EstadoDetalleReclamo" (
    "id"                SERIAL PRIMARY KEY,
    "descripcion"       VARCHAR(100) NOT NULL,
    "continuaProceso"   BOOLEAN NOT NULL DEFAULT FALSE,
    "rechazaProceso"    BOOLEAN NOT NULL DEFAULT FALSE,
    "devolucionProceso" BOOLEAN NOT NULL DEFAULT FALSE,
    "finalizarProceso"  BOOLEAN NOT NULL DEFAULT FALSE,
    "idUsuarioInterno"  BIGINT NOT NULL,
    "fechaRegistro"     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "activo"            BOOLEAN NOT NULL DEFAULT TRUE
);
