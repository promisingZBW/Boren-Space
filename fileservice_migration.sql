CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003737_InitialCreate') THEN
    CREATE TABLE "UploadedItems" (
        "Id" uuid NOT NULL,
        "FileName" character varying(255) NOT NULL,
        "FileSizeInBytes" bigint NOT NULL,
        "FileSHA256Hash" character varying(64) NOT NULL,
        "FileType" integer NOT NULL,
        "ContentType" character varying(100) NOT NULL,
        "UploadTime" timestamp without time zone NOT NULL,
        "UploaderId" uuid NOT NULL,
        "BackupUrl" character varying(1000),
        "RemoteUrl" character varying(1000),
        "StorageKey" character varying(500) NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_UploadedItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003737_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_UploadedItems_FileSize_Hash" ON "UploadedItems" ("FileSizeInBytes", "FileSHA256Hash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003737_InitialCreate') THEN
    CREATE INDEX "IX_UploadedItems_IsDeleted" ON "UploadedItems" ("IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003737_InitialCreate') THEN
    CREATE INDEX "IX_UploadedItems_UploaderId" ON "UploadedItems" ("UploaderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003737_InitialCreate') THEN
    CREATE INDEX "IX_UploadedItems_UploadTime" ON "UploadedItems" ("UploadTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003737_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260116003737_InitialCreate', '8.0.0');
    END IF;
END $EF$;
COMMIT;

