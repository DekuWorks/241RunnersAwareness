import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { fetchCases, deleteCase } from '../../features/cases/casesSlice';
import { toast } from 'react-toastify';

const CaseList = ({ onEdit }) => {
  const dispatch = useDispatch();
  const { cases, loading, error } = useSelector((state) => state.cases);
  const { user } = useSelector((state) => state.auth);
  
  useEffect(() => {
    dispatch(fetchCases({}));
  }, [dispatch]);

  const handleDelete = async (caseId) => {
    if (window.confirm('Are you sure you want to delete this case?')) {
      try {
        await dispatch(deleteCase(caseId)).unwrap();
        toast.success('Case deleted successfully');
      } catch (err) {
        toast.error(err.message || 'Failed to delete case');
      }
    }
  };

  const statusColors = {
    missing: 'bg-yellow-100 text-yellow-800',
    found: 'bg-green-100 text-green-800',
    urgent: 'bg-red-100 text-red-800',
    resolved: 'bg-blue-100 text-blue-800',
  };

  return (
    <div className="bg-white shadow-lg rounded-lg overflow-hidden">
      <table className="min-w-full">
        <thead className="bg-gray-800 text-white">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">Title</th>
            <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">Status</th>
            <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">Actions</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {loading ? (
            <tr>
              <td colSpan="3" className="text-center py-8">Loading cases...</td>
            </tr>
          ) : error ? (
            <tr>
              <td colSpan="3" className="text-center py-8 text-red-500">{error}</td>
            </tr>
          ) : (
            cases.map((caseItem) => (
              <tr key={caseItem.id}>
                <td className="px-6 py-4 whitespace-nowrap">{caseItem.title}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className={`capitalize px-3 py-1 inline-flex text-sm leading-5 font-semibold rounded-full ${statusColors[caseItem.status] || 'bg-gray-100 text-gray-800'}`}>
                    {caseItem.status}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  {user?.role === 'admin' && (
                    <>
                      <button onClick={() => onEdit(caseItem)} className="text-indigo-600 hover:text-indigo-900 mr-4">Edit</button>
                      <button onClick={() => handleDelete(caseItem.id)} className="text-red-600 hover:text-red-900">Delete</button>
                    </>
                  )}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};

export default CaseList; 