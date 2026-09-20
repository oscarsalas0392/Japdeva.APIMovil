INSERT INTO "Tbl_Proceso" (id, nombre, descripcion)
SELECT 1, 'Reclamo', 'Proceso de atención y resolución de reclamos presentados por usuarios externos'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Proceso" WHERE "id" = 1);

INSERT INTO "Tbl_Proceso" (id, nombre, descripcion)
SELECT 2, 'Apelación', 'Proceso de apelación ante la resolución emitida en el proceso de Reclamo'
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Proceso" WHERE "id" = 2);
