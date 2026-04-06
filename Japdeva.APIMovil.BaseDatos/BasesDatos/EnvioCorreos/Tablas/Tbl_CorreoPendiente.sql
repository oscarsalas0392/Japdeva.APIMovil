CREATE TABLE IF NOT EXISTS "Tbl_CorreoPendiente" (
    "id"            BIGSERIAL PRIMARY KEY,
    "destinatario"  VARCHAR(200) NOT NULL,
    "asunto"        VARCHAR(500) NOT NULL,
    "cuerpo"        TEXT NOT NULL,
    "intentos"      INTEGER NOT NULL DEFAULT 0,
    "ultimoError"   TEXT NULL,
    "esCuerpoHtml"  BOOLEAN NOT NULL DEFAULT TRUE,
    "enviado"       BOOLEAN NOT NULL DEFAULT FALSE,
    "fechaRegistro" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion"  TIMESTAMP WITH TIME ZONE NULL
);
