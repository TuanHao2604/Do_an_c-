using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentLinkAsync(Guid userId, string packageCode, string type = "New");
    Task ProcessWebhookAsync(string transactionId, string status);
    Task<string> GetPaymentStatusAsync(Guid paymentId);
    Task<List<Payment>> GetPaymentsByUserAsync(Guid userId);
}
