import React from "react";
import { NavLink } from "react-router-dom";

const HeaderNav = ({ pendingRequestsCount = 0, unreadMessagesCount = 0 }) => (
  <nav className="flex-grow-1 d-flex justify-content-center">
    <div className="d-flex align-items-center gap-3">
      <NavLink
        to="/"
        className={({ isActive }) =>
          "btn btn-primary mx-2 position-relative" + (isActive ? " active-nav" : "")
        }
      >
        <i className="bi bi-chat-dots me-2"></i>
        Chats
        {unreadMessagesCount > 0 && (
          <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
            {unreadMessagesCount}
          </span>
        )}
      </NavLink>
      <NavLink
        to="/friends"
        className={({ isActive }) =>
          "btn btn-primary mx-2 position-relative" + (isActive ? " active-nav" : "")
        }
      >
        <i className="bi bi-people me-2"></i>
        Friends
        {pendingRequestsCount > 0 && (
          <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
            {pendingRequestsCount}
            <span className="visually-hidden">Pending friend requests</span>
          </span>
        )}
      </NavLink>
    </div>
  </nav>
);

export default HeaderNav;