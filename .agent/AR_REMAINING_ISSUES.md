# AR Remaining Issues & System Status Report

> **Cập nhật**: 30/05/2026 (A2-A4, P0-P2, dead code x3, arch debt #1-#4, prereqs removed permanently)  
> **Mục đích**: Consolidate tất cả vấn đề còn tồn tại từ codebase exploration.  
> **Phạm vi**: 4 activities (QuantityMatch, CompareQuantity, NumberBonds, NumberLineJump) + app shell + AR services.

---

## 1. Dead Code & Technical Debt

### 1.2 Architecture Debt

| # | Issue | Current | Required | Trạng thái |
|---|---|---|---|---|
| 1 | **View files quá lớn** | QuantityMatchView ~1918, NumberLineJumpView ~1139, CompareQuantityView ~977 | Target <500 lines per file | ❌ Chưa làm — cần refactor lớn |
| 2 | **No unit tests** | Tất cả `Tests/` directories chỉ có `.gitkeep` | Cần test cho Presenter validation, state transitions | ❌ Chưa làm |

---

## 2. iOS Performance & Build Requirements

### 2.1 Performance Checklist

| # | Item | Trạng thái | Ghi chú |
|---|---|---|---|
| 1 | Object count clamping via `RuntimePerformanceSettings` | ⚠️ Cần verify | Phải gọi trong mọi Presenter trước spawn |
| 2 | Tắt plane detection sau placement | ❌ Chưa làm | `planeManager.enabled = false` khi `HasLearningArea == true` |
| 3 | Collider simplification | ⚠️ Cần verify | Tất cả prefab phải dùng SphereCollider/BoxCollider, không MeshCollider |
| 4 | `Application.targetFrameRate = 30` | ❌ Chưa set | Phải set trong BootLoader hoặc LearningSceneServices |
| 5 | `LateUpdate` cho fallback quad | ⚠️ Tồn tại | `ARSessionService.LateUpdate` thêm frame budget |

### 2.2 Build Settings

| # | Item | Required Value |
|---|---|---|
| 1 | Architecture | ARM64 |
| 2 | Scripting Backend | IL2CPP |
| 3 | Managed Stripping Level | Low |
| 4 | Camera Usage Description | "Ứng dụng cần truy cập camera để hiển thị nội dung thực tế ảo" |
| 5 | ARKit capability | Enabled (trong Xcode) |
| 6 | Development Build | ON (bắt buộc — đã disable toggle prerequisite lock) |

### 2.3 AR Error Handling

| # | Scenario | Required |
|---|---|---|
| 1 | User từ chối camera permission | Hiển thị dialog hướng dẫn vào Settings |
| 2 | Device không hỗ trợ ARKit | Hiển thị thông báo thân thiện |
| 3 | AR session loss | Show overlay + auto-recover |

---

## 3. Canvas Resolution ✅ ĐÃ ĐỒNG NHẤT

Tất cả activities hiện dùng `1920x1080` (landscape):
- `QuantityMatchView.cs:1535`: Đã sửa từ `1080x1920` → `1920x1080`.
- `ActivityRuntimeCanvas.cs:25`: Default `(1920f, 1080f)` — dùng bởi NumberBonds, CompareQuantity, NumberLineJump.

---

## 4. UI Consistent Issues

### 4.1 Feedback Colors Không Đồng Nhất

### 4.2 Feedback Auto-Hide Timing Không Đồng Nhất

### 4.3 NumberLineJump — Thiếu Panel Background Tint

Không giống CompareQuantity và NumberBonds, NumberLineJump chỉ đổi `feedbackText.color`, không tint panel background.

---

## 5. Hardcoded Strings (~56+ Strings)

Tất cả các View đều dùng hardcoded tiếng Việt thay vì `SimpleLocalization.Get()`:

| View | Số lượng | Ví dụ |
|---|---|---|
| QuantityMatchView | ~16 | "Chính xác! Con giỏi quá!", "Nhom X", "Tiep tuc" |
| CompareQuantityView | ~6 | "Ben trai", "Ben trai > < = ben phai?" |
| NumberBondsView | ~7 | "Tong", "Phan A", "Tim phan con thieu cua X" |
| NumberLineJumpView | ~8 | "Dang o: X", "Bat dau o X" |
| Shell (MainMenu/ActivitySelect/Progress) | ~19 | "Chon bai hoc", "Tien do hoc tap" |

---

## 6. Priority Action Plan

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
[P1] UIHintBubble (dead code)               → ✅ Đã xóa (backup in .agent/_deprecated/)
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
[C6] Standardize feedback colors
[C7] Feedback auto-hide timing
[C8] NumberLineJump panel background tint
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

---

## 7. Files Marked for Cleanup

Các file sau đã được consolidate vào báo cáo này và có thể xóa:

| File | Lý do |
|---|---|
| `.agent/UIUX_IMPROVEMENT_PLAN.md` | Nội dung đã được verify và merge vào báo cáo này |
| `.agent/AR_DEMO_GAP_LOG.md` | Gap log đã được tích hợp vào sections 2-8 |

**Giữ lại**:
- `.agent/AR_UNITY_ARCHITECTURE_PIPELINE.md` — Tài liệu kiến trúc tham chiếu
- `.agent/AR_UNITY_IOS_WORKFLOW_RULES.md` — Rules phát triển iOS
- `.agent/LESSON_01_QUANTITY_RECOGNITION.md` đến `LESSON_04_SEQUENCING.md` — Plan chi tiết từng lesson

---

*Tổng hợp: 30/05/2026 — từ codebase exploration. Sprint A+B fixes applied: IActivityView event, HighlightGroup, ShowMaxJumpsWarning, zone glow animation, feedback confetti/audio.*
