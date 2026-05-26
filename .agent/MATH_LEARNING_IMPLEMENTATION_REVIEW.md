# Đánh Giá Hệ Thống Unity Cho Trẻ Khó Học Toán

**Ngày rà soát:** 2026-05-26  
**Phạm vi:** Unity client trong `apps/unity-client`, trọng tâm là trải nghiệm học toán AR cho trẻ gặp khó khăn với số lượng, so sánh, trục số và phép tính cơ bản.  
**Cách đọc:** tài liệu này đánh giá mức đã implement, thiếu sót, và đề xuất bổ sung bài giảng/tính năng/giao diện.

## 1. Kết luận ngắn

Hệ thống hiện đã có nền tảng tương đối tốt cho MVP local-first:

- Có AR Core thật và mock trong Editor.
- Có framework học tập dùng chung.
- Có 3 hoạt động học toán: Quantity Match, Number Line Jump, Compare Quantity.
- Có config easy cho mỗi hoạt động, mỗi config khoảng 10 round.
- Có runtime UI fallback, local progress, hint, feedback hook và progress dashboard.

Tuy vậy, hệ thống vẫn chưa nên xem là sản phẩm hoàn chỉnh cho trẻ khó học toán. Phần logic và khung đã khá ổn, nhưng phần sư phạm, trải nghiệm trẻ nhỏ, accessibility, audio/VFX thật, lesson progression, kiểm thử trên thiết bị và dữ liệu học tập thích nghi còn thiếu.

Ước lượng hiện trạng theo góc nhìn sản phẩm học tập:

| Hạng mục | Mức hiện tại | Nhận xét |
|---|---:|---|
| Nền tảng kỹ thuật Unity/AR | 70-80% | Có code và scene, cần device test |
| Vertical slice Quantity Match | 75-85% | Gần playable trong Editor/mock, cần xác nhận runtime |
| Hoạt động Number Line Jump | 55-65% | Có code/config/UI fallback, còn thiếu tile/prefab/animation/polish |
| Hoạt động Compare Quantity | 60-70% | Có code/config/UI fallback, cần verify route/playtest |
| Nội dung bài học cho trẻ khó học toán | 35-45% | Có round easy nhưng chưa thành giáo trình có lộ trình |
| Giao diện trẻ nhỏ | 40-50% | Runtime UI dùng được để test, chưa tối ưu cảm xúc/khả năng tiếp cận |
| Feedback đa giác quan | 25-35% | Có hook, chưa có audio/VFX/narration thật |
| Theo dõi tiến bộ | 50-60% | Có JSON/dashboard, thiếu phân tích lỗi và gợi ý can thiệp |
| Parent/teacher mode | 10-20% | Có placeholder, chưa có trải nghiệm thật |

## 2. Hệ thống đã implement đến đâu

### 2.1 Khung học tập

Đã có:

- `ActivityPresenter` làm state machine cho activity: initialize, start round, submit answer, correct/incorrect, hint, save result.
- `ActivityConfig` làm base ScriptableObject.
- `ActivityResult`, `ActivityAnswer`, `ActivityHint`, `ActivityState`.
- `HintSystem` dùng chung cho gợi ý theo cấp độ.
- `FeedbackServiceProxy` để bắn correct/incorrect/success feedback.
- `LocalProgressStorage` lưu local JSON và tính thống kê.

Điểm tốt:

- Learning layer tách khỏi AR Foundation bằng `IARPlacementService`, `IARInteractionService`, `IARSessionService`.
- Logic đúng/sai nằm trong presenter/model, không để UI quyết định.
- Có thể thêm activity mới theo pattern presenter/view/config.

Thiếu:

- Chưa có test tự động cho `CheckAnswer`, hint escalation, progress serialization.
- `ErrorType?` trong `ActivityResult` nên được kiểm tra lại vì Unity `JsonUtility` không mạnh với nullable enum.
- Hint mới chủ yếu dựa trên cấp độ, chưa chọn hint theo lỗi cụ thể của trẻ.

### 2.2 AR và scene

Đã có:

- `ARSessionService`, `ARPlacementService`, `ARPlacementServiceMock`, `ARInteractionService`.
- `ARServiceBootstrap` để resolve service trong scene.
- `SC_TestSandbox.unity` để test spawn/tap/clear.
- `SC_ARGameplay.unity` có AR bootstrap, learning services, Quantity Match root.
- Mock placement trong Editor giúp test không cần thiết bị AR.

Thiếu:

- Chưa có bằng chứng device AR pass trong repo.
- Chưa có checklist kết quả test thực tế với ARCore/ARKit.
- Trải nghiệm phát hiện mặt phẳng, đặt vùng học, hướng dẫn trẻ giữ máy còn sơ khai.

### 2.3 Hoạt động Quantity Match

Mục tiêu học tập hiện tại: nối số viết với số lượng vật thể, đếm nhóm và chọn nhóm đúng.

Đã có:

- `QuantityMatchPresenter`, `QuantityMatchView`, `QuantityMatchRuntimeUI`.
- Config easy 10 round.
- Spawn group bằng AR placement/mock.
- Chọn group qua UI hoặc tap AR object.
- Có hint, feedback, save result.
- Có chế độ nhập số trong presenter/view.

Thiếu:

- Chưa có lesson progression rõ: từ nhận biết 1-3, subitizing, đếm một-một, đến 10.
- Chưa có audio đọc số và hướng dẫn.
- Chưa có object/prefab riêng được thiết kế cho trẻ; đang ưu tiên animal/imported resources hoặc placeholder.
- UI cần tối ưu cho trẻ có khó khăn về chú ý và xử lý thị giác.

### 2.4 Hoạt động Number Line Jump

Mục tiêu học tập hiện tại: hiểu trục số, vị trí số, nhảy trái/phải, quan hệ cộng/trừ đơn giản.

Đã có:

- `NumberLineJumpPresenter`, `NumberLineJumpView`, `NumberLineJumpRuntimeUI`.
- Config easy 10 round trong khoảng 0-10.
- Logic start number, target number, jump direction, max jumps, equation.
- Có feedback cho đúng, overshoot, boundary, max jumps.
- Router có thể tạo activity runtime khi chọn từ activity select.

Thiếu:

- `GetTilePrefab()` còn TODO, tile/prefab số đang cần fallback.
- Nhảy nhân vật chưa thật sự mượt, boundary bump còn TODO.
- Cần text/label số trên tile thật rõ và ổn định trong AR.
- Cần bài giảng dẫn dắt trước khi bắt trẻ tự nhảy trên trục số.

### 2.5 Hoạt động Compare Quantity

Mục tiêu học tập hiện tại: so sánh nhiều hơn, ít hơn, bằng nhau.

Đã có:

- `CompareQuantityPresenter`, `CompareQuantityView`, `CompareQuantityRuntimeUI`.
- Config easy 10 round.
- So sánh left/right group.
- Nút More/Fewer/Equal.
- Hint riêng cho bài bằng nhau.
- Feedback theo loại đáp án.

Thiếu:

- Chưa thấy scene root riêng được bake sẵn; chủ yếu dựa vào router/runtime creation.
- Cần đổi nhãn tiếng Việt hoặc hệ localization; hiện labels/hints chủ yếu tiếng Anh.
- Cần dạy biểu tượng `>`, `<`, `=` từng bước, chưa chỉ dừng ở More/Fewer/Equal.

### 2.6 App shell và progress

Đã có:

- `SC_MainMenu`, `SC_ActivitySelect`, `SC_ProgressDashboard`.
- Controller cho menu, chọn activity, dashboard.
- `SelectedActivityData` và `ActivityFlowNavigator`.
- Dashboard đọc thống kê local theo activity.

Thiếu:

- `SC_Boot` có tồn tại nhưng chưa nằm trong Build Settings hiện tại.
- Parent mode chưa có chức năng.
- Dashboard mới là số liệu tổng hợp; chưa giải thích trẻ đang yếu kỹ năng nào.
- Chưa có hồ sơ trẻ, nhiều trẻ, hoặc phân biệt chế độ trẻ/phụ huynh/giáo viên.

## 3. Thiếu sót chính đối với trẻ khó học toán

### 3.1 Thiếu lộ trình sư phạm đủ nhỏ

Trẻ khó học toán thường cần bước rất nhỏ, lặp lại có chủ đích và giảm tải trí nhớ làm việc. Hệ thống hiện có các round easy, nhưng chưa thành chuỗi bài học có thứ tự rõ:

```text
Nhận biết ít/nhiều
-> Đếm 1-1 với object
-> Ghép số 1-3
-> Ghép số 1-5
-> Ghép số 1-10
-> So sánh 2 nhóm
-> Dấu > < =
-> Trục số 0-5
-> Trục số 0-10
-> Cộng/trừ bằng bước nhảy
```

Cần bổ sung lesson map thay vì chỉ có danh sách round.

### 3.2 Thiếu hỗ trợ đa giác quan

Hiện có visual và text, nhưng trẻ khó học toán thường cần thêm:

- Audio đọc yêu cầu: "Hãy chọn nhóm có 3 con vật".
- Audio đọc số khi chạm object.
- Âm thanh nhẹ cho đúng/sai, không gây giật mình.
- Highlight từng object khi đếm.
- VFX đơn giản để củng cố thành công.
- Tùy chọn tắt/giảm animation cho trẻ dễ quá tải cảm giác.

### 3.3 Thiếu phân tích lỗi học tập

Hệ thống có `ErrorType`, nhưng chưa tận dụng để can thiệp sư phạm. Ví dụ:

| Lỗi | Can thiệp nên có |
|---|---|
| Chọn nhóm ít hơn target | Gợi ý đếm thêm, highlight số còn thiếu |
| Chọn nhóm nhiều hơn target | Gợi ý dừng đếm đúng target |
| Nhầm More/Fewer | Dạy so sánh bằng ghép cặp 1-1 |
| Nhầm Equal | Đưa hai nhóm thành hàng để trẻ thấy bằng nhau |
| Nhảy sai hướng | Mũi tên trái/phải lớn hơn, nhắc "số lớn ở bên phải" |
| Nhảy sai số bước | Đếm to từng bước, đánh dấu bước đã đi |

### 3.4 Nội dung chưa cá nhân hóa

Hiện chưa thấy:

- Baseline assessment để biết trẻ bắt đầu ở mức nào.
- Adaptive difficulty tăng/giảm số lượng dựa trên kết quả.
- Spaced repetition cho dạng trẻ hay sai.
- Hồ sơ trẻ và mục tiêu theo ngày/tuần.
- Cơ chế khóa/mở bài theo mastery.

### 3.5 Giao diện chưa tối ưu cho trẻ nhỏ/khó học

Runtime UI giúp test nhanh nhưng chưa phải UI cuối:

- Text còn tiếng Anh, chưa có localization tiếng Việt.
- Nút và vùng chạm cần cực lớn, nhất quán.
- Cần giảm số lựa chọn khi trẻ mới học.
- Cần trạng thái "đang đếm", "đã chọn", "thử lại" rõ hơn.
- Cần tránh quá nhiều chữ trên màn hình.
- Cần hướng dẫn bằng icon/audio thay vì chỉ text.
- AR object cần ổn định kích thước và không bị rối nền.

## 4. Tính năng nên bổ sung

### 4.1 Tính năng MVP cần ưu tiên

| Ưu tiên | Tính năng | Lý do |
|---|---|---|
| P0 | Device AR validation | Chưa thể gọi là AR learning app nếu chưa pass thiết bị |
| P0 | Build Settings thêm `SC_Boot` hoặc thống nhất entry scene | Cold-start flow cần rõ |
| P0 | Localization tiếng Việt | Đối tượng người dùng là trẻ Việt Nam |
| P0 | Audio instruction cơ bản | Trẻ khó đọc/khó tập trung cần nghe hướng dẫn |
| P0 | UI prefab/polish cho Quantity Match | Đây là vertical slice chính |
| P1 | Error-specific hints | Biến sai lầm thành can thiệp đúng |
| P1 | Lesson map và khóa/mở bài | Tạo tiến trình học thay vì chơi rời rạc |
| P1 | Parent/teacher progress summary | Người lớn cần biết trẻ yếu chỗ nào |
| P1 | Accessibility settings | Tắt animation, đổi cỡ chữ, đổi màu, giảm âm |
| P2 | EditMode/PlayMode tests | Giữ logic ổn định khi thêm bài |
| P2 | Real VFX/audio manager | Thay log-only hook |

### 4.2 Tính năng cho trẻ khó học toán

Nên thêm các cơ chế sau:

- **Guided counting:** khi trẻ chạm từng con vật, hệ thống đọc "một, hai, ba" và đánh dấu object đã đếm.
- **One-to-one pairing:** khi so sánh, kéo/ghép từng object bên trái với bên phải để thấy thừa/thiếu.
- **Subitizing mode:** nhận nhanh nhóm 1-5 không cần đếm từng cái.
- **Representational bridge:** sau AR object, hiện chấm tròn/khung 5 rồi mới hiện chữ số.
- **Symbol bridge:** More/Fewer chuyển dần thành `>`, `<`, `=`.
- **Number line scaffolding:** ban đầu hiển thị tất cả số, sau đó ẩn bớt nhãn để trẻ tự suy luận.
- **Retry without shame:** sai thì giữ nhịp nhẹ, không hiệu ứng tiêu cực mạnh.
- **Mastery badge nhỏ:** ghi nhận "đếm đến 5", "so sánh bằng nhau", "nhảy đúng 3 bước".
- **Session length limit:** buổi học 5-10 phút, nhiều nghỉ ngắn.

## 5. Bài giảng nên bổ sung

### 5.1 Lộ trình đề xuất theo kỹ năng

| Giai đoạn | Bài học | Activity phù hợp |
|---|---|---|
| 1 | Nhận biết số lượng 1-3 | Quantity Match |
| 2 | Đếm từng object 1-5 | Quantity Match guided counting |
| 3 | Ghép chữ số với số lượng 1-5 | Quantity Match |
| 4 | Ghép chữ số với số lượng 6-10 | Quantity Match |
| 5 | Nhận biết nhiều hơn/ít hơn bằng trực quan | Compare Quantity |
| 6 | So sánh bằng ghép cặp | Compare Quantity |
| 7 | Học dấu `>`, `<`, `=` | Compare Quantity + symbol overlay |
| 8 | Thứ tự số 0-10 | Number Line Jump |
| 9 | Cộng bằng nhảy sang phải | Number Line Jump |
| 10 | Trừ bằng nhảy sang trái | Number Line Jump |
| 11 | Ôn tập trộn kỹ năng | Mixed review |

### 5.2 Nội dung cần có trong mỗi bài

Mỗi bài nên có cấu trúc:

```text
1. Mục tiêu rất ngắn
2. Demo mẫu có hướng dẫn
3. 2-3 câu luyện tập có hỗ trợ nhiều
4. 3-5 câu tự làm
5. Tổng kết bằng lời khen cụ thể
6. Lưu kết quả và kỹ năng đã đạt
```

Ví dụ với Quantity Match:

- Bài 1: "Chọn nhóm có 1, 2, 3".
- Bài 2: "Đếm từng con vật rồi chọn số".
- Bài 3: "Nhìn số 4 hoặc 5, chọn nhóm đúng".
- Bài 4: "Số 6-10, nhóm xếp thành hàng/khung 5 để dễ đếm".

Ví dụ với Compare Quantity:

- Bài 1: "Nhóm nào nhiều hơn?" chỉ dùng hai lựa chọn.
- Bài 2: "Nhóm nào ít hơn?" chỉ dùng hai lựa chọn.
- Bài 3: "Hai nhóm có bằng nhau không?"
- Bài 4: Giới thiệu `>`, `<`, `=`.

Ví dụ với Number Line Jump:

- Bài 1: "Tìm số bên phải".
- Bài 2: "Tìm số bên trái".
- Bài 3: "Từ 2 nhảy 3 bước sang phải".
- Bài 4: "Từ 8 nhảy 2 bước sang trái".

## 6. Giao diện cần tối ưu

### 6.1 Nguyên tắc UI cho trẻ khó học toán

- Mỗi màn hình chỉ nên có một nhiệm vụ chính.
- Dùng chữ lớn, tương phản cao, ít câu dài.
- Ưu tiên audio/icon/animation có mục đích thay vì đoạn text hướng dẫn.
- Vùng bấm tối thiểu lớn, đặt cố định.
- Tránh nền AR quá rối: có thể thêm learning area marker hoặc nền bán trong nhẹ cho object.
- Khi trẻ sai, chỉ highlight phần cần sửa, không reset toàn bộ quá nhanh.
- Luôn có nút nghe lại yêu cầu.
- Cho phép đổi tốc độ, tắt âm, tắt animation.

### 6.2 Cải tiến cụ thể theo màn hình

| Màn hình | Cần cải tiến |
|---|---|
| Main Menu | Ít chữ, 2 nút lớn: "Học tiếp" và "Tiến bộ"; có icon |
| Activity Select | Hiển thị bài đã mở khóa, độ khó, kỹ năng; ẩn/khóa bài chưa sẵn sàng |
| AR Gameplay | Hướng dẫn đặt mặt phẳng rõ ràng; target number thật lớn; nút nghe lại |
| Quantity Match | Highlight object khi đếm; group label tiếng Việt; giảm lựa chọn ở bài đầu |
| Compare Quantity | Xếp object thành hàng dễ ghép cặp; chuyển dần từ chữ sang ký hiệu |
| Number Line Jump | Số trên tile rõ, camera-facing; mũi tên trái/phải lớn; preview đường nhảy |
| Progress Dashboard | Dùng ngôn ngữ phụ huynh: "Con đang tốt ở..." và "Nên luyện thêm..." |

### 6.3 Ngôn ngữ hiển thị

Hiện strings chủ yếu là tiếng Anh (`Great job`, `More`, `Fewer`, `Equal`). Nên bổ sung hệ localization:

| English | Vietnamese đề xuất |
|---|---|
| Quantity Match | Ghép số lượng |
| Compare Quantity | So sánh số lượng |
| Number Line Jump | Nhảy trên trục số |
| More | Nhiều hơn |
| Fewer | Ít hơn |
| Equal | Bằng nhau |
| Hint | Gợi ý |
| Try again | Thử lại nhé |
| Great job | Con làm tốt lắm |

## 7. Roadmap đề xuất

### Phase A - Cố định vertical slice cho Quantity Match

- Chạy test trong Unity Editor theo `.agent/PHASE2_TEST_GUIDE.md`.
- Sửa Build Settings hoặc quyết định entry scene chính.
- Tạo UI prefab/polish cho Quantity Match.
- Thêm audio đọc số 1-10 và audio đọc yêu cầu.
- Bổ sung guided counting.
- Verify `learning_progress.json` sau 1 session.

### Phase B - Biến 3 activity thành chuỗi học

- Thêm lesson map và trạng thái unlock/mastery.
- Cấu hình các level: 1-3, 1-5, 1-10, compare, number line.
- Disable bài chưa đủ ổn định hoặc đánh dấu "sắp có".
- Router/config cần hoạt động chắc trong build, không phụ thuộc `AssetDatabase`.

### Phase C - Hỗ trợ trẻ khó học toán sâu hơn

- Hint theo lỗi cụ thể.
- Adaptive difficulty.
- Dashboard cho phụ huynh/giáo viên.
- Accessibility settings.
- Real audio/VFX manager.

### Phase D - Kiểm thử và hoàn thiện

- EditMode tests cho answer checking.
- PlayMode tests cho flow cơ bản với mock AR.
- Device tests Android/iOS.
- Kiểm tra UI ở nhiều tỉ lệ màn hình.
- Kiểm tra với trẻ/người dùng thật nếu có điều kiện.

## 8. Rủi ro nếu tiếp tục mà không xử lý

| Rủi ro | Hậu quả |
|---|---|
| Không test device AR | Demo có thể chỉ chạy trong Editor/mock |
| Không có localization/audio | Trẻ khó đọc hoặc trẻ nhỏ khó tự học |
| Không có lesson map | Activity thành mini-game rời rạc, khó chứng minh hiệu quả học |
| UI runtime chưa polish | Trẻ dễ nhầm, phụ huynh khó tin tưởng sản phẩm |
| Không có error-specific hint | Hệ thống biết trẻ sai nhưng chưa giúp đúng chỗ |
| Không có test tự động | Dễ regress khi thêm bài và sửa scene |

## 9. Định nghĩa "đủ tốt để báo cáo MVP"

Một MVP hợp lý cho hệ thống này nên đạt:

- Mở app từ scene chính và vào được học.
- Quantity Match chạy đủ 1 session 5-10 câu trong Editor và ít nhất 1 device AR.
- Có tiếng Việt và audio hướng dẫn tối thiểu.
- Có hint 3 cấp và ít nhất một loại hint theo lỗi.
- Kết quả được lưu local và dashboard đọc được.
- Number Line Jump hoặc Compare Quantity chạy được như activity thứ hai, dù chưa cần polish bằng Quantity Match.
- Không còn lỗi đỏ compile/runtime lặp lại trong Console.
- Có ảnh/video minh chứng gameplay và file JSON kết quả.

## 10. Kết luận

Hệ thống đã có nền móng kỹ thuật tốt: AR services, scene, activity framework, ba hoạt động học, config assets, runtime UI, progress local. Phần cần đầu tư tiếp không chỉ là thêm code, mà là biến khung kỹ thuật đó thành một lộ trình học thật sự cho trẻ khó học toán: ít chữ hơn, nhiều hướng dẫn nghe/nhìn/chạm hơn, bài học chia nhỏ hơn, hint thông minh hơn, và giao diện dịu, rõ, khó bấm nhầm.
