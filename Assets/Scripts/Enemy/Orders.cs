public enum Orders
{
    IDLE, //No orders are assigned, use to indicate a unit needs new orders
    PATROL, //Travel around a set of nodes, stopping for a time at important nodes
    INVESTIGATE, //Go to a node and start a search pattern to spot the player(s) again
    REPOSITION, //Travel from one node to another and then get new orders
    GUARD, //Stay at one node or a set and protect assets
    ASSAULT_STANDBY, //Functions as guard, but if the player is detected closest squad with this order changes to ASSAULT
    ASSAULT, //Go to the player's position and engage in combat
    RETREAT, //Travel to a safe node and do not engage targets
    FALLBACK //Move to a safer node while still engaging nearby targets
}
