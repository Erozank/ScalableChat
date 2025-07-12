import React, { useEffect, useState } from 'react';

const FriendsList = ({ apiServer, jwt }) => {
  const [friends, setFriends] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!jwt) {
      return; // No hace nada si no hay JWT
    }

    const fetchFriends = async () => {
      try {
        const response = await fetch(`${apiServer}/friends`, {
          headers: {
            Authorization: `Bearer ${jwt}`,
          },
        });
        if (!response.ok) throw new Error('Error fetching friends');
        const data = await response.json();
        setFriends(data);
      } catch (error) {
        console.error('Failed to fetch friends:', error);
        setFriends([]);
      } finally {
        setLoading(false);
      }
    };

    fetchFriends();
  }, [apiServer, jwt]);

  if (!jwt) return <div>Waiting for authentication...</div>;
  if (loading) return <div>Loading friends...</div>;
  if (!friends.length) return <div>You don't have friends.</div>;

  return (
    <div>
      <h5>Friends</h5>
      <ul>
        {friends.map(friend => (
          <li key={friend.UserId}>{friend.Nickname}</li>
        ))}
      </ul>
    </div>
  );
};

export default FriendsList;