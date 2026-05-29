import type { NotificationSettings } from '../services/userService';

export const mockUserProfile = {
  id: 'user-123',
  email: 'top1@mathmate.com',
  full_name: 'Nguyễn Diệu Mai Vy',
  phone_number: '+84 912 345 678',
  onboarding_completed: true,
  children_count: 1,
  created_at: new Date().toISOString(),
};

export const mockNotificationSettings: NotificationSettings = {
  email: {
    enabled: true,
    coin_redemption: true,
    task_reminders: true,
    emotional_trends: false,
    weekly_reports: true,
  },
  push: {
    enabled: false,
    coin_redemption: false,
    task_reminders: false,
    emotional_trends: false,
    weekly_reports: false,
  },
};

export default {
  profile: mockUserProfile,
  notifications: mockNotificationSettings,
};
