public enum NodeTraversal
{
    NOT_SET, //Used as error handling, meaning you didn't assign anything to the node
    INFANTRY, //If this node can't fit aircraft or vehicles than only infantry move through it
    LIGHT_VEHICLE, //Handles light vehicles only, can't fit heavy tanks and such. Excludes aircraft
    HEAVY_VEHICLE, //Only used for the big boi tanks, though I can't think of an example so this may get axed
    AIRCRAFT, //This node is used for aircraft pathing and not for ground movement
    ANY_GROUND, //Any ground based movement can traverse this node
    ANY_VEHICLE //Any vehicle can move along this node, excluding infantry
}
