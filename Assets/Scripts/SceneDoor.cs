using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Representation of an item that can be used by the player to open new exits in a scene.
    /// </summary>
    class SceneDoor : SceneItem
    {
        public readonly string requiredItemIdentifier;
        public readonly string successMessage;
        public readonly string failMessage;
        public readonly string linkedScene;
        public readonly string exitID;
        public readonly string exitName;

        public SceneDoor(string identifier, string fullName, string requiredItemIdentifier, string successMessage, string failMessage, string linkedScene, string exitID, string exitName) : base(identifier, fullName, ComponentType.Door)
        {
            this.requiredItemIdentifier = requiredItemIdentifier;
            this.successMessage = successMessage;
            this.failMessage = failMessage;
            this.linkedScene = linkedScene;
            this.exitID = exitID;
            this.exitName = exitName;
        }

        public bool haveRequiredItem(SceneItem[] inventory)
        {
            bool output = false;
            if (requiredItemIdentifier == "")
                output = true;
            else
            {
                foreach (SceneItem item in inventory)
                {
                    if (item.identifier == requiredItemIdentifier)
                    {
                        output = true;
                        break;
                    }
                }
            }
            return output;
        }
    }
}
