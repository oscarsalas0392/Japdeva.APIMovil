CREATE TABLE IF NOT EXISTS"Tbl_Reclamo" (
    id                              BIGSERIAL,
    titulo                          VARCHAR(200) NOT NULL,
    descripcion                     VARCHAR(1000) NOT NULL,
    "idEstadoReclamo"              INTEGER NOT NULL,
    "idUsuarioExterno"             BIGINT NOT NULL,
    "fechaRegistro"                TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "idDepartamentoActual"         BIGINT NOT NULL,
    "descripcionResolucion"        VARCHAR(200) DEFAULT '',
    "EstaEnHistorico"              BOOLEAN DEFAULT FALSE,

	CONSTRAINT pk_Reclamo_id PRIMARY KEY(id),
	CONSTRAINT fk_Reclamo_idEstadoReclamo FOREIGN KEY ("idEstadoReclamo") REFERENCES "Tbl_EstadoReclamo"(id) ON UPDATE CASCADE ON DELETE RESTRICT
);