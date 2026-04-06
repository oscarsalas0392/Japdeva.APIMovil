CREATE TABLE IF NOT EXISTS "Tbl_DocumentoUsuario" (
    id                      BIGSERIAL ,
    "idReclamo"            BIGINT NOT NULL,
    "nombreDocumento"      VARCHAR(255) NOT NULL,
    documento              VARCHAR(4000) NOT NULL,
    "fechaRegistro"        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

	CONSTRAINT pk_DocumentoUsuario_id PRIMARY KEY(id),
	CONSTRAINT fk_DocumentoUsuario_idReclamo FOREIGN KEY ("idReclamo") REFERENCES "Tbl_Reclamo"(id)
);