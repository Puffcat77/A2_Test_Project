using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TestProject.DTO;


namespace TestProject.DAL
{
    internal class DbRepository
    {
        private DataConverter converter;

        public DbRepository(DataConverter dataConverter)
        {
            converter = dataConverter;
        }

        private string FormInsertCompanyQuery(List<SourceCompanyDTO> companies)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"INSERT INTO Company (INN, Name, IsRussianINN) VALUES");
            for (int i = 0; i < companies.Count - 1; i++)
            {
                SourceCompanyDTO company = companies[i];
                sb.Append($" ({converter.ToDBInsertQuery(company)}),");
            }
            if (companies.Count - 1 > 0)
            {
                SourceCompanyDTO company = companies.LastOrDefault();
                sb.Append($" ({converter.ToDBInsertQuery(company)})");
            }
            return sb.ToString();
        }

        private string FormInsertDealQuery(List<SourceDealDTO> totalDeals)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO Deal (Id, DealNumber, SellerId, BuyerId," +
                " WoodVolumeBuyer, WoodVolumeSeller, DealDate, IsDealCorrect) \n VALUES");
            for (int i = 0; i < totalDeals.Count - 1; i++)
            {
                SourceDealDTO deal = totalDeals[i];
                sb.Append($" ({converter.ToDBInsertString(deal)}), ");
            }
            if (totalDeals.Count - 1 > 0)
            {
                SourceDealDTO deal = totalDeals.LastOrDefault();
                sb.Append($" ({converter.ToDBInsertString(deal)})");
            }
            return sb.ToString();
        }

        public void InsertDeals(List<SourceDealDTO> totalDeals, SqlConnection connection)
        {
            using (SqlCommand command =
                new SqlCommand(FormInsertDealQuery(totalDeals), connection))
            {
                int result = command.ExecuteNonQuery();
            }
        }

        internal List<SourceCompanyDTO> GetDbCompanies(SqlConnection connection)
        {
            string query = @"SELECT Id, INN, Name FROM Company";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                List<SourceCompanyDTO> result =
                    new List<SourceCompanyDTO>();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = (int)reader.GetValue(0);
                        string inn = (string)reader.GetValue(1);
                        string name = (string)reader.GetValue(2);
                        result.Add(new SourceCompanyDTO() { Id = id, INN = inn, Name = name });
                    }
                }

                reader.Close();
                return result;
            }
        }

        public static Dictionary<string, string> GetNameShortages()
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings[Constants.DB_CONNECTION_STRING]
                .ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"SELECT FullName, ShortName FROM NameShortage";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    Dictionary<string, string> result =
                        new Dictionary<string, string>();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string fullName = (string)reader.GetValue(0);
                            string shortName = (string)reader.GetValue(1);
                            result.Add(fullName, shortName);
                        }
                    }

                    reader.Close();
                    return result;
                }
            }
        }

        internal void InsertCompanies(List<SourceCompanyDTO> companiesForInsert,
            SqlConnection connection)
        {
            using (SqlCommand command =
                new SqlCommand(FormInsertCompanyQuery(companiesForInsert), connection))
            {
                int result = command.ExecuteNonQuery();
            }
        }

        internal Dictionary<string, SourceDealDTO> GetDealsDb(SqlConnection connection)
        {
            string query = "SELECT Id, DealNumber, SellerId," +
                " BuyerId, WoodVolumeBuyer, WoodVolumeSeller, DealDate FROM Deal";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                Dictionary<string, SourceDealDTO> result =
                    new Dictionary<string, SourceDealDTO>();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string dealNumber = (string)reader.GetValue(1);

                        SourceDealDTO dealDTO = new SourceDealDTO();
                        dealDTO.Id = (Guid)reader.GetValue(0);
                        dealDTO.BuyerId = (int)reader.GetValue(3);
                        dealDTO.SellerId = (int)reader.GetValue(2);
                        dealDTO.DealDate = reader.IsDBNull(6) ? null: (DateTime?)reader.GetValue(6);
                        dealDTO.DeclarationNumber = dealNumber;
                        dealDTO.WoodVolumeByuer = (double)(decimal)reader.GetValue(4);
                        dealDTO.WoodVolumeSeller = (double)(decimal)reader.GetValue(5);
                        result.Add(dealNumber, dealDTO);
                    }
                }

                reader.Close();
                return result;
            }
        }

        internal void UpdateDeals(List<SourceDealDTO> dealsForUpdate, SqlConnection connection)
        {
            string query = "dbo.UpdateDeals";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter parameter = command.Parameters
                              .AddWithValue("@Deals", FormUpdateDealParameter(dealsForUpdate));

                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "UpdateDealType";

                command.ExecuteNonQuery();
            }
        }

        private DataTable FormUpdateDealParameter(List<SourceDealDTO> dealsForUpdate)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(Guid));
            table.Columns.Add("WoodVolumeBuyer", typeof(decimal));
            table.Columns.Add("WoodVolumeSeller", typeof(decimal));
            table.Columns.Add("DealDate", typeof(DateTime));
            table.Columns.Add("IsDealCorrect", typeof(bool));
            foreach (SourceDealDTO deal in dealsForUpdate)
            {
                table.Rows.Add(deal.Id, deal.WoodVolumeByuer,
                    deal.WoodVolumeSeller, deal.DealDate, deal.IsDealCorrect);
            }
            return table;
        }

        internal void UpdateCompanies(List<SourceCompanyDTO> companies, SqlConnection connection)
        {
            string query = "dbo.UpdateCompanies";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter parameter = command.Parameters
                              .AddWithValue("@Companies", FormUpdateCompaniesParameter(companies));

                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "UpdateCompanyType";

                command.ExecuteNonQuery();
            }
        }

        private DataTable FormUpdateCompaniesParameter(List<SourceCompanyDTO> companies)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("CompanyName", typeof(string));
            foreach (SourceCompanyDTO company in companies)
            {
                table.Rows.Add(company.Id, converter.CorrectForDB(company.Name));
            }
            return table;
        }
    }
}