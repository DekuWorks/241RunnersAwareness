import React from 'react';

const RunnerAvatar = ({ 
  individual, 
  size = 'md', 
  showUploadButton = false, 
  onUploadClick,
  className = '' 
}) => {
  // Get primary photo or fallback to initials
  const primaryPhoto = individual?.photos?.find(photo => photo.isPrimary);
  const initials = individual ? 
    `${individual.firstName?.charAt(0) || ''}${individual.lastName?.charAt(0) || ''}`.toUpperCase() : 
    '';

  // Size classes
  const getSizeClasses = (size) => {
    switch (size) {
      case 'xs':
        return 'w-6 h-6 text-xs';
      case 'sm':
        return 'w-8 h-8 text-sm';
      case 'md':
        return 'w-12 h-12 text-base';
      case 'lg':
        return 'w-16 h-16 text-lg';
      case 'xl':
        return 'w-20 h-20 text-xl';
      case '2xl':
        return 'w-24 h-24 text-2xl';
      default:
        return 'w-12 h-12 text-base';
    }
  };

  const sizeClasses = getSizeClasses(size);

  return (
    <div className={`relative inline-block ${className}`}>
      {primaryPhoto ? (
        <img
          src={primaryPhoto.imageUrl}
          alt={`${individual?.firstName} ${individual?.lastName}`}
          className={`${sizeClasses} rounded-full object-cover border-2 border-gray-200 dark:border-gray-700`}
        />
      ) : (
        <div className={`${sizeClasses} rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center border-2 border-gray-200 dark:border-gray-700`}>
          <span className="font-semibold text-gray-600 dark:text-gray-300">
            {initials || '?'}
          </span>
        </div>
      )}
      
      {/* Upload button overlay */}
      {showUploadButton && onUploadClick && (
        <button
          onClick={onUploadClick}
          className="absolute -bottom-1 -right-1 w-6 h-6 rounded-full bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 flex items-center justify-center hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
        >
          <svg className="w-3 h-3 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </button>
      )}
    </div>
  );
};

export default RunnerAvatar;
