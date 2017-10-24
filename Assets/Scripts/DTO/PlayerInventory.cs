using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    class PlayerInventory
    {
        [PrimaryKey, AutoIncrement]
        public int identifier { get; set; }
        public int playerID { get; set; }
        public string componentID { get; set; }

        public override string ToString()
        {
            return String.Format("PlayerInventory: playerID={0}, componentID={1}", playerID, componentID);
        }
    }
}
