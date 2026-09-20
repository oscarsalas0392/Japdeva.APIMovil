CREATE TABLE IF NOT EXISTS "Tbl_Parametro" (
    "id"               SERIAL PRIMARY KEY,
    "nombre"           VARCHAR(100) NOT NULL,
    "valor1"           TEXT NOT NULL,
    "valor2"           TEXT NULL,
    "descripcion"      VARCHAR(255) NOT NULL DEFAULT '',
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro"    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"     TIMESTAMP WITH TIME ZONE NULL,
    "activo"           BOOLEAN NOT NULL DEFAULT TRUE
);
