/**
 * Stats Overview Component - Redesigned
 * Clean, minimal design with unified 2D icons and details layout
 */

import { memo } from 'react';
import { BookOpen, Clock, Flame, Target } from 'lucide-react';

interface StatsOverviewProps {
  exercisesCompleted: number;
  totalExercises: number;
  totalTimeSpent: number;
  streak: number;
  correctRate: number;
  incorrectRate: number;
}

const StatsOverview = ({
  exercisesCompleted,
  totalExercises,
  totalTimeSpent,
  streak,
  correctRate,
  incorrectRate,
}: StatsOverviewProps) => {
  const formatTime = (minutes: number) => {
    if (minutes < 60) return `${minutes}p`;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}g ${mins}p` : `${hours}g`;
  };

  const stats = [
    {
      key: 'exercises',
      label: 'Bài tập',
      value: `${exercisesCompleted}/${totalExercises}`,
      Icon: BookOpen,
      bgTint: 'bg-[#64aaeb]/10 text-[#64aaeb] border border-[#64aaeb]/20',
    },
    {
      key: 'time',
      label: 'Thời gian',
      value: formatTime(totalTimeSpent),
      Icon: Clock,
      bgTint: 'bg-indigo-50 text-indigo-600 border border-indigo-100/50',
    },
    {
      key: 'streak',
      label: 'Streak',
      value: `${streak} ngày`,
      Icon: Flame,
      bgTint: 'bg-orange-50 text-orange-600 border border-orange-100/50',
    },
    {
      key: 'accuracy',
      label: 'Độ chính xác',
      value: `${correctRate}%`,
      Icon: Target,
      bgTint: 'bg-emerald-50 text-emerald-600 border border-emerald-100/50',
      subValue: `Sai: ${incorrectRate}%`,
    },
  ];

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
      {stats.map((stat) => (
        <div
          key={stat.key}
          className="bg-white rounded-2xl p-4 border border-gray-100 hover:shadow-md transition-all duration-300 flex items-center gap-4"
        >
          {/* Left Column: Icon with rounded border bg */}
          <div className={`w-12 h-12 flex items-center justify-center rounded-2xl flex-shrink-0 ${stat.bgTint}`}>
            <stat.Icon className="w-5.5 h-5.5" />
          </div>

          {/* Right Column: Title on top, stats below */}
          <div className="flex-1 min-w-0">
            <p className="text-xs text-gray-400 font-bold uppercase tracking-wider">{stat.label}</p>
            <p className="text-xl font-bold text-gray-900 mt-0.5 leading-none">{stat.value}</p>
            {stat.subValue && (
              <p className="text-[10px] text-gray-500 mt-1 font-semibold">{stat.subValue}</p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

export default memo(StatsOverview);
