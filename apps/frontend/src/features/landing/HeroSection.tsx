import { useNavigate } from 'react-router-dom';
import { useEffect, useRef, useState } from 'react';
import { Sparkles, BookOpen, CheckCircle2, ArrowRight } from 'lucide-react';
import Button from '../../components/ui/Button';

const HeroSection = () => {
  const navigate = useNavigate();
  const sectionRef = useRef<HTMLElement>(null);
  const [showScrollIndicator, setShowScrollIndicator] = useState(true);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        setShowScrollIndicator(entry.intersectionRatio > 0.2);
      },
      {
        threshold: [0, 0.2, 0.5, 1],
        rootMargin: '0px',
      }
    );

    if (sectionRef.current) {
      observer.observe(sectionRef.current);
    }

    return () => {
      if (sectionRef.current) {
        observer.unobserve(sectionRef.current);
      }
    };
  }, []);

  return (
    <section 
      ref={sectionRef}
      className="relative flex items-center overflow-hidden bg-gradient-to-br from-white via-blue-50 to-purple-50 min-h-screen"
      style={{
        minHeight: 'calc(100vh - env(safe-area-inset-top) - env(safe-area-inset-bottom))',
      }}
    >
      {/* Animated Background Decorations */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-20 left-10 w-72 h-72 bg-blue-400 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-pulse-soft" />
        <div className="absolute top-40 right-20 w-96 h-96 bg-purple-400 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-pulse-soft" style={{ animationDelay: '1s' }} />
        <div className="absolute -bottom-20 left-1/2 w-80 h-80 bg-pink-400 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-pulse-soft" style={{ animationDelay: '2s' }} />
      </div>

      <div className="relative w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-12 items-center min-h-[calc(100vh-5rem)]">
          {/* Left Content */}
          <div className="flex flex-col justify-center text-center lg:text-left space-y-8 animate-fade-in">
            <h1 className="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-bold leading-tight text-gradient-primary">
              MathMate Support
            </h1>
            
            <p className="text-lg sm:text-xl md:text-2xl text-gray-600 max-w-2xl mx-auto lg:mx-0 leading-relaxed" style={{ lineHeight: '1.7' }}>
              Hỗ trợ phụ huynh có con gặp khó khăn với việc học toán <span className="font-medium text-gray-700">(Dyscalculia)</span>.
              <br />
              Áp dụng phương pháp CPA - Toán Singapore hiệu quả nhất cho trẻ.
            </p>

            {/* CTA Buttons - Đồng bộ style */}
            <div className="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start pt-4">
              <Button 
                size="lg" 
                onClick={() => navigate('/register')}
                icon={<Sparkles className="w-5 h-5 text-white" />}
                className="btn-premium-solid-gradient text-lg shadow-lg hover:shadow-xl hover:scale-105 transition-all duration-300 group"
              >
                Bắt đầu miễn phí
                <ArrowRight className="w-5 h-5 ml-2 text-white group-hover:translate-x-1 transition-transform" />
              </Button>
              <Button 
                size="lg" 
                variant="outline"
                icon={<BookOpen className="w-5 h-5" />}
                className="btn-premium-gradient text-lg shadow-lg hover:shadow-xl transition-all duration-300"
              >
                Tìm hiểu thêm
              </Button>
            </div>

            {/* Info badges - Icon màu xanh dương/tím */}
            <div className="flex flex-col sm:flex-row items-center gap-4 sm:gap-6 justify-center lg:justify-start text-sm text-gray-600 pt-2">
              <div className="flex items-center gap-2 bg-white/80 backdrop-blur-sm px-4 py-2 rounded-full shadow-sm">
                <CheckCircle2 className="w-5 h-5 text-blue-500 shrink-0" />
                <span className="font-medium">Miễn phí 100%</span>
              </div>
              <div className="flex items-center gap-2 bg-white/80 backdrop-blur-sm px-4 py-2 rounded-full shadow-sm">
                <CheckCircle2 className="w-5 h-5 text-purple-500 shrink-0" />
                <span className="font-medium">Dành cho trẻ 4-6 tuổi</span>
              </div>
            </div>
          </div>

          {/* Right - Illustration */}
          <div className="flex items-center justify-center lg:justify-end">
            <div 
              className="w-full h-[400px] sm:h-[500px] lg:h-[600px] xl:h-[700px] animate-fade-in" 
              style={{ animationDelay: '0.2s' }}
            >
              <div className="relative w-full h-full flex items-center justify-center">
                {/* Main Illustration - Soft shadow */}
                <div className="w-96 h-96 bg-gradient-to-br from-blue-50 via-purple-50 to-pink-50 rounded-full flex items-center justify-center relative shadow-xl shadow-purple-200/50">
                  {/* Inner circle */}
                  <div className="absolute inset-4 bg-white/50 rounded-full" />
                  
                  {/* Content */}
                  <div className="relative z-10 text-center space-y-6">
                    <div className="text-7xl">🔢</div>
                    <div className="space-y-3">
                      <div className="text-2xl font-bold text-gray-800">Phương pháp CPA</div>
                      <div className="flex gap-2 justify-center flex-wrap items-center">
                        <span className="px-2 py-1 bg-blue-500 text-white rounded-full text-xs font-medium shadow-sm">
                          🧱 Concrete
                        </span>
                        <span className="px-2 py-1 bg-purple-500 text-white rounded-full text-xs font-medium shadow-sm">
                          🎨 Pictorial
                        </span>
                        <span className="px-2 py-1 bg-pink-500 text-white rounded-full text-xs font-medium shadow-sm">
                          🔢 Abstract
                        </span>
                      </div>
                    </div>
                  </div>
                  
                  {/* Decorative floating icons - Vector style */}
                  <div className="absolute -top-2 left-1/4 w-12 h-12 bg-gradient-to-br from-blue-100 to-blue-200 rounded-xl flex items-center justify-center shadow-lg" style={{ animation: 'float 3s ease-in-out infinite' }}>
                    <span className="text-2xl font-bold text-blue-600">123</span>
                  </div>
                  <div className="absolute top-1/4 -right-4 w-12 h-12 bg-gradient-to-br from-purple-100 to-purple-200 rounded-xl flex items-center justify-center shadow-lg" style={{ animation: 'float 3s ease-in-out infinite', animationDelay: '1s' }}>
                    <span className="text-2xl">➕</span>
                  </div>
                  <div className="absolute bottom-1/4 -left-4 w-12 h-12 bg-gradient-to-br from-pink-100 to-pink-200 rounded-xl flex items-center justify-center shadow-lg" style={{ animation: 'float 3s ease-in-out infinite', animationDelay: '1.5s' }}>
                    <span className="text-2xl">➖</span>
                  </div>
                  <div className="absolute -bottom-2 right-1/4 w-12 h-12 bg-gradient-to-br from-blue-100 to-purple-200 rounded-xl flex items-center justify-center shadow-lg" style={{ animation: 'float 3s ease-in-out infinite', animationDelay: '2s' }}>
                    <span className="text-2xl font-bold text-blue-600">=</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* CSS Animation */}
      <style>{`
        @keyframes float {
          0%, 100% { transform: translateY(0px); }
          50% { transform: translateY(-10px); }
        }
      `}</style>

      <div 
        className={`absolute bottom-8 left-1/2 -translate-x-1/2 hidden lg:block transition-all duration-500 ${
          showScrollIndicator ? 'opacity-100 translate-y-0 animate-bounce' : 'opacity-0 translate-y-4 pointer-events-none'
        }`}
      >
      </div>
    </section>
  );
};

export default HeroSection;
