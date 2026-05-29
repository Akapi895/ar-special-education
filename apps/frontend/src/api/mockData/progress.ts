/**
 * Mock Progress Data
 * Learning progress for demo
 */

import type { ProgressData, SessionRecord } from '../services/progressService';

export const mockProgress: ProgressData = {
  childId: 'child-001',
  cpaProgress: {
    concrete: 75,
    pictorial: 40,
    abstract: 15,
  },
  mathProgress: {
    counting: { current: 8, target: 10 },
    comparison: { current: 5, target: 10 },
    addition: { current: 3, target: 10 },
    subtraction: { current: 1, target: 10 },
  },
  totalSessions: 24,
  totalTimeSpent: 320, // minutes
  exercisesCompleted: 12,
  streak: 5, // days
  lastSessionDate: new Date().toISOString().split('T')[0],
};

export const mockSessionHistory: SessionRecord[] = [
  {
    id: 'session-001',
    date: new Date().toISOString(),
    exerciseId: 'cpa-001',
    exerciseTitle: 'Đếm đồ vật 1-5',
    cpaStage: 'concrete',
    duration: 15,
    correctCount: 8,
    incorrectCount: 2,
    notes: 'Con hứng thú với bài tập. Cần củng cố thêm.',
  },
  {
    id: 'session-002',
    date: new Date(Date.now() - 86400000).toISOString(),
    exerciseId: 'cpa-010',
    exerciseTitle: 'So sánh nhiều hơn - ít hơn (1-5)',
    cpaStage: 'concrete',
    duration: 12,
    correctCount: 6,
    incorrectCount: 4,
    notes: 'Con bắt đầu hiểu khái niệm "nhiều hơn".',
  },
  {
    id: 'session-003',
    date: new Date(Date.now() - 172800000).toISOString(),
    exerciseId: 'cpa-020',
    exerciseTitle: 'Ghép số với hình ảnh (1-5)',
    cpaStage: 'pictorial',
    duration: 10,
    correctCount: 7,
    incorrectCount: 3,
    notes: 'Con đã nhận diện được số 1-5.',
  },
  {
    id: 'session-004',
    date: new Date(Date.now() - 259200000).toISOString(),
    exerciseId: 'cpa-002',
    exerciseTitle: 'Đếm đồ vật 6-10',
    cpaStage: 'concrete',
    duration: 15,
    correctCount: 9,
    incorrectCount: 1,
    notes: 'Tiến bộ tốt! Con đếm được đến 10.',
  },
  {
    id: 'session-005',
    date: new Date(Date.now() - 345600000).toISOString(),
    exerciseId: 'cpa-030',
    exerciseTitle: 'Minh họa phép cộng bằng hình ảnh',
    cpaStage: 'pictorial',
    duration: 20,
    correctCount: 5,
    incorrectCount: 5,
    notes: 'Cần thực hành thêm với phép cộng.',
  },
  {
    id: 'session-006',
    date: new Date(Date.now() - 432000000).toISOString(),
    exerciseId: 'cpa-021',
    exerciseTitle: 'Ghép số với hình ảnh (1-10)',
    cpaStage: 'pictorial',
    duration: 15,
    correctCount: 8,
    incorrectCount: 2,
    notes: 'Con nhầm 6 và 9. Cần luyện thêm.',
  },
  {
    id: 'session-007',
    date: new Date(Date.now() - 518400000).toISOString(),
    exerciseId: 'cpa-003',
    exerciseTitle: 'Đếm lùi từ 5 về 1',
    cpaStage: 'concrete',
    duration: 10,
    correctCount: 4,
    incorrectCount: 1,
    notes: 'Con đếm lùi tốt hơn khi kết hợp vỗ tay.',
  },
];

export default {
  mockProgress,
  mockSessionHistory,
};
