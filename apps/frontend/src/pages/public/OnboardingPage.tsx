import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Sparkles, ArrowRight } from 'lucide-react';
import Button from '../../components/ui/Button';

const OnboardingPage = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);

  const handleStart = () => {
    // Skip onboarding for demo - go directly to dashboard
    navigate('/parent/dashboard');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-purple-50 to-pink-50">
      <div className="max-w-4xl mx-auto px-4 py-12">
        {/* Header */}
        <div className="text-center mb-12">
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-white rounded-full shadow-sm mb-6">
            <Sparkles className="w-5 h-5 text-primary-500" />
            <span className="text-sm font-medium text-gradient-primary">MathMate Support</span>
          </div>
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            Chào mừng đến với MathMate Support
          </h1>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            Hỗ trợ phụ huynh có con gặp khó khăn với việc học toán (Dyscalculia)
            bằng phương pháp CPA hiệu quả.
          </p>
        </div>

        {/* Content */}
        <div className="space-y-6">
          {/* Step 1: Child Info */}
          {step === 1 && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white rounded-2xl shadow-lg p-8"
            >
              <h2 className="text-2xl font-bold text-gray-900 mb-6">Thông tin về con</h2>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Tên của con
                  </label>
                  <input
                    type="text"
                    placeholder="Nhập tên con"
                    defaultValue="Minh"
                    className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Ngày sinh
                  </label>
                  <input
                    type="date"
                    defaultValue="2021-03-15"
                    className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Mức độ Dyscalculia
                  </label>
                  <div className="flex gap-3">
                    {['Nhẹ', 'Trung bình', 'Nặng'].map((level) => (
                      <label
                        key={level}
                        className="flex-1 cursor-pointer"
                      >
                        <input
                          type="radio"
                          name="level"
                          value={level}
                          defaultChecked={level === 'Trung bình'}
                          className="peer hidden"
                        />
                        <div className="px-4 py-3 border-2 border-gray-200 rounded-xl text-center peer-checked:border-primary-500 peer-checked:bg-primary-50 transition-colors">
                          {level}
                        </div>
                      </label>
                    ))}
                  </div>
                </div>
              </div>

              <div className="mt-8 flex justify-end">
                <Button onClick={() => setStep(2)} size="lg">
                  Tiếp tục
                  <ArrowRight className="w-5 h-5 ml-2" />
                </Button>
              </div>
            </motion.div>
          )}

          {/* Step 2: Assessment */}
          {step === 2 && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white rounded-2xl shadow-lg p-8"
            >
              <h2 className="text-2xl font-bold text-gray-900 mb-2">Đánh giá ban đầu</h2>
              <p className="text-gray-600 mb-6">
                Giúp chúng tôi hiểu hơn về khó khăn của con với toán học
              </p>

              <div className="space-y-4">
                <div className="p-4 rounded-xl border border-primary-100 bg-gradient-primary-soft">
                  <h3 className="font-semibold text-gray-900 mb-2">Minh gặp khó khăn với:</h3>
                  <div className="space-y-2">
                    {[
                      'Không hiểu đề bài',
                      'Tính toán chậm',
                      'Sợ học toán',
                      'Không nắm vững kiến thức cơ bản',
                    ].map((item) => (
                      <label key={item} className="flex items-center gap-3 cursor-pointer">
                        <input
                          type="checkbox"
                          defaultChecked
                          className="w-5 h-5 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                        />
                        <span className="text-gray-700">{item}</span>
                      </label>
                    ))}
                  </div>
                </div>
              </div>

              <div className="mt-8 flex justify-between">
                <Button variant="secondary" onClick={() => setStep(1)}>
                  Quay lại
                </Button>
                <Button onClick={handleStart} size="lg" className="btn-premium-solid-gradient transition-all rounded-full py-3 shadow-lg hover:shadow-xl">
                  Bắt đầu sử dụng
                  <ArrowRight className="w-5 h-5 ml-2 text-white" />
                </Button>
              </div>
            </motion.div>
          )}
        </div>

        {/* Progress indicators */}
        <div className="flex justify-center gap-2 mt-8">
          {[1, 2].map((s) => (
            <div
              key={s}
              className={`w-3 h-3 rounded-full transition-colors ${
                s === step ? 'bg-primary-600' : 'bg-gray-300'
              }`}
            />
          ))}
        </div>
      </div>
    </div>
  );
};

export default OnboardingPage;
