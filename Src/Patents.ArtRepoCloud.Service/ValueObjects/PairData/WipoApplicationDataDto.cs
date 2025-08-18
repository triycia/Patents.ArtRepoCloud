namespace Patents.ArtRepoCloud.Service.ValueObjects.PairData
{
    public class WipoApplicationDataDto
    {
        public WipoApplicationDataDto(
            string? internationalPublicationNumber,
            DateTime? internationalPublicationDate,
            string? relatedApplicationNumberText)
        {
            InternationalPublicationNumber = internationalPublicationNumber;
            InternationalPublicationDate = internationalPublicationDate;
            RelatedApplicationNumberText = relatedApplicationNumberText;
        }

        public string? InternationalPublicationNumber { get; }
        public DateTime? InternationalPublicationDate { get; }
        public string? RelatedApplicationNumberText { get; }
    }
}