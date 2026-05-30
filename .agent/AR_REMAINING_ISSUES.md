# AR Remaining Issues & System Status Report

> **Cập nhật**: 30/05/2026 (A2-A4, P0-P2 bugs đã sửa; lesson prerequisites disabled)  
> **Mục đích**: Consolidate tất cả vấn đề còn tồn tại từ codebase exploration.  
> **Phạm vi**: 4 activities (QuantityMatch, CompareQuantity, NumberBonds, NumberLineJump) + app shell + AR services.

---

## 0. Xác Nhận Kiến Trúc Đúng

### ✅ Character KHÔNG phải là con của Main Camera/AR Camera

Đã kiểm tra toàn bộ:
- **Scene files** (6 scenes): Tất cả camera đều có `m_Children: []` — không có Character nào là con của camera.
- **Prefab files**: XR Origin (AR Rig) prefab có Main Camera với children rỗng.
- **Runtime code**: JumpCharacter trong NumberLineJump được parent vào `placementService.LearningAreaContentRoot` (AR anchor area), không phải camera.
- **Không có** `SetParent` nào trỏ vào `Camera.main.transform` ngoại trừ fallback quad trong `ARSessionService.cs:300`.

**Kết luận**: Không tồn tại cấu trúc sai `Main Camera └── Character`. Nhân vật được đặt đúng trong learning area AR, độc lập với camera.

---

## 3. Dead Code & Technical Debt

### 3.1 Dead Code

| File | Lines | Lý do |
|---|---|---|
| `Core/UI/Components/UIHintBubble.cs` | 151 | Không được reference bởi bất kỳ code nào |
| `Core/UI/Navigation/UIScreenManager.cs` | 96 | Không được reference bởi bất kỳ code nào |
| `Core/UI/Navigation/UIScreen.cs` | — | Base class, không được dùng |

### 3.2 Architecture Debt

| # | Issue | Current | Required |
|---|---|---|---|
| 1 | **Static HintSystem** | `HintSystem` là static class, không cleanup | Chuyển thành service có lifecycle |
| 2 | **MaterialPropertyBlock không reuse** | Tạo block mới mỗi highlight → GC pressure | Cache instance field |
| 3 | **Bootstrap polling** | `Invoke("TryStartActivity", 0.5f)` chờ placement | Event-driven: subscribe `OnLearningAreaPlaced` |
| 4 | **No object pooling** | Instantiate/destroy mỗi round | `SimpleObjectPool` cho production |
| 5 | **View files quá lớn** | QuantityMatchView ~1918, NumberLineJumpView ~1139, CompareQuantityView ~977 | Target <500 lines per file |
| 6 | **No unit tests** | Tất cả `Tests/` directories chỉ có `.gitkeep` | Cần test cho Presenter validation, state transitions |

---

## 4. iOS Performance & Build Requirements

### 4.1 Performance Checklist

| # | Item | Trạng thái | Ghi chú |
|---|---|---|---|
| 1 | Object count clamping via `RuntimePerformanceSettings` | ⚠️ Cần verify | Phải gọi trong mọi Presenter trước spawn |
| 2 | Tắt plane detection sau placement | ❌ Chưa làm | `planeManager.enabled = false` khi `HasLearningArea == true` |
| 3 | Collider simplification | ⚠️ Cần verify | Tất cả prefab phải dùng SphereCollider/BoxCollider, không MeshCollider |
| 4 | `Application.targetFrameRate = 30` | ❌ Chưa set | Phải set trong BootLoader hoặc LearningSceneServices |
| 5 | `LateUpdate` cho fallback quad | ⚠️ Tồn tại | `ARSessionService.LateUpdate` thêm frame budget |

### 4.2 Build Settings

| # | Item | Required Value |
|---|---|---|
| 1 | Architecture | ARM64 |
| 2 | Scripting Backend | IL2CPP |
| 3 | Managed Stripping Level | Low |
| 4 | Camera Usage Description | "Ứng dụng cần truy cập camera để hiển thị nội dung thực tế ảo" |
| 5 | ARKit capability | Enabled (trong Xcode) |
| 6 | Development Build | ON (cho test) |

### 4.3 AR Error Handling

| # | Scenario | Required |
|---|---|---|
| 1 | User từ chối camera permission | Hiển thị dialog hướng dẫn vào Settings |
| 2 | Device không hỗ trợ ARKit | Hiển thị thông báo thân thiện |
| 3 | AR session loss | Show overlay + auto-recover |

---

## 5. Canvas Resolution ✅ ĐÃ ĐỒNG NHẤT

Tất cả activities hiện dùng `1920x1080` (landscape):
- `QuantityMatchView.cs:1535`: Đã sửa từ `1080x1920` → `1920x1080`.
- `ActivityRuntimeCanvas.cs:25`: Default `(1920f, 1080f)` — dùng bởi NumberBonds, CompareQuantity, NumberLineJump.

---

## 6. UI Consistent Issues

### 6.1 Feedback Colors Không Đồng Nhất

| Activity | Correct color | Incorrect color |
|---|---|---|
| QuantityMatch | Green `Color.green` | Orange `(1.0, 0.5, 0)` |
| CompareQuantity | Green `Color.green` | Red `Color.red` |
| NumberBonds | Green | Red |
| NumberLineJump | Green | Red |

### 6.2 Feedback Auto-Hide Timing Không Đồng Nhất

| Activity | Correct auto-hide | Incorrect auto-hide |
|---|---|---|
| QuantityMatch | 2s | 1.5s |
| CompareQuantity | 1.8s (overlay) | 1.2s (overlay) |
| NumberBonds | None | None |
| NumberLineJump | None | None |

### 6.3 NumberLineJump — Thiếu Panel Background Tint

Không giống CompareQuantity và NumberBonds, NumberLineJump chỉ đổi `feedbackText.color`, không tint panel background.

---

## 7. Hardcoded Strings (~56+ Strings)

Tất cả các View đều dùng hardcoded tiếng Việt thay vì `SimpleLocalization.Get()`:

| View | Số lượng | Ví dụ |
|---|---|---|
| QuantityMatchView | ~16 | "Chính xác! Con giỏi quá!", "Nhom X", "Tiep tuc" |
| CompareQuantityView | ~6 | "Ben trai", "Ben trai > < = ben phai?" |
| NumberBondsView | ~7 | "Tong", "Phan A", "Tim phan con thieu cua X" |
| NumberLineJumpView | ~8 | "Dang o: X", "Bat dau o X" |
| Shell (MainMenu/ActivitySelect/Progress) | ~19 | "Chon bai hoc", "Tien do hoc tap" |

---

## 8. Priority Action Plan

### Sprint A (Blocker P0/P1) — ✅ Đã hoàn thành

```
[P0] NumberLineJump equation format       → ✅ Code đã đúng từ đầu
[P0] NumberLineJump jump direction enforce → ✅ Code đã đúng từ đầu
[P1] CompareQuantity IActivityView event   → ✅ Đã wire onAnswerSelectedInterface
[P1] NumberLineJump config Resources path  → 🟢 Runtime fallback đủ dùng
```

✅ **Đã sửa thêm**: Canvas resolution A2, UpdateButtonLabels rename A3, dead buttons A4.

### Sprint B (Demo-ready UX) — ✅ Đã hoàn thành

```
[P1] CompareQuantity: HighlightGroup()     → ✅ Đã implement highlight text + scale
[P1] NumberLineJump: ShowMaxJumpsWarning() → ✅ Đã hiển thị trên feedback panel + auto-hide
[P1] CompareQuantity: confetti/scale/audio → ✅ Đã thêm qua FeedbackServiceProxy + SimpleAudioManager
[P1] NumberLineJump: audio feedback         → 🟡 Một phần (step sound còn thiếu)
[P1] NumberBonds: zone glow animation      → ✅ Đã thêm PulseCoroutine
[P2] Particle warning (QuantityMatch)       → ✅ Đã thêm procedural confetti fallback
[P2] SymbolCompare question type           → ✅ Đã thêm QuestionType enum + display
[P2] Compose/MissingPart behavior          → ✅ Đã thêm FromQuestion cases + ValidateCurrentState
[P1] UIHintBubble: Tích hợp vào Views      → ❌ Chưa làm
[P2] Standardize feedback colors           → ❌ Chưa làm
[P2] Tăng feedback auto-hide timing        → ❌ Chưa làm
[P2] NumberLineJump: panel background tint → ❌ Chưa làm
```

### Sprint C (Accessibility + Localization)

```
[C1] Thêm ~56 keys vào SimpleLocalization
[C2] Replace hardcoded strings với SimpleLocalization.Get()
[C3] Thêm ✅/❌ icons cạnh text feedback
[C4] iOS safe area support (Screen.safeArea)
[C5] Color-blind friendly mode
[C6] UIHintBubble integration
[C7] Standardize feedback colors
[C8] Feedback auto-hide timing
[C9] NumberLineJump panel background tint
```

### Sprint D (iOS Build)

```
[D1] Set Application.targetFrameRate = 30
[D2] Tắt plane detection sau placement
[D3] Verify collider simplification (no MeshCollider)
[D4] Verify RuntimePerformanceSettings clamping
[D5] Build iOS + device test
[D6] Memory test: 10 rounds liên tiếp
```
