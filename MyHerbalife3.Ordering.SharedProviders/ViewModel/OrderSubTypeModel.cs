using System;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.SharedProviders.ViewModel
{
    public class OrderSubTypeModel
    {
        #region key static fields

        public static OrderSubTypeModel F = new OrderSubTypeModel("F", "First HAP personal consumption order", 3);
        public static OrderSubTypeModel H = new OrderSubTypeModel("H", "HAP personal consumption order", 4);
        public static OrderSubTypeModel R = new OrderSubTypeModel("R", "HAP resale order", 5);
        public static OrderSubTypeModel A1 = new OrderSubTypeModel("A1", "Incaricato Consignment Order", 6);
        public static OrderSubTypeModel A2 = new OrderSubTypeModel("A2", "Incaricato Personal Consumption", 7);
        public static OrderSubTypeModel A3 = new OrderSubTypeModel("A3", "Incaricato Business Expenses", 8);
        public static OrderSubTypeModel B1 = new OrderSubTypeModel("B1", "Occasional Incaricato Consignment Order", 9);

        public static OrderSubTypeModel B2 = new OrderSubTypeModel("B2", "Occasional Incaricato Personal Consumption",
                                                                   10);

        public static OrderSubTypeModel C = new OrderSubTypeModel("C", "Concessionari with VAT", 11);

        public static OrderSubTypeModel D2 = new OrderSubTypeModel("D2",
                                                                   "Concessionari  without VAT – Personal Consumption",
                                                                   12);

        public static OrderSubTypeModel D = new OrderSubTypeModel("D",
                                                                   "Concessionari  without VAT – Personal Consumption",
                                                                   13);

        public static OrderSubTypeModel E = new OrderSubTypeModel("E", "DS Ctry of Addr is not Italy", 14);

        public static OrderSubTypeModel A = new OrderSubTypeModel("A", "DS Ctry of Addr is not Italy", 15);

        public static OrderSubTypeModel B = new OrderSubTypeModel("B", "DS Ctry of Addr is not Italy", 16);

        #endregion

        #region ctor

        private OrderSubTypeModel(string key, string name, int id)
        {
            this.Key = key;
            this.Name = name;
            this.Id = id;
        }

        #endregion

        #region Methods

        public static OrderSubTypeModel Parse(string orderSubtypeKey)
        {
            OrderSubTypeModel result = null;
            if (!TryParse(orderSubtypeKey, out result))
            {
                throw new ArgumentOutOfRangeException("Invalid OrderSubTypeModel key: " + orderSubtypeKey);
            }
            return result;
        }


        public static bool TryParse(string orderSubTypeCode, out OrderSubTypeModel orderSubTypeModel)
        {
            var result = true;
            switch (orderSubTypeCode.ToUpper())
            {
                case "F":
                    orderSubTypeModel = F;
                    break;
                case "H":
                    orderSubTypeModel = H;
                    break;
                case "R":
                    orderSubTypeModel = R;
                    break;
                case "A1":
                    orderSubTypeModel = A1;
                    break;
                case "A2":
                    orderSubTypeModel = A2;
                    break;
                case "A3":
                    orderSubTypeModel = A3;
                    break;
                case "B1":
                    orderSubTypeModel = B1;
                    break;
                case "B2":
                    orderSubTypeModel = B2;
                    break;
                case "C":
                    orderSubTypeModel = C;
                    break;
                case "D2":
                    orderSubTypeModel = D2;
                    break;
                case "D":
                    orderSubTypeModel = D;
                    break;
                case "E":
                    orderSubTypeModel = E;
                    break;
                case "A":
                    orderSubTypeModel = A;
                    break;
                case "B":
                    orderSubTypeModel = B;
                    break;
                default:
                    result = false;
                    orderSubTypeModel = null;
                    break;
            }

            return result;
        }


        public override bool Equals(object obj)
        {
            return (obj != null) && ((obj as OrderSubTypeModel).Key == this.Key);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        /// <summary>
        ///     Enumerates all possible values for this type.
        /// </summary>
        /// <returns>An enumerator which will enumerate all values of this type</returns>
        public static IEnumerable<OrderSubTypeModel> All()
        {
            yield return F;
            yield return H;
            yield return R;
            yield return A1;
            yield return A2;
            yield return A3;
            yield return B1;
            yield return B2;
            yield return C;
            yield return D2;
            yield return D;
            yield return E;
            yield return A;
            yield return B;
        }


        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return string.Format("key: [{0}]\tname:[{1}]\tcode:[{2}]", this.Key, this.Name, this.Id);
        }

        #endregion Methods

        #region Properties

        public string Key { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        #endregion
    }
}