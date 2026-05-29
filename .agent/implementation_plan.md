# Implementation Plan - Hoàn Thiện Hệ Thống AR, Logic Và Gameplay

**Ngày tạo:** 2026-05-26  
**Nguồn kế hoạch:** `.agent/ar_system_optimization.md`  
**Phạm vi:** Unity client `apps/unity-client`  
**Trạng thái:** Kế hoạch triển khai, chưa thực hiện code ở bước này.

## 1. Mục Tiêu

Kế hoạch này chuyển toàn bộ các vấn đề đã liệt kê trong `ar_system_optimization.md` thành roadmap triển khai theo phase. Mục tiêu cuối cùng là biến prototype hiện tại thành một hệ thống AR học toán có thể demo/nghiệm thu ở mức sản phẩm:

- Có luồng mở app rõ ràng.
- Có calibration AR trước khi học.
- Có learning area anchor ổn định.
- Trẻ có thể chạm để đếm mà không nộp nhầm.
- Activity được load bằng một cơ chế duy nhất.
- Quantity Match là vertical slice hoàn chỉnh.
- Number Line Jump và Compare Quantity chạy đúng flow.
- UI tiếng Việt, ít chữ, có hỗ trợ audio/hint trực quan.
- Progress/dashboard phản ánh kỹ năng học tập, không chỉ log kết quả.
- Có test Editor/mock, device smoke test và báo cáo QA.

## 2. Nguyên Tắc Triển Khai

1. Không thêm activity mới trước khi xử lý xong nhóm P0.
2. Mỗi phase phải có acceptance criteria rõ và có thể kiểm chứng trong Unity.
3. Ưu tiên sửa flow và semantics trước polish hình ảnh.
4. Không để lỗi kỹ thuật AR bị tính là lỗi học tập của trẻ.
5. Không phụ thuộc `AssetDatabase` trong gameplay build.
6. Editor/mock path phải luôn còn để test nhanh.
7. Device path phải được validate riêng, không suy từ Editor mock.
8. Sau mỗi phase lớn phải cập nhật tài liệu test hoặc checklist.

## 3. Bảng Bao Phủ Vấn Đề

| ID | Vấn đề | Phase xử lý chính |
|---|---|---|
| AR-01 | `LearningAreaAnchor` chưa là tọa độ chính | Phase 2 |
| AR-02 | Thiếu calibration trước bài học | Phase 2 |
| AR-03 | `ARPlacementService` và `ARPlacementController` chồng trách nhiệm | Phase 2 |
| AR-04 | Tracking/session chưa gate gameplay | Phase 2 |
| AR-05 | Plane visualization còn debug | Phase 2 |
| AR-06 | Chưa ràng buộc kích thước vùng học | Phase 2, Phase 4 |
| AR-07 | Chưa có cảnh báo khoảng cách/safety | Phase 10 |
| INT-01 | Tap-to-count lẫn tap-to-submit | Phase 3 |
| INT-02 | Number input mode mất hỗ trợ chạm-đếm | Phase 3 |
| INT-03 | Hitbox chưa phù hợp trẻ nhỏ | Phase 3, Phase 8 |
| INT-04 | UI raycast có thể chặn AR | Phase 3 |
| INT-05 | Highlight phá material/asset | Phase 3, Phase 8 |
| INT-06 | Drag/reposition chưa có chủ đích | Phase 3 |
| GP-01 | Activity routing phân tán | Phase 1 |
| GP-02 | `SC_ARGameplay` ưu tiên Quantity Match mặc định | Phase 1 |
| GP-03 | Cold-start chưa thống nhất | Phase 1 |
| GP-04 | Auto-advance quá nhanh | Phase 4 |
| GP-05 | Failure flow chưa đủ sư phạm | Phase 4, Phase 7 |
| GP-06 | Save result theo round nhưng dashboard gọi activity | Phase 7 |
| GP-07 | Number Line Jump chưa đủ cảm giác game | Phase 5 |
| GP-08 | Compare Quantity thiếu ghép cặp trực quan | Phase 6 |
| GP-09 | Chưa có lesson map/mastery | Phase 7 |
| GP-10 | Chưa có adaptive difficulty | Phase 11 |
| CT-01 | Nội dung còn tiếng Anh | Phase 4, Phase 8 |
| CT-02 | Config loading phụ thuộc Editor fallback | Phase 1 |
| CT-03 | Thiếu prefab/UI chuẩn | Phase 4, Phase 8 |
| CT-04 | Animal prefab chưa nhất quán học tập | Phase 8 |
| CT-05 | Number line tile chưa chuẩn | Phase 5 |
| CT-06 | Thiếu audio/VFX thật | Phase 8 |
| UI-01 | Runtime UI chưa phải UI sản phẩm | Phase 8 |
| UI-02 | Label 3D chưa đồng nhất billboard | Phase 5, Phase 6, Phase 8 |
| UI-03 | Thiếu accessibility settings | Phase 8 |
| UI-04 | Feedback/hint phụ thuộc chữ | Phase 4, Phase 8 |
| UI-05 | Thiếu UX lỗi kỹ thuật AR | Phase 2 |
| DATA-01 | Nullable enum serialize không chắc | Phase 7 |
| DATA-02 | Chưa tách technical issue và learning issue | Phase 2, Phase 7 |
| DATA-03 | Dashboard chưa có nhận định học tập | Phase 7 |
| DATA-04 | Chưa có learner profiles | Phase 11 |
| ARCH-01 | Namespace không đồng nhất | Phase 9 |
| ARCH-02 | Comment interface lỗi thời | Phase 9 |
| ARCH-03 | Chưa có asmdef | Phase 9 |
| ARCH-04 | Runtime reflection trong router | Phase 1, Phase 9 |
| PERF-01 | Spawn/destroy nhiều gây giật | Phase 10 |
| PERF-02 | Runtime material repair tốn chi phí | Phase 10 |
| PERF-03 | `Resources.LoadAll` thiếu kiểm soát memory | Phase 10 |
| TEST-01 | Chưa có device AR pass report | Phase 0, Phase 10 |
| TEST-02 | `unity_compile.log` không chứng minh compile pass | Phase 0 |
| TEST-03 | Chưa có EditMode/PlayMode tests | Phase 10 |
| TEST-04 | Chưa có AR edge-case QA | Phase 10 |

## 4. Roadmap Tổng Thể

```text
Phase 0  - Baseline, test gate và scope freeze
Phase 1  - Thống nhất entry scene, activity host và config loading
Phase 2  - AR calibration, learning area anchor và session gate
Phase 3  - Chuẩn hóa interaction semantics: count, select, submit, hitbox
Phase 4  - Hoàn thiện Quantity Match vertical slice
Phase 5  - Hoàn thiện Number Line Jump gameplay
Phase 6  - Hoàn thiện Compare Quantity gameplay
Phase 7  - Lesson map, result model, progress analytics và dashboard
Phase 8  - UI/UX, localization, audio/VFX, prefab production
Phase 9  - Kiến trúc, namespace, API cleanup
Phase 10 - Performance, test automation, device QA
Phase 11 - Adaptive difficulty, learner profile, parent/teacher mode
```

## 5. Phase 0 - Baseline, Test Gate Và Scope Freeze

**Mục tiêu:** xác định trạng thái thật trước khi sửa code, tránh build trên giả định sai.

**Vấn đề xử lý:** TEST-01, TEST-02.

### 5.1 Entry criteria

- Repo mở được trong Unity `6000.0.71f1`.
- Không có thay đổi ngoài ý muốn cần revert.
- Đã đọc `ar_system_optimization.md`, `LOCAL_UNITY_FULL_TEST_GUIDE.md`, `PHASE2_TEST_GUIDE.md`.

### 5.2 Công việc chi tiết

1. Kiểm tra `git status`.
2. Ghi lại Unity version và package versions.
3. Đóng mọi Unity instance đang mở trước khi chạy batch compile.
4. Chạy compile/test baseline:
   - Unity batchmode compile nếu có thể.
   - Nếu không thể batchmode, mở Editor và ghi nhận Console manually.
5. Mở các scene hiện có và ghi nhận trạng thái:
   - `SC_MainMenu`
   - `SC_ActivitySelect`
   - `SC_ARGameplay`
   - `SC_TestSandbox`
   - `SC_ProgressDashboard`
   - `SC_Boot`
6. Chạy smoke test Editor/mock:
   - Sandbox: spawn grid, spawn circle, clear, tap object.
   - Gameplay: Quantity Match start, answer wrong/right, save JSON.
7. Lập file test log ngắn nếu chưa có:
   - ngày test;
   - Unity version;
   - pass/fail từng scene;
   - lỗi đỏ Console;
   - path progress JSON;
   - kết luận baseline.

### 5.3 File dự kiến dùng/chạm tới

| File | Loại |
|---|---|
| `.agent/LOCAL_UNITY_FULL_TEST_GUIDE.md` | tham chiếu |
| `.agent/PHASE2_TEST_GUIDE.md` | tham chiếu |
| `unity_compile.log` | output kiểm thử |
| `.agent/baseline_test_log.md` | có thể tạo mới |

### 5.4 Acceptance criteria

- Có kết luận rõ: compile pass/fail.
- Có kết luận rõ: Editor/mock sandbox pass/fail.
- Có kết luận rõ: Quantity Match Editor/mock pass/fail.
- Nếu fail, có danh sách lỗi blocker trước Phase 1.

### 5.5 Exit gate

Không sang Phase 1 nếu project còn compile error đỏ chưa hiểu nguyên nhân.

## 6. Phase 1 - Thống Nhất Entry Scene, Activity Host Và Config Loading

**Mục tiêu:** làm sạch luồng mở app và cơ chế load activity trước khi đụng sâu AR/gameplay.

**Vấn đề xử lý:** GP-01, GP-02, GP-03, CT-02, ARCH-04 một phần.

### 6.1 Quyết định kiến trúc cần chốt

1. Entry scene chính:
   - Khuyến nghị: `SC_Boot` là scene index 0.
   - `SC_Boot` chỉ init services tối thiểu rồi load `SC_MainMenu`.
2. Activity host:
   - Khuyến nghị: `SC_ARGameplay` chỉ có `GameplayRoot`, `ARServiceBootstrap`, `LearningSceneServices`, `ActivityHost`.
   - Không để `QuantityMatchActivity` tự start mặc định trong flow production.
3. Activity registry:
   - Khuyến nghị: tạo một registry asset hoặc serialized component chứa `activityId`, config reference, prefab/root factory.
   - Không dùng `AssetDatabase` trong runtime build.

### 6.2 Công việc chi tiết

1. Kiểm kê scene `SC_ARGameplay`:
   - xác nhận có `QuantityMatchActivityBootstrap`;
   - xác nhận có `LearningSceneServices`;
   - xác nhận có hoặc không có `ActivityLoader`;
   - xác nhận `GameplayActivityRouter` được add runtime.
2. Chọn loader duy nhất:
   - Option A: giữ `GameplayActivityRouter`, bỏ vai trò `ActivityLoader`.
   - Option B: giữ `ActivityLoader`, loại bỏ runtime reflection trong `GameplayActivityRouter`.
   - Khuyến nghị dài hạn: tạo `ActivityHost` + `ActivityRegistry`.
3. Thiết kế `ActivityRegistry`:
   - `activityId`;
   - display name;
   - config reference;
   - activity prefab hoặc bootstrap factory;
   - trạng thái enabled/disabled;
   - lesson ids về sau.
4. Thống nhất `SelectedActivityData`:
   - chỉ lưu `activityId` và optional `lessonId`;
   - không dùng `ConfigPath` kiểu mơ hồ.
5. Chỉnh Build Settings:
   - `SC_Boot` index 0 nếu quyết định dùng Boot;
   - `SC_MainMenu`;
   - `SC_ActivitySelect`;
   - `SC_ARGameplay`;
   - `SC_ProgressDashboard`;
   - `SC_TestSandbox`.
6. Cập nhật flow:
   - Main Menu -> Activity Select -> Gameplay;
   - Activity Select chỉ hiện activity enabled;
   - nếu NumberLine/Compare chưa pass, hiển thị khóa hoặc "sắp có" thay vì route sai.
7. Loại bỏ runtime config dependency vào `AssetDatabase`:
   - config phải được reference trong registry/scene hoặc nằm trong Resources/Addressables;
   - build device không được cần Editor API.

### 6.3 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `Assets/_Project/Scripts/GameplayActivityRouter.cs` | thay thế hoặc đơn giản hóa |
| `Assets/_Project/Scripts/ActivityLoader.cs` | giữ hoặc loại khỏi production path |
| `Assets/_Project/Scripts/ActivitySelectController.cs` | route theo registry |
| `Assets/_Project/Scripts/ActivityFlowNavigator.cs` | dùng `activityId`/`lessonId` chuẩn |
| `Assets/_Project/Scenes/SC_ARGameplay.unity` | chuyển sang `ActivityHost` |
| `ProjectSettings/EditorBuildSettings.asset` | thống nhất scene order |

### 6.4 Acceptance criteria

- Từ Main Menu chọn `QuantityMatch` thì vào đúng Quantity Match.
- Nếu chọn `NumberLineJump` thì vào đúng Number Line Jump hoặc button bị khóa rõ ràng.
- Nếu chọn `CompareQuantity` thì vào đúng Compare Quantity hoặc button bị khóa rõ ràng.
- Build path không gọi `AssetDatabase`.
- Không còn hai loader cùng có quyền start activity trong production flow.

### 6.5 Exit gate

Không sang Phase 2 nếu `SC_ARGameplay` vẫn có nhiều cơ chế tự start cạnh tranh nhau.

## 7. Phase 2 - AR Calibration, Learning Area Anchor Và Session Gate

**Mục tiêu:** làm AR trở thành nền ổn định cho mọi activity.

**Vấn đề xử lý:** AR-01, AR-02, AR-03, AR-04, AR-05, AR-06, UI-05, DATA-02 một phần.

### 7.1 Thiết kế trạng thái AR gameplay

Gameplay scene cần có state machine:

```text
Initializing
-> CheckingSupport
-> WaitingForPermission
-> ScanningPlane
-> PlaneFound
-> PlacingLearningArea
-> LearningAreaReady
-> StartingActivity
-> ActivityRunning
-> TrackingLostPaused
-> RecoveringTracking
-> ActivityCompleted
```

### 7.2 Công việc chi tiết

1. Chuẩn hóa placement API:
   - `IARPlacementService` phải biết có learning area chưa;
   - expose content root hoặc API spawn local trong learning area;
   - nếu chưa có learning area, activity không được spawn.
2. Hợp nhất vai trò placement:
   - `ARPlacementController` xử lý tap-to-place;
   - `ARPlacementService` là API mà activity dùng;
   - hai class phải liên kết rõ hoặc merge.
3. Tạo calibration UI:
   - overlay quét mặt phẳng;
   - hướng dẫn tiếng Việt;
   - marker preview khi có plane hợp lệ;
   - nút hoặc tap xác nhận đặt vùng học;
   - nút đặt lại vùng học.
4. Lock learning area:
   - sau khi đặt, ẩn plane visualizer;
   - spawn content dưới `LearningAreaAnchor.ContentRoot`;
   - mọi round mới dùng local layout.
5. Session/tracking gate:
   - nếu `IsTrackingStable = false`, pause input;
   - hiện overlay không tính là lỗi học tập;
   - resume khi tracking ổn định.
6. Technical issue handling:
   - spawn failed;
   - no plane;
   - tracking lost;
   - missing camera permission;
   - unsupported device.
7. Editor mock path:
   - trong Editor, mock tự tạo learning area default;
   - vẫn có nút debug reset.
8. Layout safety:
   - mỗi activity báo kích thước layout mong muốn;
   - placement flow kiểm tra plane/area đủ lớn;
   - nếu không đủ, yêu cầu chọn mặt phẳng khác.

### 7.3 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `IARPlacementService.cs` | thêm contract learning area/local spawn |
| `ARPlacementService.cs` | hỗ trợ anchor/local spawn/layout bounds |
| `ARPlacementServiceMock.cs` | tạo mock learning area |
| `ARPlacementController.cs` | tích hợp calibration flow |
| `LearningAreaAnchor.cs` | content root, area bounds |
| `ARPlaneDetectionController.cs` | scan state và plane visibility |
| `ARSessionService.cs` | tracking quality events/gate |
| `LearningSceneServices.cs` | orchestration trước khi start activity |
| `SC_ARGameplay.unity` | overlay calibration |

### 7.4 Acceptance criteria

- Trên Editor mock: vào gameplay có learning area default và activity start được.
- Trên device: activity không start trước khi quét/đặt vùng học.
- Khi mất tracking: input bị khóa, hiện thông báo thân thiện.
- Khi tracking hồi phục: có thể tiếp tục hoặc đặt lại vùng học.
- Object của round mới nằm dưới learning area root.
- Plane visualizer bị ẩn sau khi placement xong.

### 7.5 Exit gate

Không sang Phase 3 nếu activity vẫn spawn trực tiếp theo `CurrentPlacementPosition` mà không có learning area root.

## 8. Phase 3 - Chuẩn Hóa Interaction Semantics

**Mục tiêu:** sửa triệt để lỗi nhập liệu làm trẻ nộp nhầm, đồng thời chuẩn hóa hitbox/highlight.

**Vấn đề xử lý:** INT-01, INT-02, INT-03, INT-04, INT-05, INT-06.

### 8.1 Interaction vocabulary cần thống nhất

| Hành động | Ý nghĩa |
|---|---|
| Tap object | Đếm hoặc inspect object, không nộp bài |
| Tap group label | Chọn nhóm |
| Tap submit button | Nộp đáp án đã chọn |
| Double tap group | Tùy chọn nâng cao, có thể nộp nhanh nếu bật |
| Drag object | Chỉ dùng trong activity kéo-thả hoặc mode setup |
| Tap outside | Bỏ chọn hoặc không làm gì |

### 8.2 Công việc chi tiết

1. Thiết kế input model mới:
   - `CountTap`;
   - `GroupSelect`;
   - `SubmitAnswer`;
   - `ResetCounting`;
   - `TechnicalTapIgnored`.
2. Quantity Match:
   - tap từng object tăng count visual/audio;
   - tap group label chọn group;
   - submit qua UI hoặc label xác nhận;
   - number input mode vẫn cho tap object để đếm;
   - có nút đếm lại.
3. Interaction service:
   - phân biệt object hitbox và group hitbox;
   - data gắn vào interactable rõ kiểu;
   - tránh lưu raw object/string mơ hồ.
4. Hitbox:
   - thêm invisible collider lớn hơn visual model;
   - đảm bảo group label touch target lớn;
   - test trên mobile screen.
5. UI raycast:
   - transparent area không chặn AR;
   - chỉ button/panel cần thiết nhận raycast.
6. Highlight:
   - tách highlight count/select/correct/wrong;
   - không reset material về trắng nếu có texture;
   - dùng `MaterialPropertyBlock` hoặc outline/halo.
7. Drag:
   - tắt drag mặc định;
   - chỉ bật trong mode cụ thể;
   - nếu drag activity về sau thì phải có snap slot.

### 8.3 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `ARInteractionService.cs` | typed interactable data, hitbox/highlight cleanup |
| `QuantityMatchPresenter.cs` | tap-to-count và submit semantics |
| `QuantityMatchView.cs` | selected group, submit, reset counting |
| `ActivityPrefabSetup.cs` | child hitbox prefab/fallback |
| `CompareQuantityPresenter.cs` | chuẩn group select về sau |
| `NumberLineJumpPresenter.cs` | tắt drag không dùng, tile tap rõ |

### 8.4 Acceptance criteria

- Chạm object trong Quantity Match không submit ngay.
- Trẻ có thể đếm object bằng tap trong cả group mode và number input mode.
- Submit chỉ xảy ra khi chọn group label/button hoặc bấm submit.
- UI không chặn AR tap ngoài vùng button.
- Highlight không phá màu/texture của animal prefab.

### 8.5 Exit gate

Không sang Phase 4 nếu Quantity Match vẫn có khả năng nộp nhầm khi trẻ đang tap để đếm.

## 9. Phase 4 - Hoàn Thiện Quantity Match Vertical Slice

**Mục tiêu:** đưa Quantity Match lên mức demo/nghiệm thu tốt nhất, làm chuẩn cho activity khác.

**Vấn đề xử lý:** GP-04, GP-05, CT-01, CT-03, UI-04, AR-06 một phần.

### 9.1 Trải nghiệm mục tiêu

```text
Vào bài
-> nghe yêu cầu tiếng Việt
-> thấy số mục tiêu lớn
-> thấy 2-3 nhóm object rõ
-> có thể tap từng object để đếm
-> chọn nhóm
-> xác nhận
-> nếu sai, được hướng dẫn đếm lại
-> nếu đúng, có feedback tích cực
-> tự bấm tiếp tục sang câu sau
-> cuối bài lưu kết quả và sang dashboard hoặc bài tiếp theo
```

### 9.2 Công việc chi tiết

1. Việt hóa Quantity Match:
   - title;
   - instructions;
   - feedback;
   - hints;
   - labels;
   - buttons.
2. Guided counting:
   - tap object highlight theo thứ tự;
   - voice count 1-10;
   - hiện count badge nhỏ;
   - reset count.
3. Round progression:
   - bỏ auto-advance mặc định;
   - sau câu đúng hiện nút "Tiếp tục";
   - sau câu sai giữ object và highlight nơi cần đếm lại;
   - sau max attempts chuyển guided mode.
4. Content progression trong Quantity Match:
   - round 1-3: 1-3 object, ít lựa chọn;
   - round 4-6: 1-5 object;
   - round 7-10: 6-10 object, ten-frame hoặc layout dễ đếm;
   - number input chỉ dùng khi trẻ đã được hỗ trợ đếm.
5. UI prefab:
   - tạo hoặc thiết kế spec cho `PFB_QuantityMatchPanel`;
   - button lớn;
   - target number rõ;
   - hint/audio/retry/continue rõ.
6. Result save:
   - lưu attempts, hints, counting taps, final selection;
   - phân biệt wrong quantity và technical issue.
7. Test vertical slice:
   - Editor mock;
   - device nếu Phase 2 đã pass;
   - progress JSON.

### 9.3 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `QuantityMatchPresenter.cs` | guided counting, submit flow, failure remediation |
| `QuantityMatchView.cs` | UI state, continue, retry, audio button |
| `QuantityMatchConfig.cs` | strings/hints/lesson data |
| `SO_QuantityMatchConfig_Easy.asset` | nội dung tiếng Việt và progression |
| `ActivityPrefabSetup.cs` | object prefab ổn định |
| `LocalProgressStorage.cs` | thêm fields nếu cần |

### 9.4 Acceptance criteria

- Hoàn thành 1 lesson Quantity Match 5-10 câu trong Editor mock.
- Không có nộp nhầm khi tap object để đếm.
- Trẻ có thể nghe lại yêu cầu.
- Sai 1 lần có hint trực quan hoặc audio, không chỉ text.
- Đúng 1 câu không tự biến mất quá nhanh.
- Kết quả lưu local và dashboard đọc được.

### 9.5 Exit gate

Không sang Phase 5 nếu Quantity Match chưa đủ làm vertical slice chính.

## 10. Phase 5 - Hoàn Thiện Number Line Jump

**Mục tiêu:** biến Number Line Jump thành activity học trục số/cộng trừ có gameplay rõ và ổn định.

**Vấn đề xử lý:** GP-07, CT-05, UI-02.

### 10.1 Công việc chi tiết

1. Tile prefab:
   - `PFB_NumberTile`;
   - số lớn, camera-facing;
   - collider/hitbox rõ;
   - trạng thái start/current/target.
2. Character prefab:
   - nhân vật dễ thương, scale ổn định;
   - animation hop;
   - landing feedback.
3. Jump mechanics:
   - preview hướng trái/phải;
   - jump arc thay vì teleport;
   - lock input khi đang jump;
   - boundary bump animation.
4. Equation scaffolding:
   - hiển thị `3 + 2 = 5`;
   - đọc từng bước;
   - sau mỗi jump cập nhật equation.
5. Error handling:
   - sai hướng -> highlight hướng đúng;
   - quá số bước -> hướng dẫn đếm lại;
   - vượt biên -> bump và giải thích.
6. Lesson progression:
   - 0-5 trước;
   - 0-10 sau;
   - cộng trước, trừ sau;
   - mixed review sau cùng.

### 10.2 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `NumberLineJumpPresenter.cs` | tile prefab, jump animation, boundary feedback |
| `NumberLineJumpView.cs` | equation, arrows, continue/remediation |
| `NumberLineJumpConfig.cs` | lesson strings/progression |
| `SO_NumberLineJumpConfig_Easy.asset` | nội dung bài học |
| `ActivityPrefabSetup.cs` | fallback tile/character tốt hơn |

### 10.3 Acceptance criteria

- Chọn Number Line Jump từ Activity Select chạy đúng activity.
- Một lesson 0-5 hoàn thành được trong Editor mock.
- Tile số luôn đọc được khi camera di chuyển.
- Nhân vật nhảy rõ từng bước.
- Boundary/overshoot có feedback trực quan.
- Result lưu đúng error type.

## 11. Phase 6 - Hoàn Thiện Compare Quantity

**Mục tiêu:** giúp trẻ hiểu nhiều hơn/ít hơn/bằng nhau bằng trực quan, không chỉ nhìn rồi bấm.

**Vấn đề xử lý:** GP-08, UI-02, CT-01.

### 11.1 Công việc chi tiết

1. Việt hóa Compare:
   - "Nhiều hơn";
   - "Ít hơn";
   - "Bằng nhau";
   - "Nhóm bên trái/phải" hoặc thay bằng icon.
2. Pairing mode:
   - hiển thị hai hàng object;
   - auto nối từng cặp hoặc cho trẻ tap ghép;
   - highlight phần thừa;
   - nếu không thừa thì hiển thị bằng nhau.
3. Symbol bridge:
   - ban đầu dùng chữ/icon;
   - sau đó thêm `>`, `<`, `=`;
   - giải thích bằng audio/visual.
4. Group interaction:
   - tap object không submit;
   - tap comparison button hoặc group label để chọn;
   - confirm rõ.
5. Hint theo lỗi:
   - nhầm more/fewer;
   - nhầm equal;
   - đếm sai một group.

### 11.2 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `CompareQuantityPresenter.cs` | pairing visual, labels, selection semantics |
| `CompareQuantityView.cs` | buttons, symbol bridge, hint visual |
| `CompareQuantityConfig.cs` | strings/hints tiếng Việt |
| `SO_CompareQuantityConfig_Easy.asset` | lesson progression |

### 11.3 Acceptance criteria

- Compare Quantity chạy đúng từ Activity Select.
- Có ít nhất một lesson compare 1-5.
- Có case more, fewer, equal.
- Trẻ thấy phần thừa hoặc cặp bằng nhau.
- Result lưu đúng.

## 12. Phase 7 - Lesson Map, Result Model, Progress Analytics Và Dashboard

**Mục tiêu:** chuyển hệ thống từ mini-game rời rạc thành lộ trình học có ý nghĩa.

**Vấn đề xử lý:** GP-06, GP-09, DATA-01, DATA-02, DATA-03, GP-05 một phần.

### 12.1 Thiết kế dữ liệu học tập

Cần tách các lớp kết quả:

| Model | Ý nghĩa |
|---|---|
| `RoundResult` | Một câu/round |
| `LessonResult` | Một lesson gồm nhiều round |
| `ActivitySessionResult` | Một lần học một activity |
| `SkillMastery` | Mức thành thạo theo kỹ năng |
| `TechnicalIssueRecord` | Lỗi kỹ thuật AR không tính vào học tập |

### 12.2 Skill tags đề xuất

| Skill | Activity |
|---|---|
| Counting | Quantity Match |
| OneToOneCounting | Quantity Match, Compare |
| Subitizing | Quantity Match |
| QuantitySymbolMapping | Quantity Match |
| MoreFewer | Compare Quantity |
| Equality | Compare Quantity |
| NumberOrder | Number Line Jump |
| AdditionOnNumberLine | Number Line Jump |
| SubtractionOnNumberLine | Number Line Jump |

### 12.3 Lesson map đề xuất

```text
L01 - Nhận biết nhanh 1-3
L02 - Đếm từng vật 1-5
L03 - Ghép số với lượng 1-5
L04 - Ghép số với lượng 6-10
L05 - Nhiều hơn / ít hơn 1-5
L06 - Bằng nhau
L07 - Dấu > < =
L08 - Trục số 0-5
L09 - Trục số 0-10
L10 - Cộng bằng bước nhảy
L11 - Trừ bằng bước nhảy
L12 - Ôn tập trộn kỹ năng
```

### 12.4 Công việc chi tiết

1. Sửa serialization:
   - thay `ErrorType?` bằng structure serializable;
   - thêm technical issue type.
2. Thêm lesson metadata:
   - lesson id;
   - skill tags;
   - recommended age/difficulty;
   - prerequisites;
   - unlock condition.
3. Dashboard:
   - số câu đã làm;
   - độ chính xác;
   - số hint;
   - lỗi thường gặp;
   - kỹ năng mạnh/yếu;
   - bài nên luyện tiếp.
4. Activity Select:
   - hiển thị lesson/activity unlocked;
   - không đưa trẻ vào bài chưa sẵn sàng.
5. Remediation:
   - nếu sai cùng error nhiều lần, đề xuất quay lại lesson dễ hơn.

### 12.5 File dự kiến chỉnh ở phase sau

| File | Mục đích |
|---|---|
| `ActivityResult.cs` | result schema |
| `LocalProgressStorage.cs` | progress schema/statistics |
| `ProgressStorageProxy.cs` | API mới |
| `ProgressDashboardView.cs` | dashboard có ý nghĩa |
| `ActivitySelectController.cs` | unlock/lesson display |
| New lesson config/registry | lesson map |

### 12.6 Acceptance criteria

- JSON lưu được round + lesson + technical issue.
- Dashboard không gọi round là activity completed.
- Phụ huynh nhìn dashboard biết trẻ nên luyện kỹ năng nào.
- Lesson map có ít nhất 6 lesson đầu.

## 13. Phase 8 - UI/UX, Localization, Audio/VFX Và Prefab Production

**Mục tiêu:** nâng trải nghiệm từ test UI lên sản phẩm thân thiện trẻ nhỏ.

**Vấn đề xử lý:** CT-03, CT-04, CT-06, UI-01, UI-03, UI-04, INT-03, INT-05.

### 13.1 Công việc chi tiết

1. Localization:
   - bảng string tiếng Việt;
   - không hardcode string trong presenter/router;
   - fallback English nếu cần.
2. Audio:
   - audio manager;
   - clip đọc số 1-10;
   - clip hướng dẫn activity;
   - clip feedback đúng/sai/hint;
   - nút nghe lại.
3. VFX:
   - correct sparkle;
   - count pulse;
   - wrong gentle shake;
   - success celebration;
   - boundary bump.
4. UI prefab:
   - main menu;
   - activity select;
   - Quantity panel;
   - Compare panel;
   - NumberLine panel;
   - AR error/calibration overlay.
5. Accessibility:
   - tăng cỡ chữ;
   - giảm animation;
   - giảm/tắt âm;
   - high contrast;
   - simplified mode.
6. Prefab production:
   - animal/object set dễ đếm;
   - number tile;
   - jump character;
   - group label;
   - hitbox child.
7. Visual consistency:
   - mọi label 3D billboard/camera-facing;
   - không che object;
   - layout stable trên nhiều tỉ lệ màn hình.

### 13.2 Acceptance criteria

- Toàn bộ UI trẻ thấy trong 3 activity là tiếng Việt.
- Trẻ có thể nghe lại yêu cầu.
- Có audio số 1-10 cho counting.
- UI không phụ thuộc runtime fallback trong flow demo.
- Có setting tối thiểu: âm thanh on/off, giảm animation.

## 14. Phase 9 - Kiến Trúc, Namespace Và API Cleanup

**Mục tiêu:** giảm nợ kỹ thuật sau khi flow chính đã ổn.

**Vấn đề xử lý:** ARCH-01, ARCH-02, ARCH-03, ARCH-04.

### 14.1 Công việc chi tiết

1. Namespace:
   - thống nhất `Core.AR` hoặc `ARSpecialEducation.Core.AR`;
   - cập nhật using;
   - ghi migration note.
2. Interface comments:
   - xóa comment "TODO implement" đã lỗi thời;
   - mô tả implementation hiện có và extension points.
3. Public configure API:
   - bootstrap/activity có `Configure(...)`;
   - không set private field bằng reflection.
4. Assembly definitions:
   - lập kế hoạch asmdef theo module;
   - thêm sau khi compile/test ổn;
   - tránh phá package references.
5. Documentation:
   - cập nhật README scene flow;
   - cập nhật setup guide;
   - cập nhật test guide.

### 14.2 Acceptance criteria

- Không còn runtime reflection trong activity production path.
- Namespace AR nhất quán hoặc được document rõ.
- Interface comment phản ánh đúng hiện trạng.
- Nếu thêm asmdef, project compile clean.

## 15. Phase 10 - Performance, Test Automation Và Device QA

**Mục tiêu:** làm hệ thống bền, mượt và có bằng chứng nghiệm thu.

**Vấn đề xử lý:** PERF-01, PERF-02, PERF-03, TEST-01, TEST-02, TEST-03, TEST-04, AR-07.

### 15.1 Performance work

1. Object pooling:
   - animal/object;
   - tile;
   - labels;
   - VFX.
2. Resource control:
   - không `Resources.LoadAll` toàn bộ trong production nếu không cần;
   - registry prefab/theme;
   - preload audio/prefab cần cho lesson.
3. Material:
   - sửa material tại import/editor time;
   - hạn chế tạo material runtime.
4. Device constraints:
   - giới hạn object count;
   - quality tier;
   - target FPS.

### 15.2 Test automation

1. EditMode tests:
   - Quantity Match answer logic;
   - Compare answer logic;
   - Number Line final position;
   - progress serialization;
   - hint escalation.
2. PlayMode tests với mock AR:
   - gameplay start;
   - placement mock;
   - tap/select/submit;
   - save result.
3. Batch compile:
   - lưu compile log sạch;
   - không dùng log cũ bị lỗi project open.

### 15.3 Device QA

Checklist device:

- Camera permission granted/denied.
- Unsupported device path.
- Plane scanning.
- Place learning area.
- Hide plane visualizer.
- Spawn objects.
- Tap count.
- Submit answer.
- Tracking lost/recover.
- Save result.
- Return dashboard.

Test matrix tối thiểu:

| Thiết bị | Mục tiêu |
|---|---|
| Android ARCore | bắt buộc nếu target Android |
| iOS ARKit | nếu có target iOS |
| Windows Editor mock | regression nhanh |

### 15.4 Acceptance criteria

- Có compile log pass mới.
- Có ít nhất 1 report device pass.
- Có test tự động cho core learning logic.
- Không có spike nghiêm trọng khi chuyển round trong lesson nhỏ.
- Có cảnh báo safety cơ bản khi tracking/camera movement không ổn.

## 16. Phase 11 - Adaptive Difficulty, Learner Profile, Parent/Teacher Mode

**Mục tiêu:** hoàn thiện lớp sản phẩm giáo dục sau MVP.

**Vấn đề xử lý:** GP-10, DATA-04.

### 16.1 Công việc chi tiết

1. Learner profile:
   - tên trẻ;
   - tuổi/lớp;
   - preferences;
   - accessibility settings;
   - progress per learner.
2. Adaptive difficulty:
   - giảm choices khi sai nhiều;
   - bật guided mode tự động;
   - tăng số lượng khi đúng nhanh;
   - spaced repetition lỗi thường gặp.
3. Parent/teacher mode:
   - xem kỹ năng mạnh/yếu;
   - chỉnh session length;
   - reset/backup progress;
   - chọn lesson thủ công.
4. Reports:
   - weekly summary;
   - recommended practice;
   - export JSON/CSV nếu cần.

### 16.2 Acceptance criteria

- Có thể tạo/chọn learner profile.
- Progress không lẫn giữa nhiều trẻ.
- Hệ thống gợi ý bài tiếp theo dựa trên kết quả.
- Parent dashboard có thông tin hành động được.

## 17. Thứ Tự Triển Khai Đề Xuất Theo Sprint

### Sprint 1 - Baseline và scene/activity host

- Phase 0 hoàn tất.
- Phase 1 hoàn tất.
- Kết quả: project compile clean, scene entry rõ, chọn activity không route sai.

### Sprint 2 - AR calibration và anchor

- Phase 2 hoàn tất.
- Kết quả: gameplay không start trước khi có learning area; Editor mock vẫn chạy nhanh.

### Sprint 3 - Interaction semantics và Quantity Match

- Phase 3 hoàn tất.
- Phase 4 hoàn tất.
- Kết quả: Quantity Match demo được như vertical slice chính.

### Sprint 4 - Hai activity còn lại

- Phase 5 hoàn tất.
- Phase 6 hoàn tất.
- Kết quả: Number Line Jump và Compare Quantity chạy đúng flow cơ bản.

### Sprint 5 - Lesson map và progress

- Phase 7 hoàn tất.
- Kết quả: hệ thống có lộ trình học và dashboard có ý nghĩa.

### Sprint 6 - UX/audio/prefab

- Phase 8 hoàn tất.
- Kết quả: trải nghiệm đủ thân thiện trẻ nhỏ để quay demo.

### Sprint 7 - Hardening

- Phase 9 và Phase 10 hoàn tất.
- Kết quả: code sạch hơn, test có, device report có.

### Sprint 8 - Sau MVP

- Phase 11.
- Kết quả: cá nhân hóa và parent/teacher mode.

## 18. Checklist Nghiệm Thu Cuối

Hệ thống chỉ nên được coi là hoàn thiện mức MVP khi tick được:

- [ ] Compile clean bằng log mới.
- [ ] `SC_Boot` hoặc entry scene chính được chốt và document.
- [ ] Activity host chỉ có một cơ chế load activity.
- [ ] Config runtime không phụ thuộc `AssetDatabase`.
- [ ] Có calibration AR trước khi start activity trên device.
- [ ] Object spawn dưới learning area root.
- [ ] Tracking lost sẽ pause input và không tính là lỗi học tập.
- [ ] Tap object để đếm không nộp nhầm.
- [ ] Quantity Match hoàn thành một lesson và save result.
- [ ] Number Line Jump hoàn thành một lesson cơ bản và save result.
- [ ] Compare Quantity hoàn thành một lesson cơ bản và save result.
- [ ] UI chính là tiếng Việt.
- [ ] Có audio hướng dẫn tối thiểu.
- [ ] Dashboard hiển thị kỹ năng hoặc lỗi cần luyện.
- [ ] Có device AR pass report.
- [ ] Có test tự động cho answer logic tối thiểu.

## 19. Ghi Chú Không Làm Trong Giai Đoạn Này

Các hạng mục sau không nên đưa vào trước khi P0/P1 hoàn tất:

- Activity mới ngoài 3 activity hiện có.
- Backend/cloud sync.
- AI tutor/LLM.
- Multiplayer.
- Parent dashboard phức tạp.
- Refactor lớn không phục vụ scene/activity/AR flow.

## 20. Kết Luận

Thứ tự ưu tiên quan trọng nhất là: làm flow chạy đúng, làm AR ổn định, làm tương tác không gây nộp nhầm, rồi mới polish giao diện và mở rộng sư phạm. Nếu làm ngược lại, hệ thống sẽ có nhiều UI/asset đẹp nhưng vẫn không chứng minh được rằng trẻ có thể học toán trong AR một cách ổn định, dễ hiểu và đo được tiến bộ.

