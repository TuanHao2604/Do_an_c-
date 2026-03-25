using Microsoft.EntityFrameworkCore;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IAppDbContext _db;
    public PaymentService(IAppDbContext db) => _db = db;

    public async Task<string> CreatePaymentLinkAsync(Guid userId, string packageCode, string type = "New")
    {
        var package = await _db.ServicePackages.FirstOrDefaultAsync(sp => sp.Code == packageCode)
            ?? throw new Exception("Gói dịch vụ không tồn tại.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackageId = package.Id,
            Amount = package.Price,
            Status = "Pending",
            PaymentMethod = type,
            TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}",
            CreatedAt = DateTime.UtcNow,
        };

        // In production, integrate PayOS API here to get a real payment URL
        payment.PaymentUrl = $"https://pay.example.com/checkout/{payment.TransactionId}";

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        return payment.PaymentUrl;
    }

    public async Task ProcessWebhookAsync(string transactionId, string status)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        if (payment is null) return;

        payment.Status = status;
        if (status == "Completed")
        {
            payment.CompletedAt = DateTime.UtcNow;

            // Activate subscription
            var current = await _db.UserSubscriptions
                .Where(us => us.UserId == payment.UserId && us.Status == "Active")
                .ToListAsync();
            foreach (var sub in current) sub.Status = "Expired";

            var package = await _db.ServicePackages.FirstOrDefaultAsync(sp => sp.Id == payment.PackageId);
            if (package is not null)
            {
                _db.UserSubscriptions.Add(new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = payment.UserId,
                    PackageId = payment.PackageId,
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(package.DurationDays),
                });
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task<string> GetPaymentStatusAsync(Guid paymentId)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        return payment?.Status ?? "NotFound";
    }

    public async Task<List<Payment>> GetPaymentsByUserAsync(Guid userId)
        => await _db.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
}
