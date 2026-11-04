import os
import subprocess
import shutil

CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"

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
    print("Rendering HD atlases (no hands, zero wobbling, restored blinking)...")
    atlases = [
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/character/ghost_main_8x4_clean.svg',
         'ghost_main_8x4.png', 2048, 2048),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/cosmetics/cosmetic_hat_witch_8x4.svg',
         'cosmetic_hat_witch_8x4.png', 2048, 1024),
        ('D:/ghoster/ArtSource/GhostSurvivalArtPackV1/cosmetics/cosmetic_wings_moth_8x4.svg',
         'cosmetic_wings_moth_8x4.png', 2048, 1024),
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
        # Atomic replace to prevent Windows file locking errors when files are open in viewers
        tmp_ghost = out_ghost_art + '.tmp'
        shutil.copyfile(out_artsource, tmp_ghost)
        os.replace(tmp_ghost, out_ghost_art)

        tmp_unity = out_unity + '.tmp'
        shutil.copyfile(out_artsource, tmp_unity)
        os.replace(tmp_unity, out_unity)
        print(f"Copied {png_name} to Unity and art pack folders.")

    if os.path.exists('D:/ghoster/temp_render.html'):
        os.remove('D:/ghoster/temp_render.html')

    print("ALL HD atlases exported successfully!")

if __name__ == '__main__':
    main()
