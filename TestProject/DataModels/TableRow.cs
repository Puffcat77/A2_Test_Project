using System;

namespace TestProject.DataModel
{
    internal class TableRow
    {

        public string sellerName { get; set; }

        public string sellerInn { get; set; }

        public string buyerName { get; set; }

        public string buyerInn { get; set; }

        public double woodVolumeBuyer { get; set; }

        public double woodVolumeSeller { get; set; }

        public DateTime? dealDate { get; set; }

        public string dealNumber { get; set;}
    }
}