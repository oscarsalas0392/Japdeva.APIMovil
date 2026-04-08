INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 1, 1, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 1 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 1);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 2, 1, 4, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 2 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 2);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 3, 2, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 3 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 3);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 4, 3, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 4 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 4);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 5, 3, 5, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 5 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 5);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 6, 4, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 6 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 6);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 7, 5, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 7 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 7);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 8, 5, 5, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 8 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 8);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 9, 6, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 9 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 9);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 10, 7, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 10 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 10);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 11, 7, 5, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 11 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 11);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 12, 8, 3, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 12 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 12);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 13, 9, 4, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 12 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 13);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 14, 9, 5, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 14 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 14);

INSERT INTO Tbl_EstadoDetalleReclamoOrdenProceso (id, "idNivelProceso", "idEstadoDetalleReclamo" , "idUsuarioInterno","fechaRegistro")
SELECT 15, 9, 6, 1, CURRENT_TIMESTAMP 
WHERE NOT EXISTS (SELECT 15 FROM Tbl_EstadoDetalleReclamoOrdenProceso WHERE "id" = 15);