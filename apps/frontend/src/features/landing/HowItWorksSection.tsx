import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';

const HowItWorksSection = () => {
  const steps = [
    {
      step: '1',
      title: 'Tìm hiểu con',
      description: 'Xác định mức độ Dyscalculia và khó khăn cụ thể của con với toán học',
      icon: '👶',
      color: 'blue',
    },
    {
      step: '2',
      title: 'Học phương pháp CPA',
      description: 'Tìm hiểu phương pháp Concrete → Pictorial → Abstract phù hợp với con',
      icon: '📚',
      color: 'yellow',
    },
    {
      step: '3',
      title: 'Thực hành cùng con',
      description: 'Áp dụng các bài tập và hoạt động CPA mỗi ngày, theo dõi tiến độ',
      icon: '🎯',
      color: 'purple',
    },
  ];

  return (
    <section id="method" className="relative overflow-hidden py-20 px-4 sm:px-6 lg:px-8 bg-gradient-to-br from-blue-50 via-purple-50 to-pink-50">
      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="text-center mb-16">
          <Badge variant="info" className="mb-4">
            Phương pháp CPA
          </Badge>
          <h2 className="text-4xl sm:text-5xl font-bold text-gray-900 mb-4">
            3 bước đồng hành cùng con
          </h2>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto">
            Phương pháp Toán Singapore giúp trẻ Dyscalculia xây dựng nền tảng vững chắc
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 relative">
          {/* Connection Lines - Desktop Only */}
          <div className="hidden md:block absolute top-16 left-0 right-0 h-1">
            <div className="absolute left-1/6 right-1/6 h-full bg-gradient-to-r from-blue-400 via-yellow-400 to-purple-400 opacity-30 rounded-full" />
          </div>

          {steps.map((step, index) => (
            <div 
              key={index}
              className="relative animate-fade-in group"
              style={{ animationDelay: `${index * 150}ms` }}
            >
              <Card padding="lg" className="text-center h-full transition-all duration-300 hover:shadow-lg hover:-translate-y-2 bg-white/80 backdrop-blur-sm">
                {/* Icon */}
                <div className={`w-20 h-20 mx-auto mb-6 rounded-full bg-gradient-to-br ${
                  step.color === 'blue' ? 'from-blue-400 to-blue-600' :
                  step.color === 'yellow' ? 'from-yellow-400 to-yellow-600' :
                  'from-purple-400 to-purple-600'
                } flex items-center justify-center text-4xl shadow-lg group-hover:scale-110 transition-transform duration-300`}>
                  {step.icon}
                </div>

                {/* Step number badge */}
                <div className={`absolute -top-4 -right-4 w-12 h-12 bg-gradient-to-br ${
                  step.color === 'blue' ? 'from-blue-500 to-blue-600' :
                  step.color === 'yellow' ? 'from-yellow-500 to-yellow-600' :
                  'from-purple-500 to-purple-600'
                } rounded-full flex items-center justify-center text-white font-bold text-xl shadow-lg`}>
                  {step.step}
                </div>

                <h3 className="text-2xl font-bold text-gray-900 mb-4">
                  {step.title}
                </h3>
                <p className="text-gray-600 leading-relaxed">
                  {step.description}
                </p>
              </Card>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default HowItWorksSection;
