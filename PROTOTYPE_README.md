# 🎮 W1SPL1NGS (Ghost Survival) — Unity 2D Prototype V1

Chào mừng đến với bản Prototype đầu tiên của **W1SPL1NGS**!  
Bản prototype này đã hiện thực hóa toàn bộ art direction, animation 4 hướng, cơ chế di chuyển, hệ thống ghép lớp ngoại trang (Cosmetics), quái săn đuổi Shade Stalker, linh hồn thu thập (Soul Collectible) và hiệu ứng môi trường trong Unity 2D (URP).

---

## 🕹️ 1. Hướng Dẫn Điều Khiển (Controls)

| Phím | Chức năng | Ghi chú |
|---|---|---|
| **W, A, S, D** | Di chuyển 4 hướng | Hỗ trợ chuẩn hóa di chuyển chéo |
| **Phím Mũi Tên (Arrow Keys)** | Di chuyển 4 hướng | Thay thế hoặc dùng song song với WASD |
| **Phím H** | Bật / Tắt Mũ phù thủy (Witch Hat) | Hoặc bấm nút UI trên màn hình |
| **Phím J** | Bật / Tắt Cánh bướm đêm (Moth Wings) | Hoặc bấm nút UI trên màn hình |

---

## 🚀 2. Cách Mở và Chạy Prototype trong Unity

1. Mở dự án `WISPLINGS` trong **Unity 6 (6000.6.5f1)**.
2. Điều hướng đến thư mục:  
   `Assets/_Game/Scenes/ArtSandbox.unity`
3. Nhấp đúp chuột để mở scene **ArtSandbox**.
4. Nhấn nút **Play ▶️** ở trên cùng của Unity Editor:
   * Dùng **WASD** hoặc **Arrow Keys** để điều khiển bé ma đen lơ lửng khám phá thế giới.
   * Quan sát hiệu ứng đổi hướng 4 chiều và animation di chuyển (Idle/Walk) mượt mà.
   * Thử nhấn phím **H** và **J** để bật/tắt mũ và cánh theo thời gian thực (các lớp render luôn bám sát frame chuyển động của thân ma).
   * Đi lại gần các hạt linh hồn màu xanh cyan phát sáng (**Soul Collectibles**) để hấp thụ.
   * Để ý quái bóng đêm (**Shade Stalker**) sẽ phát hiện và bắt đầu đuổi theo khi bạn đi vào tầm nhìn của nó!

*(Lưu ý: Nếu muốn chạy lại toàn bộ quy trình slice texture, sinh animation clip và build lại scene, bạn có thể chọn menu `Tools > W1SPL1NGS > 1. Setup All (Slice, Clips, Prefabs, Scene)` trên thanh menu của Unity).*

---

## 📂 3. Danh Sách Các Tài Nguyên & Script Đã Tạo

### 📜 Scripts C# (`Assets/_Game/Scripts/`)
* **Characters:**
  * `GhostController.cs`: Điều khiển di chuyển Rigidbody2D top-down, chuẩn hóa hướng chéo, hỗ trợ cả New Input System và Legacy Input (WASD + Arrow Keys), hiệu ứng bồng bềnh/thở nhẹ.
  * `IDamageable.cs`: Interface chuẩn cho thực thể chịu sát thương.
* **Animation:**
  * `GhostDirection.cs`: Enum 4 hướng (`Down = 0, Left = 1, Right = 2, Up = 3`).
  * `GhostAnimator.cs`: Cập nhật trạng thái Animator parameter cho 4 hướng và Idle/Walk.
* **Cosmetics:**
  * `CosmeticSynchronizer.cs`: Đồng bộ hóa frame-for-frame giữa BodyRenderer và các lớp ngoại trang (Mũ + Cánh).
  * `CosmeticLoadout.cs`: Quản lý danh sách ngoại trang tài khoản (`unlocked_cosmetic_ids`), slot trang bị, phím tắt H/J và giao diện OnGUI.
* **Creatures:**
  * `ShadeStalker.cs`: AI quái săn mồi: phát hiện mục tiêu theo bán kính, bám đuổi theo 4 hướng và tấn công khi tiếp cận gần.
* **Environment:**
  * `SoulCollectible.cs`: Hạt linh hồn lơ lửng, phát sáng, tự động thu thập khi người chơi đến gần.
  * `SandboxGround.cs`: Mặt đất đơn sắc u ám theo đúng art direction.
* **Camera:**
  * `CameraFollow.cs`: Camera 2D theo dõi người chơi mượt mà với `SmoothDamp`.
* **Editor:**
  * `GhostAtlasImporter.cs`: Bộ công cụ tự động hóa slice sprite atlas, sinh clip animation, animator controller, prefabs và scene.

### 🎨 Sprite Atlases Đã Slice Chuẩn Hóa (`Assets/_Game/Art/Sprites/`)
* `ghost_main_8x4.png` (8 cols × 4 rows = 32 frames, pivot `0.5, 0.18`, 128 PPU).
* `cosmetic_hat_witch_8x4.png` (8 cols × 4 rows = 32 frames, pivot `0.5, 0.18`, 128 PPU).
* `cosmetic_wings_moth_8x4.png` (8 cols × 4 rows = 32 frames, pivot `0.5, 0.18`, 128 PPU).
* `soul_collectible_6x1.png` (6 cols × 1 row = 6 frames, pivot `0.5, 0.5`, 128 PPU).
* `shade_stalker_4x4.png` (4 cols × 4 rows = 16 frames, pivot `0.5, 0.18`, 128 PPU).
* `wisp_flame_effect.png` (Hiệu ứng lửa ma phát sáng).

### 🎬 Animation Clips & Controllers (`Assets/_Game/Animations/`)
* **Ghost Clips (8 clips, 8 FPS, Loop):**
  * `Ghost_Idle_Down`, `Ghost_Idle_Left`, `Ghost_Idle_Right`, `Ghost_Idle_Up`
  * `Ghost_Walk_Down`, `Ghost_Walk_Left`, `Ghost_Walk_Right`, `Ghost_Walk_Up`
* **Stalker Clips (4 clips, 8 FPS, Loop):**
  * `Stalker_Chase_Down`, `Stalker_Chase_Left`, `Stalker_Chase_Right`, `Stalker_Chase_Up`
* **Soul Clip (1 clip, 8 FPS, Loop):**
  * `Soul_Hover`
* **Controllers:**
  * `GhostAnimatorController.controller`
  * `ShadeStalkerController.controller`
  * `SoulController.controller`

### 🧱 Prefabs (`Assets/_Game/Prefabs/`)
1. **`GhostPlayer.prefab`**:
   * Cấu trúc phân lớp hoàn chỉnh:
     * `GhostRoot` (SortingGroup, GhostController, GhostAnimator, CosmeticLoadout, CosmeticSynchronizer, CircleCollider2D)
       * `Shadow` (Sorting Order -20)
       * `WingsRenderer` (Sorting Order -10)
       * `BodyRenderer` (Sorting Order 0, Animator)
       * `HatRenderer` (Sorting Order +10)
       * `EffectRenderer` (Sorting Order +15, Wisp Particle System)
2. **`ShadeStalker.prefab`**: Quái bóng đêm tuần tra/săn lùng.
3. **`SoulCollectible.prefab`**: Hạt linh hồn với ánh sáng hào quang phát quang.

### 🗺️ Scene (`Assets/_Game/Scenes/`)
* **`ArtSandbox.unity`**: Scene thử nghiệm đầy đủ với mặt nền xám đen tối giản, hệ thống hạt sương mù (fog), camera follow mượt mà, nhân vật chính, quái vật và 5 linh hồn thu thập được rải đều trên bản đồ.

---

## 🛡️ 4. Quy Tắc Dữ Liệu & Kiến Trúc V1
* **Single Silhouette:** Nhân vật giữ nguyên một hình dáng duy nhất, không biến dạng hay thay đổi tỷ lệ khi đổi trang bị.
* **Ngoại trang vĩnh viễn:** Mũ và cánh là dữ liệu mở khóa của Account (`unlocked_cosmetic_ids`), tuyệt đối không phải là item rơi rớt trong chuyến thám hiểm.
* **Độc lập Offline/Online:** Cấu trúc controller và event thu thập được thiết kế tách bạch, sẵn sàng để cắm Netcode / Server Authority ở các mốc phát triển tiếp theo.

