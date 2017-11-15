using Assets.Scripts.ActionResult;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.DomainClasses;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the manipulation of game data.
    /// </summary>
    public class GameManager
    {
        private const string StartScene = "room1";
        private int playerID = 0;
        private int gameID = 0;

        private string currentSceneIdentifier;

        public Scene CurrentScene
        {
            get { return new DataAccess().findScene(gameID, currentSceneIdentifier);}
        }

        public string CurrentSceneDescription
        {
            get
            {
                Scene s = CurrentScene;
                return s.Description + MultiplayerManager.Instance.getPlayersInScene(s.identifier);
            }
        }
        
        public void startNewGame(int playerDataID)
        {
            DataAccess dataAccess = new DataAccess();
            currentSceneIdentifier = StartScene;
            gameID = dataAccess.createNewGame();
            playerID = dataAccess.addPlayerToGame(playerDataID, gameID);
            Debug.Log(string.Format("New instance id: {0}", playerID));
            currentSceneIdentifier = dataAccess.getPlayerScene(playerID);
            MultiplayerManager.Instance.newSession();
        }

        public bool startOldGame(int playerDataID)
        {
            Tuple<bool, int, int> continueResult;

            DataAccess dataAccess = new DataAccess();
            continueResult = dataAccess.findContinueInstance(playerDataID);
            if(continueResult.Item1)
            {
                gameID = continueResult.Item2;
                playerID = continueResult.Item3;
                currentSceneIdentifier = dataAccess.getPlayerScene(playerID);
                MultiplayerManager.Instance.newSession();
            }
            else
            {
                startNewGame(playerDataID);

            }
            return continueResult.Item1;
        }

        public void endGame()
        {
            MultiplayerManager.Instance.endSession();
            playerID = 0;
            gameID = 0;
        }

        /// <summary>
        /// Move the player through the given exit.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput playerMove(string argument)
        {
            string outputMessage = " "; // String to update display with.
            string systemMessage = "Unexpected Error Encountered"; // String to update the system with.

            FindSceneExitResult searchResult; // Gets result from search for exit.
            Scene newScene = null; // The scene to move to
            DataAccess dataAccess = new DataAccess();

            searchResult = dataAccess.findScene(gameID, currentSceneIdentifier).findExit(argument);
            if (searchResult.Found)
            {
                newScene = dataAccess.findScene(gameID, searchResult.SceneExit.linkedSceneIdentifier);
                if (newScene != null)
                {
                    currentSceneIdentifier = newScene.identifier;
                    dataAccess.setPlayerScene(playerID, newScene.identifier);
                    outputMessage = string.Format("Moved to {0}.", searchResult.SceneExit.fullName);
                    systemMessage = "display update";

                    // Let other players know the move has happened
                    MultiplayerManager.Instance.sendPlayerMove(currentSceneIdentifier);
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

        /// <summary>
        /// Get the given item.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput playerGet(string argument)
        {
            bool success = false;
            string outputMessage = "";
            string systemMessage = "Unexpected Error Encountered";
            DataAccess dataAccess = new DataAccess();
            Scene sceneToChange = dataAccess.findScene(gameID, currentSceneIdentifier);

            FindSceneItemResult searchResult = sceneToChange.findItem(argument);
            if (searchResult.Found)
            {
                if(searchResult.SceneItem.Type == SceneComponent.ComponentType.Key)
                {
                    SceneKey item = (SceneKey)searchResult.SceneItem;
                    dataAccess.removeSceneComponent(gameID, sceneToChange, item);
                    dataAccess.addPlayerInventoryItem(playerID, item);

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

        /// <summary>
        /// Drop the given item.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput playerDrop(string argument)
        {
            bool success = false;
            string outputMessage = "";
            string systemMessage = "Unexpected Error Encountered";
            DataAccess dataAccess = new DataAccess();

            SceneItem[] searchResult = (from item in dataAccess.findPlayerInventory(playerID) where item.fullName.ToLower().Contains(argument.ToLower()) select item).ToArray();
                     
            if (searchResult.Length == 1)
            {
                Scene sceneToChange = dataAccess.findScene(gameID, currentSceneIdentifier);
                dataAccess.addSceneComponent(gameID, sceneToChange, searchResult[0]);
                dataAccess.removePlayerInventoryItem(playerID, searchResult[0]);

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

        /// <summary>
        /// Use the given item.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput playerInteract(string argument)
        {
            bool success = false;
            string outputMessage = " "; // String to update display with.
            string systemMessage = "Unexpected Error Encountered"; // String to update the system with.
            DataAccess dataAccess = new DataAccess();

            FindSceneItemResult searchResult = dataAccess.findScene(gameID, currentSceneIdentifier).findItem(argument);

            if (searchResult.Found)
            {
                if (searchResult.SceneItem.Type == SceneComponent.ComponentType.Door)
                {
                    SceneDoor item = (SceneDoor)searchResult.SceneItem;
                    if (item.haveRequiredItem(dataAccess.findPlayerInventory(playerID)))
                    {
                        Scene sceneToChange = dataAccess.findScene(gameID, currentSceneIdentifier);
                        dataAccess.removeSceneComponent(gameID, sceneToChange, item);
                        dataAccess.addSceneComponent(gameID, sceneToChange, item.exit);
                        dataAccess.removePlayerInventoryItem(playerID, new SceneKey(item.requiredItemIdentifier, "Removed Item"));
                        dataAccess.incrementPlayerScore(playerID);
                        success = true;
                        outputMessage = item.successMessage + " Your score is now " + new DataAccess().getPlayerScore(playerID).ToString();
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

        /// <summary>
        /// Find all items currently in the player's inventory.
        /// </summary>
        /// <returns></returns>
        public SceneItem[] getPlayerInventory()
        {
            return new DataAccess().findPlayerInventory(playerID);
        }
    }
}
