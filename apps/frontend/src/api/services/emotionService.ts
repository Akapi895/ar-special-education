/**
 * Emotion Service
 * Mock service for emotion journal (no backend)
 */

import { mockEmotionEntries } from '../mockData/emotions';

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export type PrimaryEmotion = 'happy' | 'frustrated' | 'anxious' | 'neutral' | 'proud';

export interface EmotionEntry {
  id: string;
  childId: string;
  date: string;
  time: string;
  emotions: {
    primary: PrimaryEmotion;
    secondary?: string;
  };
  relatedExercise?: string;
  description: string;
  triggers?: string[];
  copingStrategies?: string[];
  parentResponse: string;
  followUpActions?: string[];
}

export interface EmotionStats {
  total: number;
  happy: number;
  frustrated: number;
  anxious: number;
  neutral: number;
  proud: number;
}

export interface EmotionTrend {
  date: string;
  primary: PrimaryEmotion;
  intensity: number; // 1-5
}

export const getEmotionEntries = async (
  childId: string,
  limit?: number
): Promise<EmotionEntry[]> => {
  await delay(100);
  const entries = mockEmotionEntries.filter(e => e.childId === childId);
  return limit ? entries.slice(0, limit) : entries;
};

export const getEmotionById = async (
  childId: string,
  entryId: string
): Promise<EmotionEntry | undefined> => {
  await delay(50);
  return mockEmotionEntries.find(e => e.id === entryId && e.childId === childId);
};

export const createEmotionEntry = async (
  entry: Omit<EmotionEntry, 'id' | 'date' | 'time'>
): Promise<EmotionEntry> => {
  await delay(100);
  
  const newEntry: EmotionEntry = {
    id: `emotion-${Date.now()}`,
    date: new Date().toISOString().split('T')[0],
    time: new Date().toTimeString().slice(0, 5),
    ...entry,
  };
  
  return newEntry;
};

export const updateEmotionEntry = async (
  _childId: string,
  _entryId: string,
  updates: Partial<EmotionEntry>
): Promise<EmotionEntry> => {
  await delay(100);
  
  const existing = mockEmotionEntries[0]; // Just return first entry for mock
  
  if (!existing) {
    throw new Error('Emotion entry not found');
  }
  
  return { ...existing, ...updates };
};

export const deleteEmotionEntry = async (
  _childId: string,
  _entryId: string
): Promise<void> => {
  await delay(100);
  // In real app, this would delete from localStorage
};

export const getEmotionStats = async (
  _childId: string,
  days: number = 7
): Promise<EmotionStats> => {
  await delay(100);
  
  const entries = mockEmotionEntries
    .filter(() => true) // Use all mock entries
    .slice(0, days);
  
  const stats: EmotionStats = {
    total: entries.length,
    happy: 0,
    frustrated: 0,
    anxious: 0,
    neutral: 0,
    proud: 0,
  };
  
  entries.forEach(entry => {
    const emotion = entry.emotions.primary;
    if (emotion in stats) {
      (stats as unknown as Record<string, number>)[emotion]++;
    }
  });
  
  return stats;
};

export const getEmotionTrend = async (
  _childId: string,
  days: number = 7
): Promise<EmotionTrend[]> => {
  await delay(100);
  
  return mockEmotionEntries
    .filter(() => true) // Use all mock entries
    .slice(0, days)
    .map(entry => ({
      date: entry.date,
      primary: entry.emotions.primary,
      intensity: entry.emotions.primary === 'happy' || entry.emotions.primary === 'proud' ? 4 : 
                 entry.emotions.primary === 'neutral' ? 3 : 2,
    }));
};

export const getTodayEmotion = async (_childId: string): Promise<EmotionEntry | null> => {
  await delay(50);
  return mockEmotionEntries.find(() => true) || null;
};

export default {
  getEmotionEntries,
  getEmotionById,
  createEmotionEntry,
  updateEmotionEntry,
  deleteEmotionEntry,
  getEmotionStats,
  getEmotionTrend,
  getTodayEmotion,
};
