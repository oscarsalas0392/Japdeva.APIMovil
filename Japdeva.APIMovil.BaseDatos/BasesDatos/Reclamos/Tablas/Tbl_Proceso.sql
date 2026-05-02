CREATE TABLE IF NOT EXISTS "Tbl_Proceso" (
    "id"             SERIAL PRIMARY KEY,
    "nombre"         VARCHAR(100) NOT NULL,
    "descripcion"    VARCHAR(500) NOT NULL DEFAULT '',
    "fechaRegistro"  TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "activo"         BOOLEAN NOT NULL DEFAULT TRUE
);
