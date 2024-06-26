public interface IRoleNeeded
{
    public Role RoleNeeded { get; }
    public bool HaveRole(Role role)
    {
        if (role == RoleNeeded)
            return true;
        else
            return false;
    }

    public void OnRoleChange(Role oldRole, Role newrole);

}
