using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Data structure that holds information required to process a player's input.
    /// </summary>
    public struct Command
    {
        public bool Valid;
        private CommandDelegate method;
        private string argument;

        public Command(bool valid, CommandDelegate method, string argument)
        {
            this.Valid = valid;
            this.method = method;
            this.argument = argument;
        }

        public CommandOutput act()
        {
            return method(argument);
        }

    }
}
