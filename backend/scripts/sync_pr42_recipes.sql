BEGIN;
SET LOCAL lock_timeout = '5s';
LOCK TABLE "Categories", "Recipes", "RecipeSteps" IN SHARE ROW EXCLUSIVE MODE;

CREATE TEMP TABLE pr42_plan ON COMMIT DROP AS
SELECT item->>'slug' AS slug, (item->>'servings')::integer AS servings,
       item->'before' AS before, item->'after' AS after
FROM jsonb_array_elements(convert_from(decode(:'payload', 'hex'), 'UTF8')::jsonb) item;

CREATE TEMP TABLE pr42_targets ON COMMIT DROP AS
SELECT DISTINCT after->>'category' AS name, after->>'category_slug' AS slug
FROM pr42_plan;

-- Compare the current value for each field to the reviewed baseline and target.
-- A third value may be an author edit and must be reviewed before any changes are applied.
CREATE TEMP TABLE pr42_current ON COMMIT DROP AS
SELECT p.slug, r."Id" AS id, p.before, p.after, r."Servings" AS servings, p.servings AS expected_servings,
       jsonb_build_object(
           'category', c."Name",
           'description', r."Description",
           'nutrition', jsonb_build_array(r."Nutrition_Calories", r."Nutrition_Protein",
               r."Nutrition_Carbohydrates", r."Nutrition_Fat", r."Nutrition_Fiber", r."Nutrition_Sodium"),
           'steps', COALESCE((SELECT jsonb_agg(jsonb_build_array(s."Title", s."Description", s."DurationMinutes")
                               ORDER BY s."StepNumber")
                              FROM "RecipeSteps" s WHERE s."RecipeId" = r."Id" AND NOT s."IsDeleted"), '[]'::jsonb)
       ) AS actual
FROM pr42_plan p
LEFT JOIN "Recipes" r ON r."Slug" = p.slug AND NOT r."IsDeleted"
LEFT JOIN "Categories" c ON c."Id" = r."CategoryId";

DO $check$
DECLARE
    conflict text;
BEGIN
    SELECT string_agg(slug, ', ' ORDER BY slug) INTO conflict
    FROM pr42_current WHERE id IS NULL;
    IF conflict IS NOT NULL THEN
        RAISE EXCEPTION 'Recipes missing or in trash: %', conflict;
    END IF;

    SELECT string_agg(c.slug || '/' || f.field, ', ' ORDER BY c.slug, f.field) INTO conflict
    FROM pr42_current c
    CROSS JOIN (VALUES ('category'), ('description'), ('nutrition'), ('steps')) f(field)
    WHERE c.before->f.field IS DISTINCT FROM c.after->f.field
      AND c.actual->f.field IS DISTINCT FROM c.before->f.field
      AND c.actual->f.field IS DISTINCT FROM c.after->f.field;
    IF conflict IS NOT NULL THEN
        RAISE EXCEPTION 'Live fields differ from both baseline and target: %', conflict;
    END IF;

    SELECT string_agg(slug, ', ' ORDER BY slug) INTO conflict
    FROM pr42_current
    WHERE before->'nutrition' IS DISTINCT FROM after->'nutrition' AND servings <> expected_servings;
    IF conflict IS NOT NULL THEN
        RAISE EXCEPTION 'Servings changed on recipes with new nutrition: %', conflict;
    END IF;

    IF EXISTS (SELECT 1 FROM pr42_targets GROUP BY slug HAVING count(*) > 1)
       OR EXISTS (SELECT 1 FROM pr42_targets t JOIN "Categories" c
                  ON NOT c."IsDeleted" AND (c."Name" = t.name OR c."Slug" = t.slug)
                  WHERE c."Name" <> t.name OR c."Slug" <> t.slug) THEN
        RAISE EXCEPTION 'Category name or slug collision';
    END IF;

    IF EXISTS (
        SELECT 1 FROM (
            SELECT s."StepNumber", row_number() OVER (PARTITION BY c.id ORDER BY s."StepNumber") AS expected
            FROM pr42_current c JOIN "RecipeSteps" s ON s."RecipeId" = c.id
            WHERE c.before->'steps' IS DISTINCT FROM c.after->'steps' AND NOT s."IsDeleted"
        ) ordered_steps
        WHERE ordered_steps."StepNumber" <> ordered_steps.expected
    ) THEN
        RAISE EXCEPTION 'A recipe to update has non-contiguous step numbers';
    END IF;

    IF EXISTS (
        SELECT 1 FROM pr42_current c JOIN "RecipeSteps" s ON s."RecipeId" = c.id
        WHERE c.before->'steps' IS DISTINCT FROM c.after->'steps' AND NOT s."IsDeleted"
          AND (s."ParentStepId" IS NOT NULL
               OR (s."StepNumber" > jsonb_array_length(c.after->'steps') AND
                   (s."ImageUrl" IS NOT NULL OR EXISTS (
                       SELECT 1 FROM "RecipeSteps" child WHERE child."ParentStepId" = s."Id"
                   ))))
    ) THEN
        RAISE EXCEPTION 'A changed step has a parent, or a step to retire has an image or child';
    END IF;
END
$check$;

WITH inserted AS (
    INSERT INTO "Categories" ("Id", "Name", "Slug", "OrderIndex", "CreatedAt", "IsDeleted")
    SELECT gen_random_uuid(), t.name, t.slug,
           COALESCE((SELECT max("OrderIndex") FROM "Categories"), 0) + row_number() OVER (ORDER BY t.slug),
           now(), false
    FROM pr42_targets t
    WHERE NOT EXISTS (SELECT 1 FROM "Categories" c WHERE NOT c."IsDeleted" AND c."Slug" = t.slug)
    RETURNING 1
)
SELECT 'categories created' AS operation, count(*) FROM inserted;

CREATE TEMP TABLE pr42_changes ON COMMIT DROP AS
SELECT c.*, cat."Id" AS category_id,
       c.before->'category' IS DISTINCT FROM c.after->'category' AND c.actual->'category' IS DISTINCT FROM c.after->'category' AS change_category,
       c.before->'description' IS DISTINCT FROM c.after->'description' AND c.actual->'description' IS DISTINCT FROM c.after->'description' AS change_description,
       c.before->'nutrition' IS DISTINCT FROM c.after->'nutrition' AND c.actual->'nutrition' IS DISTINCT FROM c.after->'nutrition' AS change_nutrition,
       c.before->'steps' IS DISTINCT FROM c.after->'steps' AND c.actual->'steps' IS DISTINCT FROM c.after->'steps' AS change_steps
FROM pr42_current c JOIN "Categories" cat ON NOT cat."IsDeleted" AND cat."Slug" = c.after->>'category_slug';

SELECT 'recipes to update' AS operation, count(*) FROM pr42_changes
WHERE change_category OR change_description OR change_nutrition OR change_steps;

UPDATE "Recipes" r
SET "CategoryId" = CASE WHEN c.change_category THEN c.category_id ELSE r."CategoryId" END,
    "Description" = CASE WHEN c.change_description THEN c.after->>'description' ELSE r."Description" END,
    "Nutrition_Calories" = CASE WHEN c.change_nutrition THEN (c.after->'nutrition'->>0)::numeric ELSE r."Nutrition_Calories" END,
    "Nutrition_Protein" = CASE WHEN c.change_nutrition THEN (c.after->'nutrition'->>1)::numeric ELSE r."Nutrition_Protein" END,
    "Nutrition_Carbohydrates" = CASE WHEN c.change_nutrition THEN (c.after->'nutrition'->>2)::numeric ELSE r."Nutrition_Carbohydrates" END,
    "Nutrition_Fat" = CASE WHEN c.change_nutrition THEN (c.after->'nutrition'->>3)::numeric ELSE r."Nutrition_Fat" END,
    "Nutrition_Fiber" = CASE WHEN c.change_nutrition THEN (c.after->'nutrition'->>4)::numeric ELSE r."Nutrition_Fiber" END,
    "Nutrition_Sodium" = CASE WHEN c.change_nutrition THEN (c.after->'nutrition'->>5)::numeric ELSE r."Nutrition_Sodium" END,
    "Nutrition_Source" = CASE WHEN c.change_nutrition THEN 0 ELSE r."Nutrition_Source" END,
    "UpdatedAt" = now()
FROM pr42_changes c
WHERE r."Id" = c.id AND (c.change_category OR c.change_description OR c.change_nutrition OR c.change_steps);

CREATE TEMP TABLE pr42_steps ON COMMIT DROP AS
SELECT c.id, s.number::integer AS number, s.content
FROM pr42_changes c, LATERAL jsonb_array_elements(c.after->'steps') WITH ORDINALITY s(content, number)
WHERE c.change_steps;

UPDATE "RecipeSteps" step
SET "Title" = s.content->>0, "Description" = s.content->>1,
    "DurationMinutes" = (s.content->>2)::integer, "UpdatedAt" = now()
FROM pr42_steps s
WHERE step."RecipeId" = s.id AND step."StepNumber" = s.number AND NOT step."IsDeleted"
  AND jsonb_build_array(step."Title", step."Description", step."DurationMinutes") IS DISTINCT FROM s.content;

INSERT INTO "RecipeSteps" ("Id", "RecipeId", "StepNumber", "Title", "Description", "DurationMinutes", "CreatedAt", "IsDeleted")
SELECT gen_random_uuid(), s.id, s.number, s.content->>0, s.content->>1,
       (s.content->>2)::integer, now(), false
FROM pr42_steps s
WHERE NOT EXISTS (SELECT 1 FROM "RecipeSteps" step
                  WHERE step."RecipeId" = s.id AND step."StepNumber" = s.number AND NOT step."IsDeleted");

UPDATE "RecipeSteps" step
SET "IsDeleted" = true, "DeletedAt" = now(), "UpdatedAt" = now()
FROM pr42_changes c
WHERE step."RecipeId" = c.id AND c.change_steps AND NOT step."IsDeleted"
  AND step."StepNumber" > jsonb_array_length(c.after->'steps');

-- Keep every old category that is still referenced, including by trashed recipes.
WITH retired AS (
    UPDATE "Categories" c
    SET "IsDeleted" = true, "DeletedAt" = now(), "UpdatedAt" = now()
    WHERE NOT c."IsDeleted"
      AND EXISTS (SELECT 1 FROM pr42_plan p WHERE p.before->>'category_slug' = c."Slug")
      AND NOT EXISTS (SELECT 1 FROM pr42_targets t WHERE t.slug = c."Slug")
      AND NOT EXISTS (SELECT 1 FROM "Recipes" r WHERE r."CategoryId" = c."Id")
    RETURNING 1
)
SELECT 'empty old categories retired' AS operation, count(*) FROM retired;

SELECT 'old categories retained with references' AS operation, c."Slug", count(*)
FROM "Categories" c JOIN "Recipes" r ON r."CategoryId" = c."Id"
WHERE NOT c."IsDeleted"
  AND EXISTS (SELECT 1 FROM pr42_plan p WHERE p.before->>'category_slug' = c."Slug")
  AND NOT EXISTS (SELECT 1 FROM pr42_targets t WHERE t.slug = c."Slug")
GROUP BY c."Slug" ORDER BY c."Slug";
