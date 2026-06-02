/**
 * Dashboard Page - MathMate Support
 * Clean, minimal SaaS-style design
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { SkeletonCard, SkeletonChart } from '../../components/ui';
import PageContainer from '../../components/layout/PageContainer';

// Import dashboard components
import WelcomeBanner from '../../features/parents/dashboard/WelcomeBanner';
import CPAGauge from '../../features/parents/dashboard/CPAGauge';
import MathTypeProgress from '../../features/parents/dashboard/MathTypeProgress';
import StatsOverview from '../../features/parents/dashboard/StatsOverview';
import SuggestedExerciseCard from '../../features/parents/dashboard/SuggestedExercise';
import EmotionTracker from '../../features/parents/dashboard/EmotionTracker';
import ActivityFeed from '../../features/parents/dashboard/ActivityFeed';
import QuickActions from '../../features/parents/dashboard/QuickActions';

// Import services
import { mockChild } from '../../api/mockData';
import { mockProgress, mockSessionHistory } from '../../api/mockData/progress';
import { mockEmotionEntries } from '../../api/mockData/emotions';
import { getSuggestedExercises } from '../../api/services/exerciseService';
import type { SuggestedExercise } from '../../features/parents/dashboard/SuggestedExercise';

const DashboardPage = () => {
  const navigate = useNavigate();

  // State
  const [loading, setLoading] = useState(true);
  const [suggestedExercise, setSuggestedExercise] = useState<SuggestedExercise | null>(null);

  // Mock data
  const child = mockChild;
  const progress = mockProgress;
  const sessionHistory = mockSessionHistory;
  const emotionEntries = mockEmotionEntries;

  const loadDashboardData = useCallback(async () => {
    try {
      setLoading(true);
      
      const suggestions = await getSuggestedExercises(child.id, progress.cpaProgress);
      if (suggestions.length > 0) {
        const top = suggestions[0];
        setSuggestedExercise({
          id: top.id,
          title: top.title,
          cpaStage: top.cpaStage,
          description: top.description,
          reason: top.cpaStage === 'concrete' 
            ? 'Con đang ở giai đoạn Concrete. Hãy cho con thực hành với đồ vật thật.'
            : top.cpaStage === 'pictorial'
            ? 'Con đã sẵn sàng chuyển sang giai đoạn Pictorial. Dùng hình ảnh để minh họa.'
            : 'Con có thể bắt đầu thử các bài tập Abstract đơn giản.',
          mathType: top.mathType === 'counting' ? 'Đếm số' 
            : top.mathType === 'comparison' ? 'So sánh'
            : top.mathType === 'addition' ? 'Phép cộng'
            : 'Phép trừ',
          difficulty: top.difficulty,
        });
      }
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
    } finally {
      setLoading(false);
    }
  }, [child.id, progress.cpaProgress]);

  useEffect(() => {
    void loadDashboardData();
  }, [loadDashboardData]);

  const activities = useMemo(() => (
    sessionHistory.map((session) => ({
      id: session.id,
      type: 'exercise' as const,
      time: session.date,
      title: session.exerciseTitle,
      details: session.notes,
      accuracy: Math.round((session.correctCount / (session.correctCount + session.incorrectCount)) * 100),
      cpaStage: session.cpaStage,
    }))
  ), [sessionHistory]);

  const emotionActivities = useMemo(() => (
    emotionEntries.slice(0, 3).map((entry) => ({
      id: entry.id,
      type: 'emotion' as const,
      time: `${entry.date}T${entry.time}`,
      title: `Ghi nhận cảm xúc: ${entry.emotions.primary === 'happy' ? 'Vui vẻ'
        : entry.emotions.primary === 'frustrated' ? 'Nản lòng'
        : entry.emotions.primary === 'anxious' ? 'Lo lắng'
        : entry.emotions.primary === 'proud' ? 'Tự hào'
        : 'Bình thường'}`,
      details: entry.description.slice(0, 100),
      emotion: entry.emotions.primary,
    }))
  ), [emotionEntries]);

  const allActivities = useMemo(() => (
    [...activities, ...emotionActivities]
      .sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime())
      .slice(0, 10)
  ), [activities, emotionActivities]);

  // Handler functions
  const handleAddExercise = useCallback(() => navigate('/parent/exercises'), [navigate]);
  const handleAddEmotion = useCallback(() => navigate('/parent/journal'), [navigate]);
  const handleViewSuggestions = useCallback(() => navigate('/parent/exercises'), [navigate]);
  const handleViewMethods = useCallback(() => navigate('/parent/methods'), [navigate]);
  const handleViewReport = useCallback(() => navigate('/parent/journal'), [navigate]);
  const handleOpenSettings = useCallback(() => navigate('/parent/settings'), [navigate]);
  const handleStartExercise = useCallback((_exerciseId: string) => navigate('/parent/exercises'), [navigate]);
  const handleViewExerciseDetails = useCallback((_exerciseId: string) => navigate('/parent/exercises'), [navigate]);

  if (loading) {
    return (
      <div className="min-h-screen bg-light-bg px-4 sm:px-6 lg:px-8 py-6 space-y-6">
        <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-soft animate-pulse">
          <div className="h-7 w-64 rounded-lg bg-gray-200 skeleton mb-3" />
          <div className="h-4 w-96 max-w-full rounded-lg bg-gray-200 skeleton" />
        </div>

        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
          <SkeletonChart />
          <SkeletonChart />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <SkeletonCard />
          <SkeletonCard />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-light-bg">
      <PageContainer maxWidth="xl" padding="md" spacing="md" className="py-4 sm:py-6">
        {/* Welcome Banner */}
        <WelcomeBanner
          childName={child.name}
          childAge={child.age}
          streak={progress.streak}
        />

        {/* Suggested Exercise - Full width */}
        {suggestedExercise && (
          <SuggestedExerciseCard
            exercise={suggestedExercise}
            onStart={() => handleStartExercise(suggestedExercise.id)}
            onViewDetails={() => handleViewExerciseDetails(suggestedExercise.id)}
          />
        )}

        {/* Stats Overview */}
        <StatsOverview
          exercisesCompleted={progress.exercisesCompleted}
          totalExercises={50}
          totalTimeSpent={progress.totalTimeSpent}
          streak={progress.streak}
          correctRate={85}
          incorrectRate={15}
        />

        {/* Quick Actions - Moved up */}
        <QuickActions
          onAddExercise={handleAddExercise}
          onAddEmotion={handleAddEmotion}
          onViewSuggestions={handleViewSuggestions}
          onViewMethods={handleViewMethods}
          onViewReport={handleViewReport}
          onOpenSettings={handleOpenSettings}
        />

        {/* Progress Section */}
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
          <CPAGauge
            concrete={progress.cpaProgress.concrete}
            pictorial={progress.cpaProgress.pictorial}
            abstract={progress.cpaProgress.abstract}
          />
          <MathTypeProgress
            progress={progress.mathProgress}
          />
        </div>

        {/* Bottom Section - Emotion & Activity */}
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 items-start">
          <EmotionTracker
            entries={emotionEntries.map(e => ({
              date: e.date,
              emotion: e.emotions.primary,
              note: e.description,
            }))}
            onAddEntry={handleAddEmotion}
            onViewAll={handleViewReport}
          />
          <ActivityFeed
            activities={allActivities}
            onViewAll={handleViewReport}
          />
        </div>
      </PageContainer>
    </div>
  );
};

export default DashboardPage;
