namespace CaptchaApi.Impl
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using CaptchaApi.Contracts;

    using Constants;

    public class _2Captcha : ICaptchaApi
    {
        private const string RequestUrl = @"http://2captcha.com/in.php";
        private string ResponseUrl = @"http://2captcha.com/res.php?key={0}&action=get&id={1}";
        private string BalanceUrl = @"http://2captcha.com/res.php?action=getbalance&key={0}";
        private string postData = "key={0}&body={1}";
        private string captchaId;

        public bool CallApi(byte[] image)
        {
            try
            {
                var response = GetHttpResponse(image);
                var responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString.Contains("OK|"))
                {
                    this.captchaId = responseString.Split('|')[1];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static HttpResponseMessage GetHttpResponse(byte[] image)
        {
            var client = new HttpClient();
            MultipartFormDataContent multiPartForm = new MultipartFormDataContent();
            multiPartForm.Add(new StringContent(APIConstants.ApiKey), "key");
            //var imageContent = new StreamContent(new MemoryStream(image));
            //imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
             //multiPartForm.Add(imageContent, "file");
            multiPartForm.Add(new StringContent(Convert.ToBase64String(image)), "body");
            multiPartForm.Add(new StringContent("base64"), "method");
            // multiPartForm.Add(new StringContent("post"), "method");
            var result = client.PostAsync(RequestUrl, multiPartForm).Result;
            return result;
        }

        public string GetResult()
        {
            var balance = this.GetBalance();
            var url = string.Format(ResponseUrl, APIConstants.ApiKey, this.captchaId);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            if (responseString.Contains("OK|"))
            {
                var text = responseString.Split('|')[1];
                return text;
            }
            else if(responseString.Contains(_2CaptchaConstants.CAPCHA_NOT_READY))
            {
                return _2CaptchaConstants.CAPCHA_NOT_READY;
            }
            else if (responseString.Contains(_2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE))
            {
                return _2CaptchaConstants.ERROR_CAPTCHA_UNSOLVABLE;
            }
            else
            {
                return _2CaptchaConstants.ERROR;
            }
        }

        public string GetBalance()
        {
            var url = string.Format(BalanceUrl, APIConstants.ApiKey);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            double temp;
            if (double.TryParse(responseString, out temp))
            {
                return temp.ToString();
            }
            else if (responseString.Contains(_2CaptchaConstants.CAPCHA_NOT_READY))
            {
                return _2CaptchaConstants.CAPCHA_NOT_READY;
            }
            else
            {
                return _2CaptchaConstants.ERROR;
            }
        }
    }
}
