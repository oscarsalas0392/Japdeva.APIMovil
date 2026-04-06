CREATE INDEX IF NOT EXISTS "Idx_Mensaje_IdTipoMensaje"
    ON "Tbl_Mensaje" ("idTipoMensaje");

CREATE INDEX IF NOT EXISTS "Idx_Mensaje_IdPantalla"
    ON "Tbl_Mensaje" ("idPantalla");

CREATE INDEX IF NOT EXISTS "Idx_MenuPerfil_IdMenu"
    ON "Tbl_MenuPerfil" ("idMenu");

CREATE INDEX IF NOT EXISTS "Idx_MenuPerfil_IdPerfil"
    ON "Tbl_MenuPerfil" ("idPerfil");
