var mboxCopyright = "Copyright 1996-2014. Adobe Systems Incorporated. All rights reserved.";
var TNT = TNT || {};
TNT.a = TNT.a || {};
TNT.a.nestedMboxes = [];
TNT.a.b = {
    "companyName": "Test&amp;Target",
    "isProduction": true,
    "adminUrl": "//admin10.testandtarget.omniture.com/admin",
    "clientCode": "herbalife",
    "serverHost": "herbalife.tt.omtrdc.net",
    "mboxTimeout": 15000,
    "mboxLoadedTimeout": 16,
    "mboxFactoryDisabledTimeout": 60 * 60,
    "bodyPollingTimeout": 16,
    "sessionExpirationTimeout": 31 * 60,
    "experienceManagerDisabledTimeout": 30 * 60,
    "experienceManagerTimeout": 5000,
    "tntIdLifetime": 1209600,
    "crossDomain": "disabled",
    "trafficDuration": 10368000,
    "trafficLevelPercentage": 100,
    "clientSessionIdSupport": false,
    "clientTntIdSupport": false,
    "passPageParameters": true,
    "usePersistentCookies": true,
    "crossDomainEnabled": false,
    "crossDomainXOnly": false,
    "imsOrgId": "E1DC1042548EFE0F0A4C98A4@AdobeOrg",
    "includeExperienceManagerPlugin": true,
    "globalMboxName": "custom-global-mbox",
    "globalMboxLocationDomId": "",
    "globalMboxAutoCreate": false,
    "experienceManagerPluginUrl": "/SharedUI/Scripts/target.js",
    "siteCatalystPluginName": "tt",
    "includeSiteCatalystPlugin": false,
    "mboxVersion": 56,
    "mboxIsSupportedFunction": function () {
        return true;
    },
    "clientJavascriptFunction": function () { },
    "parametersFunction": function () {
        return "";
    },
    "cookieDomainFunction": function () {
        return mboxCookiePageDomain();
    }
};
TNT.a.c = {};
TNT.a.c.d = "mboxPage";
TNT.a.c.e = "mboxMCGVID";
TNT.a.c.f = "mboxMCGLH";
TNT.a.c.g = "mboxAAMB";
TNT.a.c.h = "mboxMCAVID";
TNT.a.c.i = "mboxMCSDID";
TNT.a.c.j = "mboxCount";
TNT.a.c.k = "mboxHost";
TNT.a.c.l = "mboxFactoryId";
TNT.a.c.m = "mboxPC";
TNT.a.c.n = "screenHeight";
TNT.a.c.o = "screenWidth";
TNT.a.c.p = "browserWidth";
TNT.a.c.q = "browserHeight";
TNT.a.c.r = "browserTimeOffset";
TNT.a.c.s = "colorDepth";
TNT.a.c.t = "mboxXDomain";
TNT.a.c.u = "mboxURL";
TNT.a.c.v = "mboxReferrer";
TNT.a.c.w = "mboxVersion";
TNT.a.c.x = "mbox";
TNT.a.c.y = "mboxId";
TNT.a.c.z = "mboxDOMLoaded";
TNT.a.c.A = "mboxTime";
TNT.a.c.B = "scPluginVersion";
TNT.a.C = {};
TNT.a.C.D = "mboxDisable";
TNT.a.C.E = "mboxSession";
TNT.a.C.F = "mboxEnv";
TNT.a.C.G = "mboxDebug";
TNT.a.H = {};
TNT.a.H.D = "disable";
TNT.a.H.E = "session";
TNT.a.H.m = "PC";
TNT.a.H.I = "level";
TNT.a.H.J = "check";
TNT.a.H.G = "debug";
TNT.a.H.K = "em-disabled";
TNT.a.L = {};
TNT.a.L.M = "default";
TNT.a.L.N = "mbox";
TNT.a.L.O = "mboxImported-";
TNT.a.L.P = 60000;
TNT.a.L.Q = "mboxDefault";
TNT.a.L.R = "mboxMarker-";
TNT.a.L.S = 250;
TNT.a.L.B = 1;
TNT.getGlobalMboxName = function () {
    return TNT.a.b.globalMboxName;
};
TNT.getGlobalMboxLocation = function () {
    return TNT.a.b.globalMboxLocationDomId;
};
TNT.isAutoCreateGlobalMbox = function () {
    return TNT.a.b.globalMboxAutoCreate;
};
TNT.getClientMboxExtraParameters = function () {
    return TNT.a.b.parametersFunction();
};
TNT.a.T = {};
TNT.a.T.U = function (V) {
    var W = {}.toString;
    return W.call(V) === '[object Undefined]';
};
TNT.a.T.X = function (V) {
    var W = {}.toString;
    return W.call(V) === '[object Null]';
};
TNT.a.T.Y = function (V) {
    var T = TNT.a.T;
    if (T.U(V) || T.X(V)) {
        return true;
    }
    return V.length === 0;
};
TNT.a.T.Z = function (V) {
    var W = {}.toString;
    return W.call(V) === '[object Function]';
};
TNT.a.T._ = function (V) {
    var W = {}.toString;
    return W.call(V) === '[object Array]';
};
TNT.a.T.ab = function (V) {
    var W = {}.toString;
    return W.call(V) === '[object String]';
};
TNT.a.T.bb = function (V) {
    var W = {}.toString;
    return W.call(V) === '[object Object]';
};
TNT.getTargetPageParameters = function () {
    var T = TNT.a.T;
    var cb = window.targetPageParams;
    if (!T.Z(cb)) {
        return [];
    }
    var db = null;
    try {
        db = cb();
    } catch (eb) { }
    if (T.X(db)) {
        return [];
    }
    if (T._(db)) {
        return db;
    }
    if (T.ab(db) && !T.Y(db)) {
        return TNT.a.fb(db);
    }
    if (T.bb(db)) {
        return TNT.a.gb(db, []);
    }
    return [];
};
TNT.a.fb = function (hb) {
    var db = [];
    var ib = /([^&=]+)=([^&]*)/g;
    var jb = decodeURIComponent;
    var kb = ib.exec(hb);
    while (kb) {
        db.push([jb(kb[1]), jb(kb[2])].join('='));
        kb = ib.exec(hb);
    }
    return db;
};
TNT.a.gb = function (lb, mb) {
    var T = TNT.a.T;
    var db = [];
    for (var nb in lb) {
        if (!lb.hasOwnProperty(nb)) {
            continue;
        }
        var V = lb[nb];
        if (T.bb(V)) {
            mb.push(nb);
            db = db.concat(TNT.a.gb(V, mb));
            mb.pop();
        } else {
            if (mb.length > 0) {
                db.push([mb.concat(nb).join('.'), V].join('='));
            } else {
                db.push([nb, V].join('='));
            }
        }
    }
    return db;
};
mboxUrlBuilder = function (ob, pb) {
    this.ob = ob;
    this.pb = pb;
    this.qb = [];
    this.rb = function (u) {
        return u;
    };
    this.sb = null;
};
mboxUrlBuilder.prototype.addNewParameter = function (tb, V) {
    this.qb.push({
        name: tb,
        value: V
    });
    return this;
};
mboxUrlBuilder.prototype.addParameterIfAbsent = function (tb, V) {
    if (V) {
        for (var ub = 0; ub < this.qb.length; ub++) {
            var vb = this.qb[ub];
            if (vb.name === tb) {
                return this;
            }
        }
        this.checkInvalidCharacters(tb);
        return this.addNewParameter(tb, V);
    }
};
mboxUrlBuilder.prototype.addParameter = function (tb, V) {
    this.checkInvalidCharacters(tb);
    for (var ub = 0; ub < this.qb.length; ub++) {
        var vb = this.qb[ub];
        if (vb.name === tb) {
            vb.value = V;
            return this;
        }
    }
    return this.addNewParameter(tb, V);
};
mboxUrlBuilder.prototype.addParameters = function (qb) {
    if (!qb) {
        return this;
    }
    for (var ub = 0; ub < qb.length; ub++) {
        var wb = qb[ub].indexOf('=');
        if (wb === -1 || wb === 0) {
            continue;
        }
        this.addParameter(qb[ub].substring(0, wb), qb[ub].substring(wb + 1, qb[ub].length));
    }
    return this;
};
mboxUrlBuilder.prototype.setServerType = function (xb) {
    this.yb = xb;
};
mboxUrlBuilder.prototype.setBasePath = function (sb) {
    this.sb = sb;
};
mboxUrlBuilder.prototype.setUrlProcessAction = function (zb) {
    this.rb = zb;
};
mboxUrlBuilder.prototype.buildUrl = function () {
    var Ab = this.sb ? this.sb : '/m2/' + this.pb + '/mbox/' + this.yb;
    var Bb = document.location.protocol == 'file:' ? 'http:' : document.location.protocol;
    var u = Bb + "//" + this.ob + Ab;
    var Cb = u.indexOf('?') != -1 ? '&' : '?';
    for (var ub = 0; ub < this.qb.length; ub++) {
        var vb = this.qb[ub];
        u += Cb + encodeURIComponent(vb.name) + '=' + encodeURIComponent(vb.value);
        Cb = '&';
    }
    return this.Db(this.rb(u));
};
mboxUrlBuilder.prototype.getParameters = function () {
    return this.qb;
};
mboxUrlBuilder.prototype.setParameters = function (qb) {
    this.qb = qb;
};
mboxUrlBuilder.prototype.clone = function () {
    var Eb = new mboxUrlBuilder(this.ob, this.pb);
    Eb.setServerType(this.yb);
    Eb.setBasePath(this.sb);
    Eb.setUrlProcessAction(this.rb);
    for (var ub = 0; ub < this.qb.length; ub++) {
        Eb.addParameter(this.qb[ub].name, this.qb[ub].value);
    }
    return Eb;
};
mboxUrlBuilder.prototype.Db = function (Fb) {
    return Fb.replace(/\"/g, '&quot;').replace(/>/g, '&gt;');
};
mboxUrlBuilder.prototype.checkInvalidCharacters = function (tb) {
    var Gb = new RegExp('(\'|")');
    if (Gb.exec(tb)) {
        throw "Parameter '" + tb + "' contains invalid characters";
    }
};
mboxStandardFetcher = function () { };
mboxStandardFetcher.prototype.getType = function () {
    return 'standard';
};
mboxStandardFetcher.prototype.fetch = function (Hb) {
    Hb.setServerType(this.getType());
    document.write('<' + 'scr' + 'ipt src="' + Hb.buildUrl() + '"><' + '\/scr' + 'ipt>');
};
mboxStandardFetcher.prototype.cancel = function () { };
mboxAjaxFetcher = function () { };
mboxAjaxFetcher.prototype.getType = function () {
    return 'ajax';
};
mboxAjaxFetcher.prototype.fetch = function (Hb) {
    Hb.setServerType(this.getType());
    var u = Hb.buildUrl();
    this.Ib = document.createElement('script');
    this.Ib.src = u;
    document.body.appendChild(this.Ib);
};
mboxAjaxFetcher.prototype.cancel = function () { };
mboxMap = function () {
    this.Jb = {};
    this.mb = [];
};
mboxMap.prototype.put = function (nb, V) {
    if (!this.Jb[nb]) {
        this.mb[this.mb.length] = nb;
    }
    this.Jb[nb] = V;
};
mboxMap.prototype.get = function (nb) {
    return this.Jb[nb];
};
mboxMap.prototype.remove = function (nb) {
    this.Jb[nb] = undefined;
    var Kb = [];
    for (var i = 0; i < this.mb.length; i++) {
        if (this.mb[i] !== nb) {
            Kb.push(this.mb[i]);
        }
    }
    this.mb = Kb;
};
mboxMap.prototype.each = function (zb) {
    for (var ub = 0; ub < this.mb.length; ub++) {
        var nb = this.mb[ub];
        var V = this.Jb[nb];
        if (V) {
            var db = zb(nb, V);
            if (db === false) {
                break;
            }
        }
    }
};
mboxMap.prototype.isEmpty = function () {
    return this.mb.length === 0;
};
mboxFactory = function (Lb, pb, Mb, Nb) {
    var b = TNT.a.b;
    var H = TNT.a.H;
    var C = TNT.a.C;
    var L = TNT.a.L;
    this.Ob = false;
    this.Mb = Mb;
    this.Pb = new mboxList();
    mboxFactories.put(Mb, this);
    this.Qb = b.mboxIsSupportedFunction() && typeof (window.attachEvent || document.addEventListener || window.addEventListener) != 'undefined';
    this.Rb = this.Qb && mboxGetPageParameter(C.D) === null;
    var Sb = Mb == L.M;
    var Tb = L.N + (Sb ? '' : ('-' + Mb));
    this.Ub = new mboxCookieManager(Tb, b.cookieDomainFunction());
    if (b.crossDomainXOnly) {
        this.Rb = this.Rb && this.Ub.isEnabled();
    }
    this.Rb = this.Rb && (this.Ub.getCookie(H.D) === null);
    if (this.isAdmin()) {
        this.enable();
    }
    this.Vb();
    this.Wb = mboxGenerateId();
    this.Xb = mboxScreenHeight();
    this.Yb = mboxScreenWidth();
    this.Zb = mboxBrowserWidth();
    this._b = mboxBrowserHeight();
    this.ac = mboxScreenColorDepth();
    this.bc = mboxBrowserTimeOffset();
    this.cc = new mboxSession(this.Wb, C.E, H.E, b.sessionExpirationTimeout, this.Ub);
    this.dc = new mboxPC(H.m, b.tntIdLifetime, this.Ub);
    this.Hb = new mboxUrlBuilder(Lb, pb);
    this.ec(this.Hb, Sb, Nb);
    this.fc = new Date().getTime();
    this.gc = this.fc;
    var hc = this;
    this.addOnLoad(function () {
        hc.gc = new Date().getTime();
    });
    if (this.Qb) {
        this.addOnLoad(function () {
            hc.Ob = true;
            hc.getMboxes().each(function (ic) {
                ic.jc();
                ic.setFetcher(new mboxAjaxFetcher());
                ic.finalize();
            });
            TNT.a.nestedMboxes = [];
        });
        if (this.Rb) {
            this.limitTraffic(b.trafficLevelPercentage, b.trafficDuration);
            this.kc();
            this.lc = new mboxSignaler(this);
        } else {
            if (!b.isProduction) {
                if (this.isAdmin()) {
                    if (!this.isEnabled()) {
                        alert("mbox disabled, probably due to timeout\n" + "Reset your cookies to re-enable\n(this message will only appear in administrative mode)");
                    } else {
                        alert("It looks like your browser will not allow " + b.companyName + " to set its administrative cookie. To allow setting the" + " cookie please lower the privacy settings of your browser.\n" + "(this message will only appear in administrative mode)");
                    }
                }
            }
        }
    }
};
mboxFactory.prototype.forcePCId = function (mc) {
    if (!TNT.a.b.clientTntIdSupport) {
        return;
    }
    if (this.dc.forceId(mc)) {
        this.cc.forceId(mboxGenerateId());
    }
};
mboxFactory.prototype.forceSessionId = function (mc) {
    if (!TNT.a.b.clientSessionIdSupport) {
        return;
    }
    this.cc.forceId(mc);
};
mboxFactory.prototype.isEnabled = function () {
    return this.Rb;
};
mboxFactory.prototype.getDisableReason = function () {
    return this.Ub.getCookie(TNT.a.H.D);
};
mboxFactory.prototype.isSupported = function () {
    return this.Qb;
};
mboxFactory.prototype.disable = function (nc, oc) {
    if (typeof nc == 'undefined') {
        nc = 60 * 60;
    }
    if (typeof oc == 'undefined') {
        oc = 'unspecified';
    }
    if (!this.isAdmin()) {
        this.Rb = false;
        this.Ub.setCookie(TNT.a.H.D, oc, nc);
    }
};
mboxFactory.prototype.enable = function () {
    this.Rb = true;
    this.Ub.deleteCookie(TNT.a.H.D);
};
mboxFactory.prototype.isAdmin = function () {
    return document.location.href.indexOf(TNT.a.C.F) != -1;
};
mboxFactory.prototype.limitTraffic = function (pc, nc) {
    if (TNT.a.b.trafficLevelPercentage != 100) {
        if (pc == 100) {
            return;
        }
        var qc = true;
        if (parseInt(this.Ub.getCookie(TNT.a.H.I)) != pc) {
            qc = (Math.random() * 100) <= pc;
        }
        this.Ub.setCookie(TNT.a.H.I, pc, nc);
        if (!qc) {
            this.disable(60 * 60, 'limited by traffic');
        }
    }
};
mboxFactory.prototype.addOnLoad = function (rc) {
    if (this.isDomLoaded()) {
        rc();
    } else {
        var sc = false;
        var tc = function () {
            if (sc) {
                return;
            }
            sc = true;
            rc();
        };
        this.uc.push(tc);
        if (this.isDomLoaded() && !sc) {
            tc();
        }
    }
};
mboxFactory.prototype.getEllapsedTime = function () {
    return this.gc - this.fc;
};
mboxFactory.prototype.getEllapsedTimeUntil = function (A) {
    return A - this.fc;
};
mboxFactory.prototype.getMboxes = function () {
    return this.Pb;
};
mboxFactory.prototype.get = function (x, y) {
    return this.Pb.get(x).getById(y || 0);
};
mboxFactory.prototype.update = function (x, qb) {
    if (!this.isEnabled()) {
        return;
    }
    var hc = this;
    if (!this.isDomLoaded()) {
        this.addOnLoad(function () {
            hc.update(x, qb);
        });
        return;
    }
    if (this.Pb.get(x).length() === 0) {
        throw "Mbox " + x + " is not defined";
    }
    this.Pb.get(x).each(function (ic) {
        var Hb = ic.getUrlBuilder();
        Hb.addParameter(TNT.a.c.d, mboxGenerateId());
        hc.vc(Hb);
        hc.wc(Hb, x);
        hc.setVisitorIdParameters(Hb, x);
        ic.load(qb);
    });
};
mboxFactory.prototype.setVisitorIdParameters = function (u, x) {
    if (typeof Visitor == 'undefined' || !TNT.a.b.imsOrgId) {
        return;
    }
    var visitor = Visitor.getInstance(TNT.a.b.imsOrgId);
    if (visitor.isAllowed()) {
        var addVisitorValueToUrl = function (param, getter, mboxName) {
            if (visitor[getter]) {
                var callback = function (value) {
                    if (value) {
                        u.addParameter(param, value);
                    }
                };
                var value;
                if (typeof mboxName != 'undefined') {
                    value = visitor[getter]("mbox:" + mboxName);
                } else {
                    value = visitor[getter](callback);
                }
                callback(value);
            }
        };
        addVisitorValueToUrl(TNT.a.c.e, "getMarketingCloudVisitorID");
        addVisitorValueToUrl(TNT.a.c.f, "getAudienceManagerLocationHint");
        addVisitorValueToUrl(TNT.a.c.g, "getAudienceManagerBlob");
        addVisitorValueToUrl(TNT.a.c.h, "getAnalyticsVisitorID");
        addVisitorValueToUrl(TNT.a.c.i, "getSupplementalDataID", x);
    }
};
mboxFactory.prototype.create = function (x, qb, xc) {
    if (!this.isSupported()) {
        return null;
    }
    var yc = new Date();
    var A = yc.getTime() - (yc.getTimezoneOffset() * TNT.a.L.P);
    var Hb = this.Hb.clone();
    Hb.addParameter(TNT.a.c.j, this.Pb.length() + 1);
    Hb.addParameter(TNT.a.c.A, A);
    Hb.addParameters(qb);
    this.vc(Hb);
    this.wc(Hb, x);
    this.setVisitorIdParameters(Hb, x);
    var y, zc, ic;
    if (xc) {
        zc = new mboxLocatorNode(xc);
    } else {
        if (this.Ob) {
            throw 'The page has already been loaded, can\'t write marker';
        }
        zc = new mboxLocatorDefault(this.Ac(x));
    }
    try {
        y = this.Pb.get(x).length();
        ic = new mbox(x, y, Hb, zc, this.Bc(x), this);
        if (this.Rb) {
            ic.setFetcher(this.Ob ? new mboxAjaxFetcher() : new mboxStandardFetcher());
        }
        var hc = this;
        ic.setOnError(function (Cc, xb) {
            ic.setMessage(Cc);
            ic.activate();
            if (!ic.isActivated()) {
                hc.disable(TNT.a.b.mboxFactoryDisabledTimeout, Cc);
                window.location.reload(false);
            }
        });
        this.Pb.add(ic);
    } catch (Dc) {
        this.disable();
        throw 'Failed creating mbox "' + x + '", the error was: ' + Dc;
    }
    return ic;
};
mboxFactory.prototype.vc = function (Hb) {
    var m = this.dc.getId();
    if (m) {
        Hb.addParameter(TNT.a.c.m, m);
    }
};
mboxFactory.prototype.wc = function (Hb, x) {
    var Ec = !TNT.isAutoCreateGlobalMbox() && TNT.getGlobalMboxName() === x;
    if (Ec) {
        Hb.addParameters(TNT.getTargetPageParameters());
    }
};
mboxFactory.prototype.getCookieManager = function () {
    return this.Ub;
};
mboxFactory.prototype.getPageId = function () {
    return this.Wb;
};
mboxFactory.prototype.getPCId = function () {
    return this.dc;
};
mboxFactory.prototype.getSessionId = function () {
    return this.cc;
};
mboxFactory.prototype.getSignaler = function () {
    return this.lc;
};
mboxFactory.prototype.getUrlBuilder = function () {
    return this.Hb;
};
mboxFactory.prototype.Fc = function (x) {
    return this.Mb + '-' + x + '-' + this.Pb.get(x).length();
};
mboxFactory.prototype.Ac = function (x) {
    return TNT.a.L.R + this.Fc(x);
};
mboxFactory.prototype.Bc = function (x) {
    return TNT.a.L.O + this.Fc(x);
};
mboxFactory.prototype.ec = function (Hb, Sb, Nb) {
    Hb.addParameter(TNT.a.c.k, document.location.hostname);
    Hb.addParameter(TNT.a.c.d, this.Wb);
    Hb.addParameter(TNT.a.c.n, this.Xb);
    Hb.addParameter(TNT.a.c.o, this.Yb);
    Hb.addParameter(TNT.a.c.p, this.Zb);
    Hb.addParameter(TNT.a.c.q, this._b);
    Hb.addParameter(TNT.a.c.r, this.bc);
    Hb.addParameter(TNT.a.c.s, this.ac);
    Hb.addParameter(TNT.a.C.E, this.cc.getId());
    if (!Sb) {
        Hb.addParameter(TNT.a.c.l, this.Mb);
    }
    this.vc(Hb);
    if (TNT.a.b.crossDomainEnabled) {
        Hb.addParameter(TNT.a.c.t, TNT.a.b.crossDomain);
    }
    var c = TNT.getClientMboxExtraParameters();
    if (c) {
        Hb.addParameters(c.split('&'));
    }
    Hb.setUrlProcessAction(function (u) {
        if (TNT.a.b.passPageParameters) {
            u += '&';
            u += TNT.a.c.u;
            u += '=' + encodeURIComponent(document.location);
            var v = encodeURIComponent(document.referrer);
            if (u.length + v.length < 2000) {
                u += '&';
                u += TNT.a.c.v;
                u += '=' + v;
            }
        }
        u += '&';
        u += TNT.a.c.w;
        u += '=' + Nb;
        return u;
    });
};
mboxFactory.prototype.kc = function () {
    document.write('<style>.' + TNT.a.L.Q + ' { visibility:hidden; }</style>');
};
mboxFactory.prototype.isDomLoaded = function () {
    return this.Ob;
};
mboxFactory.prototype.Vb = function () {
    if (this.uc) {
        return;
    }
    this.uc = [];
    var hc = this;
    (function () {
        var Gc = document.addEventListener ? "DOMContentLoaded" : "onreadystatechange";
        var Hc = false;
        var Ic = function () {
            if (Hc) {
                return;
            }
            Hc = true;
            for (var i = 0; i < hc.uc.length; ++i) {
                hc.uc[i]();
            }
        };
        if (document.addEventListener) {
            document.addEventListener(Gc, function () {
                document.removeEventListener(Gc, arguments.callee, false);
                Ic();
            }, false);
            window.addEventListener("load", function () {
                document.removeEventListener("load", arguments.callee, false);
                Ic();
            }, false);
        } else if (document.attachEvent) {
            if (self !== self.top) {
                document.attachEvent(Gc, function () {
                    if (document.readyState === 'complete') {
                        document.detachEvent(Gc, arguments.callee);
                        Ic();
                    }
                });
            } else {
                var Jc = function () {
                    try {
                        document.documentElement.doScroll('left');
                        Ic();
                    } catch (Kc) {
                        setTimeout(Jc, 13);
                    }
                };
                Jc();
            }
        }
        if (document.readyState === "complete") {
            Ic();
        }
    })();
};
mboxSignaler = function (Lc) {
    this.Mc = document;
    this.Lc = Lc;
};
mboxSignaler.prototype.signal = function (Nc, x) {
    if (!this.Lc.isEnabled()) {
        return;
    }
    var Oc = this.Pc(this.Lc.Ac(x));
    this.Qc(this.Mc.body, Oc);
    var ic = this.Lc.create(x, mboxShiftArray(arguments), Oc);
    var Hb = ic.getUrlBuilder();
    Hb.addParameter(TNT.a.c.d, mboxGenerateId());
    ic.load();
};
mboxSignaler.prototype.Pc = function (Rc) {
    var db = this.Mc.createElement('DIV');
    db.id = Rc;
    db.style.visibility = 'hidden';
    db.style.display = 'none';
    return db;
};
mboxSignaler.prototype.Qc = function (Sc, Tc) {
    Sc.appendChild(Tc);
};
mboxList = function () {
    this.Pb = [];
};
mboxList.prototype.add = function (ic) {
    var T = TNT.a.T;
    if (T.U(ic) || T.X(ic)) {
        return;
    }
    this.Pb[this.Pb.length] = ic;
};
mboxList.prototype.get = function (x) {
    var db = new mboxList();
    for (var ub = 0; ub < this.Pb.length; ub++) {
        var ic = this.Pb[ub];
        if (ic.getName() == x) {
            db.add(ic);
        }
    }
    return db;
};
mboxList.prototype.getById = function (Uc) {
    return this.Pb[Uc];
};
mboxList.prototype.length = function () {
    return this.Pb.length;
};
mboxList.prototype.each = function (zb) {
    if (typeof zb !== 'function') {
        throw 'Action must be a function, was: ' + typeof (zb);
    }
    for (var ub = 0; ub < this.Pb.length; ub++) {
        zb(this.Pb[ub]);
    }
};
mboxLocatorDefault = function (Vc) {
    this.Vc = Vc;
    document.write('<div id="' + this.Vc + '" style="visibility:hidden;display:none">&nbsp;<\/div>');
};
mboxLocatorDefault.prototype.locate = function () {
    var Wc = 1;
    var Tc = document.getElementById(this.Vc);
    while (Tc) {
        if (Tc.nodeType == Wc) {
            if (Tc.className == 'mboxDefault') {
                return Tc;
            }
        }
        Tc = Tc.previousSibling;
    }
    return null;
};
mboxLocatorDefault.prototype.force = function () {
    var Xc = document.createElement('div');
    Xc.className = 'mboxDefault';
    var Yc = document.getElementById(this.Vc);
    if (Yc) {
        Yc.parentNode.insertBefore(Xc, Yc);
    }
    return Xc;
};
mboxLocatorNode = function (Tc) {
    this.Tc = Tc;
};
mboxLocatorNode.prototype.locate = function () {
    return typeof this.Tc == 'string' ? document.getElementById(this.Tc) : this.Tc;
};
mboxLocatorNode.prototype.force = function () {
    return null;
};
mboxCreate = function (x) {
    var ic = mboxFactoryDefault.create(x, mboxShiftArray(arguments));
    if (ic && mboxFactoryDefault.isEnabled()) {
        ic.load();
    }
    return ic;
};
mboxDefine = function (xc, x) {
    var ic = mboxFactoryDefault.create(x, mboxShiftArray(mboxShiftArray(arguments)), xc);
    return ic;
};
mboxUpdate = function (x) {
    mboxFactoryDefault.update(x, mboxShiftArray(arguments));
};
mbox = function (tb, Rc, Hb, Zc, _c, Lc) {
    this.ad = null;
    this.bd = 0;
    this.zc = Zc;
    this._c = _c;
    this.cd = null;
    this.dd = new mboxOfferContent();
    this.Xc = null;
    this.Hb = Hb;
    this.message = '';
    this.ed = {};
    this.fd = 0;
    this.Rc = Rc;
    this.tb = tb;
    this.gd();
    Hb.addParameter(TNT.a.c.x, tb);
    Hb.addParameter(TNT.a.c.y, Rc);
    this.hd = function () { };
    this.id = function () { };
    this.jd = null;
    this.kd = document.documentMode >= 10 && !Lc.isDomLoaded();
    if (this.kd) {
        this.ld = TNT.a.nestedMboxes;
        this.ld.push(this.tb);
    }
};
mbox.prototype.getId = function () {
    return this.Rc;
};
mbox.prototype.gd = function () {
    var maxLength = TNT.a.L.S;
    if (this.tb.length > maxLength) {
        throw "Mbox Name " + this.tb + " exceeds max length of " + maxLength + " characters.";
    } else if (this.tb.match(/^\s+|\s+$/g)) {
        throw "Mbox Name " + this.tb + " has leading/trailing whitespace(s).";
    }
};
mbox.prototype.getName = function () {
    return this.tb;
};
mbox.prototype.getParameters = function () {
    var qb = this.Hb.getParameters();
    var db = [];
    for (var ub = 0; ub < qb.length; ub++) {
        if (qb[ub].name.indexOf('mbox') !== 0) {
            db[db.length] = qb[ub].name + '=' + qb[ub].value;
        }
    }
    return db;
};
mbox.prototype.setOnLoad = function (zb) {
    this.id = zb;
    return this;
};
mbox.prototype.setMessage = function (Cc) {
    this.message = Cc;
    return this;
};
mbox.prototype.setOnError = function (hd) {
    this.hd = hd;
    return this;
};
mbox.prototype.setFetcher = function (md) {
    if (this.cd) {
        this.cd.cancel();
    }
    this.cd = md;
    return this;
};
mbox.prototype.getFetcher = function () {
    return this.cd;
};
mbox.prototype.load = function (qb) {
    if (this.cd === null) {
        return this;
    }
    this.setEventTime("load.start");
    this.cancelTimeout();
    this.bd = 0;
    var Hb = (qb && qb.length > 0) ? this.Hb.clone().addParameters(qb) : this.Hb;
    this.cd.fetch(Hb);
    var hc = this;
    this.nd = setTimeout(function () {
        hc.hd('browser timeout', hc.cd.getType());
    }, TNT.a.b.mboxTimeout);
    this.setEventTime("load.end");
    return this;
};
mbox.prototype.loaded = function () {
    this.cancelTimeout();
    if (!this.activate()) {
        var hc = this;
        setTimeout(function () {
            hc.loaded();
        }, TNT.a.b.mboxLoadedTimeout);
    }
};
mbox.prototype.activate = function () {
    if (this.bd) {
        return this.bd;
    }
    this.setEventTime('activate' + (++this.fd) + '.start');
    if (this.kd && this.ld[this.ld.length - 1] !== this.tb) {
        return this.bd;
    }
    if (this.show()) {
        this.cancelTimeout();
        this.bd = 1;
    }
    this.setEventTime('activate' + this.fd + '.end');
    if (this.kd) {
        this.ld.pop();
    }
    return this.bd;
};
mbox.prototype.isActivated = function () {
    return this.bd;
};
mbox.prototype.setOffer = function (dd) {
    if (dd && dd.show && dd.setOnLoad) {
        this.dd = dd;
    } else {
        throw 'Invalid offer';
    }
    return this;
};
mbox.prototype.getOffer = function () {
    return this.dd;
};
mbox.prototype.show = function () {
    this.setEventTime('show.start');
    var db = this.dd.show(this);
    this.setEventTime(db == 1 ? "show.end.ok" : "show.end");
    return db;
};
mbox.prototype.showContent = function (od) {
    if (od === null) {
        return 0;
    }
    if (this.Xc === null || !this.Xc.parentNode) {
        this.Xc = this.getDefaultDiv();
        if (this.Xc === null) {
            return 0;
        }
    }
    if (this.Xc !== od) {
        this.pd(this.Xc);
        this.Xc.parentNode.replaceChild(od, this.Xc);
        this.Xc = od;
    }
    this.qd(od);
    this.id();
    return 1;
};
mbox.prototype.hide = function () {
    this.setEventTime('hide.start');
    var db = this.showContent(this.getDefaultDiv());
    this.setEventTime(db == 1 ? 'hide.end.ok' : 'hide.end.fail');
    return db;
};
mbox.prototype.finalize = function () {
    this.setEventTime('finalize.start');
    this.cancelTimeout();
    if (!this.getDefaultDiv()) {
        if (this.zc.force()) {
            this.setMessage('No default content, an empty one has been added');
        } else {
            this.setMessage('Unable to locate mbox');
        }
    }
    if (!this.activate()) {
        this.hide();
        this.setEventTime('finalize.end.hide');
    }
    this.setEventTime('finalize.end.ok');
};
mbox.prototype.cancelTimeout = function () {
    if (this.nd) {
        clearTimeout(this.nd);
    }
    if (this.cd) {
        this.cd.cancel();
    }
};
mbox.prototype.getDiv = function () {
    return this.Xc;
};
mbox.prototype.getDefaultDiv = function () {
    if (this.jd === null) {
        this.jd = this.zc.locate();
    }
    return this.jd;
};
mbox.prototype.setEventTime = function (rd) {
    this.ed[rd] = (new Date()).getTime();
};
mbox.prototype.getEventTimes = function () {
    return this.ed;
};
mbox.prototype.getImportName = function () {
    return this._c;
};
mbox.prototype.getURL = function () {
    return this.Hb.buildUrl();
};
mbox.prototype.getUrlBuilder = function () {
    return this.Hb;
};
mbox.prototype.sd = function (Xc) {
    return Xc.style.display != 'none';
};
mbox.prototype.qd = function (Xc) {
    this.td(Xc, true);
};
mbox.prototype.pd = function (Xc) {
    this.td(Xc, false);
};
mbox.prototype.td = function (Xc, ud) {
    Xc.style.visibility = ud ? "visible" : "hidden";
    Xc.style.display = ud ? "block" : "none";
};
mbox.prototype.jc = function () {
    this.kd = false;
};
mbox.prototype.relocateDefaultDiv = function () {
    this.jd = this.zc.locate();
};
mboxOfferContent = function () {
    this.id = function () { };
};
mboxOfferContent.prototype.show = function (ic) {
    var db = ic.showContent(document.getElementById(ic.getImportName()));
    if (db == 1) {
        this.id();
    }
    return db;
};
mboxOfferContent.prototype.setOnLoad = function (id) {
    this.id = id;
};
mboxOfferAjax = function (od) {
    this.od = od;
    this.id = function () { };
};
mboxOfferAjax.prototype.setOnLoad = function (id) {
    this.id = id;
};
mboxOfferAjax.prototype.show = function (ic) {
    var vd = document.createElement('div');
    vd.id = ic.getImportName();
    vd.innerHTML = this.od;
    var db = ic.showContent(vd);
    if (db == 1) {
        this.id();
    }
    return db;
};
mboxOfferDefault = function () {
    this.id = function () { };
};
mboxOfferDefault.prototype.setOnLoad = function (id) {
    this.id = id;
};
mboxOfferDefault.prototype.show = function (ic) {
    var db = ic.hide();
    if (db == 1) {
        this.id();
    }
    return db;
};
mboxCookieManager = function mboxCookieManager(tb, wd) {
    this.tb = tb;
    this.wd = wd === '' || wd.indexOf('.') === -1 ? '' : '; domain=' + wd;
    this.xd = new mboxMap();
    this.loadCookies();
};
mboxCookieManager.prototype.isEnabled = function () {
    this.setCookie(TNT.a.H.J, 'true', 60);
    this.loadCookies();
    return this.getCookie(TNT.a.H.J) == 'true';
};
mboxCookieManager.prototype.setCookie = function (tb, V, nc) {
    if (typeof tb != 'undefined' && typeof V != 'undefined' && typeof nc != 'undefined') {
        var yd = {};
        yd.name = tb;
        yd.value = encodeURIComponent(V);
        yd.expireOn = Math.ceil(nc + new Date().getTime() / 1000);
        this.xd.put(tb, yd);
        this.saveCookies();
    }
};
mboxCookieManager.prototype.getCookie = function (tb) {
    var yd = this.xd.get(tb);
    return yd ? decodeURIComponent(yd.value) : null;
};
mboxCookieManager.prototype.deleteCookie = function (tb) {
    this.xd.remove(tb);
    this.saveCookies();
};
mboxCookieManager.prototype.getCookieNames = function (zd) {
    var Ad = [];
    this.xd.each(function (tb, yd) {
        if (tb.indexOf(zd) === 0) {
            Ad[Ad.length] = tb;
        }
    });
    return Ad;
};
mboxCookieManager.prototype.saveCookies = function () {
    var Bd = TNT.a.b.crossDomainXOnly;
    var Cd = TNT.a.H.D;
    var Dd = [];
    var Ed = 0;
    this.xd.each(function (tb, yd) {
        if (!Bd || tb === Cd) {
            Dd[Dd.length] = tb + '#' + yd.value + '#' + yd.expireOn;
            if (Ed < yd.expireOn) {
                Ed = yd.expireOn;
            }
        }
    });
    var Fd = new Date(Ed * 1000);
    var Gd = [];
    Gd.push(this.tb, '=', Dd.join('|'));
    if (TNT.a.b.usePersistentCookies) {
        Gd.push('; expires=', Fd.toGMTString());
    }
    Gd.push('; path=/', this.wd);
    document.cookie = Gd.join("");
};
mboxCookieManager.prototype.loadCookies = function () {
    this.xd = new mboxMap();
    var Hd = document.cookie.indexOf(this.tb + '=');
    if (Hd != -1) {
        var Id = document.cookie.indexOf(';', Hd);
        if (Id == -1) {
            Id = document.cookie.indexOf(',', Hd);
            if (Id == -1) {
                Id = document.cookie.length;
            }
        }
        var Jd = document.cookie.substring(Hd + this.tb.length + 1, Id).split('|');
        var Kd = Math.ceil(new Date().getTime() / 1000);
        for (var ub = 0; ub < Jd.length; ub++) {
            var yd = Jd[ub].split('#');
            if (Kd <= yd[2]) {
                var Ld = {};
                Ld.name = yd[0];
                Ld.value = yd[1];
                Ld.expireOn = yd[2];
                this.xd.put(Ld.name, Ld);
            }
        }
    }
};
mboxSession = function (Md, Nd, Tb, Od, Ub) {
    this.Nd = Nd;
    this.Tb = Tb;
    this.Od = Od;
    this.Ub = Ub;
    this.Rc = typeof mboxForceSessionId != 'undefined' ? mboxForceSessionId : mboxGetPageParameter(this.Nd);
    if (this.Rc === null || this.Rc.length === 0) {
        this.Rc = Ub.getCookie(Tb);
        if (this.Rc === null || this.Rc.length === 0) {
            this.Rc = Md;
        }
    }
    this.Ub.setCookie(Tb, this.Rc, Od);
};
mboxSession.prototype.getId = function () {
    return this.Rc;
};
mboxSession.prototype.forceId = function (mc) {
    this.Rc = mc;
    this.Ub.setCookie(this.Tb, this.Rc, this.Od);
};
mboxPC = function (Tb, Od, Ub) {
    this.Tb = Tb;
    this.Od = Od;
    this.Ub = Ub;
    this.Rc = typeof mboxForcePCId != 'undefined' ? mboxForcePCId : Ub.getCookie(Tb);
    if (this.Rc) {
        Ub.setCookie(Tb, this.Rc, Od);
    }
};
mboxPC.prototype.getId = function () {
    return this.Rc;
};
mboxPC.prototype.forceId = function (mc) {
    if (this.Rc != mc) {
        this.Rc = mc;
        this.Ub.setCookie(this.Tb, this.Rc, this.Od);
        return true;
    }
    return false;
};
mboxGetPageParameter = function (tb) {
    var db = null;
    var Pd = new RegExp("\\?[^#]*" + tb + "=([^\&;#]*)");
    var Qd = Pd.exec(document.location);
    if (Qd && Qd.length >= 2) {
        db = Qd[1];
    }
    return db;
};
mboxSetCookie = function (tb, V, nc) {
    return mboxFactoryDefault.getCookieManager().setCookie(tb, V, nc);
};
mboxGetCookie = function (tb) {
    return mboxFactoryDefault.getCookieManager().getCookie(tb);
};
mboxCookiePageDomain = function () {
    var wd = (/([^:]*)(:[0-9]{0,5})?/).exec(document.location.host)[1];
    var Rd = /[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}/;
    if (!Rd.exec(wd)) {
        var Sd = (/([^\.]+\.[^\.]{3}|[^\.]+\.[^\.]+\.[^\.]{2})$/).exec(wd);
        if (Sd) {
            wd = Sd[0];
            if (wd.indexOf("www.") === 0) {
                wd = wd.substr(4);
            }
        }
    }
    return wd ? wd : "";
};
mboxShiftArray = function (Td) {
    var db = [];
    for (var ub = 1; ub < Td.length; ub++) {
        db[db.length] = Td[ub];
    }
    return db;
};
mboxGenerateId = function () {
    return (new Date()).getTime() + "-" + Math.floor(Math.random() * 999999);
};
mboxScreenHeight = function () {
    return screen.height;
};
mboxScreenWidth = function () {
    return screen.width;
};
mboxBrowserWidth = function () {
    return (window.innerWidth) ? window.innerWidth : document.documentElement ? document.documentElement.clientWidth : document.body.clientWidth;
};
mboxBrowserHeight = function () {
    return (window.innerHeight) ? window.innerHeight : document.documentElement ? document.documentElement.clientHeight : document.body.clientHeight;
};
mboxBrowserTimeOffset = function () {
    return -new Date().getTimezoneOffset();
};
mboxScreenColorDepth = function () {
    return screen.pixelDepth;
};
TNT.a.Ud = function (Vd, Wd, Tb, nc, Ub) {
    if (!Wd.targetJSLoaded) {
        Ub.setCookie(Tb, true, nc);
        Vd.location.reload();
    }
};
TNT.a.Xd = function (Vd, Mc, b, H, Ub) {
    var Yd = '_AT';
    var _d = 50;
    var Tb = H.K;
    var nc = b.experienceManagerDisabledTimeout;
    var ad = b.experienceManagerTimeout;
    var u = b.experienceManagerPluginUrl;
    var ae = Vd.setTimeout;
    var be = function (ce) { };
    var de = function (ce) {
        ae(function () {
            Vd[Yd].applyWhenReady(ce);
        }, _d);
    };
    if (Yd in Vd) {
        return;
    }
    Vd[Yd] = {};
    if (Ub.getCookie(Tb) !== 'true') {
        Mc.write('<scr' + 'ipt src="' + u + '"><\/sc' + 'ript>');
        Vd[Yd].applyWhenReady = de;
        ae(function () {
            TNT.a.Ud(Vd, Vd[Yd], Tb, nc, Ub);
        }, ad);
    } else {
        Vd[Yd].applyWhenReady = be;
    }
};
mboxVizTargetUrl = function (x) {
    if (!mboxFactoryDefault.isEnabled()) {
        return;
    }
    var c = TNT.a.c;
    var P = TNT.a.L.P;
    var pb = TNT.a.b.clientCode;
    var yc = new Date();
    var ee = yc.getTimezoneOffset() * P;
    var Hb = mboxFactoryDefault.getUrlBuilder().clone();
    Hb.setBasePath('/m2/' + pb + '/viztarget');
    Hb.addParameter(c.x, x);
    Hb.addParameter(c.y, 0);
    Hb.addParameter(c.j, mboxFactoryDefault.getMboxes().length() + 1);
    Hb.addParameter(c.A, yc.getTime() - ee);
    Hb.addParameter(c.d, mboxGenerateId());
    Hb.addParameter(c.z, mboxFactoryDefault.isDomLoaded());
    var qb = mboxShiftArray(arguments);
    if (qb && qb.length > 0) {
        Hb.addParameters(qb);
    }
    mboxFactoryDefault.vc(Hb);
    mboxFactoryDefault.wc(Hb, x);
    mboxFactoryDefault.setVisitorIdParameters(Hb, x);
    return Hb.buildUrl();
};
TNT.createGlobalMbox = function () {
    var fe = TNT.getGlobalMboxName();
    var ge = TNT.getGlobalMboxLocation();
    var he;
    if (!ge) {
        ge = "mbox-" + fe + "-" + mboxGenerateId();
        he = document.createElement("div");
        he.className = "mboxDefault";
        he.id = ge;
        he.style.visibility = "hidden";
        he.style.display = "none";
        var ie = setInterval(function () {
            if (document.body) {
                clearInterval(ie);
                document.body.insertBefore(he, document.body.firstChild);
            }
        }, TNT.a.b.bodyPollingTimeout);
    }
    var je = TNT.getTargetPageParameters();
    var ke = mboxFactoryDefault.create(fe, je, ge);
    if (ke && mboxFactoryDefault.isEnabled()) {
        ke.load();
    }
};
TNT.a.le = function (Ub, me, ne) {
    return mboxGetPageParameter(me) || Ub.getCookie(ne);
};
TNT.a.oe = function (b) {
    setTimeout(function () {
        if (typeof mboxDebugLoaded == 'undefined') {
            alert('Could not load the remote debug.\nPlease check your connection to ' + b.companyName + ' servers');
        }
    }, 60 * 60);
    var u = b.adminUrl + '/mbox/mbox_debug.jsp?mboxServerHost=' + b.serverHost + '&clientCode=' + b.clientCode;
    document.write('<' + 'scr' + 'ipt src="' + u + '"><' + '\/scr' + 'ipt>');
};
TNT.a.pe = function (b) {
    var T = TNT.a.T;
    return !T.U(b) && !T.X(b) && T.bb(b);
};
TNT.a.qe = function (b, re) {
    var T = TNT.a.T;
    var se;
    var te;
    var V;
    for (var nb in b) {
        se = b.hasOwnProperty(nb) && re.hasOwnProperty(nb);
        V = b[nb];
        te = !T.U(V) && !T.X(V);
        if (se && te) {
            re[nb] = V;
        }
    }
    return re;
};
TNT.a.ue = function () {
    var b = window.targetGlobalSettings;
    if (TNT.a.pe(b)) {
        TNT.a.b = TNT.a.qe(b, TNT.a.b);
    }
    var Nb = TNT.a.b.mboxVersion;
    var ve = TNT.a.b.serverHost;
    var pb = TNT.a.b.clientCode;
    var M = TNT.a.L.M;
    var me = TNT.a.C.G;
    var ne = TNT.a.H.G;
    if (typeof mboxVersion == 'undefined') {
        window.mboxFactories = new mboxMap();
        window.mboxFactoryDefault = new mboxFactory(ve, pb, M, Nb);
        window.mboxVersion = Nb;
    }
    if (TNT.a.le(mboxFactoryDefault.getCookieManager(), me, ne)) {
        TNT.a.oe(TNT.a.b);
    }
};
TNT.a.ue();
(function () {
    var b = TNT.a.b;
    var H = TNT.a.H;
    var Ub = mboxFactoryDefault.getCookieManager();
    TNT.a.Xd(window, document, b, H, Ub);
}());
TNT.a.b.clientJavascriptFunction();
if (TNT.isAutoCreateGlobalMbox()) {
    TNT.createGlobalMbox();
}