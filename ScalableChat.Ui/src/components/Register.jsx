import React, { useState } from 'react';
import { Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';

function Register({ setIsLoggedIn, setJwt }) {
  const [nickname, setNickname] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const apiServer = import.meta.env.VITE_CHAT_API;
  const navigate = useNavigate(); 

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    try {
      const response = await fetch(`${apiServer}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nickname, password }),
      });

      if (!response.ok) {
        throw new Error('Failed to register. Please try again.');
      }

      const data = await response.json();
      localStorage.setItem('jwt', data.token);

      setSuccess(true);
      setNickname('');
      setPassword('');
      setJwt(data.token)
      setIsLoggedIn(true);

      navigate('/');
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="form-container">
      <h2 className="form-title">Register</h2>
      {error && <Alert variant="danger" className="form-alert">{error}</Alert>}
      {success && <Alert variant="success" className="form-alert">Registration successful!</Alert>}
      <div className="form-wrapper">
        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label htmlFor="formNickname" className="form-label">Nickname</label>
            <input
              id="formNickname"
              type="text"
              className="form-input"
              placeholder="Enter nickname"
              value={nickname}
              onChange={(e) => setNickname(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="formPassword" className="form-label">Password</label>
            <input
              id="formPassword"
              type="password"
              className="form-input"
              placeholder="Enter password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="form-button">Register</button>
        </form>
      </div>
    </div>
  );
}

export default Register;
