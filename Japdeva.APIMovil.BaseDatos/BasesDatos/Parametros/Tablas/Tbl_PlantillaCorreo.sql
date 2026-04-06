CREATE TABLE IF NOT EXISTS "Tbl_PlantillaCorreo" (
    "id"               SERIAL PRIMARY KEY,
    "Plantilla"        TEXT NOT NULL,
    "EsHtml"           BOOLEAN NOT NULL DEFAULT TRUE,
    "Asunto"           TEXT NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"     TIMESTAMP WITH TIME ZONE NULL,
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE
);
