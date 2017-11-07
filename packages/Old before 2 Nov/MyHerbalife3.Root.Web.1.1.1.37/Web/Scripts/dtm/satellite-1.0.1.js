_satellite.pushAsyncScript(function (event, target, $variables) {
    function targetPageParams() {
        var obj = {
            "entity": {}
        };
        if (_AnalyticsFacts_) {
            if (_AnalyticsFacts_.CategoryId) {
                obj.entity.categoryId = (_AnalyticsFacts_.BrowseScheme + "_" + _AnalyticsFacts_.LanguageCode + "-" + _AnalyticsFacts_.CountryCode + "_" + _AnalyticsFacts_.CategoryId);
            }
            if (_AnalyticsFacts_.ProductId) {
                obj.entity.id = (_AnalyticsFacts_.BrowseScheme + "_" + _AnalyticsFacts_.LanguageCode + "-" + _AnalyticsFacts_.CountryCode + "_" + _AnalyticsFacts_.ProductId);
            }
            if (_AnalyticsFacts_.PricedCart) {
                var cartIDS = "";
                var orderTotal = 0;
                var cart = _AnalyticsFacts_.PricedCart;
                for (i = 0; i < cart.length; i++) {
                    cartIDS += (_AnalyticsFacts_.BrowseScheme + "_" + _AnalyticsFacts_.LanguageCode + "-" + _AnalyticsFacts_.CountryCode + "_" + cart[i].Sku);
                    if (i < (cart.length - 1)) { cartIDS += ","; }
                    orderTotal += (cart[i].DiscountedPrice * cart[i].Quantity);
                }
                obj.excludedIds = ("[" + cartIDS + "]");
                obj.entity.productPurchasedId = cartIDS;
                obj.entity.orderTotal = orderTotal;
            }
            if (_AnalyticsFacts_.OrderId) {
                obj.entity.orderId = _AnalyticsFacts_.OrderId;
                obj.entity.orderMonth = _AnalyticsFacts_.OrderMonth;
                obj.entity.orderIntention = _AnalyticsFacts_.OrderIntention;
            }
        }
        return obj;
    }
    adobe.target.getOffer({
        "mbox": "hl-global-mbox",
        "success": function (response) { },
        "error": function (status, error) {
            if (console && console.log) {
                console.log(status);
                console.log(error);
            }
        },
        "timeout": 5000
    });
});
