import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import api from '../services/api';
import { toast } from 'react-toastify';

const MyCase = () => {
  const [myCase, setMyCase] = useState(null);
  const [preview, setPreview] = useState('');
  const [image, setImage] = useState(null);
  const { token } = useSelector(state => state.auth);

  useEffect(() => {
    const fetchMyCase = async () => {
      try {
        const res = await api.get('/cases/me', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setMyCase(res.data);
        setPreview(res.data.image);
      } catch (err) {
        console.error('Failed to load case data', err);
      }
    };
    fetchMyCase();
  }, [token]);

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result);
      setImage(reader.result);
    };
    reader.readAsDataURL(file);
  };

  const handleSubmit = async () => {
    try {
      await api.patch(`/cases/${myCase.id}`, { image }, {
        headers: { Authorization: `Bearer ${token}` }
      });
      toast.success('Photo updated successfully!');
    } catch (err) {
      // Error is handled by the global interceptor
      console.error(err);
    }
  };

  if (!myCase) return <p className="text-center mt-12 text-gray-500">Loading...</p>;

  const lastUpdate = new Date(myCase.updatedAt);
  const nextDueDate = new Date(new Date(myCase.updatedAt).setMonth(lastUpdate.getMonth() + 6));
  const isUpdateOverdue = new Date() > nextDueDate;

  const daysSinceUpdate = (Date.now() - new Date(myCase.updatedAt)) / (1000 * 60 * 60 * 24);

  const newHandleSubmit = async () => {
    if (daysSinceUpdate < 30) {
      toast.warn('You can only update your photo every 30 days.');
      return;
    }
    await handleSubmit();
  };

  return (
    <div className="max-w-3xl mx-auto px-6 py-10">
      <h1 className="text-4xl font-bold text-gray-800 mb-8 text-center">My Case Record</h1>

      <div className="bg-white border border-gray-200 rounded-lg shadow-md p-6 space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-gray-500">Name</p>
            <p className="text-lg font-semibold text-gray-800">{myCase.name}</p>
          </div>
          <div>
            <p className="text-sm text-gray-500">Status</p>
            <span className={`inline-block px-3 py-1 text-sm font-medium rounded-full ${
              myCase.status === 'Missing' ? 'bg-yellow-100 text-yellow-800' :
              myCase.status === 'Found' ? 'bg-green-100 text-green-800' :
              myCase.status === 'Urgent' ? 'bg-red-100 text-red-800' :
              'bg-gray-100 text-gray-800'
            }`}>
              {myCase.status}
            </span>
          </div>
          <div>
            <p className="text-sm text-gray-500">Last Updated</p>
            <p className="text-base text-gray-700">
              {new Date(myCase.updatedAt).toLocaleDateString()}
            </p>
          </div>
        </div>

        <div className="pt-4">
          <label className="block mb-2 text-sm font-medium text-gray-700">Update Photo</label>
          <input
            type="file"
            accept="image/*"
            onChange={handleImageChange}
            className="block w-full text-sm text-gray-700 bg-white border border-gray-300 rounded-md cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          {isUpdateOverdue && (
            <p className="text-sm text-red-600 font-medium mt-2">
              🗓️ Next image update required by: {nextDueDate.toLocaleDateString()}
            </p>
          )}
          {preview && (
            <img
              src={preview}
              alt="Preview"
              className="mt-4 w-40 h-40 object-cover border border-gray-300 rounded shadow-sm"
            />
          )}
        </div>

        <button
          onClick={newHandleSubmit}
          className="mt-6 w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-md transition"
        >
          Submit Photo Update
        </button>
      </div>
    </div>
  );
};

export default MyCase; 