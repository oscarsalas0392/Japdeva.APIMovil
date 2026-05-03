CREATE TABLE IF NOT EXISTS "Tbl_TipoCedula" (
    "id"            SERIAL PRIMARY KEY,
    "tipo"          VARCHAR(100) NOT NULL,
    "formato"       VARCHAR(100) NOT NULL DEFAULT '',
    "fechaRegistro" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"  TIMESTAMP WITH TIME ZONE NULL,
    "activo"        BOOLEAN NOT NULL DEFAULT TRUE
);
