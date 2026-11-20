using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class CreatorWalletViewModel
    {
        public decimal ClearedBalance { get; set; }       // Available to withdraw right now
        public decimal PendingEscrowBalance { get; set; } // Locked in holding for active/pending work
        public decimal TotalLifetimeWithdrawn { get; set; }
        public string Currency { get; set; } = "INR";

        // Configured Payout Accounts (from Influencer profile)
        public string? BankAccountHolderName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfscOrSwift { get; set; }
        public string? PayoutUpiId { get; set; }

        public bool HasConfiguredPayoutMethod =>
            !string.IsNullOrWhiteSpace(PayoutUpiId) ||
            (!string.IsNullOrWhiteSpace(BankAccountNumber) && !string.IsNullOrWhiteSpace(BankIfscOrSwift));

        // Form Inputs
        [Required(ErrorMessage = "Please specify withdrawal amount")]
        [Range(500, 10000000, ErrorMessage = "Minimum payout withdrawal is 500")]
        [Display(Name = "Withdrawal Amount")]
        public decimal WithdrawAmount { get; set; }

        [Required(ErrorMessage = "Select a payout destination")]
        [Display(Name = "Payout Destination")]
        public string SelectedDestination { get; set; } = "UPI"; // "UPI" or "Bank"

        // Payout Ledger History
        public List<WalletPayoutHistoryItemViewModel> PayoutHistory { get; set; } = new();

        // Cleared Deliverables List contributing to this balance
        public List<ClearedMilestoneItemViewModel> ClearedDeliverables { get; set; } = new();
    }

    public class WalletPayoutHistoryItemViewModel
    {
        public string PayoutReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string DestinationDetail { get; set; } = string.Empty;
        public string Status { get; set; } = "Processing"; // Processing, Completed, Rejected
        public DateTime RequestedAt { get; set; }
    }

    public class ClearedMilestoneItemViewModel
    {
        public string CampaignTitle { get; set; } = string.Empty;
        public string MilestoneTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public DateTime ReleasedDate { get; set; }
    }
}