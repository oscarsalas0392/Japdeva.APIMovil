CREATE TABLE IF NOT EXISTS "Tbl_EstadoMensaje" (
    "id"            SERIAL PRIMARY KEY,
    "nombre"        VARCHAR(50) NOT NULL,
    "descripcion"   VARCHAR(200) NOT NULL DEFAULT '',
    "activo"        BOOLEAN NOT NULL DEFAULT TRUE,
    "fechaRegistro" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
