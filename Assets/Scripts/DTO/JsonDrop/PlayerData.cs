using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Scripts.JsonDrop;

namespace Assets.Scripts.DTO.JsonDrop
{
    [Serializable]
    public class PlayerData
    {
        [JsonDropPrimaryKey]
        public int identifier;
        public string username;
        public string password;
        public bool online;
        public long lastSync;
    }
}
