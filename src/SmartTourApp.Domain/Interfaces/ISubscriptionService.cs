using SmartTourApp.Domain.Entities;

namespace SmartTourApp.Domain.Interfaces;

public interface ISubscriptionService
{
    Task<UserSubscription?> GetByUserIdAsync(Guid userId);
    Task<bool> SubscribeDefaultPackageAsync(Guid userId);
    Task<bool> ActivateSubscriptionAsync(Guid userId, Guid packageId);
}
