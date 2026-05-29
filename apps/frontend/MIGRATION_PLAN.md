# KẾ HOẠCH CHUYỂN ĐỔI: MATHMATE SUPPORT
## Dành cho phụ huynh có con gặp vấn đề Dyscalculia (4-6 tuổi)

---

## TÌNH TRẠNG HIỆN TẠI

### ✅ Đã hoàn thành (Phase 1 & 2)

| Thành phần | Trạng thái |
|------------|------------|
| **Xóa Child App** | ✅ Hoàn thành |
| **Xóa Rewards** | ✅ Hoàn thành |
| **Xóa Tasks** | ✅ Hoàn thành |
| **Mock Data Layer** | ✅ Hoàn thành |
| **Exercise Library Page** | ✅ Hoàn thành |
| **Onboarding (tạm)** | ✅ Hoàn thành |

### 📋 Cần làm tiếp

| Thành phần | Ưu tiên |
|------------|---------|
| **Dashboard mới** | 🔴 CAO |
| **Emotion Journal** | 🔴 CAO |
| **Exercise Suggestion** | 🔴 CAO |
| **Methods Page** | 🟡 TRUNG BÌNH |
| **Landing Page mới** | 🟡 TRUNG BÌNH |
| **Settings mới** | 🟢 THẤP |

---

# PHẦN 1: DASHBOARD MỚI

## 1.1 Wireframe Dashboard

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  HEADER                                                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  [MathMate Support Logo]           [🔔 Thông báo]    [Cài đặt]  [👤 Minh ▼] │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  CHÀO MỪNG TRỞ LẠI!                                                  │   │
│  │  👶 Con: Minh (5 tuổi)  │  🔥 Streak: 5 ngày  │  📅 28/05/2026   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  GỢI Ý BÀI TẬP HÔM NAY                                             │   │
│  │  ┌───────────────────────────────────────────────────────────────┐ │   │
│  │  │  🟡 Pictorial  │  Ghép số với hình 1-10                     │ │   │
│  │  │  "Con đã vững Concrete, nên chuyển sang Pictorial"          │ │   │
│  │  │  [Bắt đầu bài tập này]                                     │ │   │
│  │  └───────────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌────────────────────────────┐  ┌────────────────────────────────────┐   │
│  │  TIẾN ĐỘ CPA (Trọng số) │  │  THỐNG KÊ HỌC TẬP                 │   │
│  │                            │  │                                    │   │
│  │  Concrete  ████████░░ 75% │  │  📚 Bài tập    │  ⏱️ Thời gian  │   │
│  │  Pictorial █████░░░░░░ 40% │  │     12/50      │    3h 20p      │   │
│  │  Abstract  ██░░░░░░░░░ 15% │  │               │                │   │
│  │                            │  │  🔄 Streak     │  📊 Đúng/Sai   │   │
│  │  ┌──────────────────────┐ │  │     5 ngày     │    85% / 15%  │   │
│  │  │  C ●━━━━━━○ P ●━━○ A │ │  │               │                │   │
│  │  └──────────────────────┘ │  └────────────────────────────────────┘   │
│  └────────────────────────────┘                                            │
│                                                                             │
│  ┌────────────────────────────┐  ┌────────────────────────────────────┐   │
│  │  TIẾN ĐỘ THEO LOẠI TOÁN  │  │  NHẬT KÝ CẢM XÚC (7 NGÀY)        │   │
│  │                            │  │                                    │   │
│  │  Đếm số     ████████░░ 80%│  │  😊😊😊😊😐😟😊                   │   │
│  │  So sánh    █████░░░░░░ 50%│  │                                    │   │
│  │  Cộng       ███░░░░░░░░ 30%│  │  Nhận xét: Con ổn định, ít biểu │   │
│  │  Trừ        █░░░░░░░░░░ 10%│  │  hiện lo lắng khi học.         │   │
│  │                            │  │                                    │   │
│  │  [Xem chi tiết]           │  │  [+ Ghi nhận hôm nay]            │   │
│  └────────────────────────────┘  └────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  HOẠT ĐỘNG GẦN ĐÂY                                                  │   │
│  │  ┌───────────────────────────────────────────────────────────────┐ │   │
│  │  │ 🟢 15 phút trước                                              │ │   │
│  │  │ Con hoàn thành "Đếm đồ vật 1-5" (Concrete)                  │ │   │
│  │  │ ✅ 8/10 đúng  │ 😊 Vui vẻ                                    │ │   │
│  │  └───────────────────────────────────────────────────────────────┘ │   │
│  │  ┌───────────────────────────────────────────────────────────────┐ │   │
│  │  │ 🟡 1 giờ trước                                               │ │   │
│  │  │ Ghi nhận cảm xúc: Con hứng thú với bài tập mới             │ │   │
│  │  └───────────────────────────────────────────────────────────────┘ │   │
│  │  ┌───────────────────────────────────────────────────────────────┐ │   │
│  │  │ ⚪ Hôm qua                                                     │ │   │
│  │  │ Con hoàn thành "So sánh nhiều hơn - ít hơn"                 │ │   │
│  │  │ ✅ 6/10 đúng  │ 😐 Bình thường                                │ │   │
│  │  └───────────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  HÀNH ĐỘNG NHANH                                                  │   │
│  │                                                                      │   │
│  │  [+ Ghi nhận bài tập]  [+ Ghi nhận cảm xúc]  [📚 Bài tập gợi ý]  │   │
│  │  [💡 Xem phương pháp CPA]         [📊 Xem báo cáo tuần]          │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 1.2 Component Structure

```
src/features/parents/dashboard/
├── DashboardPage.tsx                    # Main dashboard layout
│
├── components/
│   ├── WelcomeBanner.tsx               # Header với lời chào
│   ├── CPAGauge.tsx                    # Đồng hồ tiến độ CPA
│   ├── MathTypeProgress.tsx            # Thanh tiến độ theo loại toán
│   ├── StatsOverview.tsx               # Thống kê tổng quan
│   ├── SuggestedExercise.tsx            # Bài tập gợi ý
│   ├── EmotionTracker.tsx              # Biểu đồ cảm xúc
│   ├── ActivityFeed.tsx                # Hoạt động gần đây
│   └── QuickActions.tsx                # Hành động nhanh
│
└── index.ts
```

## 1.3 Chi tiết từng Component

### WelcomeBanner
```tsx
interface WelcomeBannerProps {
  childName: string;
  childAge: number;
  streak: number;
}

// UI Elements:
// - Icon 👶 + Tên con + Tuổi
// - Streak badge 🔥 với số ngày
// - Ngày hiện tại
// - Background gradient xanh dịu
```

### CPAGauge
```tsx
interface CPAGaugeProps {
  concrete: number;   // 0-100
  pictorial: number; // 0-100
  abstract: number;  // 0-100
}

// UI Elements:
// - 3 progress bars ngang với màu sắc riêng
//   - Concrete: Blue (#3B82F6)
//   - Pictorial: Yellow (#F59E0B)
//   - Abstract: Purple (#8B5CF6)
// - Progress ring ở giữa
// - Label từng giai đoạn
// - Mũi tên chỉ giai đoạn hiện tại
```

### MathTypeProgress
```tsx
interface MathTypeProgressProps {
  counting: { current: number; target: number };
  comparison: { current: number; target: number };
  addition: { current: number; target: number };
  subtraction: { current: number; target: number };
}

// UI Elements:
// - 4 progress bars dọc
// - Icon cho từng loại:
//   - Đếm số: 🔢
//   - So sánh: ⚖️
//   - Cộng: ➕
//   - Trừ: ➖
// - Percentage hiển thị
```

### StatsOverview
```tsx
interface StatsOverviewProps {
  exercisesCompleted: number;
  totalExercises: number;
  totalTimeSpent: number; // minutes
  streak: number; // days
  correctRate: number; // percentage
  incorrectRate: number; // percentage
}

// UI Elements:
// - 2x2 grid với 4 stat cards
// - Mỗi card có icon, value, label
// - Màu sắc theo loại thống kê
```

### SuggestedExercise
```tsx
interface SuggestedExerciseProps {
  exercise: {
    id: string;
    title: string;
    cpaStage: 'concrete' | 'pictorial' | 'abstract';
    description: string;
    reason: string; // Tại sao gợi ý bài này
  };
  onStart: () => void;
}

// UI Elements:
// - Card nổi bật (border gradient)
// - CPA stage badge
// - Title + Description
// - Reason text (italic, muted)
// - Button "Bắt đầu bài tập này"
```

### EmotionTracker
```tsx
interface EmotionTrackerProps {
  entries: {
    date: string;
    emotion: 'happy' | 'frustrated' | 'anxious' | 'neutral' | 'proud';
  }[];
  onAddEntry: () => void;
}

// UI Elements:
// - 7 emoji hiển thị 7 ngày gần nhất
// - Emoji mapping:
//   - happy: 😊
//   - frustrated: 😟
//   - anxious: 😰
//   - neutral: 😐
//   - proud: 😎
// - Nhận xét tổng quát
// - Button "+ Ghi nhận hôm nay"
```

### ActivityFeed
```tsx
interface ActivityFeedProps {
  activities: {
    id: string;
    type: 'exercise' | 'emotion' | 'milestone';
    time: string;
    title: string;
    details?: string;
    emotion?: string;
    accuracy?: number;
  }[];
}

// UI Elements:
// - Timeline với các entry
// - Icon theo loại:
//   - exercise: ✅
//   - emotion: 💭
//   - milestone: 🎉
// - Thời gian hiển thị tương đối
// - Chi tiết bài tập/cảm xúc
```

### QuickActions
```tsx
interface QuickActionsProps {
  onAddExercise: () => void;
  onAddEmotion: () => void;
  onViewSuggestions: () => void;
  onViewMethods: () => void;
  onViewReport: () => void;
}

// UI Elements:
// - 4-5 buttons trên 1 row
// - Icon + Label cho mỗi action
// - Responsive: 2-3 rows trên mobile
```

---

# PHẦN 2: EMOTION JOURNAL

## 2.1 Wireframe

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  NHẬT KÝ CẢM XÚC                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [Tabs: Nhật ký cảm xúc | Báo cáo tuần]                                    │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  [+ Thêm ghi chép mới]                                              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  HÔM NAY - 28/05/2026                              [Sửa] [Xóa]      │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  😊 Cảm xúc chính: Vui vẻ                                          │   │
│  │  📚 Bài tập: Đếm đồ vật 1-5 (đã hoàn thành)                      │   │
│  │  ⏱️ Thời gian: 20 phút                                             │   │
│  │                                                                      │   │
│  │  📝 Ghi chú:                                                        │   │
│  │  "Con rất hứng thú với bài tập. Con đếm đúng 8/10 lần.           │   │
│  │  Con tỏ ra tự hào khi hoàn thành!"                                │   │
│  │                                                                      │   │
│  │  💡 Điều gì khiến con vui:                                         │   │
│  │  • Được khen ngợi                                                 │   │
│  │  • Dùng đồ chơi màu sắc                                           │   │
│  │                                                                      │   │
│  │  📋 Hành động tiếp theo:                                           │   │
│  │  • Tiếp tục với bài đếm 6-10                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  HÔM QUA - 27/05/2026                              [Sửa] [Xóa]      │   │
│  │  ...                                                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  BIỂU ĐỒ CẢM XÚC (7 NGÀY)                                          │   │
│  │                                                                      │   │
│  │  T2  T3  T4  T5  T6  T7  CN                                         │   │
│  │  😊  😊  😐  😟  😊  😊  😊                                         │   │
│  │  ─────────────────────────────────────                              │   │
│  │                                                                      │   │
│  │  📊 Thống kê:                                                       │   │
│  │  • Vui vẻ: 5 lần (71%)                                            │   │
│  │  • Bình thường: 1 lần (14%)                                       │   │
│  │  • Buồn/nản: 1 lần (14%)                                          │   │
│  │                                                                      │   │
│  │  💡 Nhận xét:                                                      │   │
│  │  Con ổn định cảm xúc, ít biểu hiện lo lắng khi học.              │   │
│  │  Duy trì thói quen học tốt.                                        │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 2.2 Add/Edit Entry Modal

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  THÊM GHI CHÉP MỚI                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  📅 Ngày: [28/05/2026          ]                                             │
│  ⏱️ Giờ:  [09:30              ]                                             │
│                                                                             │
│  😊 Cảm xúc chính:                                                          │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐                                  │
│  │ 😊  │ │ 😐  │ │ 😟  │ │ 😰  │ │ 😎  │                                  │
│  │Vui  │ │Bình  │ │Nản   │ │Lo    │ │Tự hào│                                  │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘                                  │
│                                                                             │
│  📚 Bài tập liên quan (tùy chọn):                                          │
│  [Chọn bài tập...                                        ▼]                 │
│                                                                             │
│  📝 Mô tả chi tiết:                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐        │
│  │ Con rất hứng thú với bài tập...                                 │        │
│  │                                                                  │        │
│  └─────────────────────────────────────────────────────────────────┘        │
│                                                                             │
│  💡 Điều gì khiến con cảm thấy như vậy? (tùy chọn)                      │
│  ┌─────────────────────────────────────────────────────────────────┐        │
│  │ • Được khen ngợi                                                 │        │
│  │                                                                  │        │
│  └─────────────────────────────────────────────────────────────────┘        │
│                                                                             │
│  📋 Hành động tiếp theo (tùy chọn):                                       │
│  ┌─────────────────────────────────────────────────────────────────┐        │
│  │ • Tiếp tục với bài tập tiếp theo                                │        │
│  │                                                                  │        │
│  └─────────────────────────────────────────────────────────────────┘        │
│                                                                             │
│                              [Hủy]  [Lưu ghi chép]                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# PHẦN 3: EXERCISE SUGGESTIONS

## 3.1 Suggestion Engine Logic (Rule-based)

```typescript
interface SuggestionRules {
  // Rule 1: Nếu Concrete < 50%, gợi bài Concrete
  if (progress.concrete < 50) {
    suggest: 'concrete';
    reason: 'Con đang ở giai đoạn Concrete';
  }
  
  // Rule 2: Nếu Concrete 50-80%, gợi bài Concrete nâng cao
  if (progress.concrete >= 50 && progress.concrete < 80) {
    suggest: 'concrete_advanced';
    reason: 'Con đã vững cơ bản, nên củng cố thêm';
  }
  
  // Rule 3: Nếu Concrete >= 80% và Pictorial < 50%, gợi Pictorial
  if (progress.concrete >= 80 && progress.pictorial < 50) {
    suggest: 'pictorial';
    reason: 'Con đã sẵn sàng chuyển sang Pictorial';
  }
  
  // Rule 4: Nếu Pictorial >= 80% và Abstract < 30%, gợi Abstract
  if (progress.pictorial >= 80 && progress.abstract < 30) {
    suggest: 'abstract';
    reason: 'Con có thể bắt đầu thử Abstract';
  }
  
  // Rule 5: Nếu có emotion tiêu cực gần đây, gợi bài dễ hơn
  if (recentEmotions.includes('frustrated') || recentEmotions.includes('anxious')) {
    suggest: 'easy_exercise';
    reason: 'Con có vẻ gặp khó khăn, nên ôn lại bài cũ';
  }
  
  // Rule 6: Nếu streak cao, gợi bài thử thách hơn
  if (streak >= 7) {
    suggest: 'challenge_exercise';
    reason: 'Con đang có streak tốt, có thể thử thách thêm';
  }
}
```

---

# PHẦN 4: SETTINGS PAGE

## 4.1 Wireframe

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  CÀI ĐẶT                                                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [Tabs: Thông tin con | Tài khoản | Thông báo]                              │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  THÔNG TIN CON                                                       │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  👶 Avatar: [👦] [👧] [🧒] [👶]                                      │   │
│  │                                                                      │   │
│  │  Tên: [Minh                      ]                                   │   │
│  │  Ngày sinh: [15/03/2021          ]                                 │   │
│  │  Tuổi: 5 tuổi (tự tính)                                             │   │
│  │                                                                      │   │
│  │  Mức độ Dyscalculia:                                                │   │
│  │  ┌─────┐ ┌─────┐ ┌─────┐                                            │   │
│  │  │ Nhẹ │ │ TB  │ │Nặng │                                            │   │
│  │  └─────┘ │ ●   │ └─────┘                                            │   │
│  │          └─────┘                                                     │   │
│  │                                                                      │   │
│  │  Khó khăn cụ thể:                                                   │   │
│  │  ☑ Không hiểu đề bài                                               │   │
│  │  ☑ Tính toán chậm                                                  │   │
│  │  ☐ Sợ học toán (math anxiety)                                      │   │
│  │  ☑ Nhầm lẫn số (6 và 9)                                           │   │
│  │                                                                      │   │
│  │  Ghi chú:                                                           │   │
│  │  ┌─────────────────────────────────────────────────────────────┐     │   │
│  │  │ Minh là bé rất ngoan và chăm chỉ. Bé thích học khi được  │     │   │
│  │  │ khen ngợi. Cần kiên nhẫn, không ép bé quá sức.          │     │   │
│  │  └─────────────────────────────────────────────────────────────┘     │   │
│  │                                                                      │   │
│  │  [Lưu thay đổi]                                                     │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  ĐẶT LẠI DỮ LIỆU                                                   │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  ⚠️ Cảnh báo: Hành động này không thể hoàn tác!                   │   │
│  │                                                                      │   │
│  │  [Đặt lại tiến độ học tập]  (Xóa tất cả progress, giữ lại profile)│   │
│  │  [Xóa tất cả nhật ký]        (Xóa tất cả emotion entries)         │   │
│  │  [Xóa tài khoản]             (Xóa vĩnh viễn tài khoản)            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# PHẦN 5: METHODS PAGE (CPA Guide)

## 5.1 Wireframe

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  HƯỚNG DẪN PHƯƠNG PHÁP CPA                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  🎓 PHƯƠNG PHÁP CPA CHO DYSCALCULIA                                 │   │
│  │                                                                      │   │
│  │  "Con bạn không "dốt" toán. Con chỉ cần cách học phù hợp."         │   │
│  │                                                                      │   │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐                       │   │
│  │  │ CONCRETE │→│ PICTORIAL │→│ ABSTRACT  │                       │   │
│  │  │  Cụ thể  │  │ Hình ảnh  │  │ Trừu tượng│                       │   │
│  │  │  🔵 75%  │  │  🟡 40%  │  │  🟣 15%  │                       │   │
│  │  └───────────┘  └───────────┘  └───────────┘                       │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  🔵 CONCRETE (CỤ THỂ)                               [Xem chi tiết →]│   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  Trẻ học toán bằng cách cầm nắm, di chuyển đồ vật thật.          │   │
│  │                                                                      │   │
│  │  ✅ Khi nào con cần ở giai đoạn này:                               │   │
│  │     • Con mới bắt đầu học toán                                    │   │
│  │     • Con không hiểu số là gì                                       │   │
│  │     • Con cần "thấy" và "chạm" để hiểu                            │   │
│  │                                                                      │   │
│  │  💡 Hoạt động đề xuất:                                             │   │
│  │     • Xếp khối gỗ theo số lượng                                  │   │
│  │     • Đếm kẹo trong lọ                                             │   │
│  │     • Phân loại đồ chơi theo màu sắc và đếm                        │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  🟡 PICTORIAL (HÌNH ẢNH)                            [Xem chi tiết →]│   │
│  │  ...                                                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  🟣 ABSTRACT (TRỪU TƯỢNG)                          [Xem chi tiết →]│   │
│  │  ...                                                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  💡 MẸO CHO PHỤ HUYNH                                              │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  1. Kiên nhẫn - Con cần thời gian để tiếp thu                     │   │
│  │  2. Không so sánh con với trẻ khác                                  │   │
│  │  3. Thực hành mỗi ngày 10-15 phút                                 │   │
│  │  4. Biến học thành trò chơi, không phải bài tập                   │   │
│  │  5. Khen ngợi nỗ lực, không chỉ kết quả                            │   │
│  │  6. Quay lại bước trước nếu con gặp khó khăn                       │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# PHẦN 6: LANDING PAGE MỚI

## 6.1 Wireframe

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [MathMate Support Logo]           [Đăng nhập]  [Bắt đầu miễn phí]         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                                                                      │   │
│  │    "Con bạn không "dốt" toán.                                      │   │
│  │     Con chỉ cần cách học phù hợp."                                 │   │
│  │                                                                      │   │
│  │    MathMate Support giúp phụ huynh có con gặp khó khăn với          │   │
│  │    toán (Dyscalculia) tìm ra phương pháp học đúng đắn.            │   │
│  │                                                                      │   │
│  │    [Bắt đầu miễn phí]        [Tìm hiểu thêm]                     │   │
│  │                                                                      │   │
│  │    ✓ Miễn phí  ✓ Dành cho trẻ 4-6 tuổi  ✓ 100% Tiếng Việt        │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  HIỂU VỀ DYSCALCULIA                                               │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  Dyscalculia là gì?                                                 │   │
│  │                                                                      │   │
│  │  Đây là rối loạn học toán khiến trẻ gặp khó khăn với:              │   │
│  │  • Hiểu ý nghĩa của số lượng                                       │   │
│  │  • Nhớ các con số                                                  │   │
│  │  • Thực hiện phép tính                                             │   │
│  │  • Hiểu các khái niệm toán học                                     │   │
│  │                                                                      │   │
│  │  💡 Điều quan trọng:                                                │   │
│  │  Trẻ Dyscalculia không phải "kém thông minh". Não bộ của           │   │
│  │  các em xử lý thông tin số khác với người bình thường.            │   │
│  │  Với phương pháp đúng, trẻ HOÀN TOÀN có thể học tốt toán!         │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  PHƯƠNG PHÁP CPA - TOÁN SINGAPORE                                  │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐                       │   │
│  │  │ CONCRETE │→│ PICTORIAL │→│ ABSTRACT  │                       │   │
│  │  │           │  │           │  │           │                       │   │
│  │  │  🧱🧱🧱   │  │  ⭕⭕⭕   │  │    3     │                       │   │
│  │  │           │  │           │  │           │                       │   │
│  │  │  3 khối   │  │  3 hình   │  │  Số 3    │                       │   │
│  │  │   gỗ      │  │  tròn     │  │           │                       │   │
│  │  │           │  │           │  │           │                       │   │
│  │  │ (Cầm nắm) │  │ (Vẽ hình) │  │ (Số thuần)│                       │   │
│  │  └───────────┘  └───────────┘  └───────────┘                       │   │
│  │                                                                      │   │
│  │  Tại sao CPA hiệu quả với Dyscalculia?                             │   │
│  │                                                                      │   │
│  │  Não bộ của trẻ Dyscalculia gặp khó khăn ở bước Abstract.          │   │
│  │  CPA giúp trẻ xây dựng nền tảng vững chắc từ Concrete →          │   │
│  │  Pictorial trước khi chuyển sang Abstract.                          │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  TÍNH NĂNG CHÍNH                                                   │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │   │
│  │  │  📊 Theo dõi     │  │  📚 Bài tập      │  │  💡 Gợi ý       │  │   │
│  │  │  tiến độ CPA    │  │  theo phương     │  │  bài tập phù    │  │   │
│  │  │                  │  │  pháp CPA        │  │  hợp            │  │   │
│  │  │  Xem chi tiết    │  │                  │  │                  │  │   │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────┘  │   │
│  │                                                                      │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │   │
│  │  │  💭 Nhật ký    │  │  📖 Hướng dẫn   │  │  😊 Theo dõi    │  │   │
│  │  │  cảm xúc con    │  │  phương pháp    │  │  cảm xúc con    │  │   │
│  │  │                  │  │  dạy con        │  │  khi học        │  │   │
│  │  │                  │  │                  │  │                  │  │   │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────┘  │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  BẮT ĐẦU NGAY HÔM NAY                                              │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │                                                                      │   │
│  │  [Bắt đầu miễn phí]                                               │   │
│  │                                                                      │   │
│  │  Không cần thẻ tín dụng • Không cần cài đặt phức tạp • Miễn phí 100%│   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# PHẦN 7: CẤU TRÚC FILES CUỐI CÙNG

## 7.1 File Structure

```
src/
├── api/
│   ├── mockData/
│   │   ├── index.ts
│   │   ├── exercises.ts          (15+ exercises)
│   │   ├── progress.ts
│   │   ├── emotions.ts
│   │   └── child.ts
│   │
│   └── services/
│       ├── authService.ts         (Mock)
│       ├── childService.ts        (Mock)
│       ├── exerciseService.ts
│       ├── progressService.ts
│       ├── emotionService.ts
│       └── dashboardService.ts    (Mock - tổng hợp)
│
├── features/
│   ├── landing/
│   │   ├── LandingPage.tsx
│   │   ├── HeroSection.tsx       (Updated)
│   │   ├── DyscalculiaSection.tsx (NEW)
│   │   ├── CPAMethodSection.tsx   (NEW)
│   │   └── FeaturesSection.tsx    (NEW)
│   │
│   ├── parent/
│   │   ├── dashboard/
│   │   │   ├── DashboardPage.tsx  (Redesigned)
│   │   │   ├── WelcomeBanner.tsx   (NEW)
│   │   │   ├── CPAGauge.tsx       (NEW)
│   │   │   ├── MathTypeProgress.tsx (NEW)
│   │   │   ├── StatsOverview.tsx   (NEW)
│   │   │   ├── SuggestedExercise.tsx (NEW)
│   │   │   ├── EmotionTracker.tsx  (NEW)
│   │   │   ├── ActivityFeed.tsx   (NEW)
│   │   │   └── QuickActions.tsx   (NEW)
│   │   │
│   │   ├── exercises/
│   │   │   ├── ExerciseLibraryPage.tsx
│   │   │   ├── ExerciseCard.tsx
│   │   │   └── ExerciseDetail.tsx (NEW)
│   │   │
│   │   ├── journal/
│   │   │   ├── JournalPage.tsx    (NEW)
│   │   │   ├── EmotionEntry.tsx
│   │   │   └── AddEntryModal.tsx
│   │   │
│   │   ├── methods/
│   │   │   ├── MethodsPage.tsx    (NEW)
│   │   │   ├── CPASection.tsx
│   │   │   └── TipsSection.tsx
│   │   │
│   │   └── settings/
│   │       ├── SettingsPage.tsx   (Redesigned)
│   │       └── ChildSettingsTab.tsx
│   │
│   └── auth/
│       ├── AuthPage.tsx
│       └── ParentLoginForm.tsx
│
├── pages/
│   ├── public/
│   │   ├── LandingPage.tsx
│   │   ├── AuthPage.tsx
│   │   └── OnboardingPage.tsx
│   │
│   └── parent/
│       ├── DashboardPage.tsx
│       ├── ExerciseLibraryPage.tsx
│       ├── JournalPage.tsx        (NEW)
│       ├── MethodsPage.tsx        (NEW)
│       └── SettingsPage.tsx
│
├── providers/
│   ├── AuthProvider.tsx           (Parent only)
│   └── ChildProvider.tsx          (Single child)
│
├── routes/
│   ├── AppRouter.tsx
│   └── parentRoutes.tsx
│
└── types/
    ├── exercise.types.ts
    ├── progress.types.ts
    ├── emotion.types.ts
    └── child.types.ts
```

---

# PHẦN 8: LOCALSTORAGE SCHEMA

```typescript
// LocalStorage Keys
const STORAGE_KEYS = {
  AUTH_USER: 'mathmate_auth_user',
  CHILD_PROFILE: 'mathmate_child',
  CPA_PROGRESS: 'mathmate_cpa_progress',
  EMOTION_ENTRIES: 'mathmate_emotions',
  SESSION_HISTORY: 'mathmate_sessions',
  SETTINGS: 'mathmate_settings',
};

// Data Shapes
interface StoredData {
  child: {
    id: string;
    name: string;
    dateOfBirth: string;
    dyscalculiaLevel: 'nhẹ' | 'trung bình' | 'nặng';
    dyscalculiaTypes: string[];
    notes: string;
    createdAt: string;
  };
  
  cpaProgress: {
    concrete: number;
    pictorial: number;
    abstract: number;
    lastUpdated: string;
  };
  
  mathProgress: {
    counting: { current: number; target: number };
    comparison: { current: number; target: number };
    addition: { current: number; target: number };
    subtraction: { current: number; target: number };
  };
  
  emotions: EmotionEntry[];
  
  sessions: {
    id: string;
    date: string;
    exerciseId: string;
    duration: number;
    correctCount: number;
    incorrectCount: number;
  }[];
  
  streak: {
    current: number;
    lastSessionDate: string;
  };
  
  settings: {
    notifications: boolean;
    reminderTime: string;
    theme: 'light' | 'dark';
  };
}
```

---

# PHẦN 9: PRIORITY IMPLEMENTATION

## 9.1 Phase 1: Dashboard (Tuần này)

| Component | Effort | Priority |
|-----------|--------|----------|
| WelcomeBanner | 2h | P0 |
| CPAGauge | 3h | P0 |
| StatsOverview | 2h | P0 |
| SuggestedExercise | 3h | P0 |
| QuickActions | 1h | P1 |
| ActivityFeed | 3h | P1 |
| EmotionTracker | 2h | P1 |
| MathTypeProgress | 2h | P2 |

## 9.2 Phase 2: Journal (Tuần sau)

| Component | Effort | Priority |
|-----------|--------|----------|
| JournalPage | 4h | P0 |
| AddEntryModal | 3h | P0 |
| EmotionChart | 2h | P1 |
| EntryList | 2h | P1 |

## 9.3 Phase 3: Exercise (Tuần 3)

| Component | Effort | Priority |
|-----------|--------|----------|
| ExerciseDetail | 4h | P0 |
| ExerciseSuggestion | 3h | P0 |
| ExerciseFilter | 2h | P1 |

## 9.4 Phase 4: Landing + Methods (Tuần 4)

| Component | Effort | Priority |
|-----------|--------|----------|
| LandingPage rewrite | 6h | P1 |
| MethodsPage | 4h | P1 |

---

# PHỤ LỤC: MOCK DATA MẪU

## A. Child Profile
```json
{
  "id": "child-001",
  "name": "Minh",
  "dateOfBirth": "2021-03-15",
  "age": 5,
  "dyscalculiaLevel": "trung bình",
  "dyscalculiaTypes": [
    "Khó khăn trong việc hiểu ý nghĩa của số lượng",
    "Nhầm lẫn các số có hình dạng tương tự (6 và 9)",
    "Cần thời gian lâu hơn để xử lý phép tính"
  ],
  "notes": "Minh là bé rất ngoan và chăm chỉ. Bé thích học khi được khen ngợi."
}
```

## B. CPA Progress
```json
{
  "concrete": 75,
  "pictorial": 40,
  "abstract": 15,
  "lastUpdated": "2026-05-28"
}
```

## C. Math Progress
```json
{
  "counting": { "current": 8, "target": 10 },
  "comparison": { "current": 5, "target": 10 },
  "addition": { "current": 3, "target": 10 },
  "subtraction": { "current": 1, "target": 10 }
}
```

---

**Cập nhật lần cuối: 29/05/2026**
