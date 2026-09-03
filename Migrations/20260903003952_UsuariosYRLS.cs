using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ManejoTareas.Migrations
{
    /// <inheritdoc />
    public partial class UsuariosYRLS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "usuario_id",
                table: "tareas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "permisos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    grupo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol_permisos",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    permiso_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_permisos", x => new { x.rol_id, x.permiso_id });
                    table.ForeignKey(
                        name: "FK_rol_permisos_permisos_permiso_id",
                        column: x => x.permiso_id,
                        principalTable: "permisos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rol_permisos_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    asignado_por = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => new { x.usuario_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_usuario_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "clave", "descripcion", "grupo" },
                values: new object[,]
                {
                    { 1, "tareas.ver", "Ver tareas", "tareas" },
                    { 2, "tareas.crear", "Crear tareas", "tareas" },
                    { 3, "tareas.editar", "Editar tareas", "tareas" },
                    { 4, "tareas.eliminar", "Eliminar tareas", "tareas" },
                    { 5, "tareas.completar", "Marcar tareas como completadas", "tareas" },
                    { 6, "usuarios.ver", "Ver usuarios", "usuarios" },
                    { 7, "usuarios.crear", "Crear usuarios", "usuarios" },
                    { 8, "usuarios.editar", "Editar usuarios", "usuarios" },
                    { 9, "usuarios.eliminar", "Eliminar usuarios", "usuarios" },
                    { 10, "usuarios.gestionar_permisos", "Gestionar permisos/roles", "usuarios" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Acceso total, gestiona usuarios y permisos", "Administrador" },
                    { 2, "Puede crear y editar tareas, no eliminar", "Editor" },
                    { 3, "Solo puede ver tareas", "Lector" }
                });

            migrationBuilder.InsertData(
                table: "rol_permisos",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 5, 2 },
                    { 1, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tareas_usuario_id",
                table: "tareas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_permisos_clave",
                table: "permisos",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_permisos_permiso_id",
                table: "rol_permisos",
                column: "permiso_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_rol_id",
                table: "usuario_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tareas_usuarios_usuario_id",
                table: "tareas",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // ==================== RLS (Row Level Security) ====================
            // Nota: El usuario 'postgres' es superusuario y SIEMPRE bypassa RLS en PostgreSQL.
            // Para que RLS sea efectivo en produccion, crear un rol sin BYPASSRLS y usarlo en la cadena de conexion:
            //   CREATE ROLE app_user WITH LOGIN PASSWORD 'xxx' NOBYPASSRLS;
            //   GRANT ALL ON ALL TABLES IN SCHEMA public TO app_user;
            // Aqui se deja FORCE RLS + politicas demostrativas. La seguridad real se aplica tambien a nivel app via RequierePermisoAttribute.
            migrationBuilder.Sql(@"
                -- Funciones auxiliares para RLS usando variables de sesion app.current_user_id / app.is_admin
                CREATE OR REPLACE FUNCTION app_current_user_id() RETURNS INT AS $$
                DECLARE uid TEXT := current_setting('app.current_user_id', true);
                BEGIN
                    IF uid IS NULL OR uid = '' THEN RETURN NULL; END IF;
                    RETURN uid::INT;
                EXCEPTION WHEN OTHERS THEN RETURN NULL;
                END; $$ LANGUAGE plpgsql STABLE;

                CREATE OR REPLACE FUNCTION app_is_admin() RETURNS BOOLEAN AS $$
                DECLARE v TEXT := current_setting('app.is_admin', true);
                BEGIN
                    RETURN v = '1';
                EXCEPTION WHEN OTHERS THEN RETURN FALSE;
                END; $$ LANGUAGE plpgsql STABLE;

                CREATE OR REPLACE FUNCTION tiene_permiso(p_clave TEXT) RETURNS BOOLEAN AS $$
                DECLARE uid INT := app_current_user_id();
                BEGIN
                    IF app_is_admin() THEN RETURN TRUE; END IF;
                    IF uid IS NULL THEN RETURN FALSE; END IF;
                    RETURN EXISTS (
                        SELECT 1 FROM usuario_roles ur
                        JOIN rol_permisos rp ON rp.rol_id = ur.rol_id
                        JOIN permisos p ON p.id = rp.permiso_id
                        WHERE ur.usuario_id = uid AND p.clave = p_clave
                    );
                END; $$ LANGUAGE plpgsql STABLE SECURITY DEFINER;

                -- Habilitar RLS en usuarios y tareas (tablas criticas)
                ALTER TABLE usuarios ENABLE ROW LEVEL SECURITY;
                ALTER TABLE usuarios FORCE ROW LEVEL SECURITY;
                ALTER TABLE tareas ENABLE ROW LEVEL SECURITY;
                ALTER TABLE tareas FORCE ROW LEVEL SECURITY;

                -- Limpiar politicas previas si existen (idempotente)
                DROP POLICY IF EXISTS usuarios_select ON usuarios;
                DROP POLICY IF EXISTS usuarios_insert ON usuarios;
                DROP POLICY IF EXISTS usuarios_update ON usuarios;
                DROP POLICY IF EXISTS usuarios_delete ON usuarios;
                DROP POLICY IF EXISTS tareas_select ON tareas;
                DROP POLICY IF EXISTS tareas_insert ON tareas;
                DROP POLICY IF EXISTS tareas_update ON tareas;
                DROP POLICY IF EXISTS tareas_delete ON tareas;

                -- Politicas usuarios: solo admin ve todos, usuario normal solo su fila
                CREATE POLICY usuarios_select ON usuarios FOR SELECT USING (app_is_admin() OR id = app_current_user_id());
                -- Permitir insert si es admin o si la tabla esta vacia (bootstrap primer admin)
                CREATE POLICY usuarios_insert ON usuarios FOR INSERT WITH CHECK (app_is_admin() OR (SELECT COUNT(*) FROM usuarios) = 0);
                CREATE POLICY usuarios_update ON usuarios FOR UPDATE USING (app_is_admin() OR id = app_current_user_id()) WITH CHECK (app_is_admin() OR id = app_current_user_id());
                CREATE POLICY usuarios_delete ON usuarios FOR DELETE USING (app_is_admin());

                -- Politicas tareas: control por permisos granulares
                CREATE POLICY tareas_select ON tareas FOR SELECT USING (tiene_permiso('tareas.ver') OR app_is_admin());
                CREATE POLICY tareas_insert ON tareas FOR INSERT WITH CHECK (tiene_permiso('tareas.crear') OR app_is_admin());
                CREATE POLICY tareas_update ON tareas FOR UPDATE USING (tiene_permiso('tareas.editar') OR tiene_permiso('tareas.completar') OR app_is_admin()) WITH CHECK (tiene_permiso('tareas.editar') OR tiene_permiso('tareas.completar') OR app_is_admin());
                CREATE POLICY tareas_delete ON tareas FOR DELETE USING (tiene_permiso('tareas.eliminar') OR app_is_admin());

                -- Opcional: crear rol sin bypass para produccion
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'app_user') THEN
                        CREATE ROLE app_user WITH LOGIN PASSWORD 'AppUser123!' NOBYPASSRLS;
                        GRANT CONNECT ON DATABASE manejotareas TO app_user;
                        GRANT USAGE ON SCHEMA public TO app_user;
                        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
                        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_user;
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app_user;
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO app_user;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP POLICY IF EXISTS usuarios_select ON usuarios;
                DROP POLICY IF EXISTS usuarios_insert ON usuarios;
                DROP POLICY IF EXISTS usuarios_update ON usuarios;
                DROP POLICY IF EXISTS usuarios_delete ON usuarios;
                DROP POLICY IF EXISTS tareas_select ON tareas;
                DROP POLICY IF EXISTS tareas_insert ON tareas;
                DROP POLICY IF EXISTS tareas_update ON tareas;
                DROP POLICY IF EXISTS tareas_delete ON tareas;
                ALTER TABLE usuarios DISABLE ROW LEVEL SECURITY;
                ALTER TABLE tareas DISABLE ROW LEVEL SECURITY;
                DROP FUNCTION IF EXISTS tiene_permiso(TEXT);
                DROP FUNCTION IF EXISTS app_is_admin();
                DROP FUNCTION IF EXISTS app_current_user_id();
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_tareas_usuarios_usuario_id",
                table: "tareas");

            migrationBuilder.DropTable(
                name: "rol_permisos");

            migrationBuilder.DropTable(
                name: "usuario_roles");

            migrationBuilder.DropTable(
                name: "permisos");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_tareas_usuario_id",
                table: "tareas");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "tareas");
        }
    }
}
