# Kế Hoạch Tối Ưu Hóa Hệ Thống AR & UX Học Tập

Tài liệu này phân tích chi tiết các điểm hạn chế trong hệ thống AR, logic tương tác, luồng làm bài tập và giao diện hiện tại của dự án, đồng thời đề xuất giải pháp tối ưu hóa cụ thể nhằm mang lại trải nghiệm tốt nhất cho trẻ nhỏ (4-6 tuổi), đặc biệt là nhóm trẻ gặp khó khăn khi học toán (dyscalculia).

---

## 1. Tổng Quan Trạng Thái Hệ Thống AR Hiện Tại

Dự án hiện đã xây dựng được nền tảng kỹ thuật AR vững chắc bao gồm:
*   Sử dụng **AR Foundation 6.x** kết hợp với **XR Interaction Toolkit (XRI)** để quản lý cảm biến, mặt phẳng và tương tác.
*   Cơ chế **Mock Services** (`ARPlacementServiceMock`) cho phép giả lập không gian AR trong Unity Editor, giúp rút ngắn thời gian phát triển và thử nghiệm.
*   Thiết kế kiến trúc lỏng lẻo (**Decoupled Architecture**): Layer logic học tập (`ActivityPresenter`) không phụ thuộc trực tiếp vào AR Foundation mà tương tác qua các Interface (`IARPlacementService`, `IARInteractionService`).
*   Đã tích hợp các visual cues cơ bản như vòng tròn chỉ định dưới đất (`GroupAreaIndicator`) và nhãn số lơ lửng (`BillboardBehavior`).

---

## 2. Phân Tích Chi Tiết & Các Điểm Cần Cải Thiện

### A. Logic AR & Thuật Toán Vị Trí (AR Placement & Stability)

#### 1. Hiện tượng trôi lệch tọa độ (Drift) & Spawning trực tiếp theo World Space
*   **Vấn đề:** Trong [QuantityMatchPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/QuantityMatch/Scripts/QuantityMatchPresenter.cs), các vị trí nhóm vật thể được tính toán bằng cách cộng một khoảng offset trực tiếp vào tọa độ tuyệt đối `placementService.CurrentPlacementPosition`. Nếu camera thiết bị bị trôi lệch (drift) hoặc dịch chuyển trong lúc làm bài, các nhóm vật thể của round mới sẽ bị spawn lệch góc, bay lơ lửng hoặc chìm dưới mặt đất.
*   **Giải pháp tối ưu:** 
    *   Tận dụng lớp `LearningAreaAnchor` làm điểm gốc (Pivot) duy nhất cố định trong không gian thực tế sau khi quét phòng.
    *   Tất cả các vật thể game (nhóm con vật, trục số, nhãn) của mọi round phải được spawn làm **con trực tiếp** của `LearningAreaAnchor.ContentRoot` và sử dụng tọa độ cục bộ (Local Position) thay vì tọa độ thế giới (World Position). Nếu Anchor được hiệu chỉnh hoặc dịch chuyển, toàn bộ bài tập sẽ di chuyển đồng bộ theo mà không bị vỡ layout.

#### 2. Thiếu bước hiệu chuẩn không gian trước khi chơi (Calibration Phase)
*   **Vấn đề:** Hiện tại khi cảnh `SC_ARGameplay` tải, presenter lập tức cố gắng chạy round 1. Trên thiết bị di động thật, AR Foundation cần vài giây để nhận diện mặt phẳng. Nếu chưa có mặt phẳng, tọa độ placement sẽ bị sai hoặc không khả dụng.
*   **Giải pháp tối ưu:** Thiết lập một luồng khởi tạo chặt chẽ qua 4 bước (State Machine):
    1.  **Trạng thái Quét mặt phẳng (Plane Scanning):** Hiển thị màn hình mờ với hướng dẫn hoạt họa dễ thương (VD: Bàn tay di chuyển điện thoại) và tiếng Việt: *"Con hãy đưa điện thoại xung quanh phòng để tìm sàn nhà hoặc mặt bàn nhé!"*.
    2.  **Trạng thái Phát hiện mặt phẳng (Plane Found):** Khi phát hiện mặt phẳng có diện tích đủ lớn (`minimumPlaneArea >= 0.15f`), hiển thị một Marker 3D hoạt họa lấp lánh trên sàn. Hướng dẫn bé: *"Tìm thấy bàn học phép thuật rồi! Con hãy chạm vào vòng tròn màu xanh để bắt đầu học nhé!"*.
    3.  **Trạng thái Đặt Anchor (Anchor Placed):** Bé chạm vào màn hình -> Gọi `PlaceLearningArea()` tại điểm chạm -> Spawn `LearningAreaAnchor` và ẩn các lưới quét mặt phẳng phụ để tránh rối mắt.
    4.  **Trạng thái Bắt đầu học (Game Start):** Kích hoạt Presenter tải Round 1 relative với Anchor vừa đặt.

---

### B. Logic Tương Tác & Va Chạm (AR Interaction & Hitboxes)

#### 1. Cơ chế chạm để Đếm (Tap-to-count) bị đè lấn bởi chạm để Nộp bài (Tap-to-submit)
*   **Vấn đề:** Trong `QuantityMatchPresenter.cs`, khi bé chạm vào bất kỳ con vật nào trong nhóm, sự kiện `OnObjectTapped` sẽ kích hoạt `HandleGroupSelected` và **lập tức nộp câu trả lời** của nhóm đó. Trẻ 4-6 tuổi học toán thường có nhu cầu chạm tay vào từng con vật để đếm nhẩm ("Một, hai, ba..."). Việc chạm vào con vật mà bị nộp bài ngay lập tức là một lỗi UX cực kỳ nghiêm trọng, khiến trẻ liên tục bị nộp sai do chạm nhầm.
*   **Giải pháp tối ưu:** 
    *   **Tách biệt hành vi Đếm và Chọn:** Khi chạm vào từng con vật đơn lẻ trong nhóm, hệ thống **không** nộp bài. Thay vào đó, kích hoạt hiệu ứng đếm: con vật nhảy lên nhẹ (bounce), đổi màu nhẹ hoặc xuất hiện một ngôi sao nhỏ trên đầu, đồng thời phát âm thanh đếm số tương ứng ("Một!", "Hai!", "Ba!").
    *   **Cơ chế Xác nhận nộp bài (Confirmation):** Để chọn nhóm đó làm đáp án, trẻ phải chạm vào **Nhãn số lơ lửng (Pill Label)** khổng lồ phía trên nhóm hoặc bấm vào các thẻ số UI tương ứng ở nửa dưới màn hình.

#### 2. Vô hiệu hóa tương tác AR trong chế độ Nhập số (Number Input Mode)
*   **Vấn đề:** Ở các round từ 6 đến 10, chế độ nhập số được kích hoạt (`currentUsesNumberInputMode = true`). Tuy nhiên, trong code hiện tại, khi chế độ này bật thì việc chạm vào vật thể AR bị vô hiệu hóa hoàn toàn (`if (currentUsesNumberInputMode) return;` trong `HandleObjectTapped`). Trẻ bắt buộc phải đếm bằng mắt (rất khó đối với trẻ dyscalculia) rồi bấm số trên UI.
*   **Giải pháp tối ưu:** Vẫn cho phép bé chạm vào các con vật AR để kích hoạt hiệu ứng "Chạm đếm trực quan" (Interactive Touch-Counting). Bé chạm đến đâu con vật phát sáng và đếm số đến đó. Sau khi đếm xong, bé sẽ nhìn vào các nút số UI ở dưới để chọn số trùng với kết quả đếm được. Điều này giúp củng cố mối liên kết giữa số lượng thực tế và ký hiệu số số học.

---

### C. Luồng Làm Bài Tập & Tính Năng Sư Phạm (Learning Flow & Pedagogical Scaffolding)

#### 1. Thiếu giáo trình chia nhỏ độ khó (Scaffolded Lesson Progression)
*   **Vấn đề:** Trẻ khó học toán cần các bước tiến rất nhỏ và lặp đi lặp lại để giảm tải trí nhớ làm việc (working memory). Hiện tại hệ thống chỉ có một file cấu hình chung `SO_QuantityMatchConfig_Easy.asset` chứa các round ngẫu nhiên từ 2 đến 10.
*   **Giải pháp tối ưu:** Chia nhỏ tiến trình thành các Lesson Map rõ ràng:
    *   *Bài 1 (Subitizing):* Nhận biết nhanh không cần đếm trong phạm vi 1-3.
    *   *Bài 2 (One-to-one counting):* Đếm phạm vi 1-5, vật thể xếp hàng ngang cố định.
    *   *Bài 3 (Quantity Match 1-5):* Chọn nhóm có số lượng tương ứng phạm vi 1-5.
    *   *Bài 4 (Quantity Match 6-10):* Đếm phạm vi 6-10, các con vật được xếp theo khung 5 (Ten-frame) để trẻ dễ gom nhóm trực quan.
    *   *Bài 5 (Number Input):* Đếm và tự điền số phạm vi 1-10.

#### 2. Chưa khai thác dữ liệu loại lỗi (`ErrorType`) để đưa ra gợi ý sư phạm thích ứng
*   **Vấn đề:** Mặc dùPresenter đã phân loại được lỗi (`WrongQuantity`, `WrongComparison`, `WrongJumpCount`), nhưng hệ thống Hint (`HintSystem`) hiện tại chỉ hiển thị gợi ý leo thang theo cấp độ (Level 1, Level 2, Level 3) chung chung bằng chữ, chứ chưa đưa ra gợi ý can thiệp sư phạm dựa trên lỗi cụ thể của trẻ.
*   **Giải pháp tối ưu:** Xây dựng cơ chế **Contextual Hint (Gợi ý theo ngữ cảnh lỗi)**:
    *   *Lỗi chọn nhóm ít hơn mục tiêu:* Gợi ý nhảy âm thanh hoặc highlight các vị trí còn trống để gợi ý bé đếm thêm.
    *   *Lỗi chọn nhóm nhiều hơn mục tiêu:* Gợi ý vòng tròn đỏ xung quanh các con vật thừa, nhắc bé dừng lại khi đạt đủ số lượng mục tiêu.
    *   *Lỗi trục số (Number Line Jump) nhảy sai hướng hoặc nhảy quá đà:* Vẽ một mũi tên nét đứt màu xanh chỉ hướng đúng trên trục số AR, hoặc highlight tile số đích cần nhảy tới.

---

### D. Giao Diện & Trực Quan Hóa (UI/UX & Aesthetics)

#### 1. Nhãn 3D bị lệch góc nhìn khi di chuyển camera (Static Billboard Bug)
*   **Vấn đề:** Nhãn của các nhóm vật thể trong [CompareQuantityPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/CompareQuantity/Scripts/CompareQuantityPresenter.cs) và [NumberLineJumpPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/NumberLineJump/Scripts/NumberLineJumpPresenter.cs) chỉ được xoay hướng về camera một lần duy nhất lúc khởi tạo (sử dụng `Quaternion.LookRotation` trong `AddNumberLabel`). Khi bé di chuyển điện thoại xung quanh bàn để nhìn rõ hơn, các nhãn này sẽ bị ngược hoặc méo mó không đọc được.
*   **Giải pháp tối ưu:** Đồng bộ hóa tất cả các nhãn 3D trong AR bằng cách gắn thêm component [BillboardBehavior](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/QuantityMatch/Scripts/QuantityMatchPresenter.cs#L846) (được khai báo tại cuối tệp [QuantityMatchPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/QuantityMatch/Scripts/QuantityMatchPresenter.cs)) để chúng liên tục tự động quay về phía camera chính trong hàm `Update()`.

#### 2. Các nhãn văn bản cứng bằng tiếng Anh (Hardcoded English Strings)
*   **Vấn đề:** Trong `CompareQuantityPresenter.cs`, nhãn nhóm được gán cứng là `"Left Group"` và `"Right Group"`. Đối tượng sử dụng là trẻ em Việt Nam từ 4-6 tuổi không thể hiểu các từ tiếng Anh này.
*   **Giải pháp tối ưu:** 
    *   Thay thế toàn bộ chuỗi text cứng bằng tiếng Việt: `"Nhóm bên trái"`, `"Nhóm bên phải"`.
    *   Hoặc tốt hơn, loại bỏ hoàn toàn chữ viết trên nhãn AR. Thay thế bằng các biểu tượng màu sắc hoặc hình học trực quan (VD: Hình tròn màu xanh lam đại diện cho nhóm trái, hình tam giác màu cam đại diện cho nhóm phải).

#### 3. Sử dụng mô hình hình khối thô (Primitive Cubes) cho Trục số
*   **Vấn đề:** Trục số trong `NumberLineJumpPresenter.cs` đang được tạo tự động bằng các khối Cube xám xịt từ `GameObject.CreatePrimitive(PrimitiveType.Cube)`. Giao diện này quá thô sơ, tạo cảm giác nhàm chán và học thuật nặng nề đối với trẻ nhỏ.
*   **Giải pháp tối ưu:** 
    *   Thay thế khối Cube bằng một Prefab gạch đá/bậc thang được thiết kế đẹp mắt (VD: Lá sen nổi trên mặt nước cho nhân vật ếch nhảy, hoặc các khúc gỗ tròn trong rừng, hoặc những đám mây nhỏ bồng bềnh).
    *   Nhân vật nhảy nên là một prefab 3D động vật dễ thương có hoạt ảnh nhảy (Hop animation) thay vì một khối cầu tròn (`PrimitiveType.Sphere`) màu xanh.

#### 4. Trực quan hóa lưới quét mặt phẳng (Plane Visualizer) thô cứng
*   **Vấn đề:** Lưới quét mặt phẳng mặc định của AR Foundation thường là các đường lưới tam giác màu vàng hoặc đỏ gây cảm giác kỹ thuật, rối mắt và đáng sợ đối với trẻ nhỏ.
*   **Giải pháp tối ưu:** Thay đổi vật liệu (Material) của `ARFeatheredPlaneMeshVisualizer`:
    *   Sử dụng texture các đốm sáng lấp lánh (sparkles) bán trong suốt hoặc kết cấu bãi cỏ xanh mềm mại mờ.
    *   Khi mặt phẳng đã được xác định và đặt bàn học xong, lập tức ẩn hoặc làm mờ tối đa các mặt phẳng xung quanh để bé tập trung hoàn toàn vào vùng học tập AR.

---

## 3. Kế Hoạch Hành Động & Đề Xuất Sửa Đổi Mã Nguồn

Dưới đây là bảng phân công các tệp tin cần sửa đổi để thực hiện tối ưu hóa hệ thống AR:

| Tệp tin cần sửa đổi | Loại thay đổi | Nội dung chi tiết cần thực hiện |
| :--- | :---: | :--- |
| [IARPlacementService.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Core/Learning/ActivityRunner/IARPlacementService.cs) | **MODIFY** | Bổ sung định nghĩa quản lý và lấy tham chiếu tới `LearningAreaAnchor` đã được đặt. |
| [ARPlacementService.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Core/AR/Placement/ARPlacementService.cs) | **MODIFY** | Thực thi cơ chế đặt Anchor không gian thật và chuyển đổi các hàm `SpawnAtPosition` sang dạng hỗ trợ nhận Parent Transform là Anchor ContentRoot. |
| [QuantityMatchPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/QuantityMatch/Scripts/QuantityMatchPresenter.cs) | **MODIFY** | 1. Thay đổi logic `HandleObjectTapped` để không nộp bài khi chạm con vật mà chỉ chạy hiệu ứng đếm số.<br>2. Kích hoạt chạm đếm trong `currentUsesNumberInputMode`. <br>3. Chuyển đổi tính toán vị trí group về tọa độ Local so với Anchor. |
| [CompareQuantityPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/CompareQuantity/Scripts/CompareQuantityPresenter.cs) | **MODIFY** | 1. Việt hóa nhãn `"Nhóm bên trái"`, `"Nhóm bên phải"`.<br>2. Gắn thêm component `BillboardBehavior` vào nhãn 3D.<br>3. Chuyển đổi spawn về tọa độ Local so với Anchor. |
| [NumberLineJumpPresenter.cs](file:///d:/.Kỳ%20II%20năm%20Ba/Chuyên%20đề/BTL/apps/unity-client/Assets/Features/Activities/NumberLineJump/Scripts/NumberLineJumpPresenter.cs) | **MODIFY** | 1. Gắn thêm component `BillboardBehavior` cho nhãn số trên trục.<br>2. Chuẩn bị nạp Prefab gạch/lá sen nghệ thuật thay thế khối Cube mặc định. |

---

## 4. Định Nghĩa Tiêu Chuẩn "Hệ Thống AR Đạt Chuẩn Trải Nghiệm Trẻ Nhỏ"

Để ứng dụng AR có thể được báo cáo và nghiệm thu thành công với chất lượng premium, hệ thống AR phải đạt được các chỉ số trải nghiệm sau:

1.  **Ổn định không gian:** Không có hiện tượng các con vật bị trượt đi khi trẻ quay điện thoại sang hướng khác. Khoảng cách giữa các nhóm phải đủ rộng để trẻ không nhìn lẫn lộn giữa nhóm này với nhóm kia.
2.  **Không có chữ rác (Zero technical clutter):** Không hiện các thông số debug hệ thống, không hiện đường lưới quét màu sắc sặc sỡ đè lên động vật. Chỉ hiện camera thực tế, động vật sắc nét, nhãn số to rõ ràng và UI phẳng tối giản.
3.  **Tốc độ tương tác (Responsiveness):** Trẻ chạm vào con vật là phải có phản hồi tức thì (âm thanh đếm kêu lên ngay, con vật nhảy lên ngay lập tức). Tránh độ trễ khiến trẻ tưởng máy đơ rồi chạm liên tục tạo ra nhiều lệnh chạm sai.
4.  **Bảo vệ mắt và khoảng cách:** Có cảnh báo bằng tiếng Việt nếu trẻ đưa camera quá sát vật thể (VD: *"Con đứng lùi ra xa một chút để nhìn rõ các bạn thú hơn nhé!"*).
