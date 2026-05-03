CREATE INDEX IF NOT EXISTS "Idx_MensajeCola_ColaId_EstadoId"
    ON "Tbl_MensajeCola" ("colaId", "estadoId");

CREATE INDEX IF NOT EXISTS "Idx_MensajeCola_EstadoId"
    ON "Tbl_MensajeCola" ("estadoId");

CREATE INDEX IF NOT EXISTS "Idx_MensajeCola_IdRpc"
    ON "Tbl_MensajeCola" ("idRpc")
    WHERE "idRpc" <> '';

CREATE INDEX IF NOT EXISTS "Idx_MensajeColaHistorico_IdMensajeCola"
    ON "Tbl_MensajeColaHistorico" ("idMensajeCola");

CREATE UNIQUE INDEX IF NOT EXISTS "Idx_EstadoCola_ColaId"
    ON "Tbl_EstadoCola" ("colaId");
