# AR Demo Gap Log — Toàn bộ vấn đề cần xử lý để test Editor & build iOS

> **Ngày tổng hợp**: 29/05/2026  
> **Mục tiêu**: Chuẩn bị 4 bài (QuantityMatch → CompareQuantity → NumberBonds → NumberLineJump) sẵn sàng test trên Unity Editor và build iOS hoàn chỉnh.  
> **Phạm vi**: Bugs hiện có + missing features + iOS performance + architecture compliance.

---

## Tổng quan trạng thái

| Activity | LOC | P0 Complete | Bugs | Editor-ready? | iOS-ready? |
|---|---|---|---|---|---|
| QuantityMatch | ~3,440 | ~75% | 2 (warning) | ⚠️ Cần fix | ⚠️ Chưa test device |
| CompareQuantity | ~2,539 | ~70% | 2 (functional) | ❌ Cần fix bugs | ❌ Chưa test device |
| NumberBonds | ~2,517 | ~85% | 0 (functional) | ✅ Sẵn sàng | ⚠️ Thiếu zone animation |
| NumberLineJump | ~2,358 | ~75% | 3 (2 HIGH) | ❌ Cần fix bugs | ❌ Chưa test device |

---

## PHASE 1: SỬA BUGS FUNCTIONAL — Blocker cho Editor Test

Các lỗi này khiến activity không hoạt động đúng logic học tập hoặc gây silent fail. Cần fix trước khi test Editor.

### 1.1 CompareQuantity — `UpdateButtonLabels` bỏ qua tham số config

- **File**: `CompareQuantityView.cs` (dòng ~322-325)
- **Mức độ**: P0 (functional bug)
- **Vấn đề**: Method `UpdateButtonLabels(moreLabel, fewerLabel, equalLabel)` nhận tham số từ config nhưng **không dùng đến**, luôn gọi `ApplyComparisonAnswerButtonVisuals()` hiển thị ký hiệu toán học thuần (`>`, `<`, `=`). Các label tiếng Việt từ config (`Nhiều hơn`, `Ít hơn`, `Bằng nhau`) bị bỏ qua.
- **Tác động**: UI không hiển thị label theo config, mất khả năng tùy chỉnh ngôn ngữ/độ tuổi.
- **Fix**: Truyền tham số label vào `ApplyComparisonAnswerButtonVisuals()` hoặc gán trực tiếp `buttonText.text`.
- **Verify**: Chạy CompareQuantity trong Editor, kiểm tra text 3 nút khớp với `MoreButtonLabel` / `FewerButtonLabel` / `EqualButtonLabel` trong config.

### 1.2 CompareQuantity — `IActivityView.OnAnswerSelected` empty stub

- **File**: `CompareQuantityView.cs` (dòng ~90-94)
- **Mức độ**: P1 (type-safety bug)
- **Vấn đề**: Interface `IActivityView` yêu cầu `event Action<ActivityAnswer> OnAnswerSelected` nhưng add/remove accessor là **empty no-op**. View dùng một event riêng `Action<ComparisonAnswer> OnAnswerSelected` (khác type). Nếu code nào cast về `IActivityView` và subscribe event này → **silent fail**.
- **Tác động**: Mọi subscription qua interface đều không hoạt động. Hiện tại không gây crash vì Presenter subscribe trực tiếp vào View, nhưng là bom nổ chậm cho future refactor.
- **Fix**: Wire event của View vào interface stub, hoặc tạo bridge event chung.
- **Verify**: Không cần test riêng nếu không có code nào cast về `IActivityView` để subscribe.

### 1.3 NumberLineJump — Equation format sai

- **File**: `NumberLineJumpAnswer.cs`, method `GetCurrentEquation()` (dòng ~168-181)
- **Mức độ**: P0 (HIGH — ảnh hưởng trực tiếp trải nghiệm học tập)
- **Vấn đề**: `GetCurrentEquation(int startNumber, int currentPosition)` **không nhận `targetNumber`** làm tham số. Kết quả:
  - Với question `Start=3, Target=7`:
    - Sau 1 jump → hiển thị `"3 + 1 = ?"` thay vì `"3 + 4 = 4"`
    - Sau 4 jumps → hiển thị `"3 + 4 = ?"` thay vì `"3 + 4 = 7"`
  - Vế trái của phép tính luôn hiển thị số bước đã nhảy (1, 2, 3, 4) thay vì **tổng số bước cần nhảy** (luôn là 4).
- **Tác động**: Trẻ không hiểu được phép tính gốc `3 + 4`, chỉ thấy số bước tăng dần.
- **Fix**: 
  1. Thêm tham số `int targetNumber` vào `GetCurrentEquation()`
  2. Tính `totalJumpCount = targetNumber - startNumber` (giá trị tuyệt đối)
  3. Format: `$"{startNumber} {op} {totalJumpCount} = {currentPosition}"`
- **Verify**: Chạy NumberLineJump Round 1 trong Editor, tap → 4 lần, xác nhận equation hiển thị `"3 + 4 = 3"` → `"3 + 4 = 4"` → ... → `"3 + 4 = 7"`.

### 1.4 NumberLineJump — Jump direction không được enforce

- **File**: `NumberLineJumpPresenter.cs`, method `HandleJumpRequested()` (dòng ~383)
- **Mức độ**: P0 (HIGH — mất giá trị sư phạm của phép trừ)
- **Vấn đề**: Question model có field `JumpDirection` (RightOnly / LeftOnly / Both) để giới hạn hướng nhảy, nhưng `HandleJumpRequested()` **không gọi `IsDirectionAllowed()`**. User luôn nhảy được cả ← và →, kể cả với câu hỏi phép trừ.
- **Tác động**: Với câu hỏi `"8 - 3 = ?"`, trẻ có thể nhảy sang phải (tăng lên 9, 10...) thay vì buộc phải lùi — phá hỏng mục tiêu học phép trừ qua dịch chuyển.
- **Fix**: Trong `HandleJumpRequested()`, trước khi xử lý jump, gọi:
  ```csharp
  if (!IsDirectionAllowed(direction)) {
      ShowMaxJumpsWarning();
      return;
  }
  ```
- **Verify**: Chạy Round 2 (Subtract: `8 - 3`), tap →, xác nhận nút → bị disable hoặc jump bị chặn.

### 1.5 NumberLineJump — Config asset không load được từ Resources

- **File**: `GameplayActivityRouter.cs` (dòng ~21)
- **Mức độ**: P1 (MEDIUM — maintenance hazard)
- **Vấn đề**: Router khai báo resource path `"ActivityConfigs/SO_NumberLineJumpConfig_Easy"` nhưng asset thực tế nằm ở `Features/Activities/NumberLineJump/ScriptableObjects/` — **không nằm trong Resources folder nào**. Mọi inspector tweak vào .asset đều bị bỏ qua, runtime luôn dùng fallback config cứng.
- **Tác động**: Không thể tinh chỉnh config qua Editor inspector. Nếu ai sửa asset mà không biết → thay đổi không có hiệu lực.
- **Fix**: 
  1. Copy asset vào `Assets/_Project/Resources/ActivityConfigs/SO_NumberLineJumpConfig_Easy.asset`
  2. Hoặc đổi resource path trỏ đúng vào Resources folder
- **Verify**: Sửa 1 giá trị trong .asset inspector, build & run, xác nhận giá trị mới có hiệu lực.

### 1.6 QuantityMatch — Particle warning

- **File**: Confetti/particle prefab (vị trí chính xác cần xác định)
- **Mức độ**: P2 (warning, không crash)
- **Vấn đề**: Console log: `"Particle Velocity curves must all be in the same mode"` — inconsistency trong curve mode của particle system.
- **Tác động**: Chỉ warning, không ảnh hưởng gameplay. Nhưng gây nhiễu console log khi debug.
- **Fix**: Mở particle prefab, kiểm tra Velocity over Lifetime module, đảm bảo tất cả curves cùng mode (Constant / Curve / Random).
- **Verify**: Chạy QuantityMatch, hoàn thành 1 round, kiểm tra console không còn warning.

---

## PHASE 2: MISSING FEATURES — Cần để Editor test hoàn chỉnh

Các tính năng đã có trong plan nhưng chưa implement. Không phải blocker nhưng cần để demo đúng proposal.

### 2.1 CompareQuantity — `HighlightGroup()` no-op

- **File**: `CompareQuantityView.cs` (dòng ~498-502)
- **Mức độ**: P1
- **Vấn đề**: Plan A.7 yêu cầu Hint Level 2 "Highlight larger group với color overlay". Method `HighlightGroup(int groupIndex, bool highlight)` hiện tại **chỉ Debug.Log**, không làm gì.
- **Fix**: Implement logic: lấy group GameObject từ `spawnedGroups[groupIndex]`, đổi màu material hoặc overlay.
- **Verify**: Dùng 2 lần hint trong CompareQuantity, xác nhận group lớn hơn được highlight ở hint level 2.

### 2.2 CompareQuantity — Thiếu `QuestionType` enum và `SymbolCompare` display

- **File**: `CompareQuantityQuestion.cs` / `CompareQuantityView.cs`
- **Mức độ**: P2
- **Vấn đề**: Plan mô tả 4 question types (`MoreThan`, `FewerThan`, `Equal`, `SymbolCompare`) nhưng implementation chỉ có 1 prompt cứng `"Bên trái > < = bên phải?"`. Không có kiểu hiển thị `"3 ? 5"`.
- **Fix**: 
  1. Thêm `QuestionType` enum vào `CompareQuantityQuestion`
  2. Khi `SymbolCompare`, hiển thị `$"{leftCount} ? {rightCount}"` thay vì text mặc định
- **Verify**: Tạo question `SymbolCompare` với left=3, right=5, kiểm tra UI hiển thị `"3 ? 5"`.

### 2.3 CompareQuantity — Thiếu Interactive Counting Mode

- **File**: Cần tạo script mới hoặc mở rộng `CompareQuantityPresenter.cs`
- **Mức độ**: P2 (post-MVP)
- **Vấn đề**: Plan A.3 Mode 2 mô tả "tap lần lượt từng nhóm để đếm, mỗi lần tap highlight từng object, sau khi đếm cả 2 nhóm mới enable symbol buttons". Chưa implement.
- **Fix**: Tạo `CountingModeHandler` hoặc mở rộng presenter với state `CountingLeft` / `CountingRight`. Khi enable, disable symbol buttons, bắt từng tap để highlight object.
- **Verify**: Chạy CompareQuantity với `interactionMode = InteractiveCounting`, tap từng object, xác nhận đếm tuần tự và symbol buttons chỉ enable sau khi đếm xong 2 nhóm.

### 2.4 CompareQuantity — Thiếu confetti, scale animation, audio

- **File**: `CompareQuantityView.cs`
- **Mức độ**: P1-P2
- **Vấn đề**: Plan yêu cầu:
  - Confetti particles khi đúng
  - Scale animation 1.0→1.2→1.0 trên nút đúng
  - Audio celebration chime khi đúng, soft buzz khi sai
  Hiện tại feedback chỉ là text đổi màu xanh/đỏ + panel.
- **Fix**: 
  1. Instantiate confetti particle prefab tại vị trí nút đúng
  2. Thêm `UIKidFriendlyStyle.ScalePulse()` coroutine
  3. Gọi `SimpleAudioManager.PlayClip(correctClip)` / `PlayClip(incorrectClip)`
- **Verify**: Chọn đúng 1 round, xác nhận có animation + audio.

### 2.5 NumberLineJump — `ShowMaxJumpsWarning()` chỉ log, không UI

- **File**: `NumberLineJumpView.cs` (dòng ~492-499)
- **Mức độ**: P1
- **Vấn đề**: Method `ShowMaxJumpsWarning(int remainingJumps)` chỉ `Debug.Log`, không hiển thị gì cho user. Plan yêu cầu "warning icon khi còn 1-2 jumps".
- **Fix**: Hiển thị UI warning text/icon hoặc đổi màu nút jump.
- **Verify**: Nhảy đến khi còn 1 jump, xác nhận có warning visible.

### 2.6 NumberLineJump — Thiếu audio feedback

- **File**: `NumberLineJumpPresenter.cs` / `NumberLineJumpView.cs`
- **Mức độ**: P1-P2
- **Vấn đề**: Chỉ có instruction audio. Plan yêu cầu:
  - Step sound ("boing") mỗi lần nhảy
  - Number spoken khi nhảy đến vị trí mới
  - Celebration chime khi correct
  - Soft buzz khi incorrect
  - Boundary voice "Không thể đi tiếp"
- **Fix**: Gọi `SimpleAudioManager.Instance.PlayClip(...)` tại các điểm tương ứng trong jump animation và feedback.
- **Verify**: Chạy 1 round đầy đủ, xác nhận có step sound khi nhảy, celebration khi đúng.

### 2.7 NumberBonds — Thiếu animated zone glow

- **File**: `NumberBondZoneView.cs`
- **Mức độ**: P1
- **Vấn đề**: `SetValidationState()` đổi màu zone nhưng là static — plan mô tả "zones glow green/pulse" cho correct feedback.
- **Fix**: Thêm coroutine pulse animation (scale hoặc color lerp).
- **Verify**: Confirm đúng 1 round NumberBonds, xác nhận zones có hiệu ứng glow nhấp nháy.

### 2.8 NumberBonds — Compose/MissingPart mode chưa có runtime behavior

- **File**: `NumberBondRoundState.cs` (dòng ~50-69), `NumberBondsPresenter.cs`
- **Mức độ**: P2 (post-MVP)
- **Vấn đề**: Enum đã có nhưng `FromQuestion()` không khởi tạo đúng cho Compose/MissingPart (mặc định đặt hết object vào Whole), và `ValidateCurrentState()` không có validation riêng cho 2 mode này.
- **Fix**: 
  1. `FromQuestion()`: Compose → objects trong Parts, Whole=0. MissingPart → 1 Part locked có sẵn object, Whole có số còn lại.
  2. `ValidateCurrentState()`: Thêm case cho Compose (Whole==target, Parts==0) và MissingPart (missing part == correct).
- **Verify**: Tạo question Compose (PartA=2, PartB=3), xác nhận start state đúng, confirm validate đúng.

### 2.9 NumberLineJump — Mode 2/3/4 chưa implement

- **File**: `NumberLineJumpPresenter.cs`
- **Mức độ**: P3 (post-MVP)
- **Vấn đề**: Plan mô tả Tap Number Tiles (multi-step jump), Drag Character, Continuous Drag. Chỉ Mode 1 (Button Step) hoạt động.
- **Ghi chú**: Chưa cần cho demo, nhưng cần có `interactionMode` field trong config để chọn.
- **Fix**: Thêm enum `InteractionMode { ButtonStep, TapTile, DragCharacter }` vào config, implement TapTile multi-step.

---

## PHASE 3: iOS BUILD REQUIREMENTS

Các vấn đề cần giải quyết để build và chạy ổn định trên iOS device.

### 3.1 Performance — Objective count clamping

- **Vấn đề**: Mỗi activity spawn object 3D (animal cubes, number tiles, jump character). Trên iOS, quá nhiều object gây draw call cao và FPS thấp.
- **Hiện trạng**: `RuntimePerformanceSettings` đã có, nhưng không rõ đã áp dụng cho từng activity chưa.
- **Action cần làm**:
  1. Verify `RuntimePerformanceSettings.ClampGroupObjectCount()` được gọi trong mọi activity Presenter trước khi spawn
  2. Đảm bảo max object cho NumberBonds ≤ 7, CompareQuantity mỗi group ≤ 5-6, QuantityMatch group ≤ 5
  3. Đặt FPS target = 30 (`Application.targetFrameRate = 30`)
- **Verify**: Build iOS, chạy 10 rounds mỗi activity, kiểm tra FPS counter ≥ 25.

### 3.2 Plane detection — Tắt sau placement

- **Vấn đề**: `ARPlacementService.UpdatePlacementPosition` chạy liên tục kể cả sau khi đã có learning area, tốn CPU.
- **Action**: Sau khi `HasLearningArea == true`, gọi `planeManager.enabled = false` hoặc giảm tần suất update.
- **Verify**: Build iOS, sau khi đặt learning area, kiểm tra plane visualization biến mất và CPU usage giảm.

### 3.3 Collider simplification

- **Vấn đề**: Tài liệu rule yêu cầu không dùng MeshCollider. Cần verify tất cả object prefab dùng SphereCollider hoặc BoxCollider.
- **Action**: 
  1. Duyệt tất cả prefab: animal prefabs, zone objects, number tiles
  2. Nếu có MeshCollider → thay bằng SphereCollider (radius 0.08-0.12m)
- **Verify**: Build iOS, test drag-drop, không có crash hoặc performance spike.

### 3.4 Scene setup — Đảm bảo đủ object

- **Vấn đề**: `SC_ARGameplay` cần có đủ:
  - `ARServiceBootstrap` (execution order -200)
  - `LearningSceneServices` (-150)
  - `GameplayActivityRouter` (-140)
  - `ARPlacementController` (tap-to-place)
  - `ARSessionBootstrap` / `ARSessionService`
  - `ARInteractionService`
  - `ActivityPrefabSetup`
  - XR Origin + AR Camera
- **Action**: Open `SC_ARGameplay.unity`, verify từng component tồn tại, đúng execution order.
- **Verify**: Cold start app trên iOS, không có NullReferenceException.

### 3.5 Player Settings — iOS build checklist

- **Action**:
  - [ ] Architecture: **ARM64**
  - [ ] Scripting Backend: **IL2CPP**
  - [ ] Managed Stripping Level: **Low** (tránh stripping AR Foundation code)
  - [ ] Target SDK: Latest stable iOS
  - [ ] Camera Usage Description: `"Ứng dụng cần truy cập camera để hiển thị nội dung thực tế ảo"`
  - [ ] ARKit capability: enabled (trong Xcode project hoặc Unity Player Settings)
  - [ ] Development Build: **ON** (cho test)
  - [ ] `Application.targetFrameRate = 30` trong code hoặc Player Settings
- **Verify**: Build thành công, deploy lên iPhone qua Xcode, không lỗi provisioning profile.

### 3.6 Memory stability — 10 rounds liên tiếp

- **Vấn đề**: Object instantiate/destroy mỗi round gây GC pressure. Sau 10 rounds, memory có thể tăng dần (leak).
- **Action**:
  1. Test từng activity 10 rounds trên device
  2. Dùng Xcode Instruments → Allocations → check memory trend
  3. Nếu memory tăng > 20% sau 10 rounds → investigate object pool hoặc leak.
- **Verify**: Memory ổn định, không bị OOM kill.

### 3.7 ActivityPrefabSetup — Character prefab fallback

- **Vấn đề**: Nếu không có animal asset thật (import từ Asset Store/model), activity vẫn phải chạy được với placeholder procedural.
- **Hiện trạng**: `ActivityPrefabSetup` có fallback chain từ prefab → procedural animal → capsule placeholder.
- **Action**: Verify trên build iOS không crash khi không có asset import.
- **Verify**: Build không có animal prefabs, chạy các activity, xác nhận character dùng sphere/procedural animal hợp lệ.

### 3.8 Camera permission & AR unsupported

- **Vấn đề**: App phải xử lý case user từ chối camera permission hoặc device không hỗ trợ ARKit.
- **Action**: 
  1. Kiểm tra `ARSession.state` → nếu `Unsupported` → hiển thị thông báo thân thiện
  2. Permission denied → hiển thị dialog hướng dẫn vào Settings
- **Verify**: Từ chối camera permission, xác nhận có message (không crash).

---

## PHASE 4: ARCHITECTURE COMPLIANCE — Technical Debt

Các vấn đề không chặn demo nhưng cần xử lý để codebase bền vững.

### 4.1 Presenter chứa logic nặng — cần tách

| File | Lines | Cần tách thành |
|---|---|---|
| `QuantityMatchView.cs` | ~1,892 | `QuantityMatchRuntimeUI.cs` (~400L), `QuantityMatchLayoutConstants.cs` (~100L) |
| `CompareQuantityView.cs` | ~1,051 | `CompareQuantityRuntimeUI.cs` (~300L) |
| `NumberLineJumpPresenter.cs` | ~945 | `NumberLineBuilder.cs` (~200L), `NumberLineCharacterController.cs` (~150L), `NumberLineExpressionBinder.cs` (~100L) |
| `NumberBondsView.cs` | ~467 | OK (đã tách thành View + Visuals + RuntimeUI) |

### 4.2 Bootstrap polling → event-driven

- **Vấn đề**: `ActivityBootstrap.TryStartActivity()` dùng `Invoke("TryStartActivity", 0.5f)` để poll placement availability. Rule yêu cầu event-driven.
- **Fix**: Subscribe `placement.OnLearningAreaPlaced` + `session.OnSessionReady` thay vì polling.
- **Áp dụng cho**: `QuantityMatchActivityBootstrap`, `CompareQuantityActivityBootstrap`, `NumberBondsActivityBootstrap`, `NumberLineJumpActivityBootstrap`.

### 4.3 HintSystem static → injected service

- **Vấn đề**: `HintSystem` là static class, không có cleanup giữa sessions.
- **Fix**: Chuyển thành service với lifetime scope (init/cleanup mỗi session).

### 4.4 MaterialPropertyBlock — reuse

- **Vấn đề**: `ARInteractionService.ApplyHighlightColor()` tạo `MaterialPropertyBlock` mới mỗi lần highlight → GC pressure.
- **Fix**: Cache block làm instance field.

### 4.5 Object pooling

- **Vấn đề**: Object instantiate/destroy mỗi round. Với demo 2-3 rounds thì OK, nhưng production cần pooling.
- **Fix**: Tạo `SimpleObjectPool` trong `Core/AR/` hoặc shared utility.

### 4.6 Unit tests

- **Vấn đề**: Tất cả `Tests/` directories đều có `.gitkeep` trống, không có unit test nào.
- **Fix cần làm**:
  - Presenter validation logic (FreeSplit/TargetSplit)
  - Answer model `IsCorrect()`
  - State transitions
  - Hint progression

---

## PHASE 5: POLISH — Nice-to-have cho demo ấn tượng

### 5.1 Confetti / particle effects

- **Activity**: CompareQuantity, NumberLineJump, NumberBonds
- **Mô tả**: Thêm confetti particle khi correct answer.
- **Ưu tiên**: Thấp (không ảnh hưởng learning logic)

### 5.2 Character prefab thay sphere placeholder

- **Activity**: NumberLineJump
- **Mô tả**: Thay sphere thành frog/rabbit 3D model.
- **Fix**: Import asset hoặc procedural-animate capsule.

### 5.3 Connection lines giữa các Number Bond zones

- **Activity**: NumberBonds
- **Mô tả**: LineRenderer nối Whole → PartA, Whole → PartB.
- **Fix**: Thêm vào `NumberBondsView.Visuals.CreateZones()`.

### 5.4 Haptic feedback trên iOS

- **Activity**: Tất cả
- **Mô tả**: Rung nhẹ khi correct, rung khác khi incorrect.
- **Fix**: `Handheld.Vibrate()` hoặc Unity's Haptic API.

### 5.5 Count overlay trên groups trong CompareQuantity

- **Activity**: CompareQuantity
- **Mô tả**: Plan hint level 3 yêu cầu hiển thị count numbers bên trên mỗi nhóm.
- **Fix**: World-space TextMesh hiện count khi hint level 3.

---

## Tổng kết: Priority Matrix

| # | Vấn đề | Activity | Phase | Severity |
|---|---|---|---|---|
| 1.3 | Equation format sai | NumberLineJump | Phase 1 | 🔴 HIGH |
| 1.4 | Jump direction không enforce | NumberLineJump | Phase 1 | 🔴 HIGH |
| 1.1 | UpdateButtonLabels bỏ qua tham số | CompareQuantity | Phase 1 | 🟡 MEDIUM |
| 1.5 | Config không load từ Resources | NumberLineJump | Phase 1 | 🟡 MEDIUM |
| 1.2 | IActivityView event stub | CompareQuantity | Phase 1 | 🟢 LOW |
| 1.6 | Particle warning | QuantityMatch | Phase 1 | 🟢 LOW |
| 2.1 | HighlightGroup no-op | CompareQuantity | Phase 2 | 🟡 MEDIUM |
| 2.5 | MaxJumpsWarning no UI | NumberLineJump | Phase 2 | 🟡 MEDIUM |
| 2.4 | Thiếu confetti/scale/audio | CompareQuantity | Phase 2 | 🟡 MEDIUM |
| 2.6 | Thiếu audio feedback | NumberLineJump | Phase 2 | 🟡 MEDIUM |
| 2.7 | Zone glow animation | NumberBonds | Phase 2 | 🟡 MEDIUM |
| 2.2 | SymbolCompare question type | CompareQuantity | Phase 2 | 🟢 LOW |
| 2.8 | Compose/MissingPart runtime | NumberBonds | Phase 2 | 🟢 LOW |
| 3.1-3.8 | iOS build requirements | Tất cả | Phase 3 | 🟡 MEDIUM |
| 4.1-4.6 | Architecture cleanup | Tất cả | Phase 4 | 🟢 LOW |
| 5.1-5.5 | Polish | Tất cả | Phase 5 | 🟢 LOW |

---

## Kế hoạch hành động theo sprint

### Sprint 1 (2-3 ngày): Fix bugs → Editor test được 4 bài

- [ ] Fix 1.3: Equation format (NumberLineJump)
- [ ] Fix 1.4: Jump direction enforce (NumberLineJump)
- [ ] Fix 1.1: UpdateButtonLabels (CompareQuantity)
- [ ] Fix 1.5: Config Resources path (NumberLineJump)
- [ ] Fix 1.2: IActivityView event (CompareQuantity)
- [ ] Fix 1.6: Particle warning (QuantityMatch)
- [ ] Editor test: chạy tuần tự 4 bài, mỗi bài 3 rounds, không lỗi

### Sprint 2 (2-3 ngày): Missing features + iOS build

- [ ] Implement 2.1, 2.4, 2.5, 2.6, 2.7
- [ ] Implement 3.1-3.8 (iOS build checklist)
- [ ] Build iOS, deploy lên iPhone test thật
- [ ] Fix bất kỳ crash hoặc performance issue nào

### Sprint 3 (1-2 ngày): Polish + cleanup

- [ ] Architecture refactor (Phase 4) — làm sau khi iOS test OK
- [ ] Polish effects (Phase 5)
- [ ] Final test full flow 4 bài trên iOS

---

*Tổng hợp: 29/05/2026 — từ AR_DEMO_LESSON_GAP_ANALYSIS.md + 4 LESSON_0*.md + codebase exploration*
