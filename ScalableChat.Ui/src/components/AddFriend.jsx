import React, { useState } from 'react';

const AddFriend = ({ onAddFriend }) => {
  const [friendName, setFriendName] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (friendName.trim() !== '') {
      onAddFriend(friendName.trim());
      setFriendName('');
    }
  };

  return (
    <form className="d-flex align-items-center" onSubmit={handleSubmit}>
      <input
        type="text"
        className="form-control me-2"
        placeholder="Nickname"
        value={friendName}
        onChange={(e) => setFriendName(e.target.value)}
      />
      <button type="submit" className="btn btn-primary">
        Add friend
      </button>
    </form>
  );
};

export default AddFriend;