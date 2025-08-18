using Patents.ArtRepoCloud.Domain.Enums;

namespace Patents.ArtRepoCloud.Domain.ReferenceNumbers
{
    public class PairNumber
    {
        public PairNumber(string number, UsptoSearchNumberType type)
        {
            Number = number;
            Type = type;
        }

        public string Number { get; }
        public UsptoSearchNumberType Type { get; }

        public override string ToString() => Number;
    }
}