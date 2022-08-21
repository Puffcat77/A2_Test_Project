using System;


namespace TestProject.DTO
{
    internal class SourceDealDTO
    {
        public Guid Id { get; set; }

        public string DeclarationNumber { get; set; }

        public DateTime? DealDate { get; set; }

        public double WoodVolumeByuer { get; set; }

        public double WoodVolumeSeller { get; set; }

        public int SellerId { get; set; }

        public int BuyerId { get; set; }

        public bool IsDealCorrect { get; set; }
    }
}