# AR Math Learning - Dự án học toán có AR

## Tổng quan

Dự án monorepo bao gồm:
- **Unity Client**: Ứng dụng AR dạy toán cho trẻ em (apps/unity-client)
- **Backend**: API server xử lý dữ liệu và tiến độ học tập (apps/backend)
- **Docs**: Tài liệu và báo cáo (docs)
- **Scripts**: Automation scripts (scripts)

---

## Cấu trúc thư mục Unity

```
Assets/
├── _Project/              # Scene chính, settings, test harness
├── Features/              # Module tính năng (xem bên dưới)
├── Core/                  # Code nền tảng dùng chung
├── Shared/                # Asset dùng chung nhiều nơi
├── DataDefinitions/       # ScriptableObject định nghĩa nội dung
└── ThirdParty/            # Package bên ngoài
```

### Chi tiết từng khu vực

| Khu vực | Đường dẫn đầy đủ | Mục đích |
|---------|-------------------|----------|
| `_Project/` | `Assets/_Project/` | Scene chính, project settings, test harness |
| `Core/` | `Assets/Core/` | Code nền tảng (AR, Learning, Data layer, UI, Utils) |
| `Features/` | `Assets/Features/` | Module tính năng độc lập |
| `Shared/` | `Assets/Shared/` | Asset dùng chung (prefabs, icons, audio, art) |
| `DataDefinitions/` | `Assets/DataDefinitions/` | ScriptableObject định nghĩa activity, level, skill |
| `ThirdParty/` | `Assets/ThirdParty/` | Package bên ngoài hoặc import thủ công |

### Core/ Structure (Data Layer)

```
Core/
├── AR/                    # PlaneDetection, Placement, Interaction, ARSession
├── Learning/              # Models, Evaluators, ActivityRunner, Progress
├── Data/                  # Repositories, LocalStorage, DTOs, Mappers
├── Support/               # HintSystem, FeedbackSystem, Tutorial
├── UI/                    # BaseScreens, Widgets, Navigation
└── Utils/                 # Extensions, Constants, Helpers
```

**Phân biệt DataDefinitions/ vs Core/Data/:**

- `DataDefinitions/` - Chứa **ScriptableObject định nghĩa nội dung** (activity config, level data)
- `Core/Data/` - Chứa **code đọc/ghi dữ liệu** (repository, local storage, DTO mapper)

---

## Features/ Structure

```
Features/
├── Activities/            # Các bài học AR
│   ├── QuantityMatch/     # Ghép số lượng
│   ├── CompareQuantity/    # So sánh số lượng
│   └── NumberLineJump/    # Nhảy trên trục số
├── Home/                  # Màn hình chính
├── Progress/              # Xem tiến độ học tập
└── ParentMode/            # Chế độ phụ huynh
```

### Cấu trúc 1 Feature Module

```
Features/Activities/QuantityMatch/
├── Scripts/               # (Bắt buộc) Code C#
├── Prefabs/               # (Bắt buộc) Prefab riêng của feature
├── UI/                    # (Bắt buộc) Screen/prefab UI
├── ScriptableObjects/     # (Tùy chọn) Config riêng, nếu có
├── Art/                   # (Tùy chọn) Asset 3D/2D riêng của feature
├── Audio/                 # (Tùy chọn) Audio riêng của feature
└── Tests/                 # (Tùy chọn) Unit test
```

**Quy tắc:**
- `Scenes/` trong feature: **KHÔNG nên có** - MVP dùng scene chung `SC_ARGameplay.unity`
- Chỉ tạo scene riêng khi thật sự cần architecture phức tạp
- Nếu feature cần scene riêng → thêm vào `_Project/Scenes/` với prefix `SC_`

---

## Cấu trúc Scene

```
Assets/_Project/Scenes/
├── SC_Boot.unity              # Khởi tạo service, load config
├── SC_MainMenu.unity          # Màn hình chính
├── SC_ActivitySelect.unity   # Chọn bài học
├── SC_ARGameplay.unity       # Màn chơi AR (dùng chung cho mọi activity)
├── SC_ProgressDashboard.unity # Xem tiến độ
└── SC_TestSandbox.unity      # Test AR, placement, vật thể
```

**Lưu ý**: Activity content được load động vào `SC_ARGameplay` qua ScriptableObject config, không cần scene riêng cho từng bài.

---

## Quy ước đặt tên (Naming Convention)

### Thư mục
```
PascalCase
```
- ✅ `QuantityMatch`, `Core`, `SharedPrefabs`
- ❌ `quantity_match`, `core`, `shared_prefabs`

### Script/C# Class
```
PascalCase.cs
```
- ✅ `ARPlacementController.cs`, `QuantityMatchPresenter.cs`
- ❌ `ar_placement_controller.cs`, `quantitymatch.cs`

### Scene
```
SC_[Tên].unity
```
- ✅ `SC_MainMenu.unity`, `SC_ARGameplay.unity`
- ❌ `MainMenu.unity`, `ARGameplay.unity`

### Prefab
```
PFB_[Mô tả].prefab
```
- ✅ `PFB_ButtonStart.prefab`, `PFB_QuantityCard.prefab`, `PFB_ARObject.prefab`
- ❌ `ButtonStart.prefab`, `QuantityCard.prefab`

### ScriptableObject
```
SO_[Mô tả].asset
```
- ✅ `SO_ActivityQuantityMatch.asset`, `SO_LevelConfig.asset`
- ❌ `Activity.asset`, `LevelConfig.asset`

### Material
```
MAT_[Mô tả].mat
```
- ✅ `MAT_Apple.mat`, `MAT_Ground.mat`
- ❌ `Apple.mat`, `Ground.mat`

### Audio
```
[SFX/BGM]_[Mô tả].[ext]
```
- ✅ `SFX_Correct.wav`, `SFX_Click.mp3`, `BGM_Menu.mp3`
- ❌ `correct.wav`, `click.mp3`, `menu_music.mp3`

### Texture/Image
```
TEX_[Mô tả].[ext]
```
- ✅ `TEX_IconHome.png`, `TEX_BackgroundMenu.jpg`
- ❌ `home_icon.png`, `menu_bg.jpg`

### 3D Model
```
[Mô tả].[ext]
```
- ✅ `Apple.fbx`, `Carrot.fbx`, `NumberTile.fbx`
- ❌ `apple_model.fbx`, `carrot_01.fbx`

### Animation Controller
```
AC_[Mô tả].controller
```
- ✅ `AC_ButtonHover.controller`, `AC_CardFlip.controller`

---

## Cấu trúc Script trong Unity

Code C# nằm trong `Assets/Core/` và `Assets/Features/`:

```
Assets/Core/
├── AR/
│   ├── PlaneDetection/
│   ├── Placement/
│   ├── Interaction/
│   └── ARSession/
├── Learning/
│   ├── Models/
│   ├── Evaluators/
│   ├── ActivityRunner/
│   └── Progress/
├── Data/
│   ├── Repositories/
│   ├── LocalStorage/
│   ├── DTOs/
│   └── Mappers/
├── Support/
│   ├── HintSystem/
│   ├── FeedbackSystem/
│   └── Tutorial/
├── UI/
│   ├── BaseScreens/
│   ├── Widgets/
│   └── Navigation/
└── Utils/
    ├── Extensions/
    ├── Constants/
    └── Helpers/
```

### Pattern đề xuất (MVP)

```
Presenter (Logic) → View (UI) → Model (ScriptableObject/Data)
```

- **Presenter**: Xử lý logic, giao tiếp với Core services
- **View**: Chỉ quản lý UI, gọi Presenter khi có tương tác
- **Model**: Data object, ScriptableObject định nghĩa nội dung

---

## Cấu trúc Backend

```
apps/backend/
├── app/
│   ├── api/          # API endpoints, routes
│   ├── core/         # Business logic, utilities
│   ├── models/       # Database models
│   ├── services/     # Service layer
│   └── repositories/ # Data access layer
├── tests/            # Unit tests
└── docs/             # API documentation
```

---

## Git Workflow (Ngắn gọn)

### Branch naming
```
feature/[tên-tính-năng]
bugfix/[mô-tả-lỗi]
```
- ✅ `feature/quantity-match`, `bugfix/ar-placement`
- ❌ `feature1`, `fix-bug`

### Commit message
```
[type]: [mô tả ngắn]
```
Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

- ✅ `feat: add quantity match activity`
- ❌ `update`, `fix stuff`

Chi tiết: xem `docs/CONTRIBUTING.md`

---

## Thêm tính năng mới

### Các bước

1. **Tạo feature folder** trong `Assets/Features/Activities/[TênMới]/`
2. **Thêm thư mục bắt buộc**: `Scripts/`, `Prefabs/`, `UI/`
3. **Thêm thư mục tùy chọn** nếu cần: `Art/`, `Audio/`, `ScriptableObjects/`
4. **Tuân thủ naming convention**
5. **KHÔNG tạo Scenes/** trong feature - dùng `SC_ARGameplay.unity`

### Ví dụ: Thêm bài học "Ordering"

```
Assets/Features/Activities/Ordering/
├── Scripts/
│   ├── OrderingPresenter.cs
│   ├── OrderingView.cs
│   └── OrderingConfig.cs
├── Prefabs/
│   └── PFB_NumberCard.prefab
├── UI/
│   └── OrderingScreen.prefab
├── Art/                   # (Tùy chọn)
└── Audio/                 # (Tùy chọn)
```

---

## Lưu ý quan trọng

1. **Git LFS**: Dùng cho .unity, .fbx, .png, .mp3 > 5MB
2. **Core/Shared là shared code**: Cẩn thận khi sửa, giữ backward compatible
3. **Scene chỉ trong `_Project/Scenes/`**: Không để scene con trong feature
4. **DataDefinitions/ vs Core/Data/**: Phân biệt rõ - definitions vs code đọc/ghi
5. **Scenes/ trong feature**: Không nên có cho MVP
6. **Test code**: Viết unit test cho Core logic trong `Core/*/Tests/`

---

## Hỗ trợ

- Issue tracker: GitHub Issues
- Chi tiết contributing: `docs/CONTRIBUTING.md`
- Unity Editor: 6000.0.71f1
- AR Framework: AR Foundation + XR Interaction Toolkit
