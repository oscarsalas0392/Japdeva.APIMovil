INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 1, 'Mi Perfil', '/perfil', 'person-outline', 1, NULL, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 1);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 2, 'Reclamos', NULL, 'document-text-outline', 2, NULL, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 2);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 3, 'Nuevo Reclamo', '/nuevo-reclamo', 'add-circle-outline', 3, 2, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 3);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 4, 'Buscar Reclamos', '/buscar', 'search-outline', 4, 2, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 4);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 5, 'Información', '/informacion', 'information-circle-outline', 5, NULL, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 5);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 6, 'Gestión de Usuarios', '/buscar-usuario-cedula', 'people-outline', 2, NULL, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 6);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 9, 'Reclamos', NULL, 'document-text-outline', 3, NULL, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 9);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 7, 'Buscar por Fecha y Estado', '/buscar-reclamos-fecha-estado', 'search-outline', 1, 9, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 7);

INSERT INTO "Tbl_Menu" ("id", "descripcion", "ruta", "icono", "orden", "idPadre", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 8, 'Buscar por ID', '/buscar-reclamos-id', 'document-text-outline', 2, 9, TRUE, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Menu" WHERE "id" = 8);

UPDATE "Tbl_Menu" SET "descripcion" = 'Buscar por Fecha y Estado', "idPadre" = 9, "orden" = 1 WHERE "id" = 7;
UPDATE "Tbl_Menu" SET "idPadre" = 9, "orden" = 2 WHERE "id" = 8;
