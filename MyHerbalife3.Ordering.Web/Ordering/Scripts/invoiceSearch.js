
var invSearch = {
    Init: function () {
        var searchModel = kendo.observable(new invSearch.SearchModel());
        kendo.bind($('#dvInvSearch'), searchModel);
        searchModel.loadInvoice();
        searchModel.loadErrorMessages();
    },

    deleteInv: function () { return kendo.template($('#deleteInvTpl').html()); },
    createOrderInv: function () { return kendo.template($('#createOrderInvTpl').html()); },
    displayInv: function () { return kendo.template($('#displayInvTpl').html()); },
    selectMyOrdersTemplate: function () { return kendo.template($('#selectMyOrdersTpl').html()); },
    dateMyOrdersTemplate: function () { return kendo.template($('#dateMyOrdersTpl').html()); },
    statusTemplate: function () { return kendo.template($('#statusInvTpl').html()); },



    SearchModel: function () {
        this.isLoading = true;
        this.isLoaded = false;
        this.from = "";
        this.to = "";
        this.filterBy = "";
        this.filterValue = "";
        this.invoices = [];
        this.ListEmpty = false;
        this.selectedFilterItem = null;
        this.filterByValue = "";
        this.currentInvoiceId = 0;
        this.memberInvoiceNumber = 0;
        this.myOrdersFilter = "";
        this.index_Invoice_Action = window.index_Invoice_Action;
        this.myOrdersErrorText = "";
        this.errorMessages = [];
        this.statusUpdateText = "";
        this.hideControl = function () {
            if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                return true;
            }
            else {
                return false;
            }

        };

        this.loadInvoice = function () {
            var self = this;
            if (this.index_Invoice_Action == "Invoice_Search") {
                kendo.ui.progress($("#loading"), true);
                $.ajax({
                    type: 'get',
                    dataType: 'json',
                    cache: true,
                    url: '/Ordering/api/InvoiceSearch/LoadAll',
                    success: function (data) {
                        self.dataHandler(data);
                    },
                    error: function () {
                        self.error = true;
                    },
                    complete: function () {
                        kendo.ui.progress($("#loading"), false);
                    }
                });
            }
        };

        this.filterCategoriesSource = new kendo.data.DataSource({
            type: "json",
            transport: {
                read:
                {
                    url: "/Ordering/api/InvoiceSearch/LoadFilterCategory"
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

        this.onStatusChange = function (e) {
            var self = this;
            var model = {
                Id: e.data.Id,
                Status: e.data.Status
            };

            kendo.ui.progress($("#loading"), true);
            $.ajax({
                url: "/Ordering/api/InvoiceBase/UpdateStatus",
                type: 'POST',
                data: JSON.stringify(model),
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data) {
                        self.set('statusUpdateText', self.errorMessages['statusUpdateText']);
                    } else {
                        self.set('statusUpdateText', '');
                    }
                },
                error: function (data) {
                },
                complete: function () {
                    kendo.ui.progress($("#loading"), false);
                    self.loadInvoice();
                }
            });
        };

        this.loadErrorMessages = function () {
            if (this.index_Invoice_Action == "Invoice_Search") {
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

        this.doSearch = function () {
            var self = this;

            var invoiceFilterModel = {
                From: this.from,
                To: this.to
            };

            if (null != this.selectedFilterItem) {
                invoiceFilterModel.SelectedFilterId = this.selectedFilterItem;
                invoiceFilterModel.SelectedFilterValue = this.filterByValue;
            }
            kendo.ui.progress($("#loading"), true);
            $.ajax({
                url: "/Ordering/api/InvoiceSearch/Filter",
                type: 'POST',
                data: JSON.stringify(invoiceFilterModel),
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    self.dataHandler(data);
                },
                error: function (data) {
                },
                complete: function () {
                    self.set('statusUpdateText', '');
                    kendo.ui.progress($("#loading"), false);
                }
            });
        };


        this.doClear = function () {
            this.set("from", "");
            this.set("to", "");
            this.set("filterByValue", "");
            this.set("selectedFilterItem", null);
            this.set('statusUpdateText', '');
            this.loadInvoice();
        },
            this.dataHandler = function (data) {
                if (data) {
                    this.set("invoices", data);
                    this.set("isLoading ", false);
                    this.set("isLoaded ", true);
                    if (this.invoices.length == 0)
                        this.set("ListEmpty", true);
                    else
                        this.set("ListEmpty", false);
                }
            };

        this.viewDeletePopUp = function (e) {
            var self = this;
            var memberInvoiceNumber = e.data.DisplayMemberInvoiceNumber;
            var invoiceId = e.data.Id;
            self.set('currentInvoiceId', invoiceId);
            self.set('memberInvoiceNumber', memberInvoiceNumber);
            if (invoiceId != 0) {
                var wnd = $('#confirmDeleteInvoice').data('kendoWindow');
                wnd.center().open();
            }
        };

        this.closeDeleteInvoice = function () {
            var enterCertWnd = $('#confirmDeleteInvoice').data('kendoWindow');
            if (enterCertWnd) {
                enterCertWnd.close();
            }
        };

        this.DeleteInvoice = function () {
            var self = this;
            var invtoDelete = this.currentInvoiceId;
            $.ajax({
                url: "/Ordering/api/InvoiceSearch/Delete/" + invtoDelete,
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                success: function () {
                },
                error: function () {

                },
                complete: function () {
                    self.loadInvoice();
                    self.closeDeleteInvoice();
                }
            });
        };

        this.createOrderRedirect = function () {
            var self = this;
            $.ajax({
                url: "/Ordering/Invoice/CreateOrderRedirect/",
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                success: function () {
                },
                error: function () {
                },
                complete: function () {
                }
            });
        };

        this.clickMyOrders = function () {
            kendo.ui.progress($("#loading"), true);
            var wnd = $('#dvInvMyOrders').data('kendoWindow');
            wnd.center().open();
            this.myOrdersDataSource.read();
            kendo.ui.progress($("#loading"), false);
        };

        this.selectMyOrders = function (e) {
            window.location.href = "/Ordering/Invoice/EditByOrderId/" + e.data.Id;
            return false;
        };

        this.searchMyOrders = function () {
            this.myOrdersDataSource.filter([
                {
                    "logic": "or",
                    "filters": [
                        {
                            "field": "Id",
                            "operator": "startswith",
                            "value": this.myOrdersFilter
                        },
                        {
                            "field": "Name",
                            "operator": "startswith",
                            "value": this.contactsFilter
                        }
                    ]
                }
            ]);
        };

        this.myOrdersDataSource = new kendo.data.DataSource({
            type: "json",
            serverFiltering: true,
            serverPaging: true,
            pageSize: 10,
            transport: {
                read: {
                    url: '/Ordering/api/MyOrders',
                    type: "POST"
                }
            },
            schema: {
                data: function (data) {
                    if (data && data.Items && data.Items.$values) {
                        return data.Items.$values;
                    } else if (data && data.Items) {
                        return data.Items;
                    } else {
                        return null;
                    }
                },
                total: function (data) {
                    if (data) {
                        return data.TotalCount;
                    } else {
                        return 0;
                    }
                }
            },
            requestStart: function (e) {
                kendo.ui.progress($("#dgMyOrders"), true);
            },
            requestEnd: function (e) {
                kendo.ui.progress($("#dgMyOrders"), false);
            }
        });

        this.myOrdersDataBound = function (e) {
            kendo.ui.progress($("#dgMyOrders"), false);
            if (this.myOrdersDataSource && this.myOrdersDataSource.total() == 0) {
                this.set('myOrdersErrorText', this.errorMessages['myOrdersError']);
            } else {
                this.set('myOrdersErrorText', '');
            }
        };

        this.loadMyOrders = function () {
            var self = this;
            $.ajax({
                type: 'get',
                dataType: 'json',
                cache: true,
                url: '/Ordering/api/MyOrders',
                success: function (data) {
                    self.set("myOrders", data);
                },
                error: function () {
                    self.error = true;
                },
                beforeSend: function () {
                    kendo.ui.progress($("#dgMyOrders"), true);
                },
                complete: function () {
                    kendo.ui.progress($("#dgMyOrders"), false);
                }
            });
        };

        
            this.OpenTutorialModal = function (e) {

                var _template = Template(e.target.dataset.parameter, e.target.dataset.account, e.target.dataset.player);
                var _videoContainer = $('#video-tutorial');
                _videoContainer.append(_template);
                _videoContainer.fadeIn('fast');
                var _videoModal = $('#modal-video-tutorial').data('kendoWindow');
                _videoModal.center().open();

            };

            this.CloseTutorialModal = function (e) {
                var _videoContainer = $('#video-tutorial');
                _videoContainer.fadeOut('slow', function () {
                    _videoContainer.empty();
                });

            };

            // =====================================
            // ========= Privated Methods ==========
            // =====================================

            var Template = function (id, account, player) {

                id = id || '';
                account = account || '';
                player = player || '';

                var _template = '<video data-account="' + account + '" ' +
                                    'data-player="' + player + '" ' +
                                    'data-embed="default" data-video-id="' + id + '" ' +
                                    'width="100%" height="360" class="video-js standard-player" controls autoplay>' +
                                '</video>' +
                                '<script src="//players.brightcove.net/' + account + '/' + player + '_default/index.min.js"></script>';

                return _template;

            };

        this.ExportToCSV = function () {
            kendo.ui.progress($("#loading"), true);
            var fileName;
            var grid;
            if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {
                grid = $("#dgInvoices1").data("kendoGrid");
                fileName = 'Receipt.csv';
            }
            else {
                grid = $("#dgInvoices").data("kendoGrid");
                fileName = 'Invoices.csv';
            }
            var csv = '';

            var data = grid.dataSource.view();

            //add the header row
            for (var i = 0; i < grid.columns.length; i++) {
                var field = grid.columns[i].field;
                var title = grid.columns[i].title || field;

                //NO DATA
                if (!field || field == "Delete" || field == "CreateOrder") {
                    continue;
                }

                title = title.replace(/"/g, '""');
                csv += '"' + title + '"';
                if (i < grid.columns.length - 1) {
                    csv += ',';
                }
            }
            csv += '\n';
            if (_AnalyticsFacts_.CountryCode.toUpperCase() == "CA" || _AnalyticsFacts_.CountryCode.toUpperCase() == "JM" || _AnalyticsFacts_.CountryCode.toUpperCase() == "TT") {

                for (var row in data) {
                    for (var i = 0; i < grid.columns.length; i++) {
                        var fieldName = grid.columns[i].field;

                        //VALIDATE COLUMN
                        if (!fieldName || fieldName == "Delete" || fieldName == "CreateOrder") continue;
                        var value = '';
                        if (fieldName.indexOf('.') >= 0) {
                            var properties = fieldName.split('.');

                            for (var j = 0; j < properties.length; j++) {
                                var prop = properties[j];
                                value = value + (data[row][prop] || '') + ' ';
                            }
                        } else {
                            if (fieldName == "FirstName") {
                                value1 = data[row]["FirstName"] || '';
                                value2 = data[row]["LastName"] || '';
                                value = value1 + value2;
                            }
                            else {
                                value = data[row][fieldName] || '';
                            }
                        }

                        value = value.toString().replace(/"/g, '""');
                        csv += '"' + value + '"';
                        if (i < grid.columns.length - 1) {
                            csv += ',';
                        }
                    }
                    csv += '\n';
                }
            }
            else {
                //add each row of data
                for (var row in data) {
                    for (var i = 0; i < grid.columns.length; i++) {
                        var fieldName = grid.columns[i].field;

                        //VALIDATE COLUMN
                        if (!fieldName || fieldName == "Delete" || fieldName == "CreateOrder") continue;
                        var value = '';
                        if (fieldName.indexOf('.') >= 0) {
                            var properties = fieldName.split('.');
                            var value = data[row] || '';
                            for (var j = 0; j < properties.length; j++) {
                                var prop = properties[j];
                                value = value[prop] || '';
                            }
                        } else {

                            value = data[row][fieldName] || '';
                        }

                        value = value.toString().replace(/"/g, '""');
                        csv += '"' + value + '"';
                        if (i < grid.columns.length - 1) {
                            csv += ',';
                        }
                    }
                    csv += '\n';
                }
            }

            //EXPORT TO BROWSER
            var blob = new Blob([csv], { type: 'text/csv;charset=utf-8' }); //Blob.js
            saveAs(blob, fileName); //FileSaver.js
            kendo.ui.progress($("#loading"), false);
        };
    }
};
$(function () {
    invSearch.Init();
});