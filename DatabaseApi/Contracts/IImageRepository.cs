namespace DatabaseApi.Contracts
{
    using System;
    using System.Collections.Generic;

    using Entities;

    public interface IImageRepository
    {
        IEnumerable<string> GetImages();

        Image GetImageByKey(string imageKey);

        void InsertImage(Image image);

    }
}
