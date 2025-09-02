import React, { useEffect } from "react";
import AddFriend from "./AddFriend";
import FriendRequestList from "./FriendRequestList";
import FriendList from "./FriendList";


function Friends({
  connection,
  sendFriendRequest,
  acceptFriendRequest,
  onStartChat,
  friends,
  setFriends,
  friendRequests,
  setFriendRequests
}) {

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
      setFriendRequests((prev) => prev.filter((r) => r.userId !== request.userId));
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
  }, [connection, setFriendRequests, setFriends]);

  const handleAcceptRequest = async (request) => {
    console.log("Accepting friend request:", request);
    await acceptFriendRequest(request.userId);
    setFriendRequests((prev) => prev.filter((r) => r.userId !== request.userId));
    setFriends((prev) => {
      const exists = prev.some((f) => f.userId === request.userId);
      if (exists) return prev;
      return [...prev, request];
    });
  };

  

  const handleRejectRequest = (request) => {
    console.log("Rejecting friend request:", request);
    setFriendRequests((prev) => prev.filter((r) => r.userId !== request.userId));
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