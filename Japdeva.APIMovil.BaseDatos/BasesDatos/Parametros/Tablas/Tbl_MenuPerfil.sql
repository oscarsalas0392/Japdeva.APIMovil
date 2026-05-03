CREATE TABLE IF NOT EXISTS "Tbl_MenuPerfil" (
    "id" SERIAL PRIMARY KEY,
    "idMenu" INTEGER NOT NULL,
    "idPerfil" INTEGER NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE,
    
    CONSTRAINT "fk_menuperfil_menu" FOREIGN KEY ("idMenu") REFERENCES "Tbl_Menu" ("id") ON DELETE CASCADE
);