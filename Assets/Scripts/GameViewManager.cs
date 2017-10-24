using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.DomainClasses;
/// <summary>
/// Handles view aspects of the game.
/// </summary>
namespace Assets.Scripts
{
    public class GameViewManager : MonoBehaviour
    {

        private static GameViewManager instance; // Singleton instance
        private GameManager viewModel; // Connection to game data
        /// <summary>
        /// The id of the player account logged in
        /// </summary>
        public int playerID;

        public static GameViewManager Instance
        {
            get { return instance; }
        }
        /// <summary>
        /// The game manager created to control the game.
        /// </summary>
        public GameManager ViewModel
        {
            get
            {
                if (viewModel == null)
                    viewModel = new GameManager();
                return viewModel;
            }
        }

        // Use this for initialization
        void Start()
        {
            //This can be a singleton.
            if (instance == null)
                initialise();
            else
                Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Set the initial state of the instance.
        /// </summary>
        private void initialise()
        {
            // Set up references
            instance = this; // Set singleton
        }

        /// <summary>
        /// Checks if the view is in a state where it would need to refresh the display with up-to-date data.
        /// </summary>
        /// <returns></returns>
        public bool checkGameDataNeedsUpdating()
        {
            // This is intended for the unimplementd multiplayer game mode, so it's implementation isn't important
            bool output = false;
            if (ViewManager.Instance.currentView == ViewManager.ViewType.Inventory || ViewManager.Instance.currentView == ViewManager.ViewType.Scene)
            {
                output = true;
            }
            return output;
        }

        /// <summary>
        /// Refreshes the display with up-to-date data
        /// </summary>
        public void updateSceneInformation()
        {
            getSceneDescription().text = viewModel.CurrentScene.Description;
            getSceneBackground().sprite = loadBackgroundImage(viewModel.CurrentScene.background);
        }

        /// <summary>
        /// Refreshes the display with up-to-date data
        /// </summary>
        public void updateInventoryInformation()
        {
            SceneItem[] inventory = viewModel.getPlayerInventory();
            string output = "";
            foreach (SceneItem item in inventory)
            {
                output = string.Format("{0}{1}\n", output, item.fullName);
            }
            getInventoryDescription().text = output;
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput startGame(string argument)
        {
            CommandOutput output;
            if (ViewManager.Instance.currentView == ViewManager.ViewType.Menu)
            {
                viewModel.startNewGame(playerID);
                ViewManager.Instance.enableView(ViewManager.ViewType.Scene);
                output = new CommandOutput(true, "Game Started.", "display update");
            }
            else
                output = new CommandOutput(false, "Game already started.", "Could not make new game as one is already in progress.");
            return output;
        }

        /// <summary>
        /// Continues the player's last played game.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput continueGame(string argument)
        {
            CommandOutput output;
            bool canContinue;
            if (ViewManager.Instance.currentView == ViewManager.ViewType.Menu)
            {
                canContinue = viewModel.startOldGame(playerID);
                ViewManager.Instance.enableView(ViewManager.ViewType.Scene);
                if (canContinue)
                    output = new CommandOutput(true, "Loaded last game.", "display update");
                else
                    output = new CommandOutput(true, "No game to load, started new.", "display update");

            }
            else
                output = new CommandOutput(false, "Game already started.", "Could not make new game as one is already in progress.");
            return output;
        }

        /// <summary>
        /// Find the description text user interface element.
        /// </summary>
        /// <returns></returns>
        private Text getSceneDescription()
        {
            return ViewManager.Instance.sceneCanvas.transform.Find("SceneDescription").GetComponent<Text>();
        }

        /// <summary>
        /// Find the background image user interface element.
        /// </summary>
        /// <returns></returns>
        private Image getSceneBackground()
        {
            return ViewManager.Instance.sceneCanvas.transform.Find("SceneBackground").GetComponent<Image>();
        }

        /// <summary>
        /// Find the inventory text user interface element.
        /// </summary>
        /// <returns></returns>
        private Text getInventoryDescription()
        {
            return ViewManager.Instance.inventoryCanvas.transform.Find("InventoryDescription").GetComponent<Text>();
        }

        /// <summary>
        /// Gets a background image from the game's resource folder.
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private Sprite loadBackgroundImage(string imageName)
        {
            Sprite output;
            output = Resources.Load<Sprite>(imageName);
            if (output == null) // If the system could not find the specified image, use a default one.
                output = Resources.Load<Sprite>("testPic");
            return output;
        }
    }
}
