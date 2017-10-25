namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ExpireCatalogCacheResponseViewModel : MobileResponseViewModel
    {
        public bool ProductInfoCacheExpired { get; set; }
        public bool CatalogCacheExpired { get; set; }
        public bool InventoryCacheExpired { get; set; }
        public bool SentCacheKeyCleared { get; set; }
    }
}
