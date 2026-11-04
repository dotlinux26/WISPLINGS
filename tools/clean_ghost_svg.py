import re
import os

svg_paths = [
    'D:/ghoster/ArtSource/GhostSurvivalArtPackV1/character/ghost_main_8x4_clean.svg',
    'D:/ghoster/ghost_survival_art_v1/character/ghost_main_8x4_clean.svg'
]

for p in svg_paths:
    if not os.path.exists(p):
        continue
    with open(p, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Remove dashed guide ellipse
    content = re.sub(r'<ellipse\s+cx="64"\s+cy="68"\s+rx="39"\s+ry="43"[^>]*/>\s*', '', content)

    # 2. In Idle columns, remove rotation swaying
    for r in ['-0.80', '0.40', '0.80', '-0.40']:
        content = content.replace(f'rotate({r} 64 65)', 'rotate(0.00 64 65)')

    with open(p, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Processed {p}")

