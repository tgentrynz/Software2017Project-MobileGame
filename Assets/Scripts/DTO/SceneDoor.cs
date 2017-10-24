using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;

namespace Assets.Scripts.DTO
{
    public class SceneDoor
    {
        [PrimaryKey]
        public string identifier { get; set; }
        public string keyID { get; set; }
        public string exitID { get; set; }
        public string messageSuccess { get; set; }
        public string messageFail { get; set; }

        public override string ToString()
        {
            return String.Format("SceneDoor: identifier={0}, keyID={1}, exitID={2}, messageSuccess={3}, messageFail={4}", identifier, keyID, exitID, messageSuccess, messageFail);
        }
    }
}
