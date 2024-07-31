public static class CrewRoles
{
    public static readonly Role Driver = new Role("Driver", 0, 1);
    public static readonly Role Gunner = new Role("Gunner", 1, 1);
    public static readonly Role Spotter = new Role("Spotter", 2, 1);
    public static readonly Role Radar = new Role("Radar", 3, 1);

    public static readonly Role[] ImplementedRoles = new Role[] { Driver, Gunner, };
    //The AllRoles array might not be all that important, but I'm leaving it in for now
    public static readonly Role[] AllRoles = new Role[] { Driver, Gunner, Spotter, Radar };

    public static Role GetRoleByID(uint id)
    {
        foreach (Role role in AllRoles)
        {
            if (role.ID == id)
                return role;
        }
        //Yeet an error at them if no role is found
        throw new System.Exception($"No Role ID exists for passed ID {id} in CrewRoles!");
    }

    public static Role GetRoleByName(string name)
    {
        foreach (Role role in AllRoles)
        {
            if (role.Name == name)
                return role;
        }
        throw new System.Exception($"No Role named {name} exists in CrewRoles!");
    }

}

[System.Serializable]
public class Role
{
    public string Name;
    public uint ID;
    public uint PlayerLimit;

    public Role() { }

    public Role(string name, uint id, uint limit)
    {
        Name = name;
        ID = id;
        PlayerLimit = limit;
    }
}
