var productquantityTemplate = kendo.template($('#ProductQuantityInvTpl').html());

var invCreate = {
    Init: function () {
        var createModel = kendo.observable(new invCreate.CreateModel());
        kendo.bind($('#dvCreateInvoice'), createModel);
        createModel.loadStates();
    },

    CreateModel: function () {
        this.invDate = null;
        this.invNumer = 0;

        this.countryStates = [];
        this.stateSelected = null;

        this.invType = 'member';
        


        this.loadStates = function () {
            var self = this;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                url: '/Ordering/api/InvoiceBase/LoadStates',
                success: function (data) {
                    self.CountryStateHandler(data);
                },
                error: function () {
                    self.error = true;
                }
            });
        };

        this.CountryStateHandler = function (data) {
            if (data) {
                this.set("countryStates", data);
            }
        };

        this.doClear = function() {
        };


    }
};

$(function () {
    invCreate.Init();
});