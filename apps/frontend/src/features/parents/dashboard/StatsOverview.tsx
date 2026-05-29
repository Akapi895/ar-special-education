/**
 * Stats Overview Component - Redesigned
 * Clean, minimal design with accent colors only
 */

import { memo } from 'react';

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
      icon: '📚',
      accentColor: 'blue',
    },
    {
      key: 'time',
      label: 'Thời gian',
      value: formatTime(totalTimeSpent),
      icon: '⏱️',
      accentColor: 'indigo',
    },
    {
      key: 'streak',
      label: 'Streak',
      value: `${streak} ngày`,
      icon: '🔥',
      accentColor: 'orange',
    },
    {
      key: 'accuracy',
      label: 'Độ chính xác',
      value: `${correctRate}%`,
      icon: '📊',
      accentColor: 'violet',
      subValue: `Sai: ${incorrectRate}%`,
    },
  ];

  const accentColors = {
    blue: 'text-blue-600',
    indigo: 'text-indigo-600',
    orange: 'text-orange-500',
    violet: 'text-violet-600',
  };

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
      {stats.map((stat) => (
        <div
          key={stat.key}
          className="bg-white rounded-2xl p-4 border border-gray-100 hover:shadow-md transition-all duration-300"
        >
          <div className="flex items-start justify-between">
            <div>
              <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
              {stat.subValue && (
                <p className="text-xs text-gray-500 mt-0.5">{stat.subValue}</p>
              )}
              <p className="text-sm text-gray-600 mt-1 font-medium">{stat.label}</p>
            </div>
            <div className={`${accentColors[stat.accentColor as keyof typeof accentColors]} p-2`}>
              <span className="text-xl">{stat.icon}</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default memo(StatsOverview);
