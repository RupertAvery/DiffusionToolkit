import json
import argparse
import sys
from collections import OrderedDict


def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def merge_class_maps(a, b):
    merged = OrderedDict()
    warnings = []

    all_keys = set(a.keys()) | set(b.keys())

    for key in sorted(all_keys):
        a_props = a.get(key)
        b_props = b.get(key)

        if a_props is None:
            merged[key] = b_props
            continue

        if b_props is None:
            merged[key] = a_props
            continue

        # Both exist
        set_a = set(a_props)
        set_b = set(b_props)

        if set_a != set_b:
            warnings.append(
                f"WARNING: Property mismatch for '{key}':\n"
                f"  A: {a_props}\n"
                f"  B: {b_props}"
            )

        # Keep the one with more properties
        merged[key] = a_props if len(a_props) >= len(b_props) else b_props

    return merged, warnings


def main():
    parser = argparse.ArgumentParser(
        description="Merge two class-to-input JSON maps, keeping the entry with more properties and warning on differences."
    )
    parser.add_argument("input_a", help="First input JSON file")
    parser.add_argument("input_b", help="Second input JSON file")
    parser.add_argument(
        "-o", "--output",
        help="Output JSON file (defaults to stdout)",
        default=None
    )
    parser.add_argument(
        "--no-indent",
        action="store_true",
        help="Disable pretty-printed JSON"
    )

    args = parser.parse_args()

    a = load_json(args.input_a)
    b = load_json(args.input_b)

    merged, warnings = merge_class_maps(a, b)

    for w in warnings:
        print(w, file=sys.stderr)

    indent = None if args.no_indent else 2
    output_json = json.dumps(merged, indent=indent)

    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(output_json)
    else:
        print(output_json)


if __name__ == "__main__":
    main()
