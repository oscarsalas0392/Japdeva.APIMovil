-- =============================================
-- Consultas útiles para el módulo de Parámetros
-- Base de datos: PostgreSQL
-- Proyecto: Japdeva.APIMovil.Parametros
-- =============================================

-- =============================================
-- CONSULTAS DE INFORMACIÓN
-- =============================================

-- Verificar todas las tablas del módulo
SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE tablename LIKE 'Tbl_%'
ORDER BY tablename;

-- Contar registros en todas las tablas
SELECT
    'Tbl_Pantalla' AS tabla,
    COUNT(*) AS total_registros,
    COUNT(*) FILTER (WHERE activo = TRUE) AS registros_activos
FROM "Tbl_Pantalla"
UNION ALL
SELECT 'Tbl_Menu', COUNT(*), COUNT(*) FILTER (WHERE activo = TRUE)
FROM "Tbl_Menu"
UNION ALL
SELECT 'Tbl_MenuPerfil', COUNT(*), COUNT(*) FILTER (WHERE activo = TRUE)
FROM "Tbl_MenuPerfil"
UNION ALL
SELECT 'Tbl_TipoMensaje', COUNT(*), COUNT(*) FILTER (WHERE activo = TRUE)
FROM "Tbl_TipoMensaje"
UNION ALL
SELECT 'Tbl_Mensaje', COUNT(*), COUNT(*) FILTER (WHERE activo = TRUE)
FROM "Tbl_Mensaje"
UNION ALL
SELECT 'Tbl_PlantillaCorreo', COUNT(*), COUNT(*) FILTER (WHERE activo = TRUE)
FROM "Tbl_PlantillaCorreo";

-- Verificar índices existentes
SELECT
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public'
    AND tablename LIKE 'Tbl_%'
ORDER BY tablename, indexname;

-- =============================================
-- CONSULTAS DE ANÁLISIS
-- =============================================

-- Menús más asignados a perfiles
SELECT
    m."id",
    m."descripcion" AS menu,
    COUNT(mp."id") AS total_perfiles,
    COUNT(mp."id") FILTER (WHERE mp."activo" = TRUE) AS perfiles_activos
FROM "Tbl_Menu" m
LEFT JOIN "Tbl_MenuPerfil" mp ON m."id" = mp."idMenu"
GROUP BY m."id", m."descripcion"
ORDER BY total_perfiles DESC;

-- Mensajes por pantalla y tipo
SELECT
    p."descripcion" AS pantalla,
    tm."descripcion" AS tipo_mensaje,
    COUNT(m."id") AS total_mensajes
FROM "Tbl_Mensaje" m
INNER JOIN "Tbl_Pantalla" p ON m."idPantalla" = p."id"
INNER JOIN "Tbl_TipoMensaje" tm ON m."idTipoMensaje" = tm."id"
WHERE m."activo" = TRUE
GROUP BY p."descripcion", tm."descripcion"
ORDER BY p."descripcion", total_mensajes DESC;

-- Registros creados por fecha (últimos 30 días)
SELECT
    DATE("fechaRegistro") AS fecha,
    COUNT(*) FILTER (WHERE tabla = 'Pantalla') AS pantallas,
    COUNT(*) FILTER (WHERE tabla = 'Menu') AS menus,
    COUNT(*) FILTER (WHERE tabla = 'Mensaje') AS mensajes
FROM (
    SELECT 'Pantalla' AS tabla, "fechaRegistro" FROM "Tbl_Pantalla"
    WHERE "fechaRegistro" >= NOW() - INTERVAL '30 days'
    UNION ALL
    SELECT 'Menu', "fechaRegistro" FROM "Tbl_Menu"
    WHERE "fechaRegistro" >= NOW() - INTERVAL '30 days'
    UNION ALL
    SELECT 'Mensaje', "fechaRegistro" FROM "Tbl_Mensaje"
    WHERE "fechaRegistro" >= NOW() - INTERVAL '30 days'
) AS registros
GROUP BY DATE("fechaRegistro")
ORDER BY fecha DESC;

-- =============================================
-- CONSULTAS DE MANTENIMIENTO
-- =============================================

-- Buscar registros huérfanos en MenuPerfil (sin menú asociado)
SELECT
    mp."id",
    mp."idMenu",
    mp."idPerfil"
FROM "Tbl_MenuPerfil" mp
LEFT JOIN "Tbl_Menu" m ON mp."idMenu" = m."id"
WHERE m."id" IS NULL;

-- Buscar mensajes sin tipo de mensaje o pantalla
SELECT
    m."id",
    m."descripcion",
    CASE
        WHEN tm."id" IS NULL THEN 'Sin tipo de mensaje'
        WHEN p."id" IS NULL THEN 'Sin pantalla'
        ELSE 'OK'
    END AS problema
FROM "Tbl_Mensaje" m
LEFT JOIN "Tbl_TipoMensaje" tm ON m."idTipoMensaje" = tm."id"
LEFT JOIN "Tbl_Pantalla" p ON m."idPantalla" = p."id"
WHERE tm."id" IS NULL OR p."id" IS NULL;

-- Registros sin editar en los últimos 6 meses
SELECT
    'Tbl_Pantalla' AS tabla,
    COUNT(*) AS sin_edicion
FROM "Tbl_Pantalla"
WHERE "fechaEdicion" IS NULL
    OR "fechaEdicion" < NOW() - INTERVAL '6 months'
UNION ALL
SELECT 'Tbl_Menu', COUNT(*)
FROM "Tbl_Menu"
WHERE "fechaEdicion" IS NULL
    OR "fechaEdicion" < NOW() - INTERVAL '6 months'
UNION ALL
SELECT 'Tbl_Mensaje', COUNT(*)
FROM "Tbl_Mensaje"
WHERE "fechaEdicion" IS NULL
    OR "fechaEdicion" < NOW() - INTERVAL '6 months';

-- =============================================
-- FUNCIONES DE AUDITORÍA
-- =============================================

-- Crear función para auditar cambios en fechaEdicion
CREATE OR REPLACE FUNCTION actualizar_fecha_edicion()
RETURNS TRIGGER AS $$
BEGIN
    NEW."fechaEdicion" = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Aplicar trigger a todas las tablas
DROP TRIGGER IF EXISTS trg_actualizar_fecha_edicion_pantalla ON "Tbl_Pantalla";
CREATE TRIGGER trg_actualizar_fecha_edicion_pantalla
    BEFORE UPDATE ON "Tbl_Pantalla"
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_fecha_edicion();

DROP TRIGGER IF EXISTS trg_actualizar_fecha_edicion_menu ON "Tbl_Menu";
CREATE TRIGGER trg_actualizar_fecha_edicion_menu
    BEFORE UPDATE ON "Tbl_Menu"
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_fecha_edicion();

DROP TRIGGER IF EXISTS trg_actualizar_fecha_edicion_menuperfil ON "Tbl_MenuPerfil";
CREATE TRIGGER trg_actualizar_fecha_edicion_menuperfil
    BEFORE UPDATE ON "Tbl_MenuPerfil"
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_fecha_edicion();

DROP TRIGGER IF EXISTS trg_actualizar_fecha_edicion_tipomensaje ON "Tbl_TipoMensaje";
CREATE TRIGGER trg_actualizar_fecha_edicion_tipomensaje
    BEFORE UPDATE ON "Tbl_TipoMensaje"
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_fecha_edicion();

DROP TRIGGER IF EXISTS trg_actualizar_fecha_edicion_mensaje ON "Tbl_Mensaje";
CREATE TRIGGER trg_actualizar_fecha_edicion_mensaje
    BEFORE UPDATE ON "Tbl_Mensaje"
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_fecha_edicion();

DROP TRIGGER IF EXISTS trg_actualizar_fecha_edicion_plantillacorreo ON "Tbl_PlantillaCorreo";
CREATE TRIGGER trg_actualizar_fecha_edicion_plantillacorreo
    BEFORE UPDATE ON "Tbl_PlantillaCorreo"
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_fecha_edicion();

-- =============================================
-- PROCEDIMIENTOS ALMACENADOS ÚTILES
-- =============================================

-- Función para desactivar un registro (soft delete)
CREATE OR REPLACE FUNCTION desactivar_registro(
    p_tabla TEXT,
    p_id INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    v_query TEXT;
    v_result BOOLEAN;
BEGIN
    -- Validar que la tabla existe
    IF p_tabla NOT IN ('Tbl_Pantalla', 'Tbl_Menu', 'Tbl_MenuPerfil',
                        'Tbl_TipoMensaje', 'Tbl_Mensaje', 'Tbl_PlantillaCorreo') THEN
        RAISE EXCEPTION 'Tabla no válida: %', p_tabla;
    END IF;

    -- Construir y ejecutar la consulta dinámica
    v_query := FORMAT('UPDATE %I SET activo = FALSE, "fechaEdicion" = NOW() WHERE id = %L RETURNING TRUE',
                      p_tabla, p_id);
    EXECUTE v_query INTO v_result;

    RETURN COALESCE(v_result, FALSE);
END;
$$ LANGUAGE plpgsql;

-- Función para reactivar un registro
CREATE OR REPLACE FUNCTION activar_registro(
    p_tabla TEXT,
    p_id INTEGER
)
RETURNS BOOLEAN AS $$
DECLARE
    v_query TEXT;
    v_result BOOLEAN;
BEGIN
    -- Validar que la tabla existe
    IF p_tabla NOT IN ('Tbl_Pantalla', 'Tbl_Menu', 'Tbl_MenuPerfil',
                        'Tbl_TipoMensaje', 'Tbl_Mensaje', 'Tbl_PlantillaCorreo') THEN
        RAISE EXCEPTION 'Tabla no válida: %', p_tabla;
    END IF;

    -- Construir y ejecutar la consulta dinámica
    v_query := FORMAT('UPDATE %I SET activo = TRUE, "fechaEdicion" = NOW() WHERE id = %L RETURNING TRUE',
                      p_tabla, p_id);
    EXECUTE v_query INTO v_result;

    RETURN COALESCE(v_result, FALSE);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- EJEMPLOS DE USO
-- =============================================

-- Desactivar un menú
-- SELECT desactivar_registro('Tbl_Menu', 5);

-- Reactivar un menú
-- SELECT activar_registro('Tbl_Menu', 5);

-- Ver menús de un perfil específico
-- SELECT m.*
-- FROM "Tbl_Menu" m
-- INNER JOIN "Tbl_MenuPerfil" mp ON m."id" = mp."idMenu"
-- WHERE mp."idPerfil" = 1 AND mp."activo" = TRUE AND m."activo" = TRUE;

-- Ver mensajes de una pantalla específica
-- SELECT m.*, tm."descripcion" AS tipo
-- FROM "Tbl_Mensaje" m
-- INNER JOIN "Tbl_TipoMensaje" tm ON m."idTipoMensaje" = tm."id"
-- WHERE m."idPantalla" = 2 AND m."activo" = TRUE;

-- =============================================
-- LIMPIEZA Y OPTIMIZACIÓN
-- =============================================

-- Vaciar y analizar tablas para optimizar rendimiento
VACUUM ANALYZE "Tbl_Pantalla";
VACUUM ANALYZE "Tbl_Menu";
VACUUM ANALYZE "Tbl_MenuPerfil";
VACUUM ANALYZE "Tbl_TipoMensaje";
VACUUM ANALYZE "Tbl_Mensaje";
VACUUM ANALYZE "Tbl_PlantillaCorreo";

-- Reindexar todas las tablas
REINDEX TABLE "Tbl_Pantalla";
REINDEX TABLE "Tbl_Menu";
REINDEX TABLE "Tbl_MenuPerfil";
REINDEX TABLE "Tbl_TipoMensaje";
REINDEX TABLE "Tbl_Mensaje";
REINDEX TABLE "Tbl_PlantillaCorreo";

-- =============================================
-- Fin del script de utilidades
-- =============================================
