CREATE TABLE IF NOT EXISTS "Tbl_Departamento_Usuario" (
    "id"                     BIGSERIAL   NOT NULL,
    "idUsuario"              INTEGER     NOT NULL,
    "idDepartamento"         INTEGER     NOT NULL,
    "idUsuarioAdministrador" INTEGER     NOT NULL,
    "fechaRegistro"          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "fechaEdicion"           TIMESTAMPTZ NULL,
    "activo"                 BOOLEAN     NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_Tbl_Departamento_Usuario"              PRIMARY KEY ("id"),
    CONSTRAINT "FK_Tbl_Departamento_Usuario_Usuario"      FOREIGN KEY ("idUsuario")
        REFERENCES "Tbl_Usuario" ("id"),
    CONSTRAINT "FK_Tbl_Departamento_Usuario_Departamento" FOREIGN KEY ("idDepartamento")
        REFERENCES "Tbl_Departamento" ("id"),
    CONSTRAINT "FK_Tbl_Departamento_Usuario_Administrador" FOREIGN KEY ("idUsuarioAdministrador")
        REFERENCES "Tbl_Usuario" ("id")
);