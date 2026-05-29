# MathMate Support — UI Optimization Plan

> Tổng hợp phân tích giao diện hiện tại và đề xuất tối ưu.

---

## 1. KIẾN TRÚC HIỆN TẠI

| Tầng | Công nghệ | Ghi chú |
|---|---|---|
| Framework | React 19 + TypeScript 5.9 | CSR (Client-side rendering) |
| Build tool | Vite 7 | Không có bundle analyzer |
| Routing | React Router v7 (lazy-loaded) | Route có điều kiện gây remount |
| Styling | Tailwind CSS v4 + PostCSS + CSS custom properties | **Có xung đột TW3/TW4 config** |
| State | React Query v5, React Context | Không dùng Zustand/Jotai |
| UI Icons | Lucide React | Nhất quán |
| Animation | CSS keyframes + Framer Motion | Three.js, R3F bundle nặng |
| Forms | React Hook Form + Zod | Chỉ dùng 1 phần |
| Charts | Recharts | Bundle nặng, chỉ dùng ở dashboard |
| i18n | i18next + custom runtime translation | Có anti-pattern về hiệu năng |
| 3D | @react-three/fiber + @react-three/drei | Chỉ dùng ở landing (hero?) |

---

## 2. CẤU TRÚC THƯ MỤC & ROUTE

```
src/
├── api/            # API service + mock data
├── components/
│   ├── ui/         # Button, Card, Input, Modal, Badge, Loading, EmptyState, Calendar, PasswordInput
│   ├── common/     # ErrorBoundary, ChildSelector, RouteSuspense
│   └── layout/     # Sidebar, ParentLayout (unused), ChildLayout (empty)
├── features/
│   ├── landing/    # 8 section components
│   └── parents/
│       ├── dashboard/  # 8 dashboard widgets
│       └── settings/   # 3 tab components
├── pages/
│   ├── public/     # LandingPage, AuthPage, OnboardingPage, ChildLoginPage (unrouted), NotFoundPage (unused)
│   └── parents/    # Dashboard, ExerciseLibrary, Journal, Methods, Settings
├── providers/      # AuthProvider, ChildProvider
├── routes/         # AppRouter (main), parentRoutes (unused), childRoutes (empty)
├── styles/         # index.css (partial — duplicates index.css)
├── hooks/          # (chưa xem)
├── i18n/           # resources.ts + runtime.ts (DOM-tree-walker)
└── types/          # TypeScript types
```

| Route | Component | Trạng thái |
|---|---|---|
| `/` | LandingPage | OK |
| `/login` | AuthPage (login mode) | OK |
| `/register` | AuthPage (register mode) | OK |
| `/onboarding` | OnboardingPage | OK |
| `/parent/dashboard` | DashboardPage | OK |
| `/parent/exercises` | ExerciseLibraryPage | OK |
| `/parent/journal` | JournalPage | OK |
| `/parent/methods` | MethodsPage | OK |
| `/parent/settings` | SettingsPage | OK |
| `/child-login` | ChildLoginPage | **Chưa được route** |
| `*` (404) | Redirect | **NotFoundPage không dùng** |

---

## 3. PHÁT HIỆN VẤN ĐỀ (ISSUES)

### 3.1. CRITICAL — Cần sửa ngay

#### C1. Xung đột Tailwind v3/v4
- `src/index.css` import `@import "tailwindcss"` và định nghĩa toàn bộ utility class thủ công
- `src/styles/index.css` cũng import `@import "tailwindcss"` và khai báo `@theme`
- `tailwind.config.js` được giữ lại nhưng Tailwind v4 + PostCSS plugin **không dùng file config này** → toàn bộ token (colors, shadows, gradients, animations) **có thể không hoạt động**
- Cả 2 file CSS đều được import trong `main.tsx` → **duplicate CSS rules**

#### C2. Anti-pattern hiệu năng CSS toàn cục
```css
/* src/index.css:50-54 — Áp dụng transition cho MỌI element */
* {
  transition-property: background-color, border-color, color, fill, stroke, opacity, box-shadow, transform;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
  transition-duration: 150ms;
}
```
→ Gây chậm render, đặc biệt trên mobile. Chỉ nên áp dụng cho interactive elements.

#### C3. Auth-driven conditional routing gây remount
```tsx
// AppRouter.tsx:47
{isParentAuthenticated && (
  <Route path="/parent" element={<ParentLayout />}>
    {/* all parent routes */}
  </Route>
)}
```
→ Khi auth state thay đổi, toàn bộ cây route parent bị hủy và tạo lại. Nên dùng `<Navigate>` hoặc protected route wrapper.

#### C4. Sidebar dùng `window.location.href` thay vì React Router
```tsx
// Sidebar.tsx:88-91
onClick={async () => {
  await logout();
  window.location.href = '/';  // full page reload
}}
```
→ Gây reload toàn trang, mất state client.

#### C5. i18n runtime DOM-tree-walker
```ts
// runtime.ts:162-192
export const localizeDomSubtree = (root: Node) => {
  // Duyệt toàn bộ DOM tree bằng TreeWalker để dịch text nodes
};
```
→ Cực kỳ tốn CPU vì duyệt DOM synchronous. Nên sử dụng `useTranslation()` hook và `<Trans>` component của react-i18next.

### 3.2. HIGH — Nên sửa sớm

#### H1. Không có responsive design cho mobile
- Sidebar cố định `w-20` (80px), không có hamburger menu
- Dashboard grid không collapse trên mobile (vẫn `lg:grid-cols-2`)
- Exercise grid dùng `grid-cols-1 md:grid-cols-2 lg:grid-cols-3` nhưng không có mobile breakpoint tối ưu
- AuthPage có `md:flex-row` nhưng mobile mode chưa kiểm tra kỹ

#### H2. Thiếu lazy loading cho thư viện nặng
- `three.js` (~150KB gzip), `@react-three/fiber`, `@react-three/drei` luôn được bundle
- `recharts` (~120KB gzip) luôn được bundle
- `framer-motion` (~30KB gzip) luôn được bundle
- Không có dynamic import cho components nặng (chỉ lazy route)

#### H3. Modal ESC handler stale closure
```tsx
// Modal.tsx:28-37
useEffect(() => {
  const handleEscape = (e: KeyboardEvent) => {
    if (e.key === 'Escape' && isOpen) onClose();
  };
  document.addEventListener('keydown', handleEscape);
  return () => document.removeEventListener('keydown', handleEscape);
}, [isOpen, onClose]); // onClose có thể thay đổi → cần memo từ parent
```

#### H4. Input component xung đột icon phải
- Khi vừa có `type="password"` vừa có custom `icon` bên phải, logic ưu tiên hiển thị chưa rõ ràng
- Password toggle của Input và PasswordInput bị trùng lặp

#### H5. Button component dùng gradient inline thay vì token
```tsx
// Button.tsx:31 — Hardcode gradient thay vì dùng bg-gradient-primary từ config
'bg-gradient-to-r from-blue-500 to-purple-600'
```
→ Không nhất quán với design token.

#### H6. Title app sai
`index.html` title = "KiddyMate", nhưng app hiển thị "MathMate Support". Đây là 2 tên khác nhau.

### 3.3. MEDIUM — Cần cải thiện

#### M1. Thiếu skeleton loading
Tất cả trang đều dùng full-page spinner (`Loading fullScreen`) thay vì skeleton loading cục bộ cho từng widget.
→ `SkeletonCard`, `SkeletonTable`, `SkeletonChart` đã được export nhưng **không được dùng** ở đâu.

#### M2. Không có React.memo / useMemo
Không component nào được memo hóa. Dashboard render lại toàn bộ widget dù chỉ 1 widget thay đổi.

#### M3. Inconsistent spacing & padding
| Vị trí | Padding |
|---|---|
| DashboardPage | `px-4 sm:px-6 lg:px-8 py-6 space-y-6` |
| ExerciseLibraryPage | `p-6 max-w-7xl mx-auto` |
| SettingsPage | `p-4 md:p-6 lg:p-8` |
| AuthPage | `p-8 md:p-12` |
| Card component | `p-4 / p-6 / p-8` tùy variant |

→ Không có layout container duy nhất, mỗi page tự set padding riêng.

#### M4. Accessibility chưa đầy đủ
- Thiếu `aria-label`, `aria-describedby`, `role` trên interactive elements
- Modal không có focus trap
- Không có skip-to-content link
- Calendar component (`react-day-picker`) chưa kiểm tra keyboard navigation

#### M5. Trạng thái lỗi chưa nhất quán
- `ErrorBoundary` dùng tiếng Việt
- `EmptyState` và `NoResultsFound` hardcode tiếng Anh
- ExerciseLibraryPage dùng emoji 🔍 làm empty state, thay vì dùng component EmptyState có sẵn

#### M6. Validation form trùng lặp
AuthPage.tsx viết validation thủ công cho login/register, thay vì dùng Zod schema (đã có trong dependency).

### 3.4. LOW — Nice to have

#### L1. Không có dark mode
Toàn bộ màu nền là hardcode (bg-white, bg-gray-50, bg-slate-900). Chưa có CSS variable cho theme.

#### L2. Không có breadcrumbs
Parent routes không có breadcrumb navigation.

#### L3. Thiếu page transitions
Không có animation chuyển trang khi navigate.

#### L4. Không có PWA / offline support
Không có service worker, không có manifest.json.

#### L5. Không có confirmation dialog
Destructive actions (xóa, reset data) không có confirm dialog riêng.

#### L6. Không có virtual scrolling
Exercise list không dùng virtual scroll, dù có thể có 50+ items.

#### L7. Bundle chưa được phân tích
Không có `rollup-plugin-visualizer` hoặc `vite-bundle-analyzer`.

---

## 4. KẾ HOẠCH TỐI ƯU (ROADMAP)

### Phase 1: Sửa lỗi nền tảng (1-2 ngày)

| # | Hạng mục | Ước lượng | Mô tả |
|---|---|---|---|
| 1.1 | **Fix Tailwind config** | 2h | Chọn 1 hướng: hoặc dùng TW4 CSS-first config (gỡ bỏ `tailwind.config.js`), hoặc downgrade về TW3. Hợp nhất 2 file CSS về 1 file duy nhất. |
| 1.2 | **Fix CSS performance** | 30m | Gỡ `*` universal transition, chỉ áp dụng cho `.transition-element` class. |
| 1.3 | **Fix conditional routing** | 1h | Dùng `<ProtectedRoute>` wrapper thay vì `{isAuth && <Route>}`. |
| 1.4 | **Fix sidebar navigation** | 30m | Dùng `useNavigate()` thay vì `window.location.href`. |
| 1.5 | **Fix i18n performance** | 2h | Gỡ `localizeDomSubtree`, chuyển hoàn toàn sang `useTranslation()` + `<Trans>`. |

### Phase 2: Responsive & Mobile (1-2 ngày)

| # | Hạng mục | Ước lượng | Mô tả |
|---|---|---|---|
| 2.1 | **Sidebar responsive** | 3h | Mobile: collapse thành hamburger menu. Tablet: thu nhỏ thành icon-only (đã có sẵn). |
| 2.2 | **Dashboard grid responsive** | 2h | `grid-cols-1` trên mobile, `grid-cols-2` trên tablet, layout hiện tại trên desktop. |
| 2.3 | **AuthPage mobile** | 2h | Stack form thay vì slide overlay trên mobile. |
| 2.4 | **ExerciseLibrary filters mobile** | 1h | Collapsible filter panel trên mobile. |
| 2.5 | **Settings tabs mobile** | 1h | Scrollable horizontal tabs trên mobile. |

### Phase 3: Hiệu năng (1 ngày)

| # | Hạng mục | Ước lượng | Mô tả |
|---|---|---|---|
| 3.1 | **Lazy-load thư viện nặng** | 2h | `React.lazy` + `Suspense` cho Three.js, Recharts, Framer Motion components. |
| 3.2 | **React.memo cho widgets** | 1h | Memo hóa các dashboard widget component. |
| 3.3 | **Skeleton loading** | 1h | Thay full-page spinner bằng skeleton cho từng widget. Dùng lại `SkeletonCard`, `SkeletonTable`, `SkeletonChart` đã có. |
| 3.4 | **Bundle analysis** | 30m | Cài `rollup-plugin-visualizer`, phân tích bundle size. |
| 3.5 | **Code splitting routes** | 1h | Đã có sẵn, kiểm tra lại lazy loading hoạt động đúng. |

### Phase 4: Design System & Consistency (2 ngày)

| # | Hạng mục | Ước lượng | Mô tả |
|---|---|---|---|
| 4.1 | **Thống nhất spacing** | 2h | Tạo `<PageContainer>` component với padding chuẩn. |
| 4.2 | **Fix Button gradient** | 1h | Dùng CSS variable thay vì hardcode gradient. |
| 4.3 | **Fix EmptyState usage** | 1h | Thay emoji/div thủ công bằng `EmptyState` / `NoResultsFound` component có sẵn. |
| 4.4 | **Zod validation** | 2h | Thay validation thủ công trong AuthPage bằng Zod schema. |
| 4.5 | **Fix title app** | 10m | Đổi `index.html` title thành "MathMate Support". |

### Phase 5: Accessibility (1 ngày)

| # | Hạng mục | Ước lượng | Mô tả |
|---|---|---|---|
| 5.1 | **Modal focus trap** | 1h | Thêm focus trap + `aria-modal`. |
| 5.2 | **Skip-to-content** | 30m | Thêm skip link ẩn. |
| 5.3 | **aria labels** | 2h | Thêm `aria-label` cho icon-only buttons, `aria-describedby` cho error messages. |
| 5.4 | **Keyboard navigation** | 1h | Kiểm tra tab order trong sidebar, forms, modals. |

### Phase 6: UX Enhancements (1-2 ngày)

| # | Hạng mục | Ước lượng | Mô tả |
|---|---|---|---|
| 6.1 | **Dark mode** | 4h | CSS variables cho light/dark theme + Tailwind `dark:` variant. |
| 6.2 | **Confirmation dialog** | 1h | Tạo `<ConfirmDialog>` component dựa trên Modal có sẵn. |
| 6.3 | **Page transitions** | 1h | Framer Motion `AnimatePresence` cho route transitions. |
| 6.4 | **Breadcrumbs** | 1h | Auto breadcrumb dựa trên route path. |
| 6.5 | **Toast consistency** | 30m | Định nghĩa các toast helper function (success/error/loading). |

---

## 5. TỔNG HỢP: THỐNG KÊ COMPONENT

### UI Components (`src/components/ui/`)
| Component | Lines | Variants | Đánh giá |
|---|---|---|---|
| Button | 70 | 7 variants × 3 sizes | Có ripple effect, loading state, icon slot. Cần fix gradient token. |
| Card | 82 | 4 variants × 4 paddings | Có hover lift/shine. Dùng tốt. |
| Input | 147 | ForwardedRef | Có password toggle, error/success icons, left/right icon slots. Xung đột logic right icons. |
| Modal | 115 | 5 sizes | ESC close, backdrop blur, scroll lock. Thiếu focus trap. |
| Loading | 92 | 3 sizes + skeleton exports | Skeleton components (Card/Table/Chart) đã viết sẵn nhưng không dùng. |
| EmptyState | 136 | 8 icon presets + custom | Có specialized exports (NoResultsFound, NoTasksYet, NoRewardsYet). Không được dùng ở nhiều nơi. |
| Badge | 78 | 6 variants × 3 sizes | Có dot + pulse animation. |
| Calendar | ? | react-day-picker v9 wrapper | OK. |
| PasswordInput | 116 | ForwardedRef | Password strength meter 5 levels. Trùng code show/hide với Input. |

### Layout Components
| Component | Lines | Đánh giá |
|---|---|---|
| Sidebar | 110 | 80px fixed, icon-only, tooltip. Thiếu responsive. Dùng window.location. |
| ParentLayout | ~15 | **Không dùng** — AppRouter có layout inline. |
| ChildLayout | ~5 | **Trống** — chưa implement. |

---

## 6. BREAKING CHANGES CẢNH BÁO

1. **Gỡ `tailwind.config.js`** → tất cả class dùng `bg-primary-500`, `text-accent`, `shadow-soft`, `animate-slide-up`, `bg-gradient-primary`... sẽ không hoạt động nếu chưa migrate sang CSS-first config của TW4.

2. **Sửa conditional routing** → Cần test kỹ auth flow (login → redirect, refresh page vẫn giữ route).

3. **Gỡ `localizeDomSubtree`** → Tất cả string chưa được wrap trong `t()` hoặc `<Trans>` sẽ hiển thị tiếng Anh. Cần audit toàn bộ codebase.

---

## 7. KHUYẾN NGHỊ THÊM

- Cài `@tailwindcss/typography` và `@tailwindcss/forms` plugin cho form chuẩn.
- Cân nhắc dùng `zustand` cho global state thay vì React Context (tránh re-render không cần thiết).
- Cài `eslint-plugin-jsx-a11y` để tự động phát hiện accessibility issues.
- Cài `@vitejs/plugin-legacy` nếu cần hỗ trợ trình duyệt cũ.
- Thêm `vite-plugin-compression` để gzip/brotli bundle.
