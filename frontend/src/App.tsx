import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';
import { store } from './store';
import { useAppDispatch, useAppSelector } from './hooks/redux';
import { fetchMe } from './store/slices/authSlice';
import Login from './pages/Login';
import Signup from './pages/Signup';
import Profile from './pages/Profile';
import Runners from './pages/Runners';
import Header from './components/Header';
import PrivateRoute from './components/PrivateRoute';
import './App.css';

function AppContent() {
  const dispatch = useAppDispatch();
  const { isAuthenticated, token } = useAppSelector((state) => state.auth);

  useEffect(() => {
    // If we have a token but no user data, fetch user info
    if (token && !isAuthenticated) {
      dispatch(fetchMe());
    }
  }, [dispatch, token, isAuthenticated]);

  return (
    <div className="App">
      <Router>
        {isAuthenticated && <Header />}
        <main className="main-content">
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
            <Route 
              path="/profile" 
              element={
                <PrivateRoute>
                  <Profile />
                </PrivateRoute>
              } 
            />
            <Route 
              path="/runners" 
              element={
                <PrivateRoute>
                  <Runners />
                </PrivateRoute>
              } 
            />
            <Route 
              path="/" 
              element={
                isAuthenticated ? <Navigate to="/profile" replace /> : <Navigate to="/login" replace />
              } 
            />
          </Routes>
        </main>
      </Router>
    </div>
  );
}

function App() {
  return (
    <Provider store={store}>
      <AppContent />
    </Provider>
  );
}

export default App;
