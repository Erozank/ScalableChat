import { useState } from "react";
import axios from "axios";
import React from "react";
import { Link } from "react-router-dom";

function Login({ setIsLoggedIn, setJwt }) {
  const [nickname, setNickname] = useState("");
  const [password, setPassword] = useState("");
  const [errorMsg, setErrorMsg] = useState("");
  const apiServer = import.meta.env.VITE_CHAT_API;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrorMsg("");
    try {
      const response = await axios.post(`${apiServer}/login`, {
        nickname,
        password,
      });
      if (response.status === 200) {
        const { token } = response.data; 
        localStorage.setItem("jwt", token); // Store the JWT in localStorage
        setJwt(token)
        setIsLoggedIn(true);
      }
    } catch (error) {
      setErrorMsg("Incorrect nickname or password");
      console.error("Login failed:", error);
    }
  };

  return (
    <div className="form-container">
      <h2 className="form-title">Login</h2>
      <form onSubmit={handleSubmit} className="form">
        <div className="form-group">
          <label>Nickname:</label>
          <input
            type="text"
            value={nickname}
            onChange={(e) => setNickname(e.target.value)}
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
        {errorMsg && (
          <div className="form-error">
            {errorMsg}
          </div>
        )}
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
