using System;
using System.Collections.Generic;

namespace HorniTank
{
    public static class WinState
    {
        /// <summary>
        /// If the game has been completed, this will be set to true. Outside sources cannot change this value.
        /// </summary>
        public static bool GameWon { get; private set; }
        /// <summary>
        /// A list of all of the teams that have completed their objectives. For most game modes, this will only contain one team
        /// </summary>
        public static List<TeamInfo> WinningTeams { get; private set; } = new();

        /// <summary>
        /// When this is set to true, the time remaining for the match has expired and we should check if anybody has won
        /// </summary>
        public static bool TimeUp { get; private set; }

        /// <summary>
        /// Scripts that need to know about when the game ends subscribe to this event. This is fired from <see cref="WinGame"/>
        /// </summary>
        public static event Action OnGameWon;
        /// <summary>
        /// Scripts that need to know when the timer for the match has run out subscribes to this event. This event is fired from <see cref="TimesUp"/>
        /// </summary>
        public static event Action OnTimeUp;

        /// <summary>
        /// Inform the WinState that at a team has won their objective. This does not mean they have won the game until <see cref="GameWon"/> is True.
        /// </summary>
        /// <param name="team"></param>
        public static void TeamWins(TeamInfo team)
        {
            if (!WinningTeams.Contains(team))
            {
                WinningTeams.Add(team);
            }
        }

        /// <summary>
        /// Sets the WinState to finished and triggers the OnGameWon event. <see cref="OnGameWon"/>
        /// </summary>
        public static void WinGame()
        {
            //Changes the GameWon state to true and inform all listeners of the victory
            GameWon = true;
            OnGameWon?.Invoke(); //Invoke the event if it exists
        }

        /// <summary>
        /// Sets the TimeUp to true and triggers the OnTimeUp event. <see cref="OnTimeUp"/>
        /// </summary>
        public static void TimesUp()
        {
            TimeUp = true;
            OnTimeUp?.Invoke(); //Inform anybody listening to this event that the time ran out
        }

    }

}
