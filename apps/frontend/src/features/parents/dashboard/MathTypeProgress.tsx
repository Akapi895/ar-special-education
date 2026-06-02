/**
 * Math Type Progress Component - Redesigned
 * Clean design with unified indigo progress bars
 */

import { memo } from 'react';

interface MathProgress {
  counting: { current: number; target: number };
  comparison: { current: number; target: number };
  addition: { current: number; target: number };
  subtraction: { current: number; target: number };
}

interface MathTypeProgressProps {
  progress: MathProgress;
  compact?: boolean;
}

const MathTypeProgress = ({ progress, compact = false }: MathTypeProgressProps) => {
  const mathTypes = [
    {
      key: 'counting',
      label: 'Đếm số',
      icon: '🔢',
      progress: progress.counting,
    },
    {
      key: 'comparison',
      label: 'So sánh',
      icon: '⚖️',
      progress: progress.comparison,
    },
    {
      key: 'addition',
      label: 'Phép cộng',
      icon: '➕',
      progress: progress.addition,
    },
    {
      key: 'subtraction',
      label: 'Phép trừ',
      icon: '➖',
      progress: progress.subtraction,
    },
  ];

  const calculatePercentage = (current: number, target: number) => {
    if (target === 0) return 0;
    return Math.round((current / target) * 100);
  };

  if (compact) {
    return (
      <div className="grid grid-cols-2 gap-3">
        {mathTypes.map((type) => {
          const percentage = calculatePercentage(type.progress.current, type.progress.target);
          
          return (
            <div key={type.key} className="flex items-center gap-2">
              <span className="text-lg">{type.icon}</span>
              <div className="flex-1">
                <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-[#64aaeb] transition-all duration-500 rounded-full"
                    style={{ width: `${percentage}%` }}
                  />
                </div>
              </div>
              <span className="text-xs font-medium text-gray-600">{percentage}%</span>
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5">
      <h3 className="text-lg font-bold text-gray-900 mb-4">Tiến độ theo loại toán</h3>
      
      <div className="space-y-4">
        {mathTypes.map((type) => {
          const percentage = calculatePercentage(type.progress.current, type.progress.target);
          
          return (
            <div key={type.key} className="space-y-2">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="text-lg">{type.icon}</span>
                  <span className="font-medium text-gray-900">{type.label}</span>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm text-gray-500">
                    {type.progress.current}/{type.progress.target}
                  </span>
                  <span className="font-bold text-[#64aaeb]">{percentage}%</span>
                </div>
              </div>
              
              <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                <div
                  className="h-full bg-[#64aaeb] transition-all duration-500 rounded-full"
                  style={{ width: `${percentage}%` }}
                />
              </div>
            </div>
          );
        })}
      </div>

      <div className="mt-4 pt-4 border-t border-gray-100">
        <div className="flex flex-wrap gap-4 text-xs text-gray-500">
          {mathTypes.map((type) => (
            <span key={type.key} className="flex items-center gap-1.5">
              <span>{type.icon}</span>
              <span>{type.label}</span>
            </span>
          ))}
        </div>
      </div>
    </div>
  );
};

export default memo(MathTypeProgress);
