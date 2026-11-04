import re
import os

svg_paths = [
    r'd:\ghoster\ArtSource\GhostSurvivalArtPackV1\character\ghost_main_8x4_clean.svg',
    r'd:\ghoster\ghost_survival_art_v1\character\ghost_main_8x4_clean.svg'
]

# We want cell (col=3, row=0, 1, 2, 3) to have crisp, unmistakable closed blinking eye slits:
# Row 0 (Down / Nhìn thẳng): cy=58, left cx=53, right cx=75
# Row 1 (Left): left cx=47 cy=57, right cx=66 cy=59
# Row 2 (Right): left cx=62 cy=59, right cx=81 cy=57
# Row 3 (Up / Lên): cy=43, left cx=53, right cx=75

def update_svg_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        text = f.read()

    matches = list(re.finditer(r'<g transform="translate\((\d+) (\d+)\)">', text))
    print(f'Updating {path}, found {len(matches)} cells')

    # Reconstruct the SVG cell by cell
    new_parts = []
    last_idx = 0

    for i, m in enumerate(matches):
        x, y = int(m.group(1)), int(m.group(2))
        col = x // 128
        row = y // 128
        start = m.start()
        end = matches[i+1].start() if i+1 < len(matches) else text.rfind('</svg>')

        cell_text = text[start:end]

        if col == 3: # Frame 3 of Idle (The Blink Frame!)
            if row == 0: # Down (Nhìn thẳng / Xuống)
                # Replace existing eyes in this cell with distinct closed eyelids
                # Remove any existing eyes/ellipses for this cell (except shadow at cy=109)
                def replace_eyes_row0(match):
                    return (
                        '<ellipse cx="53" cy="58" rx="5.8" ry="1.2" fill="#F7F7FF"/>'
                        '<ellipse cx="75" cy="58" rx="5.8" ry="1.2" fill="#F7F7FF"/>'
                    )
                # In row 0 cell 3, find the eye area
                old_eyes = re.search(r'(<ellipse cx="53"[^>]*>[\s\S]*?<ellipse cx="75"[^>]*>)', cell_text)
                if old_eyes:
                    cell_text = cell_text[:old_eyes.start()] + replace_eyes_row0(None) + cell_text[old_eyes.end():]
                    print('  Row 0 Col 3 (Down) blink updated!')

            elif row == 1: # Left
                def replace_eyes_row1(match):
                    return (
                        '<ellipse cx="47" cy="57" rx="5.2" ry="1.2" fill="#F7F7FF"/>'
                        '<ellipse cx="66" cy="59" rx="4.8" ry="1.2" fill="#F7F7FF"/>'
                    )
                old_eyes = re.search(r'(<ellipse cx="47"[^>]*>[\s\S]*?<ellipse cx="66"[^>]*>)', cell_text)
                if old_eyes:
                    cell_text = cell_text[:old_eyes.start()] + replace_eyes_row1(None) + cell_text[old_eyes.end():]
                    print('  Row 1 Col 3 (Left) blink updated!')

            elif row == 2: # Right
                def replace_eyes_row2(match):
                    return (
                        '<ellipse cx="62" cy="59" rx="4.8" ry="1.2" fill="#F7F7FF"/>'
                        '<ellipse cx="81" cy="57" rx="5.2" ry="1.2" fill="#F7F7FF"/>'
                    )
                old_eyes = re.search(r'(<ellipse cx="62"[^>]*>[\s\S]*?<ellipse cx="81"[^>]*>)', cell_text)
                if old_eyes:
                    cell_text = cell_text[:old_eyes.start()] + replace_eyes_row2(None) + cell_text[old_eyes.end():]
                    print('  Row 2 Col 3 (Right) blink updated!')

            elif row == 3: # Up
                def replace_eyes_row3(match):
                    return (
                        '<ellipse cx="53" cy="43" rx="5.2" ry="1.2" fill="#F7F7FF"/>'
                        '<ellipse cx="75" cy="43" rx="5.2" ry="1.2" fill="#F7F7FF"/>'
                    )
                old_eyes = re.search(r'(<ellipse cx="53"[^>]*>[\s\S]*?<ellipse cx="75"[^>]*>)', cell_text)
                if old_eyes:
                    cell_text = cell_text[:old_eyes.start()] + replace_eyes_row3(None) + cell_text[old_eyes.end():]
                    print('  Row 3 Col 3 (Up) blink updated!')

        new_parts.append(text[last_idx:start])
        new_parts.append(cell_text)
        last_idx = end

    new_parts.append(text[last_idx:])
    full_new_text = ''.join(new_parts)

    with open(path, 'w', encoding='utf-8') as f:
        f.write(full_new_text)

for p in svg_paths:
    if os.path.exists(p):
        update_svg_file(p)
