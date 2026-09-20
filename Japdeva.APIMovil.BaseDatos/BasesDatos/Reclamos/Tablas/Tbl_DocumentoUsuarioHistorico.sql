CREATE TABLE IF NOT EXISTS "Tbl_DocumentoUsuarioHistorico" (
    id                      BIGSERIAL,
    "idReclamo"            BIGINT NOT NULL,
    "nombreDocumento"      VARCHAR(255) NOT NULL,
    documento              TEXT NOT NULL,
    "fechaRegistro"        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

	CONSTRAINT pk_DocumentoUsuarioHistorico_id PRIMARY KEY(id),
	CONSTRAINT fk_DocumentoUsuarioHistorico_idReclamo FOREIGN KEY ("idReclamo") REFERENCES "Tbl_Reclamo"(id)
);