/**
 * Mock Emotion Data
 * Emotion journal entries for demo
 */

import type { EmotionEntry } from '../services/emotionService';

const today = new Date();
const formatDate = (daysAgo: number) => {
  const date = new Date(today);
  date.setDate(date.getDate() - daysAgo);
  return date.toISOString().split('T')[0];
};

export const mockEmotionEntries: EmotionEntry[] = [
  {
    id: 'emotion-001',
    childId: 'child-001',
    date: formatDate(0),
    time: '09:30',
    emotions: {
      primary: 'happy',
      secondary: 'excited',
    },
    relatedExercise: 'cpa-001',
    description: 'Con rất hứng thú với bài đếm khối gỗ. Con đếm đúng 8/10 lần.',
    triggers: ['Đồ chơi màu sắc', 'Được khen ngợi'],
    copingStrategies: ['Tự hào khi đếm đúng'],
    parentResponse: 'Mẹ khen con rất ngoan. Con đếm giỏi lắm!',
    followUpActions: ['Tiếp tục với bài tập đếm 6-10'],
  },
  {
    id: 'emotion-002',
    childId: 'child-001',
    date: formatDate(1),
    time: '10:15',
    emotions: {
      primary: 'frustrated',
      secondary: 'upset',
    },
    relatedExercise: 'cpa-030',
    description: 'Con tỏ ra nản khi không hiểu phép cộng. Con muốn bỏ học sau 5 phút.',
    triggers: ['Bài tập mới', 'Không hiểu "thêm vào"'],
    copingStrategies: ['Mẹ dùng kẹo để minh họa', 'Giảm số lượng bài tập'],
    parentResponse: 'Mẹ hiểu con đang khó. Chúng ta nghỉ 5 phút rồi làm lại nhé!',
    followUpActions: ['Quay lại bài đếm trước', 'Dùng đồ vật nhiều hơn'],
  },
  {
    id: 'emotion-003',
    childId: 'child-001',
    date: formatDate(2),
    time: '08:45',
    emotions: {
      primary: 'proud',
      secondary: 'happy',
    },
    relatedExercise: 'cpa-020',
    description: 'Con tự hào khi hoàn thành bài ghép số. Con hét lên "Con làm được!"',
    triggers: ['Hoàn thành bài tập', 'Được mẹ khen'],
    copingStrategies: ['Tự hào về bản thân'],
    parentResponse: 'Con giỏi lắm! Mẹ rất tự hào về con!',
    followUpActions: ['Tiếp tục với bài 1-10'],
  },
  {
    id: 'emotion-004',
    childId: 'child-001',
    date: formatDate(3),
    time: '14:00',
    emotions: {
      primary: 'neutral',
      secondary: 'focused',
    },
    relatedExercise: 'cpa-010',
    description: 'Con làm bài tập so sánh khá bình thường. Không hào hứng lắm nhưng cũng không chán.',
    triggers: ['Bài tập quen thuộc'],
    copingStrategies: ['Tập trung làm bài'],
    parentResponse: 'Con làm tốt lắm!继续保持！',
    followUpActions: ['Luyện thêm so sánh với số lớn hơn'],
  },
  {
    id: 'emotion-005',
    childId: 'child-001',
    date: formatDate(4),
    time: '09:00',
    emotions: {
      primary: 'anxious',
      secondary: 'nervous',
    },
    relatedExercise: 'cpa-002',
    description: 'Con lo lắng khi thấy số lượng nhiều hơn (6-10). Con nói "Số nhiều quá, con không đếm được".',
    triggers: ['Số lượng nhiều', 'Áp lực'],
    copingStrategies: ['Mẹ đếm cùng con', 'Chia nhỏ thành 2 nhóm'],
    parentResponse: 'Không sao con ơi! Con chia 1-5 trước, sau đó 6-10 nhé.',
    followUpActions: ['Chia bài thành 2 phần nhỏ', 'Cho con nghỉ giữa chừng'],
  },
  {
    id: 'emotion-006',
    childId: 'child-001',
    date: formatDate(5),
    time: '10:30',
    emotions: {
      primary: 'happy',
      secondary: 'excited',
    },
    relatedExercise: 'cpa-003',
    description: 'Con thích trò đếm lùi. Đặc biệt khi dùng kẹo "ăn" để đếm.',
    triggers: ['Dùng kẹo để minh họa', 'Trò chơi'],
    copingStrategies: ['Biến thành trò chơi'],
    parentResponse: 'Hay quá! Mẹ cũng thích trò này!',
    followUpActions: ['Kết hợp với phép trừ sau'],
  },
  {
    id: 'emotion-007',
    childId: 'child-001',
    date: formatDate(6),
    time: '08:30',
    emotions: {
      primary: 'proud',
      secondary: 'happy',
    },
    relatedExercise: 'cpa-021',
    description: 'Con nhận diện được tất cả số 1-10 lần này! Con vui lắm.',
    triggers: ['Thành công', 'Được khen'],
    copingStrategies: ['Tự hào', 'Muốn làm thêm'],
    parentResponse: 'Tuyệt vời con ơi! Con nhớ hết các số rồi!',
    followUpActions: ['Tiến sang bài ghép số với hình'],
  },
];

export const emotionEmojis: Record<string, string> = {
  happy: '😊',
  frustrated: '😟',
  anxious: '😰',
  neutral: '😐',
  proud: '😎',
};

export const emotionColors: Record<string, string> = {
  happy: 'bg-green-100 text-green-800 border-green-200',
  frustrated: 'bg-red-100 text-red-800 border-red-200',
  anxious: 'bg-yellow-100 text-yellow-800 border-yellow-200',
  neutral: 'bg-gray-100 text-gray-800 border-gray-200',
  proud: 'bg-blue-100 text-blue-800 border-blue-200',
};

export default {
  mockEmotionEntries,
  emotionEmojis,
  emotionColors,
};
