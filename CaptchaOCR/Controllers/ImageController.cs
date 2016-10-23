namespace OCRApi.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Web;
    using System.Web.Http;

    using CaptchaApi.Contracts;
    using CaptchaApi.Impl;

    using DatabaseApi.Contracts;
    using DatabaseApi.Impl;

    using Entities;

    using Helpers;

    public class ImageController : ApiController
    {
        private readonly ICaptchaApi captchaApi;
        private readonly IImageRepository repository;
        private readonly IDbAdapter adapter;

        private Image databaseImage;

        static ImageController()
        {
            ConfigurationKeys.DataSource = ConfigurationManager.TryGetValue("DataSource");
        }

        public ImageController()
        {
            this.captchaApi = new _2Captcha();
            this.adapter = new MsAccessAdapter();
            this.repository = new ImageRepository(this.adapter);
        }

        public IEnumerable<string> Get()
        {
            return this.repository.GetImages();
        }

        [HttpPost]
        public HttpResponseMessage GetResult()
        {
            var imageBytes = Request.Content.ReadAsByteArrayAsync().Result;
            var image = Convert.ToBase64String(imageBytes);
            string imageKey = image.GetImageKey();
            if (this.IsImagePresent(imageKey))
            {
                // Retrieve databaseImage from DB and send the text
                return new HttpResponseMessage { Content = new StringContent(this.databaseImage.Text) };
            }
            else
            {
                // Make API call 
                var output = this.MakeApiCall(imageBytes);
                if (output.Equals(
                    Constants._2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return new HttpResponseMessage { Content = new StringContent(Constants._2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED) };
                }
                else if (output.Equals("ERROR", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new HttpResponseMessage { Content = new StringContent("Error while making API call") };
                }
                else if (output.Equals(Constants._2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new HttpResponseMessage { Content = new StringContent(Constants._2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE) };
                }
                else
                {
                    // Add databaseImage to DB    
                    this.repository.InsertImage(new Image(imageKey, output));
                    return new HttpResponseMessage { Content = new StringContent(output) };
                }
            }
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

                    if (result.Contains(Constants._2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE))
                    {
                        return Constants._2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE;
                    }
                    else if (result.Contains(Constants._2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED))
                    {
                        return Constants._2CaptchaConstants.ERROR_IMAGE_TYPE_NOT_SUPPORTED;
                    }
                    else if (result.Contains(Constants._2CaptchaConstants.ERROR))
                    {
                        return "ERROR";
                    }
                    else if (result.Contains(Constants._2CaptchaConstants.CAPCHA_NOT_READY))
                    {
                        continue;
                    }
                    else
                    {
                        return result;
                    }
                }
                
            }
            else
            {
                return "ERROR";
            }
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
            else
            {
                return false;
            }
        }
    }
}
