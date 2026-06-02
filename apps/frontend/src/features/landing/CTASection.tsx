import { useNavigate } from 'react-router-dom';
import { Sparkles, Heart, CheckCircle2, ArrowRight } from 'lucide-react';
import Button from '../../components/ui/Button';

const CTASection = () => {
  const navigate = useNavigate();

  const benefits = [
    {
      text: 'Miễn phí 100%',
      textColor: 'text-emerald-700',
      borderColor: 'border-emerald-200',
      iconColor: 'text-emerald-500',
    },
    {
      text: 'Dành cho trẻ 4-6 tuổi',
      textColor: 'text-blue-700',
      borderColor: 'border-blue-200',
      iconColor: 'text-blue-500',
    },
    {
      text: '100% Tiếng Việt',
      textColor: 'text-purple-700',
      borderColor: 'border-purple-200',
      iconColor: 'text-purple-500',
    },
    {
      text: 'Không cần cài đặt',
      textColor: 'text-amber-700',
      borderColor: 'border-amber-200',
      iconColor: 'text-amber-500',
    },
  ];

  return (
    <section className="py-20 px-4 sm:px-6 lg:px-8 bg-[#eaf5fa] text-gray-900 overflow-hidden relative">
      {/* Animated Background Elements */}
      <div className="absolute inset-0 opacity-15">
        <div className="absolute top-10 left-10 w-32 h-32 bg-blue-300 rounded-full blur-3xl animate-pulse" />
        <div className="absolute bottom-10 right-10 w-40 h-40 bg-pink-300 rounded-full blur-3xl animate-pulse" style={{ animationDelay: '1s' }} />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-64 h-64 bg-purple-300 rounded-full blur-3xl animate-pulse-soft" />
      </div>

      <div className="max-w-4xl mx-auto text-center relative z-10">
        <div className="mb-8">
          <div className="w-24 h-24 mx-auto bg-blue-100/50 border border-blue-200/50 backdrop-blur-sm rounded-full flex items-center justify-center text-5xl mb-6 shadow-xl" style={{ animation: 'bounce 2s ease-in-out infinite' }}>
            🔢
          </div>
        </div>

        <h2 className="text-4xl sm:text-5xl font-bold mb-6">Bắt đầu hành trình cùng con</h2>

        <p className="text-xl mb-8 text-gray-700 leading-relaxed"><span className="font-bold text-gray-900">MathMate</span> <span className="text-gradient-primary font-bold">Support</span> giúp bạn hiểu con hơn và đồng hành cùng con trên con đường học toán</p>

        {/* Benefits */}
        <div className="flex flex-wrap justify-center gap-4 mb-8">
          {benefits.map((b) => (
            <div key={b.text} className={`flex items-center gap-2 bg-white px-5 py-2.5 rounded-full border-2 ${b.borderColor} shadow-xs hover:scale-105 transition-transform duration-200`}>
              <CheckCircle2 className={`w-5 h-5 ${b.iconColor}`} />
              <span className={`font-semibold text-sm ${b.textColor}`}>{b.text}</span>
            </div>
          ))}
        </div>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Button size="lg" onClick={() => navigate('/register')} icon={<Sparkles className="w-5 h-5 text-white" />} className="btn-premium-solid-gradient text-lg shadow-xl hover:shadow-2xl hover:scale-105 active:scale-95 transition-all duration-300 font-semibold">
            Bắt đầu miễn phí
            <ArrowRight className="w-5 h-5 ml-2 text-white" />
          </Button>

          <Button size="lg" variant="secondary" onClick={() => navigate('/onboarding')} icon={<Heart className="w-5 h-5" />} className="btn-premium-gradient text-lg shadow-lg hover:shadow-xl transition-all duration-300 font-semibold">
            Tìm hiểu thêm
          </Button>
        </div>
      </div>

      <style>{`
        @keyframes bounce {
          0%, 100% { transform: translateY(0); }
          50% { transform: translateY(-10px); }
        }
      `}</style>
    </section>
  );
};

export default CTASection;
