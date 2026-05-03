CREATE TABLE IF NOT EXISTS "Tbl_DetalleApelacionReclamo" (
    id                              BIGSERIAL,
    "idApelacionReclamo"            BIGINT NOT NULL,
    "idNivelProceso"                INTEGER NOT NULL,
    "idDepartamento"                BIGINT NOT NULL,
    "idEstadoDetalleReclamo"        INTEGER NOT NULL,
    "idUsuarioInterno"              BIGINT,
    descripcion                     VARCHAR(1000) DEFAULT '',
    "fechaRegistro"                 TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "fechaEdicion"                  TIMESTAMP,

    CONSTRAINT pk_DetalleApelacionReclamo_id PRIMARY KEY (id),
    CONSTRAINT fk_DetalleApelacionReclamo_idApelacionReclamo FOREIGN KEY ("idApelacionReclamo") REFERENCES "Tbl_ApelacionReclamo"(id),
    CONSTRAINT fk_DetalleApelacionReclamo_idNivelProceso FOREIGN KEY ("idNivelProceso") REFERENCES "Tbl_NivelProceso"(id),
    CONSTRAINT fk_DetalleApelacionReclamo_idEstadoDetalleReclamo FOREIGN KEY ("idEstadoDetalleReclamo") REFERENCES "Tbl_EstadoDetalleReclamo"(id)
);
