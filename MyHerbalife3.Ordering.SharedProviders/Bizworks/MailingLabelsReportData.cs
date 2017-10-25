using System.Collections;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.SharedProviders.Bizworks
{
    public class MailingLabelsReportData : IEnumerable<string>
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Line5 { get; set; }
        public string Line6 { get; set; }
        public string Line7 { get; set; }

        public void SetValue(string propName, string value)
        {
            switch (propName)
            {
                case "Line1":
                    {
                        Line1 = value;
                    }
                    break;
                case "Line2":
                    {
                        Line2 = value;
                    }
                    break;
                case "Line3":
                    {
                        Line3 = value;
                    }
                    break;
                case "Line4":
                    {
                        Line4 = value;
                    }
                    break;
                case "Line5":
                    {
                        Line5 = value;
                    }
                    break;
                case "Line6":
                    {
                        Line6 = value;
                    }
                    break;
                case "Line7":
                    {
                        Line7 = value;
                    }
                    break;
                default:
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return Line1;
            yield return Line2;
            yield return Line3;
            yield return Line4;
            yield return Line5;
            yield return Line6;
            yield return Line7;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            yield return Line1;
            yield return Line2;
            yield return Line3;
            yield return Line4;
            yield return Line5;
            yield return Line6;
            yield return Line7;
        }
    }
}
