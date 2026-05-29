/**
 * Journal Page - Emotion & Learning Journal
 */

import { useState } from 'react';
import { Plus, Heart, TrendingUp } from 'lucide-react';
import Button from '../../components/ui/Button';
import EmptyState from '../../components/ui/EmptyState';
import PageContainer from '../../components/layout/PageContainer';
import { mockEmotionEntries } from '../../api/mockData';

const EMOTION_CONFIG = {
  happy: { emoji: '😊', label: 'Vui vẻ', bg: 'bg-green-50', text: 'text-green-700' },
  proud: { emoji: '😎', label: 'Tự hào', bg: 'bg-blue-50', text: 'text-blue-700' },
  neutral: { emoji: '😐', label: 'Bình thường', bg: 'bg-gray-50', text: 'text-gray-700' },
  frustrated: { emoji: '😟', label: 'Nản lòng', bg: 'bg-amber-50', text: 'text-amber-700' },
  anxious: { emoji: '😰', label: 'Lo lắng', bg: 'bg-red-50', text: 'text-red-700' },
};

const JournalPage = () => {
  const [activeTab, setActiveTab] = useState<'emotions' | 'report'>('emotions');
  const entries = mockEmotionEntries;

  return (
    <div className="min-h-screen bg-light-bg">
      <PageContainer maxWidth="lg" padding="md" spacing="md" className="py-4 sm:py-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Nhật ký học tập</h1>
        <p className="text-gray-600">
          Theo dõi cảm xúc và tiến độ học tập của con
        </p>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 mb-6">
        <button
          onClick={() => setActiveTab('emotions')}
          className={`px-4 py-2 rounded-lg font-medium transition-colors ${
            activeTab === 'emotions'
              ? 'bg-indigo-600 text-white'
              : 'bg-white text-gray-700 hover:bg-gray-100 border border-gray-200'
          }`}
        >
          <Heart className="w-4 h-4 inline mr-2" />
          Nhật ký cảm xúc
        </button>
        <button
          onClick={() => setActiveTab('report')}
          className={`px-4 py-2 rounded-lg font-medium transition-colors ${
            activeTab === 'report'
              ? 'bg-indigo-600 text-white'
              : 'bg-white text-gray-700 hover:bg-gray-100 border border-gray-200'
          }`}
        >
          <TrendingUp className="w-4 h-4 inline mr-2" />
          Báo cáo tuần
        </button>
      </div>

      {/* Add Button */}
      <Button className="mb-6 bg-indigo-600 hover:bg-indigo-700">
        <Plus className="w-4 h-4 mr-2" />
        Thêm ghi chép mới
      </Button>

      {/* Entries List */}
      <div className="space-y-4">
        {entries.map((entry) => {
          const emotion = EMOTION_CONFIG[entry.emotions.primary];
          const date = new Date(entry.date);
          const formattedDate = date.toLocaleDateString('vi-VN', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            year: 'numeric',
          });

          return (
            <div
              key={entry.id}
              className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow"
            >
              {/* Header */}
              <div className="flex items-start justify-between mb-4">
                <div className="flex items-center gap-3">
                  <div className={`p-2 rounded-xl ${emotion.bg}`}>
                    <span className="text-2xl">{emotion.emoji}</span>
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">{emotion.label}</p>
                    <p className="text-sm text-gray-500">
                      {formattedDate} - {entry.time}
                    </p>
                  </div>
                </div>
                <div className="flex gap-2">
                  <button className="text-sm text-gray-500 hover:text-indigo-600">
                    Sửa
                  </button>
                  <button className="text-sm text-gray-500 hover:text-red-600">
                    Xóa
                  </button>
                </div>
              </div>

              {/* Content */}
              <div className="space-y-3">
                {entry.description && (
                  <p className="text-gray-700">{entry.description}</p>
                )}

                {entry.triggers && entry.triggers.length > 0 && (
                  <div>
                    <p className="text-sm font-medium text-gray-700 mb-1">
                      💡 Điều gì khiến con cảm thấy như vậy:
                    </p>
                    <ul className="text-sm text-gray-600 space-y-1">
                      {entry.triggers.map((trigger, i) => (
                        <li key={i} className="flex items-center gap-2">
                          <span className="w-1.5 h-1.5 bg-gray-400 rounded-full"></span>
                          {trigger}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}

                {entry.parentResponse && (
                  <div className="p-3 bg-indigo-50 rounded-xl border border-indigo-100">
                    <p className="text-sm text-indigo-800">
                      <span className="font-medium">📝 Phản hồi của phụ huynh:</span>{' '}
                      {entry.parentResponse}
                    </p>
                  </div>
                )}

                {entry.followUpActions && entry.followUpActions.length > 0 && (
                  <div>
                    <p className="text-sm font-medium text-gray-700 mb-1">
                      📋 Hành động tiếp theo:
                    </p>
                    <ul className="text-sm text-gray-600 space-y-1">
                      {entry.followUpActions.map((action, i) => (
                        <li key={i} className="flex items-center gap-2">
                          <span className="w-1.5 h-1.5 bg-indigo-400 rounded-full"></span>
                          {action}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Empty State */}
      {entries.length === 0 && (
        <EmptyState
          icon="inbox"
          title="Chưa có ghi chép nào"
          description="Bắt đầu ghi nhận cảm xúc và tiến độ học tập của con"
        />
      )}
      </PageContainer>
    </div>
  );
};

export default JournalPage;
