using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TestProject.DAL;
using TestProject.DataModel;
using TestProject.DataModels;
using TestProject.DTO;


namespace TestProject
{
    internal class DataSync
    {
        private DbRepository dbHelper;
        private DataConverter converter;

        public DataSync()
        {
            converter = new DataConverter();
            dbHelper = new DbRepository(converter);
        }

        public void RunSync()
        {
            UI.Clear();
            while (true)
            {
                try
                {
                    MakeDetour(new DataFetcher());
                    UI.LogInfo($"Обход завершен, следующий обход начнется через {Constants.FETCH_TIMEOUT / 1000} секунд");
                }
                catch (Exception ex)
                {
                    UI.LogError("Произошла ошибка при обходе");
                    UI.LogError(ex);
                    UI.LogError("Ожидание следующего обхода");
                }
                Thread.Sleep(Constants.FETCH_TIMEOUT);
            }
        }

        private void MakeDetour(DataFetcher fetcher)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            UI.LogInfo($"Начат обход страниц сайта по {Constants.PAGE_SIZE} сделок на странице.");
            DataParser parser = new DataParser();
            Dictionary<string, SourceDeal> totalDeals = new Dictionary<string, SourceDeal>();
            List<SourceDeal> currentDeals;
            Dictionary<string, Dictionary<string, SourceCompany>> totalCompanies =
                new Dictionary<string, Dictionary<string, SourceCompany>>();
            int pageNum = 0;
            Stopwatch pageStopwatch = new Stopwatch();
            do
            {
                pageStopwatch.Restart();
                Page page = fetcher.GetDataPage(pageNum);
                currentDeals = parser.Parse(page.data.searchReportWoodDeal.content);
                foreach (SourceDeal deal in currentDeals)
                {
                    TryAddSourceCompany(totalCompanies, deal.Seller);
                    TryAddSourceCompany(totalCompanies, deal.Buyer);
                    if (!totalDeals.ContainsKey(deal.DeclarationNumber))
                    {
                        totalDeals.Add(deal.DeclarationNumber, deal);
                    }
                    else
                    {
                        UpdatDealToSync(totalDeals[deal.DeclarationNumber], deal);
                    }
                }
                pageNum++;
                pageStopwatch.Stop();
                UI.LogStatistics($"Получены данные страницы {pageNum} за:" +
                    $" {pageStopwatch.ElapsedMilliseconds} мс");
            }
            while (currentDeals.Count != 0);
            sw.Stop();
            UI.LogStatistics($"Обход завершен за: {sw.ElapsedMilliseconds} мс");
            UI.LogInfo("Синхронизация данных.");
            UI.LogInfo($"Всего сделок обнаружено {totalDeals.Count}.");
            sw.Restart();
            SyncData(totalDeals, totalCompanies.Values.SelectMany(v => v.Values).ToList());
            sw.Stop();
            UI.LogStatistics($"Синхронизация завершена за: {sw.ElapsedMilliseconds} мс");
        }

        private void TryAddSourceCompany(Dictionary<string, Dictionary<string, SourceCompany>> dealCompanies, SourceCompany company)
        {
            company.Name = converter.FixName(company.Name);
            if (dealCompanies.ContainsKey(company.INN))
            {
                if (!dealCompanies[company.INN].ContainsKey(company.Name))
                {
                    dealCompanies[company.INN].Add(company.Name, company);
                }
            }
            else
            {
                dealCompanies.Add(company.INN, new Dictionary<string, SourceCompany> { { company.Name, company } });
            }
        }

        private void UpdatDealToSync(SourceDeal oldDeal, SourceDeal newDeal)
        {
            if (newDeal.WoodVolumeSeller != 0 && oldDeal.WoodVolumeSeller == 0)
            {
                oldDeal.WoodVolumeSeller = newDeal.WoodVolumeSeller;
            }
            if (newDeal.WoodVolumeByuer != 0 && oldDeal.WoodVolumeByuer == 0)
            {
                oldDeal.WoodVolumeByuer = newDeal.WoodVolumeByuer;
            }
        }

        private void SyncData(Dictionary<string, SourceDeal> deals,
            List<SourceCompany> totalCompanies)
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings[Constants.DB_CONNECTION_STRING]
                .ConnectionString;
            Stopwatch sw = new Stopwatch();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                UI.LogInfo("Синхронизация компаний.");
                List<SourceCompanyDTO> cashedCompanies =
                    dbHelper.GetDbCompanies(connection);
                
                sw.Restart();
                SyncCompanies(totalCompanies, cashedCompanies, connection);
                sw.Stop();
                UI.LogStatistics($"Синхронизация компаний выполнена за: {sw.ElapsedMilliseconds} мс");
                cashedCompanies = dbHelper.GetDbCompanies(connection);
                
                UI.LogInfo("Синхронизация сделок.");
                sw.Restart();
                SyncDeals(deals, cashedCompanies, connection);
                sw.Stop();
                UI.LogStatistics($"Синхронизация сделок выполнена за: {sw.ElapsedMilliseconds} мс");
                
                connection.Close();
            }
        }

        private void SyncCompanies(List<SourceCompany> companies,
            List<SourceCompanyDTO> cashedCompanies, SqlConnection connection)
        {
            Dictionary<string, List<SourceCompanyDTO>> cashedDTOCompanies = SeparateDTOCompanies(cashedCompanies);
            Dictionary<string, List<SourceCompanyDTO>> companiesForInsert = new Dictionary<string, List<SourceCompanyDTO>>();
            Dictionary<string, List<SourceCompanyDTO>> companiesForUpdate = new Dictionary<string, List<SourceCompanyDTO>>();
            foreach (SourceCompany company in companies)
            {
                try
                {
                    SourceCompanyDTO dbCompany = GetSimilarCompanyDTO(cashedDTOCompanies, company);
                    if (dbCompany == null)
                    {
                        InsertCompany(companiesForInsert, company, dbCompany);
                    }
                    else
                    {
                        if (dbCompany.Name.ToUpper() == company.Name.ToUpper())
                        {
                            continue;
                        }
                        InsertCompany(companiesForUpdate, company, dbCompany);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Constants.ERROR_TEXT,
                        $"обработке компании {company.INN} {company.Name}", ex.Message, ex.StackTrace));
                }
            }

            InsertByPackage(connection, companiesForInsert.Values.SelectMany(v => v).ToList());
            UpdateByPackage(connection, companiesForUpdate.Values.SelectMany(v => v).ToList());
        }

        private Dictionary<string, List<SourceCompanyDTO>> SeparateDTOCompanies(List<SourceCompanyDTO> sourceCompanyDTOs)
        {
            Dictionary<string, List<SourceCompanyDTO>> result = new Dictionary<string, List<SourceCompanyDTO>>();
            foreach (SourceCompanyDTO companyDTO in sourceCompanyDTOs)
            {
                if (result.ContainsKey(companyDTO.INN))
                {
                    SourceCompanyDTO similarComp = GetSimilarCompanyDTO(result, companyDTO);
                    if (similarComp == null)
                    {
                        result[companyDTO.INN].Add(companyDTO);
                    }
                    else if (similarComp.Name?.Length < companyDTO.Name?.Length)
                    {
                        similarComp.Name = companyDTO.Name;
                    }
                }
                else
                {
                    result.Add(companyDTO.INN, new List<SourceCompanyDTO> { companyDTO });
                }
            }
            return result;
        }

        private void InsertCompany(Dictionary<string, List<SourceCompanyDTO>> dtoCompanies,
            SourceCompany company, SourceCompanyDTO dbCompany)
        {
            if (!dtoCompanies.ContainsKey(company.INN))
            {
                dtoCompanies.Add(company.INN, new List<SourceCompanyDTO> { converter.ConvertCompanyToDTO(company, dbCompany?.Id) });
                return;
            }
            SourceCompanyDTO newComp = GetSimilarCompanyDTO(dtoCompanies, company);
            if (newComp == null)
            {
                if (string.IsNullOrWhiteSpace(dbCompany?.Name) || company.Name.Length >= dbCompany?.Name.Length)
                {
                    if (dtoCompanies[company.INN] == null)
                    {
                        dtoCompanies[company.INN] = new List<SourceCompanyDTO>();
                    }
                    dtoCompanies[company.INN].Add(converter.ConvertCompanyToDTO(company, dbCompany?.Id));
                }
            }
        }

        private SourceCompanyDTO GetSimilarCompanyDTO(
            Dictionary<string, List<SourceCompanyDTO>> companies,
            SourceCompany company)
        {
            if (companies.ContainsKey(company.INN))
            {
                return companies[company.INN].FirstOrDefault(c => CleanName(c.Name) == CleanName(company.Name));
            }
            return null;
        }

        private SourceCompanyDTO GetSimilarCompanyDTO(
            Dictionary<string, List<SourceCompanyDTO>> companies,
            SourceCompanyDTO company)
        {
            if (companies.ContainsKey(company.INN))
            {
                return companies[company.INN].FirstOrDefault(c => CleanName(c.Name) == CleanName(company.Name));
            }
            return null;
        }

        private string CleanName(string name)
        {
            return converter.CleanName(name).ToUpper();
        }

        private void SyncDeals(Dictionary<string, SourceDeal> deals,
            List<SourceCompanyDTO> dbCompanies, SqlConnection connection)
        {
            Dictionary<string, List<SourceCompanyDTO>> cashedDTOCompanies = SeparateDTOCompanies(dbCompanies);
            Dictionary<string, SourceDealDTO> dealsDb = dbHelper.GetDealsDb(connection);
            Dictionary<string, SourceDealDTO> dealsForInsert = new Dictionary<string, SourceDealDTO>();
            Dictionary<string, SourceDealDTO> dealsForUpdate = new Dictionary<string, SourceDealDTO>();
            foreach (string key in deals.Keys)
            {
                try
                {
                    deals[key].Buyer.Name = converter.FixName(deals[key].Buyer.Name);
                    deals[key].Seller.Name = converter.FixName(deals[key].Seller.Name);
                    SourceCompanyDTO dealBuyer =
                        GetSimilarCompanyDTO(cashedDTOCompanies, deals[key].Buyer);
                    SourceCompanyDTO dealSeller =
                        GetSimilarCompanyDTO(cashedDTOCompanies, deals[key].Seller);
                    if (dealsDb.ContainsKey(key))
                    {
                        SourceDealDTO oldDeal = dealsDb[key];
                        SourceDeal newDeal = deals[key];
                        if (oldDeal.DealDate < newDeal.DealDate
                            || oldDeal.WoodVolumeByuer != newDeal.WoodVolumeByuer
                            || oldDeal.WoodVolumeSeller != newDeal.WoodVolumeSeller)
                        {
                            dealsForUpdate.Add(key,
                                converter.ConvertDealToDTO(dealsDb[key].Id, newDeal, dealBuyer, dealSeller));
                        }
                    }
                    if (!dealsDb.ContainsKey(key))
                    {
                        dealsForInsert.Add(key,
                            converter.ConvertDealToDTO(Guid.NewGuid(), deals[key], dealBuyer, dealSeller));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Constants.ERROR_TEXT,
                        $"сделки {key}", ex.Message, ex.StackTrace));
                }
            }

            InsertByPackage(connection, dealsForInsert.Values.ToList());
            UpdateByPackage(connection, dealsForUpdate.Values.ToList());
        }

        private void InsertByPackage(SqlConnection connection,
            List<SourceDealDTO> deals)
        {
            UI.LogInfo($"Сделок на вставку: {deals.Count}");
            List<SourceDealDTO> package = new List<SourceDealDTO>();
            int inserted = 0;
            foreach (SourceDealDTO deal in deals)
            {
                inserted++;
                package.Add(deal);
                if (inserted % Constants.PACKAGE_SIZE == 0)
                {
                    dbHelper.InsertDeals(package, connection);
                    UI.LogStatistics($"Сделок вставлено: {inserted} из {deals.Count}");
                    package.Clear();
                }
            }
            if (package.Any())
            {
                dbHelper.InsertDeals(package, connection);
                UI.LogStatistics($"Сделок вставлено: {inserted} из {deals.Count}");
            }
        }

        private void UpdateByPackage(SqlConnection connection,
            List<SourceDealDTO> deals)
        {
            UI.LogInfo($"Сделок на обновление: {deals.Count}");
            List<SourceDealDTO> package = new List<SourceDealDTO>();
            int inserted = 0;
            foreach (SourceDealDTO deal in deals)
            {
                inserted++;
                package.Add(deal);
                if (inserted % Constants.PACKAGE_SIZE == 0)
                {
                    dbHelper.UpdateDeals(package, connection);
                    UI.LogStatistics($"Сделок обновлено: {inserted} из {deals.Count}");
                    package.Clear();
                }
            }
            if (package.Any())
            {
                dbHelper.UpdateDeals(package, connection);
                UI.LogStatistics($"Сделок обновлено: {inserted} из {deals.Count}");
            }
        }

        private void InsertByPackage(SqlConnection connection,
            List<SourceCompanyDTO> companies)
        {
            UI.LogInfo($"Компаний на вставку: {companies.Count}");
            List<SourceCompanyDTO> package = new List<SourceCompanyDTO>();
            int inserted = 0;
            foreach (SourceCompanyDTO company in companies)
            {
                inserted++;
                package.Add(company);
                if (inserted % Constants.PACKAGE_SIZE == 0)
                {
                    dbHelper.InsertCompanies(package, connection);
                    UI.LogStatistics($"Компаний вставлено: {inserted} из {companies.Count}");
                    package.Clear();
                }
            }
            if (package.Any())
            {
                dbHelper.InsertCompanies(package, connection);
                UI.LogStatistics($"Компаний вставлено: {inserted} из {companies.Count}");
            }
        }

        private void UpdateByPackage(SqlConnection connection,
            List<SourceCompanyDTO> companies)
        {
            UI.LogInfo($"Компаний на обновление: {companies.Count}");
            List<SourceCompanyDTO> package = new List<SourceCompanyDTO>();
            int inserted = 0;
            foreach (SourceCompanyDTO company in companies)
            {
                inserted++;
                package.Add(company);
                if (inserted % Constants.PACKAGE_SIZE == 0)
                {
                    dbHelper.UpdateCompanies(package, connection);
                    UI.LogStatistics($"Компаний обновлено: {inserted} из {companies.Count}");
                    package.Clear();
                }
            }
            if (package.Any())
            {
                dbHelper.UpdateCompanies(package, connection);
                UI.LogStatistics($"Компаний обновлено: {inserted} из {companies.Count}");
            }
        }
    }
}