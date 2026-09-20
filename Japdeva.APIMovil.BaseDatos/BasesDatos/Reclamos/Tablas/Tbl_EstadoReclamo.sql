CREATE TABLE IF NOT EXISTS "Tbl_EstadoReclamo" (
    "id"               SERIAL PRIMARY KEY,
    "descripcion"      VARCHAR(100) NOT NULL,
    "idUsuarioInterno" BIGINT NOT NULL,
    "fechaRegistro"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE
);
