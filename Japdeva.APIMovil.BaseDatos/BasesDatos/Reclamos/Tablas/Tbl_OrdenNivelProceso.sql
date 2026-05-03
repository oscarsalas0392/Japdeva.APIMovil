CREATE TABLE IF NOT EXISTS "Tbl_OrdenNivelProceso" (
    id                      SERIAL,
    "idNivelSuperior"       INTEGER NOT NULL,
    "idNivelInferior"       INTEGER NOT NULL,
    "devolucionNivel"       BOOLEAN DEFAULT FALSE,
    "idUsuarioInterno"      BIGINT NOT NULL,
    "fechaRegistro"         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    activo                  BOOLEAN DEFAULT TRUE,
	CONSTRAINT pk_OrdenNivelProceso_id PRIMARY KEY(id),
	CONSTRAINT fk_OrdenNivelProceso_idNivelSuperior FOREIGN KEY ("idNivelSuperior") REFERENCES "Tbl_NivelProceso"(id),
	CONSTRAINT fk_OrdenNivelProceso_idNivelInferior FOREIGN KEY ("idNivelInferior") REFERENCES "Tbl_NivelProceso"(id)
);