using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using SQLite4Unity3d;
using Assets.Scripts.DTO;
using Assets.Scripts.DomainClasses;

namespace Assets.Scripts
{
    /// <summary>
    /// Access to the game's data.
    /// </summary>
    public class DataAccess
    {
        // The connection to the database.
        private SQLiteConnection connection;

        public DataAccess()
        {
            string DatabaseName = "game.db";
            bool preExistingData;
#if UNITY_EDITOR
            var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID 
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif
            // Check database exists beforehand, so game data can be created if it's absent
            preExistingData = File.Exists(dbPath);
            // Create a connection
            connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            Debug.Log("Final PATH: " + dbPath);
            // If there was no pre-existing database, create initial game data
            if (!preExistingData)
            {
                createDatabaseSchema();
                createStoryData();
                Debug.Log("created new database");
            }

        }

        /// <summary>
        /// Finds the scene with the given ID
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public DomainClasses.Scene findScene(int gameID, string identifier)
        {
            return new DomainClasses.Scene(connection.Table<DTO.Scene>().Where(x => x.identifier == identifier).FirstOrDefault(), findSceneComponents(gameID, identifier));
        }

        /// <summary>
        /// Gets the components in a scene.
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="sceneIdentifier"></param>
        /// <returns></returns>
        private DomainClasses.SceneComponent[] findSceneComponents(int gameID, string sceneIdentifier)
        {
            List<DomainClasses.SceneComponent> output = new List<DomainClasses.SceneComponent>();
            // Get identifier for all components placed in scene
            string[] componentIDs = (from c in (connection.Table<DTO.SceneInventory>().Where(x => x.gameID == gameID && x.sceneID == sceneIdentifier).ToArray()) select c.componentID).ToArray();
            // Get entities from those component ids
            DTO.SceneComponent[] components = connection.Table<DTO.SceneComponent>().Where(x => componentIDs.Contains<string>(x.identifier)).ToArray();
            // Turn data models into game objects
            foreach (DTO.SceneComponent c in components)
            {
                switch (c.type)
                {
                    case "key":
                        output.Add(new SceneKey(c));
                        break;
                    case "exit":
                        output.Add(loadExitComponent(c));
                        break;
                    case "door":
                        output.Add(loadDoorComponent(c));
                        break;
                    default:
                        break;
                }
            }
            return output.ToArray();
        }
        /// <summary>
        /// Maps an exit component from the database to the game system.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private DomainClasses.SceneExit loadExitComponent(DTO.SceneComponent component)
        {
            DTO.SceneExit exit = connection.Table<DTO.SceneExit>().Where(x => x.identifier == component.identifier).FirstOrDefault();
            return new DomainClasses.SceneExit(component, exit);
        }
        /// <summary>
        /// Maps a door component from the database to the game system.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private DomainClasses.SceneDoor loadDoorComponent(DTO.SceneComponent component)
        {
            DTO.SceneDoor door = connection.Table<DTO.SceneDoor>().Where(x => x.identifier == component.identifier).FirstOrDefault();
            DTO.SceneComponent exitComponent = connection.Table<DTO.SceneComponent>().Where(x => x.identifier == door.exitID).FirstOrDefault();
            return new DomainClasses.SceneDoor(component, door, loadExitComponent(exitComponent));
        }

        /// <summary>
        /// Adds a scene component to a scene's inventory in the database.
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="scene"></param>
        /// <param name="item"></param>
        public void addSceneComponent(int gameID, DomainClasses.Scene scene, DomainClasses.SceneComponent item)
        {
            connection.Insert(new DTO.SceneInventory { gameID = gameID, sceneID = scene.identifier, componentID = item.identifier });
        }

        /// <summary>
        /// Removes a scene component from a scene's inventory in the database.
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="scene"></param>
        /// <param name="item"></param>
        public void removeSceneComponent(int gameID, DomainClasses.Scene scene, DomainClasses.SceneComponent item)
        {
            DTO.SceneInventory itemToRemove = connection.Table<DTO.SceneInventory>().Where(x => x.gameID == gameID && x.sceneID == scene.identifier && x.componentID == item.identifier).ToArray().FirstOrDefault();
            if (itemToRemove != null)
            {
                connection.Delete<SceneInventory>(itemToRemove.identifier);
            }
        }

        /// <summary>
        /// Find's all the items in the given player's inventory in the database.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public SceneItem[] findPlayerInventory(int playerID)
        {
            List<SceneItem> output = new List<SceneItem>();
            // Get identifier for all components placed in scene
            string[] componentIDs = (from c in (connection.Table<PlayerInventory>().Where(x => x.playerID == playerID).ToArray()) select c.componentID).ToArray();
            // Get entities from those component ids
            DTO.SceneComponent[] components = connection.Table<DTO.SceneComponent>().Where(x => componentIDs.Contains<string>(x.identifier)).ToArray();
            foreach(DTO.SceneComponent c in components)
            {
                if (c.type == "key")
                    output.Add(new SceneKey(c));
            }
            return output.ToArray();
        }

        /// <summary>
        /// Adds an item to a player's inventory in the database.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="item"></param>
        public void addPlayerInventoryItem(int playerID, DomainClasses.SceneComponent item)
        {
            connection.Insert(new PlayerInventory { playerID = playerID, componentID = item.identifier });
            connection.Close();
        }

        /// <summary>
        /// Removes an item from the player's inventory in the database.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="item"></param>
        public void removePlayerInventoryItem(int playerID, DomainClasses.SceneComponent item)
        {
            PlayerInventory itemToRemove = connection.Table<PlayerInventory>().Where(x => x.playerID == playerID && x.componentID == item.identifier).ToArray().FirstOrDefault();
            if (itemToRemove != null)
            {
                connection.Delete<PlayerInventory>(itemToRemove.identifier);
            }
        }

        /// <summary>
        /// Creates all the tables in the database.
        /// </summary>
        private void createDatabaseSchema()
        {
            connection.CreateTable<DTO.GameInstance>();
            connection.CreateTable<DTO.PlayerData>();
            connection.CreateTable<DTO.PlayerInstance>();
            connection.CreateTable<DTO.PlayerInventory>();
            connection.CreateTable<DTO.Scene>();
            connection.CreateTable<DTO.SceneComponent>();
            connection.CreateTable<DTO.SceneDoor>();
            connection.CreateTable<DTO.SceneExit>();
            connection.CreateTable<DTO.SceneInventory>();
            connection.CreateTable<DTO.Story>();
        }

        /// <summary>
        /// Creates all the initial game data when creating a new database.
        /// </summary>
        private void createStoryData()
        {
            /* Subprocedures to enter entities into database */
            // Will insert data into table related to story data
            Action<string, string, string, string> createStory = (identifier, title, description, openingScene) => connection.Insert(new DTO.Story { identifier = identifier, title = title, description = description, openingScene = openingScene });
            // Will insert data into tables related to scene data
            Action<string, string, string, string> createScene =
                (identifier, storyID, description, background) =>
                    connection.Insert(
                        new DTO.Scene
                        {
                            identifier = identifier,
                            storyID = storyID,
                            description = description,
                            background = background
                        });
            // Will insert data into tables related to key item data
            Action<string, string, string> createKey =
                (identifier, fullName, initialSceneID) =>
                    connection.Insert(
                        new DTO.SceneComponent {
                            identifier = identifier,
                            fullName = fullName,
                            initialSceneID = initialSceneID,
                            type = "key"
                        });
            // Will insert data into tables related to exit data
            Action<string, string, string, string> createExit =
                (identifier, fullName, initialScenID, linkedScene) =>
                    connection.InsertAll(
                        new object[] {
                            new DTO.SceneComponent {
                                identifier = identifier,
                                fullName = fullName,
                                initialSceneID = initialScenID,
                                type = "exit"
                            },
                            new DTO.SceneExit {
                                identifier = identifier,
                                linkedScene = linkedScene
                            },
                        });
            // Will insert data into tables related to door data
            Action<string, string, string, string, string, string, string, string, string> createDoor =
                (identifier, fullName, initialSceneID, keyIdentifier, exitIdentifier, exitFullName, linkedScene, messageSuccess, messageFail) =>
                connection.InsertAll(
                        new object[]
                        {
                            new DTO.SceneComponent
                            {
                                identifier = identifier,
                                fullName = fullName,
                                initialSceneID = initialSceneID,
                                type = "door"
                            },
                            new DTO.SceneDoor
                            {
                                identifier = identifier,
                                keyID = keyIdentifier,
                                exitID = exitIdentifier,
                                messageSuccess = messageSuccess,
                                messageFail = messageFail
                            },
                            new DTO.SceneComponent
                            {
                                identifier = exitIdentifier,
                                fullName = exitFullName,
                                type = "exit"
                            },
                            new DTO.SceneExit
                            {
                                identifier = exitIdentifier,
                                linkedScene = linkedScene
                            }
                        });

            /* Story creation */
            createStory("test_story", "Test Story", "A story for testing the game.", "room1");

            createScene("room1", "test_story", "You are in a small room.", "bg_01");
            createExit("exit_room1ToRoom2", "Side Room", "room1", "room2");
            createDoor("door_room1DoorToRoom3", "Broken Lever", "room1", "key_room2LeverHandle", "exit_room1DoorToRoom3", "Secret Room", "room3", "A secret door opened up.", "It is missing its handle.");
            createDoor("door_room1DoorToRoom4", "Solid Door", "room1", "key_room3DoorKey", "exit_room1DoorToRoom4", "Unlocked Door", "room4", "The door unlocked.", "This door needs a key.");

            createScene("room2", "test_story", "You are now in a smaller room.", "bg_02");
            createExit("exit_room2ToRoom1", "The First Room", "room2", "room1");
            createKey("key_room2LeverHandle", "Lever Handle", "room2");

            createScene("room3", "test_story", "You are in the secret room.", "bg_03");
            createExit("exit_room3ToRoom1", "The First Room", "room3", "room1");
            createKey("key_room3DoorKey", "Rusty Key", "room3");

            createScene("room4", "test_story", "Congratulations, this is the winners' room.", "bg_04");
        }

        /// <summary>
        /// Starts a new game isntance.
        /// </summary>
        /// <returns></returns>
        public int createNewGame()
        {
            int newGameID;
            string storyID = "test_story"; // Hardcoded in this implementation
            // Create the new game
            connection.Insert(new DTO.GameInstance { storyID = storyID });
            // Get the game's id
            newGameID = (from g in connection.Table<DTO.GameInstance>().ToArray() select g.identifier).Max();
            // Initialise the game
            // Get id of every scene in story
            string[] sceneIDs = (from s in connection.Table<DTO.Scene>().Where(x => x.storyID == storyID).ToArray() select s.identifier).ToArray();
            foreach (string s in sceneIDs) { Debug.Log(s); }
            foreach (DTO.SceneComponent c in connection.Table<DTO.SceneComponent>().Where(x => sceneIDs.Contains(x.initialSceneID)).ToArray())
            {
                connection.Insert(new DTO.SceneInventory { gameID = newGameID, sceneID = c.initialSceneID, componentID = c.identifier });
            }
            Debug.Log(String.Format("Created game {0}", newGameID));
            return newGameID;
        }

        /// <summary>
        /// Creates a player instance in a game instance.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public int addPlayerToGame(int playerID, int gameID)
        {
            // Find the first scene of the story
            DTO.GameInstance game = connection.Table<DTO.GameInstance>().Where(x => x.identifier == gameID).FirstOrDefault();
            string story = game.storyID;
            string scene = connection.Table<DTO.Story>().Where(x => x.identifier == story).FirstOrDefault().openingScene;
            // Holds the player's current data
            DTO.PlayerData playerData;

            // Create new player instance
            connection.Insert(new DTO.PlayerInstance { gameID = gameID, playerID = playerID, scene = scene});

            // Update the player's continue value
            playerData = connection.Table<DTO.PlayerData>().Where(x => x.identifier == playerID).FirstOrDefault();
            Debug.Log(String.Format("Player {0 }exists? {1}", playerID, playerData != null));
            playerData.savedGameID = gameID;
            connection.Update(playerData);

            // Return the new instance's id
            return (from p in connection.Table<DTO.PlayerInstance>().ToArray() select p.identifier).Max();
        }

        /// <summary>
        /// Finds the scene the given player is currently in.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public string getPlayerScene(int playerID)
        {
            DTO.PlayerInstance player = connection.Table<DTO.PlayerInstance>().Where(x => x.identifier == playerID).FirstOrDefault();
            return player.scene;
        }

        /// <summary>
        /// Moves the given player to the given scene.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="newScene"></param>
        public void setPlayerScene(int playerID, string newScene)
        {
            DTO.PlayerInstance playerToUpdate = connection.Table<DTO.PlayerInstance>().Where(x => x.identifier == playerID).FirstOrDefault();
            playerToUpdate.scene = newScene;
            connection.Update(playerToUpdate);
        }

        /// <summary>
        /// Adds to the player's score.
        /// </summary>
        /// <param name="playerID"></param>
        public void incrementPlayerScore(int playerID)
        {
            DTO.PlayerInstance playerToUpdate = connection.Table<DTO.PlayerInstance>().Where(x => x.identifier == playerID).FirstOrDefault();
            playerToUpdate.score = playerToUpdate.score + 1;
            connection.Update(playerToUpdate);
        }

        public int getPlayerScore(int playerID)
        {
            int output = -1;
            DTO.PlayerInstance playerToRetrieve = connection.Table<DTO.PlayerInstance>().Where(x => x.identifier == playerID).FirstOrDefault();
            if (playerToRetrieve != null)
                output = playerToRetrieve.score;
            return output;
        }

        /// <summary>
        /// Allows the system to store a player's id in the local database so the can save their game locally
        /// </summary>
        /// <param name="identifier"></param>
        public void registerPlayerLocally(int identifier)
        {
            if(connection.Table<DTO.PlayerData>().Where(x => x.identifier == identifier).FirstOrDefault() == null)
            {
                connection.Insert(new PlayerData() { identifier = identifier });
            }
        }
        
        public Tuple<bool, int, int> findContinueInstance(int playerDataID)
        {
            int gameInstanceID = (from i in connection.Table<DTO.PlayerData>().Where(x => x.identifier == playerDataID).ToArray() select i.savedGameID).FirstOrDefault();
            int playerInstanceID = (from i in connection.Table<DTO.PlayerInstance>().Where(x => x.gameID == gameInstanceID && x.playerID == playerDataID).ToArray() select i.identifier).FirstOrDefault();
            Debug.Log(gameInstanceID + " : " + playerInstanceID);
            return Tuple.Create<bool, int, int>((gameInstanceID != 0), gameInstanceID, playerInstanceID);
        }

    }
}
