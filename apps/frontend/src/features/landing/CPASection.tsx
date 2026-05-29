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
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
          {/* Concrete */}
          <Card padding="lg" className="text-center bg-white border-2 border-blue-200 hover:border-blue-400 transition-colors">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-br from-blue-400 to-blue-600 rounded-2xl flex items-center justify-center text-4xl shadow-lg">
              🧱
            </div>
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
          </Card>

          {/* Pictorial */}
          <Card padding="lg" className="text-center bg-white border-2 border-yellow-200 hover:border-yellow-400 transition-colors transform md:-translate-y-4">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-br from-yellow-400 to-yellow-600 rounded-2xl flex items-center justify-center text-4xl shadow-lg">
              🎨
            </div>
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
          </Card>

          {/* Abstract */}
          <Card padding="lg" className="text-center bg-white border-2 border-purple-200 hover:border-purple-400 transition-colors">
            <div className="w-20 h-20 mx-auto mb-4 bg-gradient-to-br from-purple-400 to-purple-600 rounded-2xl flex items-center justify-center text-4xl shadow-lg">
              🔢
            </div>
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
          </Card>
        </div>

        {/* Arrow indicator */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center gap-4 text-gray-400">
            <span className="text-2xl">🧱</span>
            <span className="text-3xl">→</span>
            <span className="text-2xl">🎨</span>
            <span className="text-3xl">→</span>
            <span className="text-2xl">🔢</span>
          </div>
        </div>

        {/* Key message */}
        <Card padding="lg" className="bg-gradient-to-r from-blue-600 to-purple-600 text-white text-center">
          <p className="text-xl font-medium">
            💡 Não bộ trẻ Dyscalculia gặp khó khăn ở bước Abstract. 
            CPA giúp trẻ xây dựng nền tảng từ Concrete → Pictorial → Abstract một cách tự nhiên.
          </p>
        </Card>
      </div>
    </section>
  );
};

export default CPASection;
