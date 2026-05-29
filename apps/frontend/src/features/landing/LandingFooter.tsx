import { Calculator } from 'lucide-react';

const LandingFooter = () => {
  return (
    <footer className="bg-gray-900 text-gray-400 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          {/* Brand */}
          <div className="md:col-span-2">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-purple-600 rounded-xl flex items-center justify-center">
                <Calculator className="w-6 h-6 text-white" />
              </div>
              <span className="text-xl font-bold text-white">MathMate Support</span>
            </div>
            <p className="text-gray-400 mb-4">
              Hỗ trợ phụ huynh có con gặp khó khăn với việc học toán (Dyscalculia).
              Áp dụng phương pháp CPA - Toán Singapore hiệu quả nhất cho trẻ.
            </p>
          </div>

          {/* Quick Links */}
          <div>
            <h4 className="text-white font-semibold mb-4">Liên kết nhanh</h4>
            <ul className="space-y-2">
              <li><a href="#about" className="hover:text-white transition-colors">Giới thiệu</a></li>
              <li><a href="#method" className="hover:text-white transition-colors">Phương pháp CPA</a></li>
              <li><a href="#features" className="hover:text-white transition-colors">Tính năng</a></li>
            </ul>
          </div>

          {/* Support */}
          <div>
            <h4 className="text-white font-semibold mb-4">Hỗ trợ</h4>
            <ul className="space-y-2">
              <li><a href="#" className="hover:text-white transition-colors">Câu hỏi thường gặp</a></li>
              <li><a href="#" className="hover:text-white transition-colors">Liên hệ</a></li>
              <li><a href="#" className="hover:text-white transition-colors">Chính sách bảo mật</a></li>
            </ul>
          </div>
        </div>

        <div className="border-t border-gray-800 pt-8 text-center">
          <p>© 2025 MathMate Support. Bảo lưu mọi quyền.</p>
          <p className="mt-2 text-sm text-gray-500">
            Được tạo ra với ❤️ dành cho mọi gia đình có con gặp khó khăn với toán học.
          </p>
        </div>
      </div>
    </footer>
  );
};

export default LandingFooter;
