using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components.Authorization;

namespace Infrastructure.Utility
{
    public static partial class Utility
    {
        public static async Task<UserRole.Roles> GetRole(Task<AuthenticationState> authState)
        {
            ClaimsPrincipal user = (await authState).User;

            return user.Identity.IsAuthenticated
                ? UserRole.NameToUserRole(user.FindFirst(v => v.Type == ClaimTypes.Role).Value)
                : UserRole.Roles.Undefined;
        }
    }
}