
using Microsoft.AspNet.Identity.EntityFramework;

namespace Models
{
    public class IdentityDb : IdentityDbContext<ApplicationUser>
    {
        public IdentityDb()
            : base("DefaultConnection")
       
        {
        }
    }
}