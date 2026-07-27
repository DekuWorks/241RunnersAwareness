-- Public-safe read views for anonymous map / cases listing (no PII beyond approved public fields)

CREATE OR REPLACE VIEW v_public_cases AS
SELECT
    c."Id",
    c."RunnerId",
    c."Title",
    c."Description",
    c."LastSeenDate",
    c."LastSeenTime",
    c."LastSeenLocation",
    c."LastSeenLatitude",
    c."LastSeenLongitude",
    c."Status",
    c."Priority",
    c."ClothingDescription",
    c."CaseImageUrls",
    c."ViewCount",
    c."ShareCount",
    c."TipCount",
    c."CreatedAt",
    c."UpdatedAt"
FROM "Cases" c
WHERE c."IsPublic" = TRUE
  AND c."IsApproved" = TRUE
  AND c."Status" IN ('Missing', 'Found');

CREATE OR REPLACE VIEW v_public_runners_map AS
SELECT
    r."Id",
    r."Name",
    r."Status",
    r."ShowOnMap",
    r."MapLatitude",
    r."MapLongitude",
    r."ProfileImageUrl",
    r."PhysicalDescription",
    r."Gender",
    r."DateOfBirth"
FROM "Runners" r
WHERE r."ShowOnMap" = TRUE
  AND r."IsActive" = TRUE
  AND r."MapLatitude" IS NOT NULL
  AND r."MapLongitude" IS NOT NULL;

GRANT SELECT ON v_public_cases TO anon, authenticated;
GRANT SELECT ON v_public_runners_map TO anon, authenticated;

COMMENT ON VIEW v_public_cases IS 'Approved public missing-person cases; no contact PII.';
COMMENT ON VIEW v_public_runners_map IS 'Runners opted in to public map display.';
