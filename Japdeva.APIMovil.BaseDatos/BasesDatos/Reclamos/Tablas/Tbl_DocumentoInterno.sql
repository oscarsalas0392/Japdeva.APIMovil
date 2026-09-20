CREATE TABLE IF NOT EXISTS "Tbl_DocumentoInterno" (
    id                      BIGSERIAL,
    "idDetalleReclamo"            BIGINT NOT NULL, -- Nota: El [Column] dice "idReclamo" pero la propiedad es IdDetalleReclamo
    "nombreDocumento"      VARCHAR(255) NOT NULL,
    documento              TEXT NOT NULL,
    "fechaRegistro"        TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "idUsuarioInterno"     BIGINT NOT NULL,
    activo                 BOOLEAN DEFAULT TRUE,

	CONSTRAINT pk_DocumentoInterno_id PRIMARY KEY(id),
	CONSTRAINT fk_DocumentoInterno_idDetalleReclamo FOREIGN KEY ("idDetalleReclamo") REFERENCES "Tbl_DetalleReclamo"(id)
);