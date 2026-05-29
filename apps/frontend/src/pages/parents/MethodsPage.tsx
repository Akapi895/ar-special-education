/**
 * Methods Page - CPA Method Guide
 */

import { useState } from 'react';
import { ChevronDown, ChevronUp, BookOpen, Lightbulb } from 'lucide-react';
import Badge from '../../components/ui/Badge';
import PageContainer from '../../components/layout/PageContainer';

interface CPAStage {
  id: 'concrete' | 'pictorial' | 'abstract';
  title: string;
  subtitle: string;
  icon: string;
  color: string;
  bgColor: string;
  borderColor: string;
  description: string;
  whenToUse: string[];
  activities: string[];
  tips: string[];
  examples: string[];
}

const CPA_STAGES: CPAStage[] = [
  {
    id: 'concrete',
    title: 'Concrete',
    subtitle: 'Cụ thể',
    icon: '🧱',
    color: 'text-blue-600',
    bgColor: 'bg-blue-50',
    borderColor: 'border-blue-200',
    description:
      'Trẻ học toán bằng cách cầm nắm, di chuyển đồ vật thật. Đây là bước quan trọng nhất vì não bộ của trẻ Dyscalculia cần "thấy" và "chạm" để hiểu.',
    whenToUse: [
      'Trẻ mới bắt đầu học toán',
      'Trẻ không hiểu số là gì',
      'Trẻ cần "thấy" và "chạm" để hiểu',
      'Khi chuyển sang giai đoạn mới',
    ],
    activities: [
      'Xếp khối gỗ theo số lượng',
      'Đếm kẹo trong lọ',
      'Phân loại đồ chơi theo màu sắc và đếm',
      'Dùng ngón tay để minh họa số',
      'Xếp que tính thành hàng',
    ],
    tips: [
      'Cho trẻ chạm vào từng đồ vật khi đếm',
      'Đừng vội vàng, để trẻ đếm theo tốc độ của mình',
      'Khen ngợi khi trẻ đếm đúng, không chê khi sai',
      'Thực hành mỗi ngày 10-15 phút',
    ],
    examples: [
      '3 khối gỗ = "Ba khối"',
      '5 quả táo trên bàn',
      '2 chiếc xe hơi',
    ],
  },
  {
    id: 'pictorial',
    title: 'Pictorial',
    subtitle: 'Hình ảnh',
    icon: '🎨',
    color: 'text-yellow-600',
    bgColor: 'bg-yellow-50',
    borderColor: 'border-yellow-200',
    description:
      'Chuyển đồ vật thật thành hình vẽ, sơ đồ. Trẻ bắt đầu liên kết giữa đồ vật và hình ảnh đại diện.',
    whenToUse: [
      'Trẻ đã vững giai đoạn Concrete',
      'Trẻ có thể đếm đồ vật thành thạo',
      'Trẻ cần chuyển từ "thấy" sang "hình dung"',
    ],
    activities: [
      'Vẽ hình tròn và đếm',
      'Dùng que tính để minh họa phép cộng/trừ',
      'Tô màu theo số lượng',
      'Ghép số với hình ảnh tương ứng',
      'Vẽ sơ đồ để so sánh số lượng',
    ],
    tips: [
      'Bắt đầu với hình ảnh đơn giản',
      'Dùng nhiều màu sắc để thu hút',
      'Cho trẻ tự vẽ thay vì dán sticker',
      'Giải thích: "Hình này đại diện cho 3 khối"',
    ],
    examples: [
      'Vẽ 3 hình tròn = "Ba hình tròn"',
      'Que tính: 2 que + 3 que = 5 que',
      'Tranh đếm với các con vật',
    ],
  },
  {
    id: 'abstract',
    title: 'Abstract',
    subtitle: 'Trừu tượng',
    icon: '🔢',
    color: 'text-purple-600',
    bgColor: 'bg-purple-50',
    borderColor: 'border-purple-200',
    description:
      'Chỉ dùng ký hiệu số và phép tính. Đây là bước cuối cùng và cũng là bước khó nhất với trẻ Dyscalculia. Chỉ chuyển sang khi trẻ đã vững ở 2 giai đoạn trước.',
    whenToUse: [
      'Trẻ đã vững cả Concrete và Pictorial',
      'Trẻ có thể liên kết số với lượng',
      'Trẻ hiểu bản chất của phép cộng/trừ',
    ],
    activities: [
      'Viết số và đọc to',
      'Làm phép cộng: 2 + 3 = ?',
      'Làm phép trừ: 5 - 2 = ?',
      'So sánh số: 4 > 2',
    ],
    tips: [
      'Không ép trẻ nếu chưa sẵn sàng',
      ' Quay lại giai đoạn trước nếu trẻ gặp khó khăn',
      'Dùng ngón tay như công cụ hỗ trợ',
      'Khen ngợi nỗ lực, không chỉ kết quả',
    ],
    examples: [
      'Số 5 = Năm',
      '2 + 3 = 5 (Hai cộng ba bằng năm)',
      '7 - 2 = 5 (Bảy trừ hai bằng năm)',
    ],
  },
];

const MethodsPage = () => {
  const [expandedStage, setExpandedStage] = useState<string | null>('concrete');
  const [currentProgress] = useState({
    concrete: 75,
    pictorial: 40,
    abstract: 15,
  });

  const toggleStage = (id: string) => {
    setExpandedStage(expandedStage === id ? null : id);
  };

  return (
    <div className="min-h-screen bg-light-bg">
      <PageContainer maxWidth="lg" padding="md" spacing="md" className="py-4 sm:py-6">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Hướng dẫn phương pháp CPA
        </h1>
        <p className="text-gray-600">
          Phương pháp Toán Singapore - "Xương sống" cho trẻ Dyscalculia
        </p>
      </div>

      {/* Info Banner */}
      <div className="bg-linear-to-r from-blue-50 to-purple-50 rounded-2xl p-6 mb-8 border border-gray-200">
        <h2 className="text-lg font-bold text-gray-900 mb-3">
          💡 Tại sao CPA quan trọng với trẻ Dyscalculia?
        </h2>
        <p className="text-gray-700 leading-relaxed">
          Não bộ của trẻ Dyscalculia gặp khó khăn trong việc xử lý thông tin số ở bước
          Abstract (trừu tượng). CPA giúp trẻ xây dựng nền tảng vững chắc từ đồ vật
          thật (Concrete), qua hình ảnh (Pictorial), rồi mới đến con số (Abstract).
        </p>
        <p className="text-gray-700 mt-3 font-medium">
          Quy tắc vàng: <span className="text-primary-600">Concrete → Pictorial → Abstract</span>
        </p>
      </div>

      {/* CPA Stages */}
      <div className="space-y-4">
        {CPA_STAGES.map((stage) => {
          const isExpanded = expandedStage === stage.id;
          const progress = currentProgress[stage.id];
          const isComplete = progress >= 80;

          return (
            <div
              key={stage.id}
              className={`bg-white rounded-2xl border-2 ${stage.borderColor} overflow-hidden transition-all`}
            >
              {/* Header */}
              <button
                onClick={() => toggleStage(stage.id)}
                className="w-full p-5 flex items-center justify-between hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center gap-4">
                  <div className={`w-14 h-14 ${stage.bgColor} rounded-2xl flex items-center justify-center text-2xl`}>
                    {stage.icon}
                  </div>
                  <div className="text-left">
                    <div className="flex items-center gap-2">
                      <h3 className={`text-xl font-bold ${stage.color}`}>
                        {stage.title}
                      </h3>
                      <span className="text-gray-500">({stage.subtitle})</span>
                      {isComplete && (
                        <Badge className="bg-green-100 text-green-700">Đã vững</Badge>
                      )}
                    </div>
                    <div className="flex items-center gap-3 mt-1">
                      <div className="w-32 h-2 bg-gray-200 rounded-full overflow-hidden">
                        <div
                          className={`h-full ${stage.color.replace('text-', 'bg-')} transition-all`}
                          style={{ width: `${progress}%` }}
                        />
                      </div>
                      <span className="text-sm text-gray-500">{progress}%</span>
                    </div>
                  </div>
                </div>
                {isExpanded ? (
                  <ChevronUp className="w-5 h-5 text-gray-400" />
                ) : (
                  <ChevronDown className="w-5 h-5 text-gray-400" />
                )}
              </button>

              {/* Content */}
              {isExpanded && (
                <div className={`p-5 pt-0 ${stage.bgColor} space-y-4`}>
                  <p className="text-gray-700">{stage.description}</p>

                  <div className="grid md:grid-cols-2 gap-4">
                    {/* When to use */}
                    <div className="bg-white rounded-xl p-4">
                      <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                        <BookOpen className="w-4 h-4" />
                        Khi nào cần giai đoạn này:
                      </h4>
                      <ul className="space-y-1">
                        {stage.whenToUse.map((item, i) => (
                          <li key={i} className="text-sm text-gray-600 flex items-start gap-2">
                            <span className="w-1.5 h-1.5 bg-gray-400 rounded-full mt-1.5 shrink-0"></span>
                            {item}
                          </li>
                        ))}
                      </ul>
                    </div>

                    {/* Activities */}
                    <div className="bg-white rounded-xl p-4">
                      <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                        <Lightbulb className="w-4 h-4" />
                        Hoạt động đề xuất:
                      </h4>
                      <ul className="space-y-1">
                        {stage.activities.map((item, i) => (
                          <li key={i} className="text-sm text-gray-600 flex items-start gap-2">
                            <span className="w-1.5 h-1.5 bg-primary-400 rounded-full mt-1.5 shrink-0"></span>
                            {item}
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>

                  {/* Tips */}
                  <div className="bg-white rounded-xl p-4">
                    <h4 className="font-semibold text-gray-900 mb-2">💡 Mẹo cho phụ huynh:</h4>
                    <ul className="space-y-1">
                      {stage.tips.map((item, i) => (
                        <li key={i} className="text-sm text-gray-600 flex items-start gap-2">
                          <span className="w-1.5 h-1.5 bg-yellow-400 rounded-full mt-1.5 shrink-0"></span>
                          {item}
                        </li>
                      ))}
                    </ul>
                  </div>

                  {/* Examples */}
                  <div className="bg-white rounded-xl p-4">
                    <h4 className="font-semibold text-gray-900 mb-2">📝 Ví dụ thực tế:</h4>
                    <div className="flex flex-wrap gap-2">
                      {stage.examples.map((example, i) => (
                        <span
                          key={i}
                          className={`px-3 py-1.5 rounded-full text-sm ${stage.bgColor} ${stage.color} font-medium`}
                        >
                          {example}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* General Tips */}
      <div className="mt-8 bg-linear-to-r from-green-50 to-blue-50 rounded-2xl p-6 border border-gray-200">
        <h2 className="text-lg font-bold text-gray-900 mb-4">
          🌟 6 Nguyên tắc vàng khi dạy trẻ Dyscalculia
        </h2>
        <div className="grid md:grid-cols-2 gap-4">
          {[
            { num: '1', text: 'Kiên nhẫn - Con cần thời gian để tiếp thu' },
            { num: '2', text: 'Không so sánh con với trẻ khác' },
            { num: '3', text: 'Thực hành mỗi ngày 10-15 phút' },
            { num: '4', text: 'Biến học thành trò chơi' },
            { num: '5', text: 'Khen ngợi nỗ lực, không chỉ kết quả' },
            { num: '6', text: 'Quay lại bước trước nếu con gặp khó khăn' },
          ].map((tip) => (
            <div key={tip.num} className="flex items-start gap-3">
              <span className="w-7 h-7 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center font-bold text-sm shrink-0">
                {tip.num}
              </span>
              <p className="text-gray-700">{tip.text}</p>
            </div>
          ))}
        </div>
      </div>
      </PageContainer>
    </div>
  );
};

export default MethodsPage;
