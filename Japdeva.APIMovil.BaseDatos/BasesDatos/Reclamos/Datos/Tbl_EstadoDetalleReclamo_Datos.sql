INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 1, 'Pendiente', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 1);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 2, 'En Revision', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 2);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 3, 'Aprobado', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 3);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 4, 'Rechazado', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 4);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 5, 'Devuelto', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 5);

INSERT INTO "Tbl_EstadoDetalleReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 6, 'Finalizado', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoDetalleReclamo" WHERE "id" = 6);
