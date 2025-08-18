namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class CustomerAttorneyDto
    {
        public CustomerAttorneyDto(
            string registrationStatus,
            string registrationNumber,
            string firstName,
            string middleName,
            string lastName,
            string phoneNumber)
        {
            RegistrationStatus = registrationStatus;
            RegistrationNumber = registrationNumber;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }

        public string RegistrationStatus { get; }
        public string RegistrationNumber { get; }
        public string FirstName { get; }
        public string MiddleName { get; }
        public string LastName { get; }
        public string PhoneNumber { get; }
    }
}