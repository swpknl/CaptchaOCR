namespace DatabaseApi.Impl
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DatabaseApi.Contracts;

    using Entities;

    public class ImageRepository : IImageRepository
    {
        private readonly IDbAdapter adapter;

        private string getAllImagesQuery = @"SELECT * FROM IMAGES";

        private string getImageByKeyFormat = @"SELECT * FROM IMAGES WHERE IMAGEKEY = '{0}'";
        
        private string insertImageQueryFormat = @"INSERT INTO IMAGES (IMAGEKEY, RESULT) VALUES ('{0}', '{1}')";

        public ImageRepository(IDbAdapter adapter)
        {
            this.adapter = adapter;
        }

        public IEnumerable<string> GetImages()
        {
            var dt = this.adapter.Get(this.getAllImagesQuery);
            if (dt == null)
            {
                return null;
            }

            var images = from DataRow row in dt.AsEnumerable() select row[2].ToString();
            return images;
        }

        public Image GetImageByKey(string imageKey)
        {
            var query = string.Format(this.getImageByKeyFormat, imageKey);
            var dt = this.adapter.Get(query);
            if (dt == null)
            {
                return null;    
            }

            var images = from DataRow row in dt.AsEnumerable() select new Image(row[1].ToString(), row[2].ToString());
            return images.Any() ? images.Distinct().First() : null;
        }

        public void InsertImage(Image image)
        {
            var query = string.Format(this.insertImageQueryFormat, image.ImageKey, image.Text);
            this.adapter.Insert(query);
        }
    }
}
