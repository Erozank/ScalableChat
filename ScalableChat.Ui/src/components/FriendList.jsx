import React from "react";
import { Button } from "react-bootstrap";
import "../styles/chat.css";

const FriendList = ({ friends, onStartChat }) => {
  if (!friends || !friends.length)
    return <div className="text-muted">You don't have any friends yet.</div>;

  return (
    <div>
      <h5>Friends</h5>
      <ul className="list-unstyled">
        {friends.map((friend) => (
          <li
            key={friend.userId}
            className="friend-item d-flex align-items-center justify-content-between"
          >
            <strong>{friend.nickname}</strong>
            <Button
              size="sm"
              className="chat-button"
              onClick={() => onStartChat(friend)}
            >
              Chat
            </Button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default FriendList;