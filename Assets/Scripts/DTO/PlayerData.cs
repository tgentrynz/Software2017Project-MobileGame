using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class PlayerData
    {
        [PrimaryKey]
        public int identifier { get; set; }
        public int savedGameID { get; set; }

        public override string ToString()
        {
            return String.Format("PlayerData: identifier={0}, savedGameID={1}", identifier, savedGameID);
        }
    }
}
