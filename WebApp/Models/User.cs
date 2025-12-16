using Microsoft.AspNetCore.Identity;

namespace WebApp.Models
{
    public class User : IdentityUser<int>
    {
        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
