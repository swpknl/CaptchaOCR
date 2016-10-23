namespace CaptchaOCR.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;

    using CaptchaApi.Contracts;
    using CaptchaApi.Impl;

    using Constants;

    using DatabaseApi.Contracts;
    using DatabaseApi.Impl;

    using Entities;

    using Helpers;

    public class ImageController : ApiController
    {
        private readonly ICaptchaApi captchaApi;
        private readonly IImageRepository repository;

        private Image databaseImage;

        static ImageController()
        {
            ConfigurationKeys.DataSource = ConfigurationManager.TryGetValue("DataSource");
        }

        public ImageController()
        {
            this.captchaApi = new TwoCaptcha();
            IDbAdapter adapter = new MsAccessAdapter();
            this.repository = new ImageRepository(adapter);
        }

        public IEnumerable<string> Get()
        {
            return this.repository.GetImages();
        }

        [HttpPost]
        public HttpResponseMessage GetResult()
        {
            var imageBytes = this.Request.Content.ReadAsByteArrayAsync().Result;
            var image = Convert.ToBase64String(imageBytes);
            string imageKey = image.GetImageKey();
            if (this.IsImagePresent(imageKey))
            {
                // Retrieve databaseImage from DB and send the text
                return new HttpResponseMessage { Content = new StringContent(this.databaseImage.Text) };
            }

            // Make API call 
            var output = this.MakeApiCall(imageBytes);
            if (output.Equals(
                _2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return new HttpResponseMessage
                           {
                               Content =
                                   new StringContent(
                                   _2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED)
                           };
            }

            if (output.Equals("ERROR", StringComparison.InvariantCultureIgnoreCase))
            {
                return new HttpResponseMessage { Content = new StringContent("Error while making API call") };
            }

            if (output.Equals(_2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE, StringComparison.InvariantCultureIgnoreCase))
            {
                return new HttpResponseMessage
                           {
                               Content =
                                   new StringContent(_2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE)
                           };
            }

            // Add databaseImage to DB    
            this.repository.InsertImage(new Image(imageKey, output));
            return new HttpResponseMessage { Content = new StringContent(output) };
        }

        private string MakeApiCall(byte[] image)
        {
            if (this.captchaApi.CallApi(image))
            {
                while (true)
                {
                    // Wait for 3 seconds before making the API call
                    Thread.Sleep(1000);
                    var result = this.captchaApi.GetResult();

                    if (result.Contains(_2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE))
                    {
                        return _2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE;
                    }

                    if (result.Contains(_2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED))
                    {
                        return _2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED;
                    }

                    if (result.Contains(_2CaptchaConstants.ERROR))
                    {
                        return "ERROR";
                    }

                    if (result.Contains(_2CaptchaConstants.CAPCHA_NOT_READY))
                    {
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            return "ERROR";
        }

        private bool IsImagePresent(string imageKey)
        {
            // Make DB call
            var result = this.repository.GetImageByKey(imageKey);
            if (result != null)
            {
                this.databaseImage = new Image(result.ImageKey, result.Text);
                return true;
            }

            return false;
        }
    }
}
