using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    class GameInstance
    {
        [PrimaryKey, AutoIncrement]
        public int identifier { get; set; }
        public string storyID { get; set; }
    }
}
