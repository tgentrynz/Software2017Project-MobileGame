using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ActionResult;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the translation of raw input into system usable commands.
    /// </summary>
    public static class InputManager
    {
        static private Dictionary<string, CommandDelegate> gameCommands;
        static private Dictionary<string, CommandDelegate> GameCommands
        {
            get
            {
                if (gameCommands == null)
                {
                    CommandDelegate start = new CommandDelegate(GameViewManager.Instance.startGame);
                    CommandDelegate continueGame = new CommandDelegate(GameViewManager.Instance.continueGame);
                    CommandDelegate move = new CommandDelegate(GameViewManager.Instance.ViewModel.playerMove);
                    CommandDelegate get = new CommandDelegate(GameViewManager.Instance.ViewModel.playerGet);
                    CommandDelegate drop = new CommandDelegate(GameViewManager.Instance.ViewModel.playerDrop);
                    CommandDelegate interact = new CommandDelegate(GameViewManager.Instance.ViewModel.playerInteract);
                    CommandDelegate view = new CommandDelegate(ViewManager.Instance.changeGameView);
                    CommandDelegate openHelp = new CommandDelegate(ViewManager.Instance.openHelp);
                    CommandDelegate closeHelp = new CommandDelegate(ViewManager.Instance.closeHelp);
                    CommandDelegate exit = new CommandDelegate(ViewManager.Instance.exitGame);

                    // Initilise dictionary, organised for human ease of access
                    gameCommands = new Dictionary<string, CommandDelegate>()
                    {
                        { "new", start},
                        { "continue", continueGame},
                        { "go", move },
                        { "goto", move },
                        { "go to", move },
                        { "move", move },
                        { "move to", move },
                        { "get", get },
                        { "grab", get },
                        { "take", get },
                        { "pickup", get },
                        { "pick up", get },
                        { "drop", drop},
                        { "lose", drop},
                        { "place", drop},
                        { "use", interact },
                        { "activate", interact},
                        { "unlock", interact},
                        { "view", view},
                        { "show", view},
                        { "help", openHelp},
                        { "close", closeHelp},
                        { "exit", exit}
                    };
                    // Sort dictionary by length of key, for use in further functionality
                    gameCommands = (from pair in gameCommands orderby pair.Key.Length descending select pair).ToDictionary(pair => pair.Key, pair => pair.Value);
                }
                return gameCommands;
            }
        }

        public static Command checkInput(string rawInput)
        {
            int splitIndex = 0; // The point to split the string
            bool inputFound = false;
            CommandDelegate commandMethod = null;

            rawInput = rawInput.ToLower(); // Convert string to lowercase so it can be checked against easier.

            foreach (string command in GameCommands.Keys.ToList<string>())
            {
                if (rawInput.Substring(0, System.Math.Min(command.Length, rawInput.Length)).Equals(command))
                {
                    splitIndex = command.Length;
                    inputFound = true;
                    break;
                }
            }
            if (inputFound)
                commandMethod = gameCommands[rawInput.Substring(0, splitIndex)];

            return new Command
                (
                    inputFound,
                    commandMethod,
                    rawInput.Substring(System.Math.Min(splitIndex + 1, rawInput.Length)).Trim()
                );
        }
    }
}
