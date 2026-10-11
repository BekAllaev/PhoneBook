namespace EkospolTest.Backend.Models
{
    public class PhoneNumber
    {
        public int Id { get; set; }

        public string Number { get; set; } = string.Empty;

        public bool IsPublic { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public string OwnerUsername { get; set; } = string.Empty;
    }
}
