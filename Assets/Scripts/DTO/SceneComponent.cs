using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class SceneComponent
    {
        [PrimaryKey]
        public string identifier { get; set; }
        public string fullName { get; set; }
        public string initialSceneID { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            return String.Format("SceneComponent: identifier={0}, fullName={1}, initialSceneID={2}, type={3}", identifier, fullName, initialSceneID, type);
        }
    }
}
