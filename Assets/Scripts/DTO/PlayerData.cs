using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class PlayerData
    {
        [PrimaryKey, AutoIncrement]
        public int identifier { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int savedGameID { get; set; }

        public override string ToString()
        {
            return String.Format("PlayerData: identifier={0}, username={1}, password={2}, savedGameID={3}", identifier, username, password, savedGameID);
        }
    }
}
