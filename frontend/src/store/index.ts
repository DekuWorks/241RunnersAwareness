import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';
import runnersReducer from './slices/runnersSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    runners: runnersReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
