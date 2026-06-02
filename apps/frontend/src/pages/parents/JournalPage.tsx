/**
 * Journal Page - Emotion & Learning Journal
 */

import { useState } from 'react';
import { Plus, Heart, TrendingUp, Pencil, Trash2, Lightbulb, CheckCircle2, MessageSquare } from 'lucide-react';
import Button from '../../components/ui/Button';
import EmptyState from '../../components/ui/EmptyState';
import PageContainer from '../../components/layout/PageContainer';
import { mockEmotionEntries } from '../../api/mockData';

const EMOTION_CONFIG = {
  happy: { emoji: '😊', label: 'Vui vẻ', bg: 'bg-green-50', text: 'text-green-700', border: 'border-green-400' },
  proud: { emoji: '😎', label: 'Tự hào', bg: 'bg-blue-50', text: 'text-blue-700', border: 'border-blue-400' },
  neutral: { emoji: '😐', label: 'Bình thường', bg: 'bg-gray-50', text: 'text-gray-700', border: 'border-gray-300' },
  frustrated: { emoji: '😟', label: 'Nản lòng', bg: 'bg-amber-50', text: 'text-amber-700', border: 'border-amber-400' },
  anxious: { emoji: '😰', label: 'Lo lắng', bg: 'bg-red-50', text: 'text-red-700', border: 'border-red-400' },
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

      {/* Tabs and Action Row */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div className="flex gap-2">
          <button
            onClick={() => setActiveTab('emotions')}
            className={`px-4 py-2 rounded-full font-medium transition-colors ${
              activeTab === 'emotions'
                ? 'bg-[#64aaeb] hover:bg-[#5299da] text-white'
                : 'bg-white text-gray-700 hover:bg-gray-100 border border-gray-200'
            }`}
          >
            <Heart className="w-4 h-4 inline mr-2" />
            Nhật ký cảm xúc
          </button>
          <button
            onClick={() => setActiveTab('report')}
            className={`px-4 py-2 rounded-full font-medium transition-colors ${
              activeTab === 'report'
                ? 'bg-[#64aaeb] hover:bg-[#5299da] text-white'
                : 'bg-white text-gray-700 hover:bg-gray-100 border border-gray-200'
            }`}
          >
            <TrendingUp className="w-4 h-4 inline mr-2" />
            Báo cáo tuần
          </button>
        </div>

        <Button className="bg-[#64aaeb] hover:bg-[#5299da] rounded-full self-start sm:self-auto">
          <Plus className="w-4 h-4 mr-2" />
          Thêm ghi chép mới
        </Button>
      </div>

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
                  <button className="p-2 border border-emerald-100 hover:border-emerald-300 text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors shadow-2xs" title="Sửa">
                    <Pencil className="w-4 h-4" />
                  </button>
                  <button className="p-2 border border-red-100 hover:border-red-300 text-red-600 hover:bg-red-50 rounded-lg transition-colors shadow-2xs" title="Xóa">
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>

              {/* Content */}
              <div className="space-y-4">
                {entry.description && (
                  <div className={`p-4 bg-gray-50/70 border-l-4 ${emotion.border} rounded-r-2xl text-gray-700 italic font-medium leading-relaxed my-2 shadow-2xs`}>
                    "{entry.description}"
                  </div>
                )}

                {/* Details Grid: Two columns on md/lg screens */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Left Column: Triggers & Follow-up Actions */}
                  <div className="space-y-3">
                    {/* Triggers (Lý do) */}
                    {entry.triggers && entry.triggers.length > 0 && (
                      <div className="p-3 bg-amber-50/30 rounded-xl border border-amber-100/50">
                        <p className="text-sm font-bold text-amber-800 mb-2 flex items-center gap-1.5">
                          <Lightbulb className="w-4 h-4 text-amber-500 flex-shrink-0" />
                          Lý do khiến con cảm thấy như vậy:
                        </p>
                        <ul className="text-sm text-amber-900/80 space-y-1.5">
                          {entry.triggers.map((trigger, i) => (
                            <li key={i} className="flex items-start gap-2">
                              <Lightbulb className="w-3.5 h-3.5 text-amber-500/70 mt-0.5 flex-shrink-0" />
                              <span>{trigger}</span>
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {/* Follow Up Actions (Hành động tiếp theo) */}
                    {entry.followUpActions && entry.followUpActions.length > 0 && (
                      <div className="p-3 bg-emerald-50/30 rounded-xl border border-emerald-100/50">
                        <p className="text-sm font-bold text-emerald-800 mb-2 flex items-center gap-1.5">
                          <CheckCircle2 className="w-4 h-4 text-emerald-500 flex-shrink-0" />
                          Hành động tiếp theo:
                        </p>
                        <ul className="text-sm text-emerald-900/80 space-y-1.5">
                          {entry.followUpActions.map((action, i) => (
                            <li key={i} className="flex items-start gap-2">
                              <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500/70 mt-0.5 flex-shrink-0" />
                              <span>{action}</span>
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}
                  </div>

                  {/* Right Column: Parent Response */}
                  <div className="h-full">
                    {entry.parentResponse && (
                      <div className="p-4 bg-[#64aaeb]/5 rounded-xl border border-[#64aaeb]/20 h-full flex flex-col justify-start">
                        <p className="text-sm font-bold text-[#1e3a8a] mb-2 flex items-center gap-1.5">
                          <MessageSquare className="w-4 h-4 text-[#64aaeb] flex-shrink-0" />
                          Phản hồi của phụ huynh:
                        </p>
                        <p className="text-sm text-gray-700 leading-relaxed italic bg-white/80 p-3 rounded-lg border border-[#64aaeb]/10 shadow-3xs flex-1">
                          {entry.parentResponse}
                        </p>
                      </div>
                    )}
                  </div>
                </div>
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
