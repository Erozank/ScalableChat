import { useState } from "react";
import axios from "axios";
import React from "react";
import { Link } from "react-router-dom";

function Login({ setIsLoggedIn, setJwt }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMsg, setErrorMsg] = useState(""); // Nuevo estado para el mensaje de error
  const apiServer = import.meta.env.VITE_CHAT_API;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrorMsg(""); // Limpiar mensaje de error antes de intentar login
    try {
      const response = await axios.post(`${apiServer}/login`, {
        email,
        password,
      });
      if (response.status === 200) {
        const { token } = response.data; 
        localStorage.setItem("jwt", token); // Store the JWT in localStorage
        setJwt(token)
        setIsLoggedIn(true);
      }
    } catch (error) {
      setErrorMsg("Incorrect email or password"); // Mostrar mensaje de error
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
