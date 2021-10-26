using System.ComponentModel.DataAnnotations;

namespace Sikiro.Ids4.Models
{
    public class LoginInputModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; set; }
    }
}
