using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileDualOrderMonthProvider
    {
        DualOrderMonthViewModel GetDualOrderMonth(string country);
    }
}