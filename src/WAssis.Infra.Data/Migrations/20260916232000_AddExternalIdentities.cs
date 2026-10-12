using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20260916232000_AddExternalIdentities")]
public sealed class AddExternalIdentities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE security.external_identities (
          issuer text NOT NULL,
          subject text NOT NULL,
          profile_id uuid NOT NULL REFERENCES erp.profiles(id) ON DELETE RESTRICT,
          created_at timestamptz NOT NULL DEFAULT now(),
          CONSTRAINT pk_external_identities PRIMARY KEY (issuer, subject),
          CONSTRAINT ck_external_identities_issuer CHECK (issuer = btrim(issuer) AND issuer <> '' AND right(issuer, 1) <> '/'),
          CONSTRAINT ck_external_identities_subject CHECK (subject = btrim(subject) AND subject <> '')
        );
        CREATE INDEX ix_external_identities_profile ON security.external_identities(profile_id);
        CREATE OR REPLACE FUNCTION security.resolve_external_profile(p_issuer text, p_subject text)
        RETURNS uuid
        LANGUAGE sql
        STABLE
        SECURITY DEFINER
        SET search_path = pg_catalog
        AS $function$
          SELECT external_identity.profile_id
          FROM security.external_identities external_identity
          WHERE external_identity.issuer = p_issuer AND external_identity.subject = p_subject
        $function$;
        REVOKE ALL ON FUNCTION security.resolve_external_profile(text, text) FROM PUBLIC;
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP FUNCTION IF EXISTS security.resolve_external_profile(text, text);
        DROP TABLE IF EXISTS security.external_identities;
        """);
}
