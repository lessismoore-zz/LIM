using Microsoft.AspNet.Identity;

namespace LessIsMoore.Web
{
    public class IdentityUser : IUser
    {
        public string Id => throw new System.NotImplementedException();

        public string UserName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}