namespace App.Models
{
    public class BearerTokenOptions
    {
        public const string Name = "BearerTokenOptions";

        public string SecretKey { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
    }
}
