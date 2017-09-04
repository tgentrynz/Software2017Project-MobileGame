using Assets.Scripts.ActionResult;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the manipulation of game data.
    /// </summary>
    public class GameManager
    {
        private DataAccess dataAccess;

        private const string StartScene = "room1";
        private string playerID = "";

        private Scene currentScene;

        public Scene CurrentScene
        {
            get { return currentScene; }
        }

        //Constructor
        public GameManager()
        {
            initialise();
        }

        public void initialise()
        {
            dataAccess = new DataAccess();
            currentScene = dataAccess.findScene(StartScene);
        }
        
        public CommandOutput playerMove(string argument)
        {
            string outputMessage = " "; // String to update display with.
            string systemMessage = "Unexpected Error Encountered"; // String to update the system with.

            FindSceneExitResult searchResult; // Gets result from search for exit.
            Scene newScene = null; // The scene to move to

                
            searchResult = dataAccess.findScene(currentScene.identifier).findExit(argument);
            if (searchResult.Found)
            {
                newScene = dataAccess.findScene(searchResult.SceneExit.linkedSceneIdentifier);
                if (newScene != null)
                {
                    currentScene = newScene;
                    outputMessage = string.Format("Moved to {0}.", searchResult.SceneExit.fullName);
                    systemMessage = "display update";
                }
                else
                {
                    outputMessage = "Could not move.";
                    systemMessage = "The linked scene could not be found in the database.";
                }
            }
            else
            {
                outputMessage = searchResult.Message;
                systemMessage = searchResult.Message;
            }

            return new CommandOutput(true, outputMessage, systemMessage);
        }

        public CommandOutput playerGet(string argument)
        {
            bool success = false;
            string outputMessage = "";
            string systemMessage = "Unexpected Error Encountered";
            Scene sceneToChange = dataAccess.findScene(currentScene.identifier);

            FindSceneItemResult searchResult = sceneToChange.findItem(argument);
            if (searchResult.Found)
            {
                if(searchResult.SceneItem.Type == SceneComponent.ComponentType.Key)
                {
                    SceneKey item = (SceneKey)searchResult.SceneItem;
                    sceneToChange.removeComponent(item.identifier);
                    dataAccess.addPlayerInventoryItem(item);

                    dataAccess.commitSceneChange(sceneToChange);
                    dataAccess.commitInventoryChange();

                    success = true;
                    outputMessage = string.Format("Got {0}.", item.fullName);
                    systemMessage = "display update";
                }
                else
                {
                    outputMessage = string.Format("{0} is an item that you use.", searchResult.SceneItem.fullName);
                    systemMessage = "Tried to pick up item of wrong type.";
                }
            }
            else
            {
                outputMessage = searchResult.Message;
                systemMessage = searchResult.Message;
            }

            return new CommandOutput(success, outputMessage, systemMessage);
        }

        public CommandOutput playerDrop(string argument)
        {
            bool success = false;
            string outputMessage = "";
            string systemMessage = "Unexpected Error Encountered";

            SceneItem[] searchResult = (from item in dataAccess.findPlayerInventory(playerID) where item.fullName.ToLower().Contains(argument.ToLower()) select item).ToArray();
                     
            if (searchResult.Length == 1)
            {
                Scene sceneToChange = dataAccess.findScene(currentScene.identifier);
                sceneToChange.addKeyItem(searchResult[0].identifier, searchResult[0].fullName);
                dataAccess.removePlayerInventoryItem(searchResult[0].identifier);

                dataAccess.commitSceneChange(sceneToChange);
                dataAccess.commitInventoryChange();

                success = true;
                outputMessage = string.Format("Dropped {0}", searchResult[0].fullName);
                systemMessage = "display update";
            }
            else if(searchResult.Length > 1)
            {
                outputMessage = "You need to be more specific.";
                systemMessage = "Too many results returned";
            }
            else
            {
                outputMessage = string.Format("{0} could not be found in inventory.", argument);
                systemMessage = "Item did not exist.";
            }

            return new CommandOutput(success, outputMessage, systemMessage);
        }

        public CommandOutput playerInteract(string argument)
        {
            bool success = false;
            string outputMessage = " "; // String to update display with.
            string systemMessage = "Unexpected Error Encountered"; // String to update the system with.

            FindSceneItemResult searchResult = dataAccess.findScene(currentScene.identifier).findItem(argument);

            if (searchResult.Found)
            {
                if (searchResult.SceneItem.Type == SceneComponent.ComponentType.Door)
                {
                    SceneDoor item = (SceneDoor)searchResult.SceneItem;
                    if (item.haveRequiredItem(dataAccess.findPlayerInventory("")))
                    {
                        Scene sceneToChange = dataAccess.findScene(currentScene.identifier);
                        sceneToChange.removeComponent(item.identifier);
                        sceneToChange.addExit(item.exitID, item.exitName, item.linkedScene);
                        dataAccess.removePlayerInventoryItem(item.requiredItemIdentifier);

                        dataAccess.commitSceneChange(sceneToChange);
                        dataAccess.commitInventoryChange();

                        success = true;
                        outputMessage = item.successMessage;
                        systemMessage = "display update";
                    }
                    else
                    {
                        outputMessage = item.failMessage;
                        systemMessage = "Did not have correct key item for door.";
                    }
                }
                else
                {
                    outputMessage = string.Format("{0} is an item you can pick up.", searchResult.SceneItem.fullName);
                    systemMessage = "Wrong type of item.";
                }
            }
            else
            {
                outputMessage = searchResult.Message;
                systemMessage = searchResult.Message;
            }
            return new CommandOutput(success, outputMessage, systemMessage);
        }

        public SceneItem[] getPlayerInventory()
        {
            return dataAccess.findPlayerInventory(playerID);
        }

        public enum GameState
        {
            menu
        }
    }
}
