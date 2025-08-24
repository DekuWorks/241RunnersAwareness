import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { createCase } from '../features/cases/casesSlice';

const ReportCaseForm = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { loading, error } = useSelector((state) => state.cases);
  const { user } = useSelector((state) => state.auth);

  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    individualId: '',
    lastSeenLocation: '',
    lastSeenDate: '',
    riskLevel: 'medium',
    lawEnforcementCaseNumber: '',
    investigatingAgency: '',
    latitude: '',
    longitude: '',
    isPublic: true,
  });

  const [errors, setErrors] = useState({});

  const steps = [
    { id: 1, title: 'Basic Information', description: 'Case title and description' },
    { id: 2, title: 'Location Details', description: 'Where the person was last seen' },
    { id: 3, title: 'Additional Details', description: 'Risk level and law enforcement info' },
    { id: 4, title: 'Review & Submit', description: 'Review all information' },
  ];

  const validateStep = (step) => {
    const newErrors = {};

    switch (step) {
      case 1:
        if (!formData.title.trim()) newErrors.title = 'Title is required';
        if (!formData.description.trim()) newErrors.description = 'Description is required';
        break;
      case 2:
        if (!formData.lastSeenLocation.trim()) newErrors.lastSeenLocation = 'Last seen location is required';
        if (!formData.lastSeenDate) newErrors.lastSeenDate = 'Last seen date is required';
        break;
      case 3:
        if (!formData.riskLevel) newErrors.riskLevel = 'Risk level is required';
        break;
      default:
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));

    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const nextStep = () => {
    if (validateStep(currentStep)) {
      setCurrentStep(prev => Math.min(prev + 1, steps.length));
    }
  };

  const prevStep = () => {
    setCurrentStep(prev => Math.max(prev - 1, 1));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateStep(currentStep)) {
      return;
    }

    try {
      const result = await dispatch(createCase(formData)).unwrap();
      navigate(`/dashboard/cases/${result.id}`);
    } catch (error) {
      console.error('Failed to create case:', error);
    }
  };

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <div className="space-y-6">
            <div>
              <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-2">
                Case Title *
              </label>
              <input
                type="text"
                id="title"
                name="title"
                value={formData.title}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.title ? 'border-red-300' : 'border-gray-300'
                }`}
                placeholder="Enter a descriptive title for this case"
              />
              {errors.title && <p className="mt-1 text-sm text-red-600">{errors.title}</p>}
            </div>

            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
                Case Description *
              </label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                rows={4}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.description ? 'border-red-300' : 'border-gray-300'
                }`}
                placeholder="Provide a detailed description of the missing person and circumstances"
              />
              {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description}</p>}
            </div>

            <div>
              <label htmlFor="individualId" className="block text-sm font-medium text-gray-700 mb-2">
                Individual ID (Optional)
              </label>
              <input
                type="text"
                id="individualId"
                name="individualId"
                value={formData.individualId}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter the individual's ID if available"
              />
              <p className="mt-1 text-sm text-gray-500">
                If you have an existing individual profile, enter their ID to link this case.
              </p>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-6">
            <div>
              <label htmlFor="lastSeenLocation" className="block text-sm font-medium text-gray-700 mb-2">
                Last Seen Location *
              </label>
              <input
                type="text"
                id="lastSeenLocation"
                name="lastSeenLocation"
                value={formData.lastSeenLocation}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.lastSeenLocation ? 'border-red-300' : 'border-gray-300'
                }`}
                placeholder="e.g., 123 Main St, City, State"
              />
              {errors.lastSeenLocation && <p className="mt-1 text-sm text-red-600">{errors.lastSeenLocation}</p>}
            </div>

            <div>
              <label htmlFor="lastSeenDate" className="block text-sm font-medium text-gray-700 mb-2">
                Last Seen Date *
              </label>
              <input
                type="datetime-local"
                id="lastSeenDate"
                name="lastSeenDate"
                value={formData.lastSeenDate}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.lastSeenDate ? 'border-red-300' : 'border-gray-300'
                }`}
              />
              {errors.lastSeenDate && <p className="mt-1 text-sm text-red-600">{errors.lastSeenDate}</p>}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label htmlFor="latitude" className="block text-sm font-medium text-gray-700 mb-2">
                  Latitude (Optional)
                </label>
                <input
                  type="number"
                  id="latitude"
                  name="latitude"
                  value={formData.latitude}
                  onChange={handleInputChange}
                  step="any"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="e.g., 40.7128"
                />
              </div>

              <div>
                <label htmlFor="longitude" className="block text-sm font-medium text-gray-700 mb-2">
                  Longitude (Optional)
                </label>
                <input
                  type="number"
                  id="longitude"
                  name="longitude"
                  value={formData.longitude}
                  onChange={handleInputChange}
                  step="any"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="e.g., -74.0060"
                />
              </div>
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-6">
            <div>
              <label htmlFor="riskLevel" className="block text-sm font-medium text-gray-700 mb-2">
                Risk Level *
              </label>
              <select
                id="riskLevel"
                name="riskLevel"
                value={formData.riskLevel}
                onChange={handleInputChange}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                  errors.riskLevel ? 'border-red-300' : 'border-gray-300'
                }`}
              >
                <option value="low">Low Risk</option>
                <option value="medium">Medium Risk</option>
                <option value="high">High Risk</option>
                <option value="critical">Critical Risk</option>
              </select>
              {errors.riskLevel && <p className="mt-1 text-sm text-red-600">{errors.riskLevel}</p>}
            </div>

            <div>
              <label htmlFor="lawEnforcementCaseNumber" className="block text-sm font-medium text-gray-700 mb-2">
                Law Enforcement Case Number (Optional)
              </label>
              <input
                type="text"
                id="lawEnforcementCaseNumber"
                name="lawEnforcementCaseNumber"
                value={formData.lawEnforcementCaseNumber}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter the case number if law enforcement is involved"
              />
            </div>

            <div>
              <label htmlFor="investigatingAgency" className="block text-sm font-medium text-gray-700 mb-2">
                Investigating Agency (Optional)
              </label>
              <input
                type="text"
                id="investigatingAgency"
                name="investigatingAgency"
                value={formData.investigatingAgency}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="e.g., Local Police Department"
              />
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                id="isPublic"
                name="isPublic"
                checked={formData.isPublic}
                onChange={handleInputChange}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
              <label htmlFor="isPublic" className="ml-2 block text-sm text-gray-700">
                Make this case public (visible to all users)
              </label>
            </div>
          </div>
        );

      case 4:
        return (
          <div className="space-y-6">
            <div className="bg-gray-50 rounded-lg p-6">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Review Your Case Information</h3>
              
              <div className="space-y-4">
                <div>
                  <h4 className="font-medium text-gray-700">Basic Information</h4>
                  <p className="text-sm text-gray-600"><strong>Title:</strong> {formData.title}</p>
                  <p className="text-sm text-gray-600"><strong>Description:</strong> {formData.description}</p>
                  {formData.individualId && (
                    <p className="text-sm text-gray-600"><strong>Individual ID:</strong> {formData.individualId}</p>
                  )}
                </div>

                <div>
                  <h4 className="font-medium text-gray-700">Location Details</h4>
                  <p className="text-sm text-gray-600"><strong>Last Seen:</strong> {formData.lastSeenLocation}</p>
                  <p className="text-sm text-gray-600"><strong>Date:</strong> {formData.lastSeenDate ? new Date(formData.lastSeenDate).toLocaleString() : 'Not specified'}</p>
                  {(formData.latitude || formData.longitude) && (
                    <p className="text-sm text-gray-600">
                      <strong>Coordinates:</strong> {formData.latitude}, {formData.longitude}
                    </p>
                  )}
                </div>

                <div>
                  <h4 className="font-medium text-gray-700">Additional Details</h4>
                  <p className="text-sm text-gray-600"><strong>Risk Level:</strong> {formData.riskLevel}</p>
                  {formData.lawEnforcementCaseNumber && (
                    <p className="text-sm text-gray-600"><strong>Case Number:</strong> {formData.lawEnforcementCaseNumber}</p>
                  )}
                  {formData.investigatingAgency && (
                    <p className="text-sm text-gray-600"><strong>Agency:</strong> {formData.investigatingAgency}</p>
                  )}
                  <p className="text-sm text-gray-600"><strong>Public:</strong> {formData.isPublic ? 'Yes' : 'No'}</p>
                </div>
              </div>
            </div>

            <div className="bg-blue-50 rounded-lg p-4">
              <div className="flex">
                <div className="flex-shrink-0">
                  <span className="text-blue-400 text-lg">ℹ️</span>
                </div>
                <div className="ml-3">
                  <p className="text-sm text-blue-700">
                    By submitting this case, you confirm that all information provided is accurate to the best of your knowledge. 
                    This case will be reviewed and may be shared with law enforcement if necessary.
                  </p>
                </div>
              </div>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Report Missing Person Case</h1>
          <p className="mt-2 text-gray-600">
            Help us locate missing persons by providing detailed information about the case.
          </p>
        </div>

        {/* Progress Steps */}
        <div className="mb-8">
          <nav aria-label="Progress">
            <ol className="flex items-center">
              {steps.map((step, stepIdx) => (
                <li key={step.name} className={`relative ${stepIdx !== steps.length - 1 ? 'pr-8 sm:pr-20' : ''} flex-1`}>
                  <div className="absolute inset-0 flex items-center" aria-hidden="true">
                    <div className={`h-0.5 w-full ${step.id <= currentStep ? 'bg-blue-600' : 'bg-gray-200'}`} />
                  </div>
                  <div className={`relative flex h-8 w-8 items-center justify-center rounded-full ${
                    step.id <= currentStep ? 'bg-blue-600' : 'bg-gray-200'
                  }`}>
                    <span className={`text-sm font-medium ${
                      step.id <= currentStep ? 'text-white' : 'text-gray-500'
                    }`}>
                      {step.id}
                    </span>
                  </div>
                  <div className="absolute top-10 left-1/2 transform -translate-x-1/2">
                    <span className={`text-xs font-medium ${
                      step.id <= currentStep ? 'text-blue-600' : 'text-gray-500'
                    }`}>
                      {step.title}
                    </span>
                  </div>
                </li>
              ))}
            </ol>
          </nav>
        </div>

        {/* Form */}
        <div className="bg-white rounded-lg shadow p-6">
          <form onSubmit={handleSubmit}>
            {renderStepContent()}

            {/* Error Display */}
            {error && (
              <div className="mt-4 bg-red-50 border border-red-200 rounded-md p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <span className="text-red-400 text-lg">⚠️</span>
                  </div>
                  <div className="ml-3">
                    <p className="text-sm text-red-700">{error}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Navigation Buttons */}
            <div className="mt-8 flex justify-between">
              <button
                type="button"
                onClick={prevStep}
                disabled={currentStep === 1}
                className={`px-4 py-2 border border-gray-300 rounded-md text-sm font-medium ${
                  currentStep === 1
                    ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                    : 'bg-white text-gray-700 hover:bg-gray-50'
                }`}
              >
                Previous
              </button>

              <div className="flex space-x-3">
                {currentStep < steps.length ? (
                  <button
                    type="button"
                    onClick={nextStep}
                    className="px-4 py-2 bg-blue-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-blue-700"
                  >
                    Next
                  </button>
                ) : (
                  <button
                    type="submit"
                    disabled={loading}
                    className="px-4 py-2 bg-green-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {loading ? 'Creating Case...' : 'Create Case'}
                  </button>
                )}
              </div>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default ReportCaseForm;
