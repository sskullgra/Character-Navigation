using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class NetworkManager : MonoBehaviour {
    private static NetworkManager instance;

    public static NetworkManager Instance {
        get {
            if (instance == null){
                Debug.Log("No singleton exists! Creating new one.");
                GameObject owner = new GameObject("NetworkManager");
                instance = owner.AddComponent<NetworkManager>();
            }
            return instance;
        }
    }

    private Rect PlayerNameWindowRect = new Rect(10, 10, 150,80);
    private Rect ServerListWindowRect = new Rect(Screen.width - 400 - 10, 10, 400, 250);
    private Rect CreateServerWindowRect = new Rect(10,10,250,150);
    private Rect ChatWindowRect = new Rect(Screen.width - 400 - 10, 250 + 20, 400, 250);
	private Rect LevelListWindowRect = new Rect(10,150+20,400,250);

    private bool showPlayerNameWindow = true,showChatWindow = false, showCreateServerWindow = false, showServerListWindow = false, showLevelListWindow = false;
    private bool showCreateServerButton = true;
    private Vector2 chatScrollPosition,serverListScrollPosition,levelListScrollPosition;

    public GUIStyle FontStyle;

    private string chatInput = "Mage rocks!";

    private string gameType = "mage_test_1337"; //move to GameData?
    private string gameName = "Mage Test Server";
    private string gameComment = "mode";
    private int maxPlayers = 8;
    private int serverPort = 25001;

    private string directConnectIP = "192.168.0.109";
    private int directConnectPort = 25001;

    private List<string> chatMessageBuffer = new List<string>();
    private List<HostData> serverList = new List<HostData>();
    private HostData currentServer = null;//server the client is currently connected to

    private string playerName = "Player";//move this to GameData script
    private NetworkView NV = null;
	
	private string[] supportedNetworkLevels = {"testmap1","Revo_TestScene"}; // TODO: add level names here
	private string disconnectedLevel = "Menu";
 	private int lastLevelPrefix = 0;

    private bool stopCountdown = false,countdownIsRunning = false;

    //move to awake
	void Start () {
        NV = GetComponent<NetworkView>();
        if (NV == null){
            Debug.LogWarning("NetworkView missing!");
        } else { 
            NV.group = 1;
        }        
	}

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this){
            Debug.Log("A singleton already exists! Destroying new one.");
            Destroy(this);
        }

        MasterServer.ClearHostList();
        MasterServer.RequestHostList(gameType);        
    }

    //replace gametype with a enum
    void StartServer(int port,int maxPlayers,bool useNat,string gameType,string gameName,string gameComment) {
        showCreateServerButton = false;
        Debug.Log("Starting server at port: " + port + " max players: " + maxPlayers + " Nat: " + useNat);
        Network.InitializeServer(maxPlayers, port, useNat);
        if (maxPlayers > 0) { //no need to register to masterserver if user is singleplayer
            Debug.Log("Registering server " + gameType + " " + gameName + " " + gameComment);
            MasterServer.RegisterHost(gameType, gameName, gameComment);
        }
    }

    void OnServerInitialized() { 
        showCreateServerButton = true;
        DisplayChatMsg("Server initialized");
    }

    //Called on the server whenever a new player has successfully connected.
    void OnPlayerConnected(NetworkPlayer player) { 
        SendChatMsg("Player connected from " + player.ipAddress + ":" + player.port);
    }
    //Called on the server whenever a player is disconnected from the server.
    void OnPlayerDisconnected(NetworkPlayer player){
        SendChatMsg("Player disconnected (" + player + ")");
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    //Called on the client when you have successfully connected to a server.
    void OnConnectedToServer() { 
        DisplayChatMsg("Connected to server");
    }
    //Called on client during disconnection from server, but also on the server when the connection has disconnected.
    void OnDisconnectedFromServer(NetworkDisconnection info){
        if (Network.isServer) {
            DisplayChatMsg("Server closed");
        } else {
            if (info == NetworkDisconnection.LostConnection){
                DisplayChatMsg("Lost connection to the server");
            } else { 
                DisplayChatMsg("Successfully diconnected from the server");
            }  
			currentServer = null;
            MasterServer.ClearHostList();
            MasterServer.RequestHostList(gameType);
        }		
		// Load the default level (Menu)
		if(!Application.loadedLevelName.Equals(disconnectedLevel)) Application.LoadLevel(disconnectedLevel);
    }

    //Called on the client when a connection attempt fails for some reason.
    void OnFailedToConnect(NetworkConnectionError error) {
        DisplayChatMsg("Could not connect to server: " + error);
        currentServer = null;
    }	
	
	[RPC]
	void LoadLevel (string level, int levelPrefix){
		StartCoroutine(loadLevel(level, levelPrefix));
	}
	private IEnumerator loadLevel(string level, int levelPrefix)
	{
		lastLevelPrefix = levelPrefix;

		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);	

		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;

		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(levelPrefix);
		Application.LoadLevel(level);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		Network.SetSendingEnabled(0, true);

		// Notify objects that level and network is ready
		foreach (GameObject go in FindObjectsOfType(typeof(GameObject))) {
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);            
		}
	}
	
    void SendChatMsg(string msg) {
        if (Network.peerType == NetworkPeerType.Disconnected) {
            DisplayChatMsg("No connection");
        } else { 
            NV.RPC("DisplayChatMsg", RPCMode.AllBuffered, playerName + ": " + msg);
        }
    }

    [RPC]
    void DisplayChatMsg(string msg) { 
        if (chatMessageBuffer.Count + 1 > 100) chatMessageBuffer.RemoveAt(0);
        chatMessageBuffer.Add(msg);
    }

    void OnMasterServerEvent(MasterServerEvent mse) {
        switch (mse) { 
            case MasterServerEvent.HostListReceived:
                serverList.Clear();
                serverList.AddRange(MasterServer.PollHostList());
                Debug.Log("HostList received with " + serverList.Count + " Servers");
                break;
            case MasterServerEvent.RegistrationSucceeded:
                DisplayChatMsg("Server registration succeeded");
                break;
            case MasterServerEvent.RegistrationFailedGameName:
                DisplayChatMsg("Server registration failed: game name");
                break;
            case MasterServerEvent.RegistrationFailedGameType:
                DisplayChatMsg("Server registration failed: game type");
                break;
            case MasterServerEvent.RegistrationFailedNoServer:
                DisplayChatMsg("Server registration failed: no server");
                break;
        }
    }

    void OnFailedToConnectToMasterServer(NetworkConnectionError info){
        DisplayChatMsg("Could not connect to master server: " + info);
    }

    void RefreshServerList() {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList(gameType);    
    }

    void OnGUI() {
        if (showPlayerNameWindow) {
            showChatWindow = showCreateServerWindow = showServerListWindow = showLevelListWindow = false;
            PlayerNameWindowRect = GUI.Window(0, PlayerNameWindowRect, PlayerNameWindow, "PlayerName");
        } 

        if (showChatWindow) 
			ChatWindowRect = GUI.Window(1, ChatWindowRect, ChatWindow, "Chat");
        if (showCreateServerWindow) 
			CreateServerWindowRect = GUI.Window(2, CreateServerWindowRect, CreateServerWindow, "Create Server");
        if (showServerListWindow && Network.peerType != NetworkPeerType.Server) 
			ServerListWindowRect = GUI.Window(3, ServerListWindowRect, ServerListWindow, "Join Server");	
		if (showLevelListWindow && Network.peerType == NetworkPeerType.Server)
			LevelListWindowRect = GUI.Window(4, LevelListWindowRect, LevelListWindow, "Map List");
    }

    void PlayerNameWindow(int id) {
        playerName = GUILayout.TextField(playerName);
        if (playerName.Length > 3) {
            if (GUILayout.Button("Set PlayerName") || (Event.current.type == EventType.keyDown && Event.current.character == '\n')) { 
                showPlayerNameWindow = false;
                showChatWindow = showCreateServerWindow = showServerListWindow = showLevelListWindow = true;
            } 
        }
    }
	
	private string oldTempText = string.Empty;
    void ChatWindow(int id) {
		chatScrollPosition = GUILayout.BeginScrollView (chatScrollPosition,FontStyle);
		
		string tempText = string.Empty;
        foreach (string chatmsg in chatMessageBuffer) {
			tempText += chatmsg+"\n";
        }

        GUILayout.TextArea(tempText, FontStyle);
		
		GUILayout.EndScrollView ();
           
		if(!oldTempText.Equals(tempText)) {
			GUIContent gc = new GUIContent (tempText);
            chatScrollPosition.y = FontStyle.CalcHeight(gc, 150) - 150;
		}

		chatInput = GUILayout.TextField(chatInput);

        GUILayout.BeginHorizontal();
        if (countdownIsRunning) {
            if (GUILayout.Button("Stop Countdown")) {
                if (Network.peerType == NetworkPeerType.Server){
                    StopCountdown();
                } else if(Network.peerType == NetworkPeerType.Client) { 
                    NV.RPC("StopCountdown", RPCMode.Server);
                } 
                SendChatMsg("Stopped the countdown");
            }
        }
        if(GUILayout.Button("Send") || (Event.current.type == EventType.keyDown && Event.current.character == '\n')){
            if (chatInput.Length > 0) { 
                SendChatMsg(chatInput);
                chatInput = "";
            }
        }        
        GUILayout.EndHorizontal();
		oldTempText = tempText;
    }

    void CreateServerWindow(int id) {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Port");
        serverPort = System.Int32.Parse(GUILayout.TextField(serverPort.ToString()));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Server name");
        gameName = GUILayout.TextField(gameName);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Game mode");
        gameComment = GUILayout.TextField(gameComment);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Players: " + maxPlayers);
        maxPlayers = (int)GUILayout.HorizontalSlider(maxPlayers, 1, 8);
        GUILayout.EndHorizontal();

        if (Network.peerType == NetworkPeerType.Server){
            if (GUILayout.Button("Stop")) {
                if(countdownIsRunning)stopCountdown = true;
                Network.Disconnect();
                MasterServer.UnregisterHost();
            }
        } else {
            if (gameName.Length > 0 && gameComment.Length > 0) { 
                if (GUILayout.Button("Start")) {
                    StartServer(serverPort, maxPlayers-1, !Network.HavePublicAddress(), gameType, gameName, gameComment);
                }
            }            
        }             
    }

    void ServerListWindow(int id) { 
        serverListScrollPosition = GUILayout.BeginScrollView(serverListScrollPosition);

        foreach (HostData server in serverList) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(server.gameName);
            GUILayout.Label(server.comment);
            GUILayout.Label(server.connectedPlayers + "/" + server.playerLimit);
            if (server.connectedPlayers < server.playerLimit){
                if (!AssembleIP(server).Equals(AssembleIP(currentServer))) {
                    if (GUILayout.Button("Connect")) {
                        if (Network.connections.Length > 0) Network.CloseConnection(Network.connections[0], true);
                        DisplayChatMsg("Connecting to " + AssembleIP(server) + ":" + server.port + "...");
                        Network.Connect(server);
                        currentServer = server;
                        RefreshServerList();
                    }
                }
            } else {
                GUILayout.Label("full");
            }
                        
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh")) RefreshServerList();
        if (Network.connections.Length > 0) { 
            if (GUILayout.Button("Disconnect")) { 
                Network.CloseConnection(Network.connections[0], true);
                currentServer = null;
                RefreshServerList();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Connect to IP");
        directConnectIP = GUILayout.TextField(directConnectIP);
        directConnectPort = System.Int32.Parse(GUILayout.TextField(directConnectPort.ToString()));
        if (directConnectIP.Length > 0 && !AssembleIP(currentServer).Equals(directConnectIP)) { 
            if (GUILayout.Button("Connect")) {
                DisplayChatMsg("Connecting to " + directConnectIP + ":" + directConnectPort + "...");
                HostData host = new HostData();
                host.ip = directConnectIP.Split('.');
                host.port = directConnectPort;
                if (Network.connections.Length > 0) Network.CloseConnection(Network.connections[0], true);
                Network.Connect(directConnectIP, directConnectPort);
                currentServer = host;
                RefreshServerList();
            } 
        }        
        GUILayout.EndHorizontal();
    }
	
	void LevelListWindow(int id) {
        levelListScrollPosition = GUILayout.BeginScrollView(levelListScrollPosition);		
	
		foreach(string level in supportedNetworkLevels){
            GUILayout.BeginHorizontal();
            GUILayout.Label(level);
            if (Application.loadedLevelName != level && countdownIsRunning == false) { 
                if (GUILayout.Button("Play")){
                    StartCoroutine(Countdown(level,10));
			    }
            }			
            GUILayout.EndHorizontal();
		}		
		
        GUILayout.EndScrollView();
	}

    [RPC]
    void StopCountdown() {
        if(countdownIsRunning) stopCountdown = true;
    }

    [RPC] 
    void CountdownStarted(){
        countdownIsRunning = true;
    }

    [RPC]
    void CountdownStopped() {
        countdownIsRunning = false;
        stopCountdown = false;
    }

    private IEnumerator Countdown(string level, int seconds) {
        NV.RPC("CountdownStarted", RPCMode.AllBuffered);
        SendChatMsg("Loading " + level);
        while (seconds > 0 && !stopCountdown) {
            SendChatMsg(seconds + "...");
            seconds--;
            yield return new WaitForSeconds(1.0f);
        }
        if (stopCountdown == false) {
            Network.RemoveRPCsInGroup(0);
	        Network.RemoveRPCsInGroup(1);
	        NV.RPC("LoadLevel", RPCMode.AllBuffered, level, lastLevelPrefix + 1);
            SendChatMsg("GO!!!");
        }
        NV.RPC("CountdownStopped", RPCMode.AllBuffered);
    }
	
    string AssembleIP(HostData host) {
        string ip = string.Empty;
        if (host == null) return ip;
        foreach (string token in host.ip) {
            ip += token + ".";
        }
        return ip.TrimEnd('.');
    }
}
