-- =============================================
-- Script de creación de tablas para el módulo de Parámetros
-- Base de datos: PostgreSQL
-- Proyecto: Japdeva.APIMovil.Parametros
-- =============================================

-- =============================================
-- Tabla: Tbl_Pantalla
-- Descripción: Almacena las pantallas del sistema
-- =============================================
CREATE TABLE IF NOT EXISTS "Tbl_Pantalla" (
    "id" SERIAL PRIMARY KEY,
    "descripcion" VARCHAR(100) NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Índices para Tbl_Pantalla
CREATE INDEX IF NOT EXISTS "idx_pantalla_activo" ON "Tbl_Pantalla" ("activo");
CREATE INDEX IF NOT EXISTS "idx_pantalla_fechaRegistro" ON "Tbl_Pantalla" ("fechaRegistro");

-- Comentarios para Tbl_Pantalla
COMMENT ON TABLE "Tbl_Pantalla" IS 'Almacena las pantallas del sistema';
COMMENT ON COLUMN "Tbl_Pantalla"."id" IS 'Identificador único de la pantalla';
COMMENT ON COLUMN "Tbl_Pantalla"."descripcion" IS 'Descripción de la pantalla';
COMMENT ON COLUMN "Tbl_Pantalla"."idUsuarioInterno" IS 'Usuario que registró la pantalla';
COMMENT ON COLUMN "Tbl_Pantalla"."fechaRegistro" IS 'Fecha de registro de la pantalla';
COMMENT ON COLUMN "Tbl_Pantalla"."fechaEdicion" IS 'Fecha de última edición de la pantalla';
COMMENT ON COLUMN "Tbl_Pantalla"."activo" IS 'Indica si la pantalla está activa';

-- =============================================
-- Tabla: Tbl_Menu
-- Descripción: Almacena los menús del sistema
-- =============================================
CREATE TABLE IF NOT EXISTS "Tbl_Menu" (
    "id" SERIAL PRIMARY KEY,
    "descripcion" VARCHAR(100) NOT NULL,
    "mostrar" BOOLEAN NOT NULL DEFAULT TRUE,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Índices para Tbl_Menu
CREATE INDEX IF NOT EXISTS "idx_menu_activo" ON "Tbl_Menu" ("activo");
CREATE INDEX IF NOT EXISTS "idx_menu_mostrar" ON "Tbl_Menu" ("mostrar");
CREATE INDEX IF NOT EXISTS "idx_menu_fechaRegistro" ON "Tbl_Menu" ("fechaRegistro");

-- Comentarios para Tbl_Menu
COMMENT ON TABLE "Tbl_Menu" IS 'Almacena los menús del sistema';
COMMENT ON COLUMN "Tbl_Menu"."id" IS 'Identificador único del menú';
COMMENT ON COLUMN "Tbl_Menu"."descripcion" IS 'Descripción del menú';
COMMENT ON COLUMN "Tbl_Menu"."mostrar" IS 'Indica si el menú debe mostrarse en la interfaz';
COMMENT ON COLUMN "Tbl_Menu"."idUsuarioInterno" IS 'Usuario que registró el menú';
COMMENT ON COLUMN "Tbl_Menu"."fechaRegistro" IS 'Fecha de registro del menú';
COMMENT ON COLUMN "Tbl_Menu"."fechaEdicion" IS 'Fecha de última edición del menú';
COMMENT ON COLUMN "Tbl_Menu"."activo" IS 'Indica si el menú está activo';

-- =============================================
-- Tabla: Tbl_MenuPerfil
-- Descripción: Relaciona menús con perfiles de usuario
-- =============================================
CREATE TABLE IF NOT EXISTS "Tbl_MenuPerfil" (
    "id" SERIAL PRIMARY KEY,
    "idMenu" INTEGER NOT NULL,
    "idPerfil" INTEGER NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE,

    -- Clave foránea
    CONSTRAINT "fk_menuperfil_menu" FOREIGN KEY ("idMenu") REFERENCES "Tbl_Menu" ("id") ON DELETE CASCADE
);

-- Índices para Tbl_MenuPerfil
CREATE INDEX IF NOT EXISTS "idx_menuperfil_idMenu" ON "Tbl_MenuPerfil" ("idMenu");
CREATE INDEX IF NOT EXISTS "idx_menuperfil_idPerfil" ON "Tbl_MenuPerfil" ("idPerfil");
CREATE INDEX IF NOT EXISTS "idx_menuperfil_activo" ON "Tbl_MenuPerfil" ("activo");
CREATE INDEX IF NOT EXISTS "idx_menuperfil_idMenu_idPerfil" ON "Tbl_MenuPerfil" ("idMenu", "idPerfil");

-- Comentarios para Tbl_MenuPerfil
COMMENT ON TABLE "Tbl_MenuPerfil" IS 'Relaciona menús con perfiles de usuario';
COMMENT ON COLUMN "Tbl_MenuPerfil"."id" IS 'Identificador único de la relación menú-perfil';
COMMENT ON COLUMN "Tbl_MenuPerfil"."idMenu" IS 'Identificador del menú';
COMMENT ON COLUMN "Tbl_MenuPerfil"."idPerfil" IS 'Identificador del perfil';
COMMENT ON COLUMN "Tbl_MenuPerfil"."idUsuarioInterno" IS 'Usuario que registró la relación';
COMMENT ON COLUMN "Tbl_MenuPerfil"."fechaRegistro" IS 'Fecha de registro de la relación';
COMMENT ON COLUMN "Tbl_MenuPerfil"."fechaEdicion" IS 'Fecha de última edición de la relación';
COMMENT ON COLUMN "Tbl_MenuPerfil"."activo" IS 'Indica si la relación está activa';

-- =============================================
-- Tabla: Tbl_TipoMensaje
-- Descripción: Almacena los tipos de mensajes del sistema
-- =============================================
CREATE TABLE IF NOT EXISTS "Tbl_TipoMensaje" (
    "id" SERIAL PRIMARY KEY,
    "descripcion" VARCHAR(100) NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Índices para Tbl_TipoMensaje
CREATE INDEX IF NOT EXISTS "idx_tipomensaje_activo" ON "Tbl_TipoMensaje" ("activo");
CREATE INDEX IF NOT EXISTS "idx_tipomensaje_fechaRegistro" ON "Tbl_TipoMensaje" ("fechaRegistro");

-- Comentarios para Tbl_TipoMensaje
COMMENT ON TABLE "Tbl_TipoMensaje" IS 'Almacena los tipos de mensajes del sistema';
COMMENT ON COLUMN "Tbl_TipoMensaje"."id" IS 'Identificador único del tipo de mensaje';
COMMENT ON COLUMN "Tbl_TipoMensaje"."descripcion" IS 'Descripción del tipo de mensaje';
COMMENT ON COLUMN "Tbl_TipoMensaje"."idUsuarioInterno" IS 'Usuario que registró el tipo de mensaje';
COMMENT ON COLUMN "Tbl_TipoMensaje"."fechaRegistro" IS 'Fecha de registro del tipo de mensaje';
COMMENT ON COLUMN "Tbl_TipoMensaje"."fechaEdicion" IS 'Fecha de última edición del tipo de mensaje';
COMMENT ON COLUMN "Tbl_TipoMensaje"."activo" IS 'Indica si el tipo de mensaje está activo';

-- =============================================
-- Tabla: Tbl_Mensaje
-- Descripción: Almacena los mensajes del sistema asociados a pantallas
-- =============================================
CREATE TABLE IF NOT EXISTS "Tbl_Mensaje" (
    "id" SERIAL PRIMARY KEY,
    "descripcion" VARCHAR(300) NOT NULL,
    "idTipoMensaje" INTEGER NOT NULL,
    "idPantalla" INTEGER NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE,

    -- Claves foráneas
    CONSTRAINT "fk_mensaje_tipomensaje" FOREIGN KEY ("idTipoMensaje") REFERENCES "Tbl_TipoMensaje" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_mensaje_pantalla" FOREIGN KEY ("idPantalla") REFERENCES "Tbl_Pantalla" ("id") ON DELETE RESTRICT
);

-- Índices para Tbl_Mensaje
CREATE INDEX IF NOT EXISTS "idx_mensaje_idTipoMensaje" ON "Tbl_Mensaje" ("idTipoMensaje");
CREATE INDEX IF NOT EXISTS "idx_mensaje_idPantalla" ON "Tbl_Mensaje" ("idPantalla");
CREATE INDEX IF NOT EXISTS "idx_mensaje_activo" ON "Tbl_Mensaje" ("activo");
CREATE INDEX IF NOT EXISTS "idx_mensaje_idPantalla_activo" ON "Tbl_Mensaje" ("idPantalla", "activo");

-- Comentarios para Tbl_Mensaje
COMMENT ON TABLE "Tbl_Mensaje" IS 'Almacena los mensajes del sistema asociados a pantallas';
COMMENT ON COLUMN "Tbl_Mensaje"."id" IS 'Identificador único del mensaje';
COMMENT ON COLUMN "Tbl_Mensaje"."descripcion" IS 'Contenido del mensaje';
COMMENT ON COLUMN "Tbl_Mensaje"."idTipoMensaje" IS 'Identificador del tipo de mensaje';
COMMENT ON COLUMN "Tbl_Mensaje"."idPantalla" IS 'Identificador de la pantalla asociada';
COMMENT ON COLUMN "Tbl_Mensaje"."idUsuarioInterno" IS 'Usuario que registró el mensaje';
COMMENT ON COLUMN "Tbl_Mensaje"."fechaRegistro" IS 'Fecha de registro del mensaje';
COMMENT ON COLUMN "Tbl_Mensaje"."fechaEdicion" IS 'Fecha de última edición del mensaje';
COMMENT ON COLUMN "Tbl_Mensaje"."activo" IS 'Indica si el mensaje está activo';

-- =============================================
-- Tabla: Tbl_PlantillaCorreo
-- Descripción: Almacena plantillas de correo electrónico para notificaciones
-- =============================================
CREATE TABLE IF NOT EXISTS "Tbl_PlantillaCorreo" (
    "id" SERIAL PRIMARY KEY,
    "Plantilla" TEXT NOT NULL,
    "idUsuarioInterno" BIGINT NULL,
    "fechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "fechaEdicion" TIMESTAMP WITHOUT TIME ZONE NULL,
    "activo" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Índices para Tbl_PlantillaCorreo
CREATE INDEX IF NOT EXISTS "idx_plantillacorreo_activo" ON "Tbl_PlantillaCorreo" ("activo");
CREATE INDEX IF NOT EXISTS "idx_plantillacorreo_fechaRegistro" ON "Tbl_PlantillaCorreo" ("fechaRegistro");

-- Comentarios para Tbl_PlantillaCorreo
COMMENT ON TABLE "Tbl_PlantillaCorreo" IS 'Almacena plantillas de correo electrónico para notificaciones';
COMMENT ON COLUMN "Tbl_PlantillaCorreo"."id" IS 'Identificador único de la plantilla de correo';
COMMENT ON COLUMN "Tbl_PlantillaCorreo"."Plantilla" IS 'Contenido de la plantilla de correo en formato HTML o texto';
COMMENT ON COLUMN "Tbl_PlantillaCorreo"."idUsuarioInterno" IS 'Usuario que registró la plantilla';
COMMENT ON COLUMN "Tbl_PlantillaCorreo"."fechaRegistro" IS 'Fecha de registro de la plantilla';
COMMENT ON COLUMN "Tbl_PlantillaCorreo"."fechaEdicion" IS 'Fecha de última edición de la plantilla';
COMMENT ON COLUMN "Tbl_PlantillaCorreo"."activo" IS 'Indica si la plantilla está activa';

-- =============================================
-- Fin del script de creación de tablas
-- =============================================
