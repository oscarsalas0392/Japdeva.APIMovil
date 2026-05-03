CREATE TABLE IF NOT EXISTS "Tbl_DocumentoInternoHistorico" (
    id                      BIGSERIAL,
    "idDetalleReclamo"            BIGINT NOT NULL, 
    "nombreDocumento"      VARCHAR(255) NOT NULL,
    documento              TEXT NOT NULL,
    "fechaRegistro"        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "idUsuarioInterno"     BIGINT NOT NULL,
    activo                 BOOLEAN DEFAULT TRUE,
	CONSTRAINT pk_DocumentoInternoHistorico_id PRIMARY KEY(id),
	CONSTRAINT fk_DocumentoInternoHistorico_idDetalleReclamo FOREIGN KEY ("idDetalleReclamo") REFERENCES "Tbl_DetalleReclamoHistorico"(id)
);