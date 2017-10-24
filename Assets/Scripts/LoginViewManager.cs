using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ActionResult;

namespace Assets.Scripts{
    /// <summary>
    /// Handles the login user interface.
    /// </summary>
    public class LoginViewManager : MonoBehaviour {
        public static LoginViewManager instance;
        public static LoginViewManager Instance
        {
            get { return instance; }
        }
        /*Unity UI elements*/

        // Menu Buttons
        public Button menuButtonLogin;
        public Button menuButtonCreate;
        public Button returnButton;
        // Text entry elements
        public InputField textBoxUsername;
        public InputField textBoxPassword;
        // Submit login attempt button
        public Button loginButton;
        // Submit account button
        public Button createButton;

        // Canvases
        public Canvas menuButtonCanvas;
        public Canvas textEntryCanvas;
        public Canvas loginSubmitCanvas;
        public Canvas createSubmitCanvas;

        // Output text
        public Text outputText;

        /*End Unity elements*/

        private MenuStage currentStage;

        private Canvas[] AllCanvases
        {
            get
            {
                return new Canvas[] { menuButtonCanvas, textEntryCanvas, loginSubmitCanvas, createSubmitCanvas};
            }
        }

        // Use this for initialization
       void Start()
        {
            if (instance == null)
            {
                instance = this;
                initialise();
            }
            else
                Destroy(gameObject);
        }
        private void initialise () {
            Button.ButtonClickedEvent buttonPress; // Will holds button press events

            // Reposition all canvases into correct position
            foreach (Canvas c in AllCanvases)
            {
                c.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(-15, -218);
                c.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(15, 0);
                c.gameObject.SetActive(false);
            }
            setMenuStage(MenuStage.menu);

            //Events to handle button presses
            // Menu login button
            buttonPress = new Button.ButtonClickedEvent();
            buttonPress.AddListener(menuButtonLoginPress);
            menuButtonLogin.onClick = buttonPress;

            // Menu create button
            buttonPress = new Button.ButtonClickedEvent();
            buttonPress.AddListener(menuButtonCreatePress);
            menuButtonCreate.onClick = buttonPress;

            // Return button
            buttonPress = new Button.ButtonClickedEvent();
            buttonPress.AddListener(returnButtonPress);
            returnButton.onClick = buttonPress;

            // Submenu login button
            buttonPress = new Button.ButtonClickedEvent();
            buttonPress.AddListener(loginButtonPress);
            loginButton.onClick = buttonPress;

            // Submenu create button
            buttonPress = new Button.ButtonClickedEvent();
            buttonPress.AddListener(createButtonPress);
            createButton.onClick = buttonPress;

        }
	
	    // Update is called once per frame
	    void Update () {
		
	    }

        private void menuButtonLoginPress()
        {
            setMenuStage(MenuStage.login);
        }
    
        private void menuButtonCreatePress()
        {
            setMenuStage(MenuStage.create);
        }

        private void returnButtonPress()
        {
            switch (currentStage)
            {
                case MenuStage.menu:
                    ViewManager.Instance.endGame();
                    break;
                default:
                    setMenuStage(MenuStage.menu);
                    break;
            }
        }

        private void loginButtonPress()
        {
            LoginResult loginResult = LoginManager.login(textBoxUsername.text, textBoxPassword.text);
            if (loginResult.success)
                ViewManager.Instance.loginSuccess(loginResult.playerID);
            else
            {
                outputText.color = new Color(1f, 0.2f, 0.2f);
                outputText.text = loginResult.message;
            }
        }

        private void createButtonPress()
        {
            LoginResult loginResult = LoginManager.create(textBoxUsername.text, textBoxPassword.text);
            if (loginResult.success)
                ViewManager.Instance.loginSuccess(loginResult.playerID);
            else
            {
                outputText.color = new Color(1f, 0.2f, 0.2f);
                outputText.text = loginResult.message;
            }
        }

        private void disableElements()
        {
            foreach(Canvas c in AllCanvases)
            {
                c.gameObject.SetActive(false);
            }
        }

        public void setMenuStage(MenuStage stageToSet)
        {
            disableElements();
            outputText.color = new Color(0.2f, 0.2f, 0.2f);
            switch (stageToSet)
            {
                case MenuStage.menu:
                    outputText.text = "What would you like to do?";
                    menuButtonCanvas.gameObject.SetActive(true);
                    break;
                case MenuStage.login:
                    outputText.text = "Enter your account details.";
                    textEntryCanvas.gameObject.SetActive(true);
                    loginSubmitCanvas.gameObject.SetActive(true);
                    textBoxUsername.text = "";
                    textBoxPassword.text = "";
                    break;
                case MenuStage.create:
                    outputText.text = "Enter your desired account details.";
                    textEntryCanvas.gameObject.SetActive(true);
                    createSubmitCanvas.gameObject.SetActive(true);
                    textBoxUsername.text = "";
                    textBoxPassword.text = "";
                    break;
            }
            currentStage = stageToSet;
        }

        public enum MenuStage
        {
            menu,
            login,
            create
        }
    }
}
