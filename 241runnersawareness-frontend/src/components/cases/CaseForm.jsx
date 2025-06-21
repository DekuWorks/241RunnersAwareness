import React, { useState, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { createCase, updateCase } from '../../features/cases/casesSlice';
import { toast } from 'react-toastify';

const CaseForm = ({ isOpen, onClose, caseData }) => {
  const dispatch = useDispatch();
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    status: 'missing',
  });

  useEffect(() => {
    if (caseData) {
      setFormData(caseData);
    } else {
      setFormData({ title: '', description: '', status: 'missing' });
    }
  }, [caseData]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (caseData) {
        await dispatch(updateCase({ id: caseData.id, caseData: formData })).unwrap();
        toast.success('Case updated successfully!');
      } else {
        await dispatch(createCase(formData)).unwrap();
        toast.success('Case created successfully!');
      }
      onClose();
    } catch (err) {
      toast.error(err.message || 'Failed to save case');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed z-50 inset-0 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen">
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75"></div>
        <div className="bg-white rounded-lg shadow-xl transform transition-all sm:max-w-lg sm:w-full">
          <form onSubmit={handleSubmit} className="p-6">
            <h3 className="text-2xl font-bold text-gray-800 mb-6">
              {caseData ? 'Edit Case' : 'Add New Case'}
            </h3>
            <div className="space-y-4">
              <div>
                <label htmlFor="title" className="block text-sm font-medium text-gray-700">Title</label>
                <input type="text" name="title" id="title" value={formData.title} onChange={handleChange} required className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-red-500 focus:border-red-500"/>
              </div>
              <div>
                <label htmlFor="description" className="block text-sm font-medium text-gray-700">Description</label>
                <textarea name="description" id="description" value={formData.description} onChange={handleChange} required rows="4" className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-red-500 focus:border-red-500"></textarea>
              </div>
              <div>
                <label htmlFor="status" className="block text-sm font-medium text-gray-700">Status</label>
                <select id="status" name="status" value={formData.status} onChange={handleChange} className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-red-500 focus:border-red-500">
                  <option value="missing">Missing</option>
                  <option value="found">Found</option>
                  <option value="urgent">Urgent</option>
                  <option value="resolved">Resolved</option>
                </select>
              </div>
            </div>
            <div className="mt-8 flex justify-end gap-4">
              <button type="button" onClick={onClose} className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300">Cancel</button>
              <button type="submit" className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700">{caseData ? 'Save Changes' : 'Create Case'}</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default CaseForm; 