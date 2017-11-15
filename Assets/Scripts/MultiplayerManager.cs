using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Assets.Scripts;

public class MultiplayerManager : MonoBehaviour {

    // Singleton implementation
    private static MultiplayerManager instance;
    public static MultiplayerManager Instance { get { return instance; } }
    
    private List<Player> players;
    private MqttClient client;
    private bool gameRunning;
    private const string TOPIC = "SDV602$2017$Tim$Game$Test/State";
    private float updateTime = 0.0f;

    // Use this for initialization
    void Start () {
        if (instance == null)
        {
            Debug.Log("Setting Instance");
            instance = this;

            // create client instance 
            client = new MqttClient("newsimland.com", 443, false, null);

            // register to message received 
            client.MqttMsgPublishReceived += messageRecieved;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // subscribe to the topic
            client.Subscribe(new string[] { TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }
        else
            Destroy(this);
    }
	
	// Update is called once per frame
	void Update () {
        if (GameViewManager.Instance.playerID != 0) // If a player is logged in
        {
            // Keep track of how long it has been
            updateTime += Time.deltaTime;
            Debug.Log(updateTime);
            // If it has been a minute, resync with the server
            if (updateTime >= 60f)
            {
                LoginManager.reSync(GameViewManager.Instance.playerID);
                // Restart timer
                updateTime = 0f;
            }
        }
	}

    private void publish(string message)
    {
        Debug.Log(string.Format("Sending: {0}", message));
        client.Publish(TOPIC, System.Text.Encoding.UTF8.GetBytes(message));
    }

    /// <summary>
    /// Resets the lists held by this game object
    /// </summary>
    public void newSession()
    {
        players = new List<Player>();
        gameRunning = true;
        publish(JsonUtility.ToJson(new Query() { Command = "JOIN", Player = GameViewManager.Instance.playerName }));
    }

    public void endSession()
    {
        gameRunning = false;
        publish(JsonUtility.ToJson(new Query() { Command = "LEAVE", Player = GameViewManager.Instance.playerName }));
    }

    private void messageRecieved(object sender, MqttMsgPublishEventArgs evnt)
    {
        if (gameRunning)
        {
            Query q;
            try
            {
                q = JsonUtility.FromJson<Query>(System.Text.Encoding.UTF8.GetString(evnt.Message));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                q = new Query() { Command = "ERROR" };
            }
            Debug.Log(JsonUtility.ToJson(q));
            switch (q.Command)
            {
                case "MOVE":
                    // So long as it's not us, log where the person is
                    if (q.Player != GameViewManager.Instance.playerName)
                        logPlayerMove(q.Player, q.Argument);
                    break;
                case "JOIN":
                    // If someone has joined, tell them where we are
                    sendPlayerMove(GameViewManager.Instance.ViewModel.CurrentScene.identifier);
                    break;
                case "LEAVE":
                    logPlayerLeft(q.Player);
                    break;
            }
        }
    }

    public void sendPlayerMove(string room)
    {
        publish(JsonUtility.ToJson(new Query() { Command = "MOVE", Player = GameViewManager.Instance.playerName, Argument = room}));
    }

    private void logPlayerMove(string name, string room)
    {
        Player existingPlayer = (from p in players where p.PlayerName == name select p).FirstOrDefault();
        if(existingPlayer != null)
        {
            existingPlayer.RoomID = room;
        }
        else
        {
            players.Add(new Player(room, name));
        }
    }
    
    private void logPlayerLeft(string name)
    {
        Player existingPlayer = (from p in players where p.PlayerName == name select p).FirstOrDefault();
        if(existingPlayer != null)
        {
            players.Remove(existingPlayer);
        }
    }

    public string getPlayersInScene(string sceneID)
    {
        string output = "";
        bool playersInScene = false;
        foreach(Player p in players)
        {
            if(p.RoomID == sceneID)
            {
                playersInScene = true;
                output = output + string.Format("{0}, ", p.PlayerName);
            }
        }
        if (playersInScene)
            output = "\n\nPlayers in this room\n" + output;
        return output;
    }

    private class Player
    {
        public string RoomID;
        public string PlayerName;

        public Player(string RoomID, string PlayerName)
        {
            this.RoomID = RoomID;
            this.PlayerName = PlayerName;
        }
    }

    private struct Query
    {
        public string Command;
        public string Player;
        public string Argument;

        public Query(string Command, string Player, string Argument)
        {
            this.Command = Command;
            this.Player = Player;
            this.Argument = Argument;
        }
    }
}
