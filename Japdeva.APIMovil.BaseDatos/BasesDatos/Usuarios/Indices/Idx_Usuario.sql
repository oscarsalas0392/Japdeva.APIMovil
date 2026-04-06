CREATE UNIQUE INDEX IF NOT EXISTS "Idx_Usuario_Correo"
    ON "Tbl_Usuario" ("correo")
    WHERE "activo" = TRUE;

CREATE INDEX IF NOT EXISTS "Idx_Usuario_Identificacion"
    ON "Tbl_Usuario" ("identificacion");

CREATE INDEX IF NOT EXISTS "Idx_Usuario_Rol_IdUsuario"
    ON "Tbl_Usuario_Rol" ("idUsuario");

CREATE INDEX IF NOT EXISTS "Idx_Usuario_Rol_IdRol"
    ON "Tbl_Usuario_Rol" ("idRol");

CREATE INDEX IF NOT EXISTS "Idx_Departamento_Usuario_IdUsuario"
    ON "Tbl_Departamento_Usuario" ("idUsuario");

CREATE INDEX IF NOT EXISTS "Idx_Departamento_Usuario_IdDepartamento"
    ON "Tbl_Departamento_Usuario" ("idDepartamento");
