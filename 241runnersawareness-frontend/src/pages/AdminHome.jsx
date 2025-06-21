import React from "react";
import { useSelector } from "react-redux";

const AdminHome = () => {
  const { user } = useSelector((state) => state.auth);

  // Dummy data for dashboard metrics
  const stats = [
    { name: 'Total Users', value: '1,204', change: '+2.5%', changeType: 'positive' },
    { name: 'Active Cases', value: '82', change: '-1.2%', changeType: 'negative' },
    { name: 'Resolved Cases', value: '450', change: '+10.8%', changeType: 'positive' },
    { name: 'Pending Approvals', value: '12', change: '0%', changeType: 'neutral' },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Dashboard</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat) => (
          <div key={stat.name} className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-gray-500 text-sm font-medium">{stat.name}</h3>
            <p className="text-3xl font-semibold text-gray-900 mt-2">{stat.value}</p>
            <p className={`mt-2 flex items-baseline text-sm font-semibold ${
              stat.changeType === 'positive' ? 'text-green-600' : 
              stat.changeType === 'negative' ? 'text-red-600' : 'text-gray-500'
            }`}>
              {stat.change} from last month
            </p>
          </div>
        ))}
      </div>

      <div className="mt-8 bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-bold text-gray-800">Recent Activity</h2>
        <p className="mt-4 text-gray-600">
          Activity feed coming soon. This area will display recent user registrations, case updates, and other important events.
        </p>
      </div>
    </div>
  );
};

export default AdminHome;
