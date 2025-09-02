import React, { useState } from "react";

const AddFriend = ({ onAddFriend }) => {
  const [friendName, setFriendName] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    const trimmed = friendName.trim();
    if (trimmed) {
      onAddFriend(trimmed);
      setFriendName("");
    }
  };

  return (
    <form className="d-flex align-items-center" onSubmit={handleSubmit} autoComplete="off">
      <input
        type="text"
        className="form-control me-2"
        placeholder="Nickname"
        value={friendName}
        onChange={(e) => setFriendName(e.target.value)}
        aria-label="Friend nickname"
      />
      <button type="submit" className="btn btn-primary" disabled={!friendName.trim()}>
        Add friend
      </button>
    </form>
  );
};

export default AddFriend;