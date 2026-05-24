# Hướng dẫn kiểm thử đầy đủ (Local Unity Editor)

**Mục tiêu:** Kiểm thử toàn bộ tính năng hiện có của Unity client trên Unity Editor (local), tách rõ phần đã playable và phần mới chỉ có khung.

**Dự án:** `apps/unity-client`  
 **Phiên bản Unity:** `6000.0.71f1`  
 **Ngày lập:** 2026-05-21  
 **Phạm vi:** Phase 1 → Phase 4 (local-first, Editor/mock AR là chính). Device AR chỉ là smoke test.

---

## 0. Tóm tắt trạng thái hiện tại

| Phase                                   | Trạng thái                                   | Mức ước tính | Ghi chú                                                                                                                                                                      |
| --------------------------------------- | -------------------------------------------- | -----------: | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Phase 0 - Scope freeze                  | Hoàn thành                                   |         100% | Tài liệu và boundary đã rõ.                                                                                                                                                  |
| Phase 1 - AR Core + Sandbox             | Gần hoàn thành (code)                        |       85–90% | Có `ARSessionService`, `ARPlacementService` mock, interaction, sandbox scene. Có thể playtest trong Editor/device.                                                           |
| Phase 2 - Quantity Match vertical slice | Gần hoàn thành (code + scene)                |       80–85% | Có config asset, `SC_ARGameplay`, runtime UI, mock spawn, feedback/progress hooks. Có thể chạy E2E và kiểm tra JSON lưu trữ.                                                 |
| Phase 3 - Number Line Jump              | Một phần                                     |       35–45% | Có presenter/bootstrap/config factory nhưng runtime UI builder còn placeholder; chưa fully scene-wired.                                                                      |
| Phase 4 - App shell                     | Code sẵn sàng, scene integration chưa đầy đủ |       45–55% | Có scripts Boot/Main/Select/Dashboard, nhưng shell scenes hiện tại thiếu cấu hình trong YAML, Build Settings chưa chuẩn, `SC_ARGameplay` hiện chỉ auto-start Quantity Match. |

Ngắn gọn: hệ thống hiện tại vượt xa skeleton cũ và có thể test Phase 1 + Phase 2 trong Editor. Chưa thể công nhận Phase 4 full-pass vì cold-start flow và activity selection chưa được tích hợp hoàn chỉnh.

---

## 1. Chuẩn bị trước khi test

1. Mở Unity Hub.
2. Mở project: `d:\.Kỳ II nam Ba\Chuyen de\BTL\apps\unity-client`.
3. Đợi Unity import/compile xong.
4. Mở Console: `Window > General > Console`.
5. Bật lọc `Log`, `Warning`, `Error`.
6. Nhấn `Clear`.
7. Đảm bảo không còn lỗi biên dịch (màu đỏ).

Nếu còn compile errors: dừng test gameplay và sửa lỗi compile trước.

---

## 2. Tạo asset và scene cần thiết

Chạy các menu trong Unity theo thứ tự dưới.

### 2.1 Tạo config assets

1. `AR Learning > Create Quantity Match Easy Config`
2. `AR Learning > Create Number Line Jump Easy Config`
3. `AR Learning > Create Compare Quantity Easy Config`

Kiểm tra các file:

- `Assets/Features/Activities/QuantityMatch/ScriptableObjects/SO_QuantityMatchConfig_Easy.asset`
- `Assets/Features/Activities/NumberLineJump/ScriptableObjects/SO_NumberLineJumpConfig_Easy.asset`
- `Assets/Features/Activities/CompareQuantity/ScriptableObjects/SO_CompareQuantityConfig_Easy.asset`

Lưu ý: tại thời điểm lập hướng dẫn, repo có sẵn `Quantity Match` config; hai config còn lại cần tạo bằng menu nếu chưa tồn tại.

### 2.2 Tạo AR scenes

1. `AR Learning > Setup Test Sandbox Scene`
2. `AR Learning > Setup AR Gameplay Scene (Quantity Match)`

Kiểm tra:

- `Assets/_Project/Scenes/SC_TestSandbox.unity`
- `Assets/_Project/Scenes/SC_ARGameplay.unity`

### 2.3 Tạo shell scenes

Chạy tiếp:

1. `AR Learning > Setup Scenes > Setup Boot Scene`
2. `AR Learning > Setup Scenes > Setup Main Menu Scene`
3. `AR Learning > Setup Scenes > Setup Activity Select Scene`
4. `AR Learning > Setup Scenes > Setup Progress Dashboard Scene`

Kiểm tra:

- `Assets/_Project/Scenes/SC_Boot.unity`
- `Assets/_Project/Scenes/SC_MainMenu.unity`
- `Assets/_Project/Scenes/SC_ActivitySelect.unity`
- `Assets/_Project/Scenes/SC_ProgressDashboard.unity`

### 2.4 Build Settings cho shell flow

Mở `File > Build Settings` (hoặc `File > Build Profiles` tùy layout Unity).

Đảm bảo thứ tự scene trong Build Settings:

1. `Assets/_Project/Scenes/SC_Boot.unity`
2. `Assets/_Project/Scenes/SC_MainMenu.unity`
3. `Assets/_Project/Scenes/SC_ActivitySelect.unity`
4. `Assets/_Project/Scenes/SC_ARGameplay.unity`
5. `Assets/_Project/Scenes/SC_ProgressDashboard.unity`
6. `Assets/_Project/Scenes/SC_TestSandbox.unity`

Nếu `Assets/Scenes/SampleScene.unity` đang ở đầu danh sách thì vẫn có thể để lại cho testing, nhưng product shell nên bắt đầu từ `SC_Boot`.

Chú ý: menu `AR Learning > Repair Product Scenes` có thể tái tạo scene và Build Settings nhưng có thể để lại placeholder canvas/label — nếu chạy menu này, hãy chạy lại 4 menu `Setup Scenes` ở mục 2.3.

---

## 3. Điều kiện compile (Compile gate)

Sau khi tạo scene/config:

1. Đợi Unity compile xong.
2. Console không được có lỗi biên dịch.
3. Đặc biệt không được còn các lỗi sau:
   - `CS0246 BootLoader could not be found`
   - `CS0246 MainMenuController could not be found`
   - `CS0246 ActivitySelectController could not be found`
   - `CS0246 ProgressDashboardView could not be found`
   - `CS0117 ProgressStorageProxy.Initialize`
   - `CS0117 FeedbackServiceProxy.Initialize`
   - `CS0029 LocalProgressStorage to ProgressStorageProxy`

Nếu pass compile gate thì mới tiến hành gameplay tests.

---

## 4. Test Phase 1 — AR sandbox (Editor)

1. Mở scene: `Assets/_Project/Scenes/SC_TestSandbox.unity`.
2. Nhấn `Play`.
3. Trong Console mong thấy các log tương tự:
   - `[ARPlacementServiceMock] Initialized (editor/mock mode).`
   - `[ARInteractionService] Initialized.`
   - `[ARServiceBootstrap] Ready. Placement=ARPlacementServiceMock`
   - `[ARSandbox] Placement=ARPlacementServiceMock, Available=True`

Vì đang chạy mock placement trong Editor, không cần plane detection thật.

### 4.1 Kiểm thử bằng phím

Trong Game view:

1. Nhấn `G` — các sphere spawn dạng grid; Console có `[ARSandbox] Spawned grid...`.
2. Click vào một sphere — Console có `[ARSandbox] OnObjectTapped: ...`; object được highlight.
3. Nhấn `C` — spawn circle; Console có `[ARSandbox] Spawned circle...`.
4. Nhấn `X` — xóa object; Console có `[ARSandbox] Cleared spawned objects.`.
5. Nhấn `L` — in trạng thái placement.

Pass Phase 1 khi: G/C/X/L hoạt động, click object ghi log, không có lỗi biên dịch.

---

## 5. Test Phase 2 — Quantity Match (Editor)

1. Stop Play.
2. Mở scene: `Assets/_Project/Scenes/SC_ARGameplay.unity`.
3. Nhấn `Play`.
4. Đợi 1–2 giây.

### 5.1 Khởi động mong đợi

Trong Game view:

- Hiển thị giao diện Quantity Match.
- Hiển thị target number.
- Hiển thị progress text (ví dụ: `Question 1 of 2`).
- Các nút `Hint`, `Cancel`, `Group 1`, `Group 2`, `Group 3`.
- Object groups spawn trong scene/Game view.

Console có thể ghi:

- `[ARServiceBootstrap] Ready. Placement=ARPlacementServiceMock`
- `[ProgressStorageProxy] Session started: ...`
- `[QuantityMatchPresenter] Loading round 1: Target = 3`
- `[QuantityMatchActivityBootstrap] Quantity Match started.`

### 5.2 Ví dụ dữ liệu round

| Round | Target | Group 1   | Group 2   | Group 3   | Đáp án  |
| ----: | -----: | --------- | --------- | --------- | ------- |
|     1 |      3 | 2 objects | 3 objects | 4 objects | Group 2 |
|     2 |      5 | 4 objects | 5 objects | 6 objects | Group 2 |

### 5.3 Kiểm thử Hint

Ở Round 1:

1. Nhấn `Hint` — UI hiển thị hint đầu tiên.
2. Nhấn `Hint` lần 2 — UI hiển thị hint tiếp theo.
3. Nhấn `Hint` lần 3 — UI hiển thị hint cuối.

Pass khi: hint hiển thị trên UI, `HintsUsedCount` tăng trong dữ liệu lưu, không có lỗi.

### 5.4 Kiểm thử trả lời sai

Ở Round 1 (target = 3, đáp án Group 2):

1. Nhấn `Group 1`.
2. UI hiển thị feedback sai; app không chuyển round ngay.
3. Console có log ví dụ:
   - `[QuantityMatchPresenter] Group selected: 0`
   - `-> INCORRECT`
   - `[ActivityPresenter] Incorrect answer. Attempt 1 of 3`

### 5.5 Kiểm thử trả lời đúng và chuyển round

Ở Round 1:

1. Nhấn `Group 2`.
2. UI hiển thị feedback đúng.
3. Console có log:
   - `[QuantityMatchPresenter] Group selected: 1`
   - `-> CORRECT`
   - `[ProgressStorageProxy] Result saved: QuantityMatch L1 = True`
4. Đợi ~2 giây — UI chuyển sang `Question 2 of 2`.
5. Hoàn thành Round 2 tương tự và kiểm tra log lưu kết quả.

Pass Phase 2 khi: hoàn thành đủ 2 round, kết quả được lưu, có feedback, không có lỗi.

---

## 6. Kiểm thử thao tác click vào object AR

1. Restart Play trong `SC_ARGameplay`.
2. Khi object spawn xong, click vào group giữa ở Round 1.
3. Ứng xử ứng dụng phải giống như chọn `Group 2`.
4. Console ghi log tương ứng `Group selected: 1`.

Nếu click trong Game view khó trúng, dùng UI button fallback để test end-to-end trên Editor. Trên device, tap object cần test riêng.

---

## 7. Kiểm tra file progress local (JSON)

Thiết lập project hiện tại:

- `companyName`: `DefaultCompany`
- `productName`: `unity-client`

Đường dẫn lưu trên Windows dự kiến:

`C:\Users\tuanp\AppData\LocalLow\DefaultCompany\unity-client\learning_progress.json`

Sau khi hoàn thành Quantity Match:

1. Mở File Explorer, điều hướng đến đường dẫn trên.
2. Mở `learning_progress.json`.
3. Kiểm tra có `allResults` và ít nhất 2 result.

Trường dữ liệu mong muốn:

- `"ActivityId": "QuantityMatch"`
- `"LevelNumber": 1` và `2`
- `"IsCorrect": true`
- `"TotalAttempts"` >= 1
- `"HintsUsedCount"` đúng với số hint đã bấm
- `"TimeSpentSeconds"` > 0
- `startTimeString` / `endTimeString` là ISO timestamp

Pass khi: file tồn tại, dữ liệu round 1/2 được ghi, restart Unity Play vẫn đọc được dashboard.

---

## 8. Kiểm thử Progress Dashboard

1. Stop Play.
2. Đảm bảo đã chạy: `AR Learning > Setup Scenes > Setup Progress Dashboard Scene`.
3. Mở scene: `Assets/_Project/Scenes/SC_ProgressDashboard.unity`.
4. Nhấn `Play`.

Mong thấy:

- Overall Progress: Activities Completed, Total Sessions, Total Results.
- Activity Statistics: QuantityMatch có Attempts, Success Rate, Avg Time, Best Time, Hints Used.
- NumberLineJump / CompareQuantity có thể hiển thị `No data yet` nếu chưa có dữ liệu.

Nhấn `Back`:

- Nếu `SC_MainMenu` có trong Build Settings, app load về main menu; nếu không, Unity sẽ báo lỗi load scene — quay lại mục 2.4 để sửa.

---

## 9. Test Phase 4 — Shell flow (partial)

### 9.1 Chuẩn bị

Đảm bảo đã chạy 4 menu thiết lập scene:

- `AR Learning > Setup Scenes > Setup Boot Scene`
- `AR Learning > Setup Scenes > Setup Main Menu Scene`
- `AR Learning > Setup Scenes > Setup Activity Select Scene`
- `AR Learning > Setup Scenes > Setup Progress Dashboard Scene`

Và Build Settings có thứ tự scene theo mục 2.4.

### 9.2 Cold start flow

1. Mở scene `Assets/_Project/Scenes/SC_Boot.unity`.
2. Nhấn `Play`.
3. Console có thể ghi: `[BootLoader] Services initialized.`
4. Sau ~0.5s, app load `SC_MainMenu`.
5. Từ Main Menu: `View Progress` → `SC_ProgressDashboard` → `Back` → `Start Learning` → `SC_ActivitySelect` → chọn `Quantity Match` → load `SC_ARGameplay`.

Hiện hành vi mong đợi: `SC_ARGameplay` được setup cho `Quantity Match` nên Quantity Match auto-start.

### 9.3 Lưu ý về gap hiện tại

Hiện tại có `ActivityLoader` hỗ trợ nhiều activity nhưng `SC_ARGameplay` hiện chỉ có `QuantityMatchActivityBootstrap`. Vì vậy:

- `Quantity Match` button có thể pass.
- `Number Line Jump` và `Compare Quantity` có thể không pass Phase 4 full nếu vẫn load vào Quantity Match hoặc không load đúng activity.

Để đạt Phase 4 full-pass cần một trong hai hướng:

- disable các button chưa playable; hoặc
- bake `ActivityLoader`/dispatcher vào `SC_ARGameplay` và gán presenter/view/config tương ứng cho từng activity.

---

## 10. Kiểm thử tùy chọn — Number Line Jump (tham khảo)

Feature này chưa fully scene-wired.

1. Tạo config: `AR Learning > Create Number Line Jump Easy Config`.
2. Duplicate `SC_ARGameplay` thành scene tạm, ví dụ `SC_ARGameplay_NumberLineJump_Test.unity`.
3. Trong scene tạm, disable hoặc xóa `QuantityMatchActivity`.
4. Tạo GameObject `NumberLineJumpActivity` và thêm components:
   - `NumberLineJumpPresenter`, `NumberLineJumpView`, `ActivityPrefabSetup`, `NumberLineJumpRuntimeUI`, `NumberLineJumpActivityBootstrap`.
5. Trong `NumberLineJumpActivityBootstrap`, assign `presenter`, `view`, `config = SO_NumberLineJumpConfig_Easy.asset`.
6. Play.

Hạn chế: `NumberLineJumpRuntimeUI` có thể log `Runtime UI creation not yet implemented`. Nếu không assign UI refs thủ công, activity có thể chạy logic nhưng không test được UI đầy đủ.

---

## 11. Kiểm thử tùy chọn — Compare Quantity (tham khảo)

1. Tạo config: `AR Learning > Create Compare Quantity Easy Config`.
2. Duplicate `SC_ARGameplay` thành scene tạm `SC_ARGameplay_CompareQuantity_Test.unity`.
3. Disable/xóa `QuantityMatchActivity`.
4. Tạo GameObject `CompareQuantityActivity` và thêm components: `CompareQuantityPresenter`, `CompareQuantityView`, `ActivityPrefabSetup`, `CompareQuantityRuntimeUI`, `CompareQuantityActivityBootstrap`.
5. Trong `CompareQuantityActivityBootstrap`, assign `presenter`, `view`, `config = SO_CompareQuantityConfig_Easy.asset`.
6. Play.

Hạn chế: `CompareQuantityRuntimeUI` chưa tạo UI hoàn chỉnh; `CompareQuantityPresenter.GetObjectPrefab()` có thể cảnh báo và dùng fallback placeholder.

---

## 12. Device AR — smoke test (tóm tắt)

Editor mock không thay thế device AR. Khi build lên Android/iOS:

1. Build `SC_TestSandbox` → camera chạy, plane detection khả dụng, spawn/tap hoạt động.
2. Build `SC_ARGameplay` → Quantity Match spawn trên plane thật, tap trả lời lưu kết quả.

Pass trên device khi: object hiển thị trên AR thật và tap hoạt động mà không cần keyboard/mock.

---

## 13. Checklist pass/fail tổng hợp

### Phase 1 pass

- [ ] Unity compile clean.
- [ ] `SC_TestSandbox` Play không lỗi.
- [ ] Console có `ARServiceBootstrap Ready`.
- [ ] `G` spawn grid.
- [ ] `C` spawn circle.
- [ ] `X` clear objects.
- [ ] Click object có tap/highlight.

### Phase 2 pass

- [ ] `SO_QuantityMatchConfig_Easy.asset` tồn tại.
- [ ] `SC_ARGameplay` Play không lỗi.
- [ ] Quantity Match auto-start.
- [ ] UI hiển thị target/progress/group buttons.
- [ ] Hint hoạt động.
- [ ] Wrong answer cho retry.
- [ ] Correct answer lưu result.
- [ ] Round 1 → Round 2 transition.
- [ ] Summary hoàn thành.
- [ ] `learning_progress.json` chứa result level 1 và 2.

### Phase 4 partial pass

- [ ] `SC_Boot` có `BootLoader`.
- [ ] `SC_MainMenu` có `MainMenuController` và 2 button.
- [ ] `SC_ActivitySelect` có `ActivitySelectController` và activity buttons.
- [ ] `SC_ProgressDashboard` có `ProgressDashboardView`.
- [ ] Build Settings có thứ tự scene đúng.
- [ ] Boot → Main Menu thành công.
- [ ] Main Menu → Progress Dashboard thành công.
- [ ] Main Menu → Activity Select thành công.
- [ ] Activity Select → Quantity Match thành công.

### Phase 4 full pass (thiếu)

- [ ] `SC_ARGameplay` có `ActivityLoader` hoặc dispatcher từ `SelectedActivityData`.
- [ ] Number Line Jump / Compare Quantity buttons bị disable nếu chưa playable hoặc load đúng activity.
- [ ] Các shell scenes đã được bake controller trong scene YAML (không phụ thuộc manual setup khi clone).

---

## 14. Lỗi thường gặp và cách xử lý

| Triệu chứng                                                   | Nguyên nhân thường gặp                         | Cách xử lý                                                            |
| ------------------------------------------------------------- | ---------------------------------------------- | --------------------------------------------------------------------- |
| Boot không load MainMenu                                      | Scene chưa có trong Build Settings             | Thực hiện mục 2.4                                                     |
| MainMenu chỉ hiển thị label                                   | Chưa chạy `Setup Main Menu Scene`              | Chạy lại menu setup shell                                             |
| Nút click nhưng không hoạt động                               | Canvas label placeholder che raycast           | Xóa placeholder hoặc tái tạo scene bằng menu setup                    |
| ActivitySelect bấm NumberLine/Compare nhưng vào QuantityMatch | `SC_ARGameplay` chỉ có QuantityMatch bootstrap | Thêm `ActivityLoader` hoặc disable button chưa playable               |
| Quantity Match không start                                    | Thiếu config/bootstrap/ARServiceBootstrap      | Chạy `Create Quantity Match Easy Config` và `Setup AR Gameplay Scene` |
| Object không spawn trong Editor                               | Mock placement/ARServiceBootstrap thiếu        | Test `SC_TestSandbox`, kiểm tra log `ARPlacementServiceMock`          |
| Tap object không nhận                                         | Collider hoặc interaction registration thiếu   | Dùng UI button fallback; kiểm tra `ARInteractionService`              |
| Không có progress JSON                                        | Save fail hoặc chưa lưu                        | Kiểm tra log `[ProgressStorageProxy] Result saved`                    |
| Dashboard hiển thị toàn 0                                     | Chưa có `learning_progress.json` hoặc path sai | Hoàn thành Quantity Match trước và kiểm tra file JSON                 |

---

## 15. Kết luận

Hệ thống có thể được xem là **sẵn sàng báo cáo Phase 2 pass trên local Editor** nếu checklist Phase 1 và Phase 2 đều được tick. Phase 4 chỉ nên được công nhận full-pass khi shell scenes đã bake controller, Build Settings đúng thứ tự và cold-start flow chạy từ `SC_Boot`, đồng thời Activity Select không dẫn người dùng vào activity chưa playable.
