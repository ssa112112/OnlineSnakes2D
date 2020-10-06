using System.Linq;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Rating : MonoBehaviourPunCallbacks
{
    [SerializeField] Text[] ratingBoard;
    RatingWrite[] ratingJournal;
    int playersInRoom;
    
    const string YouPrefix = "YOU - ";
    const string OpponentPrefix = "FOE - ";

    public void Initialize(int playersInRoom)
    {
        //Save playersCount
        this.playersInRoom = playersInRoom;
        
        //Initialize journal
        ratingJournal = new RatingWrite[playersInRoom];
        for (var i = 0; i < playersInRoom; i++)
        {
            ratingJournal[i] = new RatingWrite(i + 1, 0);
        }
        
        //Display!
        Display();
    }

    void Display()
    {
        for (var i = playersInRoom-1; i >= 0; i--)
        {
            string prefix = ratingJournal[i].IsMine ? YouPrefix : OpponentPrefix;
            //Set text
            ratingBoard[i].text = $"{i+1}) {prefix}{ratingJournal[i].Score}";
            //Set color
            ratingBoard[i].color = ColorByActorID.Get(ratingJournal[i].ActorID);
        }
    }

    /// <summary>
    /// Add score fot the snake. Auto update display.
    /// </summary>
    /// <param name="score"></param>
    /// <param name="actorID"></param>
    public void AddScore(int score, int actorID)
    {
        for (var i = 0; i < playersInRoom; i++)
        {
            if (ratingJournal[i].ActorID == actorID)
                ratingJournal[i].Score += score;
        }
        SortJournal();
        Display();
    }

    void SortJournal()
    {
        ratingJournal = ratingJournal.OrderBy(w => -w.Score).ToArray();
    }
    
    /// <summary>
    /// A card for storage info about a score
    /// </summary>
    private class RatingWrite
    {
        public readonly int ActorID;
        public int Score { get; set; }
        public readonly bool IsMine;

        public RatingWrite(int actorID, int score)
        {
            ActorID = actorID;
            Score = score;
            IsMine = actorID == PhotonNetwork.LocalPlayer.GetPlayerNumber()+2;
        }
    }

}


