import { configureStore } from '@reduxjs/toolkit'
import authReducer from './features/auth/authSlice'
import usersReducer from './features/users/usersSlice'
import adminReducer from './features/admin/adminSlice'
import casesReducer from './features/cases/casesSlice'
import auditLogReducer from './features/audit/auditLogSlice'

// You can add more slices later (alerts, users, etc.)
export const store = configureStore({
  reducer: {
    auth: authReducer,
    users: usersReducer,
    admin: adminReducer,
    cases: casesReducer,
    audit: auditLogReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // Ignore these action types
        ignoredActions: ['auth/login/fulfilled', 'auth/logout/fulfilled'],
        // Ignore these field paths in all actions
        ignoredActionPaths: ['meta.arg', 'payload.timestamp'],
        // Ignore these paths in the state
        ignoredPaths: ['items.dates'],
      },
    }),
})

export default store
