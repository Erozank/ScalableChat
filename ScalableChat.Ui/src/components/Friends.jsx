import React, { useState, useEffect, useCallback } from "react";
import AddFriend from "./AddFriend";
import FriendRequestList from "./FriendRequestList";
import FriendList from "./FriendList";
import axios from "axios";

function Friends({ apiServer, jwt, connection, sendFriendRequest, acceptFriendRequest, onStartChat }) {
  const [friends, setFriends] = useState([]);
  const [friendRequests, setFriendRequests] = useState([]);

  const fetchFriends = useCallback(async () => {
    try {
      const response = await axios.get(`${apiServer}/friends`, {
        headers: { Authorization: `Bearer ${jwt}` },
      });
      setFriends(response.data.friends || []);
      console.log("Friends fetched:", response.data.friends);
    } catch (error) {
      console.error("Error fetching friends:", error);
    }
  }, [apiServer, jwt]);

  const fetchFriendRequests = useCallback(async () => {
    try {
      const response = await axios.get(`${apiServer}/friend-requests`, {
        headers: { Authorization: `Bearer ${jwt}` },
      });
      setFriendRequests(response.data.receivedRequests || []);
      console.log("Friend requests fetched:", response.data.receivedRequests);
    } catch (error) {
      console.error("Error fetching friend requests:", error);
    }
  }, [apiServer, jwt]);

  useEffect(() => {
    fetchFriends();
    fetchFriendRequests();
  }, [fetchFriends, fetchFriendRequests]);

  // Suscripción al evento SignalR para recibir friend requests
  useEffect(() => {
    if (!connection) return;

    const handleReceiveFriendRequest = (friendData) => {
      setFriendRequests((prev) => {
        const exists = prev.some((req) => req.userId === friendData.userId);
        if (exists) return prev;
        return [
          ...prev,
          { nickname: friendData.nickname, userId: friendData.userId }
        ];
      });
      console.log("Received friend request from: ", friendData.nickname);
    };

    const handleFriendRequestAccepted = (request) => {
    console.log("Friend request accepted:", request);
    setFriendRequests(friendRequests.filter((r) => r.userId !== request.userId));
    setFriends((prev) => {
      const exists = prev.some((f) => f.userId === request.userId);
      if (exists) return prev;
      return [...prev, request];
    });
  };

    connection.on("ReceiveFriendRequest", handleReceiveFriendRequest);

    connection.on("FriendRequestAccepted", handleFriendRequestAccepted);

    return () => {
      connection.off("ReceiveFriendRequest", handleReceiveFriendRequest);
      connection.off("FriendRequestAccepted", handleFriendRequestAccepted);
    };
  }, [connection]);

  const handleAcceptRequest = async (request) => {
    console.log("Accepting friend request:", request);
    await acceptFriendRequest(request.userId);
    setFriendRequests(friendRequests.filter((r) => r.userId !== request.userId));
    setFriends((prev) => {
      const exists = prev.some((f) => f.userId === request.userId);
      if (exists) return prev;
      return [...prev, request];
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
      <FriendList friends={friends} onStartChat={onStartChat} />
    </div>
  );
}

export default Friends;