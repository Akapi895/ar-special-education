import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Calculator, Menu, X } from 'lucide-react';
import Button from '../../components/ui/Button';

const LandingNavbar = () => {
  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [scrollY, setScrollY] = useState(0);

  useEffect(() => {
    const handleScroll = () => setScrollY(window.scrollY);
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <nav 
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        scrollY > 50 
          ? 'bg-white/95 backdrop-blur-lg shadow-medium' 
          : 'bg-transparent'
      }`}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-20">
          {/* Logo */}
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-primary rounded-xl flex items-center justify-center shadow-md">
              <Calculator className="w-6 h-6 text-white" />
            </div>
            <span className="text-2xl font-bold bg-gradient-to-r from-[#6c9aee] to-[#5ac1e5] bg-clip-text text-transparent">
              MathMate Support
            </span>
          </div>

          {/* Desktop Menu - Căn giữa với gap lớn hơn */}
          <div className="hidden md:flex items-center justify-center gap-12 flex-1 px-8">
            <a href="#about" className="text-gray-700 hover:text-primary-600 font-medium transition-colors whitespace-nowrap">
              Giới thiệu
            </a>
            <a href="#method" className="text-gray-700 hover:text-primary-600 font-medium transition-colors whitespace-nowrap">
              Phương pháp CPA
            </a>
            <a href="#features" className="text-gray-700 hover:text-primary-600 font-medium transition-colors whitespace-nowrap">
              Tính năng
            </a>
          </div>

          {/* Auth Buttons - Chỉ outline button */}
          <div className="hidden md:flex items-center gap-4">
            <Button 
              variant="outline" 
              onClick={() => navigate('/login')}
              className="btn-premium-solid-gradient"
            >
              Đăng nhập
            </Button>
            <Button variant="secondary" onClick={() => navigate('/register')} className="btn-premium-solid-gradient">
              Bắt đầu miễn phí
            </Button>
          </div>

          {/* Mobile Menu Button */}
          <button
            onClick={() => setIsMenuOpen(!isMenuOpen)}
            className="md:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
          >
            {isMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {isMenuOpen && (
        <div className="md:hidden bg-white border-t border-gray-100 shadow-lg animate-slide-down">
          <div className="px-4 py-6 space-y-4">
            <a href="#about" className="block text-gray-700 hover:text-primary-600 font-medium py-2">
              Giới thiệu
            </a>
            <a href="#method" className="block text-gray-700 hover:text-primary-600 font-medium py-2">
              Phương pháp CPA
            </a>
            <a href="#features" className="block text-gray-700 hover:text-primary-600 font-medium py-2">
              Tính năng
            </a>
            <div className="pt-4 space-y-3 border-t border-gray-100">
              <Button variant="outline" fullWidth onClick={() => navigate('/login')} className="btn-premium-solid-gradient">
                Đăng nhập
              </Button>
              <Button variant="secondary" fullWidth onClick={() => navigate('/register')} className="btn-premium-solid-gradient">
                Bắt đầu miễn phí
              </Button>
            </div>
          </div>
        </div>
      )}
    </nav>
  );
};

export default LandingNavbar;
