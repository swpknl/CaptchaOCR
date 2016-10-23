namespace Entities
{
    public class Image
    {
        public Image(string imageKey, string text)
        {
            this.ImageKey = imageKey;
            this.Text = text;
        }

        public string ImageKey { get; set; }

        public string Text { get; set; }
    }
}