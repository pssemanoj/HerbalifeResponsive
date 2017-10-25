
var invEdit = {
    Init: function () {
        var redirect = RedirectToReceipts();

        if (redirect) {
            window.location = redirect;
            return false;
        }

        var editModel = kendo.observable(new invEdit.invoiceEditViewModel());
        kendo.bind($('#dvInvEdit'), editModel);
        editModel.loadInvoicedata();
        editModel.loadErrorMessages();
      
    },

    productquantityTemplate: function () {
        return kendo.template($('#ProductQuantityInvTpl').html());
    },
    productTotalPriceTemplate: function () { return kendo.template($('#ProductTotalPriceInvTpl').html()); },

    deleteProductTemplate: function () { return kendo.template($('#deleteProductTpl').html()); },
    selectContactTemplate: function () {
        return kendo.template($('#selectContactTpl').html());
    },
    savedCartsTemplate: function () { return kendo.template($('#savedCartsTpl').html()); },

    savedDateTemplate: function () { return kendo.template($('#savedDateTpl').html()); },

    productDescTemplate: function () {
        return kendo.template($('#productDescTpl').html());
    },
    validator: function () {
        return $("#dvInvEdit").kendoValidator({
            rules: {
                ProductValidation: function (input) {
                    if (input[0].id == "individual-products") {
                        var model = input.prop("kendoBindingTarget").source;
                        if (model.invoice.ReceiptChannel == "ClubSaleReceipt") {
                            return true;
                         }
                        if (!model.invoice.InvoiceLines || model.invoice.InvoiceLines.length == 0) {
                            return false;
                        }
                        model.set('validIndProducts', true);
                        return true;
                    } else {
                        return true;
                    }
                },
                PhoneValidation: function (input) {
                    if ( input[0].id == "phone") {
                        var model = input.prop("kendoBindingTarget").source;                       
                        if (model.invoice.Phone) {
                            if (model.invoice.Phone.length < 10 ) {
                                return false;
                            }
                        };
                        
                    }
                    return true;
                }
               
               
            },
            messages: {
                ProductValidation: function (input) {
                    var model = input.prop("kendoBindingTarget").source;
                    return model.errorMessages['reqProducts'];
                },
                PhoneValidation: function (input) {
                    var model = input.prop("kendoBindingTarget").source;
                    return model.errorMessages['Phone'];
                },
            }
        }).data("kendoValidator");
    },
    contactValidator: function () {
        return $(".customer-info").kendoValidator({
            rules: {
                PhoneValidation: function (input) {
                    if (input[0].id == "phone") {
                        var model = input.prop("kendoBindingTarget").source;
                        if (model.invoice.Phone) {
                            if (model.invoice.Phone.length < 10 || model.invoice.Phone.length > 10) {
                                return false;
                            }
                        };

                    }
                    return true;
                }
            },
            messages: {
                PhoneValidation: function (input) {
                    var model = input.prop("kendoBindingTarget").source;
                    return model.errorMessages['Phone'];
                },
            }
        }).data("kendoValidator");
    },

    invoiceEditViewModel: function () {
        this.isLoading = true;
        this.isLoaded = false;
        this.invoice = null;
        this.stateSelected = null;
        this.rootCategories = [];
        this.contacts = [];
        this.selectedRootCategoryItem = null;
        this.categories = [];
        this.contactsFilter = "";
        this.productsFilter = "";
        this.savedCartsFilter = "";
        this.autocompleteData = [];
        this.savedCarts = [];
        this.selectedProduct = null;
        this.edit_InvoiceId = window.edit_InvoiceId;
        this.edit_OrderId = window.edit_OrderId;
        this.edit_CopyInvoiceId = window.edit_CopyInvoiceId;
        this.statusSelected = "Unpaid";
        this.PaymentSelected = "Cash";
        this.memberAddressstateSelected = null;
        this.errorMessages = [];
        this.selectedAddress = null;
        this.selectedIdAddress = null;
        this.errorText = "";
        this.edit_Invoice_Action = window.edit_Invoice_Action;
        this.productSearchErrorText = "";
        this.addProductErrorText = "";
        this.contactsErrorText = "";
        this.edit_Order_Source = window.edit_Order_Source;
        this.edit_Invoice_Date = window.edit_Invoice_Date;
        this.priceAlertText = "";
        this.memberTaxEdit = false;
        this.memberShippingEdit = false;
        this.IsEditTaxClickClicked = false;
        this.IsEditHandlingFeeClicked = false;
        this.validIndProducts = false;
        this.IsClubReciept = false;
        this.CountryCode = _AnalyticsFacts_.CountryCode.toUpperCase();
        this.EnableCreateContact = (self.edit_InvoiceId > 0) ? false : true;
        this.DuplicadedAddressContactId = 0;
        this.DuplicadedPhoneDetailsId = 0;
        this.DuplicadedAddressContactAddressId = 0

        this.hideControl = function () {
            if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                return true;
            }
            else {
                return false;
            }

        };

        this.editHandlingFeeClick = function () {
            this.set('IsEditHandlingFeeClicked', true);
        };

        this.editTaxClick = function () {
            this.set('IsEditTaxClickClicked', true);
        };

        this.onMemberHandlingFeeChange = function () {
            this.invoice.set('IsMemberHandlingFeeEdited', true);
            this.calculateMemberTotal();
        };

        this.onMemberTaxChange = function () {
            this.invoice.set('IsMemberTaxEdited', true);
            this.calculateMemberTotal();
        };
        this.CreateReceiptWithWaring = function () {
            var self = this;
            if (self.invoice.isProfitable == false || self.invoice.isPurchased == false) {
                this.showwarning();
            }
            else {
                this.saveChanges();
            }
        };
        this.CreateReceipt = function () {
            if (invEdit.validator().validate()) {
                var that = this;
                if (that.invoice.Email == null && that.invoice.Phone == null  ) {
                    this.showWarningforemail();
                }
                else if (that.invoice.Email == "" && that.invoice.Phone == "")  {
                    this.showWarningforemail();
                }
                else if (that.invoice.Email == null && that.invoice.Phone == "") {
                    this.showWarningforemail();
                }
                else if (that.invoice.Email == "" && that.invoice.Phone == null) {
                    this.showWarningforemail();
                }
                else {
                    this.CreateReceiptWithWaring();

                }
            }
            else {
             if (!invEdit.validator().validateInput('#first-name')) {

                invEdit.scrollTo('#first-name');

             }
            }
        };
        this.showWarningforemail = function () {
            var wnd = $('#SendConfirmationforemail').data('kendoWindow');
            wnd.center().open();
        };
        this.showwarning = function () {
            var wnd = $('#SendConfirmation').data('kendoWindow');
            wnd.center().open();
        };
        this.CancelshowWarningforemail = function () {
            var wnd = $('#SendConfirmationforemail').data('kendoWindow');
            if (wnd) {
                wnd.close();
            }
        };
        this.OkshowWarningforemail = function () {
            var wnd = $('#SendConfirmationforemail').data('kendoWindow');
            if (wnd) {
                wnd.close();
            }
            this.CreateReceiptWithWaring();
        };
        this.CancelshowWarning = function () {
            var wnd = $('#SendConfirmation').data('kendoWindow');
            if (wnd) {
                wnd.close();
            }
        };
        this.OkshowWarning = function () {
            this.saveChanges();
        };

        this.saveChanges = function () {

            if (invEdit.validator().validate()) {
                var self = this;
                var invoiceModel = this.invoice;
                if (this.statusSelected) {
                    if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                      invoiceModel.Status = 'Closed';}
                 else  if (this.statusSelected.Value) {
                        invoiceModel.Status = this.statusSelected.Value;
                    } else {
                        invoiceModel.Status = this.statusSelected;
                    }
                }
                if (this.PaymentSelected) {
                    if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                        if (this.PaymentSelected.Value) {
                            invoiceModel.PaymentType = this.PaymentSelected.Value;
                        } else {
                            invoiceModel.PaymentType = this.PaymentSelected;
                        }

                    }
                    else {
                        invoiceModel.PaymentType = null;
                    }
                }
                if (this.stateSelected) {
                    if (this.stateSelected.Value) {
                        invoiceModel.Address.StateStatus = this.stateSelected.Value;
                    } else {
                        if (typeof (this.stateSelected) === 'object') {
                            invoiceModel.Address.State = "";
                        }
                        else {
                            invoiceModel.Address.State = this.stateSelected;
                        }                            
                    }
                }
                if (invoiceModel.InvoiceDate) {
                    invoiceModel.InvoiceDate = new Date(moment(invoiceModel.InvoiceDate));
                }

                $.ajax({
                    url: '/Ordering/api/InvoiceEdit/Post',
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(invoiceModel),

                    success: function (data) {
                        if (data) {

                            if (!data.IsSuccess) {
                                self.set('errorText', self.errorMessages[data.ErrorCodeKey]);
                            } else {
                                self.set('errorText', "");
                                self.setInvoiceData(data.InvoiceModel);
                                window.location.href = "/Ordering/Invoice/Display/" + self.invoice.Id;
                            }
                        }
                    },
                    error: function (data) {

                    },
                    beforeSend: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                        kendo.ui.progress($("#dvinvPricing"), true);
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                        kendo.ui.progress($("#dvinvPricing"), false);
                    }
                });
            } else {

                var input = $('#individual-products');
                var model = input.prop("kendoBindingTarget").source;

                if (!invEdit.validator().validateInput('#status')) {

                    invEdit.scrollTo('[for="status"]');

                } else if (!invEdit.validator().validateInput('#first-name')) {

                    invEdit.scrollTo('#first-name');

                }
                else if (!invEdit.validator().validateInput('#phone')) {

                    invEdit.scrollTo('#phone');

                }
                else if (!model.get('validIndProducts')) {

                    invEdit.scrollTo("#individual-products");

                } else if (this.invoice.InvoiceShipToAddress) {
                    $.ajax({
                        url: '/Ordering/api/InvoiceEdit/Post',
                        type: 'POST',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify(invoiceModel),

                        success: function (data) {
                            if (data) {

                                if (!data.IsSuccess) {
                                    self.set('errorText', self.errorMessages[data.ErrorCodeKey]);
                                } else {
                                    self.set('errorText', "");
                                    self.setInvoiceData(data.InvoiceModel);
                                    window.location.href = "/Ordering/Invoice/Display/" + self.invoice.Id;
                                }
                            }
                        },
                        error: function (data) {

                        },
                        beforeSend: function () {
                            kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                            kendo.ui.progress($("#dvinvPricing"), true);
                        },
                        complete: function () {
                            kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                            kendo.ui.progress($("#dvinvPricing"), false);
                        }
                    });
                }
            }
        };

        this.invoiceAddressClick = function (e) {
            e.preventDefault();
            this.invoice.set("InvoiceShipToAddress", this.invoice.InvoiceShipToAddress);
            this.calculateBasePrice();
        };

        this.invoiceTypeClick = function (e) {

            var grid = $('#dgProducts').data('kendoGrid');

            
            this.invoice.set('InvoicePrice', new invEdit.InvoicePriceModel());
            
            if (this.invoice.Type == "Distributor") {
                grid.showColumn("EarnBase");
                this.invoice.set('IsGdoMemberPricing', true);
                this.invoice.InvoicePrice.set("DiscountAmount", 0);
                this.invoice.InvoicePrice.set("DiscountPercentage", 25);
                this.invoice.InvoicePrice.set("TaxAmount", 0);
                this.invoice.InvoicePrice.set("TaxPercentage", 0);
                this.invoice.InvoicePrice.set("ShippingAmount", 0);
                this.invoice.InvoicePrice.set("ShippingPercentage", 0);
            } else {
                grid.hideColumn("EarnBase");
                this.invoice.set('IsGdoMemberPricing', false);
                this.invoice.InvoicePrice.set("DiscountAmount", 0);
                this.invoice.InvoicePrice.set("TaxAmount", 0);
                this.invoice.InvoicePrice.set("TaxPercentage", 0);
                this.invoice.InvoicePrice.set("DiscountPercentage", 0);
                this.invoice.InvoicePrice.set("ShippingAmount", 0);
                this.invoice.InvoicePrice.set("ShippingPercentage", 0);
            }
        };
        this.togglePricing = function (e) {
            var that = this;
            if (this.invoice.ReceiptChannel == "ClubSaleReceipt") {
                that.set("IsClubReciept", true);
                $("#lbldescription").attr('required', 'required');
                $("#txtVP").attr('required', 'required');
                $("#lbltotalprice").attr('required', 'required');
                $("#txtQTY").attr('required', 'required');
            }
            else {
                that.set("IsClubReciept", false);
                $("#lbldescription").removeAttr('required');
                $("#txtVP").removeAttr('required');
                $("#lbltotalprice").removeAttr('required');
                $("#txtQTY").removeAttr('required');

            }            
        };
        this.PaymentTypeChange = function (e) {
            var self = this;
            if ($("#Payment").val() != null) {
                var x =$("#Payment").val();
                self.set("PaymentType", x);
            }
        };
        this.CountryChange = function (e) {
            var self = this;
            if ($("#Country").val() != null) {
                var Country = $("#Country").val();
                self.set("invoice.Address.Country", Country);
            }
        };

        this.togglePurchased = function (e) {
            var that = this;
            if ($("#Purchased").val() === "Purchased") {
                that.set("isPurchased", true);
            }
            else {
                that.set("isPurchased", false);
            }

        };
        this.toggleProfitable = function (e) {
            var that = this;
            if ($("#Profitable").val() === "Profitable") {
                that.set("isProfitable", true);
            }
            else {
                that.set("isProfitable", false);
            }

        };
       
        this.onInvoiceLinesDataBinding = function (e) {
            var grid = e.sender;

            if (this.invoice.Type == "Distributor") {
                grid.showColumn("EarnBase");
            } else {
                grid.hideColumn("EarnBase");
            }
        };
        this.calculateMemberTotal = function () {
            var self = this;

            var invoiceModel = this.invoice;
            $.ajax({
                url: '/Ordering/api/InvoiceEdit/CalculateMemberTotal',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(invoiceModel),

                success: function (data) {
                    if (data) {
                        self.setInvoiceData(data);
                    }
                },
                error: function (data) {

                },
                beforeSend: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                }
            });
        };

        this.calculateBasePrice = function () {
            var self = this;
            $("#priceInfoText").removeClass("red");
            $("#priceInfoText").addClass("red");
            var invoiceModel = this.invoice;
            $.ajax({
                url: '/Ordering/api/InvoiceEdit/CalculateBasicPrice',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(invoiceModel),

                success: function (data) {
                    if (data) {
                        self.setInvoiceData(data);
                    }
                },
                error: function (data) {

                },
                beforeSend: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                }

            });
        };
        this.CheckemailAndphone = function () {
            var self = this;
            $("#priceInfoText").removeClass("red");
            $("#priceInfoText").addClass("red");
            var invoiceModel = this.invoice;
            if (invoiceModel.Email || invoiceModel.Phone) {

                $.ajax({
                    url: '/Ordering/api/InvoiceEdit/CheckEmailAndPhone',
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(invoiceModel),

                    success: function (data) {
                        self.set("isValidEmailAndPhone", !data);
                        invoiceModel.set("ValidEmailAndPhone", !data);
                        if (!data) {
                            $("#btnCreateReceipt").attr('disabled', true);
                        }
                        else {
                            
                            self.CreateContact();
                        }

                    },
                    error: function (data) {

                    },
                    beforeSend: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                    }
                });
            }
            else {
                self.CreateContact();
            }
            
        };
        this.CheckemailAndphoneForBrowseContact = function () {
            var self = this;
            $("#priceInfoText").removeClass("red");
            $("#priceInfoText").addClass("red");
            var invoiceModel = this.invoice;
            if (invoiceModel.Email || invoiceModel.Phone) {

                $.ajax({
                    url: '/Ordering/api/InvoiceEdit/CheckEmailAndPhone',
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(invoiceModel),

                    success: function (data) {
                        self.set("isValidEmailAndPhone", !data);
                        invoiceModel.set("ValidEmailAndPhone", !data);
                        if (!data) {
                            $("#btnCreateReceipt").attr('disabled', true);
                        }
                        else {

                            self.set("EnableCreateContact", false);
                        }

                    },
                    error: function (data) {

                    },
                    beforeSend: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                    }
                });
            }
        };

        this.calculateTotalDue = function () {
            var self = this;
            $("#priceInfoText").removeClass("red");
            $("#priceInfoText").addClass("red");
            var invoiceModel = this.invoice;
            $.ajax({
                url: '/Ordering/api/InvoiceEdit/CalculateTotalDue',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(invoiceModel),

                success: function (data) {
                    if (data) {
                        self.setInvoiceData(data);
                    }
                },
                error: function (data) {

                },
                beforeSend: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                }
            });
        };


        this.price = function () {
            $("#priceInfoText").removeClass("red");
            
            if (invEdit.validator().validate()) {
                var self = this;
                var invoiceModel = this.invoice;

                if (this.stateSelected) {
                    if (this.stateSelected.Value) {
                        invoiceModel.Address.StateStatus = this.stateSelected.Value;
                    } else {
                        if (typeof (this.stateSelected) === 'object') {
                            invoiceModel.Address.State = "";
                        }
                        else {
                            invoiceModel.Address.State = this.stateSelected;
                        }
                    }
                }

                $.ajax({
                    url: '/Ordering/api/InvoiceEdit/Price',
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(invoiceModel),

                    success: function (data) {
                        if (data) {
                            
                            if (!data.IsSuccess) {
                                self.set('errorText', self.errorMessages[data.ErrorCodeKey]);
                                self.invoice.InvoicePrice.set('ShippingAmount', 0);
                                self.invoice.InvoicePrice.set('TaxAmount', 0);
                                self.invoice.InvoicePrice.set('DisplayShipping', data.InvoiceModel.InvoicePrice.DisplayShipping);
                                self.invoice.InvoicePrice.set('DisplayCalculatedTax', data.InvoiceModel.InvoicePrice.DisplayCalculatedTax);
                                self.invoice.InvoicePrice.set('DisplayTotalDue', data.InvoiceModel.InvoicePrice.DisplaySubtotal);
                            } else {
                                self.set('errorText', "");
                                self.setInvoiceData(data.InvoiceModel);
                                if (data.InvoiceModel) {
                                    self.invoice.set('ResetCustomerTaxValue', false);
                                }
                                if (data.InvoiceModel.Address && data.InvoiceModel.Address.State) {
                                    self.set('stateSelected', { Value: data.InvoiceModel.Address.State });
                                }
                            }
                        }
                    },
                    error: function (data) {

                    },
                    beforeSend: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                        kendo.ui.progress($("#dvinvPricing"), true);
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                        kendo.ui.progress($("#dvinvPricing"), false);
                    }
                });
            } else {
                var input = $('#individual-products');
                var model = input.prop("kendoBindingTarget").source;

                if (!invEdit.validator().validateInput('#status')) {

                    invEdit.scrollTo('[for="status"]');

                } else if (!invEdit.validator().validateInput('#first-name')) {

                    invEdit.scrollTo('#first-name');

                } else if (!invEdit.validator().validateInput('#street-address')) {

                    invEdit.scrollTo("#street-address");

                } else if (!invEdit.validator().validateInput('#city')) {

                    invEdit.scrollTo("[for='city']");

                } else if (!invEdit.validator().validateInput('#zip-code')) {

                    invEdit.scrollTo("[for='zip-code']");

                } else if (!model.get('validIndProducts')) {

                    invEdit.scrollTo("#individual-products");

                }
            }

        };

        this.cancel = function () {
            window.location.href = "/Ordering/Invoice";
            return false;
        };

        this.ChangeClicked = function () {
            kendo.ui.progress($("#loading"), true);
            var wnd = $('#dvMemberAddress').data('kendoWindow');
            wnd.center().open();
            this.memberAddressDataSource.read();
            kendo.ui.progress($("#loading"), false);
        };

        this.onChangeAddress = function () {
            var self = this;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                data: JSON.stringify(self.selectedAddress),
                url: '/Ordering/api/InvoiceEdit/FilterShippingAddress/' + self.selectedIdAddress,
                success: function (data) {
                    self.set("selectedAddress", data);
                },
                error: function () {
                    self.error = true;
                },
                beforeSend: function () {
                    kendo.ui.progress($("#changeAddressDetails"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#changeAddressDetails"), false);
                }
            });
        };

        this.AddressChanged = function () {
            var self = this;
            if (self.invoice) {
                self.invoice.MemberAddress = self.selectedAddress;
            }
            var wnd = $('#dvMemberAddress').data('kendoWindow');
            wnd.center().close();
            self.calculateBasePrice();
        };

        this.onQuantityChange = function () {
            this.calculateBasePrice();
        };
        this.CheckEmailAndPhone = function () {
            this.CheckemailAndphone();
        };

        this.autocompleteDataSource = new kendo.data.DataSource({
            type: "json",
            serverFiltering: true,
            serverPaging: true,
            pageSize: 20,
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceBase/LoadProductsForAutocomplete"
                },
                parameterMap: function () {
                    return {
                        startsWith: $("#individual-products").data("kendoAutoComplete").value(),
                        type: $('#taxAmount').prop("kendoBindingTarget").source.invoice.Type
                    };
                }
            }
        });

        this.statusDataSource = new kendo.data.DataSource({
            type: "json",
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceBase/LoadStatus"
                }
            }
        });
        this.PaymentTypeDataSource = new kendo.data.DataSource({
            type: "json",
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceBase/LoadPaymentType"
                }
            }
        });

        this.statesDataSource = new kendo.data.DataSource({
            type: "json",
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceBase/LoadStates"
                }
            }
        });

        this.memberAddressDataSource = new kendo.data.DataSource({
            type: "json",
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceEdit/GetShippingAddress"
                }
            }
        });

        this.contactsDataSource = new kendo.data.DataSource({
            type: "json",
            serverFiltering: false,
            serverPaging: false,
            pageSize: 10,
            pageable: true,
            transport: {
                read: {
                    url: '/Ordering/api/InvoiceSearch/GetCRMAddress',
                    type: "GET"
                }
            },
            schema: {
                data: function (data) {
                    if (data && data.length > 0) {
                        for (var i = 0; i < data.length; i++) {
                            if (data[i].Address) {
                                data[i].FullAddressDisplay = (data[i].Address.Address1 || '') + ',' + (data[i].Address.Address2 || '') + ',' + (data[i].Address.City || '') + ',' + (data[i].Address.State || '') + ',' + (data[i].Address.PostalCode || '');
                            }
                            else {
                                data[i].FullAddressDisplay = '';
                            }
                        }
                        return data;
                    } else {
                        return null;
                    }
                },
                total: function (data) {
                    if (data) {
                        return data.length;
                    } else {
                        return 0;
                    }
                }
            },
            requestStart: function (e) {
                kendo.ui.progress($("#dgContacts"), true);
            },
            requestEnd: function (e) {
                kendo.ui.progress($("#dgContacts"), false);
            }
        });

        this.discountsDataSource = new kendo.data.DataSource({
            type: "json",
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceEdit/GetMemberDiscounts"
                }
            }
        });

        this.loadErrorMessages = function () {
            if (this.edit_Invoice_Action == "Invoice_Edit") {
                var self = this;
                $.ajax({
                    type: 'get',
                    dataType: 'json',
                    cache: true,
                    url: '/Ordering/api/InvoiceEdit/LoadErrorMessage',
                    success: function (data) {
                        if (data) {
                            self.set("errorMessages", data);
                        }
                    },
                    error: function () {
                        self.error = true;
                    },
                    complete: function () {
                    }
                });
            }
        };

        this.loadInvoicedata = function () {
            var self = this;
            if (this.edit_Invoice_Action == "Invoice_Edit") {
                if (self.edit_InvoiceId == 0 && self.edit_OrderId != "" && self.edit_CopyInvoiceId == 0) {
                    //Came from MyOrders
                    var createInvoiceFromOrderIdRequest = {
                        OrderId: self.edit_OrderId,
                        Source: self.edit_Order_Source
                    };
                    $.ajax({
                        url: "/Ordering/api/InvoiceEdit/GetInvoiceFromOrderId",
                        type: 'POST',
                        data: JSON.stringify(createInvoiceFromOrderIdRequest),
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            self.setInvoiceData(data);
                            self.set('statusSelected', { Value: data.Status });
                            self.set('stateSelected', { Value: data.Address.State });
                            self.set('PaymentSelected', { Value: data.PaymentType });
                            self.loadShippingAddress();
                            self.togglePricing();
                        },
                        error: function (data) {
                        },
                        beforeSend: function () {
                            kendo.ui.progress($("#loading"), true);
                        },
                        complete: function () {
                            kendo.ui.progress($("#loading"), false);
                        }
                    });
                    // get the id from edit_InvoiceId and load the invoice for the id

                } else if (self.edit_InvoiceId > 0 && self.edit_OrderId == "" && self.edit_CopyInvoiceId == 0) {
                    var invoiceId = self.edit_InvoiceId;
                    $.ajax({
                        type: 'get',
                        dataType: 'json',
                        cache: true,
                        data: JSON.stringify(invoiceId),
                        url: '/Ordering/api/InvoiceSearch/Load/' + invoiceId,
                        success: function (data) {
                            if (data && data.Id > 0) {
                                self.setInvoiceData(data);
                                self.set('statusSelected', { Value: data.Status });
                                self.set('stateSelected', { Value: data.Address.State });
                                self.set('PaymentSelected', { Value: data.PaymentType });
                                self.togglePricing();
                            }
                            else {
                                window.location.href = "/Ordering/Invoice";
                                return false;
                            }
                        },
                        error: function () {
                            self.error = true;
                        },
                        beforeSend: function () {
                            kendo.ui.progress($("#loading"), true);
                        },
                        complete: function () {
                            kendo.ui.progress($("#loading"), false);
                        }
                    });
                    // get the id from edit_InvoiceId and load the invoice for the id
                } else if (self.edit_InvoiceId == 0 && self.edit_OrderId == "" && self.edit_CopyInvoiceId > 0) {
                    //Copy Invoice
                    var invoiceId = self.edit_CopyInvoiceId;
                    $.ajax({
                        type: 'get',
                        dataType: 'json',
                        cache: true,
                        data: JSON.stringify(invoiceId),
                        url: '/Ordering/api/InvoiceEdit/Copy/' + invoiceId,
                        success: function (data) {
                            if (data) {
                                self.setInvoiceData(data);
                                self.set('statusSelected', { Value: data.Status });
                                self.set('stateSelected', { Value: data.Address.State });
                                self.set('PaymentSelected', { Value: data.PaymentType });
                                self.togglePricing();
                            }
                        },
                        error: function () {
                            self.error = true;
                        },
                        beforeSend: function () {
                            kendo.ui.progress($("#loading"), true);
                        },
                        complete: function () {
                            kendo.ui.progress($("#loading"), false);
                        }
                    });
                } else {

                    self.set('invoice', new invEdit.InvoiceModel());
                    self.invoice.set('InvoiceDate', new Date(moment(self.edit_Invoice_Date)));
                    $.ajax({
                        type: 'get',
                        dataType: 'json',
                        cache: true,
                        url: '/Ordering/api/InvoiceEdit/LoadMemberAddress',
                        success: function (data) {
                            if (data) {
                                self.invoice.set('MemberAddress', data);
                                self.invoice.set('HasShippingAddress', true);
                                
                            }
                            if (data == null) {
                                self.invoice.set('InvoiceShipToAddress', true);
                                self.invoice.set('HasShippingAddress', false);
                            }
                        },
                        error: function () {
                            self.error = true;
                        },
                        beforeSend: function () {
                            kendo.ui.progress($("#dvinvProduct"), true);
                        },
                        complete: function () {
                            kendo.ui.progress($("#dvinvProduct"), false);
                        }
                    });
                }
            };
           
        };

        this.setInvoiceData = function (data) {
            if (data) {
                this.set('invoice', data);
                this.invoice.set('InvoiceDate', new Date(moment(data.InvoiceDate)));
                this.set('stateSelected', { Value: data.Address.State });
            }
        };

        this.onRootCategoryChange = function () {
            if (this.selectedRootCategoryItem) {
                this.loadCategories(this.selectedRootCategoryItem.Id);
                var pager = $("#pager").data("kendoPager");
                pager.page(1);
            }
        };

        this.dataBound = function (e) {
            e.sender.expandRow(e.sender.tbody.find("tr.k-master-row").first());
        };

        this.contactsDataBound = function (e) {
            kendo.ui.progress($("#dgContacts"), false);
            if (this.contactsDataSource && this.contactsDataSource._view.length == 0) {
               this.set('contactsErrorText', this.errorMessages['contactsError']);
            } else {
                this.set('contactsErrorText', '');
            }
        };

        this.clickContacts = function () {
            kendo.ui.progress($("#loading"), true);
            var wnd = $('#dvInvContacts').data('kendoWindow');
            wnd.center().open();
            this.contactsDataSource.read();
            kendo.ui.progress($("#loading"), false);
        };

        this.clickProducts = function () {
            kendo.ui.progress($("#loading"), true);
            this.loadRootCategories();
            var wnd = $('#dvInvProducts').data('kendoWindow');
            wnd.center().open();
            kendo.ui.progress($("#loading"), false);
        };

        this.clickSavedCarts = function () {
            kendo.ui.progress($("#loading"), true);
            this.loadSavedCarts();
            var wnd = $('#dvSavedCarts').data('kendoWindow');
            wnd.center().open();
            kendo.ui.progress($("#loading"), false);
        };

        this.addProducts = function () {
            kendo.ui.progress($("#loading"), true);
            var self = this;
            var selectedProduct = this.get("selectedProduct");

            var skuAdded = false;
            if (null != selectedProduct && null != selectedProduct.Sku) {
                for (var i = 0; i < self.invoice.InvoiceLines.length; i++) {
                    if (self.invoice.InvoiceLines[i].Sku == selectedProduct.Sku) {
                        self.invoice.InvoiceLines[i].set("Quantity", self.invoice.InvoiceLines[i].Quantity + selectedProduct.Quantity);
                        skuAdded = true;
                        break;
                    }
                }

                if (!skuAdded && selectedProduct != null && null != selectedProduct.Sku) {
                    self.invoice.InvoiceLines.push(selectedProduct);
                }

                if (selectedProduct != null && null != selectedProduct.Sku) {
                    self.set("selectedProduct", null);
                    self.calculateBasePrice();
                    invEdit.validator().hideMessages();
                    $("#individual-products").removeClass("k-invalid");
                }
                self.set('addProductErrorText', '');
            } else {
                self.set('addProductErrorText', self.errorMessages['productSearchError']);
            }
            kendo.ui.progress($("#loading"), false);
        };


        this.deleteProducts = function (e) {
            var self = this;
            self.invoice.InvoiceLines.remove(e.data);
            self.calculateBasePrice();
        };


        this.onProductChange = function () {

            var selectedProduct = this.get("selectedProduct");
            alert(selectedProduct.Sku);
            alert(kendo.stringify(selectedProduct));
        };

        this.loadRootCategories = function () {
            var self = this;

            $.when(
                $.ajax({
                    type: 'get',
                    dataType: 'json',
                    cache: true,
                    url: '/Ordering/api/InvoiceBase/LoadRootCategories',
                    success: function (data) {
                        self.set("rootCategories", data);
                        if (data) {
                            self.set("selectedRootCategoryItem", data[0]);
                        }
                    },
                    error: function () {
                        self.error = true;
                    },
                    beforeSend: function () {
                        kendo.ui.progress($("#products-search"), true);
                    },
                    complete: function () {
                        kendo.ui.progress($("#products-search"), false);
                    }
                })).then(function () {
                    if (self.selectedRootCategoryItem) {
                        self.loadCategories(self.selectedRootCategoryItem.Id);
                    }
                    kendo.ui.progress($("#products-search"), false);
                }
            );
        };

        this.loadCategories = function (id) {
            var self = this;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                url: '/Ordering/api/InvoiceBase/LoadCategories/' + id +'/'+self.invoice.Type,
                success: function (data) {
                    self.set("categories", data);
                },
                error: function () {
                    self.error = true;
                },
                beforeSend: function () {
                    kendo.ui.progress($("#lvProducts"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#lvProducts"), false);
                }
            });
        };

        this.selectCategory = function (e) {
            e.preventDefault();
            var self = this;
            var sku = $(e.target).data('parameter');

            var addedProduct = null;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                async: true,
                url: '/Ordering/api/InvoiceBase/GetInvoiceLineModel/' + sku + '/' + self.invoice.Type,
                success: function (data) {
                    if (data) {
                        addedProduct = data;
                        var skuAdded = false;
                        if (null != addedProduct) {

                            if (null == self.invoice) {
                                self.set('invoice', new invEdit.InvoiceModel());
                            }

                            for (var i = 0; i < self.invoice.InvoiceLines.length; i++) {
                                if (self.invoice.InvoiceLines[i].Sku == sku) {
                                    self.invoice.InvoiceLines[i].set("Quantity", self.invoice.InvoiceLines[i].Quantity + addedProduct.Quantity);
                                    skuAdded = true;
                                    break;
                                }
                            }

                            if (!skuAdded) {
                                self.invoice.InvoiceLines.push(addedProduct);
                            }
                            //var productsWnd = $('#dvInvProducts').data('kendoWindow');
                            //if (productsWnd) {
                            //productsWnd.close();
                            //}
                            self.calculateBasePrice();
                            invEdit.validator().hideMessages();
                            $("#individual-products").removeClass("k-invalid");
                        }
                    }
                },
                error: function () {

                },
                beforeSend: function () {
                    kendo.ui.progress($("#lvProducts"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#lvProducts"), false);
                }
            });

        };

        this.selectContact = function (e) {
            e.preventDefault();
            var invoiceCRMConatactModel = new invEdit.InvoiceCRMConatactModel();
            if (this.invoice) {
                this.invoice.set("FirstName", e.data.FirstName);
                this.invoice.set("FirstName", e.data.FirstName);
                this.invoice.set("LastName", e.data.LastName);
                this.invoice.set("Phone", e.data.PhoneDetail.PhoneNumber);
                this.invoice.set("Email", e.data.EmailDetail.EmailAddress);
                if (this.invoice.Address && e.data.Address) {
                    this.invoice.Address.set("Address1", e.data.Address.Address1);
                    this.invoice.Address.set("City", e.data.Address.City);
                    this.invoice.Address.set("State", e.data.Address.State);
                    this.set('stateSelected', e.data.Address.State);
                    this.invoice.Address.set("PostalCode", e.data.Address.PostalCode);
                    this.set("CountryCode", e.data.Address.Country);
                    invoiceCRMConatactModel.Address.Id = e.data.Address.Id;
                }
                else {
                    this.set("CountryCode", _AnalyticsFacts_.CountryCode.toUpperCase());
                }               
                invoiceCRMConatactModel.ContactId = e.data.ContactId;
                this.invoice.set("CustomerId", e.data.ContactId);
                invoiceCRMConatactModel.EmailDetail.Id = e.data.EmailDetail.Id;
                invoiceCRMConatactModel.PhoneDetail.Id = e.data.PhoneDetail.Id;
                this.set('InvoiceCRMConatactModel', invoiceCRMConatactModel);
                this.CheckemailAndphoneForBrowseContact();
            }

            var contactsWnd = $('#dvInvContacts').data('kendoWindow');
            if (contactsWnd) {
                contactsWnd.close();
            }
        };

        this.selectSavedCarts = function (e) {
            e.preventDefault();
            var self = this;
            var cartId = e.data.Id;
            $.ajax({
                url: "/Ordering/api/InvoiceEdit/GetInvoiceFromCartId",
                type: 'POST',
                data: { "": cartId },

                success: function (data) {
                    self.setInvoiceData(data);

                    var savedCartsWnd = $('#dvSavedCarts').data('kendoWindow');
                    if (savedCartsWnd) {
                        savedCartsWnd.close();
                    }
                },
                error: function (data) {
                },
                beforeSend: function () {
                    kendo.ui.progress($("#loading"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#loading"), false);
                }
            });
        };

        this.searchSavedCarts = function () {
            var self = this;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                data: JSON.stringify(this.savedCartsFilter),
                url: '/Ordering/api/InvoiceEdit/FilterSavedCarts/' + this.savedCartsFilter,
                success: function (data) {
                    self.set("savedCarts", data);
                },
                error: function () {
                    self.error = true;
                },
                beforeSend: function () {
                    kendo.ui.progress($("#dgSavedCarts"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dgSavedCarts"), false);
                }
            });
        };

        this.loadSavedCarts = function () {
            var self = this;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                url: '/Ordering/api/InvoiceEdit/LoadSavedCarts',
                success: function (data) {
                    if (data) {
                        self.set("savedCarts", data);
                    }
                },
                error: function () {
                    self.error = true;
                },
                beforeSend: function () {
                    kendo.ui.progress($("#dgSavedCarts"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dgSavedCarts"), false);
                }
            });
        };

        this.SearchContacts = function () {

            this.contactsDataSource.filter([
                {
                    "logic": "or",
                    "filters": [
                        {
                            "field": "FirstName",
                            "operator": "startswith",
                            "value": this.contactsFilter
                        },
                        {
                            "field": "LastName",
                            "operator": "startswith",
                            "value": this.contactsFilter
                        }
                    ]
                }
            ]);
        };


        this.SearchProducts = function () {
            var self = this;

            var invoiceCategoryFilter = {
                RootCategoryId: this.selectedRootCategoryItem.Id,
                Filter: this.productsFilter,
                Type: this.invoice.Type
            };
            $.ajax({
                url: '/Ordering/api/InvoiceBase/SearchCategories',
                type: 'POST',
                data: JSON.stringify(invoiceCategoryFilter),
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data && data.length > 0) {
                        self.set('productSearchErrorText', '');
                    } else {
                        self.set('productSearchErrorText', self.errorMessages['productSearchError']);
                    }
                    self.set("categories", data);
                },
                error: function (data) {
                },
                beforeSend: function () {
                    kendo.ui.progress($("#lvProducts"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#lvProducts"), false);
                }
            });
        };

        this.loadShippingAddress = function () {
            var self = this;
            if (this.edit_Invoice_Action == "Invoice_Edit") {
                $.ajax({
                    type: 'get',
                    dataType: 'json',
                    cache: true,
                    url: '/Ordering/api/InvoiceEdit/LoadMemberAddress',
                    success: function (data) {
                        if (data) {
                            self.invoice.set('MemberAddress', data);
                            self.invoice.set('HasShippingAddress', true);

                        }
                        if (data == null) {
                            self.invoice.set('InvoiceShipToAddress', true);
                            self.invoice.set('HasShippingAddress', false);
                        }
                    },
                    error: function () {
                        self.error = true;
                    },
                    beforeSend: function () {
                        kendo.ui.progress($("#dvinvProduct"), true);
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvinvProduct"), false);
                    }
                });
            }
        };

        this.onProdcutsPageChange = function (e) {
            var listview = $("#lvProducts").data("kendoListView");
            listview.dataSource.page(e.sender.page());
        };

        this.onShippingPercentageChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("ShippingAmount", 0);
                if (!(_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT")) {
                    this.invoice.InvoicePrice.set("TaxAmount", 0);
                    this.invoice.InvoicePrice.set("TaxPercentage", 0);
                }
                $("#shippingPercentage").prev().removeClass("k-state-disabled");
                $("#shippingAmount").prev().addClass("k-state-disabled");
                this.invoice.set("IsCustomerShippingHandlingEdited", true);
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.price();
                }
            }
        };

        this.onShippingAmountChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("ShippingPercentage", 0);
                if (!(_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT")) {
                    this.invoice.InvoicePrice.set("TaxAmount", 0);
                    this.invoice.InvoicePrice.set("TaxPercentage", 0);
                }
                $("#shippingAmount").prev().removeClass("k-state-disabled");
                $("#shippingPercentage").prev().addClass("k-state-disabled");
                this.invoice.set("IsCustomerShippingHandlingEdited", true);
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.price();
                }
            }
        };

        this.onDiscountPercentageChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("DiscountAmount", 0);
                if (!(_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT")) {
                    this.invoice.InvoicePrice.set("TaxAmount", 0);
                    this.invoice.InvoicePrice.set("TaxPercentage", 0);
                }
                $("#discountPercentage").prev().removeClass("k-state-disabled");
                $("#discountAmount").prev().addClass("k-state-disabled");
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.price();
                } else {

                    this.calculateTotalDue();
                }
            }
        };

        this.onDiscountDropDownPercentageChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("DiscountAmount", 0);
                this.invoice.InvoicePrice.set("TaxAmount", 0);
                this.invoice.InvoicePrice.set("TaxPercentage", 0);

                this.calculateTotalDue();
            }
        };

        this.onStateChange = function () {
            if (this.invoice && this.invoice.Address && this.invoice.Address.State) {
                this.set('stateSelected', this.invoice.Address.State);
            }
        };

        this.onDiscountAmountChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("DiscountPercentage", 0);
                if (!(_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT")) {
                    this.invoice.InvoicePrice.set("TaxAmount", 0);
                    this.invoice.InvoicePrice.set("TaxPercentage", 0);
                }
                $("#discountAmount").prev().removeClass("k-state-disabled");
                $("#discountPercentage").prev().addClass("k-state-disabled");

                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.price();
                } else {

                    this.calculateTotalDue();
                }
            }
        };

        this.onTaxPercentageChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("TaxAmount", 0);
                $("#taxPercentage").prev().removeClass("k-state-disabled");
                $("#taxAmount").prev().addClass("k-state-disabled");
                this.invoice.set("IsCustomerTaxEdited", true);
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.price();
                }
            }
        };

        this.onTaxAmountChange = function () {

            if (this.invoice && this.invoice.InvoicePrice) {
                this.invoice.InvoicePrice.set("TaxPercentage", 0);
                $("#taxAmount").prev().removeClass("k-state-disabled");
                $("#taxPercentage").prev().addClass("k-state-disabled");
                this.invoice.set("IsCustomerTaxEdited", true);
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.price();
                }
            }
        };
        this.resetToDefaultValueClicked = function () {
            if (this.invoice) {
                this.invoice.set("ResetCustomerTaxValue", true);
                this.calculateBasePrice();
            }
        };

        this.launchShippingHandling = function () {
            var myWin = $("#dvPlg");
            if (!myWin.data("kendoWindow")) {
                var pUpCntr = $('<div />').attr({ 'id': 'dvPlg', 'class': 'pnlEstimateSH' }).appendTo(document.body);
                var options = {
                    content: '/Ordering/Invoice/HtmlFragment?path=lblShippingHandlingTable.html',
                    actions: ["Close"],
                    modal: true,
                    draggable: true,
                    resizable: true,
                    visible: false,
                    height: 685,
                    width: 591
                };
                var windCntr = pUpCntr.kendoWindow(options);
                windCntr.data("kendoWindow").open().center();
            } else {
                myWin.data("kendoWindow").open().center();
            }

        };

        this.duplicateContactsDataSource = new kendo.data.DataSource({});

        this.CreateContact = function () {
            var that = this;
            if (invEdit.contactValidator().validate()) {

                if (!invEdit.contactValidator().validateInput('#first-name')) {

                    invEdit.scrollTo('#first-name');

                } else if (!invEdit.contactValidator().validateInput('#phone')) {

                    invEdit.scrollTo('#phone');

                }
                 else if (!invEdit.contactValidator().validateInput('#email')) {

                     invEdit.scrollTo('#email');

                }else {

                    // TODO - Perform Request to Save/Edit contact, and open modal if duplicated, if is new, then just do this.set("EnableCreateContact", false);
                    var invoiceCRMConatactModel = that.get('InvoiceCRMConatactModel');
                    if (!invoiceCRMConatactModel) {
                        invoiceCRMConatactModel = new invEdit.InvoiceCRMConatactModel();
                    }
                    invoiceCRMConatactModel.FirstName = that.invoice.FirstName;
                    invoiceCRMConatactModel.LastName = that.invoice.LastName;
                    invoiceCRMConatactModel.EmailDetail.EmailAddress = that.invoice.Email;
                    invoiceCRMConatactModel.PhoneDetail.PhoneNumber = that.invoice.Phone;
                    invoiceCRMConatactModel.Address.Address1 = that.invoice.Address.Address1;
                    invoiceCRMConatactModel.Address.Address2 = that.invoice.Address.Address2;
                    invoiceCRMConatactModel.Address.City = that.invoice.Address.City;
                    invoiceCRMConatactModel.Address.State = that.invoice.Address.State;
                    invoiceCRMConatactModel.Address.Country = that.get("CountryCode");
                    invoiceCRMConatactModel.Address.PostalCode = that.invoice.Address.PostalCode;
                    // set Phone , email , adresss to invoiceCRMConatactModel
                    invoiceCRMConatactModel.IsDuplicateCheckRequire = invoiceCRMConatactModel.ContactId && invoiceCRMConatactModel.ContactId > 0 ? false : true;
                    that.set('InvoiceCRMConatactModel', invoiceCRMConatactModel);

                    $.ajax({
                        url: '/Ordering/api/InvoiceSearch/SaveUpdate',
                        type: 'POST',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify(invoiceCRMConatactModel),

                        success: function (response) {
                            console.log(response);
                            if (response.DuplicatedContacts) {
                                var duplicateContactsDataSource = that.get('duplicateContactsDataSource');
                                duplicateContactsDataSource.data(response.DuplicatedContacts);
                                $("#btnOverWrite").attr('disabled', true);
                                var wnd = $('#modal-duplicate-contact').data('kendoWindow');
                                wnd.center().open();
                            }
                            else {
                                if (response.Data && response.Data.ContactId > 0) {
                                    invoiceCRMConatactModel.ContactId = response.Data.ContactId;
                                    that.invoice.CustomerId = response.Data.ContactId;
                                    invoiceCRMConatactModel.Address.Id = response.Data.Address && response.Data.Address.Id > 0 ? response.Data.Address.Id : 0;
                                    invoiceCRMConatactModel.EmailDetail.Id = response.Data.EmailDetail && response.Data.EmailDetail.Id > 0 ? response.Data.EmailDetail.Id : 0;  
                                    invoiceCRMConatactModel.PhoneDetail.Id = response.Data.PhoneDetail && response.Data.PhoneDetail.Id > 0 ? response.Data.PhoneDetail.Id : 0; 
                                    that.set('InvoiceCRMConatactModel', invoiceCRMConatactModel);
                                }
                                that.set("EnableCreateContact", false);
                            }
                        },
                        error: function (data) {
                        },
                        beforeSend: function () {
                            kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                            kendo.ui.progress($("#dvinvPricing"), true);
                        },
                        complete: function () {
                            kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                            kendo.ui.progress($("#dvinvPricing"), false);
                        }
                    });

                    
                }
                
            }

        };

        this.OnSelectDuplicatedContact = function (e) {
            var that = this;
            var _idx = e.sender.select().index();
            var _selectedContactID = e.sender.dataSource.view()[e.sender.select().index()].ContactId;
            var _selectedPhoneDetailsID = e.sender.dataSource.view()[e.sender.select().index()].PhoneDetail.Id;
            var _selectedContactAddresID = e.sender.dataSource.view()[e.sender.select().index()].Address.Id;
            that.set('DuplicadedAddressContactId', _selectedContactID);            
            that.set('DuplicadedAddressContactAddressId', _selectedContactAddresID);
            that.set('DuplicadedPhoneDetailsId', _selectedPhoneDetailsID);
            $("#btnOverWrite").attr('disabled', false);
        };

        this.EnableEditContact = function () {
            this.set("EnableCreateContact", true);
        };

        this.OverrideContact = function () {
            // get the selected contatc contactid;
            //set to invoiceCRMConatactModel.contactid
            var that = this;
            var invoiceCRMConatactModel = that.get('InvoiceCRMConatactModel');
            if (!invoiceCRMConatactModel) {
                invoiceCRMConatactModel = new invEdit.InvoiceCRMConatactModel();
            }

            invoiceCRMConatactModel.ContactId = that.get('DuplicadedAddressContactId');
            that.invoice.CustomerId = that.get('DuplicadedAddressContactId');
            invoiceCRMConatactModel.Address.Id = that.get('DuplicadedAddressContactAddressId');
            invoiceCRMConatactModel.PhoneDetail.Id = that.get('DuplicadedPhoneDetailsId');
            invoiceCRMConatactModel.IsDuplicateCheckRequire = false;
            that.set('InvoiceCRMConatactModel', invoiceCRMConatactModel);

            $.ajax({
                url: '/Ordering/api/InvoiceSearch/SaveUpdate',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(invoiceCRMConatactModel),

                success: function (response) {
                },
                error: function (data) {
                },
                beforeSend: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                    kendo.ui.progress($("#dvinvPricing"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                    kendo.ui.progress($("#dvinvPricing"), false);
                }
            });

            // TODO - Perform request to override contact, if success then perform next code
            this.set("EnableCreateContact", false);
            var wnd = $('#modal-duplicate-contact').data('kendoWindow');
            wnd.close();
            // Otherwise show error message
        };

        this.SaveAsNewContact = function () {
            var that = this;
            var invoiceCRMConatactModel = that.get('InvoiceCRMConatactModel');
            if (!invoiceCRMConatactModel) {
                invoiceCRMConatactModel = new invEdit.InvoiceCRMConatactModel();
            }
            invoiceCRMConatactModel.ContactId = 0;
            invoiceCRMConatactModel.IsDuplicateCheckRequire = false;
            that.set('InvoiceCRMConatactModel', invoiceCRMConatactModel);

            $.ajax({
                url: '/Ordering/api/InvoiceSearch/SaveUpdate',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(invoiceCRMConatactModel),

                success: function (response) {
                    if (response.Data && response.Data.ContactId > 0) {
                        invoiceCRMConatactModel.ContactId = response.Data.ContactId;
                        that.invoice.CustomerId = response.Data.ContactId;
                        invoiceCRMConatactModel.Address.Id = response.Data.Address && response.Data.Address.Id > 0 ? response.Data.Address.Id : 0;
                        invoiceCRMConatactModel.EmailDetail.Id = response.Data.EmailDetail && response.Data.EmailDetail.Id > 0 ? response.Data.EmailDetail.Id : 0;
                        invoiceCRMConatactModel.PhoneDetail.Id = response.Data.PhoneDetail && response.Data.PhoneDetail.Id > 0 ? response.Data.PhoneDetail.Id : 0;
                        that.set('InvoiceCRMConatactModel', invoiceCRMConatactModel);
                    }
                },
                error: function (data) {
                },
                beforeSend: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), true);
                    kendo.ui.progress($("#dvinvPricing"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dvCustomerPricingInfo"), false);
                    kendo.ui.progress($("#dvinvPricing"), false);
                }
            });


            // TODO - Perform request to save as NEW contact, if success then perform next code
            this.set("EnableCreateContact", false);
            var wnd = $('#modal-duplicate-contact').data('kendoWindow');
            wnd.close();
            // Otherwise show error message
        };

        this.CancelOverride = function () {
            var wnd = $('#modal-duplicate-contact').data('kendoWindow');
            wnd.close();
        };


    },

    renderProductsTemplate: function (data) {
        return kendo.Template.compile($('#invProductsTpl').html())(data);
    },
   
    InvoiceModel: function () {
        this.Id = null;
        this.MemberId = null;
        this.MemberInvoiceNumber = null;
        this.FirstName = null;
        this.LastName = null;
        this.InvoiceDate = "";
        this.CreatedDate = "";
        this.CustomerId = null;
        this.Address = new invEdit.InvoiceAddressModel();
        this.InvoiceLines = [];
        this.InvoicePrice = new invEdit.InvoicePriceModel();
        this.InvoiceContact = null;
        this.Email = null;
        this.Type = "Customer";
        this.OrderId = null;
        this.Notes = null;
        this.Status = null;
        this.PaymentType = null;
        this.Phone = null;
        this.PaymentAddress = null;
        this.TotalVolumePoints = null;
        this.Total = null;
        this.DisplayTotal = null;
        this.ShippingMethod = null;
        this.InvoiceShipToAddress = true;
        this.HasShippingAddress = true;
        this.MemberAddress = new invEdit.InvoiceAddressModel();
        this.MemberEmailAddress = null;
        this.MemberFirstName = "";
        this.MemberLastName = "";
        this.ReceiptChannel = "ProductSaleReceipt";
        this.PricingType = "";
        this.ShippingMethod = "";
        this.Source = "";
        this.IsFaceToFace = false;
        this.IsGdoMemberPricing = false;
        this.DisplayInvoiceStatus = "";
        this.DisplayInvoiceType = "";
        this.IsMemberTaxEdited = false;
        this.IsMemberHandlingFeeEdited = false;
        this.MakeMemberTaxZero = false;
        this.MakeMemberHandlingFeeZero = false;
        this.IsCustomerTaxEdited = false;
        this.IsCustomerShippingHandlingEdited = false;
        this.ResetCustomerTaxValue = false;
        this.ClubInvoice = new invEdit.ClubInvoiceModel();
        this.isProfitable = false;
        this.isPurchased = false;
        this.OrderSource = "";
        this.ValidEmailAndPhone=true;
      
    },
    ClubInvoiceModel: function () {
        this.ClubRecieptTotalVolumePoints = null;
        this.ClubRecieptQuantity = null;
        this.ClubRecieptProductName = null;
        this.ClubRecieptDisplayTotalDue = null;
     },
    InvoiceAddressModel: function () {
        this.Id = null;
        this.Address1 = null;
        this.Address2 = null;
        this.City = null;
        this.Country = null;
        this.County = null;
        this.PostalCode = null;
        this.State = null;
    },
    InvoiceEmailAddressDetail: function () {
        this.Id = null;
        this.EmailAddress = null;
    },
    InvoicePhoneDetail: function () {
        this.Id = null;
        this.PhoneNumber = null;
        this.CountryCode = null;
    },
    InvoiceCRMConatactModel: function () {
        this.ContactId = null;
        this.FirstName = null;
        this.LastName = null;
        this.Address = new invEdit.InvoiceAddressModel();
        this.PhoneDetail = new invEdit.InvoicePhoneDetail();
        this.EmailDetail = new invEdit.InvoiceAddressModel();
        this.IsDuplicateCheckRequire = false;
    },
    InvoiceLineModel: function () {
        this.Sku = null;
        this.ProductName = null;
        this.Quantity = null;
        this.RetailPrice = null;
        this.TotalRetailPrice = null;
        this.EarnBase = null;
        this.StockingSku = null;
        this.ProductType = null;
        this.ProductCategory = null;
        this.VolumePoint = null;
        //Will get calculated/Populated by a PriceFactory
        this.TotalEarnBase = null;
        this.TotalVolumePoint = null;
        this.YourPrice = null;
        this.DisplayRetailPrice = null;
        this.DisplayTotalRetailPrice = null;
        this.DisplayCurrencySymbol = null;
        this.DisplayTotalVp = null;
        this.DisplayYourPrice = null;
        this.FreightCharge = 0;
        this.CalcDiscountedAmount = 0;
    },
    InvoicePriceModel: function () {
        this.DiscountPercentage = 0;
        this.DiscountAmount = 0;
        this.CalcDiscountAmount = 0;
        this.TaxPercentage = 0;
        this.TaxAmount = 0;
        this.ShippingPercentage = 0;
        this.ShippingAmount = 0;
        this.TotalDue = 0;
        this.SubTotal = 0;

        this.TotalEarnBase = 0;

        this.CalcShippingAmount = 0;
        this.CalcTaxAmount = 0;
        this.TotalDiscountedAmount = 0;


        this.MemberTax = 0;
        this.MemberFreight = 0;
        this.MemberTotal = 0;
        this.TotalYourPrice = 0;
        this.Profit = 0;
        this.ProfitPercentage = 0;
        this.MemberStaticDiscount = 0;
        this.MemberDiscount = 0;

        //Will get calculated by a PriceFactory
        this.DisplayDiscountedAmount = null;
        this.DisplayCurrencySymbol = null;
        this.DisplaySubtotal = null;
        this.DisplayDiscount = null;
        this.DisplayShipping = null;
        this.DisplayTax = null;
        this.DisplayTotalDue = null;
        this.DisplayCalculatedTax = null;

        this.DisplayProfit = null;
        this.DisplayMemberTax = null;
        this.DisplayMemberFreight = null;
        this.DisplayMemberTotal = null;
        this.DisplayTotalYourPrice = null;
        this.DisplayProfitPercentage = null;
    },

    scrollTo: function (selector) {
        $('html, body').animate({
            scrollTop: ($(selector).offset().top - 100)
        }, 500);
    }
};
$(function () {
    invEdit.Init();

    $("#email").blur(function () {
        var EmailAddress = $('[name="email-address"]');

        if (EmailAddress.val() && !ValidateEmail(EmailAddress.val())) {
            EmailAddress.addClass('k-invalid');
            return false;
        } else {
            EmailAddress.removeClass('k-invalid');
        }
    });
    $('.collapse-member-information a').click(function (e) {
        e.preventDefault();
        toggleMemberInfo(this);
    });

    $("#taxAmount").keypress(function () {

        var model = $('#taxAmount').prop("kendoBindingTarget").source;
        if (model) {
            model.invoice.set("IsCustomerTaxEdited", true);
        }
    });

    $("#taxPercentage").keypress(function () {

        var model = $('#taxPercentage').prop("kendoBindingTarget").source;
        if (model) {
            model.invoice.set("IsCustomerTaxEdited", true);
        }
    });

    // Set functionality when clicking the arrow of the estimate S&H Fragment.
    $(document).on('click', '.selectRate', function () {
        var val = $(this).parent().parent().children('td').eq(1).html();

        if (val.indexOf('span') >= 0) {
            val = $(this).parent().parent().children('td').eq(0).html();
        }

        var model = $('#shippingPercentage').prop("kendoBindingTarget").source;
        if (val && model) {
            model.invoice.InvoicePrice.set("ShippingPercentage", val.substring(0, val.indexOf('%')));
            model.invoice.InvoicePrice.set("ShippingAmount", 0);
            model.invoice.InvoicePrice.set("TaxAmount", 0);
            model.invoice.InvoicePrice.set("TaxPercentage", 0);
            $("#shippingPercentage").prev().removeClass("k-state-disabled");
            $("#shippingAmount").prev().addClass("k-state-disabled");
            var wnd = $("#dvPlg").data("kendoWindow");
            wnd.close();


        }
        return false;
    });
    if ($('#dgProducts > table').length > 0) {
        $('.collapse-member-information').css('margin-top', ($('#dgProducts > table').offset().top - $('#dgTotals > table').offset().top));
    }
    $('#txtQTY').keydown(function (e) {
        var max_chars = 3;        
        if ($(this).val().length >= max_chars && $(this).val() > 1000) {
            $(this).val($(this).val().substr(0, max_chars));
        }
    });

    $('#txtQTY').keyup(function (e) {
        var max_chars = 3;
        if ($(this).val().length >= max_chars && $(this).val() > 1000) {
           $(this).val($(this).val().substr(0, max_chars));
        }
    });
    $("#txtVP").on('keyup blur', function (e) {

        var quantity = $("#txtQTY").val() * 20;
        var that = $(this);
      
        if (Math.round($(this).val()) > quantity
            && e.keyCode != 46
            && e.keyCode != 8) {

            e.preventDefault();
            that.val(quantity);

        }
        if ((Math.round(that.val() * 100) / 100) > quantity) {
            that.val(quantity);
        }
        if (e.keyCode == 189 || e.keyCode == 109) {
            that.val("");
        }
        ltrim(that.val(), '0');

    });
    $('#txtQTY').on("change keypress", function () {
        if ($("#txtVP").val() != "") {
            $("#txtVP").val("");
        }
    });
    $('#lbltotalprice').on("keyup", function (e) {
        if (e.keyCode == 189 || e.keyCode == 109) {
            $(this).val("");
        }
    });


    $("#invoicedatepicker").kendoDatePicker({
        value: new Date(),
        month: {
            content: $("#date-cell-template").html()
        },
        dates: [
          new Date()
        ],
        close: function (e) {
            if (this.value() > this.options.dates[0]) {
                e.preventDefault();
                $("#invoicedatepicker").val(moment(new Date()).format('M/D/YYYY'));
            }
        }
    });

    $("#invoicedatepicker").attr('readonly', true);

    $(".disabledDay").parents("td:first");
});
function toggleMemberInfo(_this) {
    var member_info = $('div.member-information');

    member_info.animate({
        left: (parseInt(member_info.css('left'), 10) == 0 || isNaN(parseInt(member_info.css('left'), 10)))
            ? -member_info.outerWidth()
            : 0
    }, {
        start: function () {
            $(_this).toggleClass('icon-arrow-circle-fl-8');
            if ($(_this).hasClass('icon-arrow-circle-fl-8')) {
                member_info.css('z-index', -1);
                $(_this).html("<span>" + showText + "</span>");
            } else {
                $(_this).html("<span>" + collapse + "</span>");
            }
        },
        complete: function () {
            if (!$(_this).hasClass('icon-arrow-circle-fl-8')) {
                member_info.css('z-index', 'auto');
            }
        }
    });
}
function RedirectToReceipts() {
    var locale = _AnalyticsFacts_.LanguageCode + '-' + _AnalyticsFacts_.CountryCode;
    var _countries = ['EN-US', 'ES-US', 'ES-PR'];

    if (is_page('invoice') && (_countries.indexOf(locale.toUpperCase()) > -1)) {
        logger.log('Page not longer available, please use Receipts page!');
        return '/' + locale + '/Shop/Receipts/Invoice/Index/Ds';;
    }
    return false;
}
function ValidateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function validateFloatKeyPress(el, evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    var number = el.value.split('.');
    if (charCode != 46 && charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    if (number.length > 1 && charCode == 46) {
        return false;
    }
    var caratPos = getSelectionStart(el);
    var dotPos = el.value.indexOf(".");
    if (caratPos > dotPos && dotPos > -1 && (number[1].length > 1)) {
        return false;
    }
    return true;
}
function getSelectionStart(o) {
    if (o.createTextRange) {
        var r = document.selection.createRange().duplicate()
        r.moveEnd('character', o.value.length)
        if (r.text == '') return o.value.length
        return o.value.lastIndexOf(r.text)
    } else return o.selectionStart
}