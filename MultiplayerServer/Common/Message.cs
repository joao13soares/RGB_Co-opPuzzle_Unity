using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public enum MessageType
    {
        PlayerName,
        NewPlayer,
        PlayerMovement,
        PlayerPickup,
        PlayerLife,
        PlayerLevelEnd,
        GameOver,
        LevelCompleted,
        FinishedSync,
        Information,
        Warning,
        Error
    }

    public class Message
    {
        public MessageType MessageType { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
        public string Description { get; set; }

        public Message(MessageType messageType, PlayerInfo playerInfo, string description)
        {
            MessageType = messageType;
            PlayerInfo = playerInfo;
            Description = description;
        }
    }
}
