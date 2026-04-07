INSERT INTO "Tbl_Rol" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 1, 'Administrador', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Rol" WHERE "id" = 1);

INSERT INTO "Tbl_Rol" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 2, 'Supervisor', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Rol" WHERE "id" = 2);

INSERT INTO "Tbl_Rol" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 3, 'Operador', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Rol" WHERE "id" = 3);

INSERT INTO "Tbl_Rol" ("id", "descripcion", "fechaRegistro", "activo")
SELECT 4, 'Usuario Externo', NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Rol" WHERE "id" = 4);
