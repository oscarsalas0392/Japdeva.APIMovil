CREATE INDEX IF NOT EXISTS "Idx_CorreoPendiente_Enviado"
    ON "Tbl_CorreoPendiente" ("enviado")
    WHERE "enviado" = FALSE;

CREATE INDEX IF NOT EXISTS "Idx_CorreoHistorico_Enviado"
    ON "Tbl_CorreoHistorico" ("enviado");

CREATE INDEX IF NOT EXISTS "Idx_CorreoHistorico_FechaMovimiento"
    ON "Tbl_CorreoHistorico" ("fechaMovimiento");
