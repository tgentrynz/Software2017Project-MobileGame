using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles view aspects of the game.
/// </summary>
namespace Assets.Scripts {
    public class ViewManager : MonoBehaviour {

        private static ViewManager instance; // Singleton instance
        private GameManager viewModel; // Connection to game data

        private ViewType currentView = ViewType.Menu;
        private ViewType viewBeforeHelp = ViewType.Menu;

        /*bindings for scene information*/
        public Canvas menuCanvas;
        public Canvas helpCanvas;
        public Canvas sceneCanvas;
        public Canvas inventoryCanvas;

        /*bindings for player input*/
        public InputField playerInput;
        public Text outputBoxText;

        public InputField.SubmitEvent inputSubmitEvent;

        public static ViewManager Instance
        {
            get { return instance; }
        }

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

        private void initialise()
        {
            // Set up references
            instance = this; // Set singleton

            // Move all canvases into camera
            menuCanvas.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            menuCanvas.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 128);

            helpCanvas.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            helpCanvas.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 128);

            sceneCanvas.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            sceneCanvas.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 128);

            inventoryCanvas.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            inventoryCanvas.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 128);
            
            // Show menu canvas on start
            disableViews();
            enableView(ViewType.Menu);

            // Create events to handle user input
            inputSubmitEvent = new InputField.SubmitEvent();
            inputSubmitEvent.AddListener(handleInput);

            // Apply events to GUI components
            playerInput.onEndEdit = inputSubmitEvent;
        }

        private void handleInput(string arg)
        {
            // Turn input string into system input
            Command command = InputManager.checkInput(playerInput.text);

            if (command.Valid)
            {
                CommandOutput output = command.act();
                outputBoxText.text = output.Message;
                if (!output.Success)
                    Debug.Log(output.SystemMessage);

                if (output.SystemMessage == "display update")
                    if (currentView == ViewType.Scene)
                        updateSceneInformation(viewModel.CurrentScene);
                    else if (currentView == ViewType.Inventory)
                        updateInventoryInformation(ViewModel.getPlayerInventory());
            }
            else
            {
                outputBoxText.text = "Unrecognised command.";
            }

            // Ready textbox for next input
            playerInput.text = "";
            playerInput.ActivateInputField();
        }

        public void updateOuputMessage(string message)
        {
            outputBoxText.text = message;
        }

        public bool checkGameDataNeedsUpdating()
        {
            bool output = false;
            if(currentView == ViewType.Inventory || currentView == ViewType.Scene)
            {
                output = true;
            }
            return output;
        }

        public void updateSceneInformation(Scene scene)
        {
            if (scene != null)
            {
                getSceneDescription().text = scene.Description;
                getSceneBackground().sprite = loadBackgroundImage(scene.background);
            }
        }

        public void updateInventoryInformation(SceneItem[] inventory)
        {
            if (inventory != null)
            {
                string output = "";
                foreach(SceneItem item in inventory)
                {
                    output = string.Format("{0}{1}\n", output, item.fullName);
                    Debug.Log(item.fullName);
                }
                getInventoryDescription().text = output;
                // Scene Background = getSceneImage
            }
        }

        private void disableViews()
        {
            menuCanvas.gameObject.SetActive(false);
            helpCanvas.gameObject.SetActive(false);
            sceneCanvas.gameObject.SetActive(false);
            inventoryCanvas.gameObject.SetActive(false);
        }

        public CommandOutput startGame(string argument)
        {
            CommandOutput output;
            if (currentView == ViewType.Menu)
            {
                viewModel.initialise();
                enableView(ViewType.Scene);
                output = new CommandOutput(true, "Game Started.", "display update");
            }
            else
                output = new CommandOutput(false, "Game already started.", "Could not make new game as one is already in progress.");
            return output;
        }

        public CommandOutput exitGame(string argument)
        {
            CommandOutput output;
            if(currentView == ViewType.Menu)
            {
                output = new CommandOutput(true, "Closing Game", "");
                Debug.Log("Application close request recieved.");
                Application.Quit();
            }
            else if(currentView == ViewType.Scene || currentView == ViewType.Inventory)
            {
                enableView(ViewType.Menu);
                output = new CommandOutput(true, "Returned to menu.", "The game was ended.");
            }
            else
            {
                output = new CommandOutput(false, "Did you mean \"close\"", "Can't exit from the help view.");
            }
            return output;
        }

        public CommandOutput changeGameView(string argument)
        {
            CommandOutput output;
            if (argument.Equals("inventory")||argument.Equals("items"))
                output = enableView(ViewType.Inventory);
            else if (argument.Equals("scene")||argument.Equals("room"))
                output = enableView(ViewType.Scene);
            else
                output = new CommandOutput(false, string.Format("{0} is unknown.", argument), "Could not change view.");

            return output;
        }

        public CommandOutput openHelp(string argument)
        {
            CommandOutput output;
            if (currentView != ViewType.Help)
            {
                viewBeforeHelp = currentView;
                output = enableView(ViewType.Help);
            }
            else
                output = new CommandOutput(false, "Type \"close\" to return.", "Failed trying to access help from help.");
            return output;
        }

        public CommandOutput closeHelp(string argument)
        {
            CommandOutput output;
            if (currentView == ViewType.Help)
                output = enableView(viewBeforeHelp);
            else
                output = new CommandOutput(false, "Did you mean \"exit\"?", "Failed trying to close help from outside of help.");
            return output;
        }

        private CommandOutput enableView(ViewType view)
        {
            // GameObject to hold the view being switched to
            GameObject newView = null;
            // Output variables
            bool sceneChanged = false;
            string message = "";
            string systemMessage = "";

            switch (view)
            {
                case ViewType.Menu:
                    newView = menuCanvas.gameObject;
                    message = "Main Menu";
                    break;
                case ViewType.Help:
                    newView = helpCanvas.gameObject;
                    message = "Type \"close\" to return.";
                    break;
                case ViewType.Scene:
                    newView = sceneCanvas.gameObject;
                    message = "Viewing scene.";

                    updateSceneInformation(viewModel.CurrentScene);
                    break;
                case ViewType.Inventory:
                    newView = inventoryCanvas.gameObject;
                    message = "Opened inventory.";

                    updateInventoryInformation(viewModel.getPlayerInventory());
                    break;
                default:
                    sceneChanged = false;
                    message = "Unknown view.";
                    systemMessage = "Could not change view.";
                    break;
            }

            if (newView != null)
            {
                disableViews();
                newView.SetActive(true);
                currentView = view;

                sceneChanged = true;
                systemMessage = "Changed View";
            }
            return new CommandOutput(sceneChanged, message, systemMessage);
            
        }

        private Text getSceneDescription()
        {
            return sceneCanvas.transform.Find("SceneDescription").GetComponent<Text>();
        }

        private Image getSceneBackground()
        {
            return sceneCanvas.transform.Find("SceneBackground").GetComponent<Image>();
        }

        private Text getInventoryDescription()
        {
            return inventoryCanvas.transform.Find("InventoryDescription").GetComponent<Text>();
        }

        private Sprite loadBackgroundImage(string imageName)
        {
            Sprite output;
            output = Resources.Load<Sprite>(imageName);
            if (output == null) // If the system could not find the specified image, use a default one.
                output = Resources.Load<Sprite>("testPic");
            return output;
        }

        private enum ViewType
        {
            Null,
            Menu,
            Help,
            Scene,
            Inventory
        }
    }
}
