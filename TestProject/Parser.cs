using System.Collections.Generic;
using System.Linq;
using TestProject.DataModel;
using TestProject.DataModels;


namespace TestProject
{
    internal class DataParser
    {
        private const string URL = "https://www.lesegais.ru/open-area/deal";

        internal List<SourceDeal> Parse(TableRow[] rows)
        {
            return rows.Select(ParseRow).ToList();
        }

        private SourceDeal ParseRow(TableRow row)
        {
            SourceDeal deal = new SourceDeal();
            deal.DeclarationNumber = row.dealNumber;
            deal.DealDate = row.dealDate;
            deal.WoodVolumeSeller = row.woodVolumeSeller;
            deal.WoodVolumeSeller = row.woodVolumeSeller;
            deal.Seller = new SourceCompany()
            {
                INN = row.sellerInn,
                Name = row.sellerName
            };

            deal.Buyer = new SourceCompany()
            {
                INN = row.buyerInn,
                Name = row.buyerName
            };
            return deal;
        }
    }
}