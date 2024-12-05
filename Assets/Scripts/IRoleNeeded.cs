/// <summary>
/// Interface used for checking control scripts have Profiles assigned with the correct roles. See <see cref="tankSteer"/>, <see cref="RadarRole"/>, and <see cref="Turret"/>
/// </summary>
public interface IRoleNeeded
{
    public Role RoleNeeded { get; }
    /// <summary>
    /// Checks that the role passed in matched the role assigned to RoleNeeded.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public bool HaveRole(Role role)
    {
        if (role == RoleNeeded)
            return true;
        else
            return false;
    }
    /// <summary>
    /// Implementing classes use this to set up any delegates they provide.
    /// </summary>
    /// <param name="oldRole"></param>
    /// <param name="newrole"></param>
    public void OnRoleChange(Role oldRole, Role newrole);

}
