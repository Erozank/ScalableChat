
import React from "react";

const LogoutButton = ({ setIsLoggedIn, setConnection, connection, setJwt }) => {
  const handleLogout = async () => {
    localStorage.removeItem("jwt");
    if (connection) {
      try {
        await connection.stop();
        console.log("SignalR connection closed.");
      } catch (err) {
        console.error("Error closing SignalR connection:", err);
      }
    }
    if (setConnection) setConnection(undefined);
    if (setJwt) setJwt("");
    setIsLoggedIn(false);
  };

  return (
    <button className="btn btn-danger" onClick={handleLogout} type="button">
      Logout
    </button>
  );
};

export default LogoutButton;