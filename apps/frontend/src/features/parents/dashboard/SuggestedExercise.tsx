/**
 * Suggested Exercise Component - Redesigned
 * Clean design with better button layout and spacing
 */

import { memo } from 'react';
import { ArrowRight } from 'lucide-react';

interface SuggestedExercise {
  id: string;
  title: string;
  cpaStage: 'concrete' | 'pictorial' | 'abstract';
  description: string;
  reason: string;
  mathType: string;
  difficulty: number;
}

interface SuggestedExerciseCardProps {
  exercise: SuggestedExercise;
  onStart: () => void;
  onViewDetails: () => void;
}

const CPA_STAGE_CONFIG = {
  concrete: {
    label: 'Concrete',
    sublabel: 'Cụ thể',
    color: 'blue',
    bg: 'bg-blue-50 border-blue-200',
    text: 'text-blue-700',
    icon: '🧱',
  },
  pictorial: {
    label: 'Pictorial',
    sublabel: 'Hình ảnh',
    color: 'amber',
    bg: 'bg-amber-50 border-amber-200',
    text: 'text-amber-700',
    icon: '🎨',
  },
  abstract: {
    label: 'Abstract',
    sublabel: 'Trừu tượng',
    color: 'violet',
    bg: 'bg-violet-50 border-violet-200',
    text: 'text-violet-700',
    icon: '🔢',
  },
};

const SuggestedExerciseCard = ({ exercise, onStart, onViewDetails }: SuggestedExerciseCardProps) => {
  const stageConfig = CPA_STAGE_CONFIG[exercise.cpaStage];
  const difficultyStars = '⭐'.repeat(exercise.difficulty) + '☆'.repeat(3 - exercise.difficulty);

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
      {/* Header */}
      <div className="bg-slate-900 px-5 py-3">
        <div className="flex items-center justify-between">
          <span className="text-white/90 text-sm font-medium">✨ Gợi ý bài tập hôm nay</span>
          <span className="text-white/70 text-xs">{difficultyStars}</span>
        </div>
      </div>

      {/* Content */}
      <div className="p-5">
        <div className="flex flex-col lg:flex-row lg:items-start gap-5">
          {/* Left - Main Content */}
          <div className="flex-1 space-y-4">
            {/* CPA Stage Badge */}
            <div className="flex items-center gap-2">
              <span
                className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm font-medium border ${stageConfig.bg} ${stageConfig.text}`}
              >
                <span>{stageConfig.icon}</span>
                <span>{stageConfig.label}</span>
                <span className="text-xs opacity-75">({stageConfig.sublabel})</span>
              </span>
              <span className="px-2.5 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-600">
                {exercise.mathType}
              </span>
            </div>

            {/* Title */}
            <h3 className="text-xl font-bold text-gray-900">{exercise.title}</h3>

            {/* Description */}
            <p className="text-gray-600 text-sm">{exercise.description}</p>

            {/* Reason */}
            <div className="p-3 bg-blue-50 rounded-xl border border-blue-100">
              <p className="text-sm text-blue-800">
                <span className="font-semibold">💡 Tại sao gợi ý bài này: </span>
                <span>{exercise.reason}</span>
              </p>
            </div>
          </div>

          {/* Right - Actions */}
          <div className="flex lg:flex-col gap-3 lg:min-w-[180px] lg:items-end">
            <button
              onClick={onStart}
              className="flex-1 lg:flex-none flex items-center justify-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold px-6 py-3 rounded-xl transition-colors shadow-sm"
            >
              ▶ Bắt đầu bài tập
            </button>
            <button
              onClick={onViewDetails}
              className="flex items-center justify-center gap-1.5 text-gray-600 hover:text-gray-900 font-medium px-4 py-3 rounded-xl hover:bg-gray-100 transition-colors"
            >
              Xem chi tiết
              <ArrowRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default memo(SuggestedExerciseCard);
export type { SuggestedExercise };
