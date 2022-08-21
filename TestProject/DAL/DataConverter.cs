using System;
using System.Collections.Generic;
using System.Text;
using TestProject.DataModels;
using TestProject.DTO;


namespace TestProject.DAL
{
    internal class DataConverter
    {
        public Dictionary<string, string> namePrefixes;
        private Dictionary<string, string> nameReplacements;

        public DataConverter()
        {
            namePrefixes = DbRepository.GetNameShortages();
            nameReplacements = new Dictionary<string, string>()
            {
                { "»", "\"" },
                { "«", "\"" },
                { ">", "\"" },
                { "<", "\"" },
                { "  ", " " }
            };
        }

        public SourceCompanyDTO ConvertCompanyToDTO(SourceCompany company, int? id)
        {
            return new SourceCompanyDTO()
            {
                Id = id ?? 0,
                INN = company.INN,
                IsRussianINN = company.IsRussianINN,
                Name = company.Name
            };
        }

        internal SourceDealDTO ConvertDealToDTO(Guid dealId, SourceDeal deal,
            SourceCompanyDTO dealBuyer, SourceCompanyDTO dealSeller)
        {
            return new SourceDealDTO()
            {
                Id = dealId,
                DeclarationNumber = deal.DeclarationNumber,
                BuyerId = dealBuyer.Id,
                SellerId = dealSeller.Id,
                WoodVolumeByuer = deal.WoodVolumeByuer,
                WoodVolumeSeller = deal.WoodVolumeSeller,
                DealDate = deal.DealDate,
                IsDealCorrect = CheckDealCorrectness(deal)
            };
        }

        private bool CheckDealCorrectness(SourceDeal deal)
        {
            return !deal.DealDate.HasValue || deal.DealDate > DateTime.UtcNow
                || deal.DealDate < new DateTime(1970, 01, 01);
        }

        public string ToDBInsertString(SourceDealDTO deal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"NEWID(), N'{deal.DeclarationNumber}',");
            sb.Append($" {deal.SellerId}, {deal.BuyerId},");
            sb.Append(" " + deal.WoodVolumeByuer.ToString(Constants.VOLUME_DB_FORMAT,
                System.Globalization.CultureInfo.InvariantCulture) + "," +
                " " + deal.WoodVolumeSeller.ToString(Constants.VOLUME_DB_FORMAT,
                System.Globalization.CultureInfo.InvariantCulture) + ",");
            sb.Append($" {DateToDBString(deal.DealDate)}, ");
            sb.Append(deal.IsDealCorrect ? "1" : "0");
            return sb.ToString();
        }

        public string ToDBInsertQuery(SourceCompanyDTO company)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"N'{company.INN}',");
            sb.Append($" N'{CorrectForDB(FixName(company.Name))}',");
            string isRussianINN = company.IsRussianINN ? "1" : "0";
            sb.Append($" {isRussianINN}");
            return sb.ToString();
        }

        public string CorrectForDB(string name)
        {
            return name?.Replace("'", "''");
        }

        public string FixName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            while (name.Contains("  "))
            {
                name = name.Replace("  ", " ");
            }
            name = name.Trim();
            foreach (string prefix in namePrefixes.Keys)
            {
                int startIndex = name.ToLower().IndexOf(prefix.ToLower());
                if (startIndex == 0)
                {
                    name = name.Substring(0, startIndex) + namePrefixes[prefix] + name.Substring(prefix.Length);
                    break;
                }
            }
            return CleanName(name).ToUpper();
        }

        public string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            foreach (string replacement in nameReplacements.Keys)
            {
                name = name.Replace(replacement, nameReplacements[replacement]);
            }
            return name;
        }

        private string DateToDBString(DateTime? date)
        {
            return date.HasValue ? $"N'{date.Value.Date.ToString("yyyy-MM-dd")}'" : "NULL";
        }

        internal object ToDBUpdateString(SourceDealDTO deal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"N'{deal.DeclarationNumber}',");
            sb.Append(" " + deal.WoodVolumeByuer.ToString(Constants.VOLUME_DB_FORMAT,
                System.Globalization.CultureInfo.InvariantCulture) + "," +
                " " + deal.WoodVolumeSeller.ToString(Constants.VOLUME_DB_FORMAT,
                System.Globalization.CultureInfo.InvariantCulture) + ",");
            sb.Append($" {DateToDBString(deal.DealDate)}, ");
            sb.Append(deal.IsDealCorrect ? "1" : "0");
            return sb.ToString();
        }
    }
}