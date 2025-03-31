import React, { useState } from 'react';
import { Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom'; // Import useNavigate

function Register() {
  const [nickname, setNickname] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const apiServer = import.meta.env.VITE_CHAT_API;
  const navigate = useNavigate(); // Initialize navigate

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    try {
      const response = await fetch(`${apiServer}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nickname, email, password }),
      });

      if (!response.ok) {
        throw new Error('Failed to register. Please try again.');
      }

      const data = await response.json(); // Parse the JSON response
      localStorage.setItem('jwt', data.token); // Store the JWT in localStorage

      setSuccess(true);
      setNickname('');
      setEmail('');
      setPassword('');
      navigate('/waitingroom'); // Redirect to WaitingRoom
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="form-container">
      <h2 className="form-title">Register</h2>
      {error && <Alert variant="danger" className="form-alert">{error}</Alert>}
      {success && <Alert variant="success" className="form-alert">Registration successful!</Alert>}
      <div className="form-wrapper"> {/* Added wrapper for border */}
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
            <label htmlFor="formEmail" className="form-label">Email</label>
            <input
              id="formEmail"
              type="email"
              className="form-input"
              placeholder="Enter email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
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
      </div> {/* End of wrapper */}
    </div>
  );
}

export default Register;
