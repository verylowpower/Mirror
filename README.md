# ⚔️ MIRROR

## 🎮 Giới thiệu
Đây là một trò chơi **survivor-like** (giống Vampire Survivors) được phát triển bằng **Unity**.  
Người chơi điều khiển nhân vật chính, di chuyển liên tục để né tránh kẻ địch và sử dụng nhiều loại **đạn phép thuật** để tiêu diệt chúng.  

Mục tiêu: **Sống sót càng lâu càng tốt** trước các đợt kẻ địch xuất hiện theo từng Wave.

---

## 🕹️ Gameplay
- Người chơi di chuyển nhân vật để tránh né và tấn công.
- Nhân vật có thể bắn ra các loại **đạn khác nhau**:
  - 🔥 **Fire Bullet**: gây sát thương kèm hiệu ứng **Burn** theo thời gian.  
  - ❄️ **Ice Spell**: làm chậm kẻ địch.  
  - ⚡ **Lightning Buff**: khi áp dụng, đạn có thể **lan sang kẻ địch khác** (chain lightning).  
  - 🟢 **Normal Bullet**: đạn thường, sát thương cơ bản.  

- Các loại nâng cấp có thể **kết hợp Buff** để tạo hiệu ứng đặc biệt.

- Kẻ địch sẽ **xuất hiện theo Wave**:
  - Mỗi Wave có thể chứa nhiều loại kẻ địch khác nhau.
  - Số lượng, tốc độ, và độ khó tăng dần.
  - Giữa các Wave có thời gian nghỉ ngắn.

---

## 📖 Cách chơi
1. Khởi động game → Nhân vật xuất hiện ở giữa bản đồ.
2. Nhân vật sẽ tự động bắn đạn theo hướng di chuyển hoặc theo logic auto-fire.
3. Tránh kẻ địch, tiêu diệt chúng để sống sót lâu nhất.
4. Thu Buff để tăng sức mạnh (Fire, Ice, Lightning).
5. Càng về sau, kẻ địch xuất hiện nhiều hơn, nhanh hơn và khó hơn.

---

## 🚀 Hướng phát triển tương lai
- Thêm nhiều loại enemy với hành vi AI khác nhau.
- Hệ thống Boss theo mốc thời gian.
- Nâng cấp nhân vật và chọn Buff khi lên cấp.
- Hiển thị bảng điểm / thời gian sống sót.
- Tối ưu hiệu năng với **Object Pooling** cho Bullet & Enemy.

---

## 🛠️ Công nghệ
- **Unity Engine**
- **C#**
- Hệ thống **Factory Pattern** cho Bullet
- **Spatial Partitioning** để tối ưu va chạm






