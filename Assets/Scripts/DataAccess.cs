using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Access to the game's data.
    /// </summary>
    public class DataAccess
    {
        /*
         * This implementatation behind this classes' interface is for testing only.
         * It will need to be replaced once database and multi-user functionality is added to the system
        */
        private List<Scene> sceneList; // Part of test implementation
        private List<SceneItem> inventory; // Part of test implementation

        public DataAccess()
        {
            // Test implementation
            createTestData();
        }

        public Scene findScene(string identifier)
        {
            //Test implementation
            return (from s in sceneList where s.identifier == identifier select s).ToList().FirstOrDefault();
        }

        public SceneItem[] findPlayerInventory(string playerID)
        {
            //Test implementation
            return inventory.ToArray();
        }

        public void addPlayerInventoryItem(SceneItem item)
        {
            inventory.Add(item);
        }

        public void removePlayerInventoryItem(string identifier)
        {
            foreach(SceneItem item in inventory)
            {
                if(item.identifier == identifier)
                {
                    inventory.Remove(item);
                    break;
                }
            }
        }

        public void commitSceneChange(Scene scene)
        {
            // Unimplemented
        }
        public void commitInventoryChange()
        {
            // Unimplemented
        }

        private void createTestData() // Part of test implementation
        {
            sceneList = new List<Scene>();
            inventory = new List<SceneItem>();
            Scene s;

            s = new Scene("room1", "You are in a small room.", "bg_01");
            s.addExit("exit_room1ToRoom2", "Side Room", "room2");
            s.addDoorItem("door_room1DoorToRoom3", "Broken Lever", "key_room2LeverHandle", "A secret door opened up.", "It is missing its handle.", "room3", "exit_room1DoorToRoom3", "Secret Room");
            s.addDoorItem("door_room1DoorToRoom4", "Solid Door", "key_room3DoorKey", "The door unlocked.", "This door needs a key.", "room4", "exit_room1DoorToRoom4", "Unlocked Door");
            sceneList.Add(s);

            s = new Scene("room2", "You are now in a smaller room.", "bg_02");
            s.addExit("exit_room2ToRoom1", "The First Room", "room1");
            s.addKeyItem("key_room2LeverHandle", "Lever Handle");
            sceneList.Add(s);

            s = new Scene("room3", "You are in the secret room.", "bg_03");
            s.addExit("exit_room3ToRoom1", "The First Room", "room1");
            s.addKeyItem("key_room3DoorKey", "Rusty Key");
            sceneList.Add(s);

            s = new Scene("room4", "Congratulations, this is the winners' room.", "bg_04");
            sceneList.Add(s);
        }
    }
}
