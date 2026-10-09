using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

public sealed partial class AddAutoQuoteSubmission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE quotes.quote_options
          ADD COLUMN "ProviderCode" text NOT NULL DEFAULT '',
          ADD COLUMN "ProviderName" text NOT NULL DEFAULT '';
        CREATE TABLE quotes.quote_auto_submissions (
          "QuoteRequestId" uuid PRIMARY KEY REFERENCES quotes.quote_requests("Id") ON DELETE CASCADE,
          "DriverName" varchar(200) NOT NULL,
          "DriverDocument" varchar(32) NOT NULL,
          "DriverBirthDate" date NOT NULL,
          "DriverGender" varchar(1) NOT NULL,
          "DriverMaritalStatus" varchar(64) NOT NULL,
          "DriverLicenseYears" integer NOT NULL CHECK ("DriverLicenseYears" BETWEEN 0 AND 80),
          "InsuranceType" varchar(32) NOT NULL CHECK ("InsuranceType" IN ('NOVO','RENOVACAO_PROPRIA','RENOVACAO_OUTRA')),
          "CoverageStartsOn" date NOT NULL,
          "CoverageEndsOn" date NOT NULL CHECK ("CoverageEndsOn" > "CoverageStartsOn"),
          "PreviousCoverageEndsOn" date,
          "PreviousPolicyNumber" varchar(120),
          "PreviousIdentificationCode" varchar(120),
          "BonusClass" integer CHECK ("BonusClass" BETWEEN 0 AND 10),
          "ClaimCount" integer CHECK ("ClaimCount" >= 0)
        );
        CREATE TABLE quotes.quote_requested_coverages (
          "QuoteRequestId" uuid NOT NULL REFERENCES quotes.quote_auto_submissions("QuoteRequestId") ON DELETE CASCADE,
          "Code" varchar(80) NOT NULL,
          "Selected" boolean NOT NULL,
          "RequestedLimit" numeric CHECK ("RequestedLimit" >= 0),
          "RequestedFipePercentage" numeric CHECK ("RequestedFipePercentage" BETWEEN 0 AND 200),
          "RequestedDeductibleType" varchar(80),
          "RequestedOption" varchar(80),
          "RequestedQuantity" integer CHECK ("RequestedQuantity" >= 0),
          PRIMARY KEY ("QuoteRequestId", "Code")
        );
        ALTER TABLE quotes.quote_auto_submissions ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_auto_submissions FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_auto_submissions
          USING (EXISTS (SELECT 1 FROM quotes.quote_requests q WHERE q."Id" = "QuoteRequestId"))
          WITH CHECK (EXISTS (SELECT 1 FROM quotes.quote_requests q WHERE q."Id" = "QuoteRequestId"));
        ALTER TABLE quotes.quote_requested_coverages ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_requested_coverages FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_requested_coverages
          USING (EXISTS (SELECT 1 FROM quotes.quote_requests q WHERE q."Id" = "QuoteRequestId"))
          WITH CHECK (EXISTS (SELECT 1 FROM quotes.quote_requests q WHERE q."Id" = "QuoteRequestId"));
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE quotes.quote_requested_coverages;
        DROP TABLE quotes.quote_auto_submissions;
        ALTER TABLE quotes.quote_options DROP COLUMN "ProviderCode", DROP COLUMN "ProviderName";
        """);
}
