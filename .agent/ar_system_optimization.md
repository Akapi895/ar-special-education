# Káº¿ Hoáº¡ch Tá»‘i Æ¯u HÃ³a Há»‡ Thá»‘ng AR, Logic VÃ  Gameplay

**NgÃ y cáº­p nháº­t:** 2026-05-26
**Pháº¡m vi:** Unity client `apps/unity-client`, trá»ng tÃ¢m lÃ  AR Core, luá»“ng gameplay, logic bÃ i há»c, UI/UX há»c toÃ¡n cho tráº» 4-6 tuá»•i vÃ  tráº» khÃ³ há»c toÃ¡n.
**Nguá»“n rÃ  soÃ¡t:** code hiá»‡n táº¡i trong `Assets/Core`, `Assets/Features`, `Assets/_Project`, cÃ¡c scene `SC_*`, cÃ¡c config `SO_*_Easy.asset`, vÃ  cÃ¡c tÃ i liá»‡u `.agent`.

## 1. Káº¿t Luáº­n Tá»•ng Quan

Há»‡ thá»‘ng hiá»‡n Ä‘Ã£ cÃ³ ná»n táº£ng AR vÃ  há»c táº­p khÃ¡ Ä‘áº§y Ä‘á»§ Ä‘á»ƒ lÃ m MVP local-first:

- CÃ³ `ARSessionService`, `ARPlacementService`, `ARPlacementServiceMock`, `ARInteractionService`.
- CÃ³ `SC_TestSandbox` Ä‘á»ƒ smoke test AR vÃ  `SC_ARGameplay` Ä‘á»ƒ cháº¡y bÃ i há»c.
- CÃ³ `ActivityPresenter`, hint, feedback hook, progress local JSON.
- CÃ³ 3 activity: `QuantityMatch`, `NumberLineJump`, `CompareQuantity`.
- CÃ³ runtime UI fallback vÃ  config easy cho cáº£ 3 activity.

Tuy nhiÃªn, náº¿u Ä‘Ã¡nh giÃ¡ theo tiÃªu chuáº©n má»™t há»‡ thá»‘ng hoÃ n thiá»‡n vá» logic vÃ  gameplay cho tráº» khÃ³ há»c toÃ¡n, há»‡ thá»‘ng váº«n cÃ²n nhiá»u thiáº¿u sÃ³t. CÃ¡c thiáº¿u sÃ³t chÃ­nh náº±m á»Ÿ 8 nhÃ³m:

| NhÃ³m | Má»©c Ä‘á»™ | TÃ³m táº¯t |
|---|---:|---|
| AR placement vÃ  stability | P0 | ChÆ°a cÃ³ learning-area calibration cháº·t, activity váº«n spawn theo world position |
| AR interaction | P0 | Tap object Ä‘ang láº«n giá»¯a Ä‘áº¿m, chá»n vÃ  ná»™p Ä‘Ã¡p Ã¡n |
| Gameplay flow | P0-P1 | Routing activity cÃ²n phÃ¢n tÃ¡n, cold-start chÆ°a thá»‘ng nháº¥t, round flow chÆ°a Ä‘á»§ sÆ° pháº¡m |
| Ná»™i dung há»c toÃ¡n | P1 | CÃ³ round easy nhÆ°ng chÆ°a cÃ³ giÃ¡o trÃ¬nh chia nhá», adaptive progression |
| UI/UX tráº» nhá» | P1 | Runtime UI dÃ¹ng Ä‘á»ƒ test, chÆ°a pháº£i giao diá»‡n cuá»‘i cho tráº» 4-6 tuá»•i |
| Audio/VFX vÃ  pháº£n há»“i Ä‘a giÃ¡c quan | P1 | CÃ³ hook nhÆ°ng chÆ°a cÃ³ playback/asset tháº­t |
| Data/progress/analytics | P1-P2 | CÃ³ lÆ°u káº¿t quáº£ nhÆ°ng chÆ°a Ä‘á»§ phÃ¢n tÃ­ch lá»—i vÃ  mastery |
| Kiá»ƒm thá»­ vÃ  build device | P0-P2 | ChÆ°a cÃ³ báº±ng chá»©ng device pass, test tá»± Ä‘á»™ng cÃ²n thiáº¿u |

## 2. Hiá»‡n Tráº¡ng Ká»¹ Thuáº­t Quan Trá»ng

### 2.1 AR services Ä‘Ã£ cÃ³

| ThÃ nh pháº§n | File | Vai trÃ² |
|---|---|---|
| Session | `Assets/Core/AR/ARSession/ARSessionService.cs` | Wrap AR Foundation session state |
| Session bootstrap | `Assets/Core/AR/ARSession/ARSessionBootstrap.cs` | Resolve `ARSession`, `XROrigin`, camera, plane/raycast manager |
| Placement | `Assets/Core/AR/Placement/ARPlacementService.cs` | Raycast plane, spawn grid/circle/object |
| Placement mock | `Assets/Core/AR/Placement/ARPlacementServiceMock.cs` | Mock vá»‹ trÃ­ trong Editor |
| Placement controller | `Assets/Core/AR/Placement/ARPlacementController.cs` | Tap-to-place `LearningAreaAnchor`, spawn object dÆ°á»›i anchor |
| Learning area | `Assets/Core/AR/Placement/LearningAreaAnchor.cs` | Anchor vÃ¹ng há»c táº­p |
| Interaction | `Assets/Core/AR/Interaction/ARInteractionService.cs` | Physics raycast, tap/select/drag/highlight |
| Plane detection | `Assets/Core/AR/PlaneDetection/ARPlaneDetectionController.cs` | Theo dÃµi plane há»£p lá»‡ |
| Bootstrap | `Assets/Core/AR/ARServiceBootstrap.cs` | Resolve service cho activity |

### 2.2 Activity/gameplay Ä‘Ã£ cÃ³

| ThÃ nh pháº§n | File | Vai trÃ² |
|---|---|---|
| Base presenter | `Assets/Core/Learning/ActivityRunner/ActivityPresenter.cs` | Round state, hint, feedback, save result |
| Quantity Match | `Assets/Features/Activities/QuantityMatch/Scripts/*` | GhÃ©p sá»‘ lÆ°á»£ng, chá»n nhÃ³m hoáº·c nháº­p sá»‘ |
| Number Line Jump | `Assets/Features/Activities/NumberLineJump/Scripts/*` | Nháº£y trÃªn trá»¥c sá»‘ |
| Compare Quantity | `Assets/Features/Activities/CompareQuantity/Scripts/*` | So sÃ¡nh nhiá»u hÆ¡n/Ã­t hÆ¡n/báº±ng nhau |
| Activity routing | `Assets/_Project/Scripts/GameplayActivityRouter.cs` | Táº¡o activity runtime cho NumberLine/Compare |
| Activity loader | `Assets/_Project/Scripts/ActivityLoader.cs` | Loader khÃ¡c theo selected activity, hiá»‡n khÃ´ng tháº¥y gáº¯n trong `SC_ARGameplay` |
| Scene services | `Assets/_Project/Scripts/LearningSceneServices.cs` | Táº¡o progress/feedback/router khi vÃ o gameplay |
| Flow navigator | `Assets/_Project/Scripts/ActivityFlowNavigator.cs` | Chuyá»ƒn next activity/dashboard |

## 3. Danh SÃ¡ch Váº¥n Äá» VÃ  Thiáº¿u SÃ³t Hiá»‡n Táº¡i

### A. AR Placement, Anchor VÃ  á»”n Äá»‹nh KhÃ´ng Gian

#### AR-01. `LearningAreaAnchor` chÆ°a lÃ  nguá»“n tá»a Ä‘á»™ chÃ­nh cá»§a activity

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `LearningAreaAnchor.cs`, `ARPlacementController.cs`, `ARPlacementService.cs`, cÃ¡c `*Presenter.cs`

**Váº¥n Ä‘á»:**
Project Ä‘Ã£ cÃ³ `LearningAreaAnchor` vÃ  `ARPlacementController`, nhÆ°ng activity hiá»‡n váº«n chá»§ yáº¿u láº¥y `placementService.CurrentPlacementPosition`, rá»“i cá»™ng offset world-space Ä‘á»ƒ spawn group/number line. Äiá»u nÃ y lÃ m layout phá»¥ thuá»™c vÃ o world coordinate táº¡m thá»i thay vÃ¬ má»™t vÃ¹ng há»c táº­p Ä‘Ã£ Ä‘Æ°á»£c tráº»/phá»¥ huynh xÃ¡c nháº­n.

**Rá»§i ro gameplay:**

- Object round sau cÃ³ thá»ƒ lá»‡ch khá»i vÃ¹ng há»c ban Ä‘áº§u.
- Tráº» xoay camera hoáº·c tracking drift cÃ³ thá»ƒ tháº¥y nhÃ³m váº­t thá»ƒ trÆ°á»£t khá»i bÃ n/sÃ n.
- KhÃ³ reset/reposition toÃ n bá»™ bÃ i há»c vÃ¬ khÃ´ng cÃ³ má»™t root chung cho content.

**Cáº§n bá»• sung:**

- `IARPlacementService` nÃªn expose `LearningAreaAnchor CurrentLearningArea`, `Transform ContentRoot`, `bool HasLearningArea`.
- CÃ¡c activity spawn object dÆ°á»›i `LearningAreaAnchor.ContentRoot`.
- Presenter chá»‰ dÃ¹ng local offsets trong vÃ¹ng há»c, khÃ´ng tá»± quáº£n world coordinate.

#### AR-02. Thiáº¿u phase calibration trÆ°á»›c khi báº¯t Ä‘áº§u bÃ i há»c

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `SC_ARGameplay.unity`, `ARServiceBootstrap.cs`, `ARPlacementService.cs`, `LearningSceneServices.cs`

**Váº¥n Ä‘á»:**
`SC_ARGameplay` hiá»‡n cÃ³ xu hÆ°á»›ng khá»Ÿi Ä‘á»™ng activity nhanh. TrÃªn device tháº­t, AR cáº§n thá»i gian tÃ¬m máº·t pháº³ng, chá»n vÃ¹ng há»c vÃ  á»•n Ä‘á»‹nh tracking. Náº¿u vÃ o bÃ i ngay, tráº» cÃ³ thá»ƒ tháº¥y object khÃ´ng xuáº¥t hiá»‡n, spawn sai chá»— hoáº·c bá»‹ máº¥t tracking giá»¯a cÃ¢u.

**Cáº§n bá»• sung state machine gameplay:**

```text
Boot gameplay scene
-> Check AR support/permission
-> Plane scanning
-> Plane found
-> Child/parent taps to place learning area
-> Lock learning area
-> Hide visual clutter
-> Start selected activity
```

**Acceptance criteria:**

- Activity khÃ´ng start khi chÆ°a cÃ³ plane/anchor há»£p lá»‡ trÃªn device.
- Editor mock váº«n cÃ³ Ä‘Æ°á»ng Ä‘i nhanh Ä‘á»ƒ test.
- UI cÃ³ hÆ°á»›ng dáº«n tiáº¿ng Viá»‡t cho tá»«ng bÆ°á»›c quÃ©t/Ä‘áº·t vÃ¹ng há»c.

#### AR-03. `ARPlacementService` vÃ  `ARPlacementController` bá»‹ chá»“ng trÃ¡ch nhiá»‡m

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ARPlacementService.cs`, `ARPlacementController.cs`

**Váº¥n Ä‘á»:**
Hai class cÃ¹ng liÃªn quan Ä‘áº¿n placement nhÆ°ng khÃ´ng rÃµ class nÃ o lÃ  API chÃ­nh cho gameplay. `ARPlacementController` cÃ³ learning area anchor, cÃ²n `ARPlacementService` lÃ  service mÃ  activity Ä‘ang dÃ¹ng. Sá»± phÃ¢n tÃ¡ch nÃ y dá»… lÃ m feature má»›i dÃ¹ng sai Ä‘Æ°á»ng.

**Cáº§n bá»• sung:**

- Chá»n má»™t Ä‘Æ°á»ng chÃ­nh: activity chá»‰ dÃ¹ng `IARPlacementService`.
- Merge hoáº·c bridge `ARPlacementController` vÃ o `ARPlacementService`.
- Náº¿u giá»¯ cáº£ hai, tÃ i liá»‡u hÃ³a rÃµ: controller lÃ  UI/calibration, service lÃ  API spawn.

#### AR-04. Tracking/session quality chÆ°a Ä‘Æ°á»£c dÃ¹ng lÃ m gameplay gate

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `ARSessionService.cs`, `ARSessionBootstrap.cs`, `ActivityPresenter.cs`, `LearningSceneServices.cs`

**Váº¥n Ä‘á»:**
`IARSessionService` cÃ³ `IsSessionReady`, `IsTrackingStable`, `TrackingQuality`, nhÆ°ng activity flow chÆ°a dÃ¹ng cÃ¡c tráº¡ng thÃ¡i nÃ y Ä‘á»ƒ pause/resume hoáº·c cáº£nh bÃ¡o khi tracking kÃ©m.

**Rá»§i ro gameplay:**

- Khi tracking máº¥t, tráº» váº«n báº¥m tráº£ lá»i trong lÃºc object khÃ´ng cÃ²n chÃ­nh xÃ¡c.
- Save result cÃ³ thá»ƒ ghi sai vÃ¬ lá»—i ká»¹ thuáº­t bá»‹ tÃ­nh nhÆ° lá»—i há»c táº­p.

**Cáº§n bá»• sung:**

- Khi tracking lost: pause input, hiá»‡n overlay "Con giá»¯ mÃ¡y cháº­m láº¡i nhÃ©".
- Khi tracking stable láº¡i: resume hoáº·c há»i tráº» cÃ³ muá»‘n Ä‘áº·t láº¡i vÃ¹ng há»c khÃ´ng.
- Result nÃªn phÃ¢n biá»‡t `TechnicalFailure` vá»›i `WrongAnswer`.

#### AR-05. Plane visualization cÃ²n thiÃªn vá» debug, chÆ°a thÃ¢n thiá»‡n tráº» nhá»

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ARPlaneDetectionController.cs`, scene AR, plane material/prefab

**Váº¥n Ä‘á»:**
Plane mesh/line renderer máº·c Ä‘á»‹nh thÆ°á»ng gÃ¢y rá»‘i máº¯t. Tráº» khÃ³ há»c toÃ¡n dá»… bá»‹ máº¥t táº­p trung náº¿u lÆ°á»›i quÃ©t cÃ²n hiá»‡n sau khi bÃ i há»c báº¯t Ä‘áº§u.

**Cáº§n bá»• sung:**

- Material plane má»m, Ã­t nhiá»…u, cÃ³ thá»ƒ dÃ¹ng sparkles ráº¥t nháº¹.
- Sau khi Ä‘áº·t learning area, áº©n toÃ n bá»™ plane visualizer.
- Chá»‰ giá»¯ marker vÃ¹ng há»c hoáº·c tháº£m há»c AR.

#### AR-06. ChÆ°a cÃ³ rÃ ng buá»™c kÃ­ch thÆ°á»›c vÃ¹ng há»c vÃ  an toÃ n bá»‘ cá»¥c

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `LearningAreaAnchor.cs`, `QuantityMatchPresenter.cs`, `CompareQuantityPresenter.cs`, `NumberLineJumpPresenter.cs`

**Váº¥n Ä‘á»:**
`LearningAreaAnchor` cÃ³ `areaSizeMeters`, nhÆ°ng activity chÆ°a dÃ¹ng Ä‘á»ƒ kiá»ƒm tra object cÃ³ vÆ°á»£t khá»i vÃ¹ng há»c khÃ´ng.

**Cáº§n bá»• sung:**

- Layout solver tÃ­nh bounding box cá»§a group/number line.
- Náº¿u khÃ´ng Ä‘á»§ diá»‡n tÃ­ch, yÃªu cáº§u tráº» chá»n máº·t pháº³ng lá»›n hÆ¡n.
- Tá»± scale layout theo diá»‡n tÃ­ch vÃ  khoáº£ng cÃ¡ch camera.

#### AR-07. ChÆ°a cÃ³ cáº£nh bÃ¡o khoáº£ng cÃ¡ch camera/safety

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** AR session/camera service, UI overlay

**Váº¥n Ä‘á»:**
Tráº» cÃ³ thá»ƒ Ä‘Æ°a mÃ¡y quÃ¡ sÃ¡t máº·t pháº³ng/object hoáº·c di chuyá»ƒn nhanh. Há»‡ thá»‘ng chÆ°a cÃ³ cáº£nh bÃ¡o khoáº£ng cÃ¡ch vÃ  tá»‘c Ä‘á»™ camera.

**Cáº§n bá»• sung:**

- Cáº£nh bÃ¡o "Con lÃ¹i ra má»™t chÃºt" khi camera quÃ¡ gáº§n.
- Cáº£nh bÃ¡o "Con di chuyá»ƒn cháº­m láº¡i" khi tracking kÃ©m do chuyá»ƒn Ä‘á»™ng nhanh.
- TÃ¹y chá»n nghá»‰ sau má»™t sá»‘ phÃºt Ä‘á»ƒ báº£o vá»‡ máº¯t.

### B. AR Interaction VÃ  Hitbox

#### INT-01. Tap-to-count bá»‹ láº«n vá»›i tap-to-submit

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `QuantityMatchPresenter.cs`

**Váº¥n Ä‘á»:**
Trong Quantity Match, tap vÃ o group/object cÃ³ thá»ƒ bá»‹ hiá»ƒu lÃ  chá»n vÃ  ná»™p Ä‘Ã¡p Ã¡n. Vá»›i tráº» 4-6 tuá»•i, tap tá»«ng váº­t Ä‘á»ƒ Ä‘áº¿m lÃ  hÃ nh vi tá»± nhiÃªn. Náº¿u tap Ä‘á»ƒ Ä‘áº¿m láº¡i bá»‹ submit, tráº» dá»… sai khÃ´ng pháº£i vÃ¬ khÃ´ng hiá»ƒu toÃ¡n mÃ  vÃ¬ UX.

**Cáº§n bá»• sung:**

- Tap object Ä‘Æ¡n láº»: chá»‰ Ä‘áº¿m/highlight/bounce/audio "má»™t, hai, ba".
- Chá»n Ä‘Ã¡p Ã¡n: tap nhÃ£n group lá»›n, tap button UI, hoáº·c double-confirm.
- LÆ°u tráº¡ng thÃ¡i object Ä‘Ã£ Ä‘áº¿m Ä‘á»ƒ trÃ¡nh Ä‘áº¿m trÃ¹ng.

#### INT-02. Number input mode Ä‘ang lÃ m máº¥t há»— trá»£ Ä‘áº¿m trá»±c quan

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `QuantityMatchPresenter.cs`, `QuantityMatchView.cs`

**Váº¥n Ä‘á»:**
á»ž cÃ¡c round nháº­p sá»‘, `HandleObjectTapped` bá» qua tap khi `currentUsesNumberInputMode = true`. ÄÃ¢y láº¡i lÃ  lÃºc tráº» cáº§n cháº¡m-Ä‘áº¿m nháº¥t, vÃ¬ sá»‘ lÆ°á»£ng lá»›n hÆ¡n.

**Cáº§n bá»• sung:**

- Cho phÃ©p tap object Ä‘á»ƒ Ä‘áº¿m trong number input mode.
- Sau khi Ä‘áº¿m, UI numpad cÃ³ thá»ƒ gá»£i Ã½ sá»‘ hiá»‡n táº¡i nhÆ°ng khÃ´ng tá»± submit.
- CÃ³ nÃºt "Ä‘áº¿m láº¡i" Ä‘á»ƒ reset highlight.

#### INT-03. Hitbox/collider chÆ°a Ä‘Æ°á»£c thiáº¿t káº¿ riÃªng cho tráº» nhá»

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ARPlacementService.cs`, `ARPlacementServiceMock.cs`, `ARInteractionService.cs`, activity prefabs

**Váº¥n Ä‘á»:**
Service tá»± thÃªm `SphereCollider` fallback vá»›i radius nhá». Animal prefab cÃ³ hÃ¬nh dáº¡ng khÃ¡c nhau, object AR nhá» vÃ  tráº» dÃ¹ng tay cháº¡m mÃ n hÃ¬nh thiáº¿u chÃ­nh xÃ¡c, nÃªn collider theo model tháº­t cÃ³ thá»ƒ quÃ¡ khÃ³ báº¥m.

**Cáº§n bá»• sung:**

- Invisible child hitbox lá»›n hÆ¡n visual model.
- Hitbox group label lá»›n vÃ  á»•n Ä‘á»‹nh.
- Debug overlay trong sandbox Ä‘á»ƒ tháº¥y vÃ¹ng báº¥m.
- Min touch target theo pixel trÃªn mÃ n hÃ¬nh, khÃ´ng chá»‰ theo world size.

#### INT-04. UI raycast cÃ³ thá»ƒ cháº·n tÆ°Æ¡ng tÃ¡c AR ngoÃ i Ã½ muá»‘n

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ARInteractionService.cs`, runtime UI view

**Váº¥n Ä‘á»:**
`ARInteractionService` bá» qua raycast AR náº¿u pointer Ä‘ang over UI. Runtime UI overlay full-screen hoáº·c panel lá»›n cÃ³ thá»ƒ vÃ´ tÃ¬nh cháº·n tap object.

**Cáº§n bá»• sung:**

- UI chá»‰ báº­t raycast cho button/panel cáº§n thiáº¿t.
- VÃ¹ng trong suá»‘t khÃ´ng cháº·n AR raycast.
- Test riÃªng: tap object á»Ÿ vÃ¹ng gáº§n UI bottom/top.

#### INT-05. Highlight hiá»‡n táº¡i Ä‘á»•i mÃ u material vÃ  scale trá»±c tiáº¿p, dá»… phÃ¡ asset

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `ARInteractionService.cs`, `ActivityPrefabSetup.cs`

**Váº¥n Ä‘á»:**
Highlight Ä‘ang set `renderer.material.color = highlightColor` vÃ  reset vá» `Color.white`. Vá»›i prefab cÃ³ texture/material riÃªng, reset vá» tráº¯ng cÃ³ thá»ƒ lÃ m máº¥t mÃ u gá»‘c.

**Cáº§n bá»• sung:**

- LÆ°u original material color/property block.
- DÃ¹ng outline/halo child object thay vÃ¬ sá»­a material tháº­t.
- TÃ¡ch hover/select/count state.

#### INT-06. Drag/reposition chÆ°a thÃ nh gameplay cÃ³ chá»§ Ä‘Ã­ch

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `ARInteractionService.cs`, `NumberLineJumpPresenter.cs`

**Váº¥n Ä‘á»:**
Interaction service cÃ³ drag events nhÆ°ng activity chÆ°a khai thÃ¡c thÃ nh gameplay á»•n Ä‘á»‹nh. Náº¿u object bá»‹ kÃ©o nháº§m, layout toÃ¡n cÃ³ thá»ƒ há»ng.

**Cáº§n bá»• sung:**

- KhÃ³a drag máº·c Ä‘á»‹nh cho learning object.
- Chá»‰ cho drag trong mode chá»‰nh vÃ¹ng há»c hoáº·c activity cáº§n kÃ©o-tháº£.
- CÃ³ snap-back/snap-to-slot rÃµ rÃ ng.

### C. Gameplay Flow VÃ  Logic Hoáº¡t Äá»™ng

#### GP-01. Activity routing cÃ²n phÃ¢n tÃ¡n giá»¯a `ActivityLoader` vÃ  `GameplayActivityRouter`

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `ActivityLoader.cs`, `GameplayActivityRouter.cs`, `SC_ARGameplay.unity`

**Váº¥n Ä‘á»:**
CÃ³ hai cÆ¡ cháº¿ load activity. `ActivityLoader` cÃ³ field cho cáº£ 3 activity nhÆ°ng khÃ´ng tháº¥y Ä‘Æ°á»£c gáº¯n trong `SC_ARGameplay`. `GameplayActivityRouter` láº¡i táº¡o NumberLine/Compare runtime báº±ng reflection/config fallback. Äiá»u nÃ y lÃ m flow khÃ³ kiá»ƒm soÃ¡t.

**Cáº§n bá»• sung:**

- Chá»n má»™t dispatcher chÃ­nh cho `SC_ARGameplay`.
- Náº¿u dÃ¹ng runtime creation, bá» hoáº·c chuyá»ƒn `ActivityLoader` thÃ nh deprecated.
- Náº¿u dÃ¹ng serialized loader, bake Ä‘á»§ root/config/view cho 3 activity trong scene.
- CÃ³ log rÃµ activity nÃ o Ä‘Æ°á»£c load vÃ  vÃ¬ sao.

#### GP-02. `SC_ARGameplay` váº«n Æ°u tiÃªn Quantity Match máº·c Ä‘á»‹nh

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `SC_ARGameplay.unity`, `QuantityMatchActivityBootstrap.cs`, `GameplayActivityRouter.cs`

**Váº¥n Ä‘á»:**
Scene cÃ³ `QuantityMatchActivity` sáºµn. Router táº¯t Quantity Match khi chá»n activity khÃ¡c. ÄÃ¢y lÃ  workaround cháº¥p nháº­n Ä‘Æ°á»£c khi test, nhÆ°ng chÆ°a pháº£i kiáº¿n trÃºc gameplay sáº¡ch.

**Cáº§n bá»• sung:**

- `SC_ARGameplay` chá»‰ cÃ³ `ActivityHost`.
- Activity Ä‘Æ°á»£c instantiate tá»« registry/prefab theo `activityId`.
- KhÃ´ng cÃ³ activity máº·c Ä‘á»‹nh tá»± start náº¿u Ä‘ang Ä‘i tá»« Activity Select, trá»« mode debug.

#### GP-03. Cold-start flow chÆ°a thá»‘ng nháº¥t

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `SC_Boot.unity`, `EditorBuildSettings.asset`, `BootLoader.cs`

**Váº¥n Ä‘á»:**
`SC_Boot` tá»“n táº¡i vÃ  cÃ³ `BootLoader`, nhÆ°ng Build Settings hiá»‡n báº¯t Ä‘áº§u tá»« `SC_MainMenu`. Äiá»u nÃ y lÃ m flow bÃ¡o cÃ¡o vÃ  flow build device khÃ´ng rÃµ rÃ ng.

**Cáº§n bá»• sung:**

- Quyáº¿t Ä‘á»‹nh entry chÃ­nh: `SC_Boot` hoáº·c `SC_MainMenu`.
- Náº¿u dÃ¹ng `SC_Boot`, Ä‘Æ°a vÃ o Build Settings index 0.
- Náº¿u bá» Boot, chuyá»ƒn init service cáº§n thiáº¿t sang MainMenu/Gameplay vÃ  xÃ³a scene Boot khá»i tÃ i liá»‡u.

#### GP-04. Auto-advance round cÃ³ thá»ƒ quÃ¡ nhanh vá»›i tráº» khÃ³ há»c toÃ¡n

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `QuantityMatchView.cs`, `CompareQuantityView.cs`, `NumberLineJumpView.cs`

**Váº¥n Ä‘á»:**
Má»™t sá»‘ view tá»± `Invoke` chuyá»ƒn round sau vÃ i giÃ¢y. Vá»›i tráº» khÃ³ há»c toÃ¡n, tráº» cáº§n thá»i gian xem láº¡i vÃ¬ sao Ä‘Ãºng, Ä‘áº¿m láº¡i hoáº·c nháº­n lá»i khen.

**Cáº§n bá»• sung:**

- Sau má»—i cÃ¢u Ä‘Ãºng: hiá»‡n "Tiáº¿p tá»¥c" lá»›n, khÃ´ng tá»± chuyá»ƒn quÃ¡ nhanh.
- TÃ¹y chá»n auto-advance chá»‰ dÃ¹ng trong debug hoáº·c cÃ i Ä‘áº·t.
- Vá»›i cÃ¢u sai: giá»¯ láº¡i váº­t thá»ƒ vÃ  hÆ°á»›ng dáº«n Ä‘áº¿m láº¡i thay vÃ¬ chá»‰ hiá»‡n text rá»“i biáº¿n máº¥t.

#### GP-05. Failure flow chÆ°a Ä‘á»§ sÆ° pháº¡m

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ActivityPresenter.cs`, activity presenters/views

**Váº¥n Ä‘á»:**
Khi sai nhiá»u láº§n, há»‡ thá»‘ng cÃ³ thá»ƒ fail round/activity, nhÆ°ng chÆ°a cÃ³ remediation flow. Tráº» khÃ³ há»c toÃ¡n cáº§n Ä‘Æ°á»£c dáº«n láº¡i tá»«ng bÆ°á»›c, khÃ´ng chá»‰ bÃ¡o sai.

**Cáº§n bá»• sung:**

- Sau `MaxAttemptsPerQuestion`, chuyá»ƒn sang guided mode.
- Cho tráº» lÃ m láº¡i cÃ¹ng cÃ¢u vá»›i nhiá»u scaffold hÆ¡n.
- LÆ°u lá»—i nhÆ°ng khÃ´ng biáº¿n thÃ nh tráº£i nghiá»‡m tháº¥t báº¡i náº·ng ná».

#### GP-06. Logic save result Ä‘ang lÆ°u theo round nhÆ°ng dashboard Ä‘áº·t tÃªn "activity completed"

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ActivityPresenter.cs`, `LocalProgressStorage.cs`, `ProgressDashboardView.cs`

**Váº¥n Ä‘á»:**
`ActivityResult` Ä‘Æ°á»£c save khi tá»«ng round correct/fail. Dashboard cÃ³ thá»ƒ hiá»ƒu `TotalActivitiesCompleted` theo sá»‘ activity cÃ³ data hoáº·c sá»‘ result, khÃ´ng pháº£i sá»‘ buá»•i/bÃ i hoÃ n thÃ nh tháº­t.

**Cáº§n bá»• sung:**

- TÃ¡ch `RoundResult`, `ActivitySessionResult`, `LessonResult`.
- Dashboard hiá»ƒn thá»‹ rÃµ: sá»‘ cÃ¢u Ä‘Ã£ lÃ m, sá»‘ bÃ i hoÃ n thÃ nh, sá»‘ buá»•i há»c.
- KhÃ´ng dÃ¹ng sá»‘ round lÃ m sá»‘ "activity completed".

#### GP-07. Number Line Jump chÆ°a hoÃ n thiá»‡n cáº£m giÃ¡c game

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `NumberLineJumpPresenter.cs`, `NumberLineJumpView.cs`

**Váº¥n Ä‘á»:**
Tile prefab cÃ²n fallback/TODO, boundary bump cÃ²n TODO, nhÃ¢n váº­t nháº£y chÆ°a cÃ³ cáº£m giÃ¡c váº­t lÃ½/animation Ä‘á»§ tá»‘t.

**Cáº§n bá»• sung:**

- Prefab tile cÃ³ sá»‘ to, camera-facing.
- Jump arc animation, sound step, landing effect.
- Preview hÆ°á»›ng vÃ  sá»‘ bÆ°á»›c.
- Boundary feedback báº±ng bump animation, khÃ´ng chá»‰ text.

#### GP-08. Compare Quantity thiáº¿u gameplay ghÃ©p cáº·p trá»±c quan

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `CompareQuantityPresenter.cs`, `CompareQuantityView.cs`

**Váº¥n Ä‘á»:**
So sÃ¡nh nhiá»u/Ã­t/báº±ng nhau hiá»‡n chá»§ yáº¿u lÃ  nhÃ¬n hai group vÃ  báº¥m More/Fewer/Equal. Tráº» khÃ³ há»c toÃ¡n thÆ°á»ng cáº§n ghÃ©p cáº·p má»™t-má»™t Ä‘á»ƒ hiá»ƒu thá»«a/thiáº¿u.

**Cáº§n bá»• sung:**

- Mode "ghÃ©p báº¡n": kÃ©o/auto ná»‘i object trÃ¡i-pháº£i thÃ nh cáº·p.
- Sau khi ghÃ©p, highlight pháº§n thá»«a.
- BÃ i equal nÃªn cho tháº¥y tá»«ng cáº·p khá»›p nhau.

#### GP-09. ChÆ°a cÃ³ lesson map, unlock vÃ  mastery

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** activity configs, `ActivitySelectController.cs`, progress storage

**Váº¥n Ä‘á»:**
Hiá»‡n cÃ³ `SO_*_Easy.asset` 10 round, nhÆ°ng chÆ°a cÃ³ lá»™ trÃ¬nh bÃ i há»c theo ká»¹ nÄƒng. Activity Select Ä‘ang chá»n activity, khÃ´ng chá»n lesson/level cÃ³ má»¥c tiÃªu rÃµ.

**Cáº§n bá»• sung lesson map:**

```text
1. Nháº­n biáº¿t 1-3
2. Äáº¿m 1-5
3. GhÃ©p sá»‘ 1-5
4. GhÃ©p sá»‘ 6-10 báº±ng ten-frame
5. So sÃ¡nh nhiá»u hÆ¡n/Ã­t hÆ¡n
6. Báº±ng nhau
7. Dáº¥u > < =
8. Trá»¥c sá»‘ 0-5
9. Trá»¥c sá»‘ 0-10
10. Cá»™ng/trá»« báº±ng bÆ°á»›c nháº£y
```

#### GP-10. ChÆ°a cÃ³ adaptive difficulty

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** configs, progress storage, activity select

**Váº¥n Ä‘á»:**
Round Ä‘Æ°á»£c cáº¥u hÃ¬nh cá»‘ Ä‘á»‹nh. Há»‡ thá»‘ng chÆ°a tÄƒng/giáº£m Ä‘á»™ khÃ³ dá»±a trÃªn sá»‘ láº§n sai, thá»i gian, sá»‘ hint, loáº¡i lá»—i.

**Cáº§n bá»• sung:**

- Náº¿u tráº» sai nhiá»u: giáº£m sá»‘ lÆ°á»£ng, giáº£m choices, báº­t guided counting.
- Náº¿u tráº» Ä‘Ãºng nhanh: tÄƒng sá»‘ lÆ°á»£ng, bá» bá»›t scaffold, trá»™n cÃ¢u há»i.
- LÆ°u mastery theo skill, khÃ´ng chá»‰ theo activity.

### D. Ná»™i Dung, Config VÃ  Asset Pipeline

#### CT-01. Ná»™i dung cÃ²n tiáº¿ng Anh vÃ  chÆ°a cÃ³ localization

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `SO_*_Easy.asset`, `*Config.cs`, `*View.cs`, `GameplayActivityRouter.cs`

**Váº¥n Ä‘á»:**
Nhiá»u string váº«n lÃ  tiáº¿ng Anh: `Great job`, `More`, `Fewer`, `Equal`, `Confirm`, `Reset`, `Hint`, `Left Group`, `Right Group`.

**Cáº§n bá»• sung:**

- Localization table tiáº¿ng Viá»‡t/tiáº¿ng Anh.
- Text trong config khÃ´ng hardcode trong code.
- Audio key Ä‘i kÃ¨m má»—i instruction/feedback.

#### CT-02. Config runtime dÃ¹ng `AssetDatabase` fallback, cÃ³ rá»§i ro khi build device

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `GameplayActivityRouter.cs`

**Váº¥n Ä‘á»:**
Router load NumberLine/Compare tá»« `Resources`, náº¿u khÃ´ng cÃ³ thÃ¬ trong Editor dÃ¹ng `AssetDatabase`, cÃ²n build thÃ¬ chá»‰ warning/fallback runtime config. Config asset hiá»‡n náº±m trong `Features/.../ScriptableObjects`, khÃ´ng pháº£i `Resources/ActivityConfigs`.

**Cáº§n bá»• sung:**

- DÃ¹ng registry ScriptableObject Ä‘Æ°á»£c reference trong scene/build.
- Hoáº·c chuyá»ƒn config cáº§n runtime load vÃ o `Resources/ActivityConfigs`.
- KhÃ´ng phá»¥ thuá»™c `AssetDatabase` trong gameplay code.

#### CT-03. Prefab/UI asset chuáº©n cÃ²n thiáº¿u

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ActivityPrefabSetup.cs`, feature `Prefabs/`, feature `UI/`

**Váº¥n Ä‘á»:**
Feature folders cÃ³ `Prefabs/` vÃ  `UI/` nhÆ°ng chÆ°a tháº¥y prefab lesson/UI riÃªng. Há»‡ thá»‘ng Ä‘ang dá»±a vÃ o runtime UI vÃ  placeholder/animal prefabs.

**Cáº§n bá»• sung:**

- `PFB_QuantityMatchPanel`, `PFB_CompareQuantityPanel`, `PFB_NumberLineJumpPanel`.
- `PFB_NumberTile`, `PFB_JumpCharacter`, group label prefab.
- Prefab object há»c táº­p cÃ³ collider/hitbox/animation/scale chuáº©n.

#### CT-04. Animal prefab pipeline chÆ°a Ä‘áº£m báº£o tÃ­nh nháº¥t quÃ¡n há»c táº­p

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `ActivityPrefabSetup.cs`, `Assets/Resources/ARAnimals`

**Váº¥n Ä‘á»:**
Random animal prefab giÃºp sinh Ä‘á»™ng, nhÆ°ng cÃ³ thá»ƒ lÃ m group khÃ³ Ä‘áº¿m vÃ¬ kÃ­ch thÆ°á»›c/silhouette khÃ¡c nhau. Má»™t sá»‘ animal cÃ³ animation/material phá»©c táº¡p.

**Cáº§n bá»• sung:**

- Theo tá»«ng round chá»‰ dÃ¹ng cÃ¹ng má»™t loáº¡i object.
- Object pháº£i cÃ³ kÃ­ch thÆ°á»›c/Ä‘á»™ tÆ°Æ¡ng pháº£n á»•n Ä‘á»‹nh.
- Cho phÃ©p chá»n theme nhÆ°ng váº«n giá»¯ layout dá»… Ä‘áº¿m.

#### CT-05. Number line tile chÆ°a cÃ³ asset vÃ  label chuáº©n

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `NumberLineJumpPresenter.cs`, `ActivityPrefabSetup.cs`

**Váº¥n Ä‘á»:**
`GetTilePrefab()` cÃ²n not implemented; fallback cube/text cÃ³ thá»ƒ Ä‘á»§ test logic nhÆ°ng chÆ°a Ä‘á»§ demo/premium UX.

**Cáº§n bá»• sung:**

- Tile prefab vá»›i TextMeshPro 3D.
- Billboard label hoáº·c world-space canvas.
- ÄÃ¡nh dáº¥u start/current/target báº±ng mÃ u riÃªng.

#### CT-06. ChÆ°a cÃ³ audio/VFX asset tháº­t

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `FeedbackServiceProxy.cs`, `FeedbackSystem.cs`, `FeedbackData.cs`

**Váº¥n Ä‘á»:**
Feedback hook Ä‘Ã£ cÃ³ nhÆ°ng playback tháº­t váº«n TODO/log. Vá»›i tráº» khÃ³ há»c toÃ¡n, audio vÃ  visual reinforcement lÃ  pháº§n quan trá»ng, khÃ´ng pháº£i polish phá»¥.

**Cáº§n bá»• sung:**

- Audio manager map key -> clip.
- Voice instruction tiáº¿ng Viá»‡t cho sá»‘ 1-10, More/Fewer/Equal, trÃ¡i/pháº£i.
- VFX nháº¹: correct sparkle, count pulse, wrong gentle shake, success celebration.

### E. UI/UX VÃ  Accessibility

#### UI-01. Runtime UI chá»‰ phÃ¹ há»£p test, chÆ°a pháº£i UI sáº£n pháº©m

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `QuantityMatchRuntimeUI.cs`, `CompareQuantityRuntimeUI.cs`, `NumberLineJumpRuntimeUI.cs`, cÃ¡c `*View.cs`

**Váº¥n Ä‘á»:**
Runtime UI Ä‘áº£m báº£o cÃ³ mÃ n Ä‘á»ƒ test, nhÆ°ng chÆ°a tá»‘i Æ°u bá»‘ cá»¥c, motion, icon, mÃ u, khoáº£ng tráº¯ng, vÃ¹ng cháº¡m cho tráº» nhá».

**Cáº§n bá»• sung:**

- UI prefab Ä‘Æ°á»£c thiáº¿t káº¿ riÃªng cho mobile portrait/landscape.
- Button lá»›n, Ã­t chá»¯, icon rÃµ.
- Text khÃ´ng trÃ n á»Ÿ nhiá»u tá»‰ lá»‡ mÃ n hÃ¬nh.
- KhÃ´ng dÃ¹ng quÃ¡ nhiá»u panel chá»“ng lÃªn camera AR.

#### UI-02. Label 3D chÆ°a Ä‘á»“ng nháº¥t billboard/camera-facing

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `QuantityMatchPresenter.cs`, `CompareQuantityPresenter.cs`, `NumberLineJumpPresenter.cs`

**Váº¥n Ä‘á»:**
Quantity Match Ä‘Ã£ cÃ³ `BillboardBehavior` cho group label, nhÆ°ng Compare/NumberLine váº«n cÃ³ cÃ¡c label Ä‘Æ°á»£c xoay má»™t láº§n táº¡i thá»i Ä‘iá»ƒm táº¡o. Khi camera di chuyá»ƒn, label cÃ³ thá»ƒ khÃ³ Ä‘á»c.

**Cáº§n bá»• sung:**

- DÃ¹ng chung `BillboardBehavior` hoáº·c world-space label component cho má»i label 3D.
- KhÃ´ng Ä‘á»ƒ text bá»‹ ngÆ°á»£c/lá»‡ch khi tráº» Ä‘i quanh bÃ n.

#### UI-03. UI chÆ°a cÃ³ cháº¿ Ä‘á»™ accessibility

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** app settings, UI views

**Váº¥n Ä‘á»:**
ChÆ°a cÃ³ tÃ¹y chá»n:

- TÄƒng cá»¡ chá»¯.
- Giáº£m animation.
- Giáº£m Ã¢m hoáº·c táº¯t Ã¢m.
- High contrast mode.
- Táº¯t hiá»‡u á»©ng gÃ¢y máº¥t táº­p trung.
- Cháº¿ Ä‘á»™ cÃ³ phá»¥ huynh há»— trá»£.

#### UI-04. Feedback text/hint quÃ¡ phá»¥ thuá»™c vÃ o chá»¯

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `*View.cs`, `*Config.cs`

**Váº¥n Ä‘á»:**
Tráº» 4-6 tuá»•i hoáº·c tráº» khÃ³ há»c cÃ³ thá»ƒ chÆ°a Ä‘á»c tá»‘t. Feedback/hint báº±ng text tiáº¿ng Anh khÃ´ng Ä‘á»§.

**Cáº§n bá»• sung:**

- Voice-over cho instruction/hint.
- Icon hoáº·c animation minh há»a hint.
- Button "nghe láº¡i".
- Hint visual trá»±c tiáº¿p trÃªn object thay vÃ¬ chá»‰ panel chá»¯.

#### UI-05. ChÆ°a cÃ³ UX cho lá»—i ká»¹ thuáº­t AR

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** AR services, gameplay overlay

**Váº¥n Ä‘á»:**
Náº¿u thiáº¿u camera permission, device khÃ´ng há»— trá»£ AR, máº¥t tracking, chÆ°a cÃ³ plane, há»‡ thá»‘ng hiá»‡n chá»§ yáº¿u log hoáº·c warning. Tráº»/phá»¥ huynh cáº§n thÃ´ng Ä‘iá»‡p rÃµ.

**Cáº§n bá»• sung:**

- Overlay lá»—i thÃ¢n thiá»‡n: "MÃ¡y chÆ°a báº­t camera", "KhÃ´ng tÃ¬m tháº¥y máº·t pháº³ng".
- NÃºt thá»­ láº¡i, má»Ÿ hÆ°á»›ng dáº«n, quay láº¡i menu.
- Fallback non-AR mode náº¿u thiáº¿t bá»‹ khÃ´ng há»— trá»£ AR.

### F. Data, Progress VÃ  Pedagogical Analytics

#### DATA-01. `ActivityResult.ErrorType` dÃ¹ng nullable enum

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ActivityResult.cs`, `LocalProgressStorage.cs`

**Váº¥n Ä‘á»:**
Unity `JsonUtility` khÃ´ng Ä‘Ã¡ng tin vá»›i nullable enum. `ErrorType?` cÃ³ thá»ƒ khÃ´ng serialize/deserialize nhÆ° mong muá»‘n.

**Cáº§n bá»• sung:**

- DÃ¹ng `bool HasErrorType` + `ErrorType ErrorType`.
- Hoáº·c dÃ¹ng string `ErrorTypeCode`.
- Viáº¿t test serialization.

#### DATA-02. ChÆ°a tÃ¡ch dá»¯ liá»‡u ká»¹ thuáº­t vÃ  dá»¯ liá»‡u há»c táº­p

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ActivityResult.cs`, `LocalProgressStorage.cs`

**Váº¥n Ä‘á»:**
Náº¿u object khÃ´ng spawn, tracking máº¥t, tap lá»—i, káº¿t quáº£ cÃ³ thá»ƒ bá»‹ tÃ­nh nhÆ° tráº» sai. Äiá»u nÃ y lÃ m sai dashboard vÃ  can thiá»‡p sÆ° pháº¡m.

**Cáº§n bá»• sung:**

- `TechnicalIssueType`: TrackingLost, SpawnFailed, InteractionFailed, TimeoutDueToAR.
- KhÃ´ng tÃ­nh technical failure vÃ o accuracy há»c táº­p.

#### DATA-03. Dashboard chÆ°a Ä‘Æ°a ra nháº­n Ä‘á»‹nh há»c táº­p

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** `ProgressDashboardView.cs`

**Váº¥n Ä‘á»:**
Dashboard hiá»‡n Ä‘á»c thá»‘ng kÃª cÆ¡ báº£n. Phá»¥ huynh/giÃ¡o viÃªn cáº§n biáº¿t tráº» yáº¿u á»Ÿ ká»¹ nÄƒng nÃ o vÃ  nÃªn luyá»‡n bÃ i gÃ¬ tiáº¿p.

**Cáº§n bá»• sung:**

- Skill tags: counting, comparison, equality, number-line, addition, subtraction.
- Mastery score theo skill.
- Gá»£i Ã½ bÃ i tiáº¿p theo.
- Lá»‹ch sá»­ lá»—i thÆ°á»ng gáº·p.

#### DATA-04. ChÆ°a cÃ³ há»“ sÆ¡ tráº» vÃ  nhiá»u learner

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** storage, main menu/parent mode

**Váº¥n Ä‘á»:**
Local progress hiá»‡n khÃ´ng phÃ¢n biá»‡t nhiá»u tráº». Vá»›i app giÃ¡o dá»¥c, phá»¥ huynh/giÃ¡o viÃªn cÃ³ thá»ƒ cáº§n nhiá»u profile.

**Cáº§n bá»• sung:**

- `LearnerProfile`.
- Káº¿t quáº£ gáº¯n vá»›i learner id.
- Parent mode quáº£n lÃ½ profile.

### G. Kiáº¿n TrÃºc VÃ  Maintainability

#### ARCH-01. Namespace khÃ´ng Ä‘á»“ng nháº¥t

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `Core.AR.*`, `ARSpecialEducation.Core.AR`

**Váº¥n Ä‘á»:**
Má»™t sá»‘ AR class dÃ¹ng namespace `Core.AR.*`, má»™t sá»‘ dÃ¹ng `ARSpecialEducation.Core.AR`. Sá»± pha trá»™n nÃ y lÃ m code khÃ³ tÃ¬m vÃ  dá»… lá»—i khi refactor.

**Cáº§n bá»• sung:**

- Thá»‘ng nháº¥t namespace AR.
- Náº¿u giá»¯ namespace cÅ©, táº¡o migration rÃµ.

#### ARCH-02. Interface comment váº«n ghi "TODO implement" dÃ¹ Ä‘Ã£ cÃ³ implementation

**Má»©c Ä‘á»™:** P3
**File liÃªn quan:** `IARPlacementService.cs`, `IARInteractionService.cs`, `IARSessionService.cs`

**Váº¥n Ä‘á»:**
Comment cÅ© nÃ³i AR team cáº§n implement interface, trong khi implementation Ä‘Ã£ cÃ³. TÃ i liá»‡u inline bá»‹ drift.

**Cáº§n bá»• sung:**

- Cáº­p nháº­t comment theo tráº¡ng thÃ¡i má»›i.
- Chá»‰ ghi TODO cho pháº§n cÃ²n thiáº¿u tháº­t.

#### ARCH-03. ChÆ°a cÃ³ assembly definition Ä‘á»ƒ giá»¯ boundary

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** toÃ n bá»™ `Assets/Core`, `Assets/Features`, `_Project`

**Váº¥n Ä‘á»:**
KhÃ´ng cÃ³ `.asmdef` nÃªn ranh giá»›i Core/Features/_Project khÃ´ng Ä‘Æ°á»£c compiler enforce.

**Cáº§n bá»• sung:**

- `Core.AR`, `Core.Learning`, `Core.Data`, `Core.Support`.
- `Features.Activities`.
- Tests asmdef riÃªng.

#### ARCH-04. Runtime reflection trong `GameplayActivityRouter` lÃ m flow khÃ³ kiá»ƒm soÃ¡t

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `GameplayActivityRouter.cs`

**Váº¥n Ä‘á»:**
Router dÃ¹ng reflection Ä‘á»ƒ set private fields trÃªn bootstrap/config runtime. CÃ¡ch nÃ y nhanh cho prototype nhÆ°ng dá»… vá»¡ khi Ä‘á»•i tÃªn field.

**Cáº§n bá»• sung:**

- Public `Configure(...)` method cho bootstrap/activity host.
- Activity registry chá»©a prefab/config reference.
- KhÃ´ng set private fields báº±ng reflection trong production flow.

### H. Performance VÃ  TÃ i NguyÃªn

#### PERF-01. Spawn/destroy nhiá»u object má»—i round cÃ³ thá»ƒ gÃ¢y giáº­t

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `ARPlacementService.cs`, activity presenters

**Váº¥n Ä‘á»:**
CÃ¡c round spawn nhiá»u object rá»“i clear/destroy. TrÃªn device yáº¿u, Ä‘iá»u nÃ y cÃ³ thá»ƒ gÃ¢y spike.

**Cáº§n bá»• sung:**

- Object pooling cho animal/tile/label.
- Preload prefab/audio.
- Giá»›i háº¡n object count theo device tier.

#### PERF-02. Runtime material repair cÃ³ thá»ƒ tá»‘n chi phÃ­

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `ActivityPrefabSetup.cs`

**Váº¥n Ä‘á»:**
Repair material runtime cho animal prefab tiá»‡n cho prototype nhÆ°ng cÃ³ thá»ƒ gÃ¢y overhead vÃ  táº¡o material instances.

**Cáº§n bá»• sung:**

- Chuáº©n hÃ³a material á»Ÿ import/editor time.
- Cache/material variants rÃµ.
- KhÃ´ng scan toÃ n bá»™ asset trong runtime production.

#### PERF-03. `Resources.LoadAll` animal prefab thiáº¿u kiá»ƒm soÃ¡t bá»™ nhá»›

**Má»©c Ä‘á»™:** P2
**File liÃªn quan:** `ActivityPrefabSetup.cs`

**Váº¥n Ä‘á»:**
Load all animal prefabs cÃ³ thá»ƒ lÃ m tÄƒng memory footprint, nháº¥t lÃ  model nhiá»u LOD/animation.

**Cáº§n bá»• sung:**

- Registry danh sÃ¡ch prefab Ä‘Æ°á»£c phÃ©p dÃ¹ng cho lesson.
- Lazy load theo theme.
- Addressables hoáº·c serialized references náº¿u project lá»›n.

### I. Testing, QA VÃ  Device Readiness

#### TEST-01. ChÆ°a cÃ³ báº±ng chá»©ng pass device AR

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `.agent/LOCAL_UNITY_FULL_TEST_GUIDE.md`, scene AR

**Váº¥n Ä‘á»:**
CÃ³ hÆ°á»›ng dáº«n test, nhÆ°ng chÆ°a cÃ³ log/bÃ¡o cÃ¡o device ARCore/ARKit pass.

**Cáº§n bá»• sung:**

- Test report: device, OS, Unity version, scene, káº¿t quáº£.
- Screenshot/video gameplay.
- Checklist spawn, tap, tracking lost/recover, save result.

#### TEST-02. `unity_compile.log` khÃ´ng chá»©ng minh compile pass

**Má»©c Ä‘á»™:** P0
**File liÃªn quan:** `unity_compile.log`

**Váº¥n Ä‘á»:**
Log hiá»‡n bÃ¡o Unity project Ä‘ang má»Ÿ bá»Ÿi instance khÃ¡c. KhÃ´ng thá»ƒ xem Ä‘Ã¢y lÃ  compile/test pass.

**Cáº§n bá»• sung:**

- Cháº¡y Unity batchmode khi project khÃ´ng má»Ÿ.
- LÆ°u compile log sáº¡ch.
- Náº¿u khÃ´ng thá»ƒ batch, ghi rÃµ Ä‘Ã£ test thá»§ cÃ´ng trong Editor.

#### TEST-03. ChÆ°a cÃ³ EditMode/PlayMode tests

**Má»©c Ä‘á»™:** P1
**File liÃªn quan:** feature `Tests/`, Core `Tests/`

**Váº¥n Ä‘á»:**
KhÃ´ng tháº¥y test C# thá»±c táº¿ cho answer checking, hint, progress serialization, activity flow.

**Cáº§n bá»• sung test tá»‘i thiá»ƒu:**

- Quantity Match: correct group, wrong quantity, number input.
- Compare Quantity: more/fewer/equal.
- Number Line Jump: target reached, wrong direction, wrong jump count.
- LocalProgressStorage serialization.
- Mock AR gameplay flow.

#### TEST-04. ChÆ°a cÃ³ AR edge-case QA

**Má»©c Ä‘á»™:** P1
**Cáº§n test thÃªm:**

- Máº·t pháº³ng nhá»/khÃ´ng Ä‘á»§ diá»‡n tÃ­ch.
- Tracking lost giá»¯a round.
- Camera permission denied.
- Device khÃ´ng há»— trá»£ AR.
- Táº¯t app giá»¯a session rá»“i má»Ÿ láº¡i.
- Orientation change.
- Tap UI vÃ  tap AR sÃ¡t nhau.
- Object bá»‹ che bá»Ÿi UI overlay.

## 4. Backlog Æ¯u TiÃªn Äá»ƒ HoÃ n Thiá»‡n Logic VÃ  Gameplay

### P0 - Báº¯t buá»™c trÆ°á»›c khi bÃ¡o cÃ¡o AR MVP

| ID | Viá»‡c cáº§n lÃ m | Káº¿t quáº£ mong muá»‘n |
|---|---|---|
| AR-01/02 | Táº¡o calibration flow vÃ  learning-area anchor chÃ­nh thá»©c | Activity chá»‰ start sau khi cÃ³ vÃ¹ng há»c há»£p lá»‡ |
| GP-01/02 | Thá»‘ng nháº¥t activity host/router | Chá»n activity nÃ o cháº¡y Ä‘Ãºng activity Ä‘Ã³ |
| GP-03 | Chá»‘t entry scene vÃ  Build Settings | Cold-start khÃ´ng mÆ¡ há»“ |
| INT-01/02 | TÃ¡ch tap-to-count vÃ  tap-to-submit | Tráº» cÃ³ thá»ƒ cháº¡m object Ä‘á»ƒ Ä‘áº¿m mÃ  khÃ´ng ná»™p nháº§m |
| CT-01 | Viá»‡t hÃ³a UI/config cÆ¡ báº£n | Tráº» Viá»‡t Nam hiá»ƒu Ä‘Æ°á»£c yÃªu cáº§u |
| CT-02 | Sá»­a runtime config loading cho build | Build device khÃ´ng phá»¥ thuá»™c `AssetDatabase` |
| UI-05 | Overlay lá»—i AR thÃ¢n thiá»‡n | KhÃ´ng chá»‰ log khi AR lá»—i |
| TEST-01/02 | Compile/device smoke test | CÃ³ báº±ng chá»©ng cháº¡y tháº­t |

### P1 - Cáº§n cho tráº£i nghiá»‡m há»c táº­p tá»‘t

| ID | Viá»‡c cáº§n lÃ m | Káº¿t quáº£ mong muá»‘n |
|---|---|---|
| GP-04/05 | Chá»‰nh round flow vÃ  failure remediation | Sai thÃ¬ Ä‘Æ°á»£c dáº«n láº¡i, khÃ´ng chá»‰ bÃ¡o sai |
| GP-07 | HoÃ n thiá»‡n Number Line Jump gameplay | Nháº£y rÃµ, tile rÃµ, boundary feedback |
| GP-08 | ThÃªm ghÃ©p cáº·p cho Compare Quantity | Tráº» hiá»ƒu so sÃ¡nh trá»±c quan |
| GP-09 | Lesson map vÃ  mastery | App cÃ³ lá»™ trÃ¬nh há»c |
| CT-03/05 | Prefab/UI chuáº©n cho activity | KhÃ´ng phá»¥ thuá»™c placeholder |
| CT-06 | Audio/VFX tháº­t | Feedback Ä‘a giÃ¡c quan |
| UI-01/03/04 | UI tráº» nhá» vÃ  accessibility | Dá»… dÃ¹ng, Ã­t chá»¯, cÃ³ audio |
| DATA-01/02/03 | Progress/analytics chuáº©n | Dashboard cÃ³ Ã½ nghÄ©a sÆ° pháº¡m |
| TEST-03/04 | Test tá»± Ä‘á»™ng vÃ  edge cases | Ãt regression |

### P2 - NÃªn lÃ m Ä‘á»ƒ sáº£n pháº©m bá»n hÆ¡n

| ID | Viá»‡c cáº§n lÃ m | Káº¿t quáº£ mong muá»‘n |
|---|---|---|
| GP-10 | Adaptive difficulty | CÃ¡ nhÃ¢n hÃ³a theo nÄƒng lá»±c tráº» |
| DATA-04 | Learner profiles | Há»— trá»£ nhiá»u tráº» |
| ARCH-01/03/04 | Clean architecture | Dá»… báº£o trÃ¬, Ã­t lá»—i refactor |
| PERF-01/02/03 | Pooling/preload/resource control | Cháº¡y mÆ°á»£t hÆ¡n trÃªn mobile |
| AR-07 | Safety/distance/session-break | Tá»‘t hÆ¡n cho tráº» nhá» |

## 5. TiÃªu Chuáº©n Nghiá»‡m Thu Há»‡ Thá»‘ng AR HoÃ n Thiá»‡n

Má»™t há»‡ thá»‘ng AR/gameplay Ä‘á»§ tá»‘t Ä‘á»ƒ bÃ¡o cÃ¡o hoáº·c demo sáº£n pháº©m nÃªn Ä‘áº¡t:

1. **Cold start rÃµ rÃ ng:** má»Ÿ app tá»« scene chÃ­nh, vÃ o menu, chá»n bÃ i, vÃ o AR gameplay khÃ´ng lá»—i.
2. **Calibration cÃ³ hÆ°á»›ng dáº«n:** tráº»/phá»¥ huynh biáº¿t cáº§n quÃ©t máº·t pháº³ng vÃ  Ä‘áº·t vÃ¹ng há»c á»Ÿ Ä‘Ã¢u.
3. **Object á»•n Ä‘á»‹nh:** content náº±m trong vÃ¹ng há»c, khÃ´ng trÃ´i/lá»‡ch rÃµ khi di chuyá»ƒn camera bÃ¬nh thÆ°á»ng.
4. **TÆ°Æ¡ng tÃ¡c khÃ´ng gÃ¢y ná»™p nháº§m:** cháº¡m object Ä‘á»ƒ Ä‘áº¿m khÃ¡c vá»›i chá»n/ná»™p Ä‘Ã¡p Ã¡n.
5. **Gameplay cÃ³ scaffold:** tráº» sai Ä‘Æ°á»£c hÆ°á»›ng dáº«n cá»¥ thá»ƒ theo lá»—i, khÃ´ng chá»‰ hiá»‡n text chung.
6. **UI tiáº¿ng Viá»‡t, Ã­t chá»¯:** cÃ³ audio hoáº·c icon há»— trá»£, nÃºt lá»›n, khÃ´ng che object quan trá»ng.
7. **3 activity cháº¡y Ä‘Ãºng:** Quantity Match á»•n nháº¥t; Number Line Jump vÃ  Compare Quantity Ã­t nháº¥t cháº¡y Ä‘Æ°á»£c má»™t lesson cÃ³ save result.
8. **Progress cÃ³ Ã½ nghÄ©a:** dashboard phÃ¢n biá»‡t cÃ¢u Ä‘Ãºng/sai, sá»‘ hint, loáº¡i lá»—i, ká»¹ nÄƒng cáº§n luyá»‡n.
9. **Device AR pass:** cÃ³ báº±ng chá»©ng cháº¡y trÃªn Ã­t nháº¥t má»™t Android ARCore hoáº·c iOS ARKit.
10. **KhÃ´ng cÃ³ lá»—i Ä‘á» láº·p láº¡i:** compile clean, Play Mode khÃ´ng cÃ³ runtime error thÆ°á»ng trá»±c.

## 6. Ghi ChÃº Thá»±c Thi

KhÃ´ng nÃªn thÃªm activity má»›i trÆ°á»›c khi hoÃ n thÃ nh P0. CÃ¡c lá»—i hiá»‡n táº¡i khÃ´ng náº±m á»Ÿ Ã½ tÆ°á»Ÿng sáº£n pháº©m, mÃ  náº±m á»Ÿ viá»‡c biáº¿n prototype thÃ nh má»™t tráº£i nghiá»‡m há»c AR cÃ³ state flow, input semantics, content pipeline vÃ  kiá»ƒm thá»­ Ä‘á»§ cháº¯c.

Thá»© tá»± sá»­a há»£p lÃ½:

```text
1. Chá»‘t entry scene + activity host
2. LÃ m calibration/learning-area anchor
3. Sá»­a tap-to-count/tap-to-submit
4. Viá»‡t hÃ³a + audio hÆ°á»›ng dáº«n tá»‘i thiá»ƒu
5. HoÃ n thiá»‡n Quantity Match nhÆ° vertical slice chÃ­nh
6. NÃ¢ng Number Line Jump vÃ  Compare Quantity lÃªn cÃ¹ng flow
7. Bá»• sung lesson map + progress analytics
8. Device QA + tests
```