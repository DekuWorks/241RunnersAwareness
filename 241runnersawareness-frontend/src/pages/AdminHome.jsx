import { useDispatch, useSelector } from "react-redux";
import { login, logout } from "../features/auth/authSlice";
import { useState } from "react";

const AdminHome = () => {
  const dispatch = useDispatch();
  const user = useSelector((state) => state.auth.user);
  const [username, setUsername] = useState("");

  const handleLogin = () => {
    dispatch(login({ name: username || "Guest", role: "admin" }));
  };

  return (
    <div className="space-y-4 p-4">
      <h1 className="text-2xl font-bold">Admin Login</h1>

      {user ? (
        <div>
          <p>Welcome, <strong>{user.name}</strong>!</p>
          <button
            onClick={() => dispatch(logout())}
            className="bg-red-500 text-white px-4 py-2 rounded"
          >
            Log out
          </button>
        </div>
      ) : (
        <div className="space-y-2">
          <input
            type="text"
            placeholder="Enter name"
            className="border p-2 rounded w-full"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
          <button
            onClick={handleLogin}
            className="bg-green-500 text-white px-4 py-2 rounded"
          >
            Log in
          </button>
        </div>
      )}
    </div>
  );
};

export default AdminHome;
