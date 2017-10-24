using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class SceneInventory
    {
        [PrimaryKey, AutoIncrement]
        public int identifier { get; set; }
        public int gameID { get; set; }
        public string sceneID { get; set; }
        public string componentID { get; set; }

        public override string ToString()
        {
            return String.Format("SceneInventory: sceneID={0}, componentID={1}", sceneID, componentID);
        }
    }
}
