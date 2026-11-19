import argparse
import json
from pathlib import Path
import re
import subprocess
import unicodedata


ROOT = Path(__file__).resolve().parents[2]
RECIPES = Path("backend/data/recipes")
NUTRIENTS = ("calories", "protein", "carbohydrates", "fat", "fiber", "sodium")


def slug(value):
    decomposed = unicodedata.normalize("NFD", value.lower().replace("đ", "d"))
    letters = "".join(char for char in decomposed if unicodedata.category(char) != "Mn")
    cleaned = re.sub(r"[^a-z0-9\s-]", "", letters)
    return re.sub(r"\s+", "-", cleaned).strip("-")[:150].rstrip("-")


def fields(recipe):
    nutrition = recipe.get("nutrition") or {}
    return {
        "category": recipe["categoryName"].strip(),
        "category_slug": slug(recipe["categoryName"]),
        "description": recipe["description"].strip(),
        "nutrition": [nutrition.get(key) for key in NUTRIENTS],
        "steps": [
            [step["title"].strip(), step["description"].strip(), step.get("durationMinutes")]
            for step in recipe["steps"]
        ],
    }


def load_plan(baseline):
    plan = []
    slugs = set()
    for path in sorted((ROOT / RECIPES).glob("*.json")):
        repo_path = (RECIPES / path.name).as_posix()
        old = json.loads(subprocess.check_output(
            ["git", "show", f"{baseline}:{repo_path}"], cwd=ROOT
        ))
        new = json.loads(path.read_text(encoding="utf-8-sig"))
        recipe_slug = slug(new["title"])
        if not recipe_slug or recipe_slug != slug(old["title"]) or recipe_slug in slugs:
            raise SystemExit(f"Missing, changed or duplicate recipe slug: {path.name}")
        slugs.add(recipe_slug)

        old_fields, new_fields = fields(old), fields(new)
        supported = {"categoryName", "description", "nutrition", "steps", "source"}
        unchanged_keys = (old.keys() | new.keys()) - supported
        if any(old.get(key) != new.get(key) for key in unchanged_keys):
            raise SystemExit(f"Unexpected change to a protected field: {path.name}")
        if any(step.get("stepNumber", index) != index
               for index, step in enumerate(new["steps"], 1)):
            raise SystemExit(f"Step numbers are not in array order: {path.name}")
        plan.append({"slug": recipe_slug, "servings": new["servings"],
                     "before": old_fields, "after": new_fields})

    if len(plan) != 42:
        raise SystemExit(f"Expected 42 recipes for PR #42, found {len(plan)}")
    return plan


def main():
    parser = argparse.ArgumentParser(description="Preview or apply the PR #42 recipe data sync.")
    parser.add_argument("--baseline", required=True, help="Full SHA of develop before PR #42.")
    parser.add_argument("--container", required=True, help="Docker container running PostgreSQL.")
    parser.add_argument("--database", required=True)
    parser.add_argument("--user", required=True)
    parser.add_argument("--apply", action="store_true", help="Commit the transaction. Default: roll back.")
    args = parser.parse_args()
    if not re.fullmatch(r"[a-fA-F0-9]{40}", args.baseline):
        parser.error("--baseline must be a full commit SHA")
    if args.container.startswith("-"):
        parser.error("--container must be a container name or ID")

    plan_json = json.dumps(load_plan(args.baseline), ensure_ascii=False, allow_nan=False)
    payload = plan_json.encode("utf-8").hex()
    sql = Path(__file__).with_suffix(".sql").read_text(encoding="utf-8")
    request = (
        f"\\set payload {payload}\n"
        + sql
        + ("\nCOMMIT;\n" if args.apply else "\nROLLBACK;\n")
    )
    mode = "APPLY" if args.apply else "DRY RUN (ROLLBACK)"
    print(f"{mode}: 42 recipes on {args.container}/{args.database}", flush=True)
    subprocess.run(
        ["docker", "exec", "-i", args.container, "psql", "-w", "-X", "-U", args.user,
         "-d", args.database, "-v", "ON_ERROR_STOP=1"],
        input=request.encode("utf-8"),
        check=True,
    )


if __name__ == "__main__":
    main()
