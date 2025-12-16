using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class AdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Min 5 or max 20 characters allowed")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password do not match")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(50, ErrorMessage = "Max 50 characters allowed")]
        [EmailAddress(ErrorMessage = "Please enter a vaild email")]
        public string Email { get; set; }

        public enum RoleType
        {
            Admin,
            User
        }

        [Required]
        public RoleType Role { get; set; }
    }
}
