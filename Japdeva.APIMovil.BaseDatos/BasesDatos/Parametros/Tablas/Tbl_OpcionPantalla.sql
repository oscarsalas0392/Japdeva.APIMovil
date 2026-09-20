CREATE TABLE IF NOT EXISTS "Tbl_OpcionPantalla" (
    "id"               SERIAL PRIMARY KEY,
    "nombre"           VARCHAR(100) NOT NULL,
    "descripcion"      VARCHAR(200) NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro"    TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"     TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE
);
