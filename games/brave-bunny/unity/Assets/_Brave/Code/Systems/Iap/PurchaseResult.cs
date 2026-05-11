// Brave Bunny — Systems / IAP
// Mirrors UnityPurchasing.PurchaseFailureReason at a coarser granularity so
// gameplay/UI code never has to reference UnityEngine.Purchasing directly.

#nullable enable

namespace Brave.Systems.Iap;

public enum PurchaseResult
{
    Success = 0,
    Cancelled = 1,
    Failed = 2,
    Pending = 3,
}
