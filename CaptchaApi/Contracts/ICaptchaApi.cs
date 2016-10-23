namespace CaptchaApi.Contracts
{
    public interface ICaptchaApi
    {
        bool CallApi(byte[] image);

        string GetResult();
    }
}
