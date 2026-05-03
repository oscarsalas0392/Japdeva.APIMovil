CREATE TABLE IF NOT EXISTS "Tbl_EstadoDetalleReclamoOrdenProceso" (
    id                              SERIAL,
    "idNivelProceso"                INTEGER NOT NULL,
    "idEstadoDetalleReclamo"        INTEGER NOT NULL,
    "idUsuarioInterno"              BIGINT NOT NULL,
    "fechaRegistro"                 TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    activo                          BOOLEAN DEFAULT TRUE,
	CONSTRAINT pk_EstadoDetalleReclamoOrdenProceso_id PRIMARY KEY(id),
	CONSTRAINT fk_EstadoDetalleReclamoOrdenProceso_idEstadoDetalleReclamo FOREIGN KEY ("idEstadoDetalleReclamo") REFERENCES "Tbl_EstadoDetalleReclamo"(id),
	CONSTRAINT fk_EstadoDetalleReclamoOrdenProceso_idNivelProceso FOREIGN KEY ("idNivelProceso") REFERENCES "Tbl_NivelProceso"(id)
);
