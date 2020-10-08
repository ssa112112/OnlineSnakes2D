using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Rating : MonoBehaviourPunCallbacks
{
    [SerializeField] Text[] ratingBoard;
    RatingWrite[] ratingJournal;
    int playersInRoom;

    const string YouPrefix = "YOU - ";
    const string OpponentPrefix = "FOE - ";

    /// <summary>
    /// Initialize when all players on place
    /// </summary>
    /// <param name="playersInRoom"></param>
    /// <param name="myActorID"></param>
    public void Initialize(int playersInRoom,int myActorID)
    {
        //Save playersCount
        this.playersInRoom = playersInRoom;

        //Initialize journal
        ratingJournal = new RatingWrite[playersInRoom];
        for (var i = 0; i < playersInRoom; i++)
        {
            var actorIDForThisSnake = i + 1;
            ratingJournal[i] = new RatingWrite(actorIDForThisSnake, 0,isMine: actorIDForThisSnake == myActorID);
        }

        //Display!
        Display();
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

    public bool AmIFist()
    {
        return ratingJournal.First(w => w.IsActive).IsMine;
    }

    public void PlayerLeft(int actorID)
    {
        for (var i = 0; i < playersInRoom; i++)
        {
            if (ratingJournal[i].ActorID == actorID)
            {
                ratingJournal[i].Deactivate();
                Display();
                return;
            }
        }
    }

    void Display()
    {
        for (var i = playersInRoom-1; i >= 0; i--)
        {
            string prefix = ratingJournal[i].IsMine ? YouPrefix : OpponentPrefix;
            //Set text
            ratingBoard[i].text = $"{i+1}) {prefix}{ratingJournal[i].Score}";
            //Set color
            ratingBoard[i].color = ratingJournal[i].IsActive ? ColorByActorID.Get(ratingJournal[i].ActorID) : Color.gray;
        }
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
        public bool IsActive { get; private set; }

        public RatingWrite(int actorID, int score,bool isMine)
        {
            ActorID = actorID;
            Score = score;
            IsMine = isMine;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }

}


