CREATE TABLE IF NOT EXISTS "Tbl_NivelProceso" (
    "id"               SERIAL PRIMARY KEY,
    "descripcion"      VARCHAR(100) NOT NULL,
    "nivel"            INTEGER NOT NULL,
    "idDepartamento"   BIGINT NOT NULL,
    "idUsuarioInterno" BIGINT NOT NULL,
    "fechaRegistro"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE,
    "idProceso"        INTEGER NOT NULL DEFAULT 1 REFERENCES "Tbl_Proceso"("id")
);
