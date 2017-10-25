#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrderSearchParameter
    {
        public const int DefaultPageSize = 16;

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string MemberId { get; set; }

        public string Status { get; set; }
        public string Locale { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string OrderNumber { get; set; }
    }
}