using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class PlayerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool HasPickedUp { get; set; }
        public int NumLives { get; set; }
        public bool CompletedLevel { get; set; }

        public PlayerInfo()
        {
            Id = Guid.NewGuid();
            Name = "";
            X = 0;
            Y = 0;
            HasPickedUp = false;
            NumLives = 3;
            CompletedLevel = false;
        }
    }
}
