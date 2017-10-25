_satellite.pushAsyncScript(function (event, target, $variables) {
    window.targetPageParams = function () {
        var obj = { "entity": {} };
        obj.entity.sent = "true";
        var at_type = window._AnalyticsFacts_.BrowseScheme;
        if (at_type == "DS") { at_type = "Ds"; }
        if (at_type == "MB") { at_type = "Mb"; }
        if (window._AnalyticsFacts_) {
            if (window._AnalyticsFacts_.CategoryId) {
                obj.entity.categoryId = (at_type + "_" + window._AnalyticsFacts_.LanguageCode + "-" + window._AnalyticsFacts_.CountryCode + "_" + window._AnalyticsFacts_.CategoryId);
            }
            if (window._AnalyticsFacts_.ProductId) {
                obj.entity.id = (at_type + "_" + window._AnalyticsFacts_.LanguageCode + "-" + window._AnalyticsFacts_.CountryCode + "_" + window._AnalyticsFacts_.ProductId);
            }
            if (window._AnalyticsFacts_.PricedCart) {
                var cartIDS = "";
                var orderTotal = 0;
                var cart = window._AnalyticsFacts_.PricedCart;
                for (i = 0; i < cart.length; i++) {
                    cartIDS += (at_type + "_" + window._AnalyticsFacts_.LanguageCode + "-" + window._AnalyticsFacts_.CountryCode + "_" + cart[i].Sku);
                    if (i < (cart.length - 1)) { cartIDS += ","; }
                    orderTotal += (cart[i].DiscountedPrice * cart[i].Quantity);
                }
                obj.excludedIds = ("[" + cartIDS + "]");
            }
            if (window._AnalyticsFacts_.OrderId) {
                obj.orderId = window._AnalyticsFacts_.OrderId;
                obj.orderMonth = window._AnalyticsFacts_.OrderMonth;
                obj.orderIntention = window._AnalyticsFacts_.OrderIntention;
                if (window._AnalyticsFacts_.PricedCart) {
                    obj.productPurchasedId = cartIDS;
                    obj.orderTotal = orderTotal;
                }
            }
        }
        return obj;
    }
    adobe.target.getOffer({
        "mbox": "hl-global-mbox",
        "success": function (offers) {
            adobe.target.applyOffer({
                "mbox": "hl-global-mbox",
                "offer": offers
            });
        },
        "error": function (status, error) {
            if (console && console.log) {
                console.log(status);
                console.log(error);
            }
        },
        "timeout": 5000
    });
});
