INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 1, 1, 4, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 1);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 2, 2, 4, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 2);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 3, 3, 4, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 3);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 4, 4, 4, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 4);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 5, 5, 4, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 5);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 6, 1, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 6);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 7, 6, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 7);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 8, 7, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 8);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 9, 8, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 9);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 10, 1, 2, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 10);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 11, 6, 2, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 11);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 12, 7, 2, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 12);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 13, 8, 2, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 13);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 14, 1, 3, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 14);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 15, 6, 3, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 15);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 16, 7, 3, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 16);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 17, 8, 3, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 17);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 18, 9, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 18);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 19, 9, 2, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 19);

INSERT INTO "Tbl_MenuPerfil" ("id", "idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 20, 9, 3, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_MenuPerfil" WHERE "id" = 20);
