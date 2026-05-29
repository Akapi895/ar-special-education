# Phân tích 4 dạng bài giảng cần demo: hiện trạng implementation, mục tiêu proposal và khoảng cách cần cải tiến

**Dự án:** AR Special Education - Unity AR Client  
**Mục tiêu tài liệu:** dùng làm tài liệu chuẩn bị demo và lập backlog implementation cho 4 dạng bài học: `Quantity Match`, `Compare Quantity`, `Number Bonds`, `Number Line Jump`.  

---

## 1. Quantity Match

### 1.1. Vai trò của bài trong demo

`Quantity Match` là bài học nền tảng đầu tiên trong proposal. Bài này nhằm giúp trẻ Dyscalculia hình thành liên kết giữa **số lượng vật thể thực tế** và **ký hiệu số học**. Proposal nhấn mạnh rằng với nhiều trẻ, con số chỉ là ký tự trừu tượng; vì vậy bài học cần giúp trẻ nhìn thấy nhóm vật thể AR, đếm số lượng, rồi ghép với thẻ số tương ứng. Đây là bước tạo `number sense` trước khi chuyển sang so sánh số lượng, tách-gộp số và phép tính trên trục số.

Trong demo, bài này nên được dùng để mở đầu vì nó dễ hiểu nhất với người xem: hệ thống đặt một nhóm vật thể 3D lên mặt bàn thật, yêu cầu trẻ chọn số đúng, sau đó phản hồi đúng/sai bằng hiệu ứng thị giác và âm thanh.

### 1.2. Hệ thống hiện tại đang hỗ trợ hoặc đã triển khai những gì

#### Trạng thái hiện tại

Theo tài liệu kiến trúc, `QuantityMatch` là một trong ba activity đã được liệt kê trong `Activity Layer`. Activity này đã có đủ ba thành phần chính theo pattern hiện tại:

```text
QuantityMatchPresenter
QuantityMatchView
QuantityMatchActivityBootstrap
```

`QuantityMatchPresenter` kế thừa từ `ActivityPresenter`, tức là có thể tận dụng các cơ chế chung của activity framework: state machine, quản lý round, hint system, result tracking, persistence và feedback. Tài liệu cũng ghi rõ `QuantityMatchView` hiện đang rất lớn, khoảng 1892 dòng, được đánh dấu là một rủi ro bảo trì.

#### Luồng người dùng hiện tại có thể demo

Luồng demo hiện tại có thể đi theo pipeline chung của hệ thống:

```text
SC_Boot
→ SC_MainMenu
→ SC_ActivitySelect
→ chọn QuantityMatch
→ SC_ARGameplay
→ đặt learning area
→ hệ thống spawn nhóm vật thể
→ trẻ chọn / tương tác với đáp án
→ hệ thống kiểm tra đúng sai
→ feedback
→ lưu kết quả round
→ chuyển round tiếp theo hoặc hoàn thành activity
```

Trong flow ví dụ của tài liệu kiến trúc, `GameplayActivityRouter.RouteSelectedActivity()` phát hiện `activityId == "QuantityMatch"`, sau đó khởi chạy `QuantityMatchActivityBootstrap`. Bootstrap chờ `Placement.IsPlacementAvailable` và `Placement.HasLearningArea`, rồi gọi:

```text
presenter.Initialize(config, view, placement, interaction)
presenter.StartActivity()
```

Sau đó Presenter gọi `LoadRound`, spawn AR groups qua `ARGroupSpawnUtility`, View hiển thị câu hỏi, người dùng tap vào AR group, `ARInteractionService.OnObjectTapped` chuyển answer vào `presenter.SubmitAnswer()`, rồi Presenter gọi `CheckAnswer`, feedback đúng/sai, persist result và chuyển round.

#### Luồng kỹ thuật phía sau

Các thành phần kỹ thuật liên quan trực tiếp đến bài này gồm:

| Thành phần | Vai trò trong Quantity Match |
|---|---|
| `ARServiceBootstrap` | Resolve `Session`, `Placement`, `Interaction`; quyết định mock placement trong Editor hay real placement trên mobile. |
| `ARPlacementService` | Tạo learning area, spawn object theo position/grid/circle, dùng plane detection và anchor. |
| `ARInteractionService` | Bắt tap/select/highlight/drag tùy cấu hình; chuyển interaction của object AR thành sự kiện cho Presenter. |
| `ActivityPresenter` | Quản lý lifecycle: `Initializing → Ready → InProgress → Completed/Failed/Cancelled`. |
| `QuantityMatchPresenter` | Chứa logic riêng của bài: load round, check answer, xử lý correct/incorrect. |
| `QuantityMatchView` | Render UI, câu hỏi, feedback, có khả năng runtime UI generation. |
| `ProgressStorageProxy` / `LocalProgressStorage` | Lưu session, activity result, tiến độ học. |
| `FeedbackServiceProxy` / `SimpleAudioManager` | Phát feedback thị giác, âm thanh, instruction replay nếu có. |

Về nguyên tắc code, tài liệu rules yêu cầu Presenter nhận dependency qua `Initialize()` hoặc constructor, không tự `FindAnyObjectByType`; View chỉ render UI, forward input và playback animation/feedback; config phải là `ScriptableObject`, không hardcode question trong code.

### 1.3. Hệ thống theo proposal đang nhắm tới mục tiêu hoặc trải nghiệm gì

Proposal mô tả `Quantity Match` theo hướng:

```text
Nhìn nhóm vật thể AR → đếm trực tiếp → kéo/thả hoặc chọn thẻ số đúng → nhận feedback tức thì.
```

Mục tiêu trải nghiệm cụ thể:

| Khía cạnh | Mục tiêu theo proposal |
|---|---|
| Năng lực học tập | Nhận biết số lượng, liên kết vật thể thực tế với ký hiệu số, phát triển `number sense`. |
| Ngữ cảnh AR | Các animal cube 3D được đặt trên mặt bàn thật. |
| Tương tác | Trẻ quan sát, đếm số lượng, kéo-thả thẻ số phù hợp vào vùng trả lời. |
| Độ khó | Cấp dễ: vật thể xếp theo hàng để hỗ trợ đếm tuần tự. Cấp khó: vật thể xuất hiện ngẫu nhiên trong không gian để rèn quan sát và ước lượng. |
| Feedback đúng | Vật thể phát sáng, có âm thanh xác nhận tích cực. |
| Feedback sai | Hệ thống highlight từng vật thể để trẻ đếm lại. |
| AR support | Surface tracking để đặt vật thể 3D lên bề mặt thật, tương tác chạm/kéo-thả, real-time visual/audio feedback. |

### 1.4. Khoảng cách giữa hiện tại và proposal

| Nhóm khoảng cách | Hiện tại trong tài liệu | Proposal mong muốn | Nhận xét cho demo |
|---|---|---|---|
| Kiểu tương tác chính | Flow ví dụ hiện tại mô tả user tap AR group rồi submit answer. | Trẻ kéo-thả thẻ số phù hợp vào vùng trả lời. | Cần xác định demo dùng `tap/select` hay `drag/drop number card`. Nếu chưa có drag/drop thẻ số, nên demo tap trước và ghi rõ đây là phiên bản rút gọn. |
| Bố cục vật thể theo độ khó | Tài liệu có `SpawnGrid`, `SpawnCircle`, `ARGroupSpawnUtility`, nhưng chưa thấy mô tả level row/random riêng cho Quantity Match. | Cấp dễ xếp hàng, cấp khó phân bố ngẫu nhiên. | Cần đưa layout mode vào `QuantityMatchConfig`. |
| Hint khi sai | Activity framework có `HintSystem`; flow có incorrect feedback. | Sai thì highlight từng vật thể để trẻ đếm lại. | Cần đảm bảo hint không chỉ là text/audio, mà có step-by-step object highlight. |
| Nội dung bài | Có `config`, `LoadRound`, `CheckAnswer`, nhưng tài liệu không mô tả schema nội dung cụ thể. | Có vật thể quen thuộc, thẻ số, target count, progression. | Cần formal hóa content schema thay vì để logic nằm trong View/Presenter. |
| Kích thước View | `QuantityMatchView` khoảng 1892 dòng, được đánh dấu rủi ro. | Proposal cần thêm UI thẻ số, feedback, hint, progression. | Không nên mở rộng tiếp trong View hiện tại; cần tách runtime UI/factory/layout. |
| Startup activity | `TryStartActivity` polling 0.5s chờ placement. | Trải nghiệm demo cần ổn định. Rules yêu cầu event-driven thay vì polling. | Đây là rủi ro demo: activity có thể start không ổn định nếu AR/placement race. |
| Tracking learning vs technical issue | Storage có progress/session/result; rules yêu cầu technical issue tracking. | Proposal tập trung learning feedback. | Cần phân biệt trẻ sai do học với lỗi do AR placement/touch/drop. |

### 1.5. Những việc cần làm tiếp theo

#### Cần làm cho demo gần

1. Chuẩn hóa một demo path đơn giản: `targetCount → spawn group → hiển thị 3-4 thẻ số → user chọn/kéo số → feedback`.
2. Nếu chưa kịp kéo-thả thẻ số, dùng tap/click chọn thẻ số hoặc chọn đáp án UI, nhưng ghi rõ đây là interaction fallback so với proposal.
3. Thêm `layoutMode` vào config: `Row`, `Grid`, `Circle`, `RandomScatter`.
4. Thêm hint sai cấp 1: highlight từng object theo thứ tự đếm.
5. Đảm bảo flow demo có learning area ổn định trước khi spawn object.

#### Cần làm để khớp proposal tốt hơn

1. Refactor `QuantityMatchView` thành các phần nhỏ hơn:

```text
QuantityMatchView.cs
QuantityMatchRuntimeUI.cs
QuantityMatchLayoutBuilder.cs
QuantityMatchAnswerCardView.cs
QuantityMatchFeedbackView.cs
```

2. Tách answer card interaction khỏi logic Presenter. View forward event `OnNumberCardDropped(number)` hoặc `OnNumberCardSelected(number)` lên Presenter.
3. Bổ sung `QuantityMatchConfig` dạng `ScriptableObject` chứa:

```text
minCount
maxCount
objectTheme
layoutMode
answerOptionsCount
interactionMode: TapCard | DragDropCard
hintMode: HighlightObjects | ReorderToLine | AudioCount
roundCount
```

4. Bổ sung metrics phục vụ học tập:

```text
selectedNumber
correctNumber
attemptCount
countingHintUsed
responseTime
layoutMode
technicalIssueFlag
```

5. Chuyển bootstrap startup từ polling sang event-driven theo rule: session ready + learning area placed + config valid thì mới start.

---

## 2. Compare Quantity

### 2.1. Vai trò của bài trong demo

`Compare Quantity` là bước thứ hai trong lộ trình proposal. Sau khi trẻ đã liên kết được ký hiệu số với số lượng thực tế ở `Quantity Match`, bài này giúp trẻ hiểu quan hệ độ lớn giữa hai nhóm: **nhiều hơn**, **ít hơn**, **bằng nhau**. Đây là bước chuyển từ nhận biết số lượng đơn lẻ sang so sánh magnitude.

Trong demo, bài này nên thể hiện rõ sự khác biệt giữa hai nhóm vật thể AR đặt cạnh nhau. Người xem cần thấy rằng trẻ không chỉ chọn ký hiệu `>`, `<`, `=`, mà còn trực tiếp quan sát được sự chênh lệch giữa hai nhóm.

### 2.2. Hệ thống hiện tại đang hỗ trợ hoặc đã triển khai những gì

#### Trạng thái hiện tại

Theo tài liệu kiến trúc, `CompareQuantity` đã tồn tại trong `Activity Layer` với ba thành phần:

```text
CompareQuantityPresenter
CompareQuantityView
CompareQuantityActivityBootstrap
```

`GameplayActivityRouter` cũng có branch:

```text
"CompareQuantity" → CreateCompareQuantityActivity()
```

Điều này cho thấy bài đã nằm trong hệ thống activity hiện tại. Tuy nhiên, tài liệu kiến trúc không mô tả chi tiết luồng riêng của `CompareQuantity` giống như ví dụ chi tiết dành cho `QuantityMatch`. `CompareQuantityView` cũng được ghi nhận là lớn, khoảng 1025 dòng, tức là có rủi ro tương tự về bảo trì và mở rộng.

#### Luồng người dùng hiện tại có thể demo

Dựa trên pipeline chung, luồng demo hiện tại có thể là:

```text
SC_MainMenu
→ ActivitySelect
→ chọn CompareQuantity
→ SC_ARGameplay
→ hệ thống chuẩn bị AR services
→ đặt learning area
→ spawn hai nhóm vật thể
→ trẻ chọn quan hệ / đáp án
→ Presenter kiểm tra answer
→ feedback đúng/sai
→ lưu round result
```

Phần chắc chắn từ tài liệu là activity đã có Presenter/View/Bootstrap và được router hỗ trợ. Phần chưa được tài liệu mô tả rõ là UI cụ thể: lựa chọn nhóm nhiều hơn, lựa chọn ký hiệu `>`, `<`, `=`, hay một dạng input khác.

#### Luồng kỹ thuật phía sau

Luồng kỹ thuật nên bám vào activity framework hiện tại:

```text
GameplayActivityRouter
→ CompareQuantityActivityBootstrap
→ CompareQuantityPresenter.Initialize(config, view, placement, interaction)
→ CompareQuantityPresenter.StartActivity()
→ LoadRound()
→ spawn left group / right group bằng IARPlacementService hoặc ARGroupSpawnUtility
→ View.ShowQuestion(leftCount, rightCount, relationOptions)
→ user input forwarded to Presenter
→ CheckAnswer()
→ feedback + persistence
```

Các AR capability hiện tại có thể hỗ trợ bài này:

| Capability hiện có | Cách dùng cho Compare Quantity |
|---|---|
| `SpawnGrid` / `SpawnCircle` / `SpawnAtLearningAreaPosition` | Spawn hai nhóm vật thể ở hai vùng trái/phải. |
| `ARInteractionService` tap/select/highlight | Cho trẻ chọn nhóm nhiều hơn hoặc chọn ký hiệu. |
| `FeedbackServiceProxy` | Feedback đúng/sai. |
| `HintSystem` | Gợi ý khi trẻ nhầm quan hệ. |
| `RuntimePerformanceSettings` | Clamp số object nếu nhóm quá lớn trên iOS. |

### 2.3. Hệ thống theo proposal đang nhắm tới mục tiêu hoặc trải nghiệm gì

Proposal mô tả `Compare Quantity` theo hướng:

```text
Hiển thị hai nhóm vật thể AR cạnh nhau → trẻ quan sát chênh lệch → chọn >, < hoặc = → nhận feedback.
```

Mục tiêu trải nghiệm cụ thể:

| Khía cạnh | Mục tiêu theo proposal |
|---|---|
| Năng lực học tập | Phát triển khả năng so sánh đại lượng, hiểu lớn hơn - nhỏ hơn - bằng nhau. |
| Ngữ cảnh AR | Hai nhóm vật thể 3D như động vật, bóng, khối lập phương được hiển thị đồng thời trên mặt phẳng. |
| Ví dụ | Bên trái có 3 con mèo, bên phải có 5 con mèo; trẻ xác định nhóm nào nhiều hơn/ít hơn/bằng nhau. |
| Tương tác | Trẻ lựa chọn ký hiệu `>`, `<`, hoặc `=`. |
| Độ khó | Cấp đầu: object xếp ngay hàng để hỗ trợ đối chiếu. Cấp cao: object phân bố tự do để rèn ước lượng và tư duy không gian. |
| AR support | Markerless AR, hiển thị nhiều nhóm vật thể trong môi trường thực, tương tác bằng tay, hiệu ứng chuyển động và âm thanh. |

### 2.4. Khoảng cách giữa hiện tại và proposal

| Nhóm khoảng cách | Hiện tại trong tài liệu | Proposal mong muốn | Nhận xét cho demo |
|---|---|---|---|
| Mức độ module | Có `CompareQuantityPresenter/View/Bootstrap`. | Cần trải nghiệm so sánh hai nhóm rõ ràng. | Module đã có, nhưng cần kiểm chứng UI/logic cụ thể có đủ cho demo không. |
| Luồng riêng | Không có flow chi tiết như QuantityMatch. | Flow rõ: hai nhóm vật thể → chọn `>`, `<`, `=`. | Cần viết/kiểm tra flow demo riêng để tránh phụ thuộc vào giả định. |
| Input mode | Tài liệu không xác nhận rõ trẻ chọn group hay chọn ký hiệu. | Chọn ký hiệu `>`, `<`, `=`. | Nên có UI symbol buttons/cards rõ ràng trong demo. |
| Layout progression | Có khả năng spawn object, nhưng không thấy config row/random riêng cho Compare. | Dễ: xếp hàng; khó: phân bố tự do. | Cần thêm `layoutMode` cho left/right groups. |
| Feedback | Có hệ thống feedback/hint chung. | Chuyển động + âm thanh tăng nhận biết trực quan. | Cần feedback trực quan cho quan hệ: highlight nhóm nhiều hơn, ghép cặp, hoặc show relation symbol. |
| View size | `CompareQuantityView` khoảng 1025 dòng. | Proposal cần mở rộng UI/feedback/hint. | Cần refactor trước khi thêm nhiều mode. |
| Chuẩn bị cho Number Bonds | Proposal coi Compare là bước chuyển sang tách-gộp. | Cần làm rõ difference giữa hai nhóm. | Hiện proposal mới nói nhiều/ít/bằng; implementation nên chuẩn bị thêm dữ liệu chênh lệch nếu demo nối sang Number Bonds. |

### 2.5. Những việc cần làm tiếp theo

#### Cần làm cho demo gần

1. Tạo một kịch bản demo rõ:

```text
Left group = 3
Right group = 5
Question = "Nhóm nào nhiều hơn?" hoặc "3 __ 5"
Answer options = >, <, =
Correct answer = < nếu biểu diễn 3 __ 5
```

2. Đảm bảo View hiển thị rõ hai vùng trái/phải, tránh object chồng lấn.
3. Thêm symbol buttons/cards `>`, `<`, `=` đủ lớn, dễ tap trên iPhone.
4. Khi sai, không chỉ báo đỏ; nên highlight từng cặp object hoặc highlight nhóm lớn hơn để trẻ nhìn lại.
5. Dùng object count nhỏ trong demo: 2-6 object mỗi nhóm để tránh rủi ro performance và giảm nhiễu thị giác.

#### Cần làm để khớp proposal tốt hơn

1. Bổ sung `CompareQuantityConfig` dạng `ScriptableObject`:

```text
leftCount
rightCount
relationTarget: More | Less | Equal | SymbolCompare
layoutMode: AlignedRows | Grid | RandomScatter
objectTheme
allowEqualCase
roundCount
hintMode: Pairing | HighlightLargerGroup | RecountBothGroups
```

2. Tách `CompareQuantityView` nếu tiếp tục mở rộng:

```text
CompareQuantityView.cs
CompareQuantityGroupView.cs
CompareQuantitySymbolInput.cs
CompareQuantityFeedbackView.cs
CompareQuantityLayoutBuilder.cs
```

3. Thêm hint theo lỗi:

| Lỗi | Hint nên có |
|---|---|
| Nhầm `>` và `<` | Hiển thị hướng đọc trái-sang-phải và đặt symbol giữa hai nhóm. |
| Không nhận ra bằng nhau | Nối cặp 1-1 giữa hai nhóm. |
| Bị nhiễu bởi object to/nhỏ | Nhắc trẻ đếm số lượng, không nhìn kích thước. |
| Đếm thiếu object phân tán | Highlight từng object theo thứ tự. |

4. Bổ sung metric:

```text
leftCount
rightCount
selectedRelation
correctRelation
layoutMode
usedPairingHint
responseTime
attemptCount
```

5. Nếu muốn nối demo sang Number Bonds, thêm bước phụ: “Bên phải nhiều hơn bên trái bao nhiêu?” Đây không có trong proposal như yêu cầu chính, nhưng là bridge hợp lý để chuyển từ so sánh sang tách-gộp.

---

## 3. Number Bonds

### 3.1. Vai trò của bài trong demo

`Number Bonds` là bài thứ ba trong proposal và là mắt xích quan trọng giữa so sánh đại lượng và phép cộng/trừ. Bài này giúp trẻ hiểu cấu trúc nội tại của một con số: một số tổng có thể được tách thành hai phần, và hai phần có thể được gộp lại thành tổng.

Trong demo, đây nên là bài thể hiện giá trị AR rõ nhất: trẻ không chỉ chọn đáp án, mà trực tiếp kéo vật thể từ vòng `Whole` sang hai vòng `Parts`, hoặc gom hai phần về tổng. Khi vật thể di chuyển, biểu thức toán học cập nhật theo thời gian thực. Đây là trải nghiệm mạnh hơn worksheet 2D vì nó biến tách-gộp số thành thao tác vật lý trong không gian.

### 3.2. Hệ thống hiện tại đang hỗ trợ hoặc đã triển khai những gì

#### Trạng thái hiện tại

Tài liệu kiến trúc hiện tại **không liệt kê `NumberBonds` như một activity đã triển khai**. Activity layer chỉ liệt kê ba activity:

```text
QuantityMatch
NumberLineJump
CompareQuantity
```

`GameplayActivityRouter` cũng chỉ nêu các branch:

```text
"QuantityMatch" → StartExistingQuantityMatch()
"NumberLineJump" → CreateNumberLineJumpActivity()
"CompareQuantity" → CreateCompareQuantityActivity()
```

Vì vậy, dựa trên tài liệu đã cung cấp, `Number Bonds` đang là **activity còn thiếu** so với proposal.

Tuy nhiên, hệ thống hiện tại có một số nền tảng có thể tái sử dụng để xây bài này:

| Nền tảng hiện có | Cách tái sử dụng cho Number Bonds |
|---|---|
| `ActivityPresenter` base class | Tạo `NumberBondsPresenter` kế thừa để quản lý round, state, result, hint. |
| `ARPlacementService` | Spawn vòng `Whole`, hai vòng `Parts`, object 3D và giữ chúng trong learning area. |
| `ARInteractionService` | Dùng drag optional hoặc tap/select để di chuyển object giữa các vùng. |
| `FeedbackServiceProxy` / `SimpleAudioManager` | Feedback đúng/sai, âm thanh cổ vũ, âm thanh sai. |
| `ProgressStorage` | Lưu kết quả từng round, session và mastery. |
| `RuntimePerformanceSettings` | Clamp số lượng object trong whole để tránh quá tải iOS. |

#### Luồng người dùng hiện tại có thể demo

Vì chưa có activity `NumberBonds` trong kiến trúc, hiện tại không có flow demo hoàn chỉnh được xác nhận bởi tài liệu. Nếu cần demo theo proposal, phải tạo ít nhất một MVP flow mới:

```text
ActivitySelect
→ chọn NumberBonds
→ SC_ARGameplay
→ đặt learning area
→ hiển thị sơ đồ 3 vòng: Whole, Part A, Part B
→ spawn N object trong Whole
→ trẻ kéo object từ Whole sang Part A hoặc Part B
→ biểu thức cập nhật theo số object ở từng vòng
→ trẻ bấm Confirm
→ Presenter check answer
→ feedback đúng/sai
→ lưu result
```

#### Luồng kỹ thuật cần xây dựa trên kiến trúc hiện có

Activity mới nên bám pattern hiện tại:

```text
Features/Activities/NumberBonds/
├── Scripts/NumberBondsPresenter.cs
├── Scripts/NumberBondsView.cs
├── Scripts/NumberBondsConfig.cs
├── Scripts/NumberBondsActivityBootstrap.cs
├── Scripts/NumberBondZone.cs
├── Scripts/NumberBondExpressionBinder.cs
└── Scripts/NumberBondObjectController.cs
```

Luồng kỹ thuật nên là:

```text
GameplayActivityRouter.RouteSelectedActivity()
→ detect activityId == "NumberBonds"
→ create/start NumberBondsActivityBootstrap
→ wait for ARServiceBootstrap.Placement and learning area
→ NumberBondsPresenter.Initialize(config, view, placement, interaction)
→ NumberBondsPresenter.StartActivity()
→ View/Placement tạo 3 zones: Whole, Part A, Part B
→ spawn objects vào Whole
→ object drag/drop đổi zone
→ Zone count changed event
→ ExpressionBinder update biểu thức
→ Confirm button
→ Presenter.CheckAnswer(zoneCounts, config)
→ feedback + persistence
```

### 3.3. Hệ thống theo proposal đang nhắm tới mục tiêu hoặc trải nghiệm gì

Proposal mô tả `Number Bonds` theo hướng:

```text
Hiển thị sơ đồ Number Bond 3 vòng trong AR
→ vòng trên là Whole, hai vòng dưới là Parts
→ trẻ tách/gộp vật thể bằng kéo-thả
→ biểu thức cập nhật realtime
→ trẻ xác nhận phép toán
→ feedback âm thanh/thị giác.
```

Mục tiêu trải nghiệm cụ thể:

| Khía cạnh | Mục tiêu theo proposal |
|---|---|
| Năng lực học tập | Hiểu bản chất tách-gộp của con số; hiểu quan hệ giữa số tổng và số thành phần. |
| Vai trò trong lộ trình | Sau Compare Quantity, trước Number Line Jump; làm cầu nối tới cộng/trừ. |
| Ngữ cảnh AR | Một Number Bond diagram ba vòng tròn trong không gian thực. |
| Whole | Vòng lớn phía trên chứa nhóm vật thể 3D, ví dụ 6 quả dâu hoặc 6 khối lập phương. |
| Parts | Hai vòng nhỏ phía dưới, ban đầu trống hoặc có object tùy mode. |
| Thao tác tách | Trẻ kéo object từ Whole xuống hai Parts. |
| Thao tác gộp | Trẻ gom object từ hai Parts về Whole. |
| Biểu thức | Ví dụ `5 = __ + __` hoặc `__ = 2 + 3`; cập nhật khi số vật thể thay đổi. |
| Xác nhận | Trẻ bấm Confirm để kiểm tra. |
| Feedback | Sai: âm thanh sai/bôi đỏ/sáng biểu thức. Đúng: âm thanh cổ vũ/vỗ tay/thông báo xanh. |
| AR support | Surface tracking + hand-tracking theo proposal; real-time data binding; audio feedback khi gộp/tách. |

### 3.4. Khoảng cách giữa hiện tại và proposal

| Nhóm khoảng cách | Hiện tại trong tài liệu | Proposal mong muốn | Nhận xét cho demo |
|---|---|---|---|
| Module activity | Không thấy `NumberBondsPresenter/View/Bootstrap`; router không có branch `NumberBonds`. | Number Bonds là bài thứ ba trong lộ trình. | Đây là gap lớn nhất. Cần tạo activity mới. |
| Sơ đồ 3 vòng | Không thấy component quản lý zone Whole/Parts. | Cần diagram ba vòng trong không gian thực. | Cần tạo `NumberBondZone` hoặc tương đương. |
| Drag/drop object giữa zones | `ARInteractionService` có drag optional, nhưng không thấy logic drop zone. | Trẻ kéo-thả object giữa Whole và Parts. | Cần bổ sung zone detection và object ownership. |
| Realtime expression binding | Không thấy binder cho biểu thức theo count. | Biểu thức cập nhật ngay khi object thay đổi. | Cần tạo `NumberBondExpressionBinder`. |
| Hand-tracking | Proposal nhắc hand-tracking. | Tương tác bằng cử chỉ tay. | Kiến trúc hiện tại chỉ ghi tap/select/highlight/drag optional qua touch/mouse fallback; chưa có bằng chứng hand-tracking đã triển khai. Demo nên dùng touch drag trước. |
| Check answer | Proposal nêu ví dụ check `A == 0` khi tách hết. | Tùy biểu thức: `5 = __ + __`, `__ = 2 + 3`, hoặc các biến thể khác. | Nếu demo MVP là free split thì `A == 0` đủ; nếu demo target split/missing part thì cần check thêm count từng zone. |
| Config content | Rules yêu cầu config ScriptableObject, không hardcode. | Cần nhiều dạng bài tách/gộp. | Cần `NumberBondsConfig`. |
| Persistence/metrics | Base có persistence. | Cần đo trẻ hiểu tách/gộp ra sao. | Cần metric riêng: wholeCount, partA, partB, moves, attempts. |

### 3.5. Những việc cần làm tiếp theo

#### Cần làm cho demo gần

1. Tạo branch router mới:

```text
"NumberBonds" → CreateNumberBondsActivity()
```

2. Tạo module tối thiểu:

```text
NumberBondsPresenter
NumberBondsView
NumberBondsActivityBootstrap
NumberBondsConfig
```

3. Chỉ demo một mode trước: `Split Whole`.

```text
Whole = 5
Expression = 5 = __ + __
Start: 5 object trong Whole, Part A = 0, Part B = 0
User: kéo object xuống Part A/B
Confirm đúng khi Whole = 0 và PartA + PartB = 5
```

4. Dùng touch drag/drop thay vì hand-tracking để giảm rủi ro kỹ thuật, vì hand-tracking chưa được mô tả là capability hiện có trong architecture.
5. Thêm expression realtime đơn giản:

```text
5 = 2 + 3
```

khi Part A có 2 object và Part B có 3 object.

#### Cần làm để khớp proposal tốt hơn

1. Tạo `NumberBondZone` cho ba vùng:

```text
zoneType: Whole | PartA | PartB
currentObjects
currentCount
OnZoneCountChanged
AcceptObject(object)
RemoveObject(object)
```

2. Tạo `NumberBondExpressionBinder`:

```text
wholeCount
partACount
partBCount
mode: Split | Compose
UpdateExpression()
HighlightCorrect()
HighlightIncorrect()
```

3. Tạo `NumberBondsConfig`:

```text
wholeCount
mode: FreeSplit | TargetSplit | Compose
knownPartA
knownPartB
objectTheme
allowZeroPart
roundCount
hintMode
```

4. Thêm các mode sau MVP:

| Mode | Ví dụ | Điều kiện đúng |
|---|---|---|
| FreeSplit | `5 = __ + __` | Whole = 0, PartA + PartB = 5. |
| TargetSplit | `5 = 2 + __` | PartA = 2, PartB = 3, Whole = 0. |
| Compose | `__ = 2 + 3` | Whole/result = 5 hoặc gom đủ 5 object. |
| MissingPart | `7 = __ + 4` | Missing part = 3. |

5. Thêm hint theo lỗi:

| Lỗi | Hint cần có |
|---|---|
| Còn object trong Whole khi confirm | Highlight Whole và nhắc trẻ chia hết object xuống hai phần. |
| PartA + PartB không bằng Whole ban đầu | Highlight object bị thiếu/thừa hoặc reset nhẹ. |
| Trẻ không hiểu biểu thức | Animation gom hai Parts lại thành Whole rồi tách lại. |
| Kéo lệch zone | Không tính learning error; đưa object về vị trí gần nhất hoặc hiện vùng drop rõ hơn. |

6. Ghi metrics riêng:

```text
wholeCount
partACount
partBCount
moveCount
confirmAttemptCount
wrongReason: NotAllSplit | WrongTargetPart | InteractionDropFail | TechnicalIssue
responseTime
```

---

## 4. Number Line Jump

### 4.1. Vai trò của bài trong demo

`Number Line Jump` là bài cuối trong chuỗi proposal. Sau khi trẻ đã hiểu số lượng, quan hệ độ lớn và cấu trúc tách-gộp, bài này chuyển từ trải nghiệm vật lý tĩnh sang biểu diễn phép cộng/trừ trên một trục số tuyến tính. Proposal mô tả cộng là hành động tiến sang phải, trừ là hành động lùi sang trái. Đây là cách biến phép tính trừu tượng thành chuyển động hình học.

Trong demo, bài này nên được dùng để kết thúc lộ trình: trẻ nhìn thấy phép tính như `3 + 4 = ?`, một nhân vật đứng ở số 3 trên trục số 0-10, rồi kéo hoặc cho nhân vật nhảy từng bước sang phải để dừng ở 7.

### 4.2. Hệ thống hiện tại đang hỗ trợ hoặc đã triển khai những gì

#### Trạng thái hiện tại

Theo tài liệu kiến trúc, `NumberLineJump` đã là một activity hiện có:

```text
NumberLineJumpPresenter
NumberLineJumpView
NumberLineJumpActivityBootstrap
```

Router cũng có branch:

```text
"NumberLineJump" → CreateNumberLineJumpActivity()
```

Điều này cho thấy module đã được nhận diện trong architecture. Tuy nhiên, tương tự `CompareQuantity`, tài liệu không mô tả chi tiết luồng riêng của `NumberLineJump` như với `QuantityMatch`. Vì vậy, cần kiểm tra code thực tế để biết module hiện tại đã có trục số, nhân vật, snap point, expression update và confirm button đến mức nào.

#### Luồng người dùng hiện tại có thể demo

Dựa trên pipeline chung, flow demo hiện tại nên là:

```text
ActivitySelect
→ chọn NumberLineJump
→ SC_ARGameplay
→ đặt learning area
→ hiển thị trục số AR
→ hiển thị nhân vật tại vị trí bắt đầu
→ hiển thị phép tính mục tiêu
→ trẻ kéo/tap nhân vật qua các vạch số
→ biểu thức cập nhật
→ trẻ bấm Confirm
→ feedback đúng/sai
→ lưu result
```

Trong trường hợp implementation hiện tại chỉ có skeleton hoặc chưa hoàn thiện UI/interaction, demo cần thu hẹp còn:

```text
spawn number line + character
→ tap/drag character đến vị trí kết quả
→ check result
```

#### Luồng kỹ thuật phía sau

Luồng kỹ thuật nên bám activity framework:

```text
GameplayActivityRouter
→ NumberLineJumpActivityBootstrap
→ NumberLineJumpPresenter.Initialize(config, view, placement, interaction)
→ NumberLineJumpPresenter.StartActivity()
→ View/Placement tạo number line 0..10 trong learning area
→ spawn character tại startNumber
→ bind expression UI
→ user drag/tap character
→ character snaps to nearest tick
→ Presenter nhận currentPosition/currentStepCount
→ Confirm
→ CheckAnswer()
→ feedback + persistence
```

Các capability hiện tại có thể hỗ trợ:

| Capability hiện có | Cách dùng cho Number Line Jump |
|---|---|
| `ARPlacementService.SpawnAtLearningAreaPosition` | Đặt vạch số, label số, nhân vật trong learning area. |
| `ARInteractionService` drag optional | Cho kéo nhân vật trên mặt phẳng. |
| `ARInteractionService` tap/select | Có thể dùng tap từng bước nếu drag chưa ổn định. |
| `FeedbackServiceProxy` | Feedback đúng/sai khi confirm. |
| `SimpleAudioManager` | Hướng dẫn “tiến lên”, “lùi lại”, hoặc đọc số. |
| `RuntimePerformanceSettings` | Giới hạn số object nếu mở rộng trục số. |

### 4.3. Hệ thống theo proposal đang nhắm tới mục tiêu hoặc trải nghiệm gì

Proposal mô tả `Number Line Jump` theo hướng:

```text
Hiển thị trục số 3D từ 0 đến 10
→ đặt nhân vật ở số bắt đầu
→ hiển thị phép tính mục tiêu
→ trẻ kéo nhân vật sang phải/trái qua từng vạch số
→ vế phải của biểu thức cập nhật theo vị trí nhân vật
→ bấm Confirm để kiểm tra.
```

Mục tiêu trải nghiệm cụ thể:

| Khía cạnh | Mục tiêu theo proposal |
|---|---|
| Năng lực học tập | Hiểu phép cộng/trừ cơ bản thông qua dịch chuyển trên trục số. |
| Ý nghĩa cộng | Dịch chuyển sang phải làm số lượng/kết quả tăng. |
| Ý nghĩa trừ | Dịch chuyển sang trái làm số lượng/kết quả giảm. |
| Ngữ cảnh AR | Trục số 3D từ 0 đến 10 nằm cố định trên mặt sàn hoặc mặt bàn thật. |
| Nhân vật | Chú ếch hoặc robot đứng tại số bắt đầu. |
| Ví dụ | `3 + 4 = ?`, nhân vật bắt đầu ở 3. |
| Tương tác | Trẻ chạm/kéo nhân vật qua từng vạch số. |
| Realtime update | Nếu nhân vật từ ô 3 kéo sang phải 1 bước, biểu thức cập nhật từ `3+4 = 3` thành `3+4 = 4`. |
| Xác nhận | Trẻ bấm Confirm; đúng thì xanh + âm thanh cổ vũ, sai thì đỏ + âm thanh sai. |

### 4.4. Khoảng cách giữa hiện tại và proposal

| Nhóm khoảng cách | Hiện tại trong tài liệu | Proposal mong muốn | Nhận xét cho demo |
|---|---|---|---|
| Module activity | Có `NumberLineJumpPresenter/View/Bootstrap`; router hỗ trợ. | Cần trục số AR 0-10, nhân vật, phép tính, confirm. | Module đã có nhưng cần kiểm tra mức hoàn thiện UI/logic. |
| Trục số | Tài liệu architecture không mô tả chi tiết number line object/ticks. | Trục số 3D từ 0 đến 10 cố định trên mặt sàn/bàn. | Cần đảm bảo có prefab hoặc runtime builder cho ticks/labels. |
| Nhân vật | Không thấy mô tả chi tiết trong architecture. | Có nhân vật hoạt hình như ếch/robot. | Nếu chưa có asset, demo có thể dùng placeholder cube/sphere nhưng nên ghi rõ. |
| Drag qua từng vạch | `ARInteractionService` có drag optional. | Trẻ kéo qua từng vạch số. | Cần snap-to-tick; không nên để free drag liên tục. |
| Realtime expression update | Không thấy binder cụ thể trong architecture. | Biểu thức cập nhật theo vị trí nhân vật. | Cần `NumberLineExpressionBinder`. |
| Step count vs position | Proposal mô tả kéo đến vị trí và cập nhật vế phải theo ô dừng. | Trẻ cần hiểu cộng/trừ là số bước di chuyển. | Nên hiển thị cả `currentPosition` và `jumpCount`; nếu không, trẻ có thể chỉ kéo đến đáp án mà không hiểu số bước. |
| Feedback | Có feedback chung. | Sai/đúng bằng màu biểu thức + âm thanh. | Cần feedback gắn với expression và vị trí trên trục số, không chỉ popup chung. |
| iOS performance/test | Rules yêu cầu test device thật, memory ổn định sau 10 rounds. | Demo AR thật trên iOS. | Trục số nhiều object label/tick cần tối ưu object count, collider đơn giản. |

### 4.5. Những việc cần làm tiếp theo

#### Cần làm cho demo gần

1. Chuẩn bị một scenario ổn định:

```text
Expression: 3 + 4 = ?
Start = 3
Operation = Add
StepCount = 4
CorrectEnd = 7
Number line = 0..10
```

2. Tạo hoặc kiểm tra runtime number line builder:

```text
Tick 0..10
Label 0..10
Snap points
Character start position
```

3. Cho nhân vật snap vào từng tick, không kéo tự do.
4. Khi nhân vật di chuyển, cập nhật expression:

```text
3 + 4 = currentPosition
```

5. Confirm button check:

```text
currentPosition == correctEnd
```

6. Nếu drag trên device chưa ổn định, dùng interaction fallback: tap nút `+1` / `-1` hoặc tap từng tick để nhân vật nhảy. Fallback này vẫn giữ được ý nghĩa “tiến/lùi trên trục số”.

#### Cần làm để khớp proposal tốt hơn

1. Tạo `NumberLineJumpConfig` rõ ràng:

```text
minNumber
maxNumber
startNumber
operation: Add | Subtract
stepCount
correctEnd
interactionMode: DragSnap | TapStep | ButtonStep
characterPrefab
showJumpArcs
roundCount
```

2. Tách logic view nếu cần:

```text
NumberLineJumpView.cs
NumberLineBuilder.cs
NumberLineCharacterController.cs
NumberLineExpressionBinder.cs
NumberLineFeedbackView.cs
```

3. Thêm `NumberLineCharacterController`:

```text
currentTick
targetTick
MoveToTick(tick)
SnapToNearestTick(position)
OnTickChanged
```

4. Thêm biểu diễn bước nhảy:

```text
Start = 3
Jumps = 4
Current = 7
Expression = 3 + 4 = 7
```

5. Thêm hint theo lỗi:

| Lỗi | Hint cần có |
|---|---|
| Kéo sai hướng | Highlight mũi tên sang phải cho cộng, sang trái cho trừ. |
| Dừng sai vị trí | Hiển thị số bước còn thiếu hoặc thừa. |
| Đếm cả điểm bắt đầu là một bước | Nhắc “vị trí bắt đầu không tính là bước nhảy”. |
| Kéo quá nhanh | Chuyển sang mode tap từng bước. |
| Không hiểu trừ | Animate nhân vật lùi từng bước và đọc số. |

6. Bổ sung metrics:

```text
startNumber
operation
stepCount
correctEnd
selectedEnd
wrongDirectionCount
overshootCount
usedHint
responseTime
interactionMode
```

