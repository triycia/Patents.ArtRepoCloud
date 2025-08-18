namespace Patents.ArtRepoCloud.GraphService.DataModels
{
    public class FileUploadModel
    {
        public FileUploadModel(string referenceNumber, string fileName, int[] designatedImages, int? representativeImagePageNumber)
        {
            ReferenceNumber = referenceNumber;
            FileName = fileName;
            DesignatedImages = designatedImages;
            RepresentativeImagePageNumber = representativeImagePageNumber;
        }

        public string ReferenceNumber { get; }
        public string FileName { get; }
        public int[] DesignatedImages { get; }
        public int? RepresentativeImagePageNumber { get; }
    }
}