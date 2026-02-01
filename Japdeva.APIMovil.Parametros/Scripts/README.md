# Scripts de Base de Datos - Módulo Parámetros

Este directorio contiene los scripts SQL para crear y configurar la base de datos PostgreSQL del módulo de Parámetros.

## Archivos Disponibles

### 1. CreateTablesParametros.sql
Script principal para la creación de todas las tablas del módulo de Parámetros.

**Contenido:**
- Creación de 6 tablas principales
- Definición de claves primarias (PRIMARY KEY)
- Definición de claves foráneas (FOREIGN KEY)
- Creación de índices para optimizar consultas
- Comentarios descriptivos en tablas y columnas

**Tablas creadas:**
- `Tbl_Pantalla` - Pantallas del sistema
- `Tbl_Menu` - Menús de navegación
- `Tbl_MenuPerfil` - Relación entre menús y perfiles
- `Tbl_TipoMensaje` - Tipos de mensajes
- `Tbl_Mensaje` - Mensajes del sistema
- `Tbl_PlantillaCorreo` - Plantillas de correo electrónico

### 2. SeedDataParametros.sql
Script de datos iniciales para poblar las tablas con información básica.

**Contenido:**
- Datos de ejemplo para todas las tablas
- Configuración inicial del sistema
- Plantilla de correo para nuevos reclamos

## Instrucciones de Uso

### Prerequisitos
- PostgreSQL 12 o superior instalado
- Cliente PostgreSQL (psql, pgAdmin, DBeaver, etc.)
- Permisos de creación de tablas en la base de datos

### Ejecución de Scripts

#### Opción 1: Usando psql (Terminal)

```bash
# 1. Conectarse a la base de datos
psql -U postgres -d nombre_base_datos

# 2. Ejecutar el script de creación de tablas
\i CreateTablesParametros.sql

# 3. Ejecutar el script de datos iniciales
\i SeedDataParametros.sql
```

#### Opción 2: Usando pgAdmin

1. Abrir pgAdmin y conectarse al servidor PostgreSQL
2. Seleccionar la base de datos destino
3. Hacer clic en Tools > Query Tool
4. Abrir el archivo `CreateTablesParametros.sql`
5. Ejecutar el script (F5 o botón Execute)
6. Repetir los pasos 3-5 con `SeedDataParametros.sql`

#### Opción 3: Usando línea de comandos

```bash
# Ejecutar directamente desde la terminal
psql -U postgres -d nombre_base_datos -f CreateTablesParametros.sql
psql -U postgres -d nombre_base_datos -f SeedDataParametros.sql
```

## Estructura de Tablas

### Tbl_Pantalla
Almacena las pantallas del sistema.

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | SERIAL | Identificador único |
| descripcion | VARCHAR(100) | Descripción de la pantalla |
| idUsuarioInterno | BIGINT | Usuario que registró |
| fechaRegistro | TIMESTAMP | Fecha de registro |
| fechaEdicion | TIMESTAMP | Fecha de última edición |
| activo | BOOLEAN | Estado activo/inactivo |

**Índices:**
- `idx_pantalla_activo` en (activo)
- `idx_pantalla_fechaRegistro` en (fechaRegistro)

### Tbl_Menu
Almacena los menús del sistema.

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | SERIAL | Identificador único |
| descripcion | VARCHAR(100) | Descripción del menú |
| mostrar | BOOLEAN | Si debe mostrarse |
| idUsuarioInterno | BIGINT | Usuario que registró |
| fechaRegistro | TIMESTAMP | Fecha de registro |
| fechaEdicion | TIMESTAMP | Fecha de última edición |
| activo | BOOLEAN | Estado activo/inactivo |

**Índices:**
- `idx_menu_activo` en (activo)
- `idx_menu_mostrar` en (mostrar)
- `idx_menu_fechaRegistro` en (fechaRegistro)

### Tbl_MenuPerfil
Relaciona menús con perfiles de usuario.

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | SERIAL | Identificador único |
| idMenu | INTEGER | Referencia a Tbl_Menu |
| idPerfil | INTEGER | Identificador del perfil |
| idUsuarioInterno | BIGINT | Usuario que registró |
| fechaRegistro | TIMESTAMP | Fecha de registro |
| fechaEdicion | TIMESTAMP | Fecha de última edición |
| activo | BOOLEAN | Estado activo/inactivo |

**Índices:**
- `idx_menuperfil_idMenu` en (idMenu)
- `idx_menuperfil_idPerfil` en (idPerfil)
- `idx_menuperfil_activo` en (activo)
- `idx_menuperfil_idMenu_idPerfil` en (idMenu, idPerfil)

**Relaciones:**
- FK: idMenu → Tbl_Menu(id) ON DELETE CASCADE

### Tbl_TipoMensaje
Almacena los tipos de mensajes del sistema.

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | SERIAL | Identificador único |
| descripcion | VARCHAR(100) | Descripción del tipo |
| idUsuarioInterno | BIGINT | Usuario que registró |
| fechaRegistro | TIMESTAMP | Fecha de registro |
| fechaEdicion | TIMESTAMP | Fecha de última edición |
| activo | BOOLEAN | Estado activo/inactivo |

**Índices:**
- `idx_tipomensaje_activo` en (activo)
- `idx_tipomensaje_fechaRegistro` en (fechaRegistro)

### Tbl_Mensaje
Almacena los mensajes del sistema asociados a pantallas.

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | SERIAL | Identificador único |
| descripcion | VARCHAR(300) | Contenido del mensaje |
| idTipoMensaje | INTEGER | Referencia a Tbl_TipoMensaje |
| idPantalla | INTEGER | Referencia a Tbl_Pantalla |
| idUsuarioInterno | BIGINT | Usuario que registró |
| fechaRegistro | TIMESTAMP | Fecha de registro |
| fechaEdicion | TIMESTAMP | Fecha de última edición |
| activo | BOOLEAN | Estado activo/inactivo |

**Índices:**
- `idx_mensaje_idTipoMensaje` en (idTipoMensaje)
- `idx_mensaje_idPantalla` en (idPantalla)
- `idx_mensaje_activo` en (activo)
- `idx_mensaje_idPantalla_activo` en (idPantalla, activo)

**Relaciones:**
- FK: idTipoMensaje → Tbl_TipoMensaje(id) ON DELETE RESTRICT
- FK: idPantalla → Tbl_Pantalla(id) ON DELETE RESTRICT

### Tbl_PlantillaCorreo
Almacena plantillas de correo electrónico para notificaciones.

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | SERIAL | Identificador único |
| Plantilla | TEXT | Contenido HTML/texto |
| idUsuarioInterno | BIGINT | Usuario que registró |
| fechaRegistro | TIMESTAMP | Fecha de registro |
| fechaEdicion | TIMESTAMP | Fecha de última edición |
| activo | BOOLEAN | Estado activo/inactivo |

**Índices:**
- `idx_plantillacorreo_activo` en (activo)
- `idx_plantillacorreo_fechaRegistro` en (fechaRegistro)

**Marcadores disponibles:**
- `@idReclamo` - Se reemplaza con el ID del reclamo

## Consideraciones de Seguridad

1. **Claves Foráneas:**
   - `ON DELETE CASCADE`: Se usa en MenuPerfil para eliminar relaciones automáticamente
   - `ON DELETE RESTRICT`: Se usa en Mensaje para evitar eliminaciones accidentales

2. **Índices:**
   - Se han creado índices en campos frecuentemente consultados
   - Los índices compuestos optimizan consultas con múltiples filtros

3. **Valores por Defecto:**
   - `fechaRegistro` tiene DEFAULT NOW()
   - `activo` tiene DEFAULT TRUE
   - Los IDs son SERIAL (auto-incrementales)

## Mantenimiento

### Verificar tablas creadas
```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'Tbl_%'
ORDER BY table_name;
```

### Verificar índices
```sql
SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename LIKE 'Tbl_%'
ORDER BY tablename, indexname;
```

### Verificar claves foráneas
```sql
SELECT
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
  ON tc.constraint_name = kcu.constraint_name
  AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
  ON ccu.constraint_name = tc.constraint_name
  AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
  AND tc.table_name LIKE 'Tbl_%';
```

## Notas Adicionales

- Los scripts están diseñados para ser idempotentes (pueden ejecutarse múltiples veces)
- Se usa `IF NOT EXISTS` para evitar errores al re-ejecutar scripts
- El script de datos usa `ON CONFLICT DO NOTHING` para evitar duplicados
- Los comentarios en las tablas facilitan la documentación automática

## Contacto y Soporte

Para reportar problemas o sugerencias sobre los scripts de base de datos, contactar al equipo de desarrollo.
