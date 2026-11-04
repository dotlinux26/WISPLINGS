import subprocess, re

# Always read the pristine 4-row SVG from main branch to prevent accumulating corruptions!
raw_main_svg = subprocess.check_output(
    ['git', 'show', 'main:ArtSource/GhostSurvivalArtPackV1/character/ghost_main_8x4_clean.svg'],
    encoding='utf-8'
)

# Parse header
first_cell = re.search(r'<g transform="translate\(0 0\)">', raw_main_svg)
header = raw_main_svg[:first_cell.start()]

# Update header dimensions to 1024x1024 (8 rows x 8 cols of 128x128 each)
header = header.replace('height="512"', 'height="1024"')
header = header.replace('viewBox="0 0 1024 512"', 'viewBox="0 0 1024 1024"')
header = header.replace('8 columns x 4 rows', '8 columns x 8 rows')

# Parse all 32 original cells
matches = list(re.finditer(r'<g transform="translate\((\d+) (\d+)\)">', raw_main_svg))
cells = {}
for i, m in enumerate(matches):
    x, y = int(m.group(1)), int(m.group(2))
    col = x // 128
    row = y // 128
    start = m.start()
    end = matches[i+1].start() if i+1 < len(matches) else raw_main_svg.rfind('</svg>')
    cells[(row, col)] = raw_main_svg[start:end]

print(f"Loaded {len(cells)} original cells from main branch")

# Function to replace ALL eyes in a cell cleanly
def set_eyes_in_cell(cell_xml, new_eye_xml):
    pattern = r'(<path d="M43 43[^>]+>)([\s\S]*?)(<circle cx="24)'
    replaced = re.sub(pattern, r'\1\n      ' + new_eye_xml + r'\n    \n        \n      \3', cell_xml)
    return replaced

def get_diagonal_eyes(direction, col):
    is_blink = (col == 3)
    is_walk = (col >= 4)
    
    if direction == "DownLeft":
        if is_blink:
            return '<ellipse cx="50" cy="61" rx="5.0" ry="1.2" fill="#F7F7FF"/><ellipse cx="70" cy="62" rx="4.7" ry="1.2" fill="#F7F7FF"/>'
        elif is_walk:
            return (
                '<ellipse cx="50" cy="65" rx="5.0" ry="7.0" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="48.5" cy="62.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
                '<ellipse cx="70" cy="66" rx="4.7" ry="6.7" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="68.5" cy="63.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
            )
        else:
            return (
                '<ellipse cx="50" cy="61" rx="5.0" ry="7.0" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="48.5" cy="58.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
                '<ellipse cx="70" cy="62" rx="4.7" ry="6.7" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="68.5" cy="59.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
            )

    elif direction == "DownRight":
        if is_blink:
            return '<ellipse cx="58" cy="62" rx="4.7" ry="1.2" fill="#F7F7FF"/><ellipse cx="78" cy="61" rx="5.0" ry="1.2" fill="#F7F7FF"/>'
        elif is_walk:
            return (
                '<ellipse cx="58" cy="66" rx="4.7" ry="6.7" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="56.5" cy="63.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
                '<ellipse cx="78" cy="65" rx="5.0" ry="7.0" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="76.5" cy="62.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
            )
        else:
            return (
                '<ellipse cx="58" cy="62" rx="4.7" ry="6.7" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="56.5" cy="59.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
                '<ellipse cx="78" cy="61" rx="5.0" ry="7.0" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="76.5" cy="58.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
            )

    elif direction == "UpLeft":
        if is_blink:
            return '<ellipse cx="50" cy="47" rx="4.9" ry="1.2" fill="#F7F7FF"/><ellipse cx="70" cy="49" rx="4.6" ry="1.2" fill="#F7F7FF"/>'
        else:
            return (
                '<ellipse cx="50" cy="47" rx="4.9" ry="6.6" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="48.5" cy="44.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
                '<ellipse cx="70" cy="49" rx="4.6" ry="6.4" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="68.5" cy="46.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
            )

    elif direction == "UpRight":
        if is_blink:
            return '<ellipse cx="58" cy="49" rx="4.6" ry="1.2" fill="#F7F7FF"/><ellipse cx="78" cy="47" rx="4.9" ry="1.2" fill="#F7F7FF"/>'
        else:
            return (
                '<ellipse cx="58" cy="49" rx="4.6" ry="6.4" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="56.5" cy="46.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
                '<ellipse cx="78" cy="47" rx="4.9" ry="6.6" fill="#F7F7FF" transform="translate(0 0.00) scale(1 1)"/>'
                '<ellipse cx="76.5" cy="44.2" rx="1.25" ry="1.8" fill="#FFFFFF" opacity=".78"/>'
            )

# 8 rows plan:
row_plan = [
    ("Down", 0, None),          # Row 0: Down (from main row 0)
    ("DownLeft", 1, "DownLeft"),# Row 1: DownLeft (from main row 1 Left with DownLeft eyes)
    ("Left", 1, None),          # Row 2: Left (from main row 1 Left)
    ("UpLeft", 1, "UpLeft"),    # Row 3: UpLeft (from main row 1 Left with UpLeft eyes)
    ("Up", 3, None),            # Row 4: Up (from main row 3 Up)
    ("UpRight", 2, "UpRight"),  # Row 5: UpRight (from main row 2 Right with UpRight eyes)
    ("Right", 2, None),         # Row 6: Right (from main row 2 Right)
    ("DownRight", 2, "DownRight")# Row 7: DownRight (from main row 2 Right with DownRight eyes)
]

output_cells = []

for new_row_idx, (dir_name, src_row, diag_eyes) in enumerate(row_plan):
    new_y = new_row_idx * 128
    for col in range(8):
        new_x = col * 128
        cell_xml = cells[(src_row, col)]
        
        # Replace the outer translate(old_x old_y) with translate(new_x new_y)
        cell_xml = re.sub(r'<g transform="translate\(\d+ \d+\)">', f'<g transform="translate({new_x} {new_y})">', cell_xml, count=1)
        
        # If diagonal direction, replace eyes cleanly
        if diag_eyes is not None:
            eye_xml = get_diagonal_eyes(diag_eyes, col)
            cell_xml = set_eyes_in_cell(cell_xml, eye_xml)
            
        output_cells.append(cell_xml)

full_svg = header + ''.join(output_cells) + '\n</svg>'

# Write to both locations
target_paths = [
    r'd:\ghoster\ArtSource\GhostSurvivalArtPackV1\character\ghost_main_8x4_clean.svg',
    r'd:\ghoster\ghost_survival_art_v1\character\ghost_main_8x4_clean.svg'
]

for p in target_paths:
    with open(p, 'w', encoding='utf-8') as f:
        f.write(full_svg)
    print(f'Wrote clean 8x8 SVG to {p}')

print('Generated clean 8x8 SVG atlas successfully!')
