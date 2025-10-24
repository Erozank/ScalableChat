namespace ScalableChat.SignalR.Models
{
    public enum KafkaActionType
    {
        Empty = 0,
        SendMessage = 1,
        FriendDeleted = 2,
        ChatDeleted = 3,
        FriendRequestAccepted = 4,
        SendFriendRequest = 5
    }
}
