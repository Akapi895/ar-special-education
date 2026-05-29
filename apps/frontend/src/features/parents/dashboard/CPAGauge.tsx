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
      color: 'bg-blue-600',
      colorLight: 'bg-blue-50',
      textColor: 'text-blue-600',
      icon: '🧱',
    },
    {
      key: 'pictorial',
      label: 'Pictorial',
      sublabel: 'Hình ảnh',
      value: pictorial,
      color: 'bg-amber-500',
      colorLight: 'bg-amber-50',
      textColor: 'text-amber-600',
      icon: '🎨',
    },
    {
      key: 'abstract',
      label: 'Abstract',
      sublabel: 'Trừu tượng',
      value: abstract,
      color: 'bg-violet-600',
      colorLight: 'bg-violet-50',
      textColor: 'text-violet-600',
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
        <div className="flex items-center justify-between">
          {stages.map((stage, idx) => (
            <div key={stage.key} className="flex items-center">
              <div
                className={`w-10 h-10 rounded-full flex items-center justify-center text-lg font-bold transition-all ${
                  stage.value >= 50
                    ? stage.color
                    : 'bg-gray-200'
                } text-white shadow-sm`}
              >
                {idx + 1}
              </div>
              {idx < stages.length - 1 && (
                <div className="w-8 h-0.5 bg-gray-200 mx-1">
                  <div
                    className={`h-full ${stage.color} transition-all duration-500`}
                    style={{ width: stage.value >= 80 ? '100%' : `${Math.min(stage.value, 100)}%` }}
                  />
                </div>
              )}
            </div>
          ))}
        </div>
        
        <div className="flex justify-between mt-2 text-xs text-gray-500 font-medium">
          <span>Cụ thể</span>
          <span>Hình ảnh</span>
          <span>Trừu tượng</span>
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
