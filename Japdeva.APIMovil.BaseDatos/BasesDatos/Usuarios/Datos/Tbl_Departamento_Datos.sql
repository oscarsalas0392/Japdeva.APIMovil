INSERT INTO "Tbl_Departamento" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 1, 'Contraloría de Servicios', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Departamento" WHERE "id" = 1);

INSERT INTO "Tbl_Departamento" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 2, 'Unidad de Intendencia Portuaria', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Departamento" WHERE "id" = 2);

INSERT INTO "Tbl_Departamento" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 3, 'Unidad Financiero Contable', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Departamento" WHERE "id" = 3);

INSERT INTO "Tbl_Departamento" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 4, 'Unidad de Asesoría Jurídica', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Departamento" WHERE "id" = 4);

INSERT INTO "Tbl_Departamento" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 5, 'Gerencia Portuaria', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Departamento" WHERE "id" = 5);
