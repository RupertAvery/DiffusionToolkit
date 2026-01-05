import json
import argparse
from collections import OrderedDict


def extract_class_inputs(input_path):
    with open(input_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    result = OrderedDict()

    for node in data.values():
        class_type = node.get("class_type")
        inputs = node.get("inputs", {})

        if not class_type:
            continue

        # Only record the first occurrence to preserve order of appearance
        if class_type not in result:
            result[class_type] = list(inputs.keys())

    return result


def main():
    parser = argparse.ArgumentParser(
        description="Extract unique class_type values and their input property names from a JSON file."
    )
    parser.add_argument(
        "input",
        help="Path to input JSON file"
    )
    parser.add_argument(
        "-o", "--output",
        help="Path to output JSON file (defaults to stdout)",
        default=None
    )
    parser.add_argument(
        "--no-indent",
        action="store_true",
        help="Disable pretty-printed JSON output"
    )

    args = parser.parse_args()

    result = extract_class_inputs(args.input)

    indent = None if args.no_indent else 2
    output_json = json.dumps(result, indent=indent)

    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(output_json)
    else:
        print(output_json)


if __name__ == "__main__":
    main()
