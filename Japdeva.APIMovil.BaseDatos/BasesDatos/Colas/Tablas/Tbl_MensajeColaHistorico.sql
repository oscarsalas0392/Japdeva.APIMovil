CREATE TABLE IF NOT EXISTS "Tbl_MensajeColaHistorico" (
    "id"                 BIGSERIAL     PRIMARY KEY,
    "idMensajeCola"      BIGINT        NOT NULL,
    "colaId"             BIGINT        NOT NULL,
    "contenidoMensaje"   VARCHAR(4000) NOT NULL,
    "estadoId"           INTEGER       NOT NULL DEFAULT 1,
    "prioridadId"        INTEGER       NOT NULL DEFAULT 2,
    "contadorReintentos" INTEGER       NOT NULL DEFAULT 0,
    "fechaRegistro"      TIMESTAMP     NOT NULL DEFAULT NOW(),
    "fechaEdicion"       TIMESTAMP     NULL,
    "mensajeError"       TEXT          NOT NULL DEFAULT '',
    "traceId"            VARCHAR(200)  NOT NULL DEFAULT '',
    "metadatos"          VARCHAR(4000) NOT NULL DEFAULT '',
    "fechaArchivado"     TIMESTAMP     NOT NULL,
    CONSTRAINT "fk_historico_cola"   FOREIGN KEY ("colaId")
        REFERENCES "Tbl_Cola" ("id"),
    CONSTRAINT "fk_historico_estado" FOREIGN KEY ("estadoId")
        REFERENCES "Tbl_EstadoMensaje" ("id"),
    CONSTRAINT "fk_historico_prio"   FOREIGN KEY ("prioridadId")
        REFERENCES "Tbl_Prioridad" ("id")
);