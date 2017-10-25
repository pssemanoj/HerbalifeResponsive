namespace MyHerbalife3.Ordering.Providers.RulesManagement
{
	/// <summary>Shopping Cart Events for Cart Processor Rules</summary>
	public enum ShoppingCartRuleReason
	{
		Unknown,
		CartBeingCreated,
		CartCreated,
        CartBeingClosed,
        CartClosed,
		CartRetrieved,
		CartBeingSaved,
		CartSaved,
		CartItemsBeingAdded,
		CartItemsAdded,
		CartItemsBeingRemoved,
		CartItemsRemoved,
		CartBeingDeleted,
		CartDeleted,
        CartBeingCalculated,
        CartCalculated,
        CartFreightCodeChanging,
        CartWarehouseCodeChanged,
        CartOrderSubTypeChanged,
        CartPaymentOptionChanged,
        CartRuleFailed,
        CartBeingPaid
	}

    public enum OrderManagementRuleReason
    {
        Unknown,
        OrderFilled,
        OrderPricing,
        OrderBeingSubmitted
    }

    public enum PurchaseRestrictionRuleReason
    {
        Unknown,
        PurchasingLimitsFetched
    }
}
