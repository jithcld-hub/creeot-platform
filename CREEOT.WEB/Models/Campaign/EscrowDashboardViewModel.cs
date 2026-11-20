namespace CREEOT.WEB.Models
{
    public class EscrowDashboardViewModel
    {
        public decimal TotalEscrowLocked { get; set; }
        public decimal TotalEscrowReleased { get; set; }
        public decimal TotalEscrowRefunded { get; set; }
        public int ActiveCampaignsCount { get; set; }

        public IEnumerable<EscrowTransactionListItemViewModel> Transactions { get; set; } = new List<EscrowTransactionListItemViewModel>();
    }

    public class EscrowTransactionListItemViewModel
    {
        public int TransactionId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentProviderMethod PaymentMethod { get; set; }
        public string? GatewayPaymentId { get; set; }
        public string? ReferenceDetail { get; set; } // Last4 / Bank / UPI ID
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}