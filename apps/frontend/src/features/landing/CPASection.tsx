import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';

const CPASection = () => {
  return (
    <section id="method" className="py-20 px-4 sm:px-6 lg:px-8 bg-gradient-to-br from-blue-50 to-purple-50">
      <div className="max-w-7xl mx-auto">
        <div className="text-center mb-12">
          <Badge variant="info" className="mb-4">
            Phương pháp CPA
          </Badge>
          <h2 className="text-4xl font-bold text-gray-900 mb-4">
            Toán Singapore cho trẻ Dyscalculia
          </h2>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto">
            CPA (Concrete → Pictorial → Abstract) giúp trẻ xây dựng nền tảng vững chắc
          </p>
        </div>

        {/* CPA Stages */}
        <div className="cards-container relative grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
          {/* Concrete */}
          <Card padding="lg" className="card h-full flex flex-col text-center bg-white border-2 border-blue-200 transition-all">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-br from-blue-400 to-blue-600 rounded-2xl flex items-center justify-center text-4xl shadow-lg">
              🧱
            </div>
            <div className="flex-1">
              <h3 className="text-2xl font-bold text-blue-600 mb-2">Concrete</h3>
              <p className="text-gray-500 text-sm mb-4">(Cụ thể)</p>
              <p className="text-gray-600 mb-4">
                Trẻ học toán bằng cách cầm nắm, di chuyển đồ vật thật
              </p>
              <div className="space-y-2 text-left">
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-blue-500">✓</span>
                  <span>Đếm khối gỗ</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-blue-500">✓</span>
                  <span>Xếp que tính</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-blue-500">✓</span>
                  <span>Dùng ngón tay</span>
                </div>
              </div>
            </div>
          </Card>

          {/* Mobile Arrow 1 */}
          <div className="flex md:hidden justify-center items-center -my-2 text-primary-400 pointer-events-none">
            <svg className="w-6 h-6 animate-bounce" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth="3" xmlns="http://www.w3.org/2000/svg">
              <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 13.5L12 21m0 0l-7.5-7.5M12 21V3" />
            </svg>
          </div>

          {/* Pictorial */}
          <Card padding="lg" className="card h-full flex flex-col text-center bg-white border-2 border-yellow-200 transition-all md:-translate-y-0">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-br from-yellow-400 to-yellow-600 rounded-2xl flex items-center justify-center text-4xl shadow-lg">
              🎨
            </div>
            <div className="flex-1">
              <h3 className="text-2xl font-bold text-yellow-600 mb-2">Pictorial</h3>
              <p className="text-gray-500 text-sm mb-4">(Hình ảnh)</p>
              <p className="text-gray-600 mb-4">
                Chuyển đồ vật thật thành hình vẽ, sơ đồ minh họa
              </p>
              <div className="space-y-2 text-left">
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-yellow-500">✓</span>
                  <span>Vẽ hình tròn đếm</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-yellow-500">✓</span>
                  <span>Dùng que tính minh họa</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-yellow-500">✓</span>
                  <span>Tô màu theo số</span>
                </div>
              </div>
            </div>
          </Card>

          {/* Mobile Arrow 2 */}
          <div className="flex md:hidden justify-center items-center -my-2 text-primary-400 pointer-events-none">
            <svg className="w-6 h-6 animate-bounce" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth="3" xmlns="http://www.w3.org/2000/svg">
              <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 13.5L12 21m0 0l-7.5-7.5M12 21V3" />
            </svg>
          </div>

          {/* Abstract */}
          <Card padding="lg" className="card h-full flex flex-col text-center bg-white border-2 border-purple-200 transition-all">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-br from-purple-400 to-purple-600 rounded-2xl flex items-center justify-center text-4xl shadow-lg">
              🔢
            </div>
            <div className="flex-1">
              <h3 className="text-2xl font-bold text-purple-600 mb-2">Abstract</h3>
              <p className="text-gray-500 text-sm mb-4">(Trừu tượng)</p>
              <p className="text-gray-600 mb-4">
                Chỉ dùng ký hiệu số và phép tính thuần túy
              </p>
              <div className="space-y-2 text-left">
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-purple-500">✓</span>
                  <span>Viết số, đọc to</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-purple-500">✓</span>
                  <span>Làm phép cộng/trừ</span>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <span className="text-purple-500">✓</span>
                  <span>So sánh số</span>
                </div>
              </div>
            </div>
          </Card>

          {/* Desktop Arrow 1: between Concrete and Pictorial */}
          <div className="hidden md:flex absolute top-1/2 -translate-y-1/2 left-1/3 -translate-x-1/2 z-20 bg-white p-2.5 rounded-full border border-blue-100 shadow-md items-center justify-center pointer-events-none hover:scale-110 transition-transform">
            <svg className="w-5 h-5 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth="3.5" xmlns="http://www.w3.org/2000/svg">
              <path strokeLinecap="round" strokeLinejoin="round" d="M13.5 4.5L21 12m0 0l-7.5 7.5M21 12H3" />
            </svg>
          </div>

          {/* Desktop Arrow 2: between Pictorial and Abstract */}
          <div className="hidden md:flex absolute top-1/2 -translate-y-1/2 left-2/3 -translate-x-1/2 z-20 bg-white p-2.5 rounded-full border border-blue-100 shadow-md items-center justify-center pointer-events-none hover:scale-110 transition-transform">
            <svg className="w-5 h-5 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth="3.5" xmlns="http://www.w3.org/2000/svg">
              <path strokeLinecap="round" strokeLinejoin="round" d="M13.5 4.5L21 12m0 0l-7.5 7.5M21 12H3" />
            </svg>
          </div>
        </div>

        <style>{` 
          .cards-container .card { 
            transition: transform 200ms ease, box-shadow 200ms ease, opacity 200ms ease; 
            will-change: transform, opacity; 
          }
          .cards-container .card:hover { 
            transform: translateY(-10px) scale(1.03); 
            box-shadow: 0 20px 40px rgba(2,6,23,0.15); 
            z-index: 10; 
            opacity: 1; 
          }
          .cards-container:hover .card:not(:hover) { 
            transform: translateY(0) scale(0.995); 
            opacity: 0.85; 
            box-shadow: 0 6px 12px rgba(2,6,23,0.06); 
          }
        `}</style>

        {/* Key message */}
        <Card padding="lg" className="!bg-gradient-to-r !from-[#6c9aee] !to-[#5ac1e5] !text-white !border-transparent text-center shadow-lg">
          <p className="text-xl font-bold">
            💡 Não bộ trẻ Dyscalculia gặp khó khăn ở bước Abstract.
          </p>
          <p className="text-base mt-3 font-medium opacity-90">
            CPA giúp trẻ xây dựng nền tảng từ Concrete → Pictorial → Abstract một cách tự nhiên.
          </p>
        </Card>
      </div>
    </section>
  );
};

export default CPASection;
