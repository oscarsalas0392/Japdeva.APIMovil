CREATE TABLE IF NOT EXISTS "Tbl_OpcionPantallaPerfil" (
    "id"                SERIAL PRIMARY KEY,
    "idOpcionPantalla"  INTEGER NOT NULL,
    "idPerfil"          INTEGER NOT NULL,
    "idUsuarioInterno"  BIGINT NULL,
    "fechaRegistro"     TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"      TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo"            BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT "fk_opcionpantallaperfil_opcionpantalla" FOREIGN KEY ("idOpcionPantalla") REFERENCES "Tbl_OpcionPantalla" ("id") ON DELETE CASCADE
);
