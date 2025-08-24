import React from 'react';
import { Edit, Plus, Camera } from 'lucide-react';
import StatusBadge from './StatusBadge';
import RunnerAvatar from './RunnerAvatar';

const RunnerHeader = ({ 
  individual, 
  isOwner, 
  isAdmin, 
  onEditProfile, 
  onUploadPhoto, 
  onNewCase 
}) => {
  const canEdit = isOwner || isAdmin;

  return (
    <header className="sticky top-0 bg-white dark:bg-neutral-900 z-40 border-b">
      {/* Row 1: Main header content */}
      <div className="px-6 py-4">
        <div className="flex items-center justify-between">
          {/* Left: Photo and Name */}
          <div className="flex items-center space-x-4">
            {/* Primary Photo */}
            <RunnerAvatar
              individual={individual}
              size="lg"
              showUploadButton={canEdit}
              onUploadClick={onUploadPhoto}
            />

            {/* Name and RunnerID */}
            <div className="flex flex-col">
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
                {individual?.firstName} {individual?.lastName}
              </h1>
              <div className="flex items-center space-x-2 mt-1">
                <StatusBadge status={individual?.runnerId || 'No ID'} size="sm" />
                <StatusBadge status={individual?.status} size="sm" />
              </div>
            </div>
          </div>

                     {/* Right: Action buttons */}
           <div className="flex items-center space-x-2">
             {canEdit && (
               <>
                 <button
                   onClick={onEditProfile}
                   className="flex items-center space-x-1 px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
                   title="Edit Profile (E)"
                 >
                   <Edit className="w-4 h-4" />
                   <span>Edit Profile</span>
                 </button>
                 <button
                   onClick={onNewCase}
                   className="flex items-center space-x-1 px-3 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
                   title="New Case (C)"
                 >
                   <Plus className="w-4 h-4" />
                   <span>New Case</span>
                 </button>
               </>
             )}
           </div>
        </div>
      </div>

             {/* Row 2: Compact Action Bar */}
       {canEdit && (
         <div className="px-6 py-2 bg-gray-50 dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
           <div className="flex items-center justify-center space-x-4">
             <button
               onClick={onEditProfile}
               className="flex items-center justify-center w-8 h-8 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
               title="Edit Profile (E)"
               aria-label="Edit Profile"
             >
               <Edit className="w-4 h-4" />
             </button>
             <button
               onClick={onUploadPhoto}
               className="flex items-center justify-center w-8 h-8 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
               title="Upload Photo (U)"
               aria-label="Upload Photo"
             >
               <Camera className="w-4 h-4" />
             </button>
             <button
               onClick={onNewCase}
               className="flex items-center justify-center w-8 h-8 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
               title="New Case (C)"
               aria-label="New Case"
             >
               <Plus className="w-4 h-4" />
             </button>
           </div>
         </div>
       )}
     </header>
  );
};

export default RunnerHeader;
