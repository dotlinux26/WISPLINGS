import glob
import re
import os

def clean_meta_files():
    pattern = 'D:/ghoster/WISPLINGS/Assets/_Game/Art/Sprites/*.meta'
    meta_files = glob.glob(pattern)
    print(f"Found {len(meta_files)} meta files to check.")

    for meta_path in meta_files:
        with open(meta_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # Check for spriteSheet
        if 'spriteSheet:' not in content:
            continue

        sprites_block = content[content.find('spriteSheet:'):]
        valid_names = set(re.findall(r'name:\s+([^\r\n]+)', sprites_block))
        if not valid_names:
            continue

        # Find internalIDToNameTable
        table_start_idx = content.find('internalIDToNameTable:')
        if table_start_idx == -1:
            continue

        table_header_end = content.find('\n', table_start_idx) + 1
        table_end_idx = content.find('externalObjects:', table_header_end)
        if table_end_idx == -1:
            continue

        table_body = content[table_header_end:table_end_idx]

        # Extract all entries in table
        entry_pattern = re.compile(r'(\s*-\s*first:\r?\n\s*213:\s*(-?\d+)\r?\n\s*second:\s*([^\r\n]+)\r?\n?)')
        entries = entry_pattern.findall(table_body)

        kept_entries = []
        kept_names = set()
        for full_entry, file_id, name in entries:
            name = name.strip()
            if name in valid_names and name not in kept_names:
                kept_entries.append(full_entry)
                kept_names.add(name)

        new_table_body = ''.join(kept_entries)
        new_content = content[:table_header_end] + new_table_body + content[table_end_idx:]

        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(new_content)

        file_name = os.path.basename(meta_path)
        print(f"Cleaned {file_name}: {len(entries)} -> {len(kept_entries)} entries (valid sprites: {len(valid_names)})")

if __name__ == '__main__':
    clean_meta_files()
