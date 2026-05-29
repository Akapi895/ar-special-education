/**
 * Progress Service
 * Mock service for learning progress (no backend)
 */

import { mockProgress, mockSessionHistory } from '../mockData/progress';

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export interface CPAProgress {
  concrete: number;   // 0-100
  pictorial: number; // 0-100
  abstract: number;  // 0-100
}

export interface MathProgress {
  counting: { current: number; target: number };
  comparison: { current: number; target: number };
  addition: { current: number; target: number };
  subtraction: { current: number; target: number };
}

export interface SessionRecord {
  id: string;
  date: string;
  exerciseId: string;
  exerciseTitle: string;
  cpaStage: 'concrete' | 'pictorial' | 'abstract';
  duration: number; // minutes
  correctCount: number;
  incorrectCount: number;
  notes?: string;
}

export interface ProgressData {
  childId: string;
  cpaProgress: CPAProgress;
  mathProgress: MathProgress;
  totalSessions: number;
  totalTimeSpent: number; // minutes
  exercisesCompleted: number;
  streak: number; // days
  lastSessionDate: string | null;
}

export const getProgress = async (_childId: string): Promise<ProgressData> => {
  await delay(100);
  return mockProgress;
};

export const getSessionHistory = async (
  _childId: string,
  limit: number = 10
): Promise<SessionRecord[]> => {
  await delay(100);
  return mockSessionHistory.slice(0, limit);
};

export const recordSession = async (
  _childId: string,
  session: Omit<SessionRecord, 'id' | 'date'>
): Promise<SessionRecord> => {
  await delay(100);
  
  const newSession: SessionRecord = {
    id: `session-${Date.now()}`,
    date: new Date().toISOString(),
    ...session,
  };
  
  // In a real app, this would save to localStorage
  return newSession;
};

export const updateCPAProgress = async (
  _childId: string,
  progress: Partial<CPAProgress>
): Promise<CPAProgress> => {
  await delay(100);
  
  // This would update localStorage in a real app
  return {
    concrete: progress.concrete ?? 0,
    pictorial: progress.pictorial ?? 0,
    abstract: progress.abstract ?? 0,
  };
};

export const getWeeklyProgress = async (_childId: string): Promise<{
  day: string;
  sessions: number;
  timeSpent: number;
}[]> => {
  await delay(100);
  
  // Return mock weekly data
  const days = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
  return days.map(day => ({
    day,
    sessions: Math.floor(Math.random() * 3),
    timeSpent: Math.floor(Math.random() * 30),
  }));
};

export default {
  getProgress,
  getSessionHistory,
  recordSession,
  updateCPAProgress,
  getWeeklyProgress,
};
