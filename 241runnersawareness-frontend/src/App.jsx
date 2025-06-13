import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import NavBar from './components/NavBar'
import Layout from './components/Layout'
import AdminHome from './pages/AdminHome'
import UsersPage from './pages/UsersPage'
import SettingsPage from './pages/SettingsPage'

function App() {
  return (
    <Router>
      <NavBar />
      <Routes>
        <Route path="/admin" element={<Layout />}>
          <Route index element={<AdminHome />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="settings" element={<SettingsPage />} />
        </Route>
      </Routes>
    </Router>
  )
}

export default App
