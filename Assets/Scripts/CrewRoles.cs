public static class CrewRoles
{
    public static readonly Role Driver = new Role("Driver", 0, 1);
    public static readonly Role Gunner = new Role("Gunner", 1, 1);
    public static readonly Role Spotter = new Role("Spotter", 2, 1);
    public static readonly Role Radar = new Role("Radar", 3, 1);

    public static readonly Role[] ImplementedRoles = new Role[] { Driver, Gunner, };
    //The AllRoles array might not be all that important, but I'm leaving it in for now
    public static readonly Role[] AllRoles = new Role[] { Driver, Gunner, Spotter, Radar };

}

public class Role
{
    public string Name { get; private set; }
    public uint ID { get; private set; }
    public uint Limit { get; private set; }

    public Role(string name, uint id, uint limit)
    {
        Name = name;
        ID = id;
        Limit = limit;
    }
}
