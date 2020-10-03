using Photon.Pun;
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
        //Set settings
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.0.1";
        
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

    public static void CreateRoom(int roomID)
    {
        PhotonNetwork.CreateRoom(roomID.ToString());
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