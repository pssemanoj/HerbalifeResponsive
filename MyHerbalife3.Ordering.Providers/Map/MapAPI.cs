using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Map
{
    public class HLGoogleMap : HLAbstractMapper
    {
        #region Private fields
        private static string Script = @"
        $(document).ready(function() {
                |*/markers/*|
                var mapOptions = {
                center: new google.maps.LatLng(markers[0].lat, markers[0].lng),
                zoom: 12,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            var infoWindow = new google.maps.InfoWindow();
            var map = new google.maps.Map(document.getElementById('dvMap'), mapOptions);
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                var myLatlng = new google.maps.LatLng(data.lat, data.lng);
                var marker = new google.maps.Marker({
                    position: myLatlng,
                    map: map,
                    title: data.title
                });
                (function (marker, data) {
                    google.maps.event.addListener(marker, 'click', function (e) {
                        infoWindow.setContent(data.description);
                        infoWindow.open(map, marker);
                    });
                })(marker, data);
            }
        });";
        #endregion

        private List<string> GetLongitudeAndLatitude(string addresses, string sensor)
        {
            List<String> returnValue = new List<string>();

            string urlAddress = "http://maps.googleapis.com/maps/api/geocode/xml?address=" +
                                HttpUtility.UrlEncode(addresses) + "&sensor=" + sensor;

            try
            {
                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(urlAddress);
                XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/GeocodeResponse/result/geometry/location");
                foreach (XmlNode objXmlNode in objXmlNodeList)
                {
                    // GET LONGITUDE 
                    returnValue.Add(objXmlNode.ChildNodes.Item(0).InnerText);

                    // GET LATITUDE 
                    returnValue.Add(objXmlNode.ChildNodes.Item(1).InnerText);
                }
            }
            catch
            {
                // Process an error action here if needed  
            }


            return returnValue;
        }

        /// <summary>
        /// Gets the longitude and latitude points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private List<string> GetLongitudeAndLatitude(string points)
        {
            var point = new List<string>();
            var idx = points.LastIndexOfAny("+-".ToCharArray());
            point.Add(points.Substring(0,idx));
            point.Add(points.Substring(idx));
            return point;
        }

        public override string ShowMap(List<Address_V01> listaddress)
        {
            try
            {
                List<string> latitudeLongitude = new List<string>();
                string getlatitudelongitude = " var markers = [";
                string append = string.Empty;
                string markers = string.Empty;
                foreach (var address in listaddress)
                {
                    latitudeLongitude = GetLongitudeAndLatitude(Convert.ToString(address.PostalCode + " " + address.Country + " " +
                                                              address.City + " " + address.StateProvinceTerritory + " " +
                                                              address.Line1), "false");
                    append += "{'lat': '" + Convert.ToString(latitudeLongitude[0]) + "',";
                    append += "'lng': '" + Convert.ToString(latitudeLongitude[1] + "',");
                    append += "'description':'" + Convert.ToString(address.Line1 + " ," + address.City + "  ," + address.StateProvinceTerritory) + "'},";
                }

                markers = markers.Remove(markers.Length - 1);
                markers = getlatitudelongitude + append + "];";
                return @"
$(document).ready(function() {"
                 + "debugger;"
                    + markers +
                    @" var mapOptions = {
                center: new google.maps.LatLng(markers[0].lat, markers[0].lng),
                zoom: 12,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            var infoWindow = new google.maps.InfoWindow();
            var map = new google.maps.Map(document.getElementById('dvMap'), mapOptions);
            for (i = 0; i < markers.length; i++) {
                var data = markers[i];
                var myLatlng = new google.maps.LatLng(data.lat, data.lng);
                var marker = new google.maps.Marker({
                    position: myLatlng,
                    map: map,
                    title: data.title
                });
                (function (marker, data) {
                    google.maps.event.addListener(marker, 'click', function (e) {
                        infoWindow.setContent(data.description);
                        infoWindow.open(map, marker);
                    });
                })(marker, data);
            }
        });";

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Dispaly GoogleMap  Country:{0}  ERR:{1}", listaddress[0].Country, ex));
            }
            return string.Empty;
        }

        /// <summary>
        /// Display the locations in the map.
        /// </summary>
        /// <param name="locations">Location list.</param>
        /// <returns></returns>
        public override string ShowMap(List<DeliveryOption> locations)
        {
            try
            {
                var markers = new StringBuilder(" var markers = [");
                foreach (var location in locations)
                {
                    var point = GetLongitudeAndLatitude(location.GeographicPoint);
                    markers.Append("{'lat': '");
                    markers.Append(point[0]);
                    markers.Append("',");
                    markers.Append("'lng': '");
                    markers.Append(point[1]);
                    markers.Append("',");
                    markers.Append("'description':'");
                    markers.Append(location.DisplayName.Replace("'", "\\'"));
                    markers.Append("'},");
                }
                markers = markers.Remove(markers.Length - 1, 1);
                markers.Append("];");
                return Script.Replace("|*/markers/*|", markers.ToString());
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Dispaly GoogleMap  Country:{0}  ERR:{1}", locations[0].Address.Country, ex));
            }
            return string.Empty;
        }
    }

    public class HLBaiduMap : HLAbstractMapper
    {
        public override string ShowMap(List<Address_V01> listaddress)
        {
            try
            {


                string alladdress = string.Empty;
                string tempCity = string.Empty;
                foreach (var address in listaddress)
                {

                    alladdress += Convert.ToString("\"" + address.City + " " + address.CountyDistrict + " " +
                                                          address.Line1 + "\",");
                    tempCity = string.IsNullOrEmpty(address.City) ? "东营市东营区" : address.City;
                }


                return " var map = new BMap.Map(\"l-map\");" +
    "map.centerAndZoom(\"" + tempCity + "\",10);" +
    "map.enableScrollWheelZoom(true);" +
    "var index = 0;" +
    "var myGeo = new BMap.Geocoder();" +
    "var adds = [" + alladdress +
   "];" +
    "function bdGEO() {" +
        "var add = adds[index];" +
        "geocodeSearch(add);" +
        "index++;" +
    "}" +
    "function geocodeSearch(add) {" +
        "if (index < adds.length) {" +
            "setTimeout(window.bdGEO, 400);" +
        "}" +
        "myGeo.getPoint(add, function (pt) {" +
            "if (pt) {" +
               "var address = new window.BMap.Point(pt.lng, pt.lat);" +
                "addMarker(address, new window.BMap.Label(index + \":\" + add, { offset: new window.BMap.Size(20, -10) }));" +
            "}" +
        "}, \"合肥市\");" +
       " window.map = map;" +
    "}" +

   "function addMarker(pt, label) {" +
        "var marker = new window.BMap.Marker(pt);" +
        "map.addOverlay(marker);" +
        "marker.setLabel(label);" +
    "} bdGEO();";



           

            }
            catch (Exception ex)
            {

                LoggerHelper.Error(string.Format("Dispaly GoogleMap  Country:{0}  ERR:{1}", listaddress[0].Country, ex));
            }
            return string.Empty;
        }
    }

}
