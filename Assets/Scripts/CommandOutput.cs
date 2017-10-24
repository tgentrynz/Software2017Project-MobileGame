using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Data structure to hold the results of a player's input.
    /// </summary>
    public struct CommandOutput
    {
        /// <summary>
        /// Indicates the command ran without a system error.
        /// </summary>
        public bool Success;
        /// <summary>
        /// Holds the player output result of the command.
        /// </summary>
        public string Message;
        /// <summary>
        /// Holds the system output result of the command.
        /// </summary>
        public string SystemMessage;
        
        public CommandOutput(bool Success = false, string Message = "", string SystemMessage = "")
        {
            this.Success = Success;
            this.Message = Message;
            this.SystemMessage = SystemMessage;
        }
    }
}
