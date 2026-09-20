CREATE TABLE IF NOT EXISTS "Tbl_Menu" (
    "id"               SERIAL PRIMARY KEY,
    "descripcion"      VARCHAR(100) NOT NULL,
    "ruta"             VARCHAR(200) NULL,
    "icono"            VARCHAR(100) NULL,
    "orden"            INTEGER NOT NULL DEFAULT 0,
    "idPadre"          INTEGER NULL,
    "mostrar"          BOOLEAN NOT NULL DEFAULT TRUE,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"     TIMESTAMP WITH TIME ZONE NULL,
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE
);
