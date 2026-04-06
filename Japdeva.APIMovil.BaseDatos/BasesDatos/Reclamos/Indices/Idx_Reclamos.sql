CREATE INDEX IF NOT EXISTS "Idx_Reclamo_IdUsuarioExterno"
    ON "Tbl_Reclamo" ("idUsuarioExterno");

CREATE INDEX IF NOT EXISTS "Idx_Reclamo_IdEstadoReclamo"
    ON "Tbl_Reclamo" ("idEstadoReclamo");

CREATE INDEX IF NOT EXISTS "Idx_DetalleReclamo_IdReclamo"
    ON "Tbl_DetalleReclamo" ("idReclamo");

CREATE INDEX IF NOT EXISTS "Idx_DetalleReclamo_IdEstado"
    ON "Tbl_DetalleReclamo" ("idEstadoDetalleReclamo");

CREATE INDEX IF NOT EXISTS "Idx_DocumentoUsuario_IdReclamo"
    ON "Tbl_DocumentoUsuario" ("idReclamo");

CREATE INDEX IF NOT EXISTS "Idx_DocumentoInterno_IdDetalleReclamo"
    ON "Tbl_DocumentoInterno" ("idDetalleReclamo");

CREATE INDEX IF NOT EXISTS "Idx_EstadoDetalleReclamoOrdenProceso_IdNivelProceso"
    ON "Tbl_EstadoDetalleReclamoOrdenProceso" ("idNivelProceso");

CREATE INDEX IF NOT EXISTS "Idx_OrdenNivelProceso_IdNivelSuperior"
    ON "Tbl_OrdenNivelProceso" ("idNivelSuperior");
