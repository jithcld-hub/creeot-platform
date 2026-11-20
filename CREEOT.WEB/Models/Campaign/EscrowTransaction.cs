using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    public enum PaymentProviderMethod
    {
        Card = 1,
        NetBanking = 2,
        UPI = 3
    }

    public enum TransactionStatus
    {
        Initiated = 0,
        LockedInEscrow = 1,
        ReleasedToCreator = 2,
        Refunded = 3,
        Failed = 4
    }

    public class EscrowTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [Required]
        public string PayerUserId { get; set; } = string.Empty; // Client Identity Id

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "INR";

        public PaymentProviderMethod PaymentMethod { get; set; }

        // Gateway transaction metadata
        public string? GatewayPaymentId { get; set; }
        public string? GatewayOrderId { get; set; }
        public string? UpiVirtualPaymentAddress { get; set; }
        public string? NetBankingBankCode { get; set; }
        public string? CardLastFour { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Initiated;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SettledAt { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }
    }
}