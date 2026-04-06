CREATE TABLE IF NOT EXISTS "Tbl_Mensaje" (
    "id" SERIAL PRIMARY KEY,
    "descripcion" VARCHAR(300) NOT NULL,
    "idTipoMensaje" INTEGER NOT NULL,
    "idPantalla" INTEGER NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT "fk_mensaje_tipomensaje" FOREIGN KEY ("idTipoMensaje") REFERENCES "Tbl_TipoMensaje" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_mensaje_pantalla" FOREIGN KEY ("idPantalla") REFERENCES "Tbl_Pantalla" ("id") ON DELETE RESTRICT
);