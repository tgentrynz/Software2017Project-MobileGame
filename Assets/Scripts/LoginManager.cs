using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.ActionResult;
using Assets.Scripts.DTO.JsonDrop;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles login and account creation behaviour.
    /// </summary>
    public static class LoginManager
    {
        private const string GAME_ID = "SDV602$2017$Tim$Game$Test";

        public static LoginResult login(string username, string password)
        {
            LoginResult output;
            bool loggedIn = false;
            JsonDrop c = JsonDrop.newConnection(GAME_ID);
            PlayerData p = c.read<PlayerData>("username",username).FirstOrDefault();
            if(p != null) 
            {
                // Check player is already logged in
                loggedIn = (p.lastSync != 0 ? (DateTime.FromBinary(p.lastSync).AddMinutes(1) > DateTime.Now) : false);
                if (!loggedIn)
                {
                    if (p.password == password)
                    {
                        // Successful login
                        GameViewManager.Instance.playerID = p.identifier; // Set the game to use this account ID
                        GameViewManager.Instance.playerName = p.username;
                        new DataAccess().registerPlayerLocally(p.identifier); // Create a local storage for the account

                        // Let server know user is logged in
                        p.online = true;
                        p.lastSync = DateTime.Now.ToBinary();
                        c.update(p);

                        output = new LoginResult(true, String.Format("Loged in as {0}", p.username), p.identifier);
                    }
                    else
                    {
                        // Bad password
                        output = new LoginResult(false, "Incorrect password entered");
                    }
                }
                else
                {
                    // Already logged in
                    output = new LoginResult(false, "This user is already logged in, try again later");
                }
            }
            else
            {
                // Bad username
                output = new LoginResult(false, "Incorrect username entered");
            }
            return output;
        }

        public static void logout(int identifier)
        {
            JsonDrop c = JsonDrop.newConnection(GAME_ID);
            PlayerData p = c.read<PlayerData>(identifier.ToString()).FirstOrDefault();
            if(p != null)
            {
                p.online = false;
                p.lastSync = 0;
                c.update(p);
            }
            GameViewManager.Instance.playerID = 0;
            GameViewManager.Instance.playerName = "";
        }

        public static LoginResult create(string username, string password)
        {
            LoginResult output;
            JsonDrop c = JsonDrop.newConnection(GAME_ID);
            // Impose restrictions on password
            if (string.IsNullOrEmpty(password))
            {
                output = new LoginResult(false, "Password can not be left empty.");
            }
            else
            {
                // Check if an account already exists
                PlayerData p = c.read<PlayerData>("username", username).FirstOrDefault();
                if (p == null)
                {
                    PlayerData[] existingPlayers = c.read<PlayerData>();
                    int newID = (existingPlayers.Length != 0 ? existingPlayers.Max(x => x.identifier) + 1 : 1);
                    c.create(new PlayerData() { identifier = newID, username = username, password = password, online = true, lastSync = DateTime.Now.ToBinary()});

                    GameViewManager.Instance.playerID = newID; // Set the game to use this account ID
                    new DataAccess().registerPlayerLocally(newID); // Create a local storage for the account

                    output = new LoginResult(true, String.Format("Create account and logged in as {0}", username), newID);
                }
                else
                    output = new LoginResult(false, "Username already exists");
            }
            return output;
        }

        public static void reSync(int playerID)
        {
            JsonDrop c = JsonDrop.newConnection(GAME_ID);
            PlayerData p = c.read<PlayerData>(playerID.ToString()).FirstOrDefault();
            if(p != null)
            {
                p.lastSync = DateTime.Now.ToBinary();
                c.update(p);
            }
        }
        
    }
}
