/**
 * Dashboard Page - MathMate Support
 * Clean, minimal SaaS-style design
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Loading } from '../../components/ui';

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

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
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
  };

  // Transform session history to activities
  const activities = sessionHistory.map(session => ({
    id: session.id,
    type: 'exercise' as const,
    time: session.date,
    title: session.exerciseTitle,
    details: session.notes,
    accuracy: Math.round((session.correctCount / (session.correctCount + session.incorrectCount)) * 100),
    cpaStage: session.cpaStage,
  }));

  // Add emotion entries to activities
  const emotionActivities = emotionEntries.slice(0, 3).map(entry => ({
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
  }));

  const allActivities = [...activities, ...emotionActivities]
    .sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime())
    .slice(0, 10);

  // Handler functions
  const handleAddExercise = () => navigate('/exercises');
  const handleAddEmotion = () => navigate('/journal');
  const handleViewSuggestions = () => navigate('/exercises');
  const handleViewMethods = () => navigate('/methods');
  const handleViewReport = () => navigate('/journal');
  const handleOpenSettings = () => navigate('/settings');
  const handleStartExercise = (exerciseId: string) => navigate(`/exercises/${exerciseId}`);
  const handleViewExerciseDetails = (exerciseId: string) => navigate(`/exercises/${exerciseId}`);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Loading text="Đang tải dashboard..." />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
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
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
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
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
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
      </div>
    </div>
  );
};

export default DashboardPage;
