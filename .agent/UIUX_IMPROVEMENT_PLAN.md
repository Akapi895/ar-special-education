# UI/UX Improvement Plan — AR Math Learning App

> **Ngày**: 29/05/2026  
> **Mục tiêu**: Nâng cao trải nghiệm người dùng cho trẻ em 5-7 tuổi trên cả Editor + iOS  
> **Phạm vi**: 4 activities (QuantityMatch, CompareQuantity, NumberBonds, NumberLineJump) + app shell

---

## Tổng quan phát hiện

| Category | Issues found | Severity |
|---|---|---|
| Code duplication | 700+ dòng helper methods lặp lại | 🟡 Medium |
| Hardcoded strings | ~56+ strings tiếng Việt không qua localization | 🟡 Medium |
| Inconsistent patterns | Feedback colors, button labels, next-round logic | 🟡 Medium |
| Dead/broken code | UIHintBubble, UIScreenManager, BuildRuntimeUi NumberBonds | 🔴 High |
| Accessibility | No color-blind palette, no TTS, no undo | 🟡 Medium |
| Navigation UX | No transitions, no confirmation dialogs, no loading screen | 🟡 Medium |
| Feedback UX | Auto-hide too fast (1.2-1.8s), inconsistent animations | 🟡 Medium |
| Missing features | Confetti, audio feedback, animated zones, haptic | 🟡 Medium |

---

## 1. CRITICAL FIXES (Blockers)

### 1.1 NumberBonds `BuildRuntimeUi` is missing — NPE at runtime
**File**: `NumberBondsRuntimeUI.cs`

`NumberBondsRuntimeUI.Awake()` calls `view.BuildRuntimeUi(CreateCanvas(transform))` if `!view.HasUiReferences`. But the `BuildRuntimeUi` method is NOT implemented in any partial file of `NumberBondsView` (`NumberBondsView.cs:467 lines`, `NumberBondsView.Visuals.cs:283 lines`, `NumberBondsView.RuntimeUI.cs:201 lines`). None contain `BuildRuntimeUi`.

**Impact**: Crash (NullReferenceException) when `NumberBondsView` is used without prefab-referenced serialized fields.

**Fix**: Either:
- Implement `BuildRuntimeUi(Canvas)` in `NumberBondsView.RuntimeUI.cs`, OR
- Override `HasUiReferences` in `NumberBondsView` to always return true (since number bonds requires AR zones, runtime canvas UI can't replace 3D elements)

---

### 1.2 Portrait/Landscape canvas mismatch
| Activity | Reference resolution |
|---|---|
| QuantityMatch | 1080x1920 (portrait phone) |
| CompareQuantity | 1920x1080 (landscape tablet) |
| NumberBonds | 1920x1080 (landscape tablet) |
| NumberLineJump | 1920x1080 (landscape tablet) |

All four activities are expected to run in the same app on the same device. This mismatch causes:
- QuantityMatch UI renders at wrong aspect ratio on landscape tablet
- Or other 3 activities render wrong on portrait phone

**Fix**: Unify to 1920x1080 (landscape) for all 4. AR apps typically run landscape for wider field of view. Update `QuantityMatchRuntimeUI.CreateCanvas` from `1080x1920` to `1920x1080`.

---

### 1.3 `UpdateButtonLabels` discards config labels — design conflict
**File**: `CompareQuantityView.cs:322-325`

Method receives 3 label string params from config but only calls `ApplyComparisonAnswerButtonVisuals()` which hardcodes `>`, `<`, `=` symbols.

**Clarified with user (29/05/2026)**: Symbols are intentional — purpose is teaching mathematical comparison signs. The method signature is misleading.

**Fix**: Rename `UpdateButtonLabels` to `RefreshAnswerButtonVisuals` (it doesn't use any labels). Or remove the method entirely and call `ApplyComparisonAnswerButtonVisuals` directly. Also remove `NormalizeComparisonButtonLabel` which converts Vietnamese labels back to English words that are then overridden anyway.

---

### 1.4 QuantityMatch increment/decrement buttons — permanently hidden dead UI
**File**: `QuantityMatchView.cs`

`decrementNumberButton` and `incrementNumberButton` are created in `CreateFriendlyNumberInputControls()` with full styling, colliders, and interaction — then immediately hidden via `gameObject.SetActive(false)`.

**Fix**: Remove the field declarations, construction code, and SetActive lines. Dead code = confusion for future maintainers.

---

## 2. CODE DEDUPLICATION (Maintenance)

### 2.1 Extract shared UI builder methods into `UIKidFriendlyStyle` or new `UIActivityLayoutBuilder`

Each View has ~700 lines of near-identical helper methods. Example duplication:

| Method | In View | Lines | Difference |
|---|---|---|---|
| `CreateUiPanel` | All 4 | ~15 each | Identical |
| `CreateTopText` | All 4 | ~20 each | Identical |
| `CreateButton` | All 4 | ~20 each | Color default differs (0.2,0.5,0.9 vs named color) |
| `CreateSubPanel` | All 4 | ~15 each | Identical |
| `CreatePanelText` | All 4 | ~20 each | Identical |
| `CreateButtonLabel` | All 4 | ~15 each | Identical |
| `CreateTopRightButton` | CompareQ, NumberLine | ~20 each | Identical |
| `CreateTopLeftButton` | CompareQ, NumberLine | ~20 each | Identical |
| `LoadSceneIfAvailable` | CompareQ, NumberLine | ~10 each | Identical |
| `NormalizeTopNavigationButtons` | All 4 | ~3 each | All empty bodies |
| `ShakeUiCoroutine` | QuantityMatch | ~30 | Unique to QM |
| **Total duplication** | | **~300-400 lines** | |

**Proposal**: Create `Core/UI/Layout/UIActivityLayoutBuilder` static class with all shared methods. Each View reduces to:
```csharp
public void BuildRuntimeUi(Canvas canvas) {
    UIActivityLayoutBuilder.BuildCommonShell(canvas, this, out var panel);
    // Activity-specific elements only
    CreateComparisonButtons(panel); // ~30 lines instead of ~700
}
```

**Effort**: Medium (needs careful extraction but purely mechanical)
**Risk**: Low (shared method with identical logic)

---

### 2.2 Unify feedback panel positioning formula

Each activity hardcodes feedback/hint panel positions as pixel values:
- `RuntimeFeedbackPanelBottomY = 450f` (QM) vs `= 296f` (CQ) vs `RuntimeFeedbackPanelCenter = new Vector2(0, -270)` (CQ)

**Proposal**: Define feedback positions relative to canvas height in `UIDesignTokens`:
```csharp
public static readonly Vector2 FeedbackPanelAnchor = new Vector2(0.5f, 0.15f); // 15% from bottom
```

---

## 3. HARDCODED STRINGS — Localization

### 3.1 Impact summary

| View | Unlocalized strings | Examples |
|---|---|---|
| QuantityMatchView | ~16 | "Chính xác! Con giỏi quá!", "Nhom X", "Tiep tuc" |
| CompareQuantityView | ~6 | "Ben trai", "Ben trai > < = ben phai?" |
| NumberBondsView | ~7 | "Tong", "Phan A", "Tim phan con thieu cua X" |
| NumberLineJumpView | ~8 | "Dang o: X", "Bat dau o X", summary template |
| MainMenu/ActivitySelect/Progress | ~19 | "Chon bai hoc", "Tien do hoc tap" |
| **Total** | **~56+** | |

### 3.2 Approach

**Step 1**: Add missing keys to `SimpleLocalization.cs` dictionary (1-2 hours). Keys to add:
```
quantity_select_group, quantity_enter_number, quantity_digit_clear, quantity_digit_submit
compare_left_side, compare_right_side, compare_question_text
numberbond_whole, numberbond_part_a, numberbond_part_b
numberline_current_position, numberline_starting_at, numberline_boundary_hit
feedback_excellent, feedback_try_again, feedback_well_done, feedback_keep_trying
menu_start_learning, menu_view_progress, select_choose_lesson
progress_title, progress_overview, progress_needs_practice, progress_doing_well
```

**Step 2**: Replace all hardcoded strings with `SimpleLocalization.Get("key")`.

**Step 3**: Add format-safe wrappers: `SimpleLocalization.Format("key", args)` that fall back to the raw template if format fails.

---

## 4. NAVIGATION UX

### 4.1 Scene transitions — add fade to/from black

Current: `SceneManager.LoadScene` is instant. On iOS this shows a white flash.

**Fix**: Use `UIScreen` base class (already exists, unused). Add a shared `SceneTransitionManager` that:
- Fades a full-screen black Image to 1.0 alpha
- Loads the new scene (async or sync)
- Fades back to 0.0 alpha

Or simpler: add a 200ms canvas overlay in BootLoader that fades in/out on scene load.

### 4.2 Cancel confirmation dialog

Current: Cancel button immediately loads `SC_MainMenu` mid-activity. No confirmation.

**Fix**: On cancel during an in-progress activity, show modal:
```
"Con muốn dừng bài học không?"
[Tiếp tục học] [Về trang chính]
```

Show activity name, current round/total, and time spent. Only in `ActivityState.InProgress` — not needed after completion.

### 4.3 Loading screen between activities

When transitioning to next activity via `LoadNextActivity`, there's a white gap while `SC_ARGameplay` reinitializes. AR session setup takes 1-3 seconds.

**Fix**: Show a "loading" overlay with the next activity's name and a spinner animation. `ActivityFlowNavigator` should set a flag that the gameplay scene checks on load to show loading UI before AR init.

---

## 5. FEEDBACK UX

### 5.1 Auto-hide timing is too fast for children

| Activity | Correct feedback auto-hide | Incorrect auto-hide |
|---|---|---|
| QuantityMatch | 2s | 1.5s |
| CompareQuantity | 1.8s (overlay) | 1.2s (overlay) |
| NumberBonds | None | None |
| NumberLineJump | None | None |

Children 5-7 have slower reading speeds. Some can't read at all — they need time to parse the visual feedback (colors, animations, faces).

**Fix**: 
- Correct feedback: 3-5s or until user taps "Next"
- Incorrect feedback: 2-3s or until user taps to retry
- Show a playful animated icon (✅/❌ face) alongside text for pre-readers
- NumberLineJump and NumberBonds should also auto-hide (or add a tap-to-dismiss)

### 5.2 Inconsistent feedback colors

| Activity | Correct color | Incorrect color |
|---|---|---|
| QuantityMatch | Green `Color.green` | Orange `(1.0, 0.5, 0)` |
| CompareQuantity | Green `Color.green` | Red `Color.red` |
| NumberBonds | Green | Red |
| NumberLineJump | Green | Red |

**Fix**: Standardize to:
- Correct: `#4CAF50` (soft green, accessible on light backgrounds)
- Incorrect: `#FF7043` (soft orange, less alarming than pure red for children)
- Define in `UIDesignTokens`

### 5.3 NumberLineJump doesn't change feedback panel background

Unlike CompareQuantity and NumberBonds which tint their feedback panel background (green/red), NumberLineJump only changes `feedbackText.color`.

**Fix**: Add panel background tinting to NumberLineJumpView to match the other activities.

---

## 6. HINT SYSTEM UX

### 6.1 `UIHintBubble` exists but is unused

**File**: `Core/UI/Components/UIHintBubble.cs`

This component has slide-in/out animations with configurable duration, but all 4 views use raw `hintPanel.SetActive(true)` instead.

**Fix**: Integrate `UIHintBubble` into the View base or call it from each View's `ShowHint()`. Benefits:
- Slide-in animation draws attention better than instant appearance
- Consistent hint appearance across activities
- Built-in auto-hide with configurable duration

### 6.2 No hint counter displayed

Children can't see how many hints they've used or have remaining.

**Fix**: Show "Gợi ý 1/3" in the hint panel header, or show hint dots (○○●) under the hint button. Decrement hint button opacity or add a visual counter on the button itself.

### 6.3 No "out of hints" visual state

When `maxHintsPerQuestion` is reached, the hint button should show a distinct state: faded, with label "Hết gợi ý" instead of "Gợi ý".

---

## 7. ACCESSIBILITY

### 7.1 Color-blind friendly mode

The current palette uses red/green distinction for feedback. ~8% of males have red-green color blindness.

**Fix**: Don't rely on color alone for correctness feedback. Always pair color with:
- Icon: ✅ / ❌
- Text: "Đúng!" / "Sai!"
- Position: correct feedback appears top-center, incorrect appears near the answer button
- Motion: correct = bounce up, incorrect = shake

Add a `ColorBlindMode` toggle in `UserPreferences` that swaps the palette to blue/orange.

### 7.2 No undo button in NumberBonds

Drag-and-drop is inherently imprecise for young children on AR surfaces. Accidental drops are common.

**Fix**: Add an "Undo" button that moves the last-dropped object back to its source zone. Track a `Stack<DragUndoRecord>` in the drag adapter.

### 7.3 Touch target size for edge buttons

NumberLineJump's left/right jump buttons are 142x218px but anchored to screen edges with only 26px margin. On devices with curved screen edges or thick cases, the buttons may be partially unreachable.

**Fix**: Increase edge margin to 40px. Make buttons fully opaque (currently have slight transparency). Add a subtle indicator animation (pulse) to draw attention to the available jump direction.

### 7.4 No text-to-speech for instruction text

Only pre-recorded audio for the core instruction (e.g., "instruction_compare_quantity"). No dynamic TTS for question-specific text ("3 + 4 = ?").

**Fix**: Add `SimpleAudioManager.SpeakNumber(int)` that plays pre-recorded number clips (0-20, hundred, thousand). Use these to read equation components aloud. This is especially important for pre-literate 5-year-olds.

---

## 8. MOTIVATION & ENGAGEMENT

### 8.1 No in-activity rewards

No stars, points, or celebration animations during play — only at activity completion.

**Proposal**:
- Each correct round: star appears with sparkle animation
- 3 consecutive correct rounds: "Streak!" text + extra celebration
- Activity completion: total stars collected + animation sequence
- Store stars in `ActivityResult` per round, display in `ProgressDashboardView`

### 8.2 Confetti only in QuantityMatch feedback overlay

`UIFeedbackOverlay.ShowCorrect()` spawns confetti particles, but `UIFeedbackOverlay` is an optional `[SerializeField]` that must be manually assigned. No other activity spawns confetti.

**Fix**: Add confetti particle instantiation to the fallback feedback path (when `feedbackOverlay` is null). Use a shared `ConfettiSpawner` utility.

### 8.3 NumberBond zones have no animation

`SetValidationState()` changes zone color immediately (no transition). The plan describes "zones glow green/pulse" for correct feedback.

**Fix**: Add coroutine in `NumberBondZoneView` that lerps the material color or emissive property over 0.5s.

---

## 9. RESPONSIVE LAYOUT

### 9.1 All panel positions are hardcoded pixels

No dynamic positioning based on screen size. On iOS devices with different aspect ratios (iPhone SE vs iPad Pro), panels may overlap or be misaligned.

**Fix**: Use `RectTransform` anchor-based positioning instead of absolute pixel offsets:
```csharp
// Instead of:
rect.anchoredPosition = new Vector2(0f, -296f);

// Use:
rect.anchorMin = new Vector2(0.5f, 0f);
rect.anchorMax = new Vector2(0.5f, 0.15f);
rect.offsetMin = Vector2.zero;
rect.offsetMax = Vector2.zero;
```

### 9.2 Font scaling doesn't account for Safe Area

iOS devices with notches (iPhone X+) have safe area insets that may clip UI elements.

**Fix**: Apply `Screen.safeArea` offset to the root Canvas RectTransform in `Awake()`. Unity's `Screen.safeArea` returns the usable area.

---

## 10. PROGRESS DASHBOARD UX

### 10.1 Current state
**File**: `ProgressDashboardView.cs`

Displays: activity name, score (always 0 — `LocalProgressStorage` is a stub), play button. Very minimal.

### 10.2 Improvements

| Feature | Current | Proposed |
|---|---|---|
| Score display | "0" (stub) | Stars per activity, total stars |
| Activity list | Hardcoded 4 activities | Dynamic from config |
| Visual progress | None | Progress bar per activity |
| Last played | None | "Chơi lần cuối: hôm qua" |
| Recommendations | "Cần luyện thêm" (hardcoded text) | Based on actual scores |
| Play button | Simple button | Large card with animal character |

---

## 11. PRIORITIZED ACTION PLAN

### Sprint A: Critical (blockers cho Editor test) — 1-2 ngày

- [ ] **A1**: Fix NumberBonds BuildRuntimeUi (hoặc override HasUiReferences)
- [ ] **A2**: Unify canvas reference resolution to 1920x1080
- [ ] **A3**: Rename/clean up `UpdateButtonLabels` + `NormalizeComparisonButtonLabel`
- [ ] **A4**: Remove QuantityMatch dead increment/decrement buttons

### Sprint B: Polish (demo-ready UX) — 2-3 ngày

- [ ] **B1**: Extract shared UI builder into `UIActivityLayoutBuilder` (~400 line reduction)
- [ ] **B2**: Implement scene transition fade (200ms overlay in BootLoader)
- [ ] **B3**: Add cancel confirmation dialog
- [ ] **B4**: Integrate `UIHintBubble` in all 4 views
- [ ] **B5**: Add hint counter display ("Gợi ý 1/3")
- [ ] **B6**: Increase feedback auto-hide to 3-5s
- [ ] **B7**: Standardize feedback colors to `UIDesignTokens`
- [ ] **B8**: Add panel background tint for NumberLineJump feedback
- [ ] **B9**: Add confetti to fallback feedback path
- [ ] **B10**: Add zone glow animation to NumberBondZoneView

### Sprint C: Accessibility & localization — 2-3 ngày

- [ ] **C1**: Add ~56 localization keys to `SimpleLocalization`
- [ ] **C2**: Replace all hardcoded strings with `SimpleLocalization.Get()`
- [ ] **C3**: Add ✅/❌ icons alongside text feedback
- [ ] **C4**: Add color-blind friendly feedback mode
- [ ] **C5**: Add undo button for NumberBonds
- [ ] **C6**: Increase edge button margin to 40px
- [ ] **C7**: Apply `Screen.safeArea` offset to root canvas

### Sprint D: Engagement & motivation — 1-2 ngày (post-MVP)

- [ ] **D1**: Star collect system per round
- [ ] **D2**: Streak counter + celebration
- [ ] **D3**: Number TTS (pre-recorded 0-20 clips)
- [ ] **D4**: Loading screen with next activity name + spinner
- [ ] **D5**: Progress dashboard overhaul with actual scores
- [ ] **D6**: Add iOS haptic feedback (light impact on correct, rigid on incorrect)

---

## Tổng kết số liệu

| Metric | Current | After |
|---|---|---|
| Duplicated helper lines | ~400 | ~0 |
| Hardcoded Vietnamese strings | ~56 | ~0 (fully localized) |
| Inconsistent feedback timings | 3 different values | 1 unified (5s correct, 3s incorrect) |
| Inconsistent feedback colors | 2 different palettes | 1 (green/orange) |
| Dead UI components | 4 (increment buttons, UIHintBubble, UIScreenManager, UIScreen) | 0 or properly integrated |
| Accessibility features | 0 | 5 (color-blind, undo, safe area, touch targets, icon+text feedback) |
| Motivation features | 0 | 3 (stars, streaks, confetti) |

---

*Tổng hợp: 29/05/2026 — từ codebase exploration 60+ files*
