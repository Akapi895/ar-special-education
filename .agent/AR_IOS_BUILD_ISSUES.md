# iOS Build Issues — Root Cause Analysis & Fixes

> **Cập nhật**: 31/05/2026  
> **Phạm vi**: 4 activities (QuantityMatch, CompareQuantity, NumberBonds, NumberLineJump)  
> **Target**: iOS build (ARKit, IL2CPP, ARM64)

---

## Issue 1 — Characters Oversized / Zoomed on iOS

### Root Cause
**Không phải lỗi scale tuyệt đối, mà là do objects FOLLOW CAMERA** (Issue 3). Khi character di chuyển theo camera, chúng ở gần camera hơn dự định → trông to hơn. Ngoài ra còn có scale chồng chéo không cần thiết:

1. `ARPlacementService.SpawnAtPosition()` áp `iosScaleAdjustment = 0.6f` cho **mọi platform** (không có `#if UNITY_IOS`)
2. `ActivityPrefabSetup.NormalizeObjectHeight()` scale lại sau đó
3. `NumberBondsView.Visuals.FitObjectHeight()` scale lần thứ 3

### Fix Applied
- **File**: `ARPlacementService.cs:141` — `iosScaleAdjustment` chỉ áp dụng trên iOS (`#if UNITY_IOS`)
- **File**: `NumberBondsView.Visuals.cs:113` — xóa `FitObjectHeight()` vì `PrepareLearningObject` đã xử lý height normalization
- **File**: `ARPlacementService.cs:397` — đã fix riêng ở Issue 3

### Verify trên Editor
- Mở scene SC_TestSandbox hoặc SC_ARGameplay
- Spawn object, kiểm tra scale hierarchy: chỉ có 1 lần scale duy nhất (từ NormalizeObjectHeight)
- Confirm: không còn scale chồng chéo

---

## Issue 2 — Plane Detection Not Working on iOS

### Root Cause
`ARPlaneDetectionController.SetDetectionEnabled()` có method nhưng **CHƯA BAO GIỜ được gọi** bởi bất kỳ code nào. ARPlaneManager có thể được add component động (không pre-place trong scene), và `ResolveReferences` có fallback logic phức tạp.

### Fix Applied
- **File**: `ARServiceBootstrap.cs:148` — gọi `planeController.SetDetectionEnabled(true)` sau khi placement initialized
- **File**: `ARPlacementService.cs:317-330` — đơn giản hóa ARPlaneManager fallback: `FindAnyObjectByType` → `AddComponent` nếu null

### Verify trên Editor
- Kiểm tra console log khi scene load: `ARPlaneDetectionController` và `ARPlaneManager` phải được khởi tạo
- Plane visualization phải xuất hiện khi scan mặt phẳng (trong Editor simulation)

---

## Issue 3 — Characters Follow Camera Position

### Root Cause (CRITICAL)
`LearningAreaAnchor` được parent vào `ARPlacementService.transform`:
```csharp
anchorObject.transform.SetParent(transform, true);
```

Nếu `ARPlacementService` GameObject nằm trong hierarchy của camera/XROrigin trong scene, toàn bộ objects (zones, characters) di chuyển theo camera. User lia camera → objects bám dính.

### Fix Applied
- **File**: `ARPlacementService.cs:397` — `SetParent(transform, true)` → `SetParent(null)`
  
  LearningAreaAnchor và tất cả objects con giờ là world-root objects, độc lập với transform của ARPlacementService.

### Verify trên Editor
- Spawn objects trong activity
- Di chuyển camera (chuột phải drag trong Editor simulation)
- Objects phải **giữ nguyên vị trí world** khi camera di chuyển
- Trước đây: objects bám theo camera → sai. Sau fix: objects ở world space → đúng.

---

## Issue 4 — Sound Not Working on iOS

### Root Cause
`SimpleAudioManager.EnsureExists()` tạo `AudioSource` component dynamic với `spatialBlend = 1f` (mặc định của Unity). Trên iOS, `spatialBlend = 1f` = 3D positional audio → âm thanh chỉ nghe được khi AudioListener ở gần. Trong AR scene, listener ở camera nhưng audio source không được đặt vị trí đúng → không nghe thấy gì.

### Fix Applied
- **File**: `SimpleAudioManager.cs:180` — thêm `ConfigureAudioSource()` set `spatialBlend = 0f` (2D non-positional)
- Gọi `ConfigureAudioSource()` sau khi `EnsureAudioSources()` trong `Awake()`

### Verify trên Editor
- Click hint button, confirm button, listen button → phải nghe được beep
- Sử dụng prefab có AudioSource component → kiểm tra inspector thấy `Spatial Blend = 0` (2D)

---

## Files Changed Summary

| # | File | Thay đổi | Liên quan Issue |
|---|------|----------|----------------|
| 1 | `ARPlacementService.cs:397` | `SetParent(transform, true)` → `SetParent(null)` | Issue 3 (critical) |
| 2 | `ARPlacementService.cs:141` | Thêm `#if UNITY_IOS` guard | Issue 1 |
| 3 | `ARPlacementService.cs:37-39` | Comment giải thích iosScaleAdjustment | Issue 1 |
| 4 | `ARPlacementService.cs:317-330` | Đơn giản hóa ARPlaneManager fallback | Issue 2 |
| 5 | `ARServiceBootstrap.cs:148` | Gọi `SetDetectionEnabled(true)` | Issue 2 |
| 6 | `NumberBondsView.Visuals.cs:113` | Xóa `FitObjectHeight()` redundant | Issue 1 |
| 7 | `SimpleAudioManager.cs:180` | Thêm `spatialBlend = 0f` (2D audio) | Issue 4 |
| 8 | `SimpleAudioManager.cs:159-176` | `EnsureAudioSources()` + `ConfigureAudioSource()` | Issue 4 |

---

## Remaining Risks

1. **Audio**: Beep generation (`GetTemporaryFallbackClip`) tạo AudioClip procedural với `AudioClip.Create()`. Trên iOS, format này có thể không tương thích với audio hardware. Cần test build thật trên device.
2. **Plane detection**: Mặc dù `SetDetectionEnabled(true)` đã được gọi, việc ARKit có detect plane hay không còn phụ thuộc vào điều kiện ánh sáng và bề mặt real-world. Cần test device thật.
3. **XROrigin setup**: Nếu scene `SC_ARGameplay` không có XROrigin với AR Camera đúng, ARPlacementService có thể fallback sai. Cần verify.
4. **ActivityPrefabSetup**: `PrepareLearningObject` thay đổi scale object để fit target height (0.48m). Nếu prefab quá nhỏ hoặc quá lớn, scale có thể bị méo.
