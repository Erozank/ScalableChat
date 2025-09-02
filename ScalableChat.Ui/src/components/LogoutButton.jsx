import React from "react";

const LogoutButton = ({ setIsLoggedIn, setConnection }) => {
  const handleLogout = () => {
    localStorage.removeItem("jwt");
    if (setConnection) setConnection(undefined);
    setIsLoggedIn(false);
  };

  return (
    <button className="btn btn-danger" onClick={handleLogout} type="button">
      Logout
    </button>
  );
};

export default LogoutButton;