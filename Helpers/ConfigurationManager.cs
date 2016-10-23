namespace Helpers
{
    using System;
    using System.Web.Configuration;

    public static class ConfigurationManager
    {
        public static string TryGetValue(string key)
        {
            try
            {
                var result = WebConfigurationManager.AppSettings.Get(key);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
