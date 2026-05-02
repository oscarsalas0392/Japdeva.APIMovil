CREATE TABLE IF NOT EXISTS "Tbl_ApelacionReclamo" (
    id                              BIGSERIAL,
    "idReclamo"                     BIGINT NOT NULL,
    titulo                          VARCHAR(200) NOT NULL,
    descripcion                     VARCHAR(1000) NOT NULL,
    "idEstadoReclamo"               INTEGER NOT NULL,
    "idUsuarioExterno"              BIGINT NOT NULL,
    "fechaRegistro"                 TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "idDepartamentoActual"          BIGINT NOT NULL,
    "descripcionResolucion"         VARCHAR(200) DEFAULT '',

    CONSTRAINT pk_ApelacionReclamo_id PRIMARY KEY (id),
    CONSTRAINT fk_ApelacionReclamo_idReclamo FOREIGN KEY ("idReclamo") REFERENCES "Tbl_Reclamo"(id),
    CONSTRAINT fk_ApelacionReclamo_idEstadoReclamo FOREIGN KEY ("idEstadoReclamo") REFERENCES "Tbl_EstadoReclamo"(id)
);
