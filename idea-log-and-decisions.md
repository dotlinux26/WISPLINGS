# 📓 SỔ TAY GHI CHÉP Ý TƯỞNG & QUYẾT ĐỊNH THIẾT KẾ (DEV NOTES & DECISIONS)

> **Loại tài liệu:** File ghi chép phụ / Sổ tay quyết định của Creator (Anh) trước khi setup Repo.  
> **Tài liệu gốc liên quan:** `ghost-survival-game-design.md` *(Tài liệu thiết kế chính - Giữ nguyên, không chỉnh sửa)*.  
> **Ngày khởi tạo:** 04/11/2026.  
> **Trạng thái:** Pre-production / Đã chốt các quyết định cốt lõi.

---

## 1. TỔNG QUAN TẦM NHÌN DỰ ÁN (CREATOR'S VISION)

* **Tên dự án (Working Title):** Ghost Survival / Ghoster
* **Thể loại:** Co-op Atmospheric Survival / Roguelite Expedition / Cute & Eerie 2D
* **Góc nhìn (Camera):** Top-down (hỗ trợ di chuyển 4 hướng)
* **Quy mô chơi:** 1–4 người (Hỗ trợ chơi Đơn Solo hoặc Rủ bạn bè vào phòng Co-op)
* **Triết lý thiết kế trung tâm:**
  > *"Một chú ma đen nhỏ xíu, mắt to tròn đáng yêu đang tìm cách sinh tồn trong một thế giới rộng lớn, ảm đạm, sương mù bao phủ và đầy rẫy những kẻ săn mồi bí ẩn."*

---

## 2. PHÂN TÍCH Ý TƯỞNG TỪ MOODBOARD THAM KHẢO

Từ các tư liệu hình ảnh tham khảo mà Anh đã lựa chọn:

1. **Cảm hứng Pac-man (Hạt sáng & Ăn quả):**
   * Cơ chế nuốt các hạt linh hồn (Soul orbs), bụi sương sáng và hái quả rừng để no bụng, hồi phục và tích lũy cấp độ.
   * Nhân vật ma có dáng vẻ bo tròn mềm mại, biểu cảm qua đôi mắt trắng to tròn.
2. **Không khí Nghĩa địa & Rừng sương (Graveyard / Foggy Forest):**
   * Nền thế giới chủ đạo là tông đơn sắc (đen, trắng, xám tro, tím than u tối).
   * Ánh sáng là thứ quý giá: nến mờ, đốm sáng linh hồn phát quang, hoa quả kỳ lạ phát sáng.
3. **Chất nghệ thuật kết hợp Don't Starve + Hollow Knight + Cute Indie:**
   * Thế giới cô độc, sương mù giăng kín cả ngày lẫn đêm. Ban đêm kéo dài hơn và nguy hiểm gấp bội.
   * Nhân vật chính là một bóng đen nhỏ bé, đi kèm một chú thú cưng (Pet) nhỏ xinh có vầng sáng yếu ớt soi đường.

---

## 3. CÁC QUYẾT ĐỊNH GAMEPLAY CỐT LÕI (CORE GAMEPLAY DECISIONS)

### 3.1. Cơ chế Sinh tồn của Ma (Ghost Survival Mechanics)
* **Dù là ma nhưng vẫn có nhu cầu sinh tồn:**
  * **HP (Trái tim / Năng lượng sinh tồn):** Giảm khi bị quái tấn công hoặc rơi vào vùng ô nhiễm tà khí.
  * **Stamina (Thể lực):** Tiêu tốn khi Dash / Chạy nhanh né quái. Tự hồi khi đi chậm hoặc đứng yên.
  * **Hunger (Độ đói / Năng lượng linh hồn):** Giảm dần theo thời gian. Ma phải đi nuốt hạt linh hồn, hạt sương sáng hoặc ăn quả lạ để duy trì.
  * *Quyết định quan trọng:* Đói về 0 sẽ không lăn đùng ra chết ngay (tránh ức chế), mà sẽ làm suy yếu tốc độ di chuyển, không hồi được Stamina và từ từ rút kiệt HP.
* **Chu kỳ Ngày – Đêm & Sương mù:**
  * Ban ngày: Vẫn có sương mù nhẹ nhưng tầm nhìn xa hơn, quái ít hung hăng hơn, tài nguyên cơ bản rải rác.
  * Ban đêm (Đêm dài hơn): Quái săn mồi hoạt động mạnh, tầm nhìn co cụm lại, nhưng bù lại xuất hiện linh hồn hiếm và kho báu giá trị.
  * *Yếu tố ánh sáng hai mặt:* Bật đèn hoặc mang đồ phát sáng giúp nhìn xa nhưng lại thu hút quái vật chú ý.

### 3.2. Hệ thống Kẻ thù & Quái vật (Threats & Mobs)
1. **Quái cây (Rooted / Treant-like):** Ngụy trang thành cây khô hoặc bụi rậm, di chuyển chậm nhưng đòn đánh nặng nề, bảo vệ các bụi quả hiếm.
2. **Thú săn mồi bóng đêm (Hunters / Stalkers):** Nhạy cảm với tiếng bước chân và ánh sáng, rình rập và lao vào tấn công khi người chơi mất cảnh giác.
3. **The Hollow (Bọn linh hồn biến chất - Giống mình nhưng ăn thịt mình):**
   * Điểm độc đáo: Có hình dáng tương tự người chơi / đồng đội.
   * Có thể lén lút đi theo sau lưng, giả vờ là linh hồn vô hại rồi bất ngờ tấn công (tạo cảm giác giật mình và nghi ngờ lẫn nhau khi chơi co-op).

---

## 4. QUYẾT ĐỊNH CƠ CHẾ CHẾT & TIẾN TRÌNH 2 TẦNG (DEATH PENALTY & PROGRESSION)

### 4.1. Vòng lặp chuyến thám hiểm (Expedition Loop)
* **The Refuge (Nơi Trú Ẩn):** Căn cứ an toàn chung, nơi chuẩn bị đồ đạc, chọn pet, thay đổi skin mũ/cánh, quản lý hòm đồ vĩnh viễn và tạo/tham gia phòng.
* **Expedition (Vào Map Sinh Tồn):** Người chơi bước vào bản đồ mở, cùng thu thập tài nguyên, né quái, lên cấp tạm thời và tự quyết định thời điểm "Rút lui an toàn" để đem của cải về Refuge.

### 4.2. Phân chia 2 loại Level
| Cấp độ | Phạm vi hoạt động | Khi chết / Kết thúc chuyến đi | Tác dụng |
|---|---|---|---|
| **Expedition Level** | Trong phiên chơi hiện tại | **Reset về 1** | Tăng tốc độ, mở kỹ năng lướt nhanh, buff chỉ số tức thời để sinh tồn qua đêm. |
| **Soul Level (Account)** | Toàn bộ tài khoản | **Vĩnh viễn (Không mất)** | Mở khóa công thức chế tạo, mở ô chứa đồ tại Refuge, tăng chỉ số nền cơ bản. |

### 4.3. Cơ chế Chết "Vừa sợ mất đồ vừa không nản"
* **Khi người chơi chết trong map:**
  * Rớt lại một **Túi linh hồn (Soul Bag)** chứa toàn bộ tài nguyên, đồ ăn, vật phẩm nhặt được trong chuyến đi đó.
  * **Trong Co-op:** Đồng đội còn sống có thể chạy lại nhặt hộ túi hoặc cứu sống đồng đội.
  * **Trong Solo:** Người chơi có 1 cơ hội quay lại xác cũ để nhặt lại túi (nếu chết lần nữa thì mất hẳn).
* **Tài sản vĩnh viễn tuyệt đối an toàn:**
  * Mũ, Cánh, Skin ngoại trang.
  * Cấp độ tài khoản (Soul Level).
  * Pet (Thú cưng không bao giờ chết vĩnh viễn, chỉ tạm biến về Refuge khi chủ gục ngã).

---

## 5. QUYẾT ĐỊNH VỀ ART, NGOẠI TRANG & PET (VISUAL & COSMETICS)

### 5.1. Nguyên tắc 1 Body duy nhất (Single Body Silhouette)
* Để đảm bảo tính nhận diện và đồng nhất tuyệt đối, nhân vật chính **chỉ có 1 hình dáng cơ thể chuẩn** (Bé ma đen bo tròn mềm, mắt trắng to, animation lơ lửng phập phồng squash & stretch).
* Không vẽ hàng chục loại nhân vật khác nhau gây phân mảnh code animation.

### 5.2. Kỹ thuật ghép lớp (Modular Layering Architecture)
Ngoại trang được hiển thị theo hệ thống lớp (Layer Sorting) chuẩn xác trong Unity:

```
[Ghost Character Hierarchy]
├── Layer -10: BackRenderer   --> Cánh (Wings), Áo choàng sau, Hiệu ứng phía sau
├── Layer 0:   BodyRenderer   --> Thân ma đen mắt trắng (Animation 4 hướng Idle / Walk)
├── Layer +10: FrontRenderer  --> Mũ (Witch Hat, Crown, Nightcap), Khăn quàng, Kính
└── Layer +15: EffectRenderer --> Vầng sáng lửa ma (Will-o'-the-wisp), Hào quang, Bụi sáng
```

* **Phân định rõ ràng:** Ngoại trang (Cosmetics) là **thành tựu vĩnh viễn của tài khoản**, lưu trong danh sách `unlocked_cosmetics[]`, không phải là đồ vật trong balo sinh tồn để bị rớt mất khi chết.

### 5.3. Hệ thống Thú Cưng (Pets)
* Pet đi theo sau người chơi, có animation nảy lơ lửng đồng điệu.
* Pet không tham gia đánh nhau trực tiếp mà mang lại tiện ích sinh tồn:
  * *Wisp (Đốm sáng):* Phát sáng nhẹ vùng xung quanh, chỉ điểm hạt linh hồn gần đó.
  * *Moth (Bướm đêm):* Đập cánh cảnh báo khi quái vật đang tiến lại gần trong sương.
  * *Bunny/Blob:* Đánh hơi thức ăn, nấm và hoa quả rừng.

### 5.4. Quy chuẩn kỹ thuật đồ họa
* **Gốc Vector (SVG):** Dễ tinh chỉnh nét vẽ, chuyển màu và tỉ lệ độ phân giải mà không vỡ hình.
* **Quy chuẩn đưa vào Unity:**
  * Cell size: **128 × 128 px**
  * Pixels Per Unit (PPU): **128**
  * Sprite Mode: **Multiple** (Grid by Cell Size 128x128)
  * Pivot nhân vật: **(0.5, 0.18)** (giữ trọng tâm bồng bềnh sát đáy)

---

## 6. QUYẾT ĐỊNH KIẾN TRÚC MẠNG & MULTIPLAYER (NETWORKING DECISIONS)

### 6.1. Giải quyết bài toán Host / VPN / Quản lý phòng
* **Quyết định dứt khoát:** **KHÔNG** bắt người chơi cài đặt VPN giả lập mạng LAN hay tự chỉnh port-forwarding (quá phức tạp và dễ lỗi).
* **Mô hình kiến trúc:** **Centralized Lobby & Room-Code Architecture**
  1. **Máy chủ điều phối (Matchmaking / Relay Lobby):** Người chơi bấm "Tạo phòng" -> Server cấp một **Mã phòng (Room Code ví dụ: `GHOST-8822`)** + Mật khẩu tùy chọn.
  2. Bạn bè chỉ cần nhập Room Code là join ngay lập tức qua mạng Internet bình thường.
  3. **Đồng bộ hóa dữ liệu (Data Authority):**
     * Dữ liệu tài khoản (Soul Level, Skin đã mở, Pet, Hòm đồ Refuge) được lưu trên Database tập trung (Cloud/Server Profile).
     * Bất kể ai làm chủ phòng, dữ liệu cá nhân của từng người chơi luôn được bảo toàn độc lập.
     * Khi chơi Solo: Vẫn dùng chung logic đó nhưng chạy Local Server hoặc Single-player instance để có thể chơi mượt mà kể cả khi rớt mạng.

---

## 7. LỘ TRÌNH TRIỂN KHAI CHO REPO (ACTION PLAN TRƯỚC KHI CODE)

* [x] **Bước 1:** Chốt toàn bộ ý tưởng & quyết định thiết kế vào file này (`idea-log-and-decisions.md`).
* [ ] **Bước 2:** Cấu hình Git Repository sạch sẽ (setup `.gitignore` chuẩn cho Unity 2D, Git LFS cho asset đồ họa).
* [ ] **Bước 3:** Tổ chức thư mục dự án Unity:
  * `Assets/_Game/Art/` (SVG, Spritesheets 128x128, Materials)
  * `Assets/_Game/Scripts/Core/` (Player Controller, Stats HP/Stamina/Hunger)
  * `Assets/_Game/Scripts/Environment/` (Fog of war, Day-Night cycle, Collectibles)
  * `Assets/_Game/Scripts/Cosmetics/` (Layer sorting manager, Hat/Wings equip)
  * `Assets/_Game/Scripts/Network/` (Room management, State sync)
* [ ] **Bước 4:** Xây dựng Prototype V1 (Bé ma di chuyển 4 hướng mượt mà + Nhặt hạt sáng + 1 quái đuổi theo + Chu kỳ ngày đêm).

---
*Tài liệu này là căn cứ quyết định để đối chiếu trong toàn bộ quá trình phát triển dự án.*



## 8. NHẬT KÝ KHẮC PHỤC SỰ CỐ KỸ THUẬT V1 (BUG FIX & OPTIMIZATION LOG)

### 8.1. Khắc phục triệt để lỗi "Animation tùm lum khi di chuyển" (Jumbled Directional Animation)
* **Nguyên nhân gốc rễ (Root Cause):**
  1. Trong file `.meta` của các sprite sheet (`ghost_main_8x4.png.meta`, `cosmetic_...`), bảng `internalIDToNameTable` bị tích lũy rác từ các lần import tự động trước đó (chứa tới 80-96 sprite ID ảo như `ghost_main_8x4_0..47` thay vì chỉ 32 slice chuẩn).
  2. Hàm `AssetDatabase.LoadAllAssetsAtPath` trả về các sub-asset theo thứ tự băm FileID ngẫu nhiên của Unity chứ không theo thứ tự lưới hay bảng chữ cái.
  3. Mảng `bodySprites` nhận các sprite lộn xộn khiến index `dir * 8 + state + frame` lấy sai frame (ví dụ: đang đi Left nhưng lại nạp nhầm frame Idle Up hoặc frame bị cắt lệch), kéo theo hệ thống đồng bộ Mũ, Cánh, Tay cũng bị trỏ sai tên.
* **Biện pháp xử lý toàn diện:**
  1. **Dọn sạch metadata:** Tạo công cụ `tools/clean_meta_tables.py` lọc sạch toàn bộ các FileID rác trong `internalIDToNameTable` của tất cả các file `.meta`, chỉ giữ lại đúng 32 frame chuẩn khớp 100% với `spriteSheet.sprites`.
  2. **Truy vấn đích danh (Exact Name-based Mapping):** Thay đổi `GhostAnimator.cs` và `CosmeticSynchronizer.cs` sang cơ chế `Dictionary<string, Sprite>` tra cứu trực tiếp theo chuỗi tên chính quy:
     `$"Ghost_{direction}_{(isMoving ? "Walk" : "Idle")}_{currentFrame}"`.
     Nhờ đó, dù mảng có bị xáo trộn thế nào thì code cũng CHỈ chọn đúng frame tương ứng, loại bỏ 100% nguy cơ nhầm lẫn.
  3. **Xếp mảng chuẩn quy phạm (Canonical Ordering):** Trong `GhostAtlasImporter.cs`, hàm `LoadOrderedSpritesList` nạp đúng 32 slot theo thứ tự 4 hướng x 8 frame (Down Idle/Walk -> Left Idle/Walk -> Right Idle/Walk -> Up Idle/Walk) trước khi serialize vào Prefab.
  4. **Triệt tiêu cảnh báo `Parameter Hash -1781152096 does not exist`:** Thêm parameter `IsChasing` (Bool) vào `ShadeStalkerController.controller` và bổ sung kiểm tra an toàn trong `ShadeStalker.cs`.

### 8.2. Học tập cách vẽ dáng nhân vật theo ảnh mẫu (Shape Study & Vector Bézier Crafting)
* **Yêu cầu & Thử nghiệm nghệ thuật:**
  1. Đã nghiên cứu giải phẫu khối hình học (Shape Anatomy) và thử nghiệm dựng các dáng Bézier thuần vector (tay cụt mái chèo, chân ma tí hon lơ lửng).
  2. **Quyết định chính thức của Creator (Anh):** Giữ nguyên thiết kế nhân vật gốc (Bản V1 hiện tại) đã ổn định trong game và hoàn tất sửa lỗi animation, không thay đổi shape nhân vật chính nữa để tập trung vào gameplay và các tính năng tiếp theo.
  3. Các file vector nghiên cứu hình học được lưu lại trong thư mục `tools/` và `ArtSource/.../concepts/` để làm tư liệu tham khảo sau này nếu cần thiết.
