/**
 * Emotion Tracker Component - Redesigned
 * Clean, minimal design with unified color palette
 */

import { memo } from 'react';
import { Heart } from 'lucide-react';

interface EmotionEntry {
  date: string;
  emotion: 'happy' | 'frustrated' | 'anxious' | 'neutral' | 'proud';
  note?: string;
}

interface EmotionTrackerProps {
  entries: EmotionEntry[];
  onAddEntry: () => void;
  onViewAll: () => void;
}

const EMOTION_CONFIG: Record<string, { emoji: string; label: string; bg: string }> = {
  happy: { emoji: '😊', label: 'Vui vẻ', bg: 'bg-green-50' },
  proud: { emoji: '😎', label: 'Tự hào', bg: 'bg-blue-50' },
  neutral: { emoji: '😐', label: 'Bình thường', bg: 'bg-gray-50' },
  frustrated: { emoji: '😟', label: 'Nản lòng', bg: 'bg-amber-50' },
  anxious: { emoji: '😰', label: 'Lo lắng', bg: 'bg-red-50' },
};

const DAYS = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

const EmotionTracker = ({ entries, onAddEntry, onViewAll }: EmotionTrackerProps) => {
  const getLast7Days = () => {
    const days: { date: string; dayLabel: string; isToday: boolean; entry: EmotionEntry | undefined }[] = [];
    for (let i = 6; i >= 0; i--) {
      const date = new Date();
      date.setDate(date.getDate() - i);
      const dateStr = date.toISOString().split('T')[0];
      const dayIndex = date.getDay() === 0 ? 6 : date.getDay() - 1;
      days.push({
        date: dateStr,
        dayLabel: DAYS[dayIndex],
        isToday: i === 0,
        entry: entries.find(e => e.date === dateStr),
      });
    }
    return days;
  };

  const last7Days = getLast7Days();

  const emotionCounts = entries.reduce((acc, entry) => {
    acc[entry.emotion] = (acc[entry.emotion] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const totalEntries = entries.length || 1;

  const getOverallTrend = () => {
    if (entries.length === 0) return { emoji: '😐', text: 'Chưa có dữ liệu' };
    
    const positive = (emotionCounts['happy'] || 0) + (emotionCounts['proud'] || 0);
    const negative = (emotionCounts['frustrated'] || 0) + (emotionCounts['anxious'] || 0);
    
    if (positive > negative * 2) return { emoji: '😊', text: 'Con đang rất tích cực!' };
    if (positive > negative) return { emoji: '🙂', text: 'Con ổn định, có tiến bộ' };
    if (negative > positive) return { emoji: '😟', text: 'Con có vẻ gặp khó khăn' };
    return { emoji: '😐', text: 'Cảm xúc con khá bình thường' };
  };

  const trend = getOverallTrend();

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5 flex flex-col">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-bold text-gray-900">Nhật ký cảm xúc</h3>
        <button
          onClick={onViewAll}
          className="text-sm text-indigo-600 hover:text-indigo-700 font-medium"
        >
          Xem tất cả
        </button>
      </div>

      {/* Week Calendar */}
      <div className="grid grid-cols-7 gap-1.5 mb-4 overflow-x-auto pb-1 min-w-[280px]">
        {last7Days.map((day, idx) => {
          const emotion = day.entry?.emotion;
          const config = emotion ? EMOTION_CONFIG[emotion] : null;
          const todayIndex = last7Days.findIndex(d => d.isToday);

          return (
            <div
              key={day.date}
              className={`flex flex-col items-center justify-center h-16 rounded-xl transition-all border-2 ${
                day.isToday
                  ? day.entry
                    ? 'bg-white border-[#64aaeb] shadow-xs font-semibold'
                    : 'bg-white border-dashed border-[#64aaeb] text-[#64aaeb] font-semibold'
                  : config
                    ? `${config.bg} border-transparent`
                    : 'bg-gray-50 border-transparent'
              } ${idx > todayIndex ? 'opacity-40' : ''}`}
            >
              <span className={`text-[10px] uppercase font-bold mb-0.5 ${
                day.isToday ? 'text-[#64aaeb]' : 'text-gray-400'
              }`}>
                {day.dayLabel}
              </span>
              <span className="text-xl leading-none">
                {config ? config.emoji : idx > todayIndex ? '⏳' : '➕'}
              </span>
            </div>
          );
        })}
      </div>

      {/* Emotion Summary */}
      <div className="grid grid-cols-2 gap-2 mb-4">
        <div className="p-3 rounded-xl bg-gray-50 border border-gray-100">
          <p className="text-xs text-gray-500">Vui vẻ</p>
          <p className="text-lg font-bold text-gray-900">
            {Math.round(((emotionCounts['happy'] || 0) / totalEntries) * 100)}%
          </p>
        </div>
        <div className="p-3 rounded-xl bg-gray-50 border border-gray-100">
          <p className="text-xs text-gray-500">Tự hào</p>
          <p className="text-lg font-bold text-gray-900">
            {Math.round(((emotionCounts['proud'] || 0) / totalEntries) * 100)}%
          </p>
        </div>
      </div>

      {/* Trend */}
      <div className="p-3 bg-gray-50 rounded-xl border border-gray-100 mb-4">
        <div className="flex items-center gap-3">
          <span className="text-2xl">{trend.emoji}</span>
          <div>
            <p className="text-sm font-medium text-gray-900">Xu hướng tuần này</p>
            <p className="text-xs text-gray-500">{trend.text}</p>
          </div>
        </div>
      </div>

      {/* Action Button */}
      <button
        onClick={onAddEntry}
        className="w-full flex items-center justify-center gap-2 bg-[#64aaeb] hover:bg-[#5299da] text-white font-semibold px-5 py-2.5 rounded-full transition-colors shadow-sm text-sm"
      >
        <Heart className="w-4.5 h-4.5" />
        Ghi nhận cảm xúc hôm nay
      </button>
    </div>
  );
};

export default memo(EmotionTracker);
