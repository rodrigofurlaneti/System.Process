using Microsoft.AspNetCore.Authorization;
using System.Process.Domain.Enums;

namespace System.Process.Application.AuthorizationHandlers
{
    public class UserDataRequirement : IAuthorizationRequirement
    {
        public ValidationProcess ValidationProcess { get; set; }

        public UserDataRequirement(ValidationProcess validationProcess)
        {
            ValidationProcess = validationProcess;
        }
    }
}
