INSERT INTO "Tbl_Prioridad" ("id", "nombre", "descripcion")
SELECT 1, 'Alta', 'Mensajes criticos con atencion inmediata'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Prioridad" WHERE "id" = 1);

INSERT INTO "Tbl_Prioridad" ("id", "nombre", "descripcion")
SELECT 2, 'Media', 'Mensajes de prioridad estandar'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Prioridad" WHERE "id" = 2);

INSERT INTO "Tbl_Prioridad" ("id", "nombre", "descripcion")
SELECT 3, 'Baja', 'Mensajes de baja urgencia'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Prioridad" WHERE "id" = 3);
