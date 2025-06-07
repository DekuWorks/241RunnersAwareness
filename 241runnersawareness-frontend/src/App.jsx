import { Routes, Route } from 'react-router-dom';
import AdminDashboard from './pages/AdminDashboard';
import Home from './pages/Home'; // ✅ NEW
import Layout from './components/Layout';

function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Home />} /> {/* ✅ Default route */}
        <Route path="admin" element={<AdminDashboard />} />
      </Route>
    </Routes>
  );
}

export default App;
