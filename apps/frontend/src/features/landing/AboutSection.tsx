import Badge from '../../components/ui/Badge';

const AboutSection = () => {
  return (
    <section id="about" className="py-20 px-4 sm:px-6 lg:px-8 bg-white">
      <div className="max-w-7xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          {/* Left Content */}
          <div>
            <Badge variant="primary" className="mb-4">
              Về Dyscalculia
            </Badge>
            <h2 className="text-4xl font-bold text-gray-900 mb-6">
              Con bạn không "dốt" toán.
              <br />
              Con chỉ cần cách học phù hợp.
            </h2>
            <div className="space-y-4 text-gray-600">
              <p>
                <strong>Dyscalculia</strong> là rối loạn học toán khiến trẻ gặp khó khăn với:
              </p>
              <ul className="space-y-2 ml-4">
                <li className="flex items-start gap-2">
                  <span className="text-primary-500 mt-1">•</span>
                  <span>Hiểu ý nghĩa của số lượng</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary-500 mt-1">•</span>
                  <span>Nhớ các con số</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary-500 mt-1">•</span>
                  <span>Thực hiện phép tính</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary-500 mt-1">•</span>
                  <span>Hiểu các khái niệm toán học</span>
                </li>
              </ul>
              <p className="mt-6 p-4 bg-blue-50 rounded-xl border border-blue-100">
                <strong className="text-blue-700">💡 Điều quan trọng:</strong> Trẻ Dyscalculia không phải "kém thông minh". 
                Não bộ của các em xử lý thông tin số khác với người bình thường. 
                Với phương pháp đúng, trẻ <strong>HOÀN TOÀN có thể học tốt toán!</strong>
              </p>
            </div>
          </div>

          {/* Right - Visual */}
          <div className="relative">
            <div className="bg-gradient-to-br from-blue-100 via-purple-100 to-pink-100 rounded-3xl p-8">
              <div className="text-center space-y-6">
                <div className="text-8xl">🧠</div>
                <div className="space-y-4">
                  <div className="bg-white rounded-2xl p-4 shadow-sm">
                    <p className="text-sm text-gray-600 mb-2">Người bình thường</p>
                    <div className="flex justify-center gap-1">
                      {[1,2,3,4,5].map(i => (
                        <div key={i} className="w-8 h-8 bg-green-400 rounded" />
                      ))}
                    </div>
                  </div>
                  <div className="bg-white rounded-2xl p-4 shadow-sm">
                    <p className="text-sm text-gray-600 mb-2">Người bị Dyscalculia</p>
                    <div className="flex justify-center gap-1">
                      {[1,2,3,4,5].map(i => (
                        <div key={i} className={`w-8 h-8 rounded ${i <= 2 ? 'bg-purple-400' : 'bg-gray-200'}`} />
                      ))}
                    </div>
                    <p className="text-xs text-gray-500 mt-2">Cần xây dựng nền tảng từ đầu</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default AboutSection;
