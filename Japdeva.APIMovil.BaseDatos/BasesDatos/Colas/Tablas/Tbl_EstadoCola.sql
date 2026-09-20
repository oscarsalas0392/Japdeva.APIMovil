CREATE TABLE IF NOT EXISTS "Tbl_EstadoCola" (
    "id"                   BIGSERIAL  PRIMARY KEY,
    "colaId"               BIGINT     NOT NULL,
    "mensajesTotales"      INTEGER    NOT NULL DEFAULT 0,
    "mensajesPendientes"   INTEGER    NOT NULL DEFAULT 0,
    "mensajesEnProceso"    INTEGER    NOT NULL DEFAULT 0,
    "mensajesProcesados"   INTEGER    NOT NULL DEFAULT 0,
    "mensajesFallidos"     INTEGER    NOT NULL DEFAULT 0,
    "mensajesCancelados"   INTEGER    NOT NULL DEFAULT 0,
    "mensajesExpirados"    INTEGER    NOT NULL DEFAULT 0,
    "fechaRegistro"        TIMESTAMP  NOT NULL DEFAULT NOW(),
    "fechaEdicion"         TIMESTAMP  NULL,
    CONSTRAINT "fk_estadocola_cola" FOREIGN KEY ("colaId")
        REFERENCES "Tbl_Cola" ("id")
);