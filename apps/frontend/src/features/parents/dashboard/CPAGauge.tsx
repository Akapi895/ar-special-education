/**
 * CPA Progress Gauge Component - Redesigned
 * Clean design with brand colors
 */

import { memo } from 'react';

interface CPAGaugeProps {
  concrete: number;
  pictorial: number;
  abstract: number;
  compact?: boolean;
}

const CPAGauge = ({ concrete, pictorial, abstract, compact = false }: CPAGaugeProps) => {
  const getCurrentStage = () => {
    if (concrete < 80) return 'concrete';
    if (pictorial < 80) return 'pictorial';
    return 'abstract';
  };

  const currentStage = getCurrentStage();

  const stages = [
    {
      key: 'concrete',
      label: 'Concrete',
      sublabel: 'Cụ thể',
      value: concrete,
      color: 'bg-[#64aaeb]',
      colorLight: 'bg-[#64aaeb]/10',
      textColor: 'text-[#64aaeb]',
      icon: '🧱',
    },
    {
      key: 'pictorial',
      label: 'Pictorial',
      sublabel: 'Hình ảnh',
      value: pictorial,
      color: 'bg-[#64aaeb]',
      colorLight: 'bg-[#64aaeb]/10',
      textColor: 'text-[#64aaeb]',
      icon: '🎨',
    },
    {
      key: 'abstract',
      label: 'Abstract',
      sublabel: 'Trừu tượng',
      value: abstract,
      color: 'bg-[#64aaeb]',
      colorLight: 'bg-[#64aaeb]/10',
      textColor: 'text-[#64aaeb]',
      icon: '🔢',
    },
  ];

  if (compact) {
    return (
      <div className="space-y-2">
        {stages.map((stage, idx) => (
          <div key={stage.key} className="flex items-center gap-3">
            <div className={`w-20 text-xs font-medium ${stage.textColor}`}>
              {stage.label}
            </div>
            <div className="flex-1 h-2 bg-gray-100 rounded-full overflow-hidden">
              <div
                className={`h-full ${stage.color} transition-all duration-500 rounded-full`}
                style={{ width: `${stage.value}%` }}
              />
            </div>
            <div className="w-12 text-right text-sm font-semibold text-gray-700">
              {stage.value}%
            </div>
            {idx < stages.length - 1 && (
              <span className="text-gray-400">→</span>
            )}
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5">
      <h3 className="text-lg font-bold text-gray-900 mb-4">Tiến độ CPA</h3>
      
      <div className="space-y-4">
        {stages.map((stage) => (
          <div key={stage.key} className="space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="text-lg">{stage.icon}</span>
                <div>
                  <span className="font-semibold text-gray-900">{stage.label}</span>
                  <span className="text-gray-500 text-sm ml-1.5">({stage.sublabel})</span>
                </div>
              </div>
              <span className={`font-bold ${stage.textColor}`}>{stage.value}%</span>
            </div>
            
            <div className={`h-2.5 ${stage.colorLight} rounded-full overflow-hidden`}>
              <div
                className={`h-full ${stage.color} transition-all duration-700 ease-out rounded-full`}
                style={{ width: `${stage.value}%` }}
              />
            </div>
          </div>
        ))}
      </div>

      <div className="mt-6 pt-4 border-t border-gray-100">
        <div className="relative flex justify-between items-start">
          {/* Connecting Lines */}
          <div className="absolute top-5 left-[16.6%] right-[16.6%] h-0.5 bg-gray-200 -translate-y-1/2 z-0">
            <div 
              className="h-full bg-[#64aaeb] transition-all duration-500"
              style={{ 
                width: concrete >= 80 
                  ? pictorial >= 80 
                    ? '100%' 
                    : '50%' 
                  : '0%' 
              }}
            />
          </div>

          {/* Stepper items */}
          {stages.map((stage, idx) => (
            <div key={stage.key} className="flex flex-col items-center flex-1 z-10">
              <div
                className={`w-10 h-10 rounded-full flex items-center justify-center text-lg font-bold transition-all ${
                  stage.value >= 50
                    ? 'bg-[#64aaeb] text-white'
                    : 'bg-gray-200 text-gray-400'
                } shadow-sm`}
              >
                {idx + 1}
              </div>
              <span className="mt-2 text-xs font-semibold text-gray-700 text-center">
                {stage.sublabel}
              </span>
            </div>
          ))}
        </div>
        
        <div className="mt-4 p-3 bg-indigo-50 rounded-xl border border-indigo-100">
          <p className="text-sm text-indigo-800">
            <span className="font-semibold">💡 Gợi ý: </span>
            {currentStage === 'concrete' && 'Con đang ở giai đoạn Concrete. Hãy cho con thực hành với đồ vật thật.'}
            {currentStage === 'pictorial' && 'Con đã sẵn sàng chuyển sang giai đoạn Pictorial. Dùng hình ảnh để minh họa.'}
            {currentStage === 'abstract' && 'Con đang tiến bộ tốt! Có thể bắt đầu thử các bài tập Abstract đơn giản.'}
          </p>
        </div>
      </div>
    </div>
  );
};

export default memo(CPAGauge);
