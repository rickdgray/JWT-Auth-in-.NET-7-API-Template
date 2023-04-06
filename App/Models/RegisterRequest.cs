namespace App.Models
{
    public class RegisterRequest
    {
        public AppUser User { get; set; }
        public string Password { get; set; }
    }
}
