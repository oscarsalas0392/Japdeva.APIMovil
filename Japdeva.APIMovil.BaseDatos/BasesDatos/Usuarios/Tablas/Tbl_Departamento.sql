CREATE TABLE IF NOT EXISTS "Tbl_Departamento" (
    "id"            SERIAL PRIMARY KEY,
    "descripcion"   VARCHAR(200) NOT NULL,
    "fechaRegistro" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"  TIMESTAMP WITH TIME ZONE NULL,
    "activo"        BOOLEAN NOT NULL DEFAULT TRUE
);
