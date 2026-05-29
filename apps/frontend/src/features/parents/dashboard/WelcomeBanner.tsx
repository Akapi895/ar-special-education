/**
 * Welcome Banner Component - Redesigned
 * Clean design with improved typography
 */

import { memo } from 'react';
import { Flame, Calendar } from 'lucide-react';

interface WelcomeBannerProps {
  childName: string;
  childAge: number;
  streak: number;
}

const WelcomeBanner = ({ childName, childAge, streak }: WelcomeBannerProps) => {
  const today = new Date();
  const formattedDate = today.toLocaleDateString('vi-VN', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });

  return (
    <div className="bg-slate-900 rounded-2xl p-6 text-white shadow-lg">
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        {/* Left side - Greeting */}
        <div>
          <h1 className="text-2xl md:text-3xl font-bold mb-3">
            Chào mừng trở lại! 👋
          </h1>
          <div className="flex flex-wrap items-center gap-4 text-slate-300">
            <div className="flex items-center gap-2">
              <span className="text-xl">👶</span>
              <span className="font-medium text-white">
                Con: <span className="font-bold">{childName}</span> ({childAge} tuổi)
              </span>
            </div>
            
            {streak > 0 && (
              <div className="flex items-center gap-2 bg-white/10 px-3 py-1.5 rounded-full">
                <Flame className="w-5 h-5 text-orange-400" />
                <span className="font-bold">{streak} ngày</span>
              </div>
            )}
          </div>
        </div>

        {/* Right side - Date */}
        <div className="flex items-center gap-3 text-slate-300 bg-white/10 px-4 py-3 rounded-xl">
          <Calendar className="w-5 h-5 text-white" />
          <span className="text-sm font-medium text-white capitalize">{formattedDate}</span>
        </div>
      </div>
    </div>
  );
};

export default memo(WelcomeBanner);
