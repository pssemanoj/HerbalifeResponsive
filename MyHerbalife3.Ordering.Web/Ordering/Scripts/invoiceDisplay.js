var invDisplay = {
    Init: function () {
        var displayModel = kendo.observable(new invDisplay.invoiceDisplayViewModel());
        kendo.bind($('#dvInvDisplay'), displayModel);
        displayModel.loadInvoicedata();
    },

    productDescTemplate: function () { return kendo.template($('#productDescTpl').html()); },

    invoiceDisplayViewModel: function () {
        this.display_InvoiceId = window.display_InvoiceId;
        this.display_Invoice_Action = window.display_Invoice_Action;

        this.loadInvoicedata = function () {
            if (this.display_Invoice_Action == "Invoice_Display") {
                var self = this;
                if (self.display_InvoiceId > 0) {
                    var invoiceId = self.display_InvoiceId;
                    kendo.ui.progress($("#dvinvProduct"), true);
                    $.ajax({
                        type: 'get',
                        dataType: 'json',
                        cache: true,
                        data: JSON.stringify(invoiceId),
                        url: '/Ordering/api/InvoiceSearch/Load/' + invoiceId,
                        success: function (data) {
                            if (data && data.Id > 0) {
                                self.setInvoiceData(data);
                            } else {
                                window.location.href = "/Ordering/Invoice";
                                return false;
                            }
                        },
                        error: function () {
                            self.error = true;
                        },
                        complete: function () {
                            kendo.ui.progress($("#dvinvProduct"), false);
                        }
                    });
                    // get the id from edit_InvoiceId and load the invoice for the id
                }
            }
        };

        this.setInvoiceData = function (data) {
            if (data) {
                this.set('invoice', data);
                this.set('invoiceDate', kendo.toString(new Date(moment(data.InvoiceDate)), "MM/dd/yyyy"));
                if (data.ReceiptChannel == "ClubSaleReceipt" || data.ReceiptChannel == "Club Visit/Sale") {
                    this.set('hideControl', false)
                }
                else {
                    this.set('hideControl', true)
                }
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.set('Hide', true)
                }
                else {
                    this.set('Hide', false)
                }
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    if (document.referrer && (document.referrer.toLowerCase().indexOf("invoice/edit") >= 0 || document.referrer.toLowerCase().indexOf("invoice/create") >= 0)) {
                        this.sendEmail();
                    }
                }
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.set('invoice.OtherEmail', data.Email)
                }
                if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                    this.set('invoice.SMSNumber', data.Phone)
                }   
            }
        };

        this.sendEmailClick = function () {
            var self = this;
            if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                $('#lblEmailMessage').css("display", "none");
                $('#lblEmailMessage').css('background-color', 'inherit');
                self.viewConfirmationEmailPopUp();
            }
            else {
                if (self.invoice.Email) {
                    self.viewConfirmationEmailPopUp();
                }
            }

        };
        this.sendSMSClick = function () {
            var self = this;
            $('#lblSMSMessage').css("display", "none");
            $('#lblSMSMessage').css('background-color', 'inherit');
            self.viewConfirmationSMSPopUp();

        };
        this.sendEmail = function () {
            var self = this;
            if (self.invoice.Email) {
                self.viewConfirmationEmail();
            }
        };
        this.confirmClick = function () {
            var self = this;
            kendo.ui.progress($("#dvinvProduct"), true);

            $.ajax({
                type: 'POST',
                dataType: 'json',
                cache: true,
                data: JSON.stringify(self.invoice),
                contentType: "application/json; charset=utf-8",
                url: '/Ordering/api/InvoiceBase/SendInvoiceEmail',
                success: function (data) {
                },
                error: function () {
                    self.error = true;
                },
                complete: function () {
                    kendo.ui.progress($("#dvinvProduct"), false);
                    self.cancelClick();
                }
            });
        };
        this.cancelClick = function () {
            var wnd = $('#emailConfirmation').data('kendoWindow');
            if (wnd) {
                wnd.close();
            }
        };
        this.cancelSMSClick = function () {
            var wnd = $('#SMSConfirmation').data('kendoWindow');
            kendo.ui.progress($("#dvinvProduct"), false);
            if (wnd) {
                wnd.close();
            }
        };
        this.confirmSMSClick = function () {
            var self = this;
            kendo.ui.progress($("#dvinvProduct"), true);
            if (self.SMSNumberValidation()) {
                $.ajax({
                    type: 'POST',
                    dataType: 'json',
                    cache: true,
                    data: JSON.stringify(self.invoice),
                    contentType: "application/json; charset=utf-8",
                    url: '/Ordering/api/InvoiceBase/SendInvoiceSMS',
                    success: function (data) {
                    },
                    error: function () {
                        self.error = true;
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvinvProduct"), false);
                        self.cancelSMSClick();
                    }
                });
            }
        };
        this.confirmEmailClick = function () {
            var self = this;
            kendo.ui.progress($("#dvinvProduct"), true);
            if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                if (self.EmailFieldsValidate()) {
                    $.ajax({
                        type: 'POST',
                        dataType: 'json',
                        cache: true,
                        data: JSON.stringify(self.invoice),
                        contentType: "application/json; charset=utf-8",
                        url: '/Ordering/api/InvoiceBase/SendInvoiceEmail',
                        success: function (data) {
                        },
                        error: function () {
                            self.error = true;
                        },
                        complete: function () {
                            kendo.ui.progress($("#dvinvProduct"), false);
                            self.cancelEmailClick();
                        }
                    });
                }
            }
            else {
                $.ajax({
                    type: 'POST',
                    dataType: 'json',
                    cache: true,
                    data: JSON.stringify(self.invoice),
                    contentType: "application/json; charset=utf-8",
                    url: '/Ordering/api/InvoiceBase/SendInvoiceEmail',
                    success: function (data) {
                    },
                    error: function () {
                        self.error = true;
                    },
                    complete: function () {
                        kendo.ui.progress($("#dvinvProduct"), false);
                        self.cancelEmailClick();
                    }
                });
            }
        };

        this.cancelEmailClick = function () {
            var wnd = $('#emailSendedConfirmation').data('kendoWindow');
            kendo.ui.progress($("#dvinvProduct"), false);
            if (wnd) {
                wnd.close();
            }
        };

        this.viewConfirmationEmailPopUp = function () {
            var wnd = $('#emailSendedConfirmation').data('kendoWindow');
            wnd.center().open();
        };
        this.viewConfirmationEmail = function () {
            var wnd = $('#emailConfirmation').data('kendoWindow');
            wnd.center().open();
        };
        this.viewConfirmationSMSPopUp = function () {
            var wnd = $('#SMSConfirmation').data('kendoWindow');
            wnd.center().open();
        };


        this.editInvoiceClick = function () {
            var scope = this;
            window.location.href = "/Ordering/Invoice/Edit/" + scope.display_InvoiceId;
            return false;
        };

        this.deleteInvoiceClick = function () {
            var scope = this;
            window.location.href = "/Ordering/Invoice/Delete/" + scope.display_InvoiceId;
            scope.closeDeleteInvoice();
            return false;
        };

        this.viewDeletePopUp = function (e) {
            var wnd = $('#confirmDeleteInvoice').data('kendoWindow');
            wnd.center().open();
        };

        this.closeDeleteInvoice = function () {
            var enterCertWnd = $('#confirmDeleteInvoice').data('kendoWindow');
            if (enterCertWnd) {
                enterCertWnd.close();
            }
        };

        this.printInvoiceClick = function () {
            var scope = this;
            window.open("/Ordering/Invoice/Print/" + scope.display_InvoiceId, '_blank');
            return false;
        };

        this.copyInvoiceClick = function () {
            var scope = this;
            window.location.href = "/Ordering/Invoice/Copy/" + scope.display_InvoiceId;
            return false;
        };

        this.createOrderClick = function () {
            var scope = this;
            window.location.href = "/Ordering/ShoppingCart.aspx?memberInvoiceId=" + scope.display_InvoiceId + "&invoiceNum=" + scope.invoice.MemberInvoiceNumber;
            return false;
        };

        this.onInvoiceLinesDataBinding = function (e) {
            var grid = e.sender;

            if (this.invoice.Type == "Distributor") {
                grid.showColumn("EarnBase");
            } else {
                grid.hideColumn("EarnBase");
            }
        };

        this.createOrderRedirect = function () {
            var self = this;
            $.ajax({
                url: "/Ordering/Invoice/CreateOrderRedirect/",
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                success: function () {
                    self.createOrderClick();
                },
                error: function () {
                },
                complete: function () {
                }
            });
        };
        this.SMSNumberValidation = function () {
            var phoneNumber = $('#smsNumber');
            if( phoneNumber.val().length < 10)
            {
                phoneNumber.css("background-color", "#ffcccc");
                $('#lblSMSMessage').css("display", "block")
                return false;
            }
            else {
                phoneNumber.css("background-color", "inherit");
                $('#lblSMSMessage').css("display", "none")
                return true;
            }
        };
        this.EmailFieldsValidate = function () {
            var EmailAddress = $('#lblEmail');
            if (EmailAddress.length == 0)
            {
                EmailAddress.css("background-color", "#ffcccc");
                $('#lblEmailMessage').css("display", "block")
                return false;
            }
            else {
                EmailAddress.css('background-color', 'inherit');
                $('#lblEmailMessage').css("display", "none")
            }
            if (!this.ValidateEmail(EmailAddress.val())) {
                EmailAddress.css("background-color", "#ffcccc");
                $('#lblEmailMessage').css("display", "block")
                return false;
            } else {
                EmailAddress.css('background-color', 'inherit');
                $('#lblEmailMessage').css("display", "none")
                return true;
            }
            return true;
        };
        this.ValidateEmail = function (email) {
            var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(email);
        };

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
        this.Type = null;
        this.OrderId = null;
        this.Notes = null;
        this.Status = null;
        this.Phone = null;
        this.PaymentAddress = null;
        this.TotalVolumePoints = null;
        this.Total = null;
        this.DisplayTotal = null;
        this.ShippingMethod = null;
        this.InvoiceShipToAddress = false;
        this.MemberAddress = new invEdit.InvoiceAddressModel();
        this.MemberEmailAddress = null;
        this.MemberFirstName = "";
        this.MemberLastName = "";
        this.MemberPhoneNumber = "";
        this.OrderSource = "";
        this.PricingType = "";
        this.ShippingMethod = "";
        this.Source = "";
        this.IsFaceToFace = false;
        this.IsGdoMemberPricing = true;
        this.DisplayInvoiceStatus = "";
        this.IsDisplayFreeShipping = false;
        this.HideTax = false;
        this.isHide = false;
        this.ReceiptChannel = "";
        this.ClubInvoice = new invEdit.ClubInvoiceModel();
        this.hideControl = null;
        this.Hide = null;
        this.HideSMS = false;
        this.PaymentType = null;
        this.OtherEmail = null;
        this.SMSNumber = null;
        this.DisplayPaymentType = "";
        this.isValidSMSNumber = null;


    },
    ClubInvoiceModel: function () {
        this.ClubRecieptTotalVolumePoints = null;
        this.ClubRecieptQuantity = null;
        this.ClubRecieptProductName = null;
        this.ClubRecieptDisplayTotalDue = null;
    },
    InvoiceAddressModel: function () {
        this.Address1 = null;
        this.Address2 = null;
        this.City = null;
        this.Country = null;
        this.County = null;
        this.PostalCode = null;
        this.State = null;
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

        //Will get calculated by a PriceFactory
        this.DisplayDiscountedAmount = null;
        this.DisplayCurrencySymbol = null;
        this.DisplaySubtotal = null;
        this.DisplayDiscount = "$0.00";
        this.DisplayShipping = null;
        this.DisplayTax = null;
        this.DisplayTotalDue = null;
        this.DisplayCalculatedTax = null;
        this.TotalVolumePoints = null;

        this.DisplayProfit = null;
        this.DisplayMemberTax = null;
        this.DisplayMemberFreight = null;
        this.DisplayMemberTotal = null;
        this.DisplayTotalYourPrice = null;
        this.DisplayProfitPercentage = null;
    },

};
$(function () {
    invDisplay.Init();
});