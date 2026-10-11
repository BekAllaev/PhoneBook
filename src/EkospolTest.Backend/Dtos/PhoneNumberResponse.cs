using EkospolTest.Backend.Models;

namespace EkospolTest.Backend.Dtos
{
    public class PhoneNumberResponse
    {
        public int Id { get; set; }

        public string Number { get; set; } = string.Empty;

        public bool IsPublic { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public string OwnerUsername { get; set; } = string.Empty;

        public static PhoneNumberResponse FromEntity(PhoneNumber phoneNumber) => new()
        {
            Id = phoneNumber.Id,
            Number = phoneNumber.Number,
            IsPublic = phoneNumber.IsPublic,
            OwnerId = phoneNumber.OwnerId,
            OwnerUsername = phoneNumber.OwnerUsername
        };
    }
}
