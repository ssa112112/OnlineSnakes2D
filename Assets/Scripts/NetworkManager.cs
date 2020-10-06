using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    const string ConnectionStatusPrefix = " Connection status: ";
    const string ConnectionStatusPrefixError = " FAILED: ";
    const float ErrorDisplayTime = 5f;
    
    float errorDisplayTimeLeft;

    [SerializeField] Text connectionStatusText;

    //For make these buttons interactable, when connect has been set
    [SerializeField] Button createGameButton, joinGameButton;

    void Start()
    {
        //If we already connected then return
        if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState != PeerStateValue.Disconnected)
            return;

        //Set settings
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.0.1";
        
        //Register "custom" type for sync
        PhotonPeer.RegisterType(typeof(Vector2Int), 99, Vector2IntExtension.SerializeVector2Int,
            Vector2IntExtension.DeserializeVector2Int);
        PhotonPeer.RegisterType(typeof(Direction), 100, DirectionSerialize.SerializeDirection,
            DirectionSerialize.DeserializeDirection);
        
        //Connect
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Handling fails //Just report to user

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ReportFail(message);
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        ReportFail(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        ReportFail(message);
    }

    void ReportFail(string message)
    {
        connectionStatusText.text = ConnectionStatusPrefixError + message;
        errorDisplayTimeLeft = ErrorDisplayTime;
    }

    #endregion

    public override void OnConnectedToMaster()
    {
        createGameButton.interactable = true;
        joinGameButton.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
    }

    void Update()
    {
        if (errorDisplayTimeLeft <= 0)
            connectionStatusText.text = ConnectionStatusPrefix + PhotonNetwork.NetworkClientState;
        else
            errorDisplayTimeLeft -= Time.deltaTime;
    }

    public static void CreateRoom(int roomID,int playersInRoom,int mapSize,int gameSpeed, int gameTime)
    {
        //Set properties
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = (byte) playersInRoom; //MaxPlayers
        roomOptions.CustomRoomProperties = new Hashtable
        {
            {RoomOptionKeys.PlayersInRoom, (byte) playersInRoom}, //MinPlayers, in this case == MaxPlayers
            {RoomOptionKeys.MapSize, (byte) mapSize},
            {RoomOptionKeys.GameSpeed, (byte) gameSpeed},
            {RoomOptionKeys.GameTimeInSeconds, (byte) gameTime}
        };

        //Create room
        PhotonNetwork.CreateRoom(roomID.ToString(),roomOptions);
    }

    public static void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public static void JoinRoomByID(int roomID)
    {
        PhotonNetwork.JoinRoom(roomID.ToString());
    }

    public static void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}