INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "continuaProceso", "rechazaProceso", "devolucionProceso", "finalizarProceso", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 1, 'Pendiente', false, false, false, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 1);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "continuaProceso", "rechazaProceso", "devolucionProceso", "finalizarProceso", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 2, 'En Revision', false, false, false, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 2);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "continuaProceso", "rechazaProceso", "devolucionProceso", "finalizarProceso", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 3, 'Validado', true, false, false, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 3);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "continuaProceso", "rechazaProceso", "devolucionProceso", "finalizarProceso", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 4, 'Denegado', false, true, false, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 4);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "continuaProceso", "rechazaProceso", "devolucionProceso", "finalizarProceso", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 5, 'Devuelto', false, false, true, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 5);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "continuaProceso", "rechazaProceso", "devolucionProceso", "finalizarProceso", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 6, 'Autorizado', false, false, false, true, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 6);
