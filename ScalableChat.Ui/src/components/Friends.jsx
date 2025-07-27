import React, { useState, useEffect } from "react";
import AddFriend from "./AddFriend";
import FriendRequestList from "./FriendRequestList";
import FriendList from "./FriendList";
import axios from "axios";

function Friends({ apiServer, jwt, sendFriendRequest, acceptFriendRequest, friendRequests, setFriendRequests }) {
  const [friends, setFriends] = useState([]);

  // Función para cargar amigos desde la API
  const fetchFriends = async () => {
    try {
      const response = await axios.get(`${apiServer}/friends`, {
        headers: { Authorization: `Bearer ${jwt}` },
      });
      setFriends(response.data.friends);
      console.log("Friends fetched:", response.data.friends);
    } catch (error) {
      console.error("Error fetching friends:", error);
    }
  };

  useEffect(() => {
    fetchFriends();
  }, [apiServer, jwt]);

  const handleAcceptRequest = async (request) => {
    console.log("Accepting friend request:", request);
    await acceptFriendRequest(request.userId);
    setFriendRequests(friendRequests.filter((r) => r.userId !== request.userId));
    setFriends((prev) => {
      const exists = prev.some((req) => req.userId === req.userId);
      if (exists) return prev;
      [...prev, request]
    });
  };

  const handleRejectRequest = (request) => {
    console.log("Rejecting friend request:", request);
    setFriendRequests(friendRequests.filter((r) => r.userId !== request.userId));
  };

  return (
    <div className="friends-container">
      <AddFriend onAddFriend={sendFriendRequest} />
      <FriendRequestList
        requests={friendRequests}
        onAccept={handleAcceptRequest}
        onReject={handleRejectRequest}
      />
      <FriendList friends={friends} />
    </div>
  );
}

export default Friends;