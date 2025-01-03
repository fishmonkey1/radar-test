using System.Collections.Generic;

namespace HorniTank
{
    public class Teams
    {
        /// <summary>
        /// There is only one instance of Teams, so we use a simple singelton pattern
        /// </summary>
        static Teams instance;
        public static Teams Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Teams();
                }
                return instance;
            }
        }
        public List<TeamInfo> AllTeams = new();

        public static TeamInfo GetTeamByID(uint id)
        {
            foreach (TeamInfo team in Instance.AllTeams)
            {
                if (team.TeamId == id)
                    return team;
            }
            UnityEngine.Debug.LogWarning($"Searched for a team ID of {id} but no teams match this. Investigate this.");
            return null; //We didn't find anything for you.
        }

        public static TeamInfo GetTeamByName(string name)
        {
            foreach (TeamInfo team in Instance.AllTeams)
            {
                if (team.TeamName == name)
                    return team;
            }
            UnityEngine.Debug.LogWarning($"Attempted to find a team with name {name} and but no teams match this. Investigate this.");
            return null;
        }

        public static bool CheckTeamsHostile(TeamInfo firstTeam, TeamInfo secondTeam)
        {
            if (firstTeam.EnemyTeamIds.Contains(secondTeam.TeamId) || secondTeam.EnemyTeamIds.Contains(firstTeam.TeamId))
            {
                return true; //At least one group is hostile to the other
            }
            return false; //Otherwise they aren't hostile to each other
        }

        public static bool CheckTeamsFriendly(TeamInfo firstTeam, TeamInfo secondTeam)
        {
            if (firstTeam.FriendlyTeamIds.Contains(secondTeam.TeamId) || secondTeam.FriendlyTeamIds.Contains(secondTeam.TeamId))
            {
                return true; //This teams are friendly to each other
            }
            return false; //Otherwise they are not friendly to each other
        }

        public static bool CheckTeamsNeutral(TeamInfo firstTeam, TeamInfo secondTeam)
        {
            if (CheckTeamsHostile(firstTeam, secondTeam) != true && CheckTeamsFriendly(firstTeam, secondTeam) != true)
            {
                return true; //These teams are neutral to each other
            }
            return false;
        }

    }

    [System.Serializable]
    public class TeamInfo
    {

        public TeamInfo()
        {
            Teams.Instance.AllTeams.Add(this); //Try to add ourselves to the team list
        }

        public string TeamName;
        /// <summary>
        /// TeamId is used as a primary key when determining which team we're trying to find.
        /// TODO: Consider turning this into a byte later on
        /// </summary>
        public uint TeamId;
        /// <summary>
        /// The number of players that can be on a team. Defaults to 32 for now.
        /// TODO: Consider changing this to a byte later on
        /// </summary>
        public uint TeamLimit = 32;
        public List<uint> EnemyTeamIds = new();
        public List<uint> FriendlyTeamIds = new();
    }
}