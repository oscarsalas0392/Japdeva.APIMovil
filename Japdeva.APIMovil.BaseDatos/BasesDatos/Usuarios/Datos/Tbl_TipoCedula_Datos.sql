INSERT INTO "Tbl_TipoCedula" ("id", "tipo", "formato", "fechaRegistro", "activo")
SELECT 1, 'Cedula Fisica', '^\d{9}$', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_TipoCedula" WHERE "id" = 1);

INSERT INTO "Tbl_TipoCedula" ("id", "tipo", "formato", "fechaRegistro", "activo")
SELECT 2, 'Cedula Juridica', '^\d{10}$', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_TipoCedula" WHERE "id" = 2);

INSERT INTO "Tbl_TipoCedula" ("id", "tipo", "formato", "fechaRegistro", "activo")
SELECT 3, 'DIMEX', '^\d{11,12}$', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_TipoCedula" WHERE "id" = 3);

INSERT INTO "Tbl_TipoCedula" ("id", "tipo", "formato", "fechaRegistro", "activo")
SELECT 4, 'Pasaporte', '^[A-Z0-9]{6,9}$', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_TipoCedula" WHERE "id" = 4);
