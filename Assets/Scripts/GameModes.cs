using System.Collections.Generic;
using UnityEngine;

namespace HorniTank
{
    public static class GameModes
    {

        public static readonly Gamemode Cooperative = new("Cooperative", 0, "Players join forces to accomplish goals in the map. Players win after completing their objectives and exfiltrating from the map.", 1);
        public static readonly Gamemode Deathmatch = new("Deathmatch", 1, "Two teams of players face off against each other. The last team standing wings the match!", 2);
        public static readonly Gamemode CaptureThePrideFlag = new("Capture the Pride Flag", 2, "Several teams compete to control the pride flag at the center of the map.", 4);

        public static readonly List<Gamemode> Gamemodes = new() { Cooperative, Deathmatch, CaptureThePrideFlag };

        static Gamemode CurrentGamemode = null;

        public static Gamemode GetCurrentGameMode()
        {
            return CurrentGamemode;
        }

        public static void SetCurrentGameMode(Gamemode gamemode)
        {
            if (!Gamemodes.Contains(gamemode))
            {
                Debug.LogWarning("Cannot assign gamemodes that are not statically defined. Please use a value from gameModes.Gamemodes.");
            }
            else
            {
                CurrentGamemode = gamemode;
            }
        }

    }

    public class Gamemode
    {
        public string Name; //What do we call the gamemode?
        public uint ID; //It's ID for fetching it
        public string Description; //A brief description to tell the player about it
        public uint PlayerTeamLimit; //The number of teams that players can join. 

        public Gamemode() { }

        public Gamemode(string name, uint iD, string description, uint teamLimit)
        {
            Name = name;
            ID = iD;
            Description = description;
            PlayerTeamLimit = teamLimit;
        }
    }
}
