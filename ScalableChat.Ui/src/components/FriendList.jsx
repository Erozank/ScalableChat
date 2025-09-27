import React from "react";
import { Button } from "react-bootstrap";
import "../styles/chat.css";

const FriendList = ({ friends, onStartChat, onDeleteFriend }) => {
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
            <div className="d-flex gap-2">
              <Button
                size="sm"
                className="chat-button"
                onClick={() => onStartChat(friend)}
              >
                Chat
              </Button>
              <Button
                size="sm"
                variant="danger"
                onClick={() => {
                  if (window.confirm(`Are you sure you want to delete the friend ${friend.nickname}?`)) {
                    onDeleteFriend(friend);
                  }
                }}
              >
                Delete Friend
              </Button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default FriendList;