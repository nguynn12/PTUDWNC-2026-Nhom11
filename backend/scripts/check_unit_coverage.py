"""Check NFR-MAINT-002 using the unit-test-only Cobertura report."""

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path, PurePosixPath


def count_lines(report: Path) -> tuple[int, int]:
    root = ET.parse(report).getroot()
    lines: dict[tuple[str, int], bool] = {}
    for item in root.findall("./packages/package[@name='CulinaryBlog.Application']/classes/class"):
        filename = item.attrib["filename"].replace("\\", "/")
        folders = PurePosixPath(filename).parts[:-1]
        if not {"Commands", "Queries", "Validators"}.intersection(folders):
            continue
        for line in item.findall("./lines/line"):
            key = filename, int(line.attrib["number"])
            # A source line can appear in more than one class (e.g. an async handler).
            lines[key] = lines.get(key, False) or int(line.attrib["hits"]) > 0

    if not lines:
        raise ValueError("No Application Commands, Queries or Validators lines in the report")
    return sum(lines.values()), len(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("report", type=Path)
    parser.add_argument("--minimum", type=int, choices=range(101), metavar="0-100")
    options = parser.parse_args()

    try:
        covered, total = count_lines(options.report)
    except (OSError, ET.ParseError, KeyError, ValueError) as error:
        print(f"Could not measure NFR-MAINT-002 coverage: {error}", file=sys.stderr)
        return 2

    print(f"NFR-MAINT-002 unit line coverage: {covered}/{total} ({covered / total:.2%})")
    if options.minimum is not None and covered * 100 < total * options.minimum:
        print(f"Required minimum: {options.minimum}%", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
