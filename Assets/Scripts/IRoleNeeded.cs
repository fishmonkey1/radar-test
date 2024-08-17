public interface IRoleNeeded
{
    public Role RoleNeeded { get; }
    public bool HaveRole(Role role)
    {
        if (role.Name == RoleNeeded.Name)
            return true;
        else
            return false;
    }

    public void OnRoleChange(Role oldRole, Role newrole);

}
