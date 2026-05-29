/**
 * Settings Page - MathMate Support
 * Simplified for single child
 */

import { useState } from 'react';
import { User, Heart, Bell } from 'lucide-react';
import AccountSettingsTab from '../../features/parents/settings/AccountSettingsTab';
import ChildProfilesTab from '../../features/parents/settings/ChildProfilesTab';
import NotificationSettingsTab from '../../features/parents/settings/NotificationSettingsTab';

type TabType = 'account' | 'children' | 'notifications';

const SettingsPage = () => {
  const [activeTab, setActiveTab] = useState<TabType>('account');

  const tabs = [
    {
      id: 'account' as TabType,
      label: 'Tài khoản',
      icon: User,
    },
    {
      id: 'children' as TabType,
      label: 'Thông tin con',
      icon: Heart,
    },
    {
      id: 'notifications' as TabType,
      label: 'Thông báo',
      icon: Bell,
    },
  ];

  return (
    <div className="min-h-screen overflow-y-auto p-4 md:p-6 lg:p-8 bg-gray-50">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            Cài đặt
          </h1>
          <p className="text-gray-600">
            Quản lý tài khoản và thông tin của con
          </p>
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          {/* Tab Headers */}
          <div className="flex border-b-2 border-gray-200 bg-gray-50">
            {tabs.map((tab) => {
              const Icon = tab.icon;
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`
                    flex items-center gap-3 px-6 py-4 text-sm font-semibold transition-all duration-300 relative
                    ${activeTab === tab.id
                      ? 'text-primary-700 bg-white'
                      : 'text-gray-600 hover:text-gray-900 hover:bg-white/70'
                    }
                  `}
                >
                  <Icon className={`w-5 h-5 ${activeTab === tab.id ? 'text-primary-600' : ''}`} />
                  <span>{tab.label}</span>
                  
                  {/* Active indicator */}
                  {activeTab === tab.id && (
                    <div 
                      className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-primary-500 to-primary-600"
                    />
                  )}
                </button>
              );
            })}
          </div>

          {/* Tab Content */}
          <div className="p-6">
            {activeTab === 'account' && <AccountSettingsTab />}
            {activeTab === 'children' && <ChildProfilesTab />}
            {activeTab === 'notifications' && <NotificationSettingsTab />}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
