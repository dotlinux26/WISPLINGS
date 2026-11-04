import os

def generate_hands_svg():
    svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="512" viewBox="0 0 1024 512">',
           '<title>Ghost cute round hands cosmetic atlas</title>',
           '<defs>',
           '  <radialGradient id="handGrad" cx="35%" cy="30%" r="70%">',
           '    <stop offset="0%" stop-color="#1A1924"/>',
           '    <stop offset="65%" stop-color="#101016"/>',
           '    <stop offset="100%" stop-color="#08080C"/>',
           '  </radialGradient>',
           '</defs>']

    directions = ["Down", "Left", "Right", "Up"]

    # Subtle vertical bobbing matching the character body breathing
    down_idle_y = [-1.0, -1.8, -0.9, 0.5]
    down_walk_y = [0.0, -1.5, 0.0, 1.5]

    BASE_Y = 85.0

    for row_idx, dir_name in enumerate(directions):
        row_y = row_idx * 128
        for col_idx in range(8):
            col_x = col_idx * 128
            is_walk = col_idx >= 4
            frame_idx = col_idx - 4 if is_walk else col_idx
            
            dy = down_walk_y[frame_idx] if is_walk else down_idle_y[frame_idx]
            
            svg.append(f'<g transform="translate({col_x} {row_y})">')
            
            if dir_name == "Down":
                if not is_walk:
                    ly = BASE_Y + dy
                    ry = BASE_Y + dy
                    lx = 48.0
                    rx = 80.0
                else:
                    swing = [-1.5, 0.0, 1.5, 0.0][frame_idx]
                    ly = BASE_Y + dy + swing
                    ry = BASE_Y + dy - swing
                    lx = 48.0
                    rx = 80.0

                # Left paw
                svg.append(f'<circle cx="{lx}" cy="{ly}" r="5.2" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{lx-1.5} {ly-1.5} Q{lx} {ly-3} {lx+1.5} {ly-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')
                # Right paw
                svg.append(f'<circle cx="{rx}" cy="{ry}" r="5.2" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{rx-1.5} {ry-1.5} Q{rx} {ry-3} {rx+1.5} {ry-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')

            elif dir_name == "Left":
                # Facing left: steady front paw and back paw, zero sideways wobble
                front_x, front_y = 44.0, BASE_Y + dy
                back_x, back_y = 66.0, BASE_Y - 1.0 + dy

                # Back paw
                svg.append(f'<circle cx="{back_x}" cy="{back_y}" r="4.0" fill="url(#handGrad)" stroke="#393845" stroke-width="1.6" opacity="0.8"/>')
                # Front paw
                svg.append(f'<circle cx="{front_x}" cy="{front_y}" r="5.4" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{front_x-1.5} {front_y-1.5} Q{front_x} {front_y-3} {front_x+1.5} {front_y-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')

            elif dir_name == "Right":
                # Facing right: steady front paw and back paw, zero sideways wobble
                front_x, front_y = 84.0, BASE_Y + dy
                back_x, back_y = 62.0, BASE_Y - 1.0 + dy

                # Back paw
                svg.append(f'<circle cx="{back_x}" cy="{back_y}" r="4.0" fill="url(#handGrad)" stroke="#393845" stroke-width="1.6" opacity="0.8"/>')
                # Front paw
                svg.append(f'<circle cx="{front_x}" cy="{front_y}" r="5.4" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{front_x-1.5} {front_y-1.5} Q{front_x} {front_y-3} {front_x+1.5} {front_y-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')

            elif dir_name == "Up":
                lx, ly = 38.0, BASE_Y + dy
                rx, ry = 90.0, BASE_Y + dy

                svg.append(f'<circle cx="{lx}" cy="{ly}" r="4.6" fill="url(#handGrad)" stroke="#393845" stroke-width="1.8"/>')
                svg.append(f'<circle cx="{rx}" cy="{ry}" r="4.6" fill="url(#handGrad)" stroke="#393845" stroke-width="1.8"/>')

            svg.append('</g>')

    svg.append('</svg>')
    
    out_paths = [
        'D:/ghoster/ArtSource/GhostSurvivalArtPackV1/cosmetics/cosmetic_hands_8x4.svg',
        'D:/ghoster/ghost_survival_art_v1/cosmetics/cosmetic_hands_8x4.svg'
    ]
    for p in out_paths:
        os.makedirs(os.path.dirname(p), exist_ok=True)
        with open(p, 'w', encoding='utf-8') as f:
            f.write('\n'.join(svg))
        print(f"Generated steady hands SVG at {p}")

if __name__ == '__main__':
    generate_hands_svg()
