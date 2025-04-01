import json


def update_json(file1_path, file2_path, output_path):
    # Load the first JSON file
    with open(file1_path, 'r', encoding='utf-8') as f1:
        json1 = json.load(f1)
    
    # Load the second JSON file
    with open(file2_path, 'r', encoding='utf-8') as f2:
        json2 = json.load(f2)
    
    # Maintain the order from json1, keeping existing keys in json2
    merged_json = {key: json2[key] if key in json2 else value for key, value in json1.items()}
    
    # Save the updated JSON to the output file
    with open(output_path, 'w', encoding='utf-8') as f_out:
        json.dump(merged_json, f_out, indent=2, ensure_ascii=False)
    
    print(f"Updated JSON saved to {output_path}")

update_json('default.json', 'en-us.json', 'en-us.json')
update_json('default.json', 'de-DE.json', 'de-DE.json')
update_json('default.json', 'es-ES.json', 'es-ES.json')
update_json('default.json', 'fr-FR.json', 'fr-FR.json')
update_json('default.json', 'ja-JP.json', 'ja-JP.json')
