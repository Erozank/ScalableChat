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
  setFriendRequests,
  setChats,
  onDeleteFriendSignalR
}) {
  // Delete friend and associated chat
  const handleDeleteFriend = async (friend) => {
    if (!connection) return;
    try {
      // Call SignalR method
      await connection.invoke("DeleteFriend", friend.userId);
    } catch (error) {
      console.error("Error deleting friend:", error);
    }
    // Remove friend from the list
    setFriends((prev) => prev.filter((f) => f.userId !== friend.userId));
    // Remove associated chat
    setChats((prev) => prev.filter((chat) => chat.name !== friend.nickname));
  };

  // Delete friend and chat by SignalR event
  useEffect(() => {
    if (!onDeleteFriendSignalR) return;
    onDeleteFriendSignalR((id) => {
      setFriends((prev) => prev.filter((f) => f.userId !== id));
      setChats((prev) => prev.filter((chat) => {
        const friend = friends.find(f => f.userId === id);
        return friend ? chat.name !== friend.nickname : true;
      }));
    });
  }, [onDeleteFriendSignalR, setFriends, setChats, friends]);

  useEffect(() => {

  
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
      <FriendList friends={friends} onStartChat={onStartChat} onDeleteFriend={handleDeleteFriend} />
    </div>
  );
}

export default Friends;