import React, { useEffect, useState } from 'react';
import { usersAPI } from '../services/api';
import { toast } from 'react-toastify';
import ImageUpload from '../components/ImageUpload';

const MyCasePage = () => {
  const [caseData, setCaseData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchCase = async () => {
      try {
        setLoading(true);
        const response = await usersAPI.getMyCase();
        setCaseData(response.data);
        setError(null);
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to fetch case data.');
        toast.error(err.response?.data?.message || 'Failed to fetch case data.');
      } finally {
        setLoading(false);
      }
    };

    fetchCase();
  }, []);

  const handleUploadSuccess = (newPhotoUrl) => {
    setCaseData((prev) => ({
      ...prev,
      photoUrl: newPhotoUrl,
      lastPhotoUpdate: new Date().toISOString(),
    }));
  };

  const getNextUpdateDate = () => {
    if (!caseData?.lastPhotoUpdate) return null;
    const lastUpdate = new Date(caseData.lastPhotoUpdate);
    const nextUpdate = new Date(lastUpdate.setMonth(lastUpdate.getMonth() + 6));
    return nextUpdate;
  };

  const isUpdateRequired = () => {
    const nextUpdate = getNextUpdateDate();
    return nextUpdate && nextUpdate < new Date();
  };

  if (loading) {
    return <div className="text-center p-8 text-xl">Loading your case file...</div>;
  }

  if (error) {
    return (
      <div className="container mx-auto p-8 text-center">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative" role="alert">
          <strong className="font-bold">Error:</strong>
          <span className="block sm:inline"> {error}</span>
        </div>
      </div>
    );
  }

  if (!caseData) {
    return <div className="text-center p-8 text-xl">No case file found.</div>;
  }

  const nextUpdateDate = getNextUpdateDate();

  return (
    <div className="container mx-auto p-4 sm:p-6 lg:p-8">
      {isUpdateRequired() && (
        <div className="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-800 p-4 mb-6 rounded-md shadow-md" role="alert">
          <p className="font-bold text-lg">Update Required</p>
          <p>Your profile image is more than 6 months old. Please upload a new one.</p>
        </div>
      )}

      <div className="bg-white shadow-xl rounded-lg overflow-hidden">
        <div className="bg-gray-50 px-6 py-4 border-b border-gray-200">
            <h1 className="text-2xl font-bold text-gray-800">My Case File</h1>
        </div>
        <div className="p-6">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              <div className="md:col-span-2">
                <h2 className="text-xl font-semibold mb-4 text-gray-700">Personal Information</h2>
                <div className="space-y-3">
                    <p><strong>Name:</strong> {caseData.firstName} {caseData.lastName}</p>
                    <p><strong>Date of Birth:</strong> {new Date(caseData.dateOfBirth).toLocaleDateString()}</p>
                    {nextUpdateDate && (
                      <p className="mt-2 text-md">
                        <strong>Next Photo Update Due:</strong> 
                        <span className="font-semibold text-red-600 ml-2">
                            {nextUpdateDate.toLocaleDateString()}
                        </span>
                      </p>
                    )}
                </div>
              </div>
              <div className="flex flex-col items-center justify-start">
                 {caseData.photoUrl ? (
                    <img src={caseData.photoUrl} alt="User" className="rounded-lg h-48 w-48 object-cover shadow-lg mb-4 border-4 border-white" />
                 ) : (
                    <div className="h-48 w-48 bg-gray-200 rounded-lg flex items-center justify-center mb-4 shadow-inner">
                        <span className="text-gray-500">No Image</span>
                    </div>
                 )}
                 <ImageUpload userId={caseData.id} onUploadSuccess={handleUploadSuccess} />
              </div>
            </div>
        </div>
      </div>
    </div>
  );
};

export default MyCasePage; 