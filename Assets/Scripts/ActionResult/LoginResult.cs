using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ActionResult
{
    public struct LoginResult
    {
        public bool success;
        public string message;
        public int playerID;

        public LoginResult(bool success, string message, int playerID = -1)
        {
            this.success = success;
            this.message = message;
            this.playerID = playerID;
        }
    }
}
