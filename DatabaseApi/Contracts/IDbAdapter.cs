namespace DatabaseApi.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Entities;

    public interface IDbAdapter : IDisposable
    {
        DataTable Get(string query);

        void Insert(string query);
    }
}
