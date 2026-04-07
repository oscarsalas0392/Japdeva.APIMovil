INSERT INTO "Tbl_EstadoMensaje" ("id", "nombre", "descripcion")
SELECT 1, 'Pendiente', 'Mensaje en espera de ser procesado'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoMensaje" WHERE "id" = 1);

INSERT INTO "Tbl_EstadoMensaje" ("id", "nombre", "descripcion")
SELECT 2, 'EnProceso', 'Mensaje siendo procesado por un consumidor'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoMensaje" WHERE "id" = 2);

INSERT INTO "Tbl_EstadoMensaje" ("id", "nombre", "descripcion")
SELECT 3, 'Exitoso', 'Mensaje procesado correctamente'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoMensaje" WHERE "id" = 3);

INSERT INTO "Tbl_EstadoMensaje" ("id", "nombre", "descripcion")
SELECT 4, 'Fallido', 'Procesamiento fallido, pendiente de reintento'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoMensaje" WHERE "id" = 4);

INSERT INTO "Tbl_EstadoMensaje" ("id", "nombre", "descripcion")
SELECT 5, 'Expirado', 'Mensaje no procesado dentro del tiempo limite'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_EstadoMensaje" WHERE "id" = 5);
