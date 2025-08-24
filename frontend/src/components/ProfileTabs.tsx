import React from 'react';
import { Clock, FileText, Image, User } from 'lucide-react';

const ProfileTabs = ({ activeTab, onTabChange }) => {
  const tabs = [
    {
      id: 'timeline',
      label: 'Timeline',
      icon: Clock,
      description: 'Activity feed and recent updates'
    },
    {
      id: 'cases',
      label: 'Cases',
      icon: FileText,
      description: 'Missing person cases and reports'
    },
    {
      id: 'photos',
      label: 'Photos',
      icon: Image,
      description: 'Photo gallery and media'
    },
    {
      id: 'details',
      label: 'Details',
      icon: User,
      description: 'Personal information and contact details'
    }
  ];

  return (
    <div className="border-b bg-white dark:bg-neutral-900" role="tablist">
      <div className="px-6">
        <div className="grid w-full grid-cols-4 h-12">
          {tabs.map((tab) => {
            const IconComponent = tab.icon;
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => onTabChange(tab.id)}
                className={`flex items-center justify-center space-x-2 px-4 py-2 text-sm font-medium transition-colors hover:text-gray-900 dark:hover:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
                  isActive 
                    ? 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-white border-b-2 border-blue-500' 
                    : 'text-gray-600 dark:text-gray-400'
                }`}
                role="tab"
                aria-selected={isActive}
                aria-controls={`tabpanel-${tab.id}`}
              >
                <IconComponent className="w-4 h-4" />
                <span>{tab.label}</span>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default ProfileTabs;
