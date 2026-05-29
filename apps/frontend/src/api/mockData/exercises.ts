/**
 * Mock Exercises Data
 * CPA exercises for Dyscalculia children (4-6 years old)
 */

import type { Exercise } from '../services/exerciseService';

export const mockExercises: Exercise[] = [
  // ============ CONCRETE STAGE ============
  {
    id: 'cpa-001',
    title: 'Đếm đồ vật 1-5',
    description: 'Trẻ học đếm từ 1 đến 5 bằng đồ vật thật',
    duration: 10,
    cpaStage: 'concrete',
    mathType: 'counting',
    numberRange: { min: 1, max: 5 },
    difficulty: 1,
    concrete: {
      instructions: 'Cho con 5 khối gỗ hoặc đồ vật nhỏ. Yêu cầu con chạm vào từng đồ vật và đếm to từ 1 đến 5.',
      materials: ['Khối gỗ', 'Nút chai', 'Kẹo', 'Trái cây', 'Đồ chơi nhỏ'],
      examples: [
        'Con có 3 khối gỗ. Đếm xem con có mấy khối? 1... 2... 3...',
        'Mẹ có 4 quả táo. Con đếm giúp mẹ nhé!',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình tròn trên giấy. Yêu cầu con đếm và khoanh tròn từng hình.',
      visualTypes: ['hinh-tron', 'hinh-vuong'],
    },
    abstract: {
      instructions: 'Chỉ vào số trên thẻ và yêu cầu con nhận diện.',
      examples: ['Số 3', 'Số 5', 'Số 1'],
      warnings: ['Đừng vội chuyển sang giai đoạn trừu tượng nếu con chưa vững ở giai đoạn cụ thể'],
    },
    parentTips: [
      'Đừng vội vàng, để con đếm theo tốc độ của mình',
      'Khen ngợi khi con đếm đúng, không chê khi sai',
      'Thực hành mỗi ngày 5-10 phút',
      'Dùng đồ vật con thích (xe hơi, búp bê) để tăng hứng thú',
    ],
    commonMistakes: [
      'Trẻ đếm nhảy số (1, 3, 5 thay vì 1, 2, 3)',
      'Trẻ chỉ đếm 1, 2 rồi dừng',
      'Trẻ đếm quá nhanh không chạm vào đồ vật',
    ],
    warningSigns: [
      'Con không thể đếm đến 3 sau 2-3 tuần luyện tập',
      'Con cần dùng ngón tay để đếm (có thể bình thường ở độ tuổi này)',
      'Con không nhận biết được 1 và 2 là gì',
    ],
    prerequisites: [],
    nextExercises: ['cpa-002'],
    tags: ['đếm', 'số nhỏ', 'dyscalculia-friendly', 'khởi đầu'],
  },
  {
    id: 'cpa-002',
    title: 'Đếm đồ vật 6-10',
    description: 'Trẻ học đếm từ 6 đến 10 bằng đồ vật thật',
    duration: 10,
    cpaStage: 'concrete',
    mathType: 'counting',
    numberRange: { min: 6, max: 10 },
    difficulty: 1,
    concrete: {
      instructions: 'Cho con 10 đồ vật. Yêu cầu con đếm từ 1 đến 10, chạm vào từng đồ vật khi đếm.',
      materials: ['Khối gỗ', 'Nút chai', 'Kẹo', 'Trái cây'],
      examples: [
        'Con có 7 chiếc xe. Đếm xem đúng không nhé!',
        'Mẹ xếp 8 quả nho. Con đếm giúp mẹ!',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình trên giấy và yêu cầu con đếm.',
      visualTypes: ['hinh-tron', 'hinh-vuong', 'hinh-tam-giac'],
    },
    abstract: {
      instructions: 'Yêu cầu con nhận diện số 6-10.',
      examples: ['Số 7', 'Số 9', 'Số 10'],
      warnings: ['Đảm bảo con đã vững đếm 1-5 trước'],
    },
    parentTips: [
      'Chia làm 2 nhóm: 1-5 và 6-10 để con dễ tiếp thu',
      'Dùng đồ vật màu sắc khác nhau để phân biệt',
      'Kết hợp trò chơi xếp hàng',
    ],
    commonMistakes: [
      'Trẻ quên đếm số 7 hoặc 9',
      'Trẻ nhầm thứ tự 6 và 9',
    ],
    warningSigns: [
      'Con không nhớ được thứ tự đếm',
      'Con cần đếm lại từ 1 mỗi lần',
    ],
    prerequisites: ['cpa-001'],
    nextExercises: ['cpa-003'],
    tags: ['đếm', 'số nhỏ', 'dyscalculia-friendly'],
  },
  {
    id: 'cpa-003',
    title: 'Đếm lùi từ 5 về 1',
    description: 'Trẻ học đếm ngược - kỹ năng quan trọng cho phép trừ',
    duration: 8,
    cpaStage: 'concrete',
    mathType: 'counting',
    numberRange: { min: 1, max: 5 },
    difficulty: 2,
    concrete: {
      instructions: 'Xếp 5 đồ vật. Yêu cầu con đếm to, sau đó bỏ đi 1 và đếm lùi.',
      materials: ['Khối gỗ', 'Nút chai', 'Kẹo'],
      examples: [
        'Con có 5 cái kẹo. Con ăn 1 cái. Còn mấy cái?',
        'Mẹ có 4 quả bóng. Cho em 1 quả. Mẹ còn mấy quả?',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình và gạch bỏ khi đếm lùi.',
      visualTypes: ['hinh-tron'],
    },
    abstract: {
      instructions: 'Yêu cầu con đếm lùi không cần đồ vật.',
      examples: ['5, 4, 3, 2, 1', '4, 3, 2, 1'],
      warnings: ['Bắt đầu từ số nhỏ trước (1-5)'],
    },
    parentTips: [
      'Kết hợp với trò chơi "bớt đi" để con hiểu bản chất',
      'Dùng ngón tay để minh họa',
      'Đếm lùi qua nhịp vỗ tay',
    ],
    commonMistakes: [
      'Trẻ đếm xuôi thay vì ngược',
      'Trẻ bỏ qua một số',
    ],
    warningSigns: [
      'Con không hiểu "bớt đi" nghĩa là gì',
      'Con cần rất lâu để đếm lùi',
    ],
    prerequisites: ['cpa-001'],
    nextExercises: ['cpa-004'],
    tags: ['đếm', 'đếm lùi', 'chuẩn bị-trừ'],
  },
  {
    id: 'cpa-010',
    title: 'So sánh nhiều hơn - ít hơn (1-5)',
    description: 'Trẻ học so sánh hai nhóm đồ vật',
    duration: 12,
    cpaStage: 'concrete',
    mathType: 'comparison',
    numberRange: { min: 1, max: 5 },
    difficulty: 1,
    concrete: {
      instructions: 'Cho con 2 nhóm đồ vật khác nhau. Yêu cầu con đếm từng nhóm rồi so sánh.',
      materials: ['Khối gỗ', 'Trái cây', 'Kẹo', 'Nút chai'],
      examples: [
        'Con có 3 quả táo và 5 quả nho. Nhóm nào nhiều hơn?',
        'Mẹ có 4 cái bánh, con có 2 cái. Ai có nhiều hơn?',
      ],
    },
    pictorial: {
      instructions: 'Vẽ 2 nhóm hình khác nhau trên giấy.',
      visualTypes: ['hinh-tron', 'hinh-vuong'],
    },
    abstract: {
      instructions: 'Dùng ký hiệu >, <, = để so sánh số.',
      examples: ['3 > 1', '2 < 5', '4 = 4'],
      warnings: ['Chỉ chuyển sang ký hiệu khi con đã hiểu bản chất'],
    },
    parentTips: [
      'Dùng từ ngữ đơn giản: "nhiều hơn", "ít hơn", "bằng nhau"',
      'Cho con tự sắp xếp để so sánh',
      'Tránh dùng ký hiệu >, < quá sớm',
    ],
    commonMistakes: [
      'Trẻ nhầm "nhiều hơn" thành "ít hơn"',
      'Trẻ chỉ nhìn kích thước thay vì đếm số lượng',
    ],
    warningSigns: [
      'Con không hiểu ý nghĩa của "nhiều hơn"',
      'Con luôn nói nhóm đầu tiên nhiều hơn',
    ],
    prerequisites: ['cpa-001'],
    nextExercises: ['cpa-011'],
    tags: ['so sánh', 'nhiều hơn', 'ít hơn'],
  },
  {
    id: 'cpa-011',
    title: 'So sánh nhiều hơn - ít hơn (1-10)',
    description: 'So sánh hai nhóm đồ vật với số lượng lớn hơn',
    duration: 12,
    cpaStage: 'concrete',
    mathType: 'comparison',
    numberRange: { min: 1, max: 10 },
    difficulty: 2,
    concrete: {
      instructions: 'Cho con 2 nhóm đồ vật với số lượng 1-10. Yêu cầu con đếm và so sánh.',
      materials: ['Khối gỗ', 'Trái cây', 'Kẹo'],
      examples: [
        'Con có 8 viên kẹo, mẹ có 6 viên. Ai có nhiều hơn?',
        'Bạn A có 7 cái bút, bạn B có 9 cái. Ai có ít hơn?',
      ],
    },
    pictorial: {
      instructions: 'Vẽ 2 nhóm hình để so sánh.',
      visualTypes: ['hinh-tron'],
    },
    abstract: {
      instructions: 'So sánh 2 số bằng ký hiệu.',
      examples: ['8 > 6', '4 < 9', '5 = 5'],
      warnings: ['Đảm bảo con vững so sánh 1-5 trước'],
    },
    parentTips: [
      'Xếp 2 hàng song song để con dễ so sánh bằng mắt',
      'Dùng que tính hoặc đũa để xếp hàng',
      'Cho con tự tạo 2 nhóm và so sánh',
    ],
    commonMistakes: [
      'Trẻ bị confuse khi số lượng gần nhau (7 vs 8)',
      'Trẻ đếm sai dẫn đến so sánh sai',
    ],
    warningSigns: [
      'Con vẫn không phân biệt được nhiều hơn/ít hơn',
    ],
    prerequisites: ['cpa-010'],
    nextExercises: ['cpa-012'],
    tags: ['so sánh', 'nhiều hơn', 'ít hơn'],
  },
  // ============ PICTORIAL STAGE ============
  {
    id: 'cpa-020',
    title: 'Ghép số với hình ảnh (1-5)',
    description: 'Trẻ học ghép số với số lượng hình tương ứng',
    duration: 10,
    cpaStage: 'pictorial',
    mathType: 'counting',
    numberRange: { min: 1, max: 5 },
    difficulty: 1,
    concrete: {
      instructions: 'Đặt thẻ số và đồ vật. Yêu cầu con đếm đồ vật và chỉ vào số tương ứng.',
      materials: ['Thẻ số', 'Đồ vật nhỏ'],
      examples: [
        'Có 3 khối gỗ. Con tìm số 3!',
        'Con đếm 4 quả táo. Số nào là 4?',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình trên giấy. Yêu cầu con khoanh tròn số đúng.',
      visualTypes: ['hinh-tron', 'hinh-vuong', 'ghep-so'],
    },
    abstract: {
      instructions: 'Chỉ vào số, yêu cầu con vẽ số lượng hình tương ứng.',
      examples: ['Vẽ 3 hình tròn', 'Vẽ 5 hình vuông'],
      warnings: ['Giai đoạn này vẫn cần hình ảnh, chưa cần số thuần túy'],
    },
    parentTips: [
      'Bắt đầu với thẻ số rõ ràng, font to',
      'Dùng hình ảnh có màu sắc thu hút',
      'Khen ngợi khi con ghép đúng',
    ],
    commonMistakes: [
      'Trẻ đếm hình nhưng chọn sai số',
      'Trẻ nhầm số 6 và 9',
    ],
    warningSigns: [
      'Con không nhận dạng được số 1-5',
      'Con cần đếm lại từ đầu mỗi lần',
    ],
    prerequisites: ['cpa-001'],
    nextExercises: ['cpa-021'],
    tags: ['ghép số', 'nhận diện số', 'hình ảnh'],
  },
  {
    id: 'cpa-021',
    title: 'Ghép số với hình ảnh (1-10)',
    description: 'Mở rộng ghép số lên 10',
    duration: 12,
    cpaStage: 'pictorial',
    mathType: 'counting',
    numberRange: { min: 1, max: 10 },
    difficulty: 2,
    concrete: {
      instructions: 'Tương tự bài 1-5 nhưng mở rộng đến 10.',
      materials: ['Thẻ số', 'Đồ vật nhỏ'],
      examples: [
        'Có 7 khối. Con tìm số 7!',
        'Đếm 9 hình tròn và chỉ vào số 9.',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình và yêu cầu con khoanh số đúng.',
      visualTypes: ['hinh-tron', 'hinh-vuong'],
    },
    abstract: {
      instructions: 'Ghép số với hình ảnh đã vẽ.',
      examples: ['Vẽ 7 hình tam giác', 'Đếm 9 ngôi sao'],
      warnings: ['Trẻ có thể nhầm 6 và 9, 2 và 5'],
    },
    parentTips: [
      'Dùng hình ảnh rõ ràng, cách nhau để đếm',
      'Có thể dùng sticker hoặc tem để dán',
      'Thực hành đều đặn mỗi ngày',
    ],
    commonMistakes: [
      'Trẻ nhầm 6 và 9',
      'Trẻ đếm thiếu hoặc thừa',
    ],
    warningSigns: [
      'Con không thể đếm chính xác đến 10',
    ],
    prerequisites: ['cpa-020'],
    nextExercises: ['cpa-022'],
    tags: ['ghép số', 'nhận diện số', '1-10'],
  },
  {
    id: 'cpa-030',
    title: 'Minh họa phép cộng bằng hình ảnh',
    description: 'Trẻ học cộng qua hình ảnh (3 + 2 = ?)',
    duration: 15,
    cpaStage: 'pictorial',
    mathType: 'addition',
    numberRange: { min: 1, max: 10 },
    difficulty: 2,
    concrete: {
      instructions: 'Dùng đồ vật để minh họa phép cộng trước khi chuyển sang hình.',
      materials: ['Khối gỗ', 'Trái cây', 'Kẹo'],
      examples: [
        'Con có 3 khối gỗ. Mẹ cho thêm 2 khối nữa. Con có tất cả mấy khối?',
        'Bạn A có 2 quả bóng, được tặng thêm 3 quả. Bạn A có mấy quả bóng?',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình để minh họa phép cộng. Dùng que tính hoặc vẽ hình tròn.',
      visualTypes: ['que-tinh', 'hinh-tron', 'so-do'],
    },
    abstract: {
      instructions: 'Viết phép cộng: 3 + 2 = ?',
      examples: ['2 + 3 = ?', '4 + 1 = ?', '1 + 4 = ?'],
      warnings: ['Chỉ chuyển sang số thuần túy khi con đã vững với hình ảnh'],
    },
    parentTips: [
      'Bắt đầu với số nhỏ (1+1, 1+2, 2+1)',
      'Luôn dùng đồ vật hoặc hình ảnh trước',
      'Giải thích: "cộng" nghĩa là "thêm vào"',
    ],
    commonMistakes: [
      'Trẻ không hiểu "thêm" nghĩa là gì',
      'Trẻ đếm lại từ đầu thay vì đếm tiếp',
      'Trẻ đếm thiếu khi đếm nhanh',
    ],
    warningSigns: [
      'Con không hiểu tại sao 3 + 2 = 5',
      'Con cần rất lâu để làm bài',
    ],
    prerequisites: ['cpa-020', 'cpa-010'],
    nextExercises: ['cpa-031'],
    tags: ['cộng', 'hình ảnh', 'số nhỏ'],
  },
  // ============ ABSTRACT STAGE ============
  {
    id: 'cpa-040',
    title: 'Nhận diện số 1-10',
    description: 'Trẻ nhận diện và viết các số từ 1 đến 10',
    duration: 15,
    cpaStage: 'abstract',
    mathType: 'counting',
    numberRange: { min: 1, max: 10 },
    difficulty: 1,
    concrete: {
      instructions: 'Đã vững ở giai đoạn Concrete và Pictorial. Bây giờ chỉ dùng số.',
      materials: ['Thẻ số', 'Giấy', 'Bút'],
      examples: [
        'Chỉ vào số và hỏi: "Đây là số mấy?"',
        '"Con viết số 5 đi!"',
      ],
    },
    pictorial: {
      instructions: 'Vẫn có thể dùng hình ảnh nhưng giảm dần.',
      visualTypes: ['thẻ số'],
    },
    abstract: {
      instructions: 'Yêu cầu con nhận diện và viết số 1-10.',
      examples: ['Nhận diện: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10', 'Viết số theo thứ tự'],
      warnings: ['Nếu con quên, quay lại giai đoạn Pictorial ngay'],
    },
    parentTips: [
      'Dùng thẻ số nhiều màu sắc',
      'Viết số trên giấy, trên cát, trên bột',
      'Đọc to số khi con viết',
    ],
    commonMistakes: [
      'Trẻ viết ngược số (3 thành E)',
      'Trẻ nhầm 6 và 9',
      'Trẻ không nhớ hình dạng số',
    ],
    warningSigns: [
      'Con không thể viết bất kỳ số nào sau nhiều lần luyện',
      'Con quên ngay sau khi học',
    ],
    prerequisites: ['cpa-021'],
    nextExercises: ['cpa-041'],
    tags: ['nhận diện số', 'viết số', '1-10'],
  },
  {
    id: 'cpa-041',
    title: 'Phép cộng với số (1-5)',
    description: 'Trẻ làm phép cộng đơn giản với số',
    duration: 15,
    cpaStage: 'abstract',
    mathType: 'addition',
    numberRange: { min: 1, max: 5 },
    difficulty: 2,
    concrete: {
      instructions: 'Nếu con gặp khó khăn, quay lại dùng đồ vật.',
      materials: ['Khối gỗ', 'Ngón tay'],
      examples: [
        'Dùng ngón tay: 2 ngón + 3 ngón = 5 ngón',
      ],
    },
    pictorial: {
      instructions: 'Vẽ que tính hoặc hình tròn để minh họa.',
      visualTypes: ['que-tinh', 'hinh-tron'],
    },
    abstract: {
      instructions: 'Giải các phép cộng: 1 + 1, 1 + 2, 2 + 1, ...',
      examples: ['1 + 1 = 2', '2 + 2 = 4', '3 + 1 = 4', '2 + 3 = 5'],
      warnings: ['Không ép con thuộc công thức, hãy để con hiểu bản chất'],
    },
    parentTips: [
      'Bắt đầu với 1+1, sau đó tăng dần',
      'Dùng ngón tay như công cụ hỗ trợ',
      'Khen ngợi quá trình, không chỉ kết quả',
    ],
    commonMistakes: [
      'Trẻ đếm sai khi cộng',
      'Trẻ không hiểu dấu + có nghĩa là gì',
    ],
    warningSigns: [
      'Con luôn cần đồ vật để làm phép cộng (có thể bình thường)',
      'Con không nhớ kết quả cộng đến 5',
    ],
    prerequisites: ['cpa-030', 'cpa-040'],
    nextExercises: ['cpa-042'],
    tags: ['cộng', 'số nhỏ', '1-5'],
  },
  {
    id: 'cpa-042',
    title: 'Phép cộng với số (1-10)',
    description: 'Mở rộng phép cộng lên đến 10',
    duration: 15,
    cpaStage: 'abstract',
    mathType: 'addition',
    numberRange: { min: 1, max: 10 },
    difficulty: 2,
    concrete: {
      instructions: 'Dùng đồ vật khi cần thiết.',
      materials: ['Khối gỗ', 'Que tính'],
      examples: [
        '6 khối + 3 khối = 9 khối',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình để minh họa.',
      visualTypes: ['que-tinh', 'hinh-tron'],
    },
    abstract: {
      instructions: 'Phép cộng với tổng đến 10.',
      examples: ['4 + 3 = 7', '5 + 4 = 9', '6 + 2 = 8', '3 + 5 = 8'],
      warnings: ['Đảm bảo con vững cộng đến 5 trước'],
    },
    parentTips: [
      'Học từng cặp số: 1+1, 1+2, 2+1, 2+2...',
      'Dùng que tính để minh họa',
      'Thực hành đều đặn',
    ],
    commonMistakes: [
      'Trẻ đếm sai khi tổng lớn hơn 5',
      'Trẻ quên làm phép cộng, chỉ viết lại số',
    ],
    warningSigns: [
      'Con không thể cộng đến 5 sau nhiều tuần luyện',
    ],
    prerequisites: ['cpa-041'],
    nextExercises: ['cpa-050'],
    tags: ['cộng', '1-10'],
  },
  // ============ SUBTRACTION ============
  {
    id: 'cpa-050',
    title: 'Phép trừ với hình ảnh',
    description: 'Trẻ học trừ qua hình ảnh (5 - 2 = ?)',
    duration: 15,
    cpaStage: 'pictorial',
    mathType: 'subtraction',
    numberRange: { min: 1, max: 10 },
    difficulty: 2,
    concrete: {
      instructions: 'Dùng đồ vật để minh họa "bớt đi".',
      materials: ['Khối gỗ', 'Trái cây', 'Kẹo'],
      examples: [
        'Con có 5 cái kẹo. Con ăn 2 cái. Còn mấy cái?',
        'Mẹ có 6 quả bóng. Cho em 3 quả. Mẹ còn mấy quả?',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình, sau đó gạch bỏ để minh họa phép trừ.',
      visualTypes: ['hinh-tron', 'gach-bo'],
    },
    abstract: {
      instructions: 'Viết phép trừ: 5 - 2 = ?',
      examples: ['4 - 1 = ?', '5 - 2 = ?', '6 - 3 = ?'],
      warnings: ['Trẻ Dyscalculia cần nhiều thời gian hơn ở bước này'],
    },
    parentTips: [
      'Giải thích "trừ" nghĩa là "bớt đi" hoặc "lấy đi"',
      'Dùng đồ vật thật trước khi chuyển sang hình',
      'Cho con tự bớt đồ vật để hiểu bản chất',
    ],
    commonMistakes: [
      'Trẻ cộng thay vì trừ',
      'Trẻ không hiểu "bớt đi" nghĩa là gì',
      'Trẻ đếm ngược khó',
    ],
    warningSigns: [
      'Con không hiểu tại sao 5 - 2 = 3',
      'Con luôn muốn cộng thay vì trừ',
    ],
    prerequisites: ['cpa-003', 'cpa-030'],
    nextExercises: ['cpa-051'],
    tags: ['trừ', 'bớt đi', 'hình ảnh'],
  },
  {
    id: 'cpa-051',
    title: 'Phép trừ với số (1-10)',
    description: 'Trẻ làm phép trừ với số thuần túy',
    duration: 15,
    cpaStage: 'abstract',
    mathType: 'subtraction',
    numberRange: { min: 1, max: 10 },
    difficulty: 3,
    concrete: {
      instructions: ' Quay lại đồ vật nếu con gặp khó khăn.',
      materials: ['Khối gỗ', 'Ngón tay'],
      examples: [
        'Dùng ngón tay: 5 ngón - 2 ngón = 3 ngón',
      ],
    },
    pictorial: {
      instructions: 'Vẽ hình và gạch bỏ.',
      visualTypes: ['que-tinh', 'hinh-tron'],
    },
    abstract: {
      instructions: 'Giải phép trừ với số.',
      examples: ['7 - 3 = 4', '9 - 4 = 5', '6 - 2 = 4'],
      warnings: ['Trẻ có thể cần nhiều thời gian hơn với phép trừ'],
    },
    parentTips: [
      'Trừ là khó hơn cộng, cần kiên nhẫn',
      'Bắt đầu với số nhỏ (5-1, 5-2)',
      'Dùng ngón tay để đếm lùi',
    ],
    commonMistakes: [
      'Trẻ trừ nhầm số',
      'Trẻ quên đếm lùi',
    ],
    warningSigns: [
      'Con hoàn toàn không hiểu phép trừ sau nhiều tuần',
    ],
    prerequisites: ['cpa-050', 'cpa-042'],
    nextExercises: [],
    tags: ['trừ', 'số', '1-10'],
  },
];

export default mockExercises;
