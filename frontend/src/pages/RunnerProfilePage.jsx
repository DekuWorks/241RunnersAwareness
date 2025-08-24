import React, { useState, useEffect, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { useSelector } from 'react-redux';
import RunnerHeader from '../components/RunnerHeader';
import ProfileTabs from '../components/ProfileTabs';

const RunnerProfilePage = () => {
  const { id } = useParams();
  const [activeTab, setActiveTab] = useState('timeline');
  const [individual, setIndividual] = useState(null);
  const [loading, setLoading] = useState(true);
  
  const user = useSelector(state => state.auth.user);
  const isAdmin = user?.role === 'admin';
  const isOwner = individual?.ownerUserId === user?.id;
  const canEdit = isOwner || isAdmin;

  // Mock data for demonstration - replace with actual API call
  useEffect(() => {
    // Simulate API call
    setTimeout(() => {
      setIndividual({
        id: id,
        firstName: 'John',
        lastName: 'Doe',
        runnerId: 'RUN-2024-001',
        status: 'Active',
        ownerUserId: user?.id,
        photos: [
          {
            id: 1,
            imageUrl: 'https://via.placeholder.com/150',
            isPrimary: true,
            caption: 'Primary photo'
          }
        ],
        activities: [],
        cases: []
      });
      setLoading(false);
    }, 1000);
  }, [id, user?.id]);

  const handleEditProfile = () => {
    console.log('Edit profile clicked');
    // TODO: Open edit profile dialog
  };

  const handleUploadPhoto = () => {
    console.log('Upload photo clicked');
    // TODO: Open upload photo dialog
  };

  const handleNewCase = () => {
    console.log('New case clicked');
    // TODO: Navigate to new case form
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  // Keyboard shortcuts
  const handleKeyDown = useCallback((event) => {
    if (!canEdit) return;
    
    switch (event.key.toLowerCase()) {
      case 'e':
        event.preventDefault();
        handleEditProfile();
        break;
      case 'u':
        event.preventDefault();
        handleUploadPhoto();
        break;
      case 'c':
        event.preventDefault();
        handleNewCase();
        break;
    }
  }, [canEdit]);

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div className="animate-pulse">
          <div className="bg-white dark:bg-neutral-900 border-b">
            <div className="px-6 py-4">
              <div className="flex items-center space-x-4">
                <div className="w-16 h-16 bg-gray-200 dark:bg-gray-700 rounded-full"></div>
                <div className="space-y-2">
                  <div className="h-6 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Sticky Header */}
      <RunnerHeader
        individual={individual}
        isOwner={isOwner}
        isAdmin={isAdmin}
        onEditProfile={handleEditProfile}
        onUploadPhoto={handleUploadPhoto}
        onNewCase={handleNewCase}
      />
      
      {/* Tabs */}
      <ProfileTabs
        activeTab={activeTab}
        onTabChange={handleTabChange}
      />
      
             {/* Tab Content */}
       <div className="px-6 py-8">
         <div className="max-w-4xl mx-auto">
           {activeTab === 'timeline' && (
             <div 
               className="space-y-4" 
               role="tabpanel" 
               id="tabpanel-timeline"
               aria-labelledby="tab-timeline"
             >
               <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Timeline</h2>
               <p className="text-gray-600 dark:text-gray-300">Activity feed will be displayed here.</p>
             </div>
           )}
           
           {activeTab === 'cases' && (
             <div 
               className="space-y-4" 
               role="tabpanel" 
               id="tabpanel-cases"
               aria-labelledby="tab-cases"
             >
               <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Cases</h2>
               <p className="text-gray-600 dark:text-gray-300">Missing person cases will be displayed here.</p>
             </div>
           )}
           
           {activeTab === 'photos' && (
             <div 
               className="space-y-4" 
               role="tabpanel" 
               id="tabpanel-photos"
               aria-labelledby="tab-photos"
             >
               <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Photos</h2>
               <p className="text-gray-600 dark:text-gray-300">Photo gallery will be displayed here.</p>
             </div>
           )}
           
           {activeTab === 'details' && (
             <div 
               className="space-y-4" 
               role="tabpanel" 
               id="tabpanel-details"
               aria-labelledby="tab-details"
             >
               <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Details</h2>
               <p className="text-gray-600 dark:text-gray-300">Personal information will be displayed here.</p>
             </div>
           )}
         </div>
       </div>
    </div>
  );
};

export default RunnerProfilePage;
