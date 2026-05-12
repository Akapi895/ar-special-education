# Instruction for Coding Agent — AR Math Learning App

## 1. Bối cảnh dự án

Đây là app học toán bằng AR cho trẻ tiểu học gặp khó khăn với toán cơ bản. App giúp trẻ học số, số lượng, thứ tự số, cộng/trừ thông qua thao tác trực quan với vật thể ảo trong không gian thật.

MVP hiện tại tập trung vào:

- Số trong phạm vi 0–20.
- Đếm số lượng vật thể.
- Ghép số với số lượng.
- So sánh nhiều hơn / ít hơn / bằng nhau.
- Học thứ tự số trên trục số.
- Cộng / trừ đơn giản bằng thao tác AR.
- Lưu tiến độ local.
- Dashboard local đơn giản cho phụ huynh.

Vai trò của phần này là xây dựng **AR Core / Gameplay Foundation**, tức là nền tảng AR chung để các activity học tập có thể chạy trên đó.

---

## 2. Vai trò của Khánh

Khánh chịu trách nhiệm phần **AR Core / Gameplay Foundation**.

Nói đơn giản, nhiệm vụ của Khánh là:

- Làm phần nền AR.
- Làm scene chung để chạy tất cả activity.
- Làm các chức năng cơ bản như scan mặt phẳng, đặt vùng học, spawn object, tương tác chạm/kéo cơ bản.
- Cung cấp event/callback rõ ràng để activity layer có thể dùng.
- Không làm logic toán học của từng activity.
- Không làm local progress hoặc dashboard.

Mục tiêu là tạo ra một “sân chơi AR” ổn định để các bài học có thể chạy trên đó.

---

## 3. Việc cần làm

### Mục tiêu chính

Hãy xây dựng nền tảng AR chung cho toàn bộ ứng dụng.

Phần này không phải là làm logic bài học, mà là tạo ra nền tảng AR ổn định để các activity như **Quantity Match**, **Compare Quantity**, **Number Line Jump** có thể sử dụng lại.

Cần đảm bảo:

1. Có một scene AR gameplay chung.
2. AR camera và AR session hoạt động đúng.
3. Plane detection hoạt động ổn định.
4. Người dùng có thể scan mặt phẳng.
5. Người dùng có thể đặt learning area hoặc anchor lên mặt phẳng.
6. Có thể spawn object mẫu vào không gian AR.
7. Có interaction cơ bản: tap, drag, select.
8. Có event/callback rõ ràng để activity layer nhận dữ liệu.
9. Code được tách lớp rõ ràng, dễ mở rộng.

---

## 4. Scene cần tạo / sử dụng

Tạo hoặc cấu hình scene chính:

```text
Assets/_Project/Scenes/SC_ARGameplay.unity
```

Scene này là scene AR gameplay chung cho toàn bộ app.

Không tạo scene riêng cho từng activity như:

```text
SC_QuantityMatch.unity
SC_CompareQuantity.unity
SC_NumberLineJump.unity
```

Nếu cần test riêng, có thể dùng:

```text
Assets/_Project/Scenes/SC_TestSandbox.unity
```

---

## 5. Folder structure cần tuân thủ

Tổ chức code theo cấu trúc sau:

```text
Assets/
├── _Project/
│   └── Scenes/
│       ├── SC_Boot.unity
│       ├── SC_MainMenu.unity
│       ├── SC_ActivitySelect.unity
│       ├── SC_ARGameplay.unity
│       └── SC_TestSandbox.unity
├── Core/
│   └── AR/
│       ├── ARSession/
│       │   └── ARSessionBootstrap.cs
│       ├── PlaneDetection/
│       │   └── ARPlaneDetectionController.cs
│       ├── Placement/
│       │   ├── ARPlacementController.cs
│       │   └── LearningAreaAnchor.cs
│       └── Interaction/
│           ├── ARTapInteractor.cs
│           ├── ARDragInteractor.cs
│           └── ARSelectableObject.cs
└── Shared/
    └── Prefabs/
```

---

## 6. Các module cần implement

### 6.1. AR Session Bootstrap

File gợi ý:

```text
Assets/Core/AR/ARSession/ARSessionBootstrap.cs
```

Nhiệm vụ:

- Khởi tạo AR Session.
- Đảm bảo AR Camera hoạt động.
- Kết nối các service AR cần thiết.
- Không chứa logic học tập.
- Không chứa logic riêng của activity nào.

---

### 6.2. Plane Detection

File gợi ý:

```text
Assets/Core/AR/PlaneDetection/ARPlaneDetectionController.cs
```

Nhiệm vụ:

- Phát hiện mặt phẳng.
- Hiển thị hoặc cập nhật trạng thái scan.
- Bắn event khi tìm thấy mặt phẳng hợp lệ.
- Cho phép bật/tắt plane visualization nếu cần.
- Không xử lý logic bài học.

Event nên có:

```csharp
OnPlaneDetected
OnPlaneLost
OnPlaneScanUpdated
```

---

### 6.3. Placement System

File gợi ý:

```text
Assets/Core/AR/Placement/ARPlacementController.cs
Assets/Core/AR/Placement/LearningAreaAnchor.cs
```

Nhiệm vụ:

- Cho phép người dùng đặt learning area lên mặt phẳng.
- Spawn object mẫu vào AR scene.
- Anchor object theo mặt phẳng đã chọn.
- Cung cấp API cho activity layer gọi spawn object.

API gợi ý:

```csharp
PlaceLearningArea(Vector3 position, Quaternion rotation)
SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
ClearSpawnedObjects()
```

Event nên có:

```csharp
OnLearningAreaPlaced
OnObjectSpawned
OnPlacementFailed
```

---

### 6.4. Interaction System

File gợi ý:

```text
Assets/Core/AR/Interaction/ARTapInteractor.cs
Assets/Core/AR/Interaction/ARDragInteractor.cs
Assets/Core/AR/Interaction/ARSelectableObject.cs
```

Nhiệm vụ:

- Tap vào object AR.
- Drag object AR.
- Select object AR.
- Bắn event để activity layer xử lý.
- Không tự kiểm tra đúng/sai.
- Không tự quyết định logic toán học.

Event nên có:

```csharp
OnObjectTapped
OnObjectDragged
OnObjectSelected
OnObjectReleased
```

---

### 6.5. Mock / Placeholder Prefabs

Tạo prefab mẫu để team activity có thể test mà không cần asset cuối.

Cần có:

```text
Assets/Shared/Prefabs/PFB_LearningAreaMarker.prefab
Assets/Shared/Prefabs/PFB_ARMockObject.prefab
Assets/Shared/Prefabs/PFB_ARInteractiveObject.prefab
Assets/Shared/Prefabs/PFB_ARAnchorMarker.prefab
```

Yêu cầu:

- Prefab đơn giản.
- Dễ thay thế asset thật sau này.
- Có collider nếu cần interaction.
- Có component `ARSelectableObject` nếu cần tap/select/drag.

---

## 7. Những điều cần chú ý

### 7.1. Tách AR Core khỏi Learning Logic

AR Core chỉ xử lý:

- AR session.
- Camera.
- Plane detection.
- Placement.
- Spawn object.
- Tap / drag / select.
- Event callback.

AR Core không được xử lý:

- Đáp án đúng/sai.
- Logic cộng/trừ.
- Logic so sánh số lượng.
- Logic Number Line Jump.
- Hint.
- Feedback học tập.
- Local progress.

Ví dụ không nên làm:

```csharp
if (selectedCount == correctNumber)
{
    ShowCorrectFeedback();
}
```

Logic kiểu này thuộc về Learning Activity, không thuộc AR Core.

---

### 7.2. Không hardcode activity cụ thể

Không được viết code kiểu:

```csharp
if (activityName == "QuantityMatch")
{
    SpawnAppleGroup();
}
```

AR Core phải dùng được cho nhiều activity khác nhau.

Thay vào đó, nên để activity layer truyền prefab/config vào AR Core.

---

### 7.3. Không trộn UI logic vào AR Core

AR Core có thể bắn event trạng thái như:

```csharp
OnPlaneDetected
OnLearningAreaPlaced
OnPlacementFailed
```

Nhưng không nên tự xử lý UI phức tạp.

Ví dụ nên tránh:

```csharp
instructionPanel.text = "Great job!";
```

UI layer hoặc activity layer sẽ nhận event rồi tự hiển thị.

---

### 7.4. Class phải có trách nhiệm rõ ràng

Mỗi class chỉ nên làm một việc chính.

Ví dụ:

- `ARSessionBootstrap` chỉ lo khởi tạo AR.
- `ARPlaneDetectionController` chỉ lo plane detection.
- `ARPlacementController` chỉ lo placement/spawn.
- `ARTapInteractor` chỉ lo tap.
- `ARDragInteractor` chỉ lo drag.
- `ARSelectableObject` chỉ đại diện cho object có thể tương tác.

Không gom toàn bộ logic vào một file lớn như:

```text
ARManager.cs
MainScript.cs
GameController.cs
TestScript.cs
```

---

## 8. Không cần làm

Agent không cần làm các phần sau:

- Không làm logic riêng cho Quantity Match.
- Không làm logic riêng cho Compare Quantity.
- Không làm logic riêng cho Number Line Jump.
- Không làm phép cộng/trừ.
- Không làm kiểm tra đáp án.
- Không làm hint system.
- Không làm feedback system.
- Không làm local storage.
- Không làm dashboard phụ huynh.
- Không làm backend.
- Không làm account/user login.
- Không làm đồng bộ nhiều thiết bị.
- Không làm nhân/chia/phân số/hình học nâng cao.
- Không tự đổi cấu trúc repo nếu chưa được yêu cầu.
- Không sửa package/template Unity lung tung nếu không thật sự cần.

Nếu task nằm giữa AR Core và Learning Activity, hãy để TODO rõ ràng và không tự implement quá phạm vi.

---

## 9. Thứ tự triển khai ưu tiên

### Giai đoạn 1 — Setup nền tảng AR

- Kiểm tra Unity project và AR packages.
- Tạo/cấu hình `SC_ARGameplay.unity`.
- Đảm bảo AR Session chạy.
- Đảm bảo AR Camera hoạt động.
- Test plane detection cơ bản.

### Giai đoạn 2 — Placement

- Làm learning area placement.
- Làm anchor trên mặt phẳng.
- Làm spawn object mẫu.
- Có trạng thái placement thành công/thất bại.

### Giai đoạn 3 — Interaction

- Tap object.
- Select object.
- Drag object.
- Release object.
- Bắn event tương ứng.

### Giai đoạn 4 — API cho Activity Layer

- Cung cấp API spawn object.
- Cung cấp API clear object.
- Cung cấp callback interaction.
- Đảm bảo activity layer có thể dùng mà không cần sửa code AR Core.

### Giai đoạn 5 — Dọn code và ổn định

- Tách class rõ ràng.
- Đặt tên đúng convention.
- Xóa code test không cần thiết.
- Viết comment ngắn cho API chính.
- Đảm bảo scene test không bị nhầm thành scene production.

---

## 10. Coding convention

Tuân thủ các quy ước sau:

- Class và file dùng PascalCase.
- Mỗi file chỉ có một class chính.
- Tên class mô tả đúng chức năng.
- Không dùng tên mơ hồ như `Script1`, `Test2`, `ManagerNew`.
- Không viết class quá lớn.
- Không hardcode activity name.
- Không hardcode learning content.
- Prefab mẫu phải dễ thay thế.
- Event/callback phải rõ ràng và dễ dùng.

---

## 11. Definition of Done

Task được coi là hoàn thành khi:

- Mở `SC_ARGameplay.unity` là chạy được.
- AR Camera khởi tạo đúng.
- Plane detection hoạt động.
- Có thể scan mặt phẳng.
- Có thể đặt learning area lên mặt phẳng.
- Có thể spawn object mẫu vào AR scene.
- Có thể tap object.
- Có thể select object.
- Có thể drag object.
- Có event/callback cho activity layer.
- Activity layer có thể gọi API placement/spawn mà không sửa AR Core.
- Code nằm đúng folder.
- Không có logic toán học trong AR Core.
- Không có hint/feedback/progress/dashboard trong AR Core.
- Có mock prefab đủ để team activity test.

---

## 12. Ghi chú quan trọng

Đây là phần nền móng của app, không phải phần mini-game.

Mục tiêu là tạo một AR gameplay foundation ổn định, sạch, có thể tái sử dụng cho nhiều activity khác nhau.

Hãy ưu tiên:

- Đơn giản.
- Rõ ràng.
- Dễ mở rộng.
- Không hardcode.
- Không trộn logic.
- Không làm vượt phạm vi MVP.
