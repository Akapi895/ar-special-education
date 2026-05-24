# Phase 2 Test Guide - Quantity Match Vertical Slice

**Mục tiêu:** xác nhận Phase 2 đã hoàn thiện ở mức vertical slice: mở `SC_ARGameplay` -> service AR/mock hoạt động -> Quantity Match tự start -> spawn nhóm vật thể -> hint/answer/feedback chạy -> kết quả được lưu vào JSON local.

> Guide này dành cho Unity `6000.0.71f1`, project `apps/unity-client`.

---

## 1. Chuẩn bị trong Unity

1. Mở Unity Hub.
2. Open project: `d:\.Kỳ II năm Ba\Chuyên đề\BTL\apps\unity-client`.
3. Chờ Unity compile xong.
4. Mở Console bằng menu `Window > General > Console`.
5. Bật các filter `Log`, `Warning`, `Error`.
6. Nhấn `Clear` trong Console để bắt đầu log sạch.

**Không được còn lỗi đỏ compile.** Nếu còn lỗi đỏ, chưa test Phase 2.

---

## 2. Tạo lại asset/scene cần thiết

Chạy các menu theo đúng thứ tự này trên thanh menu trên cùng của Unity:

1. `AR Learning > Create Quantity Match Easy Config`
2. `AR Learning > Repair Product Scenes`

Sau bước 1, trong Project panel cần thấy file:

`Assets/Features/Activities/QuantityMatch/ScriptableObjects/SO_QuantityMatchConfig_Easy.asset`

Sau bước 2, trong Project panel cần thấy các scene dưới:

- `Assets/_Project/Scenes/SC_TestSandbox.unity`
- `Assets/_Project/Scenes/SC_ARGameplay.unity`
- `Assets/_Project/Scenes/SC_Boot.unity`
- `Assets/_Project/Scenes/SC_MainMenu.unity`
- `Assets/_Project/Scenes/SC_ActivitySelect.unity`
- `Assets/_Project/Scenes/SC_ProgressDashboard.unity`

---

## 3. Smoke test AR sandbox

1. Trong Project panel, mở:
   `Assets/_Project/Scenes/SC_TestSandbox.unity`
2. Nhấn nút `Play`.
3. Nhìn Console, cần thấy các log kiểu:
   - `[ARSessionService] Initialized.`
   - `[ARServiceBootstrap] Ready. Placement=ARPlacementServiceMock`
   - `[ARSandbox] Placement=ARPlacementServiceMock, Available=True`

Trong Editor trên Windows, project dùng mock placement, nên không cần detect plane thật.

### Tương tác cần thử

Trong Game view:

1. Nhấn phím `G`.
   - Cần thấy vài sphere test spawn thành dạng grid.
   - Console có log `[ARSandbox] Spawned grid...`
2. Click vào một sphere.
   - Console có log `[ARSandbox] OnObjectTapped: ...`
   - Object được click phóng to/đổi highlight.
3. Nhấn phím `C`.
   - Cần thấy sphere spawn dạng vòng tròn.
   - Console có log `[ARSandbox] Spawned circle...`
4. Nhấn phím `X`.
   - Các object test biến mất.
   - Console có log `[ARSandbox] Cleared spawned objects.`
5. Nhấn phím `L`.
   - Console log trạng thái placement.

**Pass sandbox khi:** không có error đỏ, `G/C/X/L` hoạt động, click object có log tap.

---

## 4. Test Quantity Match trong SC_ARGameplay

1. Stop Play nếu đang chạy.
2. Mở scene:
   `Assets/_Project/Scenes/SC_ARGameplay.unity`
3. Nhấn `Play`.
4. Chờ khoảng 1-2 giây.

### Cần thấy lúc bắt đầu

Trong Game view:

- Có UI của Quantity Match.
- Có target number lớn.
- Có progress text kiểu `Question 1 of 2`.
- Có các nút `Hint`, `Cancel`, `Group 1`, `Group 2`, `Group 3`.
- Có các nhóm object được spawn trong Scene/Game view.

Trong Console cần thấy:

- `[ProgressStorageProxy] Session started: ...`
- `[QuantityMatchPresenter] Loading round 1: Target = 3`
- `[QuantityMatchActivityBootstrap] Quantity Match started.`

Config easy hiện tại có 2 round:

| Round | Target | Group 1 | Group 2 | Group 3 | Đáp án đúng |
|---|---:|---:|---:|---:|---|
| 1 | 3 | 2 objects | 3 objects | 4 objects | `Group 2` |
| 2 | 5 | 4 objects | 5 objects | 6 objects | `Group 2` |

---

## 5. Test hint

Ở Round 1:

1. Nhấn nút `Hint`.
2. Cần thấy hint panel hiện text:
   `Look carefully at the groups.`
3. Nhấn `Hint` lần 2.
4. Cần thấy hint có target được thay vào, ví dụ:
   `The number shown is 3, count each group.`
5. Nhấn `Hint` lần 3.
6. Cần thấy:
   `One group has exactly 3 objects.`

**Pass hint khi:** hint hiện trên UI, không error đỏ, text có thay `X` bằng target number ở hint 2/3.

---

## 6. Test trả lời sai

Ở Round 1, target là `3`, đáp án đúng là `Group 2`.

1. Nhấn `Group 1`.
2. Cần thấy feedback đỏ:
   `Not quite. Let's try again!`
3. Console cần thấy log gần giống:
   - `[QuantityMatchPresenter] Group selected: 0`
   - `-> INCORRECT`
   - `[ActivityPresenter] Incorrect answer. Attempt 1 of 3`

**Pass wrong answer khi:** app không chuyển round ngay, vẫn cho trả lời lại.

---

## 7. Test trả lời đúng và chuyển round

Ở Round 1:

1. Nhấn `Group 2`.
2. Cần thấy feedback xanh:
   `Great job! You found the right group!`
3. Console cần thấy:
   - `[QuantityMatchPresenter] Group selected: 1`
   - `-> CORRECT`
   - `[ProgressStorageProxy] Result saved: QuantityMatch L1 = True`
4. Chờ khoảng 2 giây.
5. UI cần chuyển sang `Question 2 of 2`.
6. Target number cần đổi thành `5`.

Ở Round 2:

1. Nhấn `Group 2`.
2. Cần thấy feedback đúng.
3. Console cần thấy:
   - `[ProgressStorageProxy] Result saved: QuantityMatch L2 = True`
4. Sau khi hoàn tất, cần thấy summary kiểu:
   - `Activity Complete!`
   - `Correct: True`
   - `Attempts: ...`
   - `Hints Used: ...`
   - `Time: ... seconds`

**Pass correct flow khi:** round 1 lưu kết quả, tự chuyển round 2, round 2 lưu kết quả và hiện complete summary.

---

## 8. Test tap trực tiếp vào AR object

Ngoài nút `Group 1/2/3`, cần test interaction service:

1. Restart Play trong `SC_ARGameplay`.
2. Khi object đã spawn, click trực tiếp vào nhóm object ở giữa (`Group 2`, nhóm có 3 object ở round 1).
3. Cần thấy app xử lý như chọn đúng:
   - feedback xanh;
   - Console có `OnObjectTapped` hoặc log `Group selected: 1`;
   - sau 2 giây chuyển round.

Nếu click trực tiếp khó trúng trong Game view, dùng fallback UI button vẫn đủ cho Phase 2 editor test. Tuy nhiên trên device, tap AR object nên hoạt động.

---

## 9. Kiểm tra file progress JSON

Local storage ghi vào `Application.persistentDataPath`.

Với current Project Settings:

- `companyName`: `DefaultCompany`
- `productName`: `unity-client`

Trên Windows, kiểm tra file:

`C:\Users\tuanp\AppData\LocalLow\DefaultCompany\unity-client\learning_progress.json`

Cách mở nhanh:

1. Mở File Explorer.
2. Dán path trên vào address bar.
3. Mở `learning_progress.json`.

Cần thấy JSON có `allResults` và ít nhất 2 result sau khi hoàn thành 2 round. Các field quan trọng:

- `"ActivityId": "QuantityMatch"`
- `"LevelNumber": 1`
- `"LevelNumber": 2`
- `"IsCorrect": true`
- `"TotalAttempts"` lớn hơn hoặc bằng 1
- `"HintsUsedCount"` phản ánh số hint đã bấm
- `"TimeSpentSeconds"` lớn hơn 0

Console cũng cần có:

- `[LocalProgressStorage] Saved result for QuantityMatch, Level 1...`
- `[LocalProgressStorage] Saved result for QuantityMatch, Level 2...`

---

## 10. Device/simulator acceptance

Editor mock pass là đủ để kiểm tra code path Phase 2. Nếu test trên device AR thật:

1. Build scene `SC_ARGameplay` lên device.
2. Mở app.
3. Camera cần chạy.
4. Khi có plane/placement valid, nhóm object cần spawn trên bề mặt.
5. Tap vào nhóm đúng phải submit answer.
6. Kết quả vẫn phải lưu local JSON.

**Pass device khi:** không cần dùng keyboard/mock, object hiện trong AR và tap trực tiếp chọn được nhóm.

---

## 11. Phase 2 được coi là hoàn thiện khi nào?

Tick hết các mục dưới:

- [ ] `Create Quantity Match Easy Config` chạy được.
- [ ] `Repair Product Scenes` chạy được.
- [ ] `SC_TestSandbox` Play không error đỏ.
- [ ] Trong sandbox, `G/C/X/L` hoạt động.
- [ ] Trong sandbox, click object có phản hồi tap/highlight.
- [ ] `SC_ARGameplay` Play không error đỏ.
- [ ] Quantity Match tự start sau khoảng 1-2 giây.
- [ ] UI hiện target/progress/hint/group buttons.
- [ ] Object groups được spawn.
- [ ] Hint hiển thị đúng.
- [ ] Chọn sai hiện feedback sai và cho retry.
- [ ] Chọn đúng round 1 lưu result và chuyển round 2.
- [ ] Chọn đúng round 2 lưu result và hiện complete summary.
- [ ] `learning_progress.json` có result của `QuantityMatch` level 1 và 2.
- [ ] Không còn các lỗi lặp trong Console:
  - `InvalidOperationException: UnityEngine.Input`
  - `ARSession reference is missing`
  - `There are 2 audio listeners`
  - `Unknown error occurred while loading ...`

Nếu tất cả mục trên pass, Phase 2 đạt code + editor E2E gate. Device gate chỉ pass sau khi test thêm trên ARCore/ARKit hoặc simulator AR tương đương.

---

## 12. Nếu test fail thì đọc lỗi theo hướng nào?

| Triệu chứng | Khả năng cao | Kiểm tra |
|---|---|---|
| Scene load lỗi | Scene chưa repair | Chạy `AR Learning > Repair Product Scenes` |
| Không có config | Chưa tạo SO | Chạy `AR Learning > Create Quantity Match Easy Config` |
| Không tự start | Missing bootstrap/config/service | Console log `QuantityMatchActivityBootstrap` |
| Không spawn object | Placement unavailable hoặc prefab setup thiếu | Console log `ARServiceBootstrap`, `ActivityPrefabSetup` |
| Bấm nhóm không phản hồi | UI/runtime event chưa chạy | Console log `Group selected` |
| Tap object không phản hồi | Collider/interaction registration | Console log `ARInteractionService`, test bằng UI button |
| Không lưu JSON | `ProgressStorageProxy` chưa tồn tại hoặc `SaveResult` không gọi | Console log `[ProgressStorageProxy] Result saved` |

