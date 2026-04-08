INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 1, 1, 2, false, 1, CURRENT_TIMESTAMP, true 
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 1)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 2, 2, 3, false, 1, CURRENT_TIMESTAMP, true 
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 2)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 3, 3, 4, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 3)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 4, 3, 6, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 4) 

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 5, 4, 5, false, 1, CURRENT_TIMESTAMP, true 
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 5)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 6, 5, 6, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 6)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 7, 6, 7, false, 1, CURRENT_TIMESTAMP, true 
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 7)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 8, 7, 8, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 8)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 9, 8, 9, false, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 9)

-- Devoluciones: Contraloría de Servicios regresa trabajo al departamento externo anterior
INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 10, 3, 2, true, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 10)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 11, 5, 4, true, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 11)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 12, 7, 6, true, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 12)

INSERT INTO "Tbl_OrdenNivelProceso" (id, "idNivelSuperior", "idNivelInferior" , "devolucionNivel","idUsuarioInterno","fechaRegistro","activo")
SELECT 13, 9, 8, true, 1, CURRENT_TIMESTAMP, true
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OrdenNivelProceso" WHERE "id" = 13)