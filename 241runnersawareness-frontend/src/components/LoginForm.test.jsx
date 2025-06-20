import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Provider } from 'react-redux';
import { BrowserRouter as Router } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import authReducer from '../../features/auth/authSlice';
import LoginForm from './LoginForm';

const store = configureStore({
  reducer: {
    auth: authReducer,
  },
});

describe('LoginForm', () => {
  test('renders login form and handles submission', async () => {
    render(
      <Provider store={store}>
        <Router>
          <LoginForm />
        </Router>
      </Provider>
    );

    // Check if form fields are rendered
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /log in/i })).toBeInTheDocument();

    // Simulate user input
    fireEvent.change(screen.getByLabelText(/email/i), {
      target: { value: 'test@example.com' },
    });
    fireEvent.change(screen.getByLabelText(/password/i), {
      target: { value: 'password123' },
    });

    // You can also mock the login thunk and test submission
    // For now, we're just checking if the form can be filled
    expect(screen.getByLabelText(/email/i).value).toBe('test@example.com');
    expect(screen.getByLabelText(/password/i).value).toBe('password123');
  });
}); 