﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wait all players and start the game, when need
/// </summary>
public class WaitingPlayers : MonoBehaviourPunCallbacks
{
     [SerializeField] Text connectedCountText;
     [SerializeField] Text roomNumberText;
     [SerializeField] Rating rating;

     //Create after end of waiting
     const string NamePlayerObject = "Snake"; 
     const string NameFruitObject = "Fruit";

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
             InitializeGame(countOfPlayersInRooms);
     }

     /// <summary>
     /// Start the game
     /// </summary>
     void InitializeGame(int playersInRoom)
     {
         //Generate ActorID
         var actorID = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 2;
         
         //Create the Snake
         PhotonNetwork.Instantiate(NamePlayerObject,transform.position,Quaternion.identity,
                 0,new object[]{actorID});

         //Save ActorID
         Hashtable hash = new Hashtable();
         hash.Add(PlayerOptionKeys.ActorID, actorID);
         PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
         
         //Activate rating
         rating.Initialize(playersInRoom, actorID);

         if (PhotonNetwork.IsMasterClient)
         {
             //Deny join
             PhotonNetwork.CurrentRoom.IsOpen = false;
             //Create apple
             PhotonNetwork.InstantiateRoomObject(NameFruitObject, transform.position, Quaternion.identity);
         }

         //Destroy this object
         Destroy(gameObject);
     }
}
