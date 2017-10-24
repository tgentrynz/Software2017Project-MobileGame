using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.ActionResult;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles login and account creation behaviour.
    /// </summary>
    public static class LoginManager
    {
        public static LoginResult login(string username, string password)
        {
            LoginResult output;
            DataAccess dataAccess = new DataAccess();
            Tuple<int, bool> loginAction = dataAccess.playerLogin(username, password);
            if (loginAction.Item1 != -1)
            {
                if (loginAction.Item2)
                {
                    GameViewManager.Instance.playerID = loginAction.Item1;
                    output = new LoginResult(true, String.Format("Loged in as {0}", username), loginAction.Item1);
                }
                else
                    output = new LoginResult(false, "Incorrect password entered");
            }
            else
                output = new LoginResult(false, "Incorrect username entered");
            return output;
        }

        public static LoginResult create(string username, string password)
        {
            LoginResult output;
            DataAccess dataAccess = new DataAccess();
            int createAction;
            // Impose restrictions on password
            if (string.IsNullOrEmpty(password))
            {
                output = new LoginResult(false, "Password can not be left empty.");
            }
            else
            {
                createAction = dataAccess.playerCreate(username, password);
                if (createAction != -1)
                {
                    GameViewManager.Instance.playerID = createAction;
                    output = new LoginResult(true, String.Format("Create account and logged in as {0}", username), createAction);
                }
                else
                    output = new LoginResult(false, "Username already exists");
            }
            return output;
        }
        
    }
}
