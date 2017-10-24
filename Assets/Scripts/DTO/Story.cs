using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    class Story
    {
        [PrimaryKey]
        public string identifier { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string openingScene { get; set; }

        public override string ToString()
        {
            return String.Format("Story: identifier={0}, title={1}, description={2}, openingScene={3}", identifier, title, description, openingScene);
        }
    }
}
