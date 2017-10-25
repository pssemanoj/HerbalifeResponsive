namespace MyHerbalife3.Ordering.Web.Ordering
{
    public interface IPurchasingLimits
    {
        bool HideEmptyListItem { get; set; }
        bool DisplayStatic { get; set; }
    }
}