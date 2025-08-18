namespace Patents.ArtRepoCloud.GraphService.Extensions
{
    public static class Extensions
    {
        public static string? GetName<T>(this T enumVal) where T : Enum
        {
            return Enum.GetName(typeof(T), enumVal);
        }

        public static List<T> ToList<T>(this T enumVal) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().OrderBy(e => e).ToList();
        }

        public static Stream? Attachment(this IHttpContextAccessor accessor, string attachmentName = "file")
        {
            var formFile = accessor.HttpContext?.Request.Form.Files[attachmentName];

            return formFile?.OpenReadStream();
        }
    }
}