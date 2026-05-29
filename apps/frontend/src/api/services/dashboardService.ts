/**
 * Dashboard Aggregation Service
 * Mock service - combines mock data for dashboard
 */

import { mockProgress, mockSessionHistory } from '../mockData/progress';
import { mockEmotionEntries } from '../mockData/emotions';
import { mockChild } from '../mockData/child';

// ==================== TYPES ====================

export interface StatsCardsData {
  level: string;
  totalCoins: string;
  achievements: string;
  completion: string;
}

export interface CompletionTrendDataPoint {
  name: string;
  completed: number;
  total: number;
  rate: number;
}

export interface CategoryProgressData {
  name: string;
  completed: number;
  total: number;
  percentage: number;
}

export interface EmotionData {
  emotion: string;
  count: number;
  percentage: number;
}

export interface ActivityTimelineItem {
  id: string;
  time: string;
  task: string;
  category: string;
  status: string;
  completed: boolean;
  reward: string;
  childName: string;
  childAvatar: string;
}

export interface SkillRadarData {
  skill: string;
  value: number;
  fullMark: number;
}

export interface DashboardData {
  stats: StatsCardsData;
  completionTrend: CompletionTrendDataPoint[];
  categoryProgress: CategoryProgressData[];
  activityTimeline: ActivityTimelineItem[];
  skillRadar: SkillRadarData[];
  emotions: EmotionData[];
}

// ==================== MOCK DATA ====================

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

// ==================== FUNCTIONS ====================

/**
 * Get stats cards data (from mock progress)
 */
export const getStatsCards = async (): Promise<StatsCardsData> => {
  await delay(100);
  
  const total = mockProgress.exercisesCompleted;
  const target = 50;
  const completion = Math.round((total / target) * 100);
  
  return {
    level: '1',
    totalCoins: '0',
    achievements: '0',
    completion: `${completion}%`,
  };
};

/**
 * Get completion trend data (7 days)
 */
export const getCompletionTrend = async (): Promise<CompletionTrendDataPoint[]> => {
  await delay(100);
  
  const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
  const today = new Date();
  const result: CompletionTrendDataPoint[] = [];
  
  for (let i = 6; i >= 0; i--) {
    const date = new Date(today);
    date.setDate(date.getDate() - i);
    const dayIndex = date.getDay();
    
    // Simulate some sessions
    const hasSession = Math.random() > 0.3;
    const completed = hasSession ? Math.floor(Math.random() * 5) + 1 : 0;
    const total = hasSession ? Math.floor(Math.random() * 3) + completed : 0;
    const rate = total > 0 ? Math.round((completed / total) * 100) : 0;
    
    result.push({
      name: dayNames[dayIndex],
      completed,
      total,
      rate,
    });
  }
  
  return result;
};

/**
 * Get category progress data (mock CPA progress)
 */
export const getCategoryProgress = async (): Promise<CategoryProgressData[]> => {
  await delay(100);
  
  return [
    { name: 'Concrete', completed: Math.round(mockProgress.cpaProgress.concrete / 10), total: 10, percentage: mockProgress.cpaProgress.concrete },
    { name: 'Pictorial', completed: Math.round(mockProgress.cpaProgress.pictorial / 10), total: 10, percentage: mockProgress.cpaProgress.pictorial },
    { name: 'Abstract', completed: Math.round(mockProgress.cpaProgress.abstract / 10), total: 10, percentage: mockProgress.cpaProgress.abstract },
  ];
};

/**
 * Get activity timeline (from mock sessions)
 */
export const getActivityTimeline = async (): Promise<ActivityTimelineItem[]> => {
  await delay(100);
  
  return mockSessionHistory.map(session => ({
    id: session.id,
    time: new Date(session.date).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
    task: session.exerciseTitle,
    category: session.cpaStage,
    status: session.correctCount > session.incorrectCount ? 'completed' : 'pending',
    completed: session.correctCount > session.incorrectCount,
    reward: '+10 XP',
    childName: mockChild.name,
    childAvatar: mockChild.name.charAt(0),
  }));
};

/**
 * Get skill radar data (mock based on CPA progress)
 */
export const getSkillRadar = async (): Promise<SkillRadarData[]> => {
  await delay(100);
  
  return [
    { skill: 'Đếm số', value: mockProgress.mathProgress.counting.current * 10, fullMark: 100 },
    { skill: 'So sánh', value: mockProgress.mathProgress.comparison.current * 10, fullMark: 100 },
    { skill: 'Cộng', value: mockProgress.mathProgress.addition.current * 10, fullMark: 100 },
    { skill: 'Trừ', value: mockProgress.mathProgress.subtraction.current * 10, fullMark: 100 },
    { skill: 'CPA Tổng', value: Math.round((mockProgress.cpaProgress.concrete + mockProgress.cpaProgress.pictorial + mockProgress.cpaProgress.abstract) / 3), fullMark: 100 },
  ];
};

/**
 * Get emotion data (from mock emotions)
 */
export const getEmotions = async (): Promise<EmotionData[]> => {
  await delay(100);
  
  const emotionCounts: Record<string, number> = {};
  
  mockEmotionEntries.forEach(entry => {
    const emotion = entry.emotions.primary;
    emotionCounts[emotion] = (emotionCounts[emotion] || 0) + 1;
  });
  
  const total = mockEmotionEntries.length || 1;
  
  return Object.entries(emotionCounts).map(([emotion, count]) => ({
    emotion,
    count,
    percentage: Math.round((count / total) * 100),
  }));
};

/**
 * Get complete dashboard data (all components)
 */
export const getDashboardData = async (): Promise<DashboardData> => {
  const results = await Promise.allSettled([
    getStatsCards(),
    getCompletionTrend(),
    getCategoryProgress(),
    getActivityTimeline(),
    getSkillRadar(),
    getEmotions(),
  ]);

  return {
    stats: results[0].status === 'fulfilled' ? results[0].value : { level: '1', totalCoins: '0', achievements: '0', completion: '0%' },
    completionTrend: results[1].status === 'fulfilled' ? results[1].value : [],
    categoryProgress: results[2].status === 'fulfilled' ? results[2].value : [],
    activityTimeline: results[3].status === 'fulfilled' ? results[3].value : [],
    skillRadar: results[4].status === 'fulfilled' ? results[4].value : [],
    emotions: results[5].status === 'fulfilled' ? results[5].value : [],
  };
};

export default {
  getStatsCards,
  getCompletionTrend,
  getCategoryProgress,
  getActivityTimeline,
  getSkillRadar,
  getEmotions,
  getDashboardData,
};
