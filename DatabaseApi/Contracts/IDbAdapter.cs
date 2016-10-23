namespace DatabaseApi.Contracts
{
    using System;
    using System.Data;

    public interface IDbAdapter : IDisposable
    {
        DataTable Get(string query);

        void Insert(string query);
    }
}
