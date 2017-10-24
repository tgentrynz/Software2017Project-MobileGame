using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class SceneExit
    {
        [PrimaryKey]
        public string identifier { get; set; }
        public string linkedScene { get; set; }

        public override string ToString()
        {
            return String.Format("SceneExit: identifier={0}, linkedScene={2}", identifier, linkedScene);
        }
    }
}
