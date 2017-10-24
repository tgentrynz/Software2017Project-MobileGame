using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.DomainClasses
{
    /// <summary>
    /// Representation of an item that can be used by the player to open new exits in a scene.
    /// </summary>
    [Serializable]
    class SceneDoor : SceneItem
    {
        public readonly string requiredItemIdentifier;
        public readonly string successMessage;
        public readonly string failMessage;
        public readonly SceneExit exit;

        public SceneDoor(string identifier, string fullName, string requiredItemIdentifier, string successMessage, string failMessage, string linkedScene, string exitID, string exitName) : base(identifier, fullName, ComponentType.Door)
        {
            this.requiredItemIdentifier = requiredItemIdentifier;
            this.successMessage = successMessage;
            this.failMessage = failMessage;
            this.exit = new SceneExit(exitID, exitName, linkedScene);
        }

        public SceneDoor(DTO.SceneComponent inputComponent, DTO.SceneDoor inputDoor, SceneExit inputExit) : base(inputComponent.identifier, inputComponent.fullName, ComponentType.Door)
        {
            this.requiredItemIdentifier = inputDoor.keyID;
            this.successMessage = inputDoor.messageSuccess;
            this.failMessage = inputDoor.messageFail;
            this.exit = inputExit;
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
