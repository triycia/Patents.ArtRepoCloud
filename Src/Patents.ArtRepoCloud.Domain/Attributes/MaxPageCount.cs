namespace Patents.ArtRepoCloud.Domain.Attributes
{
    public class MaxPageCountAttribute : Attribute
    {
        public int Count;

        public MaxPageCountAttribute(int count)
        {
            Count = count;
        }
    }
}