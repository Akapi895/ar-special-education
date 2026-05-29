import { BarChart3, BookOpen, Heart, Lightbulb } from 'lucide-react';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';

const FeaturesSection = () => {
  const features = [
    {
      icon: BarChart3,
      title: 'Theo dõi tiến độ CPA',
      description: 'Dashboard trực quan hiển thị tiến độ Concrete → Pictorial → Abstract của con. Theo dõi sự tiến bộ theo thời gian thực.',
      iconBg: 'bg-gradient-to-br from-blue-100 to-blue-200',
      iconColor: 'text-blue-600',
    },
    {
      icon: BookOpen,
      title: 'Thư viện bài tập CPA',
      description: 'Hơn 15 bài tập được thiết kế riêng cho trẻ Dyscalculia, phù hợp với từng giai đoạn phát triển.',
      iconBg: 'bg-gradient-to-br from-purple-100 to-purple-200',
      iconColor: 'text-purple-600',
    },
    {
      icon: Lightbulb,
      title: 'Gợi ý bài tập thông minh',
      description: 'Hệ thống tự động gợi ý bài tập phù hợp dựa trên tiến độ hiện tại và cảm xúc của con.',
      iconBg: 'bg-gradient-to-br from-pink-100 to-pink-200',
      iconColor: 'text-pink-600',
    },
    {
      icon: Heart,
      title: 'Nhật ký cảm xúc',
      description: 'Theo dõi cảm xúc của con khi học toán. Hiểu rõ con vui hay buồn, tự tin hay lo lắng để điều chỉnh phương pháp phù hợp.',
      iconBg: 'bg-gradient-to-br from-blue-100 to-purple-200',
      iconColor: 'text-purple-600',
    },
  ];

  return (
    <section id="features" className="relative overflow-hidden py-20 px-4 sm:px-6 lg:px-8 bg-gradient-to-br from-white via-blue-50/50 to-purple-50/50">
      <div className="max-w-7xl mx-auto">
        <div className="text-center mb-16">
          <Badge variant="primary" className="mb-4">
            Tính năng
          </Badge>
          <h2 className="text-4xl sm:text-5xl font-bold text-gray-900 mb-4">
            Mọi thứ bạn cần để hỗ trợ con
          </h2>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto leading-relaxed">
            MathMate Support cung cấp công cụ toàn diện giúp phụ huynh đồng hành cùng con trên hành trình học toán
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {features.map((feature, index) => {
            const Icon = feature.icon;
            return (
              <Card
                key={index}
                hover
                padding="lg"
                className="group animate-fade-in transition-all duration-300 hover:shadow-lg hover:-translate-y-1"
              >
                <div className={`${feature.iconBg} p-4 rounded-2xl w-fit mb-4 group-hover:scale-110 transition-transform duration-300 shadow-sm`}>
                  <Icon className={`w-8 h-8 ${feature.iconColor}`} />
                </div>
                <h3 className="text-xl font-bold text-gray-900 mb-3">
                  {feature.title}
                </h3>
                <p className="text-gray-600 leading-relaxed">
                  {feature.description}
                </p>
              </Card>
            );
          })}
        </div>
      </div>
    </section>
  );
};

export default FeaturesSection;
