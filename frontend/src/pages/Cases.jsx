import React, { useEffect, useState } from 'react';

const API_BASE_URL = '/api/cases';

const statusOptions = [
  { value: '', label: 'Status' },
  { value: 'missing', label: 'Missing' },
  { value: 'found', label: 'Found' },
  { value: 'safe', label: 'Safe' },
];

export default function Cases() {
  const [filters, setFilters] = useState({
    id: '',
    name: '',
    age: '',
    status: '',
    state: '',
    tags: '',
  });
  const [cases, setCases] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const buildQuery = () => {
    const params = [];
    if (filters.id) params.push(`id=${encodeURIComponent(filters.id)}`);
    if (filters.name) params.push(`name=${encodeURIComponent(filters.name)}`);
    if (filters.age) params.push(`age=${encodeURIComponent(filters.age)}`);
    if (filters.status) params.push(`status=${encodeURIComponent(filters.status)}`);
    if (filters.state) params.push(`state=${encodeURIComponent(filters.state)}`);
    if (filters.tags) params.push(`tags=${encodeURIComponent(filters.tags)}`);
    return params.length ? '?' + params.join('&') : '';
  };

  const fetchCases = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await fetch(API_BASE_URL + buildQuery());
      const data = await res.json();
      setCases(Array.isArray(data) ? data : []);
    } catch (e) {
      setError('Error loading cases.');
      setCases([]);
    }
    setLoading(false);
  };

  useEffect(() => {
    fetchCases();
    // eslint-disable-next-line
  }, []);

  const handleChange = e => {
    setFilters({ ...filters, [e.target.name]: e.target.value });
  };

  const handleSubmit = e => {
    e.preventDefault();
    fetchCases();
  };

  const exportCsv = async (url) => {
    try {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error('Export failed');
      }
      const blob = await response.blob();
      const downloadUrl = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = downloadUrl;
      a.download = `runners_export_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(downloadUrl);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Export error:', error);
      alert('Export failed. Please try again.');
    }
  };

  const exportAllCsv = () => {
    exportCsv('/api/individuals/export');
  };

  const exportFilteredCsv = () => {
    const params = buildQuery();
    exportCsv('/api/individuals/export/filtered' + params);
  };

  return (
    <div className="cases-container" style={{ maxWidth: 900, margin: '32px auto 0 auto', background: '#fff', borderRadius: 12, boxShadow: '0 4px 24px rgba(0,0,0,0.13)', padding: '32px 24px 24px 24px' }}>
      <h2 style={{ textAlign: 'center', marginBottom: 18 }}>Runner Cases</h2>
      <form className="filter-bar" onSubmit={handleSubmit} style={{ display: 'flex', flexWrap: 'wrap', gap: 12, marginBottom: 24, justifyContent: 'center' }}>
        <input type="text" name="id" placeholder="Runner ID" value={filters.id} onChange={handleChange} style={{ padding: 8, borderRadius: 5, border: '1.5px solid #ccc', fontSize: '1rem', minWidth: 120 }} />
        <input type="text" name="name" placeholder="Name" value={filters.name} onChange={handleChange} style={{ padding: 8, borderRadius: 5, border: '1.5px solid #ccc', fontSize: '1rem', minWidth: 120 }} />
        <input type="number" name="age" placeholder="Age" min="0" value={filters.age} onChange={handleChange} style={{ padding: 8, borderRadius: 5, border: '1.5px solid #ccc', fontSize: '1rem', minWidth: 120 }} />
        <select name="status" value={filters.status} onChange={handleChange} style={{ padding: 8, borderRadius: 5, border: '1.5px solid #ccc', fontSize: '1rem', minWidth: 120 }}>
          {statusOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
        </select>
        <input type="text" name="state" placeholder="State" value={filters.state} onChange={handleChange} style={{ padding: 8, borderRadius: 5, border: '1.5px solid #ccc', fontSize: '1rem', minWidth: 120 }} />
        <input type="text" name="tags" placeholder="Tags (comma separated)" value={filters.tags} onChange={handleChange} style={{ padding: 8, borderRadius: 5, border: '1.5px solid #ccc', fontSize: '1rem', minWidth: 120 }} />
        <button type="submit" style={{ padding: '8px 18px', borderRadius: 5, background: '#007bff', color: '#fff', border: 'none', fontWeight: 600, cursor: 'pointer' }}>Search</button>
        <button type="button" onClick={exportAllCsv} style={{ padding: '8px 18px', borderRadius: 5, background: '#28a745', color: '#fff', border: 'none', fontWeight: 600, cursor: 'pointer' }}>📊 Export All</button>
        <button type="button" onClick={exportFilteredCsv} style={{ padding: '8px 18px', borderRadius: 5, background: '#ffc107', color: '#000', border: 'none', fontWeight: 600, cursor: 'pointer' }}>📊 Export Filtered</button>
      </form>
      <div className="case-list" style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
        {loading ? (
          <div style={{ textAlign: 'center', color: '#888' }}>Loading cases...</div>
        ) : error ? (
          <div style={{ textAlign: 'center', color: '#d32f2f' }}>{error}</div>
        ) : cases.length === 0 ? (
          <div style={{ textAlign: 'center', color: '#888' }}>No cases found.</div>
        ) : (
          cases.map(c => (
            <div className="case-card" key={c.id || c.name} style={{ background: '#f8f9fa', borderRadius: 8, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', padding: '18px 20px', display: 'flex', flexDirection: 'column', gap: 6 }}>
              <div><strong>Runner ID:</strong> {c.id || ''}</div>
              <div><strong>Name:</strong> {c.name || ''}</div>
              <div><strong>Age:</strong> {c.age || ''}</div>
              <div><strong>Status:</strong> <span style={{ fontWeight: 'bold', color: '#007bff' }}>{c.status || ''}</span></div>
              <div><strong>State:</strong> {c.state || ''}</div>
              <div className="tags" style={{ marginTop: 4, display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                {(c.tags || []).map(tag => <span className="tag" key={tag} style={{ background: '#e53935', color: '#fff', borderRadius: 4, padding: '2px 8px', fontSize: '0.9rem', fontWeight: 500 }}>{tag}</span>)}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
} 