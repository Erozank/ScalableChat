import React from "react";

function FriendRequestList({ requests, onAccept, onReject }) {
  return (
    <div className="friend-request-list">
      <h4>Friend Requests</h4>
      {requests.length === 0 ? (
        <p>No friend requests.</p>
      ) : (
        <ul>
          {requests.map((req) => (
            <li key={req.userId} className="d-flex align-items-center gap-2">
              <strong>{req.nickname}</strong> (ID: {req.userId})
              <button
                className="btn btn-success btn-sm ms-2"
                title="Accept"
                onClick={() => {
                  if (onAccept) onAccept(req);
                }}
                style={{ padding: "0.2em 0.6em" }}
              >
                <i className="bi bi-check-lg"></i>
              </button>
              <button
                className="btn btn-danger btn-sm ms-1"
                title="Reject"
                onClick={() => onReject && onReject(req)}
                style={{ padding: "0.2em 0.6em" }}
              >
                <i className="bi bi-x-lg"></i>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default FriendRequestList;