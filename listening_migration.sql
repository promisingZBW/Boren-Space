CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003923_InitialCreate') THEN
    CREATE TABLE "Episodes" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "AudioUrl" text,
        "CoverImageUrl" text,
        "SubtitleUrl" text,
        "Description" text,
        "Duration" integer NOT NULL,
        "CreateTime" timestamp without time zone NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Episodes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003923_InitialCreate') THEN
    CREATE TABLE "Sentences" (
        "Id" uuid NOT NULL,
        "Content" character varying(1000) NOT NULL,
        "StartTime" interval NOT NULL,
        "EndTime" interval NOT NULL,
        "EpisodeId" uuid NOT NULL,
        CONSTRAINT "PK_Sentences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Sentences_Episodes_EpisodeId" FOREIGN KEY ("EpisodeId") REFERENCES "Episodes" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003923_InitialCreate') THEN
    CREATE INDEX "IX_Sentences_EpisodeId" ON "Sentences" ("EpisodeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260116003923_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260116003923_InitialCreate', '8.0.0');
    END IF;
END $EF$;
COMMIT;

