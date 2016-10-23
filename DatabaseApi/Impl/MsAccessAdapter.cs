namespace DatabaseApi.Impl
{
    using System;
    using System.Data;
    using System.Data.OleDb;

    using DatabaseApi.Contracts;

    using Entities;

    public class MsAccessAdapter : IDbAdapter
    {
        private readonly string connectionStringFormat = "Provider=Microsoft.Jet.OleDb.4.0;Data Source={0};";

        private readonly string connectionString;

        private OleDbConnection connection;

        public MsAccessAdapter()
        {
            this.connectionString = string.Format(this.connectionStringFormat, ConfigurationKeys.DataSource);
        }

        public DataTable Get(string query)
        {
            try
            {
                OleDbDataAdapter adapter = new OleDbDataAdapter(query, this.connectionString);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Insert(string query)
        {
            using (this.connection = new OleDbConnection(this.connectionString))
            {
                using (OleDbCommand command = this.connection.CreateCommand())
                {
                    this.connection.Open();
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }    
            }
        }

        public void Dispose()
        {
            this.connection?.Close();
        }
    }
}
