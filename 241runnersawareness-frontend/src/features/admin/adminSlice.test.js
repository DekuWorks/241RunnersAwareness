import { configureStore } from '@reduxjs/toolkit';
import adminReducer, {
  setAdminData,
  fetchAdminData,
  updateAdminSettings,
} from './adminSlice';

describe('admin slice', () => {
  let store;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        admin: adminReducer,
      },
    });
  });

  it('should handle initial state', () => {
    const state = store.getState().admin;
    expect(state.data).toEqual(null);
    expect(state.loading).toBeFalsy();
    expect(state.error).toBeNull();
  });

  it('should handle setAdminData', () => {
    const mockData = { settings: { theme: 'dark' } };
    store.dispatch(setAdminData(mockData));
    const state = store.getState().admin;
    expect(state.data).toEqual(mockData);
  });

  it('should handle fetchAdminData.pending', () => {
    store.dispatch(fetchAdminData.pending());
    const state = store.getState().admin;
    expect(state.loading).toBeTruthy();
    expect(state.error).toBeNull();
  });

  it('should handle fetchAdminData.fulfilled', () => {
    const mockData = { settings: { theme: 'light' } };
    store.dispatch(fetchAdminData.fulfilled(mockData));
    const state = store.getState().admin;
    expect(state.data).toEqual(mockData);
    expect(state.loading).toBeFalsy();
    expect(state.error).toBeNull();
  });

  it('should handle fetchAdminData.rejected', () => {
    const error = 'Failed to fetch admin data';
    store.dispatch(fetchAdminData.rejected(new Error(error)));
    const state = store.getState().admin;
    expect(state.loading).toBeFalsy();
    expect(state.error).toEqual(error);
  });

  it('should handle updateAdminSettings.fulfilled', () => {
    const mockSettings = { theme: 'dark' };
    store.dispatch(updateAdminSettings.fulfilled({ settings: mockSettings }));
    const state = store.getState().admin;
    expect(state.data.settings).toEqual(mockSettings);
    expect(state.loading).toBeFalsy();
    expect(state.error).toBeNull();
  });
}); 