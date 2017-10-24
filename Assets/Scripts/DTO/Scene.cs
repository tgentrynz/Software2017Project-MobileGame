using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class Scene
    {
        [PrimaryKey]
        public string identifier { get; set; }
        public string storyID { get; set; }
        public string description { get; set; }
        public string background { get; set; }

        public override string ToString()
        {
            return String.Format("Scene: identifier={0}, storyID={1}, description={2}, background={3}", identifier, storyID, description, background);
        }
    }
}
