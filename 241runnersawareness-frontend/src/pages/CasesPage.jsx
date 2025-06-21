import React, { useState } from 'react';
import { useSelector } from 'react-redux';
import CaseList from '../components/cases/CaseList';
import CaseForm from '../components/cases/CaseForm';

const CasesPage = () => {
  const { user } = useSelector((state) => state.auth);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCase, setSelectedCase] = useState(null);

  const handleOpenModal = (caseData = null) => {
    setSelectedCase(caseData);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setSelectedCase(null);
    setIsModalOpen(false);
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Manage Cases</h1>
        {user?.role === 'admin' && (
          <button
            onClick={() => handleOpenModal()}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Add Case
          </button>
        )}
      </div>

      <CaseList onEdit={handleOpenModal} />

      <CaseForm
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        caseData={selectedCase}
      />
    </div>
  );
};

export default CasesPage; 