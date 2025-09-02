import { useState, useEffect } from "react";
import axios from "axios";

export function useInitialChatData(isLoggedIn, apiServer, jwt) {
  const [friends, setFriends] = useState([]);
  const [friendRequests, setFriendRequests] = useState([]);
  const [chats, setChats] = useState([]);

  useEffect(() => {
    if (!isLoggedIn) return;

    // Fetch friends
    const fetchFriends = async () => {
      try {
        const response = await axios.get(`${apiServer}/friends`, {
          headers: { Authorization: `Bearer ${jwt}` },
        });
        setFriends(response.data.friends || []);
      } catch (error) {
        console.error("Error fetching friends:", error);
      }
    };

    // Fetch friend requests
    const fetchFriendRequests = async () => {
      try {
        const response = await axios.get(`${apiServer}/friend-requests`, {
          headers: { Authorization: `Bearer ${jwt}` },
        });
        setFriendRequests(response.data.receivedRequests || []);
      } catch (error) {
        console.error("Error fetching friend requests:", error);
      }
    };

    // Fetch chats
    const fetchChats = async () => {
      try {
        const response = await axios.get(`${apiServer}/chats`, {
          headers: { Authorization: `Bearer ${jwt}` },
        });
        const chatsData = response.data || [];
        const formattedChats = chatsData.map(chat => ({
          chatId: chat.chatId,
          name: chat.friend.nickname || 'No name',
          friendId: chat.friend.userId,
          messages: (chat.messages || []).sort(
            (a, b) => new Date(a.createdAt) - new Date(b.createdAt)
          ),
          unreadCount: 0
        }));
        setChats(formattedChats);
      } catch (error) {
        console.error('Error fetching chats:', error);
      }
    };

    fetchFriends();
    fetchFriendRequests();
    fetchChats();
  }, [isLoggedIn, apiServer, jwt]);

  return {
    friends, setFriends,
    friendRequests, setFriendRequests,
    chats, setChats
  };
}
