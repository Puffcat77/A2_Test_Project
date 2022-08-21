using System;
using System.Text;


namespace TestProject.DataModels
{
    internal class SourceDeal
    {
        public string DeclarationNumber { get; set; }

        public DateTime? DealDate { get; set; }

        private double woodVolumeByuer;
        public double WoodVolumeByuer
        {
            get { return woodVolumeByuer; }
            set { woodVolumeByuer = Math.Round(value, 4); } 
        }
        
        private double woodVolumeSeller;
        public double WoodVolumeSeller
        {
            get { return woodVolumeSeller; }
            set { woodVolumeSeller = Math.Round(value, 4); }
        }

        public SourceCompany Seller { get; set; }

        public SourceCompany Buyer { get; set; }
    }
}