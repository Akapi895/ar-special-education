# Local Unity Editor Full Test Guide

**Muc tieu:** test toan bo tinh nang hien co cua Unity client tren local Unity Editor, va tach ro phan da playable voi phan moi co code khung.

**Project:** `apps/unity-client`  
**Unity target:** `6000.0.71f1`  
**Ngay lap guide:** 2026-05-21  
**Pham vi:** Phase 1 -> Phase 4, local-first, Editor/mock AR la chinh. Device AR la smoke test rieng.

---

## 0. Ket luan trang thai hien tai

| Phase | Trang thai hien tai | Muc do uoc tinh | Ghi chu |
|---|---|---:|---|
| Phase 0 - Scope freeze | Hoan thanh | 100% | Tai lieu va boundary da ro. |
| Phase 1 - AR Core + Sandbox | Gan hoan thanh ve code | 85-90% | Co `ARSessionService`, `ARPlacementService`, mock placement, interaction, sandbox scene. Can playtest lai trong Editor/device. |
| Phase 2 - Quantity Match vertical slice | Gan hoan thanh ve code + scene | 80-85% | Co config asset, `SC_ARGameplay`, runtime UI, mock spawn, feedback/progress hooks. Can chay E2E va doc JSON de xac nhan. |
| Phase 3 - Number Line Jump | Mot phan | 35-45% | Co presenter/bootstrap/config factory, nhung runtime UI builder dang placeholder, chua scene-wired. |
| Phase 4 - App shell | Code san sang, scene integration chua day du | 45-55% | Co scripts Boot/Main/Select/Dashboard, nhung shell scenes hien tai chua co controller trong YAML, Build Settings chua co shell scenes, `SC_ARGameplay` hien chi auto-start Quantity Match. |

**Danh gia ngan:** he thong hien nay da vuot xa skeleton cu va co the test duoc Phase 1 + Phase 2 trong Editor. Tuy nhien chua nen danh dau Phase 4 full-pass vi cold start flow va activity selection chua duoc bake/integrate hoan chinh vao scene hien tai.

---

## 1. Chuan bi truoc khi test

1. Mo Unity Hub.
2. Open project:
   `d:\.Ky II nam Ba\Chuyen de\BTL\apps\unity-client`
3. Cho Unity import/compile xong.
4. Mo Console: `Window > General > Console`.
5. Bat filter `Log`, `Warning`, `Error`.
6. Nhan `Clear`.
7. Dam bao **khong con loi compile mau do**.

Neu con compile errors, dung test gameplay va sua compile truoc.

---

## 2. Tao asset va scene can test

Chay cac menu sau trong Unity theo thu tu.

### 2.1 Tao config assets

1. `AR Learning > Create Quantity Match Easy Config`
2. `AR Learning > Create Number Line Jump Easy Config`
3. `AR Learning > Create Compare Quantity Easy Config`

Kiem tra cac file:

- `Assets/Features/Activities/QuantityMatch/ScriptableObjects/SO_QuantityMatchConfig_Easy.asset`
- `Assets/Features/Activities/NumberLineJump/ScriptableObjects/SO_NumberLineJumpConfig_Easy.asset`
- `Assets/Features/Activities/CompareQuantity/ScriptableObjects/SO_CompareQuantityConfig_Easy.asset`

**Luu y:** tai thoi diem lap guide, repo dang co san Quantity Match config. Hai config con lai can tao bang menu neu chua ton tai.

### 2.2 Tao AR scenes

1. `AR Learning > Setup Test Sandbox Scene`
2. `AR Learning > Setup AR Gameplay Scene (Quantity Match)`

Kiem tra:

- `Assets/_Project/Scenes/SC_TestSandbox.unity`
- `Assets/_Project/Scenes/SC_ARGameplay.unity`

### 2.3 Tao shell scenes

Chay tiep:

1. `AR Learning > Setup Scenes > Setup Boot Scene`
2. `AR Learning > Setup Scenes > Setup Main Menu Scene`
3. `AR Learning > Setup Scenes > Setup Activity Select Scene`
4. `AR Learning > Setup Scenes > Setup Progress Dashboard Scene`

Kiem tra:

- `Assets/_Project/Scenes/SC_Boot.unity`
- `Assets/_Project/Scenes/SC_MainMenu.unity`
- `Assets/_Project/Scenes/SC_ActivitySelect.unity`
- `Assets/_Project/Scenes/SC_ProgressDashboard.unity`

### 2.4 Build Settings cho shell flow

Mo `File > Build Profiles` hoac `File > Build Settings` tuy layout Unity.

Dam bao scene order:

1. `Assets/_Project/Scenes/SC_Boot.unity`
2. `Assets/_Project/Scenes/SC_MainMenu.unity`
3. `Assets/_Project/Scenes/SC_ActivitySelect.unity`
4. `Assets/_Project/Scenes/SC_ARGameplay.unity`
5. `Assets/_Project/Scenes/SC_ProgressDashboard.unity`
6. `Assets/_Project/Scenes/SC_TestSandbox.unity`

Neu `Assets/Scenes/SampleScene.unity` dang o dau danh sach, co the de lai de test template, nhung product shell nen bat dau tu `SC_Boot`.

**Can than voi menu `AR Learning > Repair Product Scenes`:** menu nay tao lai scene va build settings, nhung shell scenes co the chi con canvas label placeholder. Neu da chay menu nay, hay chay lai 4 menu `Setup Scenes` o muc 2.3 sau do.

---

## 3. Compile gate

Sau khi tao scene/config:

1. Doi Unity compile xong.
2. Console khong duoc co loi do.
3. Dac biet khong duoc con:
   - `CS0246 BootLoader could not be found`
   - `CS0246 MainMenuController could not be found`
   - `CS0246 ActivitySelectController could not be found`
   - `CS0246 ProgressDashboardView could not be found`
   - `CS0117 ProgressStorageProxy.Initialize`
   - `CS0117 FeedbackServiceProxy.Initialize`
   - `CS0029 LocalProgressStorage to ProgressStorageProxy`

Neu pass compile gate moi sang gameplay test.

---

## 4. Test Phase 1 - AR sandbox

1. Open scene:
   `Assets/_Project/Scenes/SC_TestSandbox.unity`
2. Nhan `Play`.
3. Trong Console can thay log gan giong:
   - `[ARPlacementServiceMock] Initialized (editor/mock mode).`
   - `[ARInteractionService] Initialized.`
   - `[ARServiceBootstrap] Ready. Placement=ARPlacementServiceMock`
   - `[ARSandbox] Placement=ARPlacementServiceMock, Available=True`

Trong Windows Editor, mock placement duoc dung, nen khong can plane detection that.

### 4.1 Keyboard test

Trong Game view:

1. Nhan `G`.
   - Can thay sphere spawn dang grid.
   - Console co `[ARSandbox] Spawned grid...`
2. Click vao mot sphere.
   - Console co `[ARSandbox] OnObjectTapped: ...`
   - Object duoc highlight/phong to.
3. Nhan `C`.
   - Can thay sphere spawn dang circle.
   - Console co `[ARSandbox] Spawned circle...`
4. Nhan `X`.
   - Object test bien mat.
   - Console co `[ARSandbox] Cleared spawned objects.`
5. Nhan `L`.
   - Console in placement status.

**Pass Phase 1 Editor gate khi:** G/C/X/L hoat dong, click object co tap log, khong co error do.

---

## 5. Test Phase 2 - Quantity Match truc tiep

1. Stop Play.
2. Open scene:
   `Assets/_Project/Scenes/SC_ARGameplay.unity`
3. Nhan `Play`.
4. Cho 1-2 giay.

### 5.1 Expected startup

Trong Game view:

- Co UI Quantity Match.
- Co target number.
- Co progress text, vi du `Question 1 of 2`.
- Co nut `Hint`, `Cancel`, `Group 1`, `Group 2`, `Group 3`.
- Co object groups spawn trong scene/game view.

Console can co:

- `[ARServiceBootstrap] Ready. Placement=ARPlacementServiceMock`
- `[ProgressStorageProxy] Session started: ...`
- `[QuantityMatchPresenter] Loading round 1: Target = 3`
- `[QuantityMatchActivityBootstrap] Quantity Match started.`

### 5.2 Round data hien tai

| Round | Target | Group 1 | Group 2 | Group 3 | Dap an dung |
|---|---:|---:|---:|---:|---|
| 1 | 3 | 2 objects | 3 objects | 4 objects | Group 2 |
| 2 | 5 | 4 objects | 5 objects | 6 objects | Group 2 |

### 5.3 Hint test

O Round 1:

1. Nhan `Hint`.
2. UI phai hien hint dau tien.
3. Nhan `Hint` lan 2.
4. UI phai hien hint tiep theo, co noi den target/current question.
5. Nhan `Hint` lan 3.
6. UI phai hien hint cuoi.

**Pass khi:** hint hien tren UI, `HintsUsedCount` tang trong result sau khi save, khong error do.

### 5.4 Wrong answer test

O Round 1, target la `3`, dap an dung la `Group 2`.

1. Nhan `Group 1`.
2. UI phai hien feedback sai.
3. App khong chuyen round ngay.
4. Console can co log gan giong:
   - `[QuantityMatchPresenter] Group selected: 0`
   - `-> INCORRECT`
   - `[ActivityPresenter] Incorrect answer. Attempt 1 of 3`

### 5.5 Correct answer + round transition

O Round 1:

1. Nhan `Group 2`.
2. UI phai hien feedback dung.
3. Console can co:
   - `[QuantityMatchPresenter] Group selected: 1`
   - `-> CORRECT`
   - `[ProgressStorageProxy] Result saved: QuantityMatch L1 = True`
4. Cho khoang 2 giay.
5. UI chuyen sang `Question 2 of 2`, target `5`.
6. Nhan `Group 2`.
7. Console can co:
   - `[ProgressStorageProxy] Result saved: QuantityMatch L2 = True`
8. UI hien summary complete.

**Pass Phase 2 Editor gate khi:** hoan thanh du 2 round, co save result, co feedback, khong error do.

---

## 6. Test tap truc tiep vao AR object

1. Restart Play trong `SC_ARGameplay`.
2. Khi object spawn xong, click vao group giua o Round 1.
3. App nen xu ly nhu chon `Group 2`.
4. Console nen co tap log hoac `Group selected: 1`.

Neu click trong Game view kho trung, UI button fallback van du cho Editor E2E. Tren device, tap object nen duoc test rieng.

---

## 7. Test local progress JSON

Current Project Settings:

- `companyName`: `DefaultCompany`
- `productName`: `unity-client`

Windows persistent path du kien:

`C:\Users\tuanp\AppData\LocalLow\DefaultCompany\unity-client\learning_progress.json`

Sau khi hoan thanh Quantity Match:

1. Mo File Explorer.
2. Dan path tren vao address bar.
3. Mo `learning_progress.json`.
4. Kiem tra co `allResults` va it nhat 2 result.

Field can thay:

- `"ActivityId": "QuantityMatch"`
- `"LevelNumber": 1`
- `"LevelNumber": 2`
- `"IsCorrect": true`
- `"TotalAttempts"` >= 1
- `"HintsUsedCount"` dung voi so hint da bam
- `"TimeSpentSeconds"` > 0
- `startTimeString` / `endTimeString` co ISO timestamp

**Pass khi:** file ton tai, du lieu round 1/2 duoc ghi, restart Unity Play van doc lai duoc dashboard.

---

## 8. Test Progress Dashboard

1. Stop Play.
2. Dam bao da chay:
   `AR Learning > Setup Scenes > Setup Progress Dashboard Scene`
3. Open scene:
   `Assets/_Project/Scenes/SC_ProgressDashboard.unity`
4. Nhan `Play`.

Can thay:

- Overall Progress:
  - Activities Completed
  - Total Sessions
  - Total Results
- Activity Statistics:
  - QuantityMatch co Attempts, Success Rate, Avg Time, Best Time, Hints Used
  - NumberLineJump / CompareQuantity co the hien `No data yet`

Nhan `Back`:

- Neu `SC_MainMenu` co trong Build Settings, app load ve main menu.
- Neu chua co Build Settings, Unity se bao scene khong load duoc. Quay lai muc 2.4.

---

## 9. Test Phase 4 - Shell flow

### 9.1 Setup truoc shell test

Dam bao da chay 4 menu:

- `AR Learning > Setup Scenes > Setup Boot Scene`
- `AR Learning > Setup Scenes > Setup Main Menu Scene`
- `AR Learning > Setup Scenes > Setup Activity Select Scene`
- `AR Learning > Setup Scenes > Setup Progress Dashboard Scene`

Dam bao Build Settings co scene order o muc 2.4.

### 9.2 Cold start flow

1. Open:
   `Assets/_Project/Scenes/SC_Boot.unity`
2. Nhan `Play`.
3. Console can co:
   - `[BootLoader] Services initialized.`
4. Sau khoang 0.5 giay, app load `SC_MainMenu`.
5. Trong Main Menu, nhan `View Progress`.
6. App load `SC_ProgressDashboard`.
7. Nhan `Back`.
8. App ve `SC_MainMenu`.
9. Nhan `Start Learning`.
10. App load `SC_ActivitySelect`.
11. Nhan `Quantity Match`.
12. App load `SC_ARGameplay`.

**Expected current behavior:** `SC_ARGameplay` hien tai duoc setup bang `Setup AR Gameplay Scene (Quantity Match)`, nen Quantity Match auto-start. Day la path test chinh.

### 9.3 Activity Select known gap

Hien tai code co `ActivityLoader` ho tro `QuantityMatch`, `NumberLineJump`, `CompareQuantity`, nhung scene `SC_ARGameplay` hien tai khong co `ActivityLoader`; scene chi co `QuantityMatchActivityBootstrap`.

Vi vay:

- `Quantity Match` button co the pass.
- `Number Line Jump` va `Compare Quantity` button **khong duoc tinh la pass Phase 4 full** neu van load vao Quantity Match hoac khong load dung activity.
- De Phase 4 full-pass, can mot trong hai huong:
  - disable cac button chua playable; hoac
  - bake `ActivityLoader` vao `SC_ARGameplay` va assign presenter/view/config cho tung activity.

---

## 10. Optional manual test - Number Line Jump

Day la test tham khao vi feature chua scene-wired va runtime UI builder dang placeholder.

1. Tao config:
   `AR Learning > Create Number Line Jump Easy Config`
2. Duplicate `SC_ARGameplay` thanh scene tam, vi du `SC_ARGameplay_NumberLineJump_Test.unity`.
3. Trong scene tam, disable hoac delete `QuantityMatchActivity`.
4. Tao empty GameObject `NumberLineJumpActivity`.
5. Add components:
   - `NumberLineJumpPresenter`
   - `NumberLineJumpView`
   - `ActivityPrefabSetup`
   - `NumberLineJumpRuntimeUI`
   - `NumberLineJumpActivityBootstrap`
6. Trong `NumberLineJumpActivityBootstrap`, assign:
   - `presenter` = component `NumberLineJumpPresenter`
   - `view` = component `NumberLineJumpView`
   - `config` = `SO_NumberLineJumpConfig_Easy.asset`
7. Play.

**Expected current limitation:** `NumberLineJumpRuntimeUI` log `Runtime UI creation not yet implemented`. Neu khong assign UI refs thu cong, activity co the start logic nhung khong test duoc full UI. Khong tinh Phase 3 pass neu UI/scene chua hoan thien.

---

## 11. Optional manual test - Compare Quantity

Day la test tham khao vi feature chua scene-wired va runtime UI builder dang placeholder.

1. Tao config:
   `AR Learning > Create Compare Quantity Easy Config`
2. Duplicate `SC_ARGameplay` thanh scene tam, vi du `SC_ARGameplay_CompareQuantity_Test.unity`.
3. Disable hoac delete `QuantityMatchActivity`.
4. Tao empty GameObject `CompareQuantityActivity`.
5. Add components:
   - `CompareQuantityPresenter`
   - `CompareQuantityView`
   - `ActivityPrefabSetup`
   - `CompareQuantityRuntimeUI`
   - `CompareQuantityActivityBootstrap`
6. Trong `CompareQuantityActivityBootstrap`, assign:
   - `presenter` = component `CompareQuantityPresenter`
   - `view` = component `CompareQuantityView`
   - `config` = `SO_CompareQuantityConfig_Easy.asset`
7. Play.

**Expected current limitation:**

- `CompareQuantityRuntimeUI` chua tao UI.
- `CompareQuantityPresenter.GetObjectPrefab()` dang warning va fallback placeholder group.
- Khong tinh Phase 6 pass neu UI/scene/prefab chua duoc lam that.

---

## 12. Device AR smoke test

Editor mock pass khong thay the device AR pass.

Khi build len Android/iOS:

1. Build scene `SC_TestSandbox`.
2. Camera phai chay.
3. Plane detection phai san sang.
4. Spawn grid/circle hoac tap object phai hoat dong.
5. Build `SC_ARGameplay`.
6. Quantity Match object groups phai spawn tren mat phang that.
7. Tap group dung phai submit answer.
8. Result phai duoc luu local.

**Device pass khi:** khong can keyboard/mock, object hien trong AR va tap truc tiep duoc.

---

## 13. Checklist pass/fail tong hop

### Phase 1 pass

- [ ] Unity compile clean.
- [ ] `SC_TestSandbox` Play khong error do.
- [ ] Console co `ARServiceBootstrap Ready`.
- [ ] `G` spawn grid.
- [ ] `C` spawn circle.
- [ ] `X` clear objects.
- [ ] Click object co tap/highlight.

### Phase 2 pass

- [ ] `SO_QuantityMatchConfig_Easy.asset` ton tai.
- [ ] `SC_ARGameplay` Play khong error do.
- [ ] Quantity Match auto-start.
- [ ] UI hien target/progress/group buttons.
- [ ] Hint hien dung.
- [ ] Wrong answer cho retry.
- [ ] Correct answer luu result.
- [ ] Round 1 -> Round 2 transition.
- [ ] Complete summary hien.
- [ ] `learning_progress.json` co result level 1 va 2.

### Phase 4 partial pass

- [ ] `SC_Boot` co `BootLoader`.
- [ ] `SC_MainMenu` co `MainMenuController` va 2 button.
- [ ] `SC_ActivitySelect` co `ActivitySelectController` va activity buttons.
- [ ] `SC_ProgressDashboard` co `ProgressDashboardView`.
- [ ] Build Settings co shell scenes dung order.
- [ ] Boot -> Main Menu thanh cong.
- [ ] Main Menu -> Progress Dashboard thanh cong.
- [ ] Main Menu -> Activity Select thanh cong.
- [ ] Activity Select -> Quantity Match thanh cong.

### Phase 4 full pass con thieu

- [ ] `SC_ARGameplay` co `ActivityLoader` hoac co dispatcher tu `SelectedActivityData`.
- [ ] Number Line Jump button bi disable neu chua playable, hoac load dung Number Line Jump.
- [ ] Compare Quantity button bi disable neu chua playable, hoac load dung Compare Quantity.
- [ ] `SC_Boot/MainMenu/ActivitySelect/ProgressDashboard` da duoc bake san trong scene YAML, khong phu thuoc manual setup moi lan clone.

---

## 14. Loi thuong gap

| Trieu chung | Nguyen nhan hay gap | Cach xu ly |
|---|---|---|
| Boot khong load MainMenu | Scene chua nam trong Build Settings | Lam muc 2.4. |
| MainMenu chi hien label scene | Chua chay `Setup Main Menu Scene` | Chay lai menu setup shell. |
| Nut bi click khong an | Canvas label placeholder co the chan raycast | Xoa canvas/label placeholder cu hoac tao lai scene bang setup menu sach. |
| ActivitySelect bam NumberLine/Compare nhung vao QuantityMatch | `SC_ARGameplay` dang chi co QuantityMatch bootstrap | Known gap Phase 4, can ActivityLoader hoac disable button. |
| Quantity Match khong start | Config/bootstrap/ARServiceBootstrap thieu | Chay lai `Create Quantity Match Easy Config` va `Setup AR Gameplay Scene`. |
| Object khong spawn trong Editor | Mock placement/ARServiceBootstrap thieu | Test `SC_TestSandbox`, kiem log `ARPlacementServiceMock`. |
| Tap object khong nhan | Collider hoac interaction registration | Dung UI button fallback; kiem log `ARInteractionService`. |
| Khong co progress JSON | Chua tra loi dung hoac save fail | Kiem log `[ProgressStorageProxy] Result saved`. |
| Dashboard toan 0 | Chua co `learning_progress.json` hoac wrong persistent path | Hoan thanh Quantity Match truoc. |

---

## 15. Ket luan test

He thong duoc xem la **san sang bao cao Phase 2 pass tren local Editor** khi checklist Phase 1 va Phase 2 deu tick het.

He thong chi nen duoc xem la **Phase 4 pass day du** khi shell scenes da bake controller, Build Settings da dung, cold start flow chay tu `SC_Boot`, va activity select khong danh lua nguoi dung bang cac button chua load dung activity.

