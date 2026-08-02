using System;

namespace API_v2.Models.Constants
{
    public static class ProjectRoles
    {
        public const string Owner   = "Owner";
        public const string Manager = "Manager";
        public const string Member  = "Member";

        public static bool IsValid(string? role) =>
            string.Equals(role, Owner, StringComparison.OrdinalIgnoreCase) || 
            string.Equals(role, Manager, StringComparison.OrdinalIgnoreCase) || 
            string.Equals(role, Member, StringComparison.OrdinalIgnoreCase);

        public static bool IsOwnerOrManager(string? role) =>
            string.Equals(role, Owner, StringComparison.OrdinalIgnoreCase) || 
            string.Equals(role, Manager, StringComparison.OrdinalIgnoreCase);
    }
}
