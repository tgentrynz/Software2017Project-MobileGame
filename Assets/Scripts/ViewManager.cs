using System;
using System.IO;
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

        public ViewType currentView { get; private set; }
        public ViewType viewBeforeHelp { get; private set; }

        /*bindings for scene information*/
        public Canvas loginCanvas;
        public Canvas menuCanvas;
        public Canvas helpCanvas;
        public Canvas sceneCanvas;
        public Canvas inventoryCanvas;
        private Canvas[] AllCanvases
        {
            get
            {
                return new Canvas[] { loginCanvas, menuCanvas, helpCanvas, sceneCanvas, inventoryCanvas};
            }
        }

        /*bindings for player input*/
        public InputField playerInput;
        public Text outputBoxText;

        public InputField.SubmitEvent inputSubmitEvent;

        public static ViewManager Instance
        {
            get { return instance; }
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

        // Ends the game
        public void endGame()
        {
            // If the game is just being tested, do not keep any game data.
            Debug.Log("Application close request recieved.");
            Application.Quit();
        }

        private void initialise()
        {
            // Set up references
            instance = this; // Set singleton

            currentView = ViewType.Login;
            viewBeforeHelp = ViewType.Login;

            // Move all canvases into camera
            foreach (Canvas c in AllCanvases)
            {
                c.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                c.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 128);
            }
            
            // Show menu canvas on start
            disableViews();
            enableView(currentView);

            // Create events to handle user input
            inputSubmitEvent = new InputField.SubmitEvent();
            inputSubmitEvent.AddListener(handleInput);

            // Apply events to GUI components
            playerInput.onEndEdit = inputSubmitEvent;

            // Hide text input until player logs in
            playerInput.gameObject.SetActive(false);
            outputBoxText.transform.parent.gameObject.SetActive(false);

            //Change resolution
            Screen.SetResolution(320, 480, false);
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
                        GameViewManager.Instance.updateSceneInformation();
                    else if (currentView == ViewType.Inventory)
                        GameViewManager.Instance.updateInventoryInformation();
            }
            else
            {
                outputBoxText.text = "Unrecognised command.";
            }

            // Ready textbox for next input
            playerInput.text = "";
#if !UNITY_ANDROID
            /*
            We only want to reselect the input field if the control scheme is keyboard e.g. the desktop versions
            On the android version, touch controls mean that reselecting the input field after every command
            will hide the game display behind the android keyboard at all times. Android users will probably
            be more confortable pressing the input field every time they want to enter a command, since they're
            already tapping on the screen to type. Desktop users would have to move their hand to the mouse if
            the input field was not reselected, so for them it's preferable to have it reselected after every
            command.
            */
            playerInput.ActivateInputField();
#endif
        }

        /// <summary>
        /// Update the text in the output text box.
        /// </summary>
        /// <param name="message"></param>
        public void updateOuputMessage(string message)
        {
            outputBoxText.text = message;
        }

        /// <summary>
        /// Hide all the UI screens.
        /// </summary>
        private void disableViews()
        {
            foreach (Canvas c in AllCanvases)
            {
                c.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Returns to the menu from the game.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput exitGame(string argument)
        {
            CommandOutput output;
            if(currentView == ViewType.Menu)
            {
                output = new CommandOutput(true, "Returning to menu.", "");
                LoginManager.logout(GameViewManager.Instance.playerID);
                enableView(ViewType.Login);
                
            }
            else if(currentView == ViewType.Scene || currentView == ViewType.Inventory)
            {
                enableView(ViewType.Menu);
                GameViewManager.Instance.ViewModel.endGame();
                output = new CommandOutput(true, "Returned to menu.", "The game was ended.");
            }
            else
            {
                output = new CommandOutput(false, "Did you mean \"close\"", "Can't exit from the help view.");
            }
            return output;
        }

        /// <summary>
        /// Changes the active view in the game.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Opens the help screen.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Closes the help screen and returns to the previous screen.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public CommandOutput closeHelp(string argument)
        {
            CommandOutput output;
            if (currentView == ViewType.Help)
                output = enableView(viewBeforeHelp);
            else
                output = new CommandOutput(false, "Did you mean \"exit\"?", "Failed trying to close help from outside of help.");
            return output;
        }

        // Method to allow a LoginManager to move to the actual game after handling user accounts
        public void loginSuccess(int playerID)
        {
            Debug.Log(string.Format("User with ID {0} has logged in.", playerID));
            // Reset the ViewManager's state
            initialise();
            // Move to game's menu screen
            enableView(ViewType.Menu);
            // Make sure user input is visible
            playerInput.gameObject.SetActive(true);
            outputBoxText.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Enables the given screen.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public CommandOutput enableView(ViewType view)
        {
            // GameObject to hold the view being switched to
            GameObject newView = null;
            // Output variables
            bool sceneChanged = false;
            string message = "";
            string systemMessage = "";

            switch (view)
            {
                case ViewType.Login:
                    newView = loginCanvas.gameObject;
                    LoginViewManager.Instance.setMenuStage(LoginViewManager.MenuStage.menu);
                    // Hide text input until player logs in
                    playerInput.gameObject.SetActive(false);
                    outputBoxText.transform.parent.gameObject.SetActive(false);

                    message = "Login Screen";
                    break;
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

                    GameViewManager.Instance.updateSceneInformation();
                    break;
                case ViewType.Inventory:
                    newView = inventoryCanvas.gameObject;
                    message = "Opened inventory.";

                    GameViewManager.Instance.updateInventoryInformation();
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

        public enum ViewType
        {
            Null,
            Login,
            Menu,
            Help,
            Scene,
            Inventory
        }
    }
}
