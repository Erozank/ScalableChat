import { useState } from "react";
import axios from "axios";
import React from "react";
import { Link } from "react-router-dom";

function Login({ setIsLoggedIn }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const apiServer = import.meta.env.VITE_CHAT_API;

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(`${apiServer}/login`, {
        email,
        password,
      });
      if (response.status === 200) {
        const { token } = response.data; // Assuming the JWT is returned as 'token'
        localStorage.setItem("jwt", token); // Store the JWT in localStorage
        setIsLoggedIn(true);
      }
    } catch (error) {
      console.error("Login failed:", error);
    }
  };

  return (
    <div className="form-container">
      <h2 className="form-title">Login</h2>
      <form onSubmit={handleSubmit} className="form">
        <div className="form-group">
          <label>Email:</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            className="form-input"
          />
        </div>
        <div className="form-group">
          <label>Password:</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="form-input"
          />
        </div>
        <button type="submit" className="form-button">
          Login
        </button>
      </form>
      <Link to="/register" className="form-link">
        Don't have an account? Register here
      </Link>
    </div>
  );
}

export default Login;
