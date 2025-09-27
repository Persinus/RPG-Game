# 🎮 MMORPG 2D Pixel – Kế hoạch phát triển (5 tháng, solo dev)

![Unity](https://img.shields.io/badge/Unity-2022+-black?logo=unity)
![Photon Fusion](https://img.shields.io/badge/Photon-Fusion-blue)
![ASP.NET Core](https://img.shields.io/badge/Backend-ASP.NET_Core-512BD4?logo=dotnet)
![MongoDB](https://img.shields.io/badge/Database-MongoDB-47A248?logo=mongodb)
![AdMob](https://img.shields.io/badge/Ads-Google_AdMob-FF0000?logo=google)

---

## 📌 Giới thiệu
Dự án game **MMORPG 2D pixel art** phát triển bằng **Unity**, backend **ASP.NET Core + MongoDB**, hỗ trợ multiplayer qua **Photon Fusion**.  
🎯 Mục tiêu: tạo bản **demo có thể chơi được** với đăng nhập, nhân vật, quái, nhiệm vụ và lưu trữ dữ liệu.

---

## ⚙️ Công nghệ chính
- **Unity 2D** (pixel art, tilemap, animation)  
- **Photon Fusion** (multiplayer real-time)  
- **ASP.NET Core Web API** (backend)  
- **MongoDB** (database)  
- **Google AdMob** (test quảng cáo)  

---

## 🎯 Phạm vi (MVP)
✅ Đăng nhập, tạo nhân vật  
✅ Di chuyển, đánh quái, nhặt vật phẩm  
✅ Kho đồ (inventory)  
✅ Level, EXP, chỉ số nhân vật  
✅ Chat + bubble chat  
✅ Nhiệm vụ cơ bản (quest)  
❌ Không làm: bang hội, giao dịch, PvP phức tạp  

---

## 📅 Kế hoạch 5 tháng (20 tuần)

### **Tháng 1 – Nền tảng & Thiết kế**
- Tạo project, kết nối MongoDB, repo Git  
- Viết Use Case + thiết kế DB  
- Tìm asset, tạo map thử nghiệm  
- Điều khiển nhân vật + lưu local  

### **Tháng 2 – Gameplay cốt lõi**
- Quái vật & chiến đấu cơ bản  
- Inventory (UI + logic)  
- Hệ thống rớt vật phẩm  
- Level + chỉ số nhân vật  

### **Tháng 3 – Multiplayer & Lưu trữ**
- Kết nối Photon Fusion  
- Chat & bubble chat  
- API ASP.NET Core (auth, lưu nhân vật)  
- Đồng bộ game client ↔ backend  

### **Tháng 4 – Nội dung & Cân bằng**
- Hệ thống nhiệm vụ  
- Thêm map mới, quái, mini boss  
- Cân bằng chỉ số, tỉ lệ rớt  
- Cải thiện UI, âm thanh, bug fix  

### **Tháng 5 – Test & Xuất bản**
- Mời tester, sửa bug  
- Tích hợp quảng cáo test  
- Build APK + quay video demo  
- Viết báo cáo + slide  

---

## 📌 Mốc nghiệm thu
| Mốc | Thời gian | Nội dung |
|-----|-----------|----------|
| **M1 (Tuần 4)** | Cuối Tháng 1 | Project + tài liệu phân tích |
| **M2 (Tuần 8)** | Cuối Tháng 2 | Gameplay đơn (quái, inventory, level) |
| **M3 (Tuần 12)** | Cuối Tháng 3 | Multiplayer + backend |
| **M4 (Tuần 16)** | Cuối Tháng 4 | Quest + map mở rộng + cân bằng |
| **M5 (Tuần 20)** | Cuối Tháng 5 | Bản build cuối + báo cáo |

---

## 📝 Ghi chú
- Nếu quá tải → giảm phạm vi (2 map, 2 loại quái, 1–2 nhiệm vụ).  
- Ưu tiên tính năng **core gameplay** trước để có demo chạy được.  
- Báo cáo tập trung vào **quy trình phát triển + kiến trúc**, không phải số lượng feature.  
