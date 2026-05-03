CREATE TABLE IF NOT EXISTS "Tbl_Prioridad" (
    "id"            SERIAL PRIMARY KEY,
    "nombre"        VARCHAR(50) NOT NULL,
    "descripcion"   VARCHAR(200) NULL,
    "activo"        BOOLEAN NOT NULL DEFAULT TRUE,
    "fechaRegistro" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
