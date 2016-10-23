namespace Helpers
{
    public static class Extensions
    {
        public static string GetImageKey(this string image)
        {
            return image.GetHashCode().ToString();
        }
    }
}
