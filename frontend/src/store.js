import { configureStore } from '@reduxjs/toolkit'
import authReducer from './features/auth/authSlice'

// You can add more slices later (alerts, users, etc.)
export const store = configureStore({
  reducer: {
    auth: authReducer,
  },
})
