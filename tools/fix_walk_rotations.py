import re
import os

svg_paths = [
    'D:/ghoster/ArtSource/GhostSurvivalArtPackV1/character/ghost_main_8x4_clean.svg',
    'D:/ghoster/ghost_survival_art_v1/character/ghost_main_8x4_clean.svg'
]

# Specific rotation patterns in Row 1 (Left Walk) and Row 2 (Right Walk)
# Row 1 (Left Walk): -5.00, -1.50, 2.00, -1.50
# Row 2 (Right Walk): -2.00, 1.50, 5.00, 1.50

for path in svg_paths:
    if not os.path.exists(path):
        continue
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Split into cell groups (<g transform="translate(x y)">)
    # Row 1 is y=128 (Left), cols 4..7 (x=512, 640, 768, 896)
    # Row 2 is y=256 (Right), cols 4..7 (x=512, 640, 768, 896)
    
    # We can match all cells in Row 1 and Row 2 walk and replace their rotation
    def fix_row_walk_rotations(text, row_y):
        pattern = rf'(<g transform="translate\((512|640|768|896)\s+{row_y}\)">.*?<g transform="translate\(0 [^"]+\)\s+)rotate\([^)]+\)(\s+translate\(0 65\))'
        return re.sub(pattern, r'\1rotate(0.00 64 65)\3', text, flags=re.DOTALL)

    content = fix_row_walk_rotations(content, 128) # Left Walk
    content = fix_row_walk_rotations(content, 256) # Right Walk

    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Removed Left/Right walk rotations in {path}")
