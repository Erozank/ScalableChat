import React from "react";
import { NavLink } from "react-router-dom";

const HeaderNav = () => (
  <nav className="flex-grow-1 d-flex justify-content-center">
    <div className="d-flex align-items-center gap-3">
      <NavLink
        to="/"
        className={({ isActive }) =>
          "btn btn-primary mx-2" + (isActive ? " active-nav" : "")
        }
      >
        <i className="bi bi-chat-dots me-2"></i>
        Chats
      </NavLink>
      <NavLink
        to="/friends"
        className={({ isActive }) =>
          "btn btn-primary mx-2" + (isActive ? " active-nav" : "")
        }
      >
        <i className="bi bi-people me-2"></i>
        Friends
      </NavLink>
    </div>
  </nav>
);

export default HeaderNav;