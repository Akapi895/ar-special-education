/**
 * Settings Page - MathMate Support
 * Simplified for single child
 */

import { useEffect, useRef, useState, type KeyboardEvent } from 'react';
import { User, Heart, Bell } from 'lucide-react';
import AccountSettingsTab from '../../features/parents/settings/AccountSettingsTab';
import ChildProfilesTab from '../../features/parents/settings/ChildProfilesTab';
import NotificationSettingsTab from '../../features/parents/settings/NotificationSettingsTab';
import PageContainer from '../../components/layout/PageContainer';

type TabType = 'account' | 'children' | 'notifications';

const SettingsPage = () => {
  const [activeTab, setActiveTab] = useState<TabType>('account');
  const tabRefs = useRef<Array<HTMLButtonElement | null>>([]);

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

  useEffect(() => {
    const activeIndex = tabs.findIndex((tab) => tab.id === activeTab);
    tabRefs.current[activeIndex]?.focus();
  }, [activeTab]);

  const handleTabKeyDown = (event: KeyboardEvent<HTMLButtonElement>, index: number) => {
    const totalTabs = tabs.length;

    let nextIndex = index;
    if (event.key === 'ArrowRight') {
      nextIndex = (index + 1) % totalTabs;
    } else if (event.key === 'ArrowLeft') {
      nextIndex = (index - 1 + totalTabs) % totalTabs;
    } else if (event.key === 'Home') {
      nextIndex = 0;
    } else if (event.key === 'End') {
      nextIndex = totalTabs - 1;
    } else {
      return;
    }

    event.preventDefault();
    setActiveTab(tabs[nextIndex].id);
  };

  return (
    <div className="min-h-screen overflow-y-auto bg-(--app-bg) transition-colors duration-300">
      <PageContainer maxWidth="lg" padding="md" spacing="md" className="py-4 sm:py-6 lg:py-8">
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
        <div className="overflow-hidden rounded-2xl border border-[var(--app-border)] bg-(--app-surface) shadow-soft transition-colors duration-300">
          {/* Tab Headers */}
          <div className="flex gap-1 overflow-x-auto border-b-2 border-[var(--app-border)] bg-(--app-bg) px-2 transition-colors duration-300 sm:px-0" role="tablist" aria-label="Cài đặt tài khoản">
            {tabs.map((tab, index) => {
              const Icon = tab.icon;
              return (
                <button
                  key={tab.id}
                  ref={(element) => {
                    tabRefs.current[index] = element;
                  }}
                  role="tab"
                  id={`settings-tab-${tab.id}`}
                  aria-selected={activeTab === tab.id}
                  aria-controls={`settings-panel-${tab.id}`}
                  tabIndex={activeTab === tab.id ? 0 : -1}
                  onClick={() => setActiveTab(tab.id)}
                  onKeyDown={(event) => handleTabKeyDown(event, index)}
                  className={`
                    flex items-center gap-3 px-5 py-4 text-sm font-semibold transition-all duration-300 relative whitespace-nowrap shrink-0
                    ${activeTab === tab.id
                      ? 'bg-(--app-surface-strong) text-primary-700'
                      : 'text-[var(--app-text-muted)] hover:bg-(--app-surface) hover:text-[var(--app-text)]'
                    }
                  `}
                >
                  <Icon className={`w-5 h-5 ${activeTab === tab.id ? 'text-primary-600' : ''}`} />
                  <span>{tab.label}</span>
                  
                  {/* Active indicator */}
                  {activeTab === tab.id && (
                    <div 
                      className="absolute bottom-0 left-0 right-0 h-1 bg-linear-to-r from-primary-500 to-primary-600"
                    />
                  )}
                </button>
              );
            })}
          </div>

          {/* Tab Content */}
          <div className="p-6">
            <div role="tabpanel" id="settings-panel-account" aria-labelledby="settings-tab-account" hidden={activeTab !== 'account'}>
              {activeTab === 'account' && <AccountSettingsTab />}
            </div>
            <div role="tabpanel" id="settings-panel-children" aria-labelledby="settings-tab-children" hidden={activeTab !== 'children'}>
              {activeTab === 'children' && <ChildProfilesTab />}
            </div>
            <div role="tabpanel" id="settings-panel-notifications" aria-labelledby="settings-tab-notifications" hidden={activeTab !== 'notifications'}>
              {activeTab === 'notifications' && <NotificationSettingsTab />}
            </div>
          </div>
        </div>
      </PageContainer>
    </div>
  );
};

export default SettingsPage;
