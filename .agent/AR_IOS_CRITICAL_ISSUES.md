# iOS Build — Remaining Issues & Build Guide

> **Cập nhật**: 31/05/2026  
> **Mục đích**: Các lỗi còn tồn tại + hướng dẫn build iOS + cách capture log

---

## 1. Remaining Unfixed Issues

### Sprint 3 (Polish — chưa implement)

| # | Issue | Root Cause | File | Priority |
|---|---|---|---|---|
| 1 | **Object pooling chưa tích hợp** | `ObjectPoolManager` đã tạo file riêng, `ARPlacementService.ClearSpawnedObjects` dùng `ObjectPoolManager.Release` nhưng activities chưa dùng pooling cho spawn | `ARPlacementService.cs` / `ObjectPoolManager.cs` | 🟢 LOW |
| 2 | **AR session loss overlay** | Khi AR tracking lost, không có visual indicator cho người dùng. `OnSessionLost` event đã fire nhưng không có UI overlay | `ARSessionService.cs` | 🟢 LOW |
| 3 | **Tracking quality indicator** | Không có HUD indicator cho chất lượng tracking (None/Poor/Fair/Good/Excellent) | `ARSessionService.cs` + UI mới | 🟢 LOW |
| 4 | **Haptic feedback trên iOS** | Không có haptic khi trả lời đúng/sai, không có trong codebase | — | 🟢 LOW |
| 5 | **Camera permission denied handling** | Khi user từ chối camera, không có UI hướng dẫn vào Settings | `ARSessionService.cs` | 🟡 MEDIUM |

### Build Settings — Cần verify thủ công

| Setting | Required Value | Location | Why |
|---|---|---|---|
| Architecture | **ARM64** | Player Settings → iOS | Apple yêu cầu ARM64 từ 2023 |
| Scripting Backend | **IL2CPP** | Player Settings → iOS | Mono không support iOS 13+ |
| Managed Stripping Level | **Low** | Player Settings → iOS | Medium/High strip ARFoundation code |
| Camera Usage Description | **"Ứng dụng cần truy cập camera..."** | Player Settings → iOS | Bắt buộc cho ARKit |
| ARKit capability | **Enabled** | Xcode project | Bắt buộc cho ARFoundation |
| Target SDK | **Latest iOS** | Player Settings → iOS | Tận dụng ARKit mới nhất |
| Metal API Support | **Enabled** | Player Settings → iOS | Mặc định, verify |
| Development Build | **ON** | Build Settings | Cho phép log console |

---

## 2. Hướng dẫn Build iOS

### Bước 1: Trong Unity Editor

- [ ] **Player Settings → iOS → Architecture**: ARM64
- [ ] **Player Settings → iOS → Scripting Backend**: IL2CPP
- [ ] **Player Settings → iOS → Managed Stripping Level**: Low
- [ ] **Player Settings → iOS → Camera Usage Description**: "Ứng dụng cần truy cập camera để hiển thị nội dung thực tế ảo"
- [ ] **Build Settings → Development Build**: ON
- [ ] **Build Settings → Build** → chọn thư mục Xcode project

### Bước 2: Trong Xcode

- [ ] **Signing & Capabilities → + → ARKit**: Enabled
- [ ] **Build Settings → iOS Deployment Target**: Latest (iOS 17+)
- [ ] **Run** (Build & Run) trên device thật

---

## 3. Cách Capture Log từ iOS

### Method 1: Xcode Console (recommended)
1. Mở Xcode project
2. Run app trên device
3. Mở **Debug Area** (View → Debug Area → Show Debug Area)
4. Filter log với `[` prefix:
   ```
   [BootLoader]          — startup sequence
   [ARSessionService]    — AR session lifecycle & permissions
   [ARPlacementService]  — plane detection, spawn, fallback
   [ARPlacementController] — tap-to-place
   [SimpleAudioManager]  — audio playback
   [SceneTransitionManager] — scene loading
   [MainMenuController]  — main menu
   [QuantityMatchPresenter] — activity logic (4 activities)
   [NumberLineJumpPresenter]
   [CompareQuantityPresenter]
   [NumberBondsPresenter]
   ```

### Method 2: Unity Console (Development Build)
- Kết nối device qua USB
- Mở Window → Analysis → Console (trong Unity Editor)
- Chọn tab **Device** và chọn iOS device

### Method 3: Log file export
- Các log tự động ghi vào `Application.persistentDataPath` (nếu có ProgressStorage)
- Truy cập qua Xcode → Window → Devices and Simulators → chọn device → Download Container

---

## 4. Quick Test Checklist on iOS

### Pre-flight
- [ ] Clean build (Build → Clean Build Folder trong Xcode)
- [ ] Development Build = ON
- [ ] Architecture = ARM64
- [ ] Scripting Backend = IL2CPP

### Runtime
- [ ] App launch không crash (check `[BootLoader] App STARTED`)
- [ ] Camera permission dialog xuất hiện (check `[ARSessionService]`)
- [ ] AR session tracking (check `SessionTracking`)
- [ ] Plane detection (check `[ARPlacementService] Valid horizontal plane FOUND`)
- [ ] Mascot hiển thị (check `[MainMenuController]`)
- [ ] Chọn activity, chơi 3 rounds (check presenter logs)
- [ ] Audio playback (check `[SimpleAudioManager] Playing`)
- [ ] Scene transition (check `[SceneTransitionManager]`)
- [ ] 10 rounds memory test (không crash, không OOM)

---

*Cập nhật: 31/05/2026 — remaining issues + build instructions*
