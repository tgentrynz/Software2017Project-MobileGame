using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class PlayerInstance
    {
        [PrimaryKey, AutoIncrement]
        public int identifier { get; set; }
        public int gameID { get; set; }
        public int playerID { get; set; }
        public int score { get; set; }
        public string scene { get; set; }

        public override string ToString()
        {
            return String.Format("PlayerInstance: identifier={0}, gameID={1}, playerID={2}, score={3}", identifier, gameID, playerID, score);
        }
    }
}
