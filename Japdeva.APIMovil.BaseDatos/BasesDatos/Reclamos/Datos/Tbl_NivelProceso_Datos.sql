INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 1, 'Revisión inicial de Reclamos ingresados por usuarios externos', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 1);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 2, 'Solicitud de criterio a la Unidad de Intendecia Portuaria', 1, 1, 2
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 2);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 3, 'Revisión de criterio enviado por la Unidad de Intendecia Portuaria', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 3);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 4, 'Envio de criterio a Unidad Financiero Contable', 1, 1, 3
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 4);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 5, 'Revisión de criterio enviado por la Unidad Financiero Contable', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 5);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 6, 'Envio de criterio a Unidad de Asesoría Jurídica', 1, 1, 4
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 6);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 7, 'Revisión de criterio enviado por la Unidad de Asesoría Jurídica', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 7);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 8, 'Envio de criterio a Gerencia Portuaria', 1, 1, 5
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 8);

INSERT INTO "Tbl_NivelProceso" (id, descripcion, nivel, "idUsuarioInterno", "idDepartamento")
SELECT 9, 'Revisión de resolución firmada por la Gerencia Portuaria', 1, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_NivelProceso" WHERE "id" = 9);
