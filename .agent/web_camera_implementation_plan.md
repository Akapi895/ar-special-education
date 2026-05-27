# Implementation Plan - Tich Hop Camera WebGL Va Fallback

**Ngay tao:** 2026-05-27  
**Pham vi:** Unity client `apps/unity-client`  
**Muc tieu:** Deploy WebGL cho dien thoai, hien camera that lam nen, dong thoi van hien UI button va animal gameplay. Neu khong co camera hoac bi tu choi quyen, game tu dong fallback sang che do mo phong va van choi duoc.  
**Trang thai:** Ke hoach trien khai, chua code trong buoc nay.

## 1. Ket Luan Hien Trang

Code hien tai **chua san sang** cho WebGL + camera:

- Chua co `WebCamTexture`, `getUserMedia`, `.jslib`, hoac luong xin quyen camera tren web.
- `ARServiceBootstrap` chi dung `ARPlacementServiceMock` trong Editor desktop. Khi WebGL build, app co kha nang chay vao `ARPlacementService` that, ma service nay phu thuoc AR Foundation raycast/plane.
- Runtime UI cua cac activity dang la `ScreenSpaceOverlay`, nen button co the hien thi tot neu co camera background dung cach.
- `IARPlacementService` da co cac API can thiet nhu `LearningAreaContentRoot`, `SpawnAtLearningAreaPosition`, `LearningAreaSizeMeters`, nen co the them service placement rieng cho WebGL ma khong can viet lai gameplay.

## 2. Nguyen Tac Thiet Ke

1. WebGL camera mode **khong phai AR Foundation**. Camera chi la video background, khong co plane tracking that.
2. Animal va UI phai chay doc lap voi viec camera co bat duoc hay khong.
3. Khong de camera background che UI hoac che animal 3D.
4. Runtime script cho WebGL khong duoc phu thuoc `UnityEditor`.
5. Neu camera loi, permission denied, insecure origin, hoac browser khong ho tro, game van vao duoc bang fallback.
6. Moi phase phai co compile gate ro rang.

## 3. Kien Truc Muc Tieu

```text
SC_ARGameplay
-> LearningSceneServices
-> ARServiceBootstrap
   -> WebGL: WebPlacementService + ARSessionFallback
   -> Editor desktop: ARPlacementServiceMock
   -> Android/iOS native AR: ARPlacementService
-> WebCameraBootstrap
   -> request camera permission
   -> start WebCamTexture
   -> render camera to camera-space background quad
   -> fallback visual background if failed
-> Activity runtime UI
   -> ScreenSpaceOverlay, sortingOrder cao
-> Activity presenters
   -> spawn animals under LearningAreaContentRoot
```

### 3.1 Layer Render De Xuat

| Layer | Cach render | Muc dich |
|---|---|---|
| Camera video | Quad gan voi main camera, material unlit, render queue background, `ZWrite Off` | Nam sau animal 3D |
| Animal/object 3D | Camera chinh render binh thuong | Noi dung hoc tap |
| UI button/question/feedback | `ScreenSpaceOverlay` | Luon nam tren cung |

Khong nen dung `RawImage` overlay lam camera background vi overlay canvas co the ve len tren scene 3D va che animal.

## 4. Phase 0 - Baseline Va Build Gate

**Muc tieu:** Xac nhan trang thai build hien tai truoc khi them camera.

### Cong viec

1. Chay `git status` de ghi nhan file dang dirty, khong revert thay doi cua nguoi dung.
2. Chay compile C# hien tai:
   - `dotnet restore ar-special-education.sln` neu thieu assets.
   - `dotnet build ar-special-education.sln --no-restore`.
3. Kiem tra scene build settings da co `SC_ARGameplay`.
4. Ghi lai Unity version va build target WebGL hien tai.

### Acceptance criteria

- Build C# hien tai pass truoc khi them code camera.
- Co log baseline trong `.agent` hoac console.
- Khong co thay doi ngoai y muon trong scene/prefab.

## 5. Phase 1 - Web Runtime Mode Va Placement Rieng Cho WebGL

**Muc tieu:** Dam bao WebGL khong di vao `ARPlacementService` phu thuoc AR Foundation.

### File them/sua du kien

| File | Thay doi |
|---|---|
| `Assets/Core/AR/Placement/WebPlacementService.cs` | Service placement rieng cho WebGL, implement `IARPlacementService`, khong dung AR Foundation |
| `Assets/Core/AR/ARServiceBootstrap.cs` | Chon `WebPlacementService` khi `UNITY_WEBGL && !UNITY_EDITOR` |
| `Assets/Core/AR/ARSession/ARSessionFallback.cs` | Dung lai lam session always-ready cho WebGL |

### Chi tiet thuc hien

1. Tao `WebPlacementService : MonoBehaviour, IARPlacementService`.
2. Khi initialize:
   - Tao `LearningAreaAnchor` runtime neu chua co.
   - Dat learning area o vi tri on dinh truoc camera, vi du `(0, 0, 3.0f)`.
   - Set `LearningAreaSizeMeters`, vi du `4.2 x 2.4`.
   - `IsPlacementAvailable = true`.
   - Fire `OnPlacementPositionAvailable`.
3. Implement cac ham spawn:
   - `SpawnAtPlacementPosition`.
   - `SpawnAtPosition`.
   - `SpawnAtLearningAreaPosition`.
   - `SpawnGrid`, `SpawnCircle`.
   - `ClearSpawnedObjects`.
4. Sua `ARServiceBootstrap`:
   - Neu WebGL runtime thi tao/tim `WebPlacementService`.
   - Dung `ARSessionFallback`.
   - Van resolve `ARInteractionService`.
5. Khong xoa `ARPlacementService` vi Android/iOS native AR van co the dung ve sau.

### Acceptance criteria

- WebGL path khong can `ARRaycastManager`, `ARPlaneManager`, `ARCameraBackground`.
- Activity co the start khi WebGL khong co AR Foundation session.
- Animals spawn duoi `LearningAreaContentRoot`.
- C# compile pass sau phase.

## 6. Phase 2 - Camera Background Cho WebGL

**Muc tieu:** Lay camera dien thoai/browser lam nen video, khong che animal va UI.

### File them/sua du kien

| File | Thay doi |
|---|---|
| `Assets/Core/AR/Web/WebCameraBootstrap.cs` | Quan ly camera permission, start/stop camera, fallback |
| `Assets/Core/AR/Web/WebCameraBackground.cs` | Gan `WebCamTexture` len background quad |
| `Assets/Core/AR/Web/WebCameraBackground.shader` | Shader unlit ve video sau scene, `ZWrite Off` |
| `Assets/_Project/Scripts/LearningSceneServices.cs` | Dam bao web camera bootstrap duoc tao trong gameplay scene |

### Chi tiet thuc hien

1. Tao `WebCameraBootstrap`.
2. Khi chay WebGL:
   - Hien nut UI "Bat camera".
   - Chi request camera sau khi nguoi dung bam nut, tranh browser chan permission do khong co user gesture.
3. Dung `Application.RequestUserAuthorization(UserAuthorization.WebCam)`.
4. Chon camera sau neu co:
   - Uu tien `WebCamDevice.isFrontFacing == false`.
   - Neu khong co, dung camera dau tien.
5. Start `WebCamTexture`.
6. Cho den khi `webCamTexture.width > 16` va `height > 16` moi coi la ready.
7. Gan texture vao material cua camera background quad.
8. Xu ly:
   - aspect ratio;
   - mirror;
   - `videoRotationAngle`;
   - pause/resume;
   - stop/dispose khi destroy.
9. Neu timeout hoac denied thi goi fallback.

### Render background quad

1. Tao quad con cua `Camera.main`.
2. Dat quad sat sau near clip nhung shader khong ghi depth.
3. Tinh scale quad theo frustum camera:
   - `height = 2 * distance * tan(fieldOfView / 2)`;
   - `width = height * camera.aspect`.
4. Material:
   - unlit;
   - render queue background;
   - `ZWrite Off`;
   - khong nhan shadow;
   - khong che object 3D.

### Acceptance criteria

- Tren WebGL, bam "Bat camera" thi browser hien permission prompt.
- Khi cho phep, video camera hien sau animal.
- Animal 3D van nhin thay ro.
- UI button/question van nam tren cung va bam duoc.
- Neu tu choi camera, game fallback va van spawn animals.
- C# compile pass sau phase.

## 7. Phase 3 - Fallback Khi Khong Co Camera

**Muc tieu:** Khong de nguoi choi bi ket neu camera loi.

### Cac truong hop phai fallback

| Truong hop | Cach xu ly |
|---|---|
| Browser khong ho tro camera | Hien nen mo phong, van choi |
| Permission denied | Hien thong bao than thien, van choi |
| Khong chay HTTPS | Hien thong bao "Can mo bang HTTPS de bat camera", van choi |
| `WebCamTexture` timeout | Dung nen mo phong |
| Khong tim thay camera sau | Dung camera co san hoac fallback |
| Camera bi stop khi tab background | Thu resume khi tab active lai, neu fail thi fallback |

### UI fallback

Thong bao de xuat:

- "Khong bat duoc camera. Con van co the choi o che do mo phong."
- "Neu muon dung camera, hay mo bang HTTPS va cho phep quyen camera."

Thong bao nay chi hien cho phu huynh/nghe ky thuat, khong can chen nhieu chu vao bai hoc cua tre.

### Acceptance criteria

- Denied permission khong lam crash.
- Timeout khong lam scene dung yen.
- Activity van start va button van bam duoc.
- Co log ro rang de debug.

## 8. Phase 4 - Dam Bao UI Va Gameplay Song Song Voi Camera

**Muc tieu:** Camera, animal, button cung hien dung thu tu.

### Cong viec

1. Kiem tra `QuantityMatchRuntimeUI`, `CompareQuantityRuntimeUI`, `NumberLineJumpRuntimeUI`:
   - tiep tuc dung `ScreenSpaceOverlay`;
   - set sorting order thong nhat, vi du `20`;
   - dam bao co `GraphicRaycaster` va `EventSystem`.
2. Dam bao camera background khong dung overlay canvas.
3. Dam bao `ARInteractionService` khong raycast vao background quad:
   - background quad dat layer rieng;
   - physics collider khong ton tai tren background;
   - neu co layer mask, loai background layer.
4. Dam bao animal co collider/hitbox rieng va van tap duoc tren WebGL/mobile.
5. Kiem tra question text, keypad number, compare buttons, home button.

### Acceptance criteria

- Quantity Match: group circle hoac animal tap duoc, number buttons hien du.
- Compare Quantity: nut `>`, `<`, `=` hien va bam duoc.
- Number Line Jump: nut dieu huong hien va bam duoc.
- Camera background khong chan click UI.

## 9. Phase 5 - Build WebGL Va Compile Camera

**Muc tieu:** Chung minh code camera compile duoc trong WebGL build.

### File them du kien

| File | Muc dich |
|---|---|
| `Assets/_Project/Editor/WebGLCameraBuildValidator.cs` | Editor utility de build/validate WebGL development build |

### Build gates

1. C# compile:
   - `dotnet build ar-special-education.sln --no-restore`.
2. Unity compile trong Editor:
   - Mo project trong Unity, dam bao Console khong co compile error.
3. WebGL build:
   - Build target `WebGL`.
   - Build development vao thu muc tam, vi du `Builds/WebGLCameraSmoke`.
4. Neu co script validator:
   - Goi `BuildPipeline.BuildPlayer` voi cac scene trong `EditorBuildSettings`.
   - Fail neu co `BuildResult.Failed`.

### Quy tac compile

- Runtime camera scripts khong dung `UnityEditor`.
- WebGL-specific code boc trong:

```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
// Web runtime behavior
#endif
```

- Non-WebGL path van compile va khong anh huong Android/iOS/Editor.
- Khong them package ngoai neu khong can.

### Acceptance criteria

- `dotnet build` pass.
- Unity Editor compile pass.
- WebGL development build pass.
- Khong co loi missing AR Foundation provider tren WebGL runtime path.

## 10. Phase 6 - Test Thiet Bi That

**Muc tieu:** Xac nhan tren dien thoai that.

### Dieu kien test

- Deploy bang HTTPS:
  - Firebase Hosting, Netlify, Vercel, GitHub Pages, hoac server HTTPS noi bo.
- Test it nhat:
  - Android Chrome;
  - iOS Safari neu co thiet bi.

### Checklist

| Test | Ky vong |
|---|---|
| Mo web lan dau | Load duoc menu va vao game |
| Bam "Bat camera" | Browser hien permission prompt |
| Allow camera | Video camera hien sau animals |
| Deny camera | Fallback hien, game van choi duoc |
| Animal render | Khong bi che, khong bien mat |
| UI render | Cau hoi, nut nghe lai, home, keypad/nut dap an hien ro |
| Touch UI | Bam nut khong bi background chan |
| Touch animal/group | Raycast gameplay van hoat dong |
| Rotate dien thoai | Aspect camera va UI khong vo |
| Refresh trang | Camera co the xin/start lai |

### Acceptance criteria

- Co it nhat 1 thiet bi Android Chrome chay pass camera mode.
- Co fallback pass khi deny permission.
- Khong co crash/soft-lock khi camera unavailable.

## 11. Phase 7 - Polish Sau Khi Camera Chay

**Muc tieu:** Lam trai nghiem gan voi AR-gia-lap hon, nhung khong anh huong compile.

### Cong viec

1. Them overlay huong dan ngan:
   - "Huong camera vao mat ban/san"
   - "Bam de dat khu hoc"
2. Cho phu huynh chon vi tri learning area bang tap man hinh:
   - map tap screen sang mot diem co dinh trong Unity world;
   - khong goi AR raycast.
3. Them nut "Doi nen mo phong / Dung camera".
4. Them warning neu device khong ho tro camera sau.
5. Ghi analytics nhe:
   - camera allowed/denied;
   - fallback reason;
   - browser/device info co ban.

## 12. Rui Ro Va Cach Giam Thieu

| Rui ro | Tac dong | Giam thieu |
|---|---|---|
| Mobile browser can HTTPS de camera | Camera khong bat | Bat buoc deploy HTTPS, fallback khi insecure |
| iOS Safari han che autoplay/permission | Camera kho start neu khong co gesture | Luon co nut "Bat camera" |
| `WebCamTexture` khong chon dung camera sau | Camera truoc duoc bat | Uu tien back-facing, cho phep doi camera o phase polish |
| Video aspect/rotation sai | Hinh bi xoay/crop | Xu ly `videoRotationAngle`, mirror, aspect fit/fill |
| Background che animal | Game khong thay object | Dung camera-space quad + shader `ZWrite Off`, khong dung overlay RawImage |
| WebGL khong co plane tracking | Khong phai AR that | Ghi ro day la camera background mode, dung WebPlacementService |
| Camera fail lam activity khong start | Soft-lock | Camera va placement doc lap; fallback van start activity |

## 13. Definition Of Done

He thong duoc coi la hoan thanh cho camera WebGL khi:

1. WebGL build thanh cong voi code camera.
2. Tren mobile browser HTTPS, nguoi dung co the bam "Bat camera".
3. Video camera hien lam background.
4. Animal 3D va vong/nut/label van render tren camera.
5. UI button van hien, bam duoc, khong bi background chan.
6. Neu camera bi tu choi hoac khong co, game fallback va van choi duoc.
7. Quantity Match, Compare Quantity, Number Line Jump deu co smoke test trong camera mode/fallback mode.
8. Co log ro rang khi camera allowed/denied/timeout/fallback.

## 14. Thu Tu Trien Khai Khuyen Nghi

```text
Phase 0 - Baseline build
Phase 1 - WebPlacementService + ARServiceBootstrap WebGL path
Phase 2 - WebCameraBootstrap + WebCameraBackground + shader
Phase 3 - Fallback no-camera
Phase 4 - UI/gameplay layering validation
Phase 5 - WebGL compile/build validator
Phase 6 - Device HTTPS QA
Phase 7 - Polish UX camera mode
```

Khong nen lam camera truoc placement WebGL. Neu camera chay nhung placement van di vao AR Foundation, animals co the khong spawn va viec test se gay nham lan.
