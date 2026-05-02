CREATE TABLE IF NOT EXISTS"Tbl_DetalleReclamoHistorico" (
    id                              BIGINT,
    "idReclamo"                     BIGINT NOT NULL,
    "idNivelProceso"                INTEGER NOT NULL,
    "idDepartamento"                BIGINT NOT NULL,
    "idEstadoDetalleReclamo"        INTEGER NOT NULL,
    "idUsuarioInterno"              BIGINT,
    descripcion                     VARCHAR(1000) DEFAULT '',
    "fechaRegistro"                 TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "fechaEdicion"                  TIMESTAMP,

	
	CONSTRAINT pk_DetalleReclamoHistorico_id PRIMARY KEY(id),
	CONSTRAINT fk_DetalleReclamoHistorico_idReclamo FOREIGN KEY ("idReclamo") REFERENCES "Tbl_Reclamo"(id),
	CONSTRAINT fk_DetalleReclamoHistorico_idNivelProceso FOREIGN KEY ("idNivelProceso") REFERENCES "Tbl_NivelProceso"(id),
	CONSTRAINT fk_DetalleReclamoHistorico_idEstadoDetalleReclamo FOREIGN KEY ("idEstadoDetalleReclamo") REFERENCES "Tbl_EstadoDetalleReclamo"(id)
);