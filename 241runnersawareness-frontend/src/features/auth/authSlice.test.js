import authReducer, { login, logout } from "./authSlice"; // ✅ correct relative path

describe("authSlice", () => {
  const initialState = {
    user: null,
    loading: false,
    error: null,
  };

  it("should return the initial state", () => {
    expect(authReducer(undefined, { type: undefined })).toEqual(initialState);
  });

  it("should handle login", () => {
    const action = login({ name: "Marcus", role: "admin" });
    const state = authReducer(initialState, action);

    expect(state.user).toEqual({ name: "Marcus", role: "admin" });
    expect(state.error).toBeNull();
  });

  it("should handle logout", () => {
    const loggedInState = {
      user: { name: "Marcus", role: "admin" },
      loading: false,
      error: null,
    };

    const state = authReducer(loggedInState, logout());

    expect(state.user).toBeNull();
    expect(state.error).toBeNull();
  });
});
