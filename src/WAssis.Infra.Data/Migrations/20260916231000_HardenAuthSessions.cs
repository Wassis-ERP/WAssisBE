using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20260916231000_HardenAuthSessions")]
public sealed class HardenAuthSessions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE security.auth_sessions ADD COLUMN IF NOT EXISTS last_seen_at timestamptz;
        UPDATE security.auth_sessions SET last_seen_at = created_at WHERE last_seen_at IS NULL;
        ALTER TABLE security.auth_sessions ALTER COLUMN last_seen_at SET DEFAULT now();
        CREATE INDEX IF NOT EXISTS ix_auth_sessions_active_expiry
          ON security.auth_sessions (expires_at, last_seen_at) WHERE revoked_at IS NULL;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP INDEX IF EXISTS security.ix_auth_sessions_active_expiry;
        ALTER TABLE security.auth_sessions DROP COLUMN IF EXISTS last_seen_at;
        """);
}
