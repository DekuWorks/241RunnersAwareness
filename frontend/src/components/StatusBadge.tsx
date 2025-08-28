import React from 'react';

const StatusBadge = ({ status, size = 'default', onClick, className = '' }) => {
  const getStatusConfig = (status) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return {
          label: 'Active',
          className: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 border-green-200 dark:border-green-700'
        };
      case 'missing':
        return {
          label: 'Missing',
          className: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200 border-red-200 dark:border-red-700'
        };
      case 'found':
        return {
          label: 'Found',
          className: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 border-blue-200 dark:border-blue-700'
        };
      case 'urgent':
        return {
          label: 'Urgent',
          className: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200 border-orange-200 dark:border-orange-700'
        };
      case 'resolved':
        return {
          label: 'Resolved',
          className: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200 border-gray-200 dark:border-gray-700'
        };
      default:
        return {
          label: status || 'Unknown',
          className: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200 border-gray-200 dark:border-gray-700'
        };
    }
  };

  const config = getStatusConfig(status);
  const sizeClass = size === 'sm' ? 'text-xs px-2 py-0.5' : 'text-sm px-2.5 py-0.5';
  const clickableClass = onClick ? 'cursor-pointer hover:opacity-80 transition-opacity' : '';

  return (
    <span 
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${config.className} ${sizeClass} ${clickableClass} ${className}`}
      onClick={onClick}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
    >
      {config.label}
    </span>
  );
};

export default StatusBadge;
