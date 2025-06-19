import { configureStore } from '@reduxjs/toolkit';
import authReducer, { login, logout, loginAsync } from "./authSlice"; // ✅ correct relative path

describe("authSlice", () => {
  let store;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        auth: authReducer,
      },
    });
  });

  it("should return the initial state", () => {
    expect(store.getState().auth).toEqual({
      user: null,
      loading: false,
      error: null,
    });
  });

  it("should handle login", () => {
    const action = login({ name: "Marcus", role: "admin" });
    const state = authReducer(undefined, action);

    expect(state.user).toEqual({ name: "Marcus", role: "admin" });
    expect(state.error).toBeNull();
  });

  it("should handle logout", () => {
    // First set some initial state
    store.dispatch({
      type: loginAsync.fulfilled.type,
      payload: { name: 'Test User', role: 'admin' },
    });

    // Then dispatch logout
    store.dispatch(logout());

    // Verify the state is reset
    expect(store.getState().auth).toEqual({
      user: null,
      loading: false,
      error: null,
    });
  });

  describe('loginAsync', () => {
    it('should set loading state when pending', () => {
      store.dispatch({ type: loginAsync.pending.type });
      expect(store.getState().auth.loading).toBe(true);
      expect(store.getState().auth.error).toBe(null);
    });

    it('should set user when fulfilled', () => {
      const user = { name: 'Test User', role: 'admin', token: 'test-token' };
      store.dispatch({ type: loginAsync.fulfilled.type, payload: user });
      
      expect(store.getState().auth.loading).toBe(false);
      expect(store.getState().auth.user).toEqual(user);
      expect(store.getState().auth.error).toBe(null);
    });

    it('should set error state when rejected', () => {
      const error = 'Login failed';
      store.dispatch({
        type: loginAsync.rejected.type,
        payload: error,
      });

      expect(store.getState().auth.loading).toBe(false);
      expect(store.getState().auth.user).toBe(null);
      expect(store.getState().auth.error).toBe(error);
    });
  });
});
