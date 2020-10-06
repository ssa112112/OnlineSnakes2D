using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPlayers : MonoBehaviourPunCallbacks
{
     [SerializeField] Text connectedCountText;
     [SerializeField] Text roomNumberText;

     const string NamePlayerObject = "Snake"; //Create after end of waiting

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
         {
             //Create the Snake
             PhotonNetwork.Instantiate(NamePlayerObject,transform.position,Quaternion.identity,
                 0,new object[]{PhotonNetwork.LocalPlayer.GetPlayerNumber()+2});
             
             if (PhotonNetwork.IsMasterClient)
             {
                 //Deny join
                 PhotonNetwork.CurrentRoom.IsOpen = false;
             }

             //Destroy this object
             Destroy(gameObject);
         }
     }
}
