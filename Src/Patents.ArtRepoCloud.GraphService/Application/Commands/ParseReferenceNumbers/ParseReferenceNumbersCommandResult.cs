using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.GraphService.Application.Commands.ParseReferenceNumbers
{
    public class ParseReferenceNumbersCommandResult
    {
        public ParseReferenceNumbersCommandResult([GraphQLType(typeof(ListType<NonNullType<ObjectType<ParsedReferenceNumber>>>))] IEnumerable<ParsedReferenceNumber> referenceNumbers)
        {
            ReferenceNumbers = referenceNumbers;
        }

        [GraphQLType(typeof(ListType<NonNullType<ObjectType<ParsedReferenceNumber>>>))]
        public IEnumerable<ParsedReferenceNumber> ReferenceNumbers { get; }
    }

    public class ParsedReferenceNumber
    {
        public ParsedReferenceNumber(string source)
        {
            Source = source;
            NumberType = ReferenceNumberSourceType.Unknown;
        }

        public ParsedReferenceNumber(
            string source, 
            string? countryCode, 
            string? number, 
            string? kindCode, 
            ReferenceNumberSourceType numberType)
        {
            Source = source;
            CountryCode = countryCode;
            Number = number;
            KindCode = kindCode;
            NumberType = numberType;
        }

        public string Source { get; }
        public string? CountryCode { get; }
        public string? Number { get; }
        public string? KindCode { get; }
        public ReferenceNumberSourceType NumberType { get; }
    }
}
