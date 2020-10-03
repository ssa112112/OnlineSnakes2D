using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPlayers : MonoBehaviourPunCallbacks
{
     [SerializeField] Text connectedCountText;
     [SerializeField] Text roomNumberText;

     void Start()
     {
         roomNumberText.text += PhotonNetwork.CurrentRoom.Name;
         UpdateConnectedCount();
     }

     public override void OnPlayerEnteredRoom(Player newPlayer) => UpdateConnectedCount();
     
     public override void OnPlayerLeftRoom(Player otherPlayer) => UpdateConnectedCount();

     void UpdateConnectedCount()
     {
         byte countOfPlayersInRooms = PhotonNetwork.CurrentRoom.PlayerCount;
         byte needPlayersInRoom = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.PlayersInRoom];
         
         connectedCountText.text =
             $"{countOfPlayersInRooms} of {needPlayersInRoom} connected";
         
         if (countOfPlayersInRooms == needPlayersInRoom)
            Destroy(gameObject);
     }
}
