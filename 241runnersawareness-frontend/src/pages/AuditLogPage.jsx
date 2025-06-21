import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { fetchAuditLogs } from '../features/audit/auditLogSlice';

const AuditLogPage = () => {
  const dispatch = useDispatch();
  const { logs, loading, error } = useSelector((state) => state.audit);

  useEffect(() => {
    dispatch(fetchAuditLogs({}));
  }, [dispatch]);

  return (
    <div>
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Audit Logs</h1>
      <div className="bg-white shadow-lg rounded-lg overflow-hidden">
        <table className="min-w-full">
          <thead className="bg-gray-800 text-white">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">User</th>
              <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">Action</th>
              <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider">Timestamp</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {loading ? (
              <tr>
                <td colSpan="3" className="text-center py-8">Loading logs...</td>
              </tr>
            ) : error ? (
              <tr>
                <td colSpan="3" className="text-center py-8 text-red-500">{error}</td>
              </tr>
            ) : (
              logs.map((log) => (
                <tr key={log.id}>
                  <td className="px-6 py-4 whitespace-nowrap">{log.user?.name || 'System'}</td>
                  <td className="px-6 py-4 whitespace-nowrap">{log.action}</td>
                  <td className="px-6 py-4 whitespace-nowrap">{new Date(log.createdAt).toLocaleString()}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AuditLogPage; 