import re
import os
import subprocess
import shutil

CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"

def update_ghost_svg():
    svg_paths = [
        'D:/ghoster/ArtSource/GhostSurvivalArtPackV1/character/ghost_main_8x4_clean.svg',
        'D:/ghoster/ghost_survival_art_v1/character/ghost_main_8x4_clean.svg'
    ]
    
    for path in svg_paths:
        if not os.path.exists(path):
            continue
        with open(path, 'r', encoding='utf-8') as f:
            content = f.read()

        # 1. Remove dashed guide ellipse
        content = re.sub(r'<ellipse\s+cx="64"\s+cy="68"\s+rx="39"\s+ry="43"[^>]*/>\s*', '', content)

        # 2. Update bodyGrad to deep ink black matching monster
        old_grad = re.search(r'<radialGradient id="bodyGrad"[^>]*>.*?</radialGradient>', content, re.DOTALL)
        if old_grad:
            new_grad = ('<radialGradient id="bodyGrad" cx="42%" cy="28%" r="75%">\n'
                        '      <stop offset="0" stop-color="#1A1924"/>\n'
                        '      <stop offset="0.55" stop-color="#101016"/>\n'
                        '      <stop offset="1" stop-color="#08080C"/>\n'
                        '    </radialGradient>')
            content = content.replace(old_grad.group(0), new_grad)

        # 3. In Idle columns (cols 0..3 of each row), remove rotation angle
        for r in ['-0.80', '0.40', '0.80', '-0.40']:
            content = content.replace(f'rotate({r} 64 65)', 'rotate(0.00 64 65)')

        # 4. Make eyes wide, bright and cute in all idle frames (remove scale(1 0.13) squished blinking slit eyes)
        content = re.sub(r'transform="translate\(0 50\.46\) scale\(1 0\.13\)"', '', content)

        # Stroke adjustment
        content = content.replace('stroke="#3A3947"', 'stroke="#393845"')

        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Updated SVG at {path}")

def render_svg_to_png(svg_path, png_path, width, height):
    html_content = f'''<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<style>
  html, body {{
    margin: 0;
    padding: 0;
    width: {width}px;
    height: {height}px;
    overflow: hidden;
    background: transparent !important;
  }}
  img {{
    width: {width}px;
    height: {height}px;
    display: block;
  }}
</style>
</head>
<body>
  <img src="{svg_path.replace('\\', '/')}" />
</body>
</html>'''

    temp_html = 'D:/ghoster/temp_render.html'
    with open(temp_html, 'w', encoding='utf-8') as f:
        f.write(html_content)

    os.makedirs(os.path.dirname(png_path), exist_ok=True)
    
    cmd = [
        CHROME_PATH,
        '--headless=new',
        '--disable-gpu',
        '--default-background-color=00000000',
        f'--screenshot={png_path}',
        f'--window-size={width},{height}',
        f'file:///{temp_html}'
    ]
    subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    print(f"Rendered {png_path} ({width}x{height})")

def main():
    print("1. Updating character SVG colors and idle frames...")
    update_ghost_svg()

    print("2. Rendering HD 256x256-per-cell PNG atlases...")
    atlases = [
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/character/ghost_main_8x4_clean.svg',
         'ghost_main_8x4.png', 2048, 1024),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/cosmetics/cosmetic_hat_witch_8x4.svg',
         'cosmetic_hat_witch_8x4.png', 2048, 1024),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/cosmetics/cosmetic_wings_moth_8x4.svg',
         'cosmetic_wings_moth_8x4.png', 2048, 1024),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/cosmetics/cosmetic_hands_8x4.svg',
         'cosmetic_hands_8x4.png', 2048, 1024),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/creatures/shade_stalker_4x4_clean.svg',
         'shade_stalker_4x4.png', 1024, 1024),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/creatures/soul_collectible_6x1_clean.svg',
         'soul_collectible_6x1.png', 1536, 256)
    ]

    for svg_file, png_name, w, h in atlases:
        out_artsource = f'D:/ghoster/ArtSource/GhostSurvivalArtPackV1/exports/{png_name}'
        render_svg_to_png(svg_file, out_artsource, w, h)

        # Copy to ghost_survival_art_v1 and Unity Assets
        out_ghost_art = f'D:/ghoster/ghost_survival_art_v1/exports/{png_name}'
        out_unity = f'D:/ghoster/WISPLINGS/Assets/_Game/Art/Sprites/{png_name}'
        shutil.copyfile(out_artsource, out_ghost_art)
        shutil.copyfile(out_artsource, out_unity)
        print(f"Copied {png_name} to Unity and art pack folders.")

    if os.path.exists('D:/ghoster/temp_render.html'):
        os.remove('D:/ghoster/temp_render.html')

    print("ALL HD atlases exported successfully!")

if __name__ == '__main__':
    main()
