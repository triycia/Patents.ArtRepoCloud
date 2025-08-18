using Patents.ArtRepoCloud.Service.Extensions;

namespace Patents.ArtRepoCloud.Service.Configuration
{
    public class IficlaimsSettings
    {
        public string IfiServiceUrl { get; init; }
        public string IfiUserName { get ; set; }
        public string IfiPassword { get; init; }
        public double IfiAttachmentFetchAllWeightRU { get; init; }
        public int IfiDelayInSecondsForHttp429 { get; init; }
        public int IfiRetryCountForHttp429 { get; init; }
        public int IfiRetryOperationTimeoutInSeconds { get; init; }

        public string Ucid { get; init; }
        public string BibDocument { get; init; }
        public string AttachmentList { get; init; }
        public string AttachmentFetch { get; init; }
        public string AttachmentFetchAll { get; init; }
        public string FamilySimple { get; init; }
        public string FamilyExtended { get; init; }

        public string UcidUrl(string referenceNumber, string countryCode) => Ucid.Replace("{referenceNumber}", referenceNumber).Replace("{countryCode}", countryCode);
        public string BibDocumentUrl(string ucid) => BibDocument.Replace("{ucid}", ucid);
        public string AttachmentListUrl(string ucid) => AttachmentList.Replace("{ucid}", ucid);
        public string AttachmentFetchUrl(string ifiPath) => AttachmentFetch.Replace("{ifiPath}", ifiPath);
        public string AttachmentFetchAllUrl(string ucid) => AttachmentFetchAll.Replace("{ucid}", ucid);
        public string FamilySimpleUrl(int familyId) => FamilySimple.Replace("{familyId}", familyId.ToString());
        public string FamilyExtendedUrl(string ucid) => FamilyExtended.Replace("{ucid}", ucid);
    }
}