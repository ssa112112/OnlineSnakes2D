using UnityEngine;
using System;
using UIControllers;

public class MenuManager : MonoBehaviour
{
   bool createGameButtonActivated, joinGameButtonActivated;
   [SerializeField] GameObject createGameCanvas, joinGameCanvas;
   [SerializeField] IDInputUIController ID;
   [SerializeField] LeftRightUIController players, mapSize, speed, time;

   /// <summary>
   /// Just activate/deactivate submenu
   /// </summary>
   public void CreateGameButtonPressed()
   {
      createGameButtonActivated = !createGameButtonActivated;
      // Only one menu should be activated at one time
      if (createGameButtonActivated  && joinGameButtonActivated)
      {
         joinGameButtonActivated = false;
         joinGameCanvas.SetActive(false);
      }
      createGameCanvas.SetActive(createGameButtonActivated);
   }
   
   /// <summary>
   /// Just activate/deactivate submenu
   /// </summary>
   public void JoinGameButtonPressed() 
   {
      joinGameButtonActivated = !joinGameButtonActivated;
      // Only one menu should be activated at one time
      if (joinGameButtonActivated && createGameButtonActivated)
      {
         createGameButtonActivated = false;
         createGameCanvas.SetActive(false);
      }
      joinGameCanvas.SetActive(joinGameButtonActivated);
   }
   
   /// <summary>
   /// Closes the APP
   /// </summary>
   public static void ExitButtonPressed() 
   {
      #if (UNITY_EDITOR)
            UnityEditor.EditorApplication.isPlaying = false;
      #elif (UNITY_WEBGL)
            Application.OpenURL("about:blank");
      #else
            Application.Quit();
      #endif
   }
   
   /// <summary>
   /// Go to the main menu
   /// </summary>
   public static void MainMenuButtonPressed() 
   {
      NetworkManager.LeaveRoom();
   }
   
   /// <summary>
   /// Really create new game
   /// </summary>
   public void CreateButtonPressed()
   {
      NetworkManager.CreateRoom(roomID: UnityEngine.Random.Range(1, Int16.MaxValue),
         players.GetCurrentValue(),mapSize.GetCurrentValue(),speed.GetCurrentValue(),time.GetCurrentValue());
   }
   
   /// <summary>
   /// Really join to a random room
   /// </summary>
   public void JoinRandomRoomButtonPressed()
   {
      NetworkManager.JoinRandomRoom();
   }
   
   /// <summary>
   /// Really join to room by ID
   /// </summary>
   public void JoinRoomByIDButtonPressed()
   {
      NetworkManager.JoinRoomByID(ID.GetCurrentInput());
   }

}
