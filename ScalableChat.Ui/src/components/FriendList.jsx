import React from 'react';

const FriendList = ({ friends }) => {
  if (!friends || !friends.length) return <div>You don't have friends.</div>;

  return (
    <div>
      <h5>Friends</h5>
      <ul>
        {friends.map(friend => (
          <li key={friend.userId}>{friend.nickname}</li>
        ))}
      </ul>
    </div>
  );
};

export default FriendList;