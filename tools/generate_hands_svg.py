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

    # 4 rows: Down, Left, Right, Up
    # 8 cols: 0..3 Idle, 4..7 Walk
    directions = ["Down", "Left", "Right", "Up"]

    # Subtle bobbing matching the character body breathing
    down_idle_y = [-1.0, -1.8, -0.9, 0.5]
    down_walk_y = [0.0, -1.5, 0.0, 1.5]

    # Lowered Y: eyes are at 58, mid-lower belly is at 84-86 (skirt is at 105)
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
                # Two cute paws resting on the lower belly
                if not is_walk:
                    ly = BASE_Y + dy
                    ry = BASE_Y + dy
                    lx = 48.0
                    rx = 80.0
                else:
                    swing = [-2.5, 0.0, 2.5, 0.0][frame_idx]
                    ly = BASE_Y + dy + swing
                    ry = BASE_Y + dy - swing
                    lx = 48.0 + swing * 0.4
                    rx = 80.0 + swing * 0.4

                # Left paw
                svg.append(f'<circle cx="{lx}" cy="{ly}" r="5.2" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{lx-1.5} {ly-1.5} Q{lx} {ly-3} {lx+1.5} {ly-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')
                # Right paw
                svg.append(f'<circle cx="{rx}" cy="{ry}" r="5.2" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{rx-1.5} {ry-1.5} Q{rx} {ry-3} {rx+1.5} {ry-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')

            elif dir_name == "Left":
                # Facing left: Left paw in front, right paw tucked slightly behind
                if not is_walk:
                    front_x, front_y = 44.0, BASE_Y + dy
                    back_x, back_y = 66.0, BASE_Y - 1.0 + dy
                else:
                    swing = [-3.0, 0.0, 3.0, 0.0][frame_idx]
                    front_x, front_y = 44.0 + swing, BASE_Y + dy - abs(swing) * 0.4
                    back_x, back_y = 66.0 - swing * 0.5, BASE_Y - 1.0 + dy

                # Back paw
                svg.append(f'<circle cx="{back_x}" cy="{back_y}" r="4.0" fill="url(#handGrad)" stroke="#393845" stroke-width="1.6" opacity="0.8"/>')
                # Front paw
                svg.append(f'<circle cx="{front_x}" cy="{front_y}" r="5.4" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{front_x-1.5} {front_y-1.5} Q{front_x} {front_y-3} {front_x+1.5} {front_y-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')

            elif dir_name == "Right":
                # Facing right: Right paw in front, left paw tucked slightly behind
                if not is_walk:
                    front_x, front_y = 84.0, BASE_Y + dy
                    back_x, back_y = 62.0, BASE_Y - 1.0 + dy
                else:
                    swing = [-3.0, 0.0, 3.0, 0.0][frame_idx]
                    front_x, front_y = 84.0 - swing, BASE_Y + dy - abs(swing) * 0.4
                    back_x, back_y = 62.0 + swing * 0.5, BASE_Y - 1.0 + dy

                # Back paw
                svg.append(f'<circle cx="{back_x}" cy="{back_y}" r="4.0" fill="url(#handGrad)" stroke="#393845" stroke-width="1.6" opacity="0.8"/>')
                # Front paw
                svg.append(f'<circle cx="{front_x}" cy="{front_y}" r="5.4" fill="url(#handGrad)" stroke="#393845" stroke-width="2.0"/>')
                svg.append(f'<path d="M{front_x-1.5} {front_y-1.5} Q{front_x} {front_y-3} {front_x+1.5} {front_y-1.5}" fill="none" stroke="#5E5C6E" stroke-width="1.0" stroke-linecap="round"/>')

            elif dir_name == "Up":
                # Facing up: hands seen from behind on outer sides
                if not is_walk:
                    lx, ly = 38.0, BASE_Y + dy
                    rx, ry = 90.0, BASE_Y + dy
                else:
                    swing = [-2.5, 0.0, 2.5, 0.0][frame_idx]
                    lx, ly = 38.0, BASE_Y + dy + swing
                    rx, ry = 90.0, BASE_Y + dy - swing

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
        print(f"Generated lower-placed hands SVG at {p}")

if __name__ == '__main__':
    generate_hands_svg()
