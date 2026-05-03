INSERT INTO "Tbl_EstadoReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 1, 'Pendiente', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoReclamo" WHERE "id" = 1);

INSERT INTO "Tbl_EstadoReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 2, 'En Proceso', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoReclamo" WHERE "id" = 2);

INSERT INTO "Tbl_EstadoReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 3, 'Aceptado', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoReclamo" WHERE "id" = 3);

INSERT INTO "Tbl_EstadoReclamo" ("id", "descripcion", "idUsuarioInterno", "fechaRegistro")
SELECT 4, 'Denegado', 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoReclamo" WHERE "id" = 4);
