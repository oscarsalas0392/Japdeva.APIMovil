CREATE TABLE IF NOT EXISTS "Tbl_Pantalla" (
    "id"               SERIAL PRIMARY KEY,
    "descripcion"      VARCHAR(100) NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"     TIMESTAMP WITH TIME ZONE NULL,
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE
);
