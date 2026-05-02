CREATE TABLE IF NOT EXISTS "Tbl_Usuario" (
    "id"             SERIAL          NOT NULL,
    "identificacion" VARCHAR(50)     NOT NULL,
    "idTipoCedula"   INTEGER         NOT NULL,
    "nombre"         VARCHAR(200)    NOT NULL,
    "apellidos"      VARCHAR(200)    NOT NULL,
    "correo"         VARCHAR(200)    NOT NULL,
    "contrasena"     TEXT            NOT NULL,
    "fechaRegistro"  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
	"fechaExpiracionContrasena"  TIMESTAMPTZ NULL DEFAULT NULL,
    "fechaEdicion"   TIMESTAMPTZ     NULL,
    "telefono"           VARCHAR(20)     NULL,
    "fechaNacimiento"    DATE            NULL,
    "activo"             BOOLEAN         NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_Tbl_Usuario"              PRIMARY KEY ("id"),
    CONSTRAINT "FK_Tbl_Usuario_TipoCedula"   FOREIGN KEY ("idTipoCedula")
        REFERENCES "Tbl_TipoCedula" ("id")
);