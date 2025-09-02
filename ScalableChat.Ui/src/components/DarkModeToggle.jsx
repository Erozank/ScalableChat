import React from "react";

const DarkModeToggle = ({ darkMode, setDarkMode }) => (
  <button
    onClick={() => setDarkMode((prev) => !prev)}
    className="btn btn-secondary toggle-darkmode-btn d-flex align-items-center"
    aria-label="Toggle dark mode"
    style={{ marginLeft: "auto", gap: "0.5rem" }}
  >
    <i
      className={`bi bi-sun-fill${!darkMode ? " active-darkmode-icon" : ""}`}
      style={{ fontSize: "1.5rem", transition: "color 0.2s" }}
    ></i>
    <i
      className={`bi bi-moon-fill${darkMode ? " active-darkmode-icon" : ""}`}
      style={{ fontSize: "1.5rem", transition: "color 0.2s" }}
    ></i>
  </button>
);

export default DarkModeToggle;