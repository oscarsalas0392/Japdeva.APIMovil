INSERT INTO "Tbl_Cola" ("id", "nombre", "activo", "fechaRegistro")
SELECT 1, 'ObtenerPlantilla', TRUE, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Cola" WHERE "id" = 1);

INSERT INTO "Tbl_Cola" ("id", "nombre", "activo", "fechaRegistro")
SELECT 2, 'Respuesta', TRUE, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Cola" WHERE "id" = 2);

INSERT INTO "Tbl_Cola" ("id", "nombre", "activo", "fechaRegistro")
SELECT 3, 'EnviarCorreo', TRUE, NOW()
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Cola" WHERE "id" = 3);
