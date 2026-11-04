# Ghost Survival — Art Pack v1

Bộ vector gốc và sprite atlas đầu tiên theo visual direction đã chốt.

## Art direction

- Top-down 2D, ma nhỏ màu đen, silhouette tròn mềm; mắt trắng là điểm nhận diện.
- Thế giới ảm đạm, gần như monochrome. Màu chỉ nhấn vào linh hồn, vật phẩm hiếm, ánh sáng và hiệu ứng phép.
- Nhân vật chính chỉ có **một silhouette cơ thể**. Hướng nhìn đổi bằng vị trí mắt/pose nhẹ; không tạo bốn model cơ thể khác nhau.
- Mũ/cánh là **cosmetic layer** độc lập, không phải item trong expedition inventory.
- Độ mềm và cảm giác lơ lửng đến từ bob, squash/stretch rất nhẹ, blink, aura, hạt bụi và chuyển động phụ.

## File chính

### Main character
- `character/ghost_main_8x4_clean.svg` — editable SVG master, atlas 8×4, cell 128×128.
- `character/ghost_main_8x4_grid.svg` — guide có lưới; **không import bản guide**.
- `character/ghost_idle_animated_preview.svg` — preview SVG có CSS bob/blink.
- `exports/ghost_main_8x4.png` — PNG atlas trong suốt để import vào Unity.

Rows: `Down`, `Left`, `Right`, `Up`.
Columns: `Idle_0..3`, `Walk_0..3`.
Idle loop có nhịp lơ lửng; walk loop có nhịp nảy/squash nhẹ. Đây là sprite timing blockout v1, có thể tinh chỉnh sau test trong game.

### Collectible spirit
- `creatures/soul_collectible_6x1_clean.svg`
- `creatures/soul_collectible_6x1_grid.svg` — guide có lưới.
- `creatures/soul_collectible_animated_preview.svg` — preview nhấp nhô.
- `exports/soul_collectible_6x1.png`

### One enemy: Shade Stalker
- `creatures/shade_stalker_4x4_clean.svg`
- `creatures/shade_stalker_4x4_grid.svg` — guide có lưới.
- `creatures/shade_stalker_animated_preview.svg`
- `exports/shade_stalker_4x4.png`

### Cosmetic layers
- `cosmetics/cosmetic_hat_witch_8x4.svg` / `exports/cosmetic_hat_witch_8x4.png`
- `cosmetics/cosmetic_wings_moth_8x4.svg` / `exports/cosmetic_wings_moth_8x4.png`
- `cosmetics/witch_hat_single.svg`, `cosmetics/moth_wings_single.svg`
- `cosmetics/attachment_anchor_guide.svg`

The hat atlas matches the body atlas frame-for-frame and is drawn **above** the body.
The wings atlas matches the body atlas frame-for-frame and is drawn **behind** the body.
Put all three renderers under one moving root so bob/knockback is shared. Keep a stable world pivot for sorting.


## Animated quick previews

Open the SVG preview files in a modern browser to see CSS/SVG motion:
- `character/ghost_idle_animated_preview.svg`
- `creatures/soul_collectible_animated_preview.svg`
- `creatures/shade_stalker_animated_preview.svg`
- `effects/wisp_flame_effect_preview.svg`

GIF previews are also included in `exports/previews/` for quick review:
- `ghost_idle.gif`, `ghost_walk.gif`, `soul_hover.gif`, `shade_stalker_chase.gif`

## Cosmetic/account rule

Mũ/cánh không nằm trong inventory chuyến đi:
- `unlocked_cosmetic_ids` thuộc account profile, vĩnh viễn sau khi mở khóa.
- `equipped_cosmetic_ids` thuộc account loadout, đồng bộ giữa thiết bị.
- Khi vào expedition, server nhận cosmetic loadout hợp lệ và chỉ dùng để render.
- Chết, rơi đồ hoặc kết thúc expedition không làm mất cosmetic.
- Không cho client tự khai báo cosmetic đã mở khóa; server/account backend xác thực ownership.

## Unity import suggestion

1. Dùng các file PNG trong `exports/` cho runtime prototype; giữ SVG là nguồn vector có thể chỉnh sửa.
2. Với atlas: `Texture Type = Sprite (2D and UI)`, `Sprite Mode = Multiple`, `Pixels Per Unit = 128`.
3. Trong Sprite Editor: `Slice → Grid By Cell Size → 128 × 128`, `Offset = 0`, `Padding = 0`; kiểm tra đúng số hàng/cột.
4. Đặt pivot custom ở normalized bottom-left `(0.5, 0.18)` cho body/enemy để sorting ổn định quanh bóng dưới chân. Với collectible có thể pivot `(0.5, 0.5)`.
5. Tạo clips trong Unity Animator từ sprite slices. Bắt đầu ở 8 FPS cho idle/walk; điều chỉnh sau khi xem trong game.
6. Hat và wings phải dùng đúng frame index tương ứng với body. Wings sort thấp hơn body; hat sort cao hơn body.

**Lưu ý:** SVG animated preview dùng CSS/SVG animation để xem trong trình duyệt. Unity không phát các animation đó từ SVG. Unity runtime nên dùng PNG sprite atlas + Animator, hoặc tách Particle System cho sương, lửa, hạt sáng. SVG là bản master chỉnh sửa, không phải clip runtime.

## Animation language

- Ghost idle: chu kỳ 4 frame, lên/xuống khoảng 2–3 px, blink rất ngắn và không quá thường xuyên.
- Ghost walk: 4 frame, bob khoảng 2 px và nghiêng tối đa khoảng 4°; tránh biến dạng mạnh.
- Soul: pulse scale khoảng 0.98–1.04, bob nhỏ, particle orbit; loop 8–10 FPS.
- Stalker: body breathe/bob, chân lệch nhịp nhẹ; mắt sáng hơn khi phát hiện người chơi.
- Flame: dùng Particle System với noise/size-over-lifetime/color-over-lifetime; giữ accent màu hiếm, không phủ màu lên toàn cảnh.

## Palette

| Role | Color |
|---|---|
| Main ink | `#111117` |
| Secondary ink | `#20202A` |
| Edge | `#3A3947` |
| Eye white | `#F7F7FF` |
| Spirit glow | `#BFE8FF` |
| Rare magic | `#C8C1FF` |
| Cosmetic accent | `#F0A9C9` |

## Source metadata

- `sprite_manifest.json` — dimensions, rows, columns, pivots, account cosmetic rules.
- `sprite_frames.csv` — individual frame coordinates and suggested names.
