CREATE TABLE IF NOT EXISTS  "Tbl_DetalleReclamo" (
    id                              BIGSERIAL,
    "idReclamo"                     BIGINT NOT NULL,
    "idNivelProceso"                INTEGER NOT NULL,
    "idDepartamento"                BIGINT NOT NULL,
    "idEstadoDetalleReclamo"        INTEGER NOT NULL,
    "idUsuarioInterno"              BIGINT,
    descripcion                     VARCHAR(1000) DEFAULT '',
    "fechaRegistro"                 TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "fechaEdicion"                  TIMESTAMP,

	CONSTRAINT pk_DetalleReclamo_id PRIMARY KEY(id),
	CONSTRAINT fk_DetalleReclamo_idReclamo FOREIGN KEY ("idReclamo") REFERENCES "Tbl_Reclamo"(id),
	CONSTRAINT fk_DetalleReclamo_idNivelProceso FOREIGN KEY ("idNivelProceso") REFERENCES "Tbl_NivelProceso"(id),
	CONSTRAINT fk_DetalleReclamo_idEstadoDetalleReclamo FOREIGN KEY ("idEstadoDetalleReclamo") REFERENCES "Tbl_EstadoDetalleReclamo"(id)
);