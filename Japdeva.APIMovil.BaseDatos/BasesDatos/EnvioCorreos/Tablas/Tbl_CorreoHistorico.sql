CREATE TABLE IF NOT EXISTS "Tbl_CorreoHistorico" (
    "id"              BIGSERIAL PRIMARY KEY,
    "destinatario"    VARCHAR(200) NOT NULL,
    "asunto"          VARCHAR(500) NOT NULL,
    "cuerpo"          TEXT NOT NULL,
    "intentos"        INTEGER NOT NULL,
    "ultimoError"     TEXT NULL,
    "esCuerpoHtml"    BOOLEAN NOT NULL,
    "enviado"         BOOLEAN NOT NULL,
    "fechaRegistro"   TIMESTAMP WITH TIME ZONE NOT NULL,
    "fechaEdicion"    TIMESTAMP WITH TIME ZONE NULL,
    "fechaMovimiento" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
