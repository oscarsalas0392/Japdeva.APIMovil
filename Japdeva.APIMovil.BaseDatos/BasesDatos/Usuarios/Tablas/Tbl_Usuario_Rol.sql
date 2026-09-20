
CREATE TABLE IF NOT EXISTS "Tbl_Usuario_Rol" (
    "id"                     BIGSERIAL   NOT NULL,
    "idUsuario"              INTEGER     NOT NULL,
    "idRol"                  INTEGER     NOT NULL,
    "idUsuarioAdministrador" INTEGER     NULL,
    "fechaRegistro"          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "fechaEdicion"           TIMESTAMPTZ NULL,
    "activo"                 BOOLEAN     NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_Tbl_Usuario_Rol"             PRIMARY KEY ("id"),
    CONSTRAINT "FK_Tbl_Usuario_Rol_Usuario"     FOREIGN KEY ("idUsuario")
        REFERENCES "Tbl_Usuario" ("id"),
    CONSTRAINT "FK_Tbl_Usuario_Rol_Rol"         FOREIGN KEY ("idRol")
        REFERENCES "Tbl_Rol" ("id"),
    CONSTRAINT "FK_Tbl_Usuario_Rol_Administrador" FOREIGN KEY ("idUsuarioAdministrador")
        REFERENCES "Tbl_Usuario" ("id")
);