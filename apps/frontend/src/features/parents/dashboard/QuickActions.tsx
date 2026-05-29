/**
 * Quick Actions Component - Redesigned
 * Compact grid layout with consistent styling
 */

import { memo } from 'react';
import { BookOpen, Heart, Lightbulb, BookMarked, BarChart3, Settings } from 'lucide-react';

interface QuickActionsProps {
  onAddExercise: () => void;
  onAddEmotion: () => void;
  onViewSuggestions: () => void;
  onViewMethods: () => void;
  onViewReport: () => void;
  onOpenSettings: () => void;
}

interface QuickAction {
  key: string;
  icon: React.ElementType;
  label: string;
  description: string;
  onClick: () => void;
}

const QuickActions = ({
  onAddExercise,
  onAddEmotion,
  onViewSuggestions,
  onViewMethods,
  onViewReport,
  onOpenSettings,
}: QuickActionsProps) => {
  const actions: QuickAction[] = [
    {
      key: 'add-exercise',
      icon: BookOpen,
      label: 'Ghi nhận bài tập',
      description: 'Lưu bài tập đã làm',
      onClick: onAddExercise,
    },
    {
      key: 'add-emotion',
      icon: Heart,
      label: 'Ghi nhận cảm xúc',
      description: 'Theo dõi cảm xúc',
      onClick: onAddEmotion,
    },
    {
      key: 'suggestions',
      icon: Lightbulb,
      label: 'Bài tập gợi ý',
      description: 'Xem bài phù hợp',
      onClick: onViewSuggestions,
    },
    {
      key: 'methods',
      icon: BookMarked,
      label: 'Phương pháp CPA',
      description: 'Hướng dẫn dạy con',
      onClick: onViewMethods,
    },
    {
      key: 'report',
      icon: BarChart3,
      label: 'Báo cáo tuần',
      description: 'Xem tổng kết',
      onClick: onViewReport,
    },
    {
      key: 'settings',
      icon: Settings,
      label: 'Cài đặt',
      description: 'Quản lý tài khoản',
      onClick: onOpenSettings,
    },
  ];

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5">
      <h3 className="text-lg font-bold text-gray-900 mb-4">Hành động nhanh</h3>
      
      <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-3">
        {actions.map((action) => {
          const Icon = action.icon;
          
          return (
            <button
              key={action.key}
              onClick={action.onClick}
              className="flex flex-col items-center p-4 rounded-xl bg-gray-50 hover:bg-gray-100 transition-all duration-200 group"
            >
              <div className="p-2.5 rounded-xl bg-white shadow-sm mb-2 group-hover:scale-110 transition-transform text-indigo-600">
                <Icon className="w-5 h-5" />
              </div>
              <span className="text-sm font-medium text-gray-900 text-center leading-tight">
                {action.label}
              </span>
              <span className="text-xs text-gray-500 text-center mt-0.5">
                {action.description}
              </span>
            </button>
          );
        })}
      </div>
    </div>
  );
};

export default memo(QuickActions);
