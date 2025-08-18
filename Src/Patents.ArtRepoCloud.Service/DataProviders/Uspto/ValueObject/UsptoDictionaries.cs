namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.ValueObject
{
    public static class UsptoDictionaries
    {
        public static Dictionary<int, string> ContinuityApplicationStatus => new()
        {
            { 150, "Patented" },
            { 250, "Patented" },
            { 454, "Reexamination certificate issued" },
            { 854, "Reexamination certificate issued" },
            { 680, "Reexamination SE certificate" },
            { 160, "Abandoned" },
            { 161, "Abandoned" },
            { 162, "Abandoned" },
            { 163, "Abandoned" },
            { 164, "Abandoned" },
            { 165, "Abandoned" },
            { 166, "Abandoned" },
            { 167, "Abandoned" },
            { 168, "Abandoned" },
            { 169, "Abandoned" },
            { 159, "Expired" },
            { 566, "PCT - International Search Report Mailed to IB" }
        };

        public static Dictionary<string, string> ContinuityDescription => new()
        {
            { "PRO", "Claims priority from a provisional application" },
            { "CON", "is a Continuation of" },
            { "NST", "is a National Stage Entry of" },
            { "CIP", "is a Continuation in-part of" },
            { "DIV", "is a Division of" },
            { "REI", "is a Reissue of" },
            { "REX", "is a Re-examination of" },
            {  "?" , "-" },
            { "SUB", "is a substitute application of" },
            { "SER", "is a Supplemental Examination of" },
            { "RIC", "unknown" }
        };
    }
}