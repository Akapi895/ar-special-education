/**
 * Activity Feed Component - Redesigned
 * Clean timeline with subtle separators
 */

import { memo } from 'react';
import { CheckCircle2, Circle, Trophy } from 'lucide-react';

interface Activity {
  id: string;
  type: 'exercise' | 'emotion' | 'milestone';
  time: string;
  title: string;
  details?: string;
  emotion?: 'happy' | 'frustrated' | 'anxious' | 'neutral' | 'proud';
  accuracy?: number;
  cpaStage?: 'concrete' | 'pictorial' | 'abstract';
}

interface ActivityFeedProps {
  activities: Activity[];
  onViewAll: () => void;
}

const EMOTION_EMOJIS: Record<string, string> = {
  happy: '😊',
  frustrated: '😟',
  anxious: '😰',
  neutral: '😐',
  proud: '😎',
};

const CPA_COLORS: Record<string, string> = {
  concrete: 'bg-blue-100 text-blue-700',
  pictorial: 'bg-amber-100 text-amber-700',
  abstract: 'bg-violet-100 text-violet-700',
};

const getTimeLabel = (timeStr: string) => {
  const time = new Date(timeStr);
  return time.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const ActivityFeed = ({ activities, onViewAll }: ActivityFeedProps) => {
  if (activities.length === 0) {
    return (
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
        <h3 className="text-lg font-bold text-gray-900 mb-4">Hoạt động gần đây</h3>
        <div className="flex flex-col items-center justify-center py-8 text-gray-400">
          <div className="text-5xl mb-3">📝</div>
          <p className="text-center font-medium">Chưa có hoạt động nào</p>
          <p className="text-sm text-center mt-1">
            Bắt đầu ghi nhận bài tập và cảm xúc của con để theo dõi tiến độ
          </p>
        </div>
      </div>
    );
  }

  const groupActivities = () => {
    const now = new Date();
    const today: Activity[] = [];
    const yesterday: Activity[] = [];
    const older: Activity[] = [];

    activities.forEach(activity => {
      const activityDate = new Date(activity.time);
      const diffDays = Math.floor((now.getTime() - activityDate.getTime()) / 86400000);
      
      if (diffDays === 0) today.push(activity);
      else if (diffDays === 1) yesterday.push(activity);
      else older.push(activity);
    });

    return { today, yesterday, older };
  };

  const grouped = groupActivities();

  const renderActivityIcon = (activity: Activity) => {
    switch (activity.type) {
      case 'exercise':
        return activity.accuracy !== undefined && activity.accuracy >= 70 ? (
          <CheckCircle2 className="w-4 h-4 text-green-500" />
        ) : (
          <Circle className="w-4 h-4 text-gray-400" />
        );
      case 'emotion':
        return <span className="text-lg">{EMOTION_EMOJIS[activity.emotion || 'neutral']}</span>;
      case 'milestone':
        return <Trophy className="w-4 h-4 text-amber-500" />
      default:
        return <Circle className="w-4 h-4 text-gray-400" />;
    }
  };

  const renderActivityItem = (activity: Activity) => (
    <div
      key={activity.id}
      className="flex items-start gap-3 py-3 border-b border-gray-100 last:border-0 hover:bg-gray-50/50 -mx-2 px-2 rounded-lg transition-colors"
    >
      {/* Icon */}
      <div className="flex-shrink-0 w-8 h-8 flex items-center justify-center">
        {renderActivityIcon(activity)}
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-0.5">
          <p className="font-medium text-gray-900 text-sm truncate">{activity.title}</p>
          {activity.cpaStage && (
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${CPA_COLORS[activity.cpaStage]}`}>
              {activity.cpaStage}
            </span>
          )}
        </div>
        
        {activity.details && (
          <p className="text-xs text-gray-500 line-clamp-1">{activity.details}</p>
        )}

        <div className="flex items-center gap-3 mt-1 text-xs text-gray-400">
          <span>{getTimeLabel(activity.time)}</span>
          {activity.accuracy !== undefined && (
            <span className={activity.accuracy >= 70 ? 'text-green-600' : 'text-orange-600'}>
              {activity.accuracy}% đúng
            </span>
          )}
        </div>
      </div>
    </div>
  );

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5 h-full">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-bold text-gray-900">Hoạt động gần đây</h3>
        <button
          onClick={onViewAll}
          className="text-sm text-indigo-600 hover:text-indigo-700 font-medium"
        >
          Xem tất cả
        </button>
      </div>

      <div className="space-y-4">
        {grouped.today.length > 0 && (
          <div>
            <h4 className="text-xs font-semibold text-gray-500 uppercase mb-1 flex items-center gap-2">
              <span className="w-1.5 h-1.5 bg-green-500 rounded-full"></span>
              Hôm nay
            </h4>
            <div>
              {grouped.today.map(renderActivityItem)}
            </div>
          </div>
        )}

        {grouped.yesterday.length > 0 && (
          <div>
            <h4 className="text-xs font-semibold text-gray-500 uppercase mb-1 flex items-center gap-2">
              <span className="w-1.5 h-1.5 bg-gray-400 rounded-full"></span>
              Hôm qua
            </h4>
            <div>
              {grouped.yesterday.map(renderActivityItem)}
            </div>
          </div>
        )}

        {grouped.older.length > 0 && (
          <div>
            <h4 className="text-xs font-semibold text-gray-500 uppercase mb-1 flex items-center gap-2">
              <span className="w-1.5 h-1.5 bg-gray-300 rounded-full"></span>
              Trước đó
            </h4>
            <div>
              {grouped.older.slice(0, 3).map(renderActivityItem)}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default memo(ActivityFeed);
export type { Activity };
