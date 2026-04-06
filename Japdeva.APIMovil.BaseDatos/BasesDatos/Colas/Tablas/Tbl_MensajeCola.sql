CREATE TABLE IF NOT EXISTS "Tbl_MensajeCola" (
    "id"                 BIGSERIAL     PRIMARY KEY,
    "idRpc"              VARCHAR(100)  NOT NULL DEFAULT '',
    "colaId"             BIGINT        NOT NULL,
    "contenidoMensaje"   TEXT          NOT NULL,
    "estadoId"           INTEGER       NOT NULL DEFAULT 1,
    "prioridadId"        INTEGER       NOT NULL DEFAULT 2,
    "contadorReintentos" INTEGER       NOT NULL DEFAULT 0,
    "fechaRegistro"      TIMESTAMP     NOT NULL DEFAULT NOW(),
    "fechaEdicion"       TIMESTAMP     NULL,
    "mensajeError"       TEXT          NOT NULL DEFAULT '',
    "traceId"            VARCHAR(200)  NOT NULL DEFAULT '',
    "metadatos"          TEXT          NOT NULL DEFAULT '',
    CONSTRAINT "fk_mensajeCola_cola"   FOREIGN KEY ("colaId")
        REFERENCES "Tbl_Cola" ("id"),
    CONSTRAINT "fk_mensajeCola_estado" FOREIGN KEY ("estadoId")
        REFERENCES "Tbl_EstadoMensaje" ("id"),
    CONSTRAINT "fk_mensajeCola_prio"   FOREIGN KEY ("prioridadId")
        REFERENCES "Tbl_Prioridad" ("id")
);