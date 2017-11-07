/*
 ============== DO NOT ALTER ANYTHING BELOW THIS LINE ! ============

 Adobe Visitor API for JavaScript version: 2.1.0
 Copyright 1996-2015 Adobe, Inc. All Rights Reserved
 More info available at http://www.omniture.com
*/
function Visitor(q, w) {
    function x(t) {
        return function (e) {
            e = e || s.location.href;
            try {
                var n = a.Xa(e, t);
                if (n) return m.Hb(n)
            } catch (i) { }
        }
    }

    function B(t) {
        function e(t, e, n) {
            return n = n ? n += "|" : n, n + (t + "=" + encodeURIComponent(e))
        }
        for (var n = "", i = 0, a = t.length; a > i; i++) {
            var r = t[i],
                o = r[0],
                r = r[1];
            r != j && r !== u && (n = e(o, r, n))
        }
        return function (t) {
            var e = m.Da(),
                t = t ? t += "|" : t;
            return t + ("TS=" + e)
        }(n)
    }
    if (!q) throw "Visitor requires Adobe Marketing Cloud Org ID";
    var a = this;
    a.version = "2.1.0";
    var s = window,
        l = s.Visitor;
    l.version = a.version, s.s_c_in || (s.s_c_il = [], s.s_c_in = 0), a._c = "Visitor", a._il = s.s_c_il, a._in = s.s_c_in, a._il[a._in] = a, s.s_c_in++, a.na = {
        La: []
    };
    var v = s.document,
        j = l.Pb;
    j || (j = null);
    var F = l.Qb;
    F || (F = void 0);
    var i = l.Va;
    i || (i = !0);
    var k = l.Sa;
    k || (k = !1);
    var n = {
        r: !!s.postMessage,
        Ra: 1,
        ea: 864e5,
        ba: "adobe_mc",
        ca: "adobe_mc_sdid",
        w: /^[0-9a-fA-F\-]+$/,
        Qa: 5,
        Ta: /^\d+$/,
        fa: /vVersion\|((\d+\.)?(\d+\.)?(\*|\d+))(?=$|\|)/
    };
    a.Rb = n, a.ka = function (t) {
        var e, n, i = 0;
        if (t)
            for (e = 0; e < t.length; e++) n = t.charCodeAt(e), i = (i << 5) - i + n, i &= i;
        return i
    }, a.u = function (t, e) {
        var n, a, r = "0123456789",
            s = "",
            c = "",
            u = 8,
            l = 10,
            d = 10;
        if (e === o && (y.isClientSideMarketingCloudVisitorID = i), 1 == t) {
            for (r += "ABCDEF", n = 0; 16 > n; n++) a = Math.floor(Math.random() * u), s += r.substring(a, a + 1), a = Math.floor(Math.random() * u), c += r.substring(a, a + 1), u = 16;
            return s + "-" + c
        }
        for (n = 0; 19 > n; n++) a = Math.floor(Math.random() * l), s += r.substring(a, a + 1), 0 == n && 9 == a ? l = 3 : (1 == n || 2 == n) && 10 != l && 2 > a ? l = 10 : n > 2 && (l = 10), a = Math.floor(Math.random() * d), c += r.substring(a, a + 1), 0 == n && 9 == a ? d = 3 : (1 == n || 2 == n) && 10 != d && 2 > a ? d = 10 : n > 2 && (d = 10);
        return s + c
    }, a.Ya = function () {
        var t;
        if (!t && s.location && (t = s.location.hostname), t)
            if (/^[0-9.]+$/.test(t)) t = "";
            else {
                var e = t.split("."),
                    n = e.length - 1,
                    i = n - 1;
                if (n > 1 && 2 >= e[n].length && (2 == e[n - 1].length || 0 > ",ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,ax,az,ba,bb,be,bf,bg,bh,bi,bj,bm,bo,br,bs,bt,bv,bw,by,bz,ca,cc,cd,cf,cg,ch,ci,cl,cm,cn,co,cr,cu,cv,cw,cx,cz,de,dj,dk,dm,do,dz,ec,ee,eg,es,et,eu,fi,fm,fo,fr,ga,gb,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gw,gy,hk,hm,hn,hr,ht,hu,id,ie,im,in,io,iq,ir,is,it,je,jo,jp,kg,ki,km,kn,kp,kr,ky,kz,la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly,ma,mc,md,me,mg,mh,mk,ml,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,na,nc,ne,nf,ng,nl,no,nr,nu,nz,om,pa,pe,pf,ph,pk,pl,pm,pn,pr,ps,pt,pw,py,qa,re,ro,rs,ru,rw,sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sx,sy,sz,tc,td,tf,tg,th,tj,tk,tl,tm,tn,to,tp,tr,tt,tv,tw,tz,ua,ug,uk,us,uy,uz,va,vc,ve,vg,vi,vn,vu,wf,ws,yt,".indexOf("," + e[n] + ",")) && i--, i > 0)
                    for (t = ""; n >= i;) t = e[n] + (t ? "." : "") + t, n--
            }
        return t
    }, a.cookieRead = function (t) {
        var t = encodeURIComponent(t),
            e = (";" + v.cookie).split(" ").join(";"),
            n = e.indexOf(";" + t + "="),
            i = 0 > n ? n : e.indexOf(";", n + 1);
        return 0 > n ? "" : decodeURIComponent(e.substring(n + 2 + t.length, 0 > i ? e.length : i))
    }, a.cookieWrite = function (t, e, n) {
        var i, r = a.cookieLifetime,
            e = "" + e,
            r = r ? ("" + r).toUpperCase() : "";
        return n && "SESSION" != r && "NONE" != r ? (i = "" != e ? parseInt(r ? r : 0, 10) : -60) ? (n = new Date, n.setTime(n.getTime() + 1e3 * i)) : 1 == n && (n = new Date, i = n.getYear(), n.setYear(i + 2 + (1900 > i ? 1900 : 0))) : n = 0, t && "NONE" != r ? (v.cookie = encodeURIComponent(t) + "=" + encodeURIComponent(e) + "; path=/;" + (n ? " expires=" + n.toGMTString() + ";" : "") + (a.cookieDomain ? " domain=" + a.cookieDomain + ";" : ""), a.cookieRead(t) == e) : 0
    }, a.h = j, a.z = function (t, e) {
        try {
            "function" == typeof t ? t.apply(s, e) : t[1].apply(t[0], e)
        } catch (n) { }
    }, a.M = function (t, e) {
        e && (a.h == j && (a.h = {}), a.h[t] == F && (a.h[t] = []), a.h[t].push(e))
    }, a.t = function (t, e) {
        if (a.h != j) {
            var n = a.h[t];
            if (n)
                for (; 0 < n.length;) a.z(n.shift(), e)
        }
    }, a.s = function (t, e, n, i) {
        if (n = encodeURIComponent(e) + "=" + encodeURIComponent(n), e = m.Fb(t), t = m.wb(t), -1 === t.indexOf("?")) return t + "?" + n + e;
        var a = t.split("?"),
            t = a[0] + "?",
            i = m.ib(a[1], n, i);
        return t + i + e
    }, a.Xa = function (t, e) {
        var n = RegExp("[\\?&#]" + e + "=([^&#]*)").exec(t);
        return n && n.length ? decodeURIComponent(n[1]) : void 0
    }, a.eb = x(n.ba), a.fb = x(n.ca), a.ha = function () {
        var t = a.fb(void 0);
        t && t.SDID && t[G] === q && (a._supplementalDataIDCurrent = t.SDID, a._supplementalDataIDCurrentConsumed.SDID_URL_PARAM = i)
    }, a.ga = function () {
        var e = a.eb();
        if (e && e.TS && !(Math.floor((m.Da() - e.TS) / 60) > n.Qa || e[G] !== q)) {
            var i = e[o],
                s = a.setMarketingCloudVisitorID;
            i && i.match(n.w) && s(i), a.j(t, -1), e = e[r], i = a.setAnalyticsVisitorID, e && e.match(n.w) && i(e)
        }
    }, a.cb = function (t) {
        function e(t) {
            m.Ga(t) && a.setCustomerIDs(t)
        }

        function n(t) {
            t = t || {}, a._supplementalDataIDCurrent = t.supplementalDataIDCurrent || "", a._supplementalDataIDCurrentConsumed = t.supplementalDataIDCurrentConsumed || {}, a._supplementalDataIDLast = t.supplementalDataIDLast || "", a._supplementalDataIDLastConsumed = t.supplementalDataIDLastConsumed || {}
        }
        if (t) try {
            if (t = m.Ga(t) ? t : m.Gb(t), t[a.marketingCloudOrgID]) {
                var i = t[a.marketingCloudOrgID];
                e(i.customerIDs), n(i.sdid)
            }
        } catch (r) {
            throw Error("`serverState` has an invalid format.")
        }
    }, a.l = j, a.$a = function (t, e, n, r) {
        e = a.s(e, "d_fieldgroup", t, 1), r.url = a.s(r.url, "d_fieldgroup", t, 1), r.m = a.s(r.m, "d_fieldgroup", t, 1), y.d[t] = i, r === Object(r) && r.m && "XMLHttpRequest" === a.pa.F.G ? a.pa.rb(r, n, t) : a.useCORSOnly || a.ab(t, e, n)
    }, a.ab = function (t, e, n) {
        var r, o = 0,
            s = 0;
        if (e && v) {
            for (r = 0; !o && 2 > r;) {
                try {
                    o = (o = v.getElementsByTagName(r > 0 ? "HEAD" : "head")) && 0 < o.length ? o[0] : 0
                } catch (c) {
                    o = 0
                }
                r++
            }
            if (!o) try {
                v.body && (o = v.body)
            } catch (u) {
                o = 0
            }
            if (o)
                for (r = 0; !s && 2 > r;) {
                    try {
                        s = v.createElement(r > 0 ? "SCRIPT" : "script")
                    } catch (l) {
                        s = 0
                    }
                    r++
                }
        }
        e && o && s ? (s.type = "text/javascript", s.src = e, o.firstChild ? o.insertBefore(s, o.firstChild) : o.appendChild(s), o = a.loadTimeout, p.d[t] = {
            requestStart: p.p(),
            url: e,
            xa: o,
            va: p.Ca(),
            wa: 0
        }, n && (a.l == j && (a.l = {}), a.l[t] = setTimeout(function () {
            n(i)
        }, o)), a.na.La.push(e)) : n && n()
    }, a.Wa = function (t) {
        a.l != j && a.l[t] && (clearTimeout(a.l[t]), a.l[t] = 0)
    }, a.la = k, a.ma = k, a.isAllowed = function () {
        return !a.la && (a.la = i, a.cookieRead(a.cookieName) || a.cookieWrite(a.cookieName, "T", 1)) && (a.ma = i), a.ma
    }, a.b = j, a.c = j;
    var H = l.gc;
    H || (H = "MC");
    var o = l.nc;
    o || (o = "MCMID");
    var G = l.kc;
    G || (G = "MCORGID");
    var I = l.hc;
    I || (I = "MCCIDH");
    var M = l.lc;
    M || (M = "MCSYNCS");
    var K = l.mc;
    K || (K = "MCSYNCSOP");
    var L = l.ic;
    L || (L = "MCIDTS");
    var C = l.jc;
    C || (C = "MCOPTOUT");
    var E = l.ec;
    E || (E = "A");
    var r = l.bc;
    r || (r = "MCAID");
    var D = l.fc;
    D || (D = "AAM");
    var A = l.dc;
    A || (A = "MCAAMLH");
    var t = l.cc;
    t || (t = "MCAAMB");
    var u = l.oc;
    u || (u = "NONE"), a.N = 0, a.ja = function () {
        if (!a.N) {
            var t = a.version;
            a.audienceManagerServer && (t += "|" + a.audienceManagerServer), a.audienceManagerServerSecure && (t += "|" + a.audienceManagerServerSecure), a.N = a.ka(t)
        }
        return a.N
    }, a.oa = k, a.f = function () {
        if (!a.oa) {
            a.oa = i;
            var t, e, o, s, c = a.ja(),
                u = k,
                l = a.cookieRead(a.cookieName),
                d = new Date;
            if (a.b == j && (a.b = {}), l && "T" != l)
                for (l = l.split("|"), l[0].match(/^[\-0-9]+$/) && (parseInt(l[0], 10) != c && (u = i), l.shift()), 1 == l.length % 2 && l.pop(), c = 0; c < l.length; c += 2) t = l[c].split("-"), e = t[0], o = l[c + 1], 1 < t.length ? (s = parseInt(t[1], 10), t = 0 < t[1].indexOf("s")) : (s = 0, t = k), u && (e == I && (o = ""), s > 0 && (s = d.getTime() / 1e3 - 60)), e && o && (a.e(e, o, 1), s > 0 && (a.b["expire" + e] = s + (t ? "s" : ""), d.getTime() >= 1e3 * s || t && !a.cookieRead(a.sessionCookieName))) && (a.c || (a.c = {}), a.c[e] = i);
            !a.a(r) && m.o() && (l = a.cookieRead("s_vi")) && (l = l.split("|"), 1 < l.length && 0 <= l[0].indexOf("v1") && (o = l[1], c = o.indexOf("["), c >= 0 && (o = o.substring(0, c)), o && o.match(n.w) && a.e(r, o)))
        }
    }, a._appendVersionTo = function (t) {
        var e = "vVersion|" + a.version,
            i = Boolean(t) ? a._getCookieVersion(t) : null;
        return i ? m.jb(i, a.version) && (t = t.replace(n.fa, e)) : t += (t ? "|" : "") + e, t
    }, a.hb = function () {
        var t, e, n = a.ja();
        for (t in a.b) !Object.prototype[t] && a.b[t] && "expire" != t.substring(0, 6) && (e = a.b[t], n += (n ? "|" : "") + t + (a.b["expire" + t] ? "-" + a.b["expire" + t] : "") + "|" + e);
        n = a._appendVersionTo(n), a.cookieWrite(a.cookieName, n, 1)
    }, a.a = function (t, e) {
        return a.b == j || !e && a.c && a.c[t] ? j : a.b[t]
    }, a.e = function (t, e, n) {
        a.b == j && (a.b = {}), a.b[t] = e, n || a.hb()
    }, a.Za = function (t, e) {
        var n = a.a(t, e);
        return n ? n.split("*") : j
    }, a.gb = function (t, e, n) {
        a.e(t, e ? e.join("*") : "", n)
    }, a.Wb = function (t, e) {
        var n = a.Za(t, e);
        if (n) {
            var i, r = {};
            for (i = 0; i < n.length; i += 2) r[n[i]] = n[i + 1];
            return r
        }
        return j
    }, a.Yb = function (t, e, n) {
        var i, r = j;
        if (e)
            for (i in r = [], e) Object.prototype[i] || (r.push(i), r.push(e[i]));
        a.gb(t, r, n)
    }, a.j = function (t, e, n) {
        var r = new Date;
        r.setTime(r.getTime() + 1e3 * e), a.b == j && (a.b = {}), a.b["expire" + t] = Math.floor(r.getTime() / 1e3) + (n ? "s" : ""), 0 > e ? (a.c || (a.c = {}), a.c[t] = i) : a.c && (a.c[t] = k), n && (a.cookieRead(a.sessionCookieName) || a.cookieWrite(a.sessionCookieName, "1"))
    }, a.ia = function (t) {
        return t && ("object" == typeof t && (t = t.d_mid ? t.d_mid : t.visitorID ? t.visitorID : t.id ? t.id : t.uuid ? t.uuid : "" + t), t && (t = t.toUpperCase(), "NOTARGET" == t && (t = u)), !t || t != u && !t.match(n.w)) && (t = ""), t
    }, a.k = function (e, n) {
        if (a.Wa(e), a.i != j && (a.i[e] = k), p.d[e] && (p.d[e].Nb = p.p(), p.J(e)), y.d[e] && y.Na(e, k), e == H) {
            y.isClientSideMarketingCloudVisitorID !== i && (y.isClientSideMarketingCloudVisitorID = k);
            var s = a.a(o);
            if (!s || a.overwriteCrossDomainMCIDAndAID) {
                if (s = "object" == typeof n && n.mid ? n.mid : a.ia(n), !s) {
                    if (a.D) return void a.getAnalyticsVisitorID(j, k, i);
                    s = a.u(0, o)
                }
                a.e(o, s)
            }
            s && s != u || (s = ""), "object" == typeof n && ((n.d_region || n.dcs_region || n.d_blob || n.blob) && a.k(D, n), a.D && n.mid && a.k(E, {
                id: n.id
            })), a.t(o, [s])
        }
        if (e == D && "object" == typeof n) {
            s = 604800, n.id_sync_ttl != F && n.id_sync_ttl && (s = parseInt(n.id_sync_ttl, 10));
            var c = a.a(A);
            c || ((c = n.d_region) || (c = n.dcs_region), c && (a.j(A, s), a.e(A, c))), c || (c = ""), a.t(A, [c]), c = a.a(t), (n.d_blob || n.blob) && ((c = n.d_blob) || (c = n.blob), a.j(t, s), a.e(t, c)), c || (c = ""), a.t(t, [c]), !n.error_msg && a.C && a.e(I, a.C)
        }
        if (e == E && (s = a.a(r), (!s || a.overwriteCrossDomainMCIDAndAID) && ((s = a.ia(n)) ? s !== u && a.j(t, -1) : s = u, a.e(r, s)), s && s != u || (s = ""), a.t(r, [s])), a.idSyncDisableSyncs ? z.Ea = i : (z.Ea = k, s = {}, s.ibs = n.ibs, s.subdomain = n.subdomain, z.Ib(s)), n === Object(n)) {
            var l;
            a.isAllowed() && (l = a.a(C)), l || (l = u, n.d_optout && n.d_optout instanceof Array && (l = n.d_optout.join(",")), s = parseInt(n.d_ottl, 10), isNaN(s) && (s = 7200), a.j(C, s, i), a.e(C, l)), a.t(C, [l])
        }
    }, a.i = j, a.v = function (e, n, s, c, l) {
        var d, f = "",
            h = m.yb(e);
        if (a.isAllowed())
            if (a.f(), f = a.a(e, N[e] === i), !(!f || a.c && a.c[e]) || a.disableThirdPartyCalls && !h) f || (e === o ? (a.M(e, s), f = a.u(0, o), a.setMarketingCloudVisitorID(f)) : e === r ? (a.M(e, s), f = "", a.setAnalyticsVisitorID(f)) : (f = "", c = i));
            else if (e == o || e == C ? d = H : e == A || e == t ? d = D : e == r && (d = E), d) return !n || a.i != j && a.i[d] || (a.i == j && (a.i = {}), a.i[d] = i, a.$a(d, n, function (t) {
                a.a(e) || (p.d[d] && (p.d[d].timeout = p.p(), p.d[d].xb = !!t, p.J(d)), t && y.Na(d, i), t = "", e == o ? t = a.u(0, o) : d == D && (t = {
                    error_msg: "timeout"
                }), a.k(d, t))
            }, l)), a.M(e, s), f ? f : (n || a.k(d, {
                id: u
            }), "");
        return e != o && e != r || f != u || (f = "", c = i), s && c && a.z(s, [f]), f
    }, a._setMarketingCloudFields = function (t) {
        a.f(), a.k(H, t)
    }, a.setMarketingCloudVisitorID = function (t) {
        a._setMarketingCloudFields(t)
    }, a.D = k, a.getMarketingCloudVisitorID = function (t, e) {
        if (a.isAllowed()) {
            a.marketingCloudServer && 0 > a.marketingCloudServer.indexOf(".demdex.net") && (a.D = i);
            var n = a.B("_setMarketingCloudFields");
            return a.v(o, n.url, t, e, n)
        }
        return ""
    }, a.bb = function (t) {
        a.getAudienceManagerBlob(t, i)
    }, l.AuthState = {
        UNKNOWN: 0,
        AUTHENTICATED: 1,
        LOGGED_OUT: 2
    }, a.A = {}, a.K = k, a.C = "", a.setCustomerIDs = function (t) {
        if (a.isAllowed() && t) {
            a.f();
            var e, n;
            for (e in t)
                if (!Object.prototype[e] && (n = t[e]))
                    if ("object" == typeof n) {
                        var r = {};
                        n.id && (r.id = n.id), n.authState != F && (r.authState = n.authState), a.A[e] = r
                    } else a.A[e] = {
                        id: n
                    };
            var t = a.getCustomerIDs(),
                r = a.a(I),
                o = "";
            r || (r = 0);
            for (e in t) Object.prototype[e] || (n = t[e], o += (o ? "|" : "") + e + "|" + (n.id ? n.id : "") + (n.authState ? n.authState : ""));
            a.C = a.ka(o), a.C != r && (a.K = i, a.bb(function () {
                a.K = k
            }))
        }
    }, a.getCustomerIDs = function () {
        a.f();
        var t, e, n = {};
        for (t in a.A) Object.prototype[t] || (e = a.A[t], n[t] || (n[t] = {}), e.id && (n[t].id = e.id), n[t].authState = e.authState != F ? e.authState : l.AuthState.UNKNOWN);
        return n
    }, a._setAnalyticsFields = function (t) {
        a.f(), a.k(E, t)
    }, a.setAnalyticsVisitorID = function (t) {
        a._setAnalyticsFields(t)
    }, a.getAnalyticsVisitorID = function (t, e, n) {
        if (!m.o() && !n) return a.z(t, [""]), "";
        if (a.isAllowed()) {
            var s = "";
            if (n || (s = a.getMarketingCloudVisitorID(function () {
                    a.getAnalyticsVisitorID(t, i)
            })), s || n) {
                var c = n ? a.marketingCloudServer : a.trackingServer,
                    u = "";
                a.loadSSL && (n ? a.marketingCloudServerSecure && (c = a.marketingCloudServerSecure) : a.trackingServerSecure && (c = a.trackingServerSecure));
                var l = {};
                if (c) {
                    var c = "http" + (a.loadSSL ? "s" : "") + "://" + c + "/id",
                        s = "d_visid_ver=" + a.version + "&mcorgid=" + encodeURIComponent(a.marketingCloudOrgID) + (s ? "&mid=" + encodeURIComponent(s) : "") + (a.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : ""),
                        d = ["s_c_il", a._in, "_set" + (n ? "MarketingCloud" : "Analytics") + "Fields"],
                        u = c + "?" + s + "&callback=s_c_il%5B" + a._in + "%5D._set" + (n ? "MarketingCloud" : "Analytics") + "Fields";
                    l.m = c + "?" + s, l.sa = d
                }
                return l.url = u, a.v(n ? o : r, u, t, e, l)
            }
        }
        return ""
    }, a._setAudienceManagerFields = function (t) {
        a.f(), a.k(D, t)
    }, a.B = function (e) {
        var n = a.audienceManagerServer,
            s = "",
            c = a.a(o),
            l = a.a(t, i),
            d = a.a(r),
            d = d && d != u ? "&d_cid_ic=AVID%01" + encodeURIComponent(d) : "";
        if (a.loadSSL && a.audienceManagerServerSecure && (n = a.audienceManagerServerSecure), n) {
            var f, h, s = a.getCustomerIDs();
            if (s)
                for (f in s) Object.prototype[f] || (h = s[f], d += "&d_cid_ic=" + encodeURIComponent(f) + "%01" + encodeURIComponent(h.id ? h.id : "") + (h.authState ? "%01" + h.authState : ""));
            return e || (e = "_setAudienceManagerFields"), n = "http" + (a.loadSSL ? "s" : "") + "://" + n + "/id", c = "d_visid_ver=" + a.version + "&d_rtbd=json&d_ver=2" + (!c && a.D ? "&d_verify=1" : "") + "&d_orgid=" + encodeURIComponent(a.marketingCloudOrgID) + "&d_nsid=" + (a.idSyncContainerID || 0) + (c ? "&d_mid=" + encodeURIComponent(c) : "") + (a.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : "") + (l ? "&d_blob=" + encodeURIComponent(l) : "") + d, l = ["s_c_il", a._in, e], s = n + "?" + c + "&d_cb=s_c_il%5B" + a._in + "%5D." + e, {
                url: s,
                m: n + "?" + c,
                sa: l
            }
        }
        return {
            url: s
        }
    }, a.getAudienceManagerLocationHint = function (t, e) {
        if (a.isAllowed() && a.getMarketingCloudVisitorID(function () {
                a.getAudienceManagerLocationHint(t, i)
        })) {
            var n = a.a(r);
            if (!n && m.o() && (n = a.getAnalyticsVisitorID(function () {
                    a.getAudienceManagerLocationHint(t, i)
            })), n || !m.o()) return n = a.B(), a.v(A, n.url, t, e, n)
        }
        return ""
    }, a.getLocationHint = a.getAudienceManagerLocationHint, a.getAudienceManagerBlob = function (e, n) {
        if (a.isAllowed() && a.getMarketingCloudVisitorID(function () {
                a.getAudienceManagerBlob(e, i)
        })) {
            var o = a.a(r);
            if (!o && m.o() && (o = a.getAnalyticsVisitorID(function () {
                    a.getAudienceManagerBlob(e, i)
            })), o || !m.o()) {
                var o = a.B(),
                    s = o.url;
                return a.K && a.j(t, -1), a.v(t, s, e, n, o)
            }
        }
        return ""
    }, a._supplementalDataIDCurrent = "", a._supplementalDataIDCurrentConsumed = {}, a._supplementalDataIDLast = "", a._supplementalDataIDLastConsumed = {}, a.getSupplementalDataID = function (t, e) {
        !a._supplementalDataIDCurrent && !e && (a._supplementalDataIDCurrent = a.u(1));
        var n = a._supplementalDataIDCurrent;
        return a._supplementalDataIDLast && !a._supplementalDataIDLastConsumed[t] ? (n = a._supplementalDataIDLast, a._supplementalDataIDLastConsumed[t] = i) : n && (a._supplementalDataIDCurrentConsumed[t] && (a._supplementalDataIDLast = a._supplementalDataIDCurrent, a._supplementalDataIDLastConsumed = a._supplementalDataIDCurrentConsumed, a._supplementalDataIDCurrent = n = e ? "" : a.u(1), a._supplementalDataIDCurrentConsumed = {}), n && (a._supplementalDataIDCurrentConsumed[t] = i)), n
    }, l.OptOut = {
        GLOBAL: "global"
    }, a.getOptOut = function (t, e) {
        if (a.isAllowed()) {
            var n = a.B("_setMarketingCloudFields");
            return a.v(C, n.url, t, e, n)
        }
        return ""
    }, a.isOptedOut = function (t, e, n) {
        return a.isAllowed() ? (e || (e = l.OptOut.GLOBAL), (n = a.getOptOut(function (n) {
            a.z(t, [n == l.OptOut.GLOBAL || 0 <= n.indexOf(e)])
        }, n)) ? n == l.OptOut.GLOBAL || 0 <= n.indexOf(e) : j) : k
    }, a.appendVisitorIDsTo = function (t) {
        var e = n.ba,
            i = B([
                [o, a.a(o)],
                [r, a.a(r)],
                [G, a.marketingCloudOrgID]
            ]);
        try {
            return a.s(t, e, i)
        } catch (s) {
            return t
        }
    }, a.appendSupplementalDataIDTo = function (t, e) {
        if (e = e || a.getSupplementalDataID(m.sb(), !0), !e) return t;
        var i, r = n.ca;
        i = "SDID=" + encodeURIComponent(e) + "|" + (G + "=" + encodeURIComponent(a.marketingCloudOrgID));
        try {
            return a.s(t, r, i)
        } catch (o) {
            return t
        }
    }, a.ra = {
        postMessage: function (t, e, i) {
            var a = 1;
            e && (n.r ? i.postMessage(t, e.replace(/([^:]+:\/\/[^\/]+).*/, "$1")) : e && (i.location = e.replace(/#.*$/, "") + "#" + +new Date + a++ + "&" + t))
        },
        X: function (t, e) {
            var i;
            try {
                n.r && (t && (i = function (n) {
                    return "string" == typeof e && n.origin !== e || "[object Function]" === Object.prototype.toString.call(e) && !1 === e(n.origin) ? !1 : void t(n)
                }), window.addEventListener ? window[t ? "addEventListener" : "removeEventListener"]("message", i, !1) : window[t ? "attachEvent" : "detachEvent"]("onmessage", i))
            } catch (a) { }
        }
    };
    var m = {
        O: function () {
            return v.addEventListener ? function (t, e, n) {
                t.addEventListener(e, function (t) {
                    "function" == typeof n && n(t)
                }, k)
            } : v.attachEvent ? function (t, e, n) {
                t.attachEvent("on" + e, function (t) {
                    "function" == typeof n && n(t)
                })
            } : void 0
        }(),
        map: function (t, e) {
            if (Array.prototype.map) return t.map(e);
            if (void 0 === t || t === j) throw new TypeError;
            var n = Object(t),
                i = n.length >>> 0;
            if ("function" != typeof e) throw new TypeError;
            for (var a = Array(i), r = 0; i > r; r++) r in n && (a[r] = e.call(e, n[r], r, n));
            return a
        },
        za: function (t, e) {
            return this.map(t, function (t) {
                return encodeURIComponent(t)
            }).join(e)
        },
        Fb: function (t) {
            var e = t.indexOf("#");
            return e > 0 ? t.substr(e) : ""
        },
        wb: function (t) {
            var e = t.indexOf("#");
            return e > 0 ? t.substr(0, e) : t
        },
        ib: function (t, e, n) {
            return t = t.split("&"), n = n != j ? n : t.length, t.splice(n, 0, e), t.join("&")
        },
        yb: function (t, e, n) {
            return t !== r ? k : (e || (e = a.trackingServer), n || (n = a.trackingServerSecure), t = a.loadSSL ? n : e, "string" == typeof t && t.length ? 0 > t.indexOf("2o7.net") && 0 > t.indexOf("omtrdc.net") : k)
        },
        Ga: function (t) {
            return Boolean(t && t === Object(t))
        },
        zb: function (t, e) {
            return 0 > a._compareVersions(t, e)
        },
        jb: function (t, e) {
            return 0 !== a._compareVersions(t, e)
        },
        Mb: function (t) {
            document.cookie = encodeURIComponent(t) + "=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;"
        },
        o: function () {
            return !!a.trackingServer || !!a.trackingServerSecure
        },
        Gb: function (a, b) {
            function c(t, e) {
                var n, i, a = t[e];
                if (a && "object" == typeof a)
                    for (n in a) Object.prototype.hasOwnProperty.call(a, n) && (i = c(a, n), void 0 !== i ? a[n] = i : delete a[n]);
                return b.call(t, e, a)
            }
            if ("object" == typeof JSON && "function" == typeof JSON.parse) return JSON.parse(a, b);
            var e;
            if (e = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, a = "" + a, e.lastIndex = 0, e.test(a) && (a = a.replace(e, function (t) {
                    return "\\u" + ("0000" + t.charCodeAt(0).toString(16)).slice(-4)
            })), /^[\],:{}\s]*$/.test(a.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, "@").replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]").replace(/(?:^|:|,)(?:\s*\[)+/g, ""))) return e = eval("(" + a + ")"), "function" == typeof b ? c({
                "": e
            }, "") : e;
            throw new SyntaxError("JSON.parse")
        },
        Da: function () {
            return Math.round((new Date).getTime() / 1e3)
        },
        Hb: function (t) {
            for (var e = {}, t = t.split("|"), n = 0, i = t.length; i > n; n++) {
                var a = t[n].split("=");
                e[a[0]] = decodeURIComponent(a[1])
            }
            return e
        },
        sb: function (t) {
            for (var t = t || 5, e = ""; t--;) e += "abcdefghijklmnopqrstuvwxyz0123456789"[Math.floor(36 * Math.random())];
            return e
        }
    };
    a.Xb = m, a.pa = {
        F: function () {
            var t = "none",
                e = i;
            return "undefined" != typeof XMLHttpRequest && XMLHttpRequest === Object(XMLHttpRequest) && ("withCredentials" in new XMLHttpRequest ? t = "XMLHttpRequest" : "undefined" != typeof XDomainRequest && XDomainRequest === Object(XDomainRequest) && (e = k), 0 < Object.prototype.toString.call(window.Ob).indexOf("Constructor") && (e = k)), {
                G: t,
                $b: e
            }
        }(),
        tb: function () {
            return "none" === this.F.G ? j : new window[this.F.G]
        },
        rb: function (t, e, n) {
            var r = this;
            e && (t.U = e);
            try {
                var o = this.tb();
                o.open("get", t.m + "&ts=" + (new Date).getTime(), i), "XMLHttpRequest" === this.F.G && (o.withCredentials = i, o.timeout = a.loadTimeout, o.setRequestHeader("Content-Type", "application/x-www-form-urlencoded"), o.onreadystatechange = function () {
                    if (4 === this.readyState && 200 === this.status) t: {
                        var e;
                        try {
                            if (e = JSON.parse(this.responseText), e !== Object(e)) {
                                r.n(t, j, "Response is not JSON");
                                break t
                            }
                        } catch (n) {
                            r.n(t, n, "Error parsing response as JSON");
                            break t
                        }
                        try {
                            for (var i = t.sa, a = window, o = 0; o < i.length; o++) a = a[i[o]];
                            a(e)
                        } catch (s) {
                            r.n(t, s, "Error forming callback function")
                        }
                    }
                }), o.onerror = function (e) {
                    r.n(t, e, "onerror")
                }, o.ontimeout = function (e) {
                    r.n(t, e, "ontimeout")
                }, o.send(), p.d[n] = {
                    requestStart: p.p(),
                    url: t.m,
                    xa: o.timeout,
                    va: p.Ca(),
                    wa: 1
                }, a.na.La.push(t.m)
            } catch (s) {
                this.n(t, s, "try-catch")
            }
        },
        n: function (t, e, n) {
            a.CORSErrors.push({
                ac: t,
                error: e,
                description: n
            }), t.U && ("ontimeout" === n ? t.U(i) : t.U(k))
        }
    };
    var z = {
        Ua: 3e4,
        da: 649,
        Pa: k,
        id: j,
        W: [],
        S: j,
        Ba: function (t) {
            return "string" == typeof t ? (t = t.split("/"), t[0] + "//" + t[2]) : void 0
        },
        g: j,
        url: j,
        ub: function () {
            var t = "http://fast.",
                e = "?d_nsid=" + a.idSyncContainerID + "#" + encodeURIComponent(v.location.href);
            return this.g || (this.g = "nosubdomainreturned"), a.loadSSL && (t = a.idSyncSSLUseAkamai ? "https://fast." : "https://"), t = t + this.g + ".demdex.net/dest5.html" + e, this.S = this.Ba(t), this.id = "destination_publishing_iframe_" + this.g + "_" + a.idSyncContainerID, t
        },
        mb: function () {
            var t = "?d_nsid=" + a.idSyncContainerID + "#" + encodeURIComponent(v.location.href);
            "string" == typeof a.L && a.L.length && (this.id = "destination_publishing_iframe_" + (new Date).getTime() + "_" + a.idSyncContainerID, this.S = this.Ba(a.L), this.url = a.L + t)
        },
        Ea: j,
        ya: k,
        Z: k,
        H: j,
        pc: j,
        Eb: j,
        qc: j,
        Y: k,
        I: [],
        Cb: [],
        Db: [],
        Ha: n.r ? 15 : 100,
        T: [],
        Ab: [],
        ta: i,
        Ka: k,
        Ja: function () {
            return !a.idSyncDisable3rdPartySyncing && (this.ya || a.Tb) && this.g && "nosubdomainreturned" !== this.g && this.url && !this.Z
        },
        Q: function () {
            function t() {
                a = document.createElement("iframe"), a.sandbox = "allow-scripts allow-same-origin", a.title = "Adobe ID Syncing iFrame", a.id = n.id, a.style.cssText = "display: none; width: 0; height: 0;", a.src = n.url, n.Eb = i, e(), document.body.appendChild(a)
            }

            function e() {
                m.O(a, "load", function () {
                    a.className = "aamIframeLoaded", n.H = i, n.q()
                })
            }
            this.Z = i;
            var n = this,
                a = document.getElementById(this.id);
            a ? "IFRAME" !== a.nodeName ? (this.id += "_2", t()) : "aamIframeLoaded" !== a.className ? e() : (this.H = i, this.Fa = a, this.q()) : t(), this.Fa = a
        },
        q: function (t) {
            var e = this;
            t === Object(t) && (this.T.push(t), this.Jb(t)), (this.Ka || !n.r || this.H) && this.T.length && (this.J(this.T.shift()), this.q()), !a.idSyncDisableSyncs && this.H && this.I.length && !this.Y && (this.Pa || (this.Pa = i, setTimeout(function () {
                e.Ha = n.r ? 15 : 150
            }, this.Ua)), this.Y = i, this.Ma())
        },
        Jb: function (t) {
            var e, n, i;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (t = 0; n > t; t++) i = e[t], i.syncOnPage && this.ua(i, "", "syncOnPage")
        },
        J: function (t) {
            var e, n, i, a, r, o = encodeURIComponent;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (i = 0; n > i; i++) a = e[i], r = [o("ibs"), o(a.id || ""), o(a.tag || ""), m.za(a.url || [], ","), o(a.ttl || ""), "", "", a.fireURLSync ? "true" : "false"], a.syncOnPage || (this.ta ? this.P(r.join("|")) : a.fireURLSync && this.ua(a, r.join("|")));
            this.Ab.push(t)
        },
        ua: function (t, e, r) {
            var o = (r = "syncOnPage" === r ? i : k) ? K : M;
            a.f();
            var s = a.a(o),
                c = k,
                u = k,
                l = Math.ceil((new Date).getTime() / n.ea);
            s ? (s = s.split("*"), u = this.Kb(s, t.id, l), c = u.pb, u = u.qb, (!c || !u) && this.Aa(r, t, e, s, o, l)) : (s = [], this.Aa(r, t, e, s, o, l))
        },
        Kb: function (t, e, n) {
            var a, r, o, s = k,
                c = k;
            for (r = 0; r < t.length; r++) a = t[r], o = parseInt(a.split("-")[1], 10), a.match("^" + e + "-") ? (s = i, o > n ? c = i : (t.splice(r, 1), r--)) : n >= o && (t.splice(r, 1), r--);
            return {
                pb: s,
                qb: c
            }
        },
        Bb: function (t) {
            if (t.join("*").length > this.da)
                for (t.sort(function (t, e) {
                        return parseInt(t.split("-")[1], 10) - parseInt(e.split("-")[1], 10)
                }) ; t.join("*").length > this.da;) t.shift()
        },
        Aa: function (t, e, n, i, r, o) {
            var s = this;
            if (t) {
                if ("img" === e.tag) {
                    var c, u, l, t = e.url,
                        n = a.loadSSL ? "https:" : "http:";
                    for (i = 0, c = t.length; c > i; i++) {
                        u = t[i], l = /^\/\//.test(u);
                        var d = new Image;
                        m.O(d, "load", function (t, e, n, i) {
                            return function () {
                                s.W[t] = j, a.f();
                                var o = a.a(r),
                                    c = [];
                                if (o) {
                                    var u, l, d, o = o.split("*");
                                    for (u = 0, l = o.length; l > u; u++) d = o[u], d.match("^" + e.id + "-") || c.push(d)
                                }
                                s.Oa(c, e, n, i)
                            }
                        }(this.W.length, e, r, o)), d.src = (l ? n : "") + u, this.W.push(d)
                    }
                }
            } else this.P(n), this.Oa(i, e, r, o)
        },
        P: function (t) {
            var e = encodeURIComponent;
            this.I.push(e(a.Ub ? "---destpub-debug---" : "---destpub---") + t)
        },
        Oa: function (t, e, n, i) {
            t.push(e.id + "-" + (i + Math.ceil(e.ttl / 60 / 24))), this.Bb(t), a.e(n, t.join("*"))
        },
        Ma: function () {
            var t, e = this;
            this.I.length ? (t = this.I.shift(), a.ra.postMessage(t, this.url, this.Fa.contentWindow), this.Cb.push(t), setTimeout(function () {
                e.Ma()
            }, this.Ha)) : this.Y = k
        },
        X: function (t) {
            var e = /^---destpub-to-parent---/;
            "string" == typeof t && e.test(t) && (e = t.replace(e, "").split("|"), "canSetThirdPartyCookies" === e[0] && (this.ta = "true" === e[1] ? i : k, this.Ka = i, this.q()), this.Db.push(t))
        },
        Ib: function (t) {
            (this.url === j || t.subdomain && "nosubdomainreturned" === this.g) && (this.g = "string" == typeof a.qa && a.qa.length ? a.qa : t.subdomain || "", this.url = this.ub()), t.ibs instanceof Array && t.ibs.length && (this.ya = i), this.Ja() && (a.idSyncAttachIframeOnWindowLoad ? (l.aa || "complete" === v.readyState || "loaded" === v.readyState) && this.Q() : this.kb()), "function" == typeof a.idSyncIDCallResult ? a.idSyncIDCallResult(t) : this.q(t), "function" == typeof a.idSyncAfterIDCallResult && a.idSyncAfterIDCallResult(t)
        },
        lb: function (t, e) {
            return a.Vb || !t || e - t > n.Ra
        },
        kb: function () {
            function t() {
                e.Z || (document.body ? e.Q() : setTimeout(t, 30))
            }
            var e = this;
            t()
        }
    };
    a.Sb = z, a.timeoutMetricsLog = [];
    var p = {
        ob: window.performance && window.performance.timing ? 1 : 0,
        Ia: window.performance && window.performance.timing ? window.performance.timing : j,
        $: j,
        R: j,
        d: {},
        V: [],
        send: function (t) {
            if (a.takeTimeoutMetrics && t === Object(t)) {
                var e, n = [],
                    i = encodeURIComponent;
                for (e in t) t.hasOwnProperty(e) && n.push(i(e) + "=" + i(t[e]));
                t = "http" + (a.loadSSL ? "s" : "") + "://dpm.demdex.net/event?d_visid_ver=" + a.version + "&d_visid_stg_timeout=" + a.loadTimeout + "&" + n.join("&") + "&d_orgid=" + i(a.marketingCloudOrgID) + "&d_timingapi=" + this.ob + "&d_winload=" + this.vb() + "&d_ld=" + this.p(), (new Image).src = t, a.timeoutMetricsLog.push(t)
            }
        },
        vb: function () {
            return this.R === j && (this.R = this.Ia ? this.$ - this.Ia.navigationStart : this.$ - l.nb), this.R
        },
        p: function () {
            return (new Date).getTime()
        },
        J: function (t) {
            var e = this.d[t],
                n = {};
            n.d_visid_stg_timeout_captured = e.xa, n.d_visid_cors = e.wa, n.d_fieldgroup = t, n.d_settimeout_overriden = e.va, e.timeout ? e.xb ? (n.d_visid_timedout = 1, n.d_visid_timeout = e.timeout - e.requestStart, n.d_visid_response = -1) : (n.d_visid_timedout = "n/a", n.d_visid_timeout = "n/a", n.d_visid_response = "n/a") : (n.d_visid_timedout = 0, n.d_visid_timeout = -1, n.d_visid_response = e.Nb - e.requestStart), n.d_visid_url = e.url, l.aa ? this.send(n) : this.V.push(n), delete this.d[t]
        },
        Lb: function () {
            for (var t = 0, e = this.V.length; e > t; t++) this.send(this.V[t])
        },
        Ca: function () {
            return "function" == typeof setTimeout.toString ? -1 < setTimeout.toString().indexOf("[native code]") ? 0 : 1 : -1
        }
    };
    a.Zb = p;
    var y = {
        isClientSideMarketingCloudVisitorID: j,
        MCIDCallTimedOut: j,
        AnalyticsIDCallTimedOut: j,
        AAMIDCallTimedOut: j,
        d: {},
        Na: function (t, e) {
            switch (t) {
                case H:
                    e === k ? this.MCIDCallTimedOut !== i && (this.MCIDCallTimedOut = k) : this.MCIDCallTimedOut = e;
                    break;
                case E:
                    e === k ? this.AnalyticsIDCallTimedOut !== i && (this.AnalyticsIDCallTimedOut = k) : this.AnalyticsIDCallTimedOut = e;
                    break;
                case D:
                    e === k ? this.AAMIDCallTimedOut !== i && (this.AAMIDCallTimedOut = k) : this.AAMIDCallTimedOut = e
            }
        }
    };
    a.isClientSideMarketingCloudVisitorID = function () {
        return y.isClientSideMarketingCloudVisitorID
    }, a.MCIDCallTimedOut = function () {
        return y.MCIDCallTimedOut
    }, a.AnalyticsIDCallTimedOut = function () {
        return y.AnalyticsIDCallTimedOut
    }, a.AAMIDCallTimedOut = function () {
        return y.AAMIDCallTimedOut
    }, a.idSyncGetOnPageSyncInfo = function () {
        return a.f(), a.a(K)
    }, a.idSyncByURL = function (t) {
        var e, n = t || {};
        e = n.minutesToLive;
        var i = "";
        if (a.idSyncDisableSyncs && (i = i ? i : "Error: id syncs have been disabled"), "string" == typeof n.dpid && n.dpid.length || (i = i ? i : "Error: config.dpid is empty"), "string" == typeof n.url && n.url.length || (i = i ? i : "Error: config.url is empty"), "undefined" == typeof e ? e = 20160 : (e = parseInt(e, 10), (isNaN(e) || 0 >= e) && (i = i ? i : "Error: config.minutesToLive needs to be a positive number")), e = {
            error: i,
            rc: e
        }, e.error) return e.error;
        var r, i = t.url,
            o = encodeURIComponent,
            n = z,
            i = i.replace(/^https:/, "").replace(/^http:/, "");
        return r = m.za(["", t.dpid, t.dpuuid || ""], ","), t = ["ibs", o(t.dpid), "img", o(i), e.ttl, "", r], n.P(t.join("|")), n.q(), "Successfully queued"
    }, a.idSyncByDataSource = function (t) {
        return t === Object(t) && "string" == typeof t.dpuuid && t.dpuuid.length ? (t.url = "//dpm.demdex.net/ibs:dpid=" + t.dpid + "&dpuuid=" + t.dpuuid, a.idSyncByURL(t)) : "Error: config or config.dpuuid is empty"
    }, a._compareVersions = function (t, e) {
        if (t === e) return 0;
        var a, r = t.toString().split("."),
            o = e.toString().split(".");
        t: {
            a = r.concat(o);
            for (var s = 0, c = a.length; c > s; s++)
                if (!n.Ta.test(a[s])) {
                    a = k;
                    break t
                }
            a = i
        }
        if (!a) return NaN;
        for (; r.length < o.length;) r.push("0");
        for (; o.length < r.length;) o.push("0");
        t: {
            for (a = 0; a < r.length; a++) {
                if (s = parseInt(r[a], 10), c = parseInt(o[a], 10), s > c) {
                    r = 1;
                    break t
                }
                if (c > s) {
                    r = -1;
                    break t
                }
            }
            r = 0
        }
        return r
    }, a._getCookieVersion = function (t) {
        return t = t || a.cookieRead(a.cookieName), (t = n.fa.exec(t)) && 1 < t.length ? t[1] : null
    }, a._resetAmcvCookie = function (t) {
        var e = a._getCookieVersion();
        (!e || m.zb(e, t)) && m.Mb(a.cookieName)
    }, 0 > q.indexOf("@") && (q += "@AdobeOrg"), a.marketingCloudOrgID = q, a.cookieName = "AMCV_" + q, a.sessionCookieName = "AMCVS_" + q, a.cookieDomain = a.Ya(), a.cookieDomain == s.location.hostname && (a.cookieDomain = ""), a.loadSSL = 0 <= s.location.protocol.toLowerCase().indexOf("https"), a.loadTimeout = 3e4, a.CORSErrors = [], a.marketingCloudServer = a.audienceManagerServer = "dpm.demdex.net";
    var N = {};
    if (N[A] = i, N[t] = i, w && "object" == typeof w) {
        for (var J in w) !Object.prototype[J] && (a[J] = w[J]);
        a.idSyncContainerID = a.idSyncContainerID || 0, a.resetBeforeVersion && a._resetAmcvCookie(a.resetBeforeVersion), a.ga(), a.ha(), a.f(), J = a.a(L);
        var O = Math.ceil((new Date).getTime() / n.ea);
        !a.idSyncDisableSyncs && z.lb(J, O) && (a.j(t, -1), a.e(L, O)), a.getMarketingCloudVisitorID(), a.getAudienceManagerLocationHint(), a.getAudienceManagerBlob(), a.cb(a.serverState)
    } else a.ga(), a.ha();
    if (!a.idSyncDisableSyncs) {
        z.mb(), m.O(window, "load", function () {
            l.aa = i, p.$ = p.p(), p.Lb();
            var t = z;
            t.Ja() && t.Q()
        });
        try {
            a.ra.X(function (t) {
                z.X(t.data)
            }, z.S)
        } catch (P) { }
    }
}
Visitor.getInstance = function (t, e) {
    var n, i, a = window.s_c_il;
    if (0 > t.indexOf("@") && (t += "@AdobeOrg"), a)
        for (i = 0; i < a.length; i++)
            if ((n = a[i]) && "Visitor" == n._c && n.marketingCloudOrgID == t) return n;
    return new Visitor(t, e)
},
    function () {
        function t() {
            e.aa = n
        }
        var e = window.Visitor,
            n = e.Va,
            i = e.Sa;
        n || (n = !0), i || (i = !1), window.addEventListener ? window.addEventListener("load", t) : window.attachEvent && window.attachEvent("onload", t), e.nb = (new Date).getTime()
    }(),
    // All code and conventions are protected by copyright
    function (t, e, n) {
        function i() {
            this.rules = I.filter(I.rules, function (t) {
                return "elementexists" === t.event
            })
        }

        function a() {
            I.getToolsByType("nielsen").length > 0 && I.domReady(I.bind(this.initialize, this))
        }

        function r() {
            I.addEventHandler(t, "orientationchange", r.orientationChange)
        }

        function o() {
            var t = this.eventRegex = /^hover\(([0-9]+)\)$/,
                e = this.rules = [];
            I.each(I.rules, function (n) {
                var i = n.event.match(t);
                i && e.push([Number(n.event.match(t)[1]), n.selector])
            })
        }

        function s() {
            this.defineEvents(), this.visibilityApiHasPriority = !0, e.addEventListener ? this.setVisibilityApiPriority(!1) : this.attachDetachOlderEventListeners(!0, e, "focusout");
            I.bindEvent("aftertoolinit", function () {
                I.fireEvent(I.visibility.isHidden() ? "tabblur" : "tabfocus")
            })
        }

        function c() {
            this.lastURL = I.URL(), this._fireIfURIChanged = I.bind(this.fireIfURIChanged, this), this._onPopState = I.bind(this.onPopState, this), this._onHashChange = I.bind(this.onHashChange, this), this._pushState = I.bind(this.pushState, this), this._replaceState = I.bind(this.replaceState, this), this.initialize()
        }

        function u() {
            this.rules = I.filter(I.rules, function (t) {
                return "videoplayed" === t.event.substring(0, 11)
            }), this.eventHandler = I.bind(this.onUpdateTime, this)
        }

        function l(t) {
            this.delay = 250, this.FB = t, I.domReady(I.bind(function () {
                I.poll(I.bind(this.initialize, this), this.delay, 8)
            }, this))
        }

        function d(e) {
            I.domReady(I.bind(function () {
                this.twttr = e || t.twttr, this.initialize()
            }, this))
        }

        function f(e) {
            e = e || I.rules, this.rules = I.filter(e, function (t) {
                return "inview" === t.event
            }), this.elements = [], this.eventHandler = I.bind(this.track, this), I.addEventHandler(t, "scroll", this.eventHandler), I.addEventHandler(t, "load", this.eventHandler)
        }

        function h() {
            var t = I.filter(I.rules, function (t) {
                return 0 === t.event.indexOf("dataelementchange")
            });
            this.dataElementsNames = I.map(t, function (t) {
                var e = t.event.match(/dataelementchange\((.*)\)/i);
                return e[1]
            }, this), this.initPolling()
        }

        function p(t) {
            I.BaseTool.call(this, t), this.name = t.name || "VisitorID", this.initialize()
        }

        function g(t) {
            I.BaseTool.call(this, t), this.name = t.name || "Basic"
        }

        function v(t) {
            I.BaseTool.call(this, t), this.styleElements = {}, this.targetPageParamsStore = {}
        }

        function m(t) {
            I.BaseTool.call(this, t)
        }

        function y(t) {
            I.BaseTool.call(this, t)
        }

        function b(t) {
            I.BaseTool.call(this, t), this.varBindings = {}, this.events = [], this.products = [], this.customSetupFuns = []
        }

        function k() {
            I.BaseTool.call(this), this.asyncScriptCallbackQueue = [], this.argsForBlockingScripts = []
        }

        function S(t) {
            I.BaseTool.call(this, t), this.defineListeners(), this.beaconMethod = "plainBeacon", this.adapt = new S.DataAdapters, this.dataProvider = new S.DataProvider.Aggregate
        }
        var C = Object.prototype.toString,
            E = t._satellite && t._satellite.override,
            I = {
                initialized: !1,
                $data: function (t, e, i) {
                    if (t) {
                        var a = "__satellite__",
                            r = I.dataCache,
                            o = t[a];
                        o || (o = t[a] = I.uuid++);
                        var s = r[o];
                        return s || (s = r[o] = {}), i === n ? s[e] : void (s[e] = i)
                    }
                },
                uuid: 1,
                dataCache: {},
                keys: function (t) {
                    var e = [];
                    for (var n in t) t.hasOwnProperty(n) && e.push(n);
                    return e
                },
                values: function (t) {
                    var e = [];
                    for (var n in t) t.hasOwnProperty(n) && e.push(t[n]);
                    return e
                },
                isArray: Array.isArray || function (t) {
                    return "[object Array]" === C.apply(t)
                },
                isObject: function (t) {
                    return null != t && !I.isArray(t) && "object" == typeof t
                },
                isString: function (t) {
                    return "string" == typeof t
                },
                isNumber: function (t) {
                    return "[object Number]" === C.apply(t) && !I.isNaN(t)
                },
                isNaN: function (t) {
                    return t !== t
                },
                isRegex: function (t) {
                    return t instanceof RegExp
                },
                isLinkTag: function (t) {
                    return !(!t || !t.nodeName || "a" !== t.nodeName.toLowerCase())
                },
                each: function (t, e, n) {
                    for (var i = 0, a = t.length; a > i; i++) e.call(n, t[i], i, t)
                },
                map: function (t, e, n) {
                    for (var i = [], a = 0, r = t.length; r > a; a++) i.push(e.call(n, t[a], a, t));
                    return i
                },
                filter: function (t, e, n) {
                    for (var i = [], a = 0, r = t.length; r > a; a++) {
                        var o = t[a];
                        e.call(n, o, a, t) && i.push(o)
                    }
                    return i
                },
                any: function (t, e, n) {
                    for (var i = 0, a = t.length; a > i; i++) {
                        var r = t[i];
                        if (e.call(n, r, i, t)) return !0
                    }
                    return !1
                },
                every: function (t, e, n) {
                    for (var i = !0, a = 0, r = t.length; r > a; a++) {
                        var o = t[a];
                        i = i && e.call(n, o, a, t)
                    }
                    return i
                },
                contains: function (t, e) {
                    return -1 !== I.indexOf(t, e)
                },
                indexOf: function (t, e) {
                    if (t.indexOf) return t.indexOf(e);
                    for (var n = t.length; n--;)
                        if (e === t[n]) return n;
                    return -1
                },
                find: function (t, e, n) {
                    if (!t) return null;
                    for (var i = 0, a = t.length; a > i; i++) {
                        var r = t[i];
                        if (e.call(n, r, i, t)) return r
                    }
                    return null
                },
                textMatch: function (t, e) {
                    if (null == e) throw new Error("Illegal Argument: Pattern is not present");
                    return null == t ? !1 : "string" == typeof e ? t === e : e instanceof RegExp ? e.test(t) : !1
                },
                stringify: function (t, e) {
                    if (e = e || [], I.isObject(t)) {
                        if (I.contains(e, t)) return "<Cycle>";
                        e.push(t)
                    }
                    if (I.isArray(t)) return "[" + I.map(t, function (t) {
                        return I.stringify(t, e)
                    }).join(",") + "]";
                    if (I.isString(t)) return '"' + String(t) + '"';
                    if (I.isObject(t)) {
                        var n = [];
                        for (var i in t) t.hasOwnProperty(i) && n.push(i + ": " + I.stringify(t[i], e));
                        return "{" + n.join(", ") + "}"
                    }
                    return String(t)
                },
                trim: function (t) {
                    return null == t ? null : t.trim ? t.trim() : t.replace(/^ */, "").replace(/ *$/, "")
                },
                bind: function (t, e) {
                    return function () {
                        return t.apply(e, arguments)
                    }
                },
                throttle: function (t, e) {
                    var n = null;
                    return function () {
                        var i = this,
                            a = arguments;
                        clearTimeout(n), n = setTimeout(function () {
                            t.apply(i, a)
                        }, e)
                    }
                },
                domReady: function (t) {
                    function n(t) {
                        for (f = 1; t = a.shift() ;) t()
                    }
                    var i, a = [],
                        r = !1,
                        o = e,
                        s = o.documentElement,
                        c = s.doScroll,
                        u = "DOMContentLoaded",
                        l = "addEventListener",
                        d = "onreadystatechange",
                        f = /^loade|^c/.test(o.readyState);
                    return o[l] && o[l](u, i = function () {
                        o.removeEventListener(u, i, r), n()
                    }, r), c && o.attachEvent(d, i = function () {
                        /^c/.test(o.readyState) && (o.detachEvent(d, i), n())
                    }), t = c ? function (e) {
                        self != top ? f ? e() : a.push(e) : function () {
                            try {
                                s.doScroll("left")
                            } catch (n) {
                                return setTimeout(function () {
                                    t(e)
                                }, 50)
                            }
                            e()
                        }()
                    } : function (t) {
                        f ? t() : a.push(t)
                    }
                }(),
                loadScript: function (t, n) {
                    var i = e.createElement("script");
                    I.scriptOnLoad(t, i, n), i.src = t, e.getElementsByTagName("head")[0].appendChild(i)
                },
                scriptOnLoad: function (t, e, n) {
                    function i(t) {
                        t && I.logError(t), n && n(t)
                    }
                    "onload" in e ? (e.onload = function () {
                        i()
                    }, e.onerror = function () {
                        i(new Error("Failed to load script " + t))
                    }) : "readyState" in e && (e.onreadystatechange = function () {
                        var t = e.readyState;
                        ("loaded" === t || "complete" === t) && (e.onreadystatechange = null, i())
                    })
                },
                loadScriptOnce: function (t, e) {
                    I.loadedScriptRegistry[t] || I.loadScript(t, function (n) {
                        n || (I.loadedScriptRegistry[t] = !0), e && e(n)
                    })
                },
                loadedScriptRegistry: {},
                loadScriptSync: function (t) {
                    return e.write ? I.domReadyFired ? void I.notify('Cannot load sync the "' + t + '" script after DOM Ready.', 1) : (t.indexOf('"') > -1 && (t = encodeURI(t)), void e.write('<script src="' + t + '"></script>')) : void I.notify('Cannot load sync the "' + t + '" script because "document.write" is not available', 1)
                },
                pushAsyncScript: function (t) {
                    I.tools["default"].pushAsyncScript(t)
                },
                pushBlockingScript: function (t) {
                    I.tools["default"].pushBlockingScript(t)
                },
                addEventHandler: t.addEventListener ? function (t, e, n) {
                    t.addEventListener(e, n, !1)
                } : function (t, e, n) {
                    t.attachEvent("on" + e, n)
                },
                removeEventHandler: t.removeEventListener ? function (t, e, n) {
                    t.removeEventListener(e, n, !1)
                } : function (t, e, n) {
                    t.detachEvent("on" + e, n)
                },
                preventDefault: t.addEventListener ? function (t) {
                    t.preventDefault()
                } : function (t) {
                    t.returnValue = !1
                },
                stopPropagation: function (t) {
                    t.cancelBubble = !0, t.stopPropagation && t.stopPropagation()
                },
                containsElement: function (t, e) {
                    return t.contains ? t.contains(e) : !!(16 & t.compareDocumentPosition(e))
                },
                matchesCss: function (n) {
                    function i(t, e) {
                        var n = e.tagName;
                        return n ? t.toLowerCase() === n.toLowerCase() : !1
                    }
                    var a = n.matchesSelector || n.mozMatchesSelector || n.webkitMatchesSelector || n.oMatchesSelector || n.msMatchesSelector;
                    return a ? function (n, i) {
                        if (i === e || i === t) return !1;
                        try {
                            return a.call(i, n)
                        } catch (r) {
                            return !1
                        }
                    } : n.querySelectorAll ? function (t, e) {
                        var n = e.parentNode;
                        if (!n) return !1;
                        if (t.match(/^[a-z]+$/i)) return i(t, e);
                        try {
                            for (var a = e.parentNode.querySelectorAll(t), r = a.length; r--;)
                                if (a[r] === e) return !0
                        } catch (o) { }
                        return !1
                    } : function (t, e) {
                        if (t.match(/^[a-z]+$/i)) return i(t, e);
                        try {
                            return I.Sizzle.matches(t, [e]).length > 0
                        } catch (n) {
                            return !1
                        }
                    }
                }(e.documentElement),
                cssQuery: function (t) {
                    return t.querySelectorAll ? function (e, n) {
                        var i;
                        try {
                            i = t.querySelectorAll(e)
                        } catch (a) {
                            i = []
                        }
                        n(i)
                    } : function (t, e) {
                        if (I.Sizzle) {
                            var n;
                            try {
                                n = I.Sizzle(t)
                            } catch (i) {
                                n = []
                            }
                            e(n)
                        } else I.sizzleQueue.push([t, e])
                    }
                }(e),
                hasAttr: function (t, e) {
                    return t.hasAttribute ? t.hasAttribute(e) : t[e] !== n
                },
                inherit: function (t, e) {
                    var n = function () { };
                    n.prototype = e.prototype, t.prototype = new n, t.prototype.constructor = t
                },
                extend: function (t, e) {
                    for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n])
                },
                toArray: function () {
                    try {
                        var t = Array.prototype.slice;
                        return t.call(e.documentElement.childNodes, 0)[0].nodeType,
                            function (e) {
                                return t.call(e, 0)
                            }
                    } catch (n) {
                        return function (t) {
                            for (var e = [], n = 0, i = t.length; i > n; n++) e.push(t[n]);
                            return e
                        }
                    }
                }(),
                equalsIgnoreCase: function (t, e) {
                    return null == t ? null == e : null == e ? !1 : String(t).toLowerCase() === String(e).toLowerCase()
                },
                poll: function (t, e, n) {
                    function i() {
                        I.isNumber(n) && a++ >= n || t() || setTimeout(i, e)
                    }
                    var a = 0;
                    e = e || 1e3, i()
                },
                escapeForHtml: function (t) {
                    return t ? String(t).replace(/\&/g, "&amp;").replace(/\</g, "&lt;").replace(/\>/g, "&gt;").replace(/\"/g, "&quot;").replace(/\'/g, "&#x27;").replace(/\//g, "&#x2F;") : t
                }
            };
        I.availableTools = {}, I.availableEventEmitters = [], I.fireOnceEvents = ["condition", "elementexists"], I.initEventEmitters = function () {
            I.eventEmitters = I.map(I.availableEventEmitters, function (t) {
                return new t
            })
        }, I.eventEmitterBackgroundTasks = function () {
            I.each(I.eventEmitters, function (t) {
                "backgroundTasks" in t && t.backgroundTasks()
            })
        }, I.initTools = function (t) {
            var e = {
                "default": new k
            },
                n = I.settings.euCookieName || "sat_track";
            for (var i in t)
                if (t.hasOwnProperty(i)) {
                    var a, r, o;
                    if (a = t[i], a.euCookie) {
                        var s = "true" !== I.readCookie(n);
                        if (s) continue
                    }
                    if (r = I.availableTools[a.engine], !r) {
                        var c = [];
                        for (var u in I.availableTools) I.availableTools.hasOwnProperty(u) && c.push(u);
                        throw new Error("No tool engine named " + a.engine + ", available: " + c.join(",") + ".")
                    }
                    o = new r(a), o.id = i, e[i] = o
                }
            return e
        }, I.preprocessArguments = function (t, e, n, i, a) {
            function r(t) {
                return i && I.isString(t) ? t.toLowerCase() : t
            }

            function o(t) {
                var c = {};
                for (var u in t)
                    if (t.hasOwnProperty(u)) {
                        var l = t[u];
                        I.isObject(l) ? c[u] = o(l) : I.isArray(l) ? c[u] = s(l, i) : c[u] = r(I.replace(l, e, n, a))
                    }
                return c
            }

            function s(t, i) {
                for (var a = [], s = 0, c = t.length; c > s; s++) {
                    var u = t[s];
                    I.isString(u) ? u = r(I.replace(u, e, n)) : u && u.constructor === Object && (u = o(u)), a.push(u)
                }
                return a
            }
            return t ? s(t, i) : t
        }, I.execute = function (t, e, n, i) {
            function a(a) {
                var r = i[a || "default"];
                if (r) try {
                    r.triggerCommand(t, e, n)
                } catch (o) {
                    I.logError(o)
                }
            }
            if (!_satellite.settings.hideActivity)
                if (i = i || I.tools, t.engine) {
                    var r = t.engine;
                    for (var o in i)
                        if (i.hasOwnProperty(o)) {
                            var s = i[o];
                            s.settings && s.settings.engine === r && a(o)
                        }
                } else t.tool instanceof Array ? I.each(t.tool, function (t) {
                    a(t)
                }) : a(t.tool)
        }, I.Logger = {
            outputEnabled: !1,
            messages: [],
            keepLimit: 100,
            flushed: !1,
            LEVELS: [null, null, "log", "info", "warn", "error"],
            message: function (t, e) {
                var n = this.LEVELS[e] || "log";
                this.messages.push([n, t]), this.messages.length > this.keepLimit && this.messages.shift(), this.outputEnabled && this.echo(n, t)
            },
            getHistory: function () {
                return this.messages
            },
            clearHistory: function () {
                this.messages = []
            },
            setOutputState: function (t) {
                this.outputEnabled != t && (this.outputEnabled = t, t ? this.flush() : this.flushed = !1)
            },
            echo: function (e, n) {
                t.console && t.console[e]("SATELLITE: " + n)
            },
            flush: function () {
                this.flushed || (I.each(this.messages, function (t) {
                    t[2] !== !0 && (this.echo(t[0], t[1]), t[2] = !0)
                }, this), this.flushed = !0)
            }
        }, I.notify = I.bind(I.Logger.message, I.Logger), I.cleanText = function (t) {
            return null == t ? null : I.trim(t).replace(/\s+/g, " ")
        }, I.cleanText.legacy = function (t) {
            return null == t ? null : I.trim(t).replace(/\s{2,}/g, " ").replace(/[^\000-\177]*/g, "")
        }, I.text = function (t) {
            return t.textContent || t.innerText
        }, I.specialProperties = {
            text: I.text,
            cleanText: function (t) {
                return I.cleanText(I.text(t))
            }
        }, I.getObjectProperty = function (t, e, i) {
            for (var a, r = e.split("."), o = t, s = I.specialProperties, c = 0, u = r.length; u > c; c++) {
                if (null == o) return n;
                var l = r[c];
                if (i && "@" === l.charAt(0)) {
                    var d = l.slice(1);
                    o = s[d](o)
                } else if (o.getAttribute && (a = l.match(/^getAttribute\((.+)\)$/))) {
                    var f = a[1];
                    o = o.getAttribute(f)
                } else o = o[l]
            }
            return o
        }, I.getToolsByType = function (t) {
            if (!t) throw new Error("Tool type is missing");
            var e = [];
            for (var n in I.tools)
                if (I.tools.hasOwnProperty(n)) {
                    var i = I.tools[n];
                    i.settings && i.settings.engine === t && e.push(i)
                }
            return e
        }, I.setVar = function () {
            var t = I.data.customVars;
            if (null == t && (I.data.customVars = {}, t = I.data.customVars), "string" == typeof arguments[0]) {
                var e = arguments[0];
                t[e] = arguments[1]
            } else if (arguments[0]) {
                var n = arguments[0];
                for (var i in n) n.hasOwnProperty(i) && (t[i] = n[i])
            }
        }, I.dataElementSafe = function (t, e) {
            if (arguments.length > 2) {
                var n = arguments[2];
                "pageview" === e ? I.dataElementSafe.pageviewCache[t] = n : "session" === e ? I.setCookie("_sdsat_" + t, n) : "visitor" === e && I.setCookie("_sdsat_" + t, n, 730)
            } else {
                if ("pageview" === e) return I.dataElementSafe.pageviewCache[t];
                if ("session" === e || "visitor" === e) return I.readCookie("_sdsat_" + t)
            }
        }, I.dataElementSafe.pageviewCache = {}, I.realGetDataElement = function (e) {
            var n;
            return e.selector ? I.hasSelector && I.cssQuery(e.selector, function (t) {
                if (t.length > 0) {
                    var i = t[0];
                    "text" === e.property ? n = i.innerText || i.textContent : e.property in i ? n = i[e.property] : I.hasAttr(i, e.property) && (n = i.getAttribute(e.property))
                }
            }) : e.queryParam ? n = e.ignoreCase ? I.getQueryParamCaseInsensitive(e.queryParam) : I.getQueryParam(e.queryParam) : e.cookie ? n = I.readCookie(e.cookie) : e.jsVariable ? n = I.getObjectProperty(t, e.jsVariable) : e.customJS ? n = e.customJS() : e.contextHub && (n = e.contextHub()), I.isString(n) && e.cleanText && (n = I.cleanText(n)), n
        }, I.getDataElement = function (t, e, i) {
            if (i = i || I.dataElements[t], null == i) return I.settings.undefinedVarsReturnEmpty ? "" : null;
            var a = I.realGetDataElement(i);
            return a === n && i.storeLength ? a = I.dataElementSafe(t, i.storeLength) : a !== n && i.storeLength && I.dataElementSafe(t, i.storeLength, a), a || e || (a = i["default"] || ""), I.isString(a) && i.forceLowerCase && (a = a.toLowerCase()), a
        }, I.getVar = function (i, a, r) {
            var o, s, c = I.data.customVars,
                u = r ? r.target || r.srcElement : null,
                l = {
                    uri: I.URI(),
                    protocol: e.location.protocol,
                    hostname: e.location.hostname
                };
            if (I.dataElements && i in I.dataElements) return I.getDataElement(i);
            if (s = l[i.toLowerCase()], s === n)
                if ("this." === i.substring(0, 5)) i = i.slice(5), s = I.getObjectProperty(a, i, !0);
                else if ("event." === i.substring(0, 6)) i = i.slice(6), s = I.getObjectProperty(r, i);
                else if ("target." === i.substring(0, 7)) i = i.slice(7), s = I.getObjectProperty(u, i);
                else if ("window." === i.substring(0, 7)) i = i.slice(7), s = I.getObjectProperty(t, i);
                else if ("param." === i.substring(0, 6)) i = i.slice(6), s = I.getQueryParam(i);
                else if (o = i.match(/^rand([0-9]+)$/)) {
                    var d = Number(o[1]),
                        f = (Math.random() * (Math.pow(10, d) - 1)).toFixed(0);
                    s = Array(d - f.length + 1).join("0") + f
                } else s = I.getObjectProperty(c, i);
            return s
        }, I.getVars = function (t, e, n) {
            var i = {};
            return I.each(t, function (t) {
                i[t] = I.getVar(t, e, n)
            }), i
        }, I.replace = function (t, e, n, i) {
            return "string" != typeof t ? t : t.replace(/%(.*?)%/g, function (t, a) {
                var r = I.getVar(a, e, n);
                return null == r ? I.settings.undefinedVarsReturnEmpty ? "" : t : i ? I.escapeForHtml(r) : r
            })
        }, I.escapeHtmlParams = function (t) {
            return t.escapeHtml = !0, t
        }, I.searchVariables = function (t, e, n) {
            if (!t || 0 === t.length) return "";
            for (var i = [], a = 0, r = t.length; r > a; a++) {
                var o = t[a],
                    s = I.getVar(o, e, n);
                i.push(o + "=" + escape(s))
            }
            return "?" + i.join("&")
        }, I.fireRule = function (t, e, n) {
            var i = t.trigger;
            if (i) {
                for (var a = 0, r = i.length; r > a; a++) {
                    var o = i[a];
                    I.execute(o, e, n)
                }
                I.contains(I.fireOnceEvents, t.event) && (t.expired = !0)
            }
        }, I.isLinked = function (t) {
            for (var e = t; e; e = e.parentNode)
                if (I.isLinkTag(e)) return !0;
            return !1
        }, I.firePageLoadEvent = function (t) {
            for (var n = e.location, i = {
                type: t,
                target: n
            }, a = I.pageLoadRules, r = I.evtHandlers[i.type], o = a.length; o--;) {
                var s = a[o];
                I.ruleMatches(s, i, n) && (I.notify('Rule "' + s.name + '" fired.', 1), I.fireRule(s, n, i))
            }
            for (var c in I.tools)
                if (I.tools.hasOwnProperty(c)) {
                    var u = I.tools[c];
                    u.endPLPhase && u.endPLPhase(t)
                }
            r && I.each(r, function (t) {
                t(i)
            })
        }, I.track = function (t) {
            t = t.replace(/^\s*/, "").replace(/\s*$/, "");
            for (var e = 0; e < I.directCallRules.length; e++) {
                var n = I.directCallRules[e];
                if (n.name === t) return I.notify('Direct call Rule "' + t + '" fired.', 1), void I.fireRule(n, location, {
                    type: t
                })
            }
            I.notify('Direct call Rule "' + t + '" not found.', 1)
        }, I.basePath = function () {
            return I.data.host ? ("https:" === e.location.protocol ? "https://" + I.data.host.https : "http://" + I.data.host.http) + "/" : this.settings.basePath
        }, I.setLocation = function (e) {
            t.location = e
        }, I.parseQueryParams = function (t) {
            var e = function (t) {
                var e = t;
                try {
                    e = decodeURIComponent(t)
                } catch (n) { }
                return e
            };
            if ("" === t || I.isString(t) === !1) return {};
            0 === t.indexOf("?") && (t = t.substring(1));
            var n = {},
                i = t.split("&");
            return I.each(i, function (t) {
                t = t.split("="), t[1] && (n[e(t[0])] = e(t[1]))
            }), n
        }, I.getCaseSensitivityQueryParamsMap = function (t) {
            var e = I.parseQueryParams(t),
                n = {};
            for (var i in e) e.hasOwnProperty(i) && (n[i.toLowerCase()] = e[i]);
            return {
                normal: e,
                caseInsensitive: n
            }
        }, I.updateQueryParams = function () {
            I.QueryParams = I.getCaseSensitivityQueryParamsMap(t.location.search)
        }, I.updateQueryParams(), I.getQueryParam = function (t) {
            return I.QueryParams.normal[t]
        }, I.getQueryParamCaseInsensitive = function (t) {
            return I.QueryParams.caseInsensitive[t.toLowerCase()]
        }, I.encodeObjectToURI = function (t) {
            if (I.isObject(t) === !1) return "";
            var e = [];
            for (var n in t) t.hasOwnProperty(n) && e.push(encodeURIComponent(n) + "=" + encodeURIComponent(t[n]));
            return e.join("&")
        }, I.readCookie = function (t) {
            for (var i = t + "=", a = e.cookie.split(";"), r = 0; r < a.length; r++) {
                for (var o = a[r];
                    " " == o.charAt(0) ;) o = o.substring(1, o.length);
                if (0 === o.indexOf(i)) return o.substring(i.length, o.length)
            }
            return n
        }, I.setCookie = function (t, n, i) {
            var a;
            if (i) {
                var r = new Date;
                r.setTime(r.getTime() + 24 * i * 60 * 60 * 1e3), a = "; expires=" + r.toGMTString()
            } else a = "";
            e.cookie = t + "=" + n + a + "; path=/"
        }, I.removeCookie = function (t) {
            I.setCookie(t, "", -1)
        }, I.getElementProperty = function (t, e) {
            if ("@" === e.charAt(0)) {
                var i = I.specialProperties[e.substring(1)];
                if (i) return i(t)
            }
            return "innerText" === e ? I.text(t) : e in t ? t[e] : t.getAttribute ? t.getAttribute(e) : n
        }, I.propertiesMatch = function (t, e) {
            if (t)
                for (var n in t)
                    if (t.hasOwnProperty(n)) {
                        var i = t[n],
                            a = I.getElementProperty(e, n);
                        if ("string" == typeof i && i !== a) return !1;
                        if (i instanceof RegExp && !i.test(a)) return !1
                    }
            return !0
        }, I.isRightClick = function (t) {
            var e;
            return t.which ? e = 3 == t.which : t.button && (e = 2 == t.button), e
        }, I.ruleMatches = function (t, e, n, i) {
            var a = t.condition,
                r = t.conditions,
                o = t.property,
                s = e.type,
                c = t.value,
                u = e.target || e.srcElement,
                l = n === u;
            if (t.event !== s && ("custom" !== t.event || t.customEvent !== s)) return !1;
            if (!I.ruleInScope(t)) return !1;
            if ("click" === t.event && I.isRightClick(e)) return !1;
            if (t.isDefault && i > 0) return !1;
            if (t.expired) return !1;
            if ("inview" === s && e.inviewDelay !== t.inviewDelay) return !1;
            if (!l && (t.bubbleFireIfParent === !1 || 0 !== i && t.bubbleFireIfChildFired === !1)) return !1;
            if (t.selector && !I.matchesCss(t.selector, n)) return !1;
            if (!I.propertiesMatch(o, n)) return !1;
            if (null != c)
                if ("string" == typeof c) {
                    if (c !== n.value) return !1
                } else if (!c.test(n.value)) return !1;
            if (a) try {
                if (!a.call(n, e, u)) return I.notify('Condition for rule "' + t.name + '" not met.', 1), !1
            } catch (d) {
                return I.notify('Condition for rule "' + t.name + '" not met. Error: ' + d.message, 1), !1
            }
            if (r) {
                var f = I.find(r, function (i) {
                    try {
                        return !i.call(n, e, u)
                    } catch (a) {
                        return I.notify('Condition for rule "' + t.name + '" not met. Error: ' + a.message, 1), !0
                    }
                });
                if (f) return I.notify("Condition " + f.toString() + ' for rule "' + t.name + '" not met.', 1), !1
            }
            return !0
        }, I.evtHandlers = {}, I.bindEvent = function (t, e) {
            var n = I.evtHandlers;
            n[t] || (n[t] = []), n[t].push(e)
        }, I.whenEvent = I.bindEvent, I.unbindEvent = function (t, e) {
            var n = I.evtHandlers;
            if (n[t]) {
                var i = I.indexOf(n[t], e);
                n[t].splice(i, 1)
            }
        }, I.bindEventOnce = function (t, e) {
            var n = function () {
                I.unbindEvent(t, n), e.apply(null, arguments)
            };
            I.bindEvent(t, n)
        }, I.isVMLPoisoned = function (t) {
            if (!t) return !1;
            try {
                t.nodeName
            } catch (e) {
                if ("Attribute only valid on v:image" === e.message) return !0
            }
            return !1
        }, I.handleEvent = function (t) {
            if (!I.$data(t, "eventProcessed")) {
                var e = t.type.toLowerCase(),
                    n = t.target || t.srcElement,
                    i = 0,
                    a = I.rules,
                    r = (I.tools, I.evtHandlers[t.type]);
                if (I.isVMLPoisoned(n)) return void I.notify("detected " + e + " on poisoned VML element, skipping.", 1);
                r && I.each(r, function (e) {
                    e(t)
                });
                var o = n && n.nodeName;
                o ? I.notify("detected " + e + " on " + n.nodeName, 1) : I.notify("detected " + e, 1);
                for (var s = n; s; s = s.parentNode) {
                    var c = !1;
                    if (I.each(a, function (e) {
                            I.ruleMatches(e, t, s, i) && (I.notify('Rule "' + e.name + '" fired.', 1), I.fireRule(e, s, t), i++, e.bubbleStop && (c = !0))
                    }), c) break
                }
                I.$data(t, "eventProcessed", !0)
            }
        }, I.onEvent = e.querySelectorAll ? function (t) {
            I.handleEvent(t)
        } : function () {
            var t = [],
                e = function (e) {
                    e.selector ? t.push(e) : I.handleEvent(e)
                };
            return e.pendingEvents = t, e
        }(), I.fireEvent = function (t, e) {
            I.onEvent({
                type: t,
                target: e
            })
        }, I.registerEvents = function (t, e) {
            for (var n = e.length - 1; n >= 0; n--) {
                var i = e[n];
                I.$data(t, i + ".tracked") || (I.addEventHandler(t, i, I.onEvent), I.$data(t, i + ".tracked", !0))
            }
        }, I.registerEventsForTags = function (t, n) {
            for (var i = t.length - 1; i >= 0; i--)
                for (var a = t[i], r = e.getElementsByTagName(a), o = r.length - 1; o >= 0; o--) I.registerEvents(r[o], n)
        }, I.setListeners = function () {
            var t = ["click", "submit"];
            I.each(I.rules, function (e) {
                "custom" === e.event && e.hasOwnProperty("customEvent") && !I.contains(t, e.customEvent) && t.push(e.customEvent)
            }), I.registerEvents(e, t)
        }, I.getUniqueRuleEvents = function () {
            return I._uniqueRuleEvents || (I._uniqueRuleEvents = [], I.each(I.rules, function (t) {
                -1 === I.indexOf(I._uniqueRuleEvents, t.event) && I._uniqueRuleEvents.push(t.event)
            })), I._uniqueRuleEvents
        }, I.setFormListeners = function () {
            if (!I._relevantFormEvents) {
                var t = ["change", "focus", "blur", "keypress"];
                I._relevantFormEvents = I.filter(I.getUniqueRuleEvents(), function (e) {
                    return -1 !== I.indexOf(t, e)
                })
            }
            I._relevantFormEvents.length && I.registerEventsForTags(["input", "select", "textarea", "button"], I._relevantFormEvents)
        }, I.setVideoListeners = function () {
            if (!I._relevantVideoEvents) {
                var t = ["play", "pause", "ended", "volumechange", "stalled", "loadeddata"];
                I._relevantVideoEvents = I.filter(I.getUniqueRuleEvents(), function (e) {
                    return -1 !== I.indexOf(t, e)
                })
            }
            I._relevantVideoEvents.length && I.registerEventsForTags(["video"], I._relevantVideoEvents)
        }, I.readStoredSetting = function (e) {
            try {
                return e = "sdsat_" + e, t.localStorage.getItem(e)
            } catch (n) {
                return I.notify("Cannot read stored setting from localStorage: " + n.message, 2), null
            }
        }, I.loadStoredSettings = function () {
            var t = I.readStoredSetting("debug"),
                e = I.readStoredSetting("hide_activity");
            t && (I.settings.notifications = "true" === t), e && (I.settings.hideActivity = "true" === e)
        }, I.isRuleActive = function (t, e) {
            function n(t, e) {
                return e = a(e, {
                    hour: t[h](),
                    minute: t[p]()
                }), Math.floor(Math.abs((t.getTime() - e.getTime()) / 864e5))
            }

            function i(t, e) {
                function n(t) {
                    return 12 * t[d]() + t[f]()
                }
                return Math.abs(n(t) - n(e))
            }

            function a(t, e) {
                var n = new Date(t.getTime());
                for (var i in e)
                    if (e.hasOwnProperty(i)) {
                        var a = e[i];
                        switch (i) {
                            case "hour":
                                n[g](a);
                                break;
                            case "minute":
                                n[v](a);
                                break;
                            case "date":
                                n[m](a)
                        }
                    }
                return n
            }

            function r(t, e) {
                var n = t[h](),
                    i = t[p](),
                    a = e[h](),
                    r = e[p]();
                return 60 * n + i > 60 * a + r
            }

            function o(t, e) {
                var n = t[h](),
                    i = t[p](),
                    a = e[h](),
                    r = e[p]();
                return 60 * a + r > 60 * n + i
            }
            var s = t.schedule;
            if (!s) return !0;
            var c = s.utc,
                u = c ? "getUTCDate" : "getDate",
                l = c ? "getUTCDay" : "getDay",
                d = c ? "getUTCFullYear" : "getFullYear",
                f = c ? "getUTCMonth" : "getMonth",
                h = c ? "getUTCHours" : "getHours",
                p = c ? "getUTCMinutes" : "getMinutes",
                g = c ? "setUTCHours" : "setHours",
                v = c ? "setUTCMinutes" : "setMinutes",
                m = c ? "setUTCDate" : "setDate";
            if (e = e || new Date, s.repeat) {
                if (r(s.start, e)) return !1;
                if (o(s.end, e)) return !1;
                if (e < s.start) return !1;
                if (s.endRepeat && e >= s.endRepeat) return !1;
                if ("daily" === s.repeat) {
                    if (s.repeatEvery) {
                        var y = n(s.start, e);
                        if (y % s.repeatEvery !== 0) return !1
                    }
                } else if ("weekly" === s.repeat) {
                    if (s.days) {
                        if (!I.contains(s.days, e[l]())) return !1
                    } else if (s.start[l]() !== e[l]()) return !1;
                    if (s.repeatEvery) {
                        var b = n(s.start, e);
                        if (b % (7 * s.repeatEvery) !== 0) return !1
                    }
                } else if ("monthly" === s.repeat) {
                    if (s.repeatEvery) {
                        var k = i(s.start, e);
                        if (k % s.repeatEvery !== 0) return !1
                    }
                    if (s.nthWeek && s.mthDay) {
                        if (s.mthDay !== e[l]()) return !1;
                        var S = Math.floor((e[u]() - e[l]() + 1) / 7);
                        if (s.nthWeek !== S) return !1
                    } else if (s.start[u]() !== e[u]()) return !1
                } else if ("yearly" === s.repeat) {
                    if (s.start[f]() !== e[f]()) return !1;
                    if (s.start[u]() !== e[u]()) return !1;
                    if (s.repeatEvery) {
                        var b = Math.abs(s.start[d]() - e[d]());
                        if (b % s.repeatEvery !== 0) return !1
                    }
                }
            } else {
                if (s.start > e) return !1;
                if (s.end < e) return !1
            }
            return !0
        }, I.isOutboundLink = function (t) {
            if (!t.getAttribute("href")) return !1;
            var e = t.hostname,
                n = (t.href, t.protocol);
            if ("http:" !== n && "https:" !== n) return !1;
            var i = I.any(I.settings.domainList, function (t) {
                return I.isSubdomainOf(e, t)
            });
            return i ? !1 : e !== location.hostname
        }, I.isLinkerLink = function (t) {
            return t.getAttribute && t.getAttribute("href") ? I.hasMultipleDomains() && t.hostname != location.hostname && !t.href.match(/^javascript/i) && !I.isOutboundLink(t) : !1
        }, I.isSubdomainOf = function (t, e) {
            if (t === e) return !0;
            var n = t.length - e.length;
            return n > 0 ? I.equalsIgnoreCase(t.substring(n), e) : !1
        }, I.getVisitorId = function () {
            var t = I.getToolsByType("visitor_id");
            return 0 === t.length ? null : t[0].getInstance()
        }, I.URI = function () {
            var t = e.location.pathname + e.location.search;
            return I.settings.forceLowerCase && (t = t.toLowerCase()), t
        }, I.URL = function () {
            var t = e.location.href;
            return I.settings.forceLowerCase && (t = t.toLowerCase()), t
        }, I.filterRules = function () {
            function t(t) {
                return I.isRuleActive(t) ? !0 : !1
            }
            I.rules = I.filter(I.rules, t), I.pageLoadRules = I.filter(I.pageLoadRules, t)
        }, I.ruleInScope = function (t, n) {
            function i(t, e) {
                function n(t) {
                    return e.match(t)
                }
                var i = t.include,
                    r = t.exclude;
                if (i && a(i, e)) return !0;
                if (r) {
                    if (I.isString(r) && r === e) return !0;
                    if (I.isArray(r) && I.any(r, n)) return !0;
                    if (I.isRegex(r) && n(r)) return !0
                }
                return !1
            }

            function a(t, e) {
                function n(t) {
                    return e.match(t)
                }
                return I.isString(t) && t !== e ? !0 : I.isArray(t) && !I.any(t, n) ? !0 : I.isRegex(t) && !n(t) ? !0 : !1
            }
            n = n || e.location;
            var r = t.scope;
            if (!r) return !0;
            var o = r.URI,
                s = r.subdomains,
                c = r.domains,
                u = r.protocols,
                l = r.hashes;
            return o && i(o, n.pathname + n.search) ? !1 : s && i(s, n.hostname) ? !1 : c && a(c, n.hostname) ? !1 : u && a(u, n.protocol) ? !1 : l && i(l, n.hash) ? !1 : !0
        }, I.backgroundTasks = function () {
            +new Date;
            I.setFormListeners(), I.setVideoListeners(), I.loadStoredSettings(), I.registerNewElementsForDynamicRules(), I.eventEmitterBackgroundTasks(); +new Date
        }, I.registerNewElementsForDynamicRules = function () {
            function t(e, n) {
                var i = t.cache[e];
                return i ? n(i) : void I.cssQuery(e, function (i) {
                    t.cache[e] = i, n(i)
                })
            }
            t.cache = {}, I.each(I.dynamicRules, function (e) {
                t(e.selector, function (t) {
                    I.each(t, function (t) {
                        var n = "custom" === e.event ? e.customEvent : e.event;
                        I.$data(t, "dynamicRules.seen." + n) || (I.$data(t, "dynamicRules.seen." + n, !0), I.propertiesMatch(e.property, t) && I.registerEvents(t, [n]))
                    })
                })
            })
        }, I.ensureCSSSelector = function () {
            return e.querySelectorAll ? void (I.hasSelector = !0) : (I.loadingSizzle = !0, I.sizzleQueue = [], void I.loadScript(I.basePath() + "selector.js", function () {
                if (!I.Sizzle) return void I.logError(new Error("Failed to load selector.js"));
                var t = I.onEvent.pendingEvents;
                I.each(t, function (t) {
                    I.handleEvent(t)
                }, this), I.onEvent = I.handleEvent, I.hasSelector = !0, delete I.loadingSizzle, I.each(I.sizzleQueue, function (t) {
                    I.cssQuery(t[0], t[1])
                }), delete I.sizzleQueue
            }))
        }, I.errors = [], I.logError = function (t) {
            I.errors.push(t), I.notify(t.name + " - " + t.message, 5)
        }, I.pageBottom = function () {
            I.initialized && (I.pageBottomFired = !0, I.firePageLoadEvent("pagebottom"))
        }, I.stagingLibraryOverride = function () {
            var t = "true" === I.readStoredSetting("stagingLibrary");
            if (t) {
                for (var n, i, a, r = e.getElementsByTagName("script"), o = /^(.*)satelliteLib-([a-f0-9]{40})\.js$/, s = /^(.*)satelliteLib-([a-f0-9]{40})-staging\.js$/, c = 0, u = r.length; u > c && (a = r[c].getAttribute("src"), !a || (n || (n = a.match(o)), i || (i = a.match(s)), !i)) ; c++);
                if (n && !i) {
                    var l = n[1] + "satelliteLib-" + n[2] + "-staging.js";
                    if (e.write) e.write('<script src="' + l + '"></script>');
                    else {
                        var d = e.createElement("script");
                        d.src = l, e.head.appendChild(d)
                    }
                    return !0
                }
            }
            return !1
        }, I.checkAsyncInclude = function () {
            t.satellite_asyncLoad && I.notify('You may be using the async installation of Satellite. In-page HTML and the "pagebottom" event will not work. Please update your Satellite installation for these features.', 5)
        }, I.hasMultipleDomains = function () {
            return !!I.settings.domainList && I.settings.domainList.length > 1
        }, I.handleOverrides = function () {
            if (E)
                for (var t in E) E.hasOwnProperty(t) && (I.data[t] = E[t])
        }, I.privacyManagerParams = function () {
            var t = {};
            I.extend(t, I.settings.privacyManagement);
            var e = [];
            for (var n in I.tools)
                if (I.tools.hasOwnProperty(n)) {
                    var i = I.tools[n],
                        a = i.settings;
                    if (!a) continue;
                    "sc" === a.engine && e.push(i)
                }
            var r = I.filter(I.map(e, function (t) {
                return t.getTrackingServer()
            }), function (t) {
                return null != t
            });
            t.adobeAnalyticsTrackingServers = r;
            for (var o = ["bannerText", "headline", "introductoryText", "customCSS"], s = 0; s < o.length; s++) {
                var c = o[s],
                    u = t[c];
                if (u)
                    if ("text" === u.type) t[c] = u.value;
                    else {
                        if ("data" !== u.type) throw new Error("Invalid type: " + u.type);
                        t[c] = I.getVar(u.value)
                    }
            }
            return t
        }, I.prepareLoadPrivacyManager = function () {
            function e(t) {
                function e() {
                    r++, r === a.length && (n(), clearTimeout(o), t())
                }

                function n() {
                    I.each(a, function (t) {
                        I.unbindEvent(t.id + ".load", e)
                    })
                }

                function i() {
                    n(), t()
                }
                var a = I.filter(I.values(I.tools), function (t) {
                    return t.settings && "sc" === t.settings.engine
                });
                if (0 === a.length) return t();
                var r = 0;
                I.each(a, function (t) {
                    I.bindEvent(t.id + ".load", e)
                });
                var o = setTimeout(i, 5e3)
            }
            I.addEventHandler(t, "load", function () {
                e(I.loadPrivacyManager)
            })
        }, I.loadPrivacyManager = function () {
            var t = I.basePath() + "privacy_manager.js";
            I.loadScript(t, function () {
                var t = I.privacyManager;
                t.configure(I.privacyManagerParams()), t.openIfRequired()
            })
        }, I.init = function (e) {
            if (!I.stagingLibraryOverride()) {
                I.configurationSettings = e;
                var i = e.tools;
                delete e.tools;
                for (var a in e) e.hasOwnProperty(a) && (I[a] = e[a]);
                I.data.customVars === n && (I.data.customVars = {}), I.data.queryParams = I.QueryParams.normal, I.handleOverrides(), I.detectBrowserInfo(), I.trackVisitorInfo && I.trackVisitorInfo(), I.loadStoredSettings(), I.Logger.setOutputState(I.settings.notifications), I.checkAsyncInclude(), I.ensureCSSSelector(), I.filterRules(), I.dynamicRules = I.filter(I.rules, function (t) {
                    return t.eventHandlerOnElement
                }), I.tools = I.initTools(i), I.initEventEmitters(), I.firePageLoadEvent("aftertoolinit"), I.settings.privacyManagement && I.prepareLoadPrivacyManager(), I.hasSelector && I.domReady(I.eventEmitterBackgroundTasks), I.setListeners(), I.domReady(function () {
                    I.poll(function () {
                        I.backgroundTasks()
                    }, I.settings.recheckEvery || 3e3)
                }), I.domReady(function () {
                    I.domReadyFired = !0, I.pageBottomFired || I.pageBottom(), I.firePageLoadEvent("domready")
                }), I.addEventHandler(t, "load", function () {
                    I.firePageLoadEvent("windowload")
                }), I.firePageLoadEvent("pagetop"), I.initialized = !0
            }
        }, I.pageLoadPhases = ["aftertoolinit", "pagetop", "pagebottom", "domready", "windowload"], I.loadEventBefore = function (t, e) {
            return I.indexOf(I.pageLoadPhases, t) <= I.indexOf(I.pageLoadPhases, e)
        }, I.flushPendingCalls = function (t) {
            t.pending && (I.each(t.pending, function (e) {
                var n = e[0],
                    i = e[1],
                    a = e[2],
                    r = e[3];
                n in t ? t[n].apply(t, [i, a].concat(r)) : t.emit ? t.emit(n, i, a, r) : I.notify("Failed to trigger " + n + " for tool " + t.id, 1)
            }), delete t.pending)
        }, I.setDebug = function (e) {
            try {
                t.localStorage.setItem("sdsat_debug", e)
            } catch (n) {
                I.notify("Cannot set debug mode: " + n.message, 2)
            }
        }, I.getUserAgent = function () {
            return navigator.userAgent
        }, I.detectBrowserInfo = function () {
            function t(t) {
                return function (e) {
                    for (var n in t)
                        if (t.hasOwnProperty(n)) {
                            var i = t[n],
                                a = i.test(e);
                            if (a) return n
                        }
                    return "Unknown"
                }
            }
            var e = t({
                "IE Edge Mobile": /Windows Phone.*Edge/,
                "IE Edge": /Edge/,
                OmniWeb: /OmniWeb/,
                "Opera Mini": /Opera Mini/,
                "Opera Mobile": /Opera Mobi/,
                Opera: /Opera/,
                Chrome: /Chrome|CriOS|CrMo/,
                Firefox: /Firefox|FxiOS/,
                "IE Mobile": /IEMobile/,
                IE: /MSIE|Trident/,
                "Mobile Safari": /Mobile(\/[0-9A-z]+)? Safari/,
                Safari: /Safari/
            }),
                n = t({
                    Blackberry: /BlackBerry|BB10/,
                    "Symbian OS": /Symbian|SymbOS/,
                    Maemo: /Maemo/,
                    Android: /Android/,
                    Linux: / Linux /,
                    Unix: /FreeBSD|OpenBSD|CrOS/,
                    Windows: /[\( ]Windows /,
                    iOS: /iPhone|iPad|iPod/,
                    MacOS: /Macintosh;/
                }),
                i = t({
                    Nokia: /Symbian|SymbOS|Maemo/,
                    "Windows Phone": /Windows Phone/,
                    Blackberry: /BlackBerry|BB10/,
                    Android: /Android/,
                    iPad: /iPad/,
                    iPod: /iPod/,
                    iPhone: /iPhone/,
                    Desktop: /.*/
                }),
                a = I.getUserAgent();
            I.browserInfo = {
                browser: e(a),
                os: n(a),
                deviceType: i(a)
            }
        }, I.isHttps = function () {
            return "https:" == e.location.protocol
        }, I.BaseTool = function (t) {
            this.settings = t || {}, this.forceLowerCase = I.settings.forceLowerCase, "forceLowerCase" in this.settings && (this.forceLowerCase = this.settings.forceLowerCase)
        }, I.BaseTool.prototype = {
            triggerCommand: function (t, e, n) {
                var i = this.settings || {};
                if (this.initialize && this.isQueueAvailable() && this.isQueueable(t) && n && I.loadEventBefore(n.type, i.loadOn)) return void this.queueCommand(t, e, n);
                var a = t.command,
                    r = this["$" + a],
                    o = r ? r.escapeHtml : !1,
                    s = I.preprocessArguments(t.arguments, e, n, this.forceLowerCase, o);
                r ? r.apply(this, [e, n].concat(s)) : this.$missing$ ? this.$missing$(a, e, n, s) : I.notify("Failed to trigger " + a + " for tool " + this.id, 1)
            },
            endPLPhase: function (t) { },
            isQueueable: function (t) {
                return "cancelToolInit" !== t.command
            },
            isQueueAvailable: function () {
                return !this.initialized && !this.initializing
            },
            flushQueue: function () {
                this.pending && (I.each(this.pending, function (t) {
                    this.triggerCommand.apply(this, t)
                }, this), this.pending = [])
            },
            queueCommand: function (t, e, n) {
                this.pending || (this.pending = []), this.pending.push([t, e, n])
            },
            $cancelToolInit: function () {
                this._cancelToolInit = !0
            }
        }, t._satellite = I, i.prototype.backgroundTasks = function () {
            I.each(this.rules, function (t) {
                I.cssQuery(t.selector, function (t) {
                    if (t.length > 0) {
                        var e = t[0];
                        if (I.$data(e, "elementexists.seen")) return;
                        I.$data(e, "elementexists.seen", !0), I.onEvent({
                            type: "elementexists",
                            target: e
                        })
                    }
                })
            })
        }, I.availableEventEmitters.push(i),
            a.prototype = {
                obue: !1,
                initialize: function () {
                    this.attachCloseListeners()
                },
                obuePrevUnload: function () { },
                obuePrevBeforeUnload: function () { },
                newObueListener: function () {
                    this.obue || (this.obue = !0, this.triggerBeacons())
                },
                attachCloseListeners: function () {
                    this.prevUnload = t.onunload, this.prevBeforeUnload = t.onbeforeunload, t.onunload = I.bind(function (e) {
                        this.prevUnload && setTimeout(I.bind(function () {
                            this.prevUnload.call(t, e)
                        }, this), 1), this.newObueListener()
                    }, this), t.onbeforeunload = I.bind(function (e) {
                        this.prevBeforeUnload && setTimeout(I.bind(function () {
                            this.prevBeforeUnload.call(t, e)
                        }, this), 1), this.newObueListener()
                    }, this)
                },
                triggerBeacons: function () {
                    I.fireEvent("leave", e)
                }
            }, I.availableEventEmitters.push(a), r.orientationChange = function (e) {
                var n = 0 === t.orientation ? "portrait" : "landscape";
                e.orientation = n, I.onEvent(e)
            }, I.availableEventEmitters.push(r), o.prototype = {
                backgroundTasks: function () {
                    var t = this;
                    I.each(this.rules, function (e) {
                        var n = e[1],
                            i = e[0];
                        I.cssQuery(n, function (e) {
                            I.each(e, function (e) {
                                t.trackElement(e, i)
                            })
                        })
                    }, this)
                },
                trackElement: function (t, e) {
                    var n = this,
                        i = I.$data(t, "hover.delays");
                    i ? I.contains(i, e) || i.push(e) : (I.addEventHandler(t, "mouseover", function (e) {
                        n.onMouseOver(e, t)
                    }), I.addEventHandler(t, "mouseout", function (e) {
                        n.onMouseOut(e, t)
                    }), I.$data(t, "hover.delays", [e]))
                },
                onMouseOver: function (t, e) {
                    var n = t.target || t.srcElement,
                        i = t.relatedTarget || t.fromElement,
                        a = (e === n || I.containsElement(e, n)) && !I.containsElement(e, i);
                    a && this.onMouseEnter(e)
                },
                onMouseEnter: function (t) {
                    var e = I.$data(t, "hover.delays"),
                        n = I.map(e, function (e) {
                            return setTimeout(function () {
                                I.onEvent({
                                    type: "hover(" + e + ")",
                                    target: t
                                })
                            }, e)
                        });
                    I.$data(t, "hover.delayTimers", n)
                },
                onMouseOut: function (t, e) {
                    var n = t.target || t.srcElement,
                        i = t.relatedTarget || t.toElement,
                        a = (e === n || I.containsElement(e, n)) && !I.containsElement(e, i);
                    a && this.onMouseLeave(e)
                },
                onMouseLeave: function (t) {
                    var e = I.$data(t, "hover.delayTimers");
                    e && I.each(e, function (t) {
                        clearTimeout(t)
                    })
                }
            }, I.availableEventEmitters.push(o), s.prototype = {
                defineEvents: function () {
                    this.oldBlurClosure = function () {
                        I.fireEvent("tabblur", e)
                    }, this.oldFocusClosure = I.bind(function () {
                        this.visibilityApiHasPriority ? I.fireEvent("tabfocus", e) : null != I.visibility.getHiddenProperty() ? I.visibility.isHidden() || I.fireEvent("tabfocus", e) : I.fireEvent("tabfocus", e)
                    }, this)
                },
                attachDetachModernEventListeners: function (t) {
                    var n = 0 == t ? "removeEventHandler" : "addEventHandler";
                    I[n](e, I.visibility.getVisibilityEvent(), this.handleVisibilityChange)
                },
                attachDetachOlderEventListeners: function (e, n, i) {
                    var a = 0 == e ? "removeEventHandler" : "addEventHandler";
                    I[a](n, i, this.oldBlurClosure), I[a](t, "focus", this.oldFocusClosure)
                },
                handleVisibilityChange: function () {
                    I.visibility.isHidden() ? I.fireEvent("tabblur", e) : I.fireEvent("tabfocus", e)
                },
                setVisibilityApiPriority: function (e) {
                    this.visibilityApiHasPriority = e, this.attachDetachOlderEventListeners(!1, t, "blur"), this.attachDetachModernEventListeners(!1), e ? null != I.visibility.getHiddenProperty() ? this.attachDetachModernEventListeners(!0) : this.attachDetachOlderEventListeners(!0, t, "blur") : (this.attachDetachOlderEventListeners(!0, t, "blur"), null != I.visibility.getHiddenProperty() && this.attachDetachModernEventListeners(!0))
                },
                oldBlurClosure: null,
                oldFocusClosure: null,
                visibilityApiHasPriority: !0
            }, I.availableEventEmitters.push(s), c.prototype = {
                initialize: function () {
                    this.setupHistoryAPI(), this.setupHashChange()
                },
                fireIfURIChanged: function () {
                    var t = I.URL();
                    this.lastURL !== t && (this.fireEvent(), this.lastURL = t)
                },
                fireEvent: function () {
                    I.updateQueryParams(), I.onEvent({
                        type: "locationchange",
                        target: e
                    })
                },
                setupSPASupport: function () {
                    this.setupHistoryAPI(), this.setupHashChange()
                },
                setupHistoryAPI: function () {
                    var e = t.history;
                    e && (e.pushState && (this.originalPushState = e.pushState, e.pushState = this._pushState), e.replaceState && (this.originalReplaceState = e.replaceState, e.replaceState = this._replaceState)), I.addEventHandler(t, "popstate", this._onPopState)
                },
                pushState: function () {
                    var t = this.originalPushState.apply(history, arguments);
                    return this.onPushState(), t
                },
                replaceState: function () {
                    var t = this.originalReplaceState.apply(history, arguments);
                    return this.onReplaceState(), t
                },
                setupHashChange: function () {
                    I.addEventHandler(t, "hashchange", this._onHashChange)
                },
                onReplaceState: function () {
                    setTimeout(this._fireIfURIChanged, 0)
                },
                onPushState: function () {
                    setTimeout(this._fireIfURIChanged, 0)
                },
                onPopState: function () {
                    setTimeout(this._fireIfURIChanged, 0)
                },
                onHashChange: function () {
                    setTimeout(this._fireIfURIChanged, 0)
                },
                uninitialize: function () {
                    this.cleanUpHistoryAPI(), this.cleanUpHashChange()
                },
                cleanUpHistoryAPI: function () {
                    history.pushState === this._pushState && (history.pushState = this.originalPushState), history.replaceState === this._replaceState && (history.replaceState = this.originalReplaceState), I.removeEventHandler(t, "popstate", this._onPopState)
                },
                cleanUpHashChange: function () {
                    I.removeEventHandler(t, "hashchange", this._onHashChange)
                }
            }, I.availableEventEmitters.push(c), u.prototype = {
                backgroundTasks: function () {
                    var t = this.eventHandler;
                    I.each(this.rules, function (e) {
                        I.cssQuery(e.selector || "video", function (e) {
                            I.each(e, function (e) {
                                I.$data(e, "videoplayed.tracked") || (I.addEventHandler(e, "timeupdate", I.throttle(t, 100)), I.$data(e, "videoplayed.tracked", !0))
                            })
                        })
                    })
                },
                evalRule: function (t, e) {
                    var n = e.event,
                        i = t.seekable,
                        a = i.start(0),
                        r = i.end(0),
                        o = t.currentTime,
                        s = e.event.match(/^videoplayed\(([0-9]+)([s%])\)$/);
                    if (s) {
                        var c = s[2],
                            u = Number(s[1]),
                            l = "%" === c ? function () {
                                return 100 * (o - a) / (r - a) >= u
                            } : function () {
                                return o - a >= u
                            };
                        !I.$data(t, n) && l() && (I.$data(t, n, !0), I.onEvent({
                            type: n,
                            target: t
                        }))
                    }
                },
                onUpdateTime: function (t) {
                    var e = this.rules,
                        n = t.target;
                    if (n.seekable && 0 !== n.seekable.length)
                        for (var i = 0, a = e.length; a > i; i++) this.evalRule(n, e[i])
                }
            }, I.availableEventEmitters.push(u), l.prototype = {
                initialize: function () {
                    return this.FB = this.FB || t.FB, this.FB && this.FB.Event && this.FB.Event.subscribe ? (this.bind(), !0) : void 0
                },
                bind: function () {
                    this.FB.Event.subscribe("edge.create", function () {
                        I.notify("tracking a facebook like", 1), I.onEvent({
                            type: "facebook.like",
                            target: e
                        })
                    }), this.FB.Event.subscribe("edge.remove", function () {
                        I.notify("tracking a facebook unlike", 1), I.onEvent({
                            type: "facebook.unlike",
                            target: e
                        })
                    }), this.FB.Event.subscribe("message.send", function () {
                        I.notify("tracking a facebook share", 1), I.onEvent({
                            type: "facebook.send",
                            target: e
                        })
                    })
                }
            }, I.availableEventEmitters.push(l), d.prototype = {
                initialize: function () {
                    var t = this.twttr;
                    t && "function" == typeof t.ready && t.ready(I.bind(this.bind, this))
                },
                bind: function () {
                    this.twttr.events.bind("tweet", function (t) {
                        t && (I.notify("tracking a tweet button", 1), I.onEvent({
                            type: "twitter.tweet",
                            target: e
                        }))
                    })
                }
            }, I.availableEventEmitters.push(d), f.offset = function (n) {
                var i = null,
                    a = null;
                try {
                    var r = n.getBoundingClientRect(),
                        o = e,
                        s = o.documentElement,
                        c = o.body,
                        u = t,
                        l = s.clientTop || c.clientTop || 0,
                        d = s.clientLeft || c.clientLeft || 0,
                        f = u.pageYOffset || s.scrollTop || c.scrollTop,
                        h = u.pageXOffset || s.scrollLeft || c.scrollLeft;
                    i = r.top + f - l, a = r.left + h - d
                } catch (p) { }
                return {
                    top: i,
                    left: a
                }
            }, f.getViewportHeight = function () {
                var n = t.innerHeight,
                    i = e.compatMode;
                return i && (n = "CSS1Compat" == i ? e.documentElement.clientHeight : e.body.clientHeight), n
            }, f.getScrollTop = function () {
                return e.documentElement.scrollTop ? e.documentElement.scrollTop : e.body.scrollTop
            }, f.isElementInDocument = function (t) {
                return e.body.contains(t)
            }, f.isElementInView = function (t) {
                if (!f.isElementInDocument(t)) return !1;
                var e = f.getViewportHeight(),
                    n = f.getScrollTop(),
                    i = f.offset(t).top,
                    a = t.offsetHeight;
                return null !== i ? !(n > i + a || i > n + e) : !1
            }, f.prototype = {
                backgroundTasks: function () {
                    var t = this.elements;
                    I.each(this.rules, function (e) {
                        I.cssQuery(e.selector, function (n) {
                            var i = 0;
                            I.each(n, function (e) {
                                I.contains(t, e) || (t.push(e), i++)
                            }), i && I.notify(e.selector + " added " + i + " elements.", 1)
                        })
                    }), this.track()
                },
                checkInView: function (t, e, n) {
                    var i = I.$data(t, "inview");
                    if (f.isElementInView(t)) {
                        i || I.$data(t, "inview", !0);
                        var a = this;
                        this.processRules(t, function (n, i, r) {
                            if (e || !n.inviewDelay) I.$data(t, i, !0), I.onEvent({
                                type: "inview",
                                target: t,
                                inviewDelay: n.inviewDelay
                            });
                            else if (n.inviewDelay) {
                                var o = I.$data(t, r);
                                o || (o = setTimeout(function () {
                                    a.checkInView(t, !0, n.inviewDelay)
                                }, n.inviewDelay), I.$data(t, r, o))
                            }
                        }, n)
                    } else {
                        if (!f.isElementInDocument(t)) {
                            var r = I.indexOf(this.elements, t);
                            this.elements.splice(r, 1)
                        }
                        i && I.$data(t, "inview", !1), this.processRules(t, function (e, n, i) {
                            var a = I.$data(t, i);
                            a && clearTimeout(a)
                        }, n)
                    }
                },
                track: function () {
                    for (var t = this.elements.length - 1; t >= 0; t--) this.checkInView(this.elements[t])
                },
                processRules: function (t, e, n) {
                    var i = this.rules;
                    n && (i = I.filter(this.rules, function (t) {
                        return t.inviewDelay == n
                    })), I.each(i, function (n, i) {
                        var a = n.inviewDelay ? "viewed_" + n.inviewDelay : "viewed",
                            r = "inview_timeout_id_" + i;
                        I.$data(t, a) || I.matchesCss(n.selector, t) && e(n, a, r)
                    })
                }
            }, I.availableEventEmitters.push(f), h.prototype.getStringifiedValue = t.JSON && t.JSON.stringify || I.stringify, h.prototype.initPolling = function () {
                0 !== this.dataElementsNames.length && (this.dataElementsStore = this.getDataElementsValues(), I.poll(I.bind(this.checkDataElementValues, this), 1e3))
            }, h.prototype.getDataElementsValues = function () {
                var t = {};
                return I.each(this.dataElementsNames, function (e) {
                    var n = I.getVar(e);
                    t[e] = this.getStringifiedValue(n)
                }, this), t
            }, h.prototype.checkDataElementValues = function () {
                I.each(this.dataElementsNames, I.bind(function (t) {
                    var n = this.getStringifiedValue(I.getVar(t)),
                        i = this.dataElementsStore[t];
                    n !== i && (this.dataElementsStore[t] = n, I.onEvent({
                        type: "dataelementchange(" + t + ")",
                        target: e
                    }))
                }, this))
            }, I.availableEventEmitters.push(h), I.visibility = {
                isHidden: function () {
                    var t = this.getHiddenProperty();
                    return t ? e[t] : !1
                },
                isVisible: function () {
                    return !this.isHidden()
                },
                getHiddenProperty: function () {
                    var t = ["webkit", "moz", "ms", "o"];
                    if ("hidden" in e) return "hidden";
                    for (var n = 0; n < t.length; n++)
                        if (t[n] + "Hidden" in e) return t[n] + "Hidden";
                    return null
                },
                getVisibilityEvent: function () {
                    var t = this.getHiddenProperty();
                    return t ? t.replace(/[H|h]idden/, "") + "visibilitychange" : null
                }
            }, I.ecommerce = {
                addItem: function () {
                    var t = [].slice.call(arguments);
                    I.onEvent({
                        type: "ecommerce.additem",
                        target: t
                    })
                },
                addTrans: function () {
                    var t = [].slice.call(arguments);
                    I.data.saleData.sale = {
                        orderId: t[0],
                        revenue: t[2]
                    }, I.onEvent({
                        type: "ecommerce.addtrans",
                        target: t
                    })
                },
                trackTrans: function () {
                    I.onEvent({
                        type: "ecommerce.tracktrans",
                        target: []
                    })
                }
            }, I.extend(p.prototype, {
                getInstance: function () {
                    return this.instance
                },
                initialize: function () {
                    var t, e = this.settings;
                    I.notify("Visitor ID: Initializing tool", 1), t = this.createInstance(e.mcOrgId, e.initVars), null !== t && (e.customerIDs && this.applyCustomerIDs(t, e.customerIDs), e.autoRequest && t.getMarketingCloudVisitorID(), this.instance = t)
                },
                createInstance: function (t, e) {
                    if (!I.isString(t)) return I.notify('Visitor ID: Cannot create instance using mcOrgId: "' + t + '"', 4), null;
                    t = I.replace(t), I.notify('Visitor ID: Create instance using mcOrgId: "' + t + '"', 1), e = this.parseValues(e);
                    var n = Visitor.getInstance(t, e);
                    return I.notify("Visitor ID: Set variables: " + I.stringify(e), 1), n
                },
                applyCustomerIDs: function (t, e) {
                    var n = this.parseIds(e);
                    t.setCustomerIDs(n), I.notify("Visitor ID: Set Customer IDs: " + I.stringify(n), 1)
                },
                parseValues: function (t) {
                    if (I.isObject(t) === !1) return {};
                    var e = {};
                    for (var n in t) t.hasOwnProperty(n) && (e[n] = I.replace(t[n]));
                    return e
                },
                parseIds: function (t) {
                    var e = {};
                    if (I.isObject(t) === !1) return {};
                    for (var n in t)
                        if (t.hasOwnProperty(n)) {
                            var i = I.replace(t[n].id);
                            i !== t[n].id && i && (e[n] = {}, e[n].id = i, e[n].authState = Visitor.AuthState[t[n].authState])
                        }
                    return e
                }
            }), I.availableTools.visitor_id = p, I.inherit(g, I.BaseTool), I.extend(g.prototype, {
                initialize: function () {
                    var t = this.settings;
                    if (this.settings.initTool !== !1) {
                        var e = t.url;
                        e = "string" == typeof e ? I.basePath() + e : I.isHttps() ? e.https : e.http, I.loadScript(e, I.bind(this.onLoad, this)), this.initializing = !0
                    } else this.initialized = !0
                },
                isQueueAvailable: function () {
                    return !this.initialized
                },
                onLoad: function () {
                    this.initialized = !0, this.initializing = !1, this.settings.initialBeacon && this.settings.initialBeacon(), this.flushQueue()
                },
                endPLPhase: function (t) {
                    var e = this.settings.loadOn;
                    t === e && (I.notify(this.name + ": Initializing at " + t, 1), this.initialize())
                },
                $fire: function (t, e, n) {
                    return this.initializing ? void this.queueCommand({
                        command: "fire",
                        arguments: [n]
                    }, t, e) : void n.call(this.settings, t, e)
                }
            }), I.availableTools.am = g, I.availableTools.adlens = g, I.availableTools.aem = g, I.availableTools.__basic = g, I.inherit(v, I.BaseTool), I.extend(v.prototype, {
                name: "tnt",
                endPLPhase: function (t) {
                    "aftertoolinit" === t && this.initialize()
                },
                initialize: function () {
                    I.notify("Test & Target: Initializing", 1), this.initializeTargetPageParams(), this.load()
                },
                initializeTargetPageParams: function () {
                    t.targetPageParams && this.updateTargetPageParams(this.parseTargetPageParamsResult(t.targetPageParams())), this.updateTargetPageParams(this.settings.pageParams), this.setTargetPageParamsFunction()
                },
                load: function () {
                    var t = this.getMboxURL(this.settings.mboxURL);
                    this.settings.initTool !== !1 ? this.settings.loadSync ? (I.loadScriptSync(t), this.onScriptLoaded()) : (I.loadScript(t, I.bind(this.onScriptLoaded, this)), this.initializing = !0) : this.initialized = !0
                },
                getMboxURL: function (e) {
                    var n = e;
                    return I.isObject(e) && (n = "https:" === t.location.protocol ? e.https : e.http), n.match(/^https?:/) ? n : I.basePath() + n
                },
                onScriptLoaded: function () {
                    I.notify("Test & Target: loaded.", 1), this.flushQueue(), this.initialized = !0, this.initializing = !1
                },
                $addMbox: function (t, e, n) {
                    var i = n.mboxGoesAround,
                        a = i + "{visibility: hidden;}",
                        r = this.appendStyle(a);
                    i in this.styleElements || (this.styleElements[i] = r), this.initialized ? this.$addMBoxStep2(null, null, n) : this.initializing && this.queueCommand({
                        command: "addMBoxStep2",
                        arguments: [n]
                    }, t, e)
                },
                $addMBoxStep2: function (n, i, a) {
                    var r = this.generateID(),
                        o = this;
                    I.addEventHandler(t, "load", I.bind(function () {
                        I.cssQuery(a.mboxGoesAround, function (n) {
                            var i = n[0];
                            if (i) {
                                var s = e.createElement("div");
                                s.id = r, i.parentNode.replaceChild(s, i), s.appendChild(i), t.mboxDefine(r, a.mboxName);
                                var c = [a.mboxName];
                                a.arguments && (c = c.concat(a.arguments)), t.mboxUpdate.apply(null, c), o.reappearWhenCallComesBack(i, r, a.timeout, a)
                            }
                        })
                    }, this)), this.lastMboxID = r
                },
                $addTargetPageParams: function (t, e, n) {
                    this.updateTargetPageParams(n)
                },
                generateID: function () {
                    var t = "_sdsat_mbox_" + String(Math.random()).substring(2) + "_";
                    return t
                },
                appendStyle: function (t) {
                    var n = e.getElementsByTagName("head")[0],
                        i = e.createElement("style");
                    return i.type = "text/css", i.styleSheet ? i.styleSheet.cssText = t : i.appendChild(e.createTextNode(t)), n.appendChild(i), i
                },
                reappearWhenCallComesBack: function (t, e, n, i) {
                    function a() {
                        var t = r.styleElements[i.mboxGoesAround];
                        t && (t.parentNode.removeChild(t), delete r.styleElements[i.mboxGoesAround])
                    }
                    var r = this;
                    I.cssQuery('script[src*="omtrdc.net"]', function (t) {
                        var e = t[0];
                        if (e) {
                            I.scriptOnLoad(e.src, e, function () {
                                I.notify("Test & Target: request complete", 1), a(), clearTimeout(i)
                            });
                            var i = setTimeout(function () {
                                I.notify("Test & Target: bailing after " + n + "ms", 1), a()
                            }, n)
                        } else I.notify("Test & Target: failed to find T&T ajax call, bailing", 1), a()
                    })
                },
                updateTargetPageParams: function (t) {
                    var e = {};
                    for (var n in t) t.hasOwnProperty(n) && (e[I.replace(n)] = I.replace(t[n]));
                    I.extend(this.targetPageParamsStore, e)
                },
                getTargetPageParams: function () {
                    return this.targetPageParamsStore
                },
                setTargetPageParamsFunction: function () {
                    t.targetPageParams = I.bind(this.getTargetPageParams, this)
                },
                parseTargetPageParamsResult: function (t) {
                    var e = t;
                    return I.isArray(t) && (t = t.join("&")), I.isString(t) && (e = I.parseQueryParams(t)), e
                }
            }), I.availableTools.tnt = v;
        var D = {
            allowLinker: function () {
                return I.hasMultipleDomains()
            },
            cookieDomain: function () {
                var e = I.settings.domainList,
                    n = I.find(e, function (e) {
                        var n = t.location.hostname;
                        return I.equalsIgnoreCase(n.slice(n.length - e.length), e)
                    }),
                    i = n ? "." + n : "auto";
                return i
            }
        };
        I.inherit(m, I.BaseTool), I.extend(m.prototype, {
            name: "GAUniversal",
            endPLPhase: function (t) {
                var e = this.settings,
                    n = e.loadOn;
                t === n && (I.notify("GAU: Initializing at " + t, 1), this.initialize(), this.flushQueue(), this.trackInitialPageView())
            },
            getTrackerName: function () {
                return this.settings.trackerSettings.name || ""
            },
            isPageCodeLoadSuppressed: function () {
                return this.settings.initTool === !1 || this._cancelToolInit === !0
            },
            initialize: function () {
                if (this.isPageCodeLoadSuppressed()) return this.initialized = !0, void I.notify("GAU: Page code not loaded (suppressed).", 1);
                var e = "ga";
                t[e] = t[e] || this.createGAObject(), t.GoogleAnalyticsObject = e, I.notify("GAU: Page code loaded.", 1), I.loadScriptOnce(this.getToolUrl());
                var n = this.settings;
                if (D.allowLinker() && n.allowLinker !== !1 ? this.createAccountForLinker() : this.createAccount(), this.executeInitCommands(), n.customInit) {
                    var i = n.customInit,
                        a = i(t[e], this.getTrackerName());
                    a === !1 && (this.suppressInitialPageView = !0)
                }
                this.initialized = !0
            },
            createGAObject: function () {
                var t = function () {
                    t.q.push(arguments)
                };
                return t.q = [], t.l = 1 * new Date, t
            },
            createAccount: function () {
                this.create()
            },
            createAccountForLinker: function () {
                var t = {};
                D.allowLinker() && (t.allowLinker = !0), this.create(t), this.call("require", "linker"), this.call("linker:autoLink", this.autoLinkDomains(), !1, !0)
            },
            create: function (t) {
                var e = this.settings.trackerSettings;
                e = I.preprocessArguments([e], location, null, this.forceLowerCase)[0], e.trackingId = I.replace(this.settings.trackerSettings.trackingId, location), e.cookieDomain || (e.cookieDomain = D.cookieDomain()), I.extend(e, t || {}), this.call("create", e)
            },
            autoLinkDomains: function () {
                var t = location.hostname;
                return I.filter(I.settings.domainList, function (e) {
                    return e !== t
                })
            },
            executeInitCommands: function () {
                var t = this.settings;
                t.initCommands && I.each(t.initCommands, function (t) {
                    var e = t.splice(2, t.length - 2);
                    t = t.concat(I.preprocessArguments(e, location, null, this.forceLowerCase)), this.call.apply(this, t)
                }, this)
            },
            trackInitialPageView: function () {
                this.suppressInitialPageView || this.isPageCodeLoadSuppressed() || this.call("send", "pageview")
            },
            call: function () {
                return "function" != typeof ga ? void I.notify("GA Universal function not found!", 4) : void (this.isCallSuppressed() || (arguments[0] = this.cmd(arguments[0]), this.log(I.toArray(arguments)), ga.apply(t, arguments)))
            },
            isCallSuppressed: function () {
                return this._cancelToolInit === !0
            },
            $missing$: function (t, e, n, i) {
                i = i || [], i = [t].concat(i), this.call.apply(this, i)
            },
            getToolUrl: function () {
                var t = this.settings,
                    e = I.isHttps();
                return t.url ? e ? t.url.https : t.url.http : (e ? "https://ssl" : "http://www") + ".google-analytics.com/analytics.js"
            },
            cmd: function (t) {
                var e = ["send", "set", "get"],
                    n = this.getTrackerName();
                return n && -1 !== I.indexOf(e, t) ? n + "." + t : t
            },
            log: function (t) {
                var e = t[0],
                    n = this.getTrackerName() || "default",
                    i = "GA Universal: sent command " + e + " to tracker " + n;
                if (t.length > 1) {
                    I.stringify(t.slice(1));
                    i += " with parameters " + I.stringify(t.slice(1))
                }
                i += ".", I.notify(i, 1)
            }
        }), I.availableTools.ga_universal = m, I.inherit(y, I.BaseTool), I.extend(y.prototype, {
            name: "GA",
            initialize: function () {
                var e = this.settings,
                    n = t._gaq,
                    i = e.initCommands || [],
                    a = e.customInit;
                if (n || (_gaq = []), this.isSuppressed()) I.notify("GA: page code not loaded(suppressed).", 1);
                else {
                    if (!n && !y.scriptLoaded) {
                        var r = I.isHttps(),
                            o = (r ? "https://ssl" : "http://www") + ".google-analytics.com/ga.js";
                        e.url && (o = r ? e.url.https : e.url.http), I.loadScript(o), y.scriptLoaded = !0, I.notify("GA: page code loaded.", 1)
                    }
                    var s = (e.domain, e.trackerName),
                        c = D.allowLinker(),
                        u = I.replace(e.account, location);
                    I.settings.domainList || [];
                    _gaq.push([this.cmd("setAccount"), u]), c && _gaq.push([this.cmd("setAllowLinker"), c]), _gaq.push([this.cmd("setDomainName"), D.cookieDomain()]), I.each(i, function (t) {
                        var e = [this.cmd(t[0])].concat(I.preprocessArguments(t.slice(1), location, null, this.forceLowerCase));
                        _gaq.push(e)
                    }, this), a && (this.suppressInitialPageView = !1 === a(_gaq, s)), e.pageName && this.$overrideInitialPageView(null, null, e.pageName)
                }
                this.initialized = !0, I.fireEvent(this.id + ".configure", _gaq, s)
            },
            isSuppressed: function () {
                return this._cancelToolInit || this.settings.initTool === !1
            },
            tracker: function () {
                return this.settings.trackerName
            },
            cmd: function (t) {
                var e = this.tracker();
                return e ? e + "._" + t : "_" + t
            },
            $overrideInitialPageView: function (t, e, n) {
                this.urlOverride = n
            },
            trackInitialPageView: function () {
                if (!this.isSuppressed() && !this.suppressInitialPageView)
                    if (this.urlOverride) {
                        var t = I.preprocessArguments([this.urlOverride], location, null, this.forceLowerCase);
                        this.$missing$("trackPageview", null, null, t)
                    } else this.$missing$("trackPageview")
            },
            endPLPhase: function (t) {
                var e = this.settings.loadOn;
                t === e && (I.notify("GA: Initializing at " + t, 1), this.initialize(), this.flushQueue(), this.trackInitialPageView())
            },
            call: function (t, e, n, i) {
                if (!this._cancelToolInit) {
                    var a = (this.settings, this.tracker()),
                        r = this.cmd(t),
                        i = i ? [r].concat(i) : [r];
                    _gaq.push(i), a ? I.notify("GA: sent command " + t + " to tracker " + a + (i.length > 1 ? " with parameters [" + i.slice(1).join(", ") + "]" : "") + ".", 1) : I.notify("GA: sent command " + t + (i.length > 1 ? " with parameters [" + i.slice(1).join(", ") + "]" : "") + ".", 1)
                }
            },
            $missing$: function (t, e, n, i) {
                this.call(t, e, n, i)
            },
            $postTransaction: function (e, n, i) {
                var a = I.data.customVars.transaction = t[i];
                this.call("addTrans", e, n, [a.orderID, a.affiliation, a.total, a.tax, a.shipping, a.city, a.state, a.country]), I.each(a.items, function (t) {
                    this.call("addItem", e, n, [t.orderID, t.sku, t.product, t.category, t.unitPrice, t.quantity])
                }, this), this.call("trackTrans", e, n)
            },
            delayLink: function (t, e) {
                var n = this;
                if (D.allowLinker() && t.hostname.match(this.settings.linkerDomains) && !I.isSubdomainOf(t.hostname, location.hostname)) {
                    I.preventDefault(e);
                    var i = I.settings.linkDelay || 100;
                    setTimeout(function () {
                        n.call("link", t, e, [t.href])
                    }, i)
                }
            },
            popupLink: function (e, n) {
                if (t._gat) {
                    I.preventDefault(n);
                    var i = this.settings.account,
                        a = t._gat._createTracker(i),
                        r = a._getLinkerUrl(e.href);
                    t.open(r)
                }
            },
            $link: function (t, e) {
                "_blank" === t.getAttribute("target") ? this.popupLink(t, e) : this.delayLink(t, e)
            },
            $trackEvent: function (t, e) {
                var n = Array.prototype.slice.call(arguments, 2);
                if (n.length >= 4 && null != n[3]) {
                    var i = parseInt(n[3], 10);
                    I.isNaN(i) && (i = 1), n[3] = i
                }
                this.call("trackEvent", t, e, n)
            }
        }), I.availableTools.ga = y, I.inherit(b, I.BaseTool), I.extend(b.prototype, {
            name: "SC",
            endPLPhase: function (t) {
                var e = this.settings.loadOn;
                t === e && this.initialize(t)
            },
            initialize: function (e) {
                if (!this._cancelToolInit)
                    if (this.settings.initVars = this.substituteVariables(this.settings.initVars, {
                        type: e
                    }), this.settings.initTool !== !1) {
                        var n = this.settings.sCodeURL || I.basePath() + "s_code.js";
                        "object" == typeof n && (n = "https:" === t.location.protocol ? n.https : n.http), n.match(/^https?:/) || (n = I.basePath() + n), this.settings.initVars && this.$setVars(null, null, this.settings.initVars), I.loadScript(n, I.bind(this.onSCodeLoaded, this)), this.initializing = !0
                    } else this.initializing = !0, this.pollForSC()
            },
            getS: function (e, n) {
                var i = n && n.hostname || t.location.hostname,
                    a = this.concatWithToolVarBindings(n && n.setVars || this.varBindings),
                    r = n && n.addEvent || this.events,
                    o = this.getAccount(i),
                    s = t.s_gi;
                if (!s) return null;
                if (this.isValidSCInstance(e) || (e = null), !o && !e) return I.notify("Adobe Analytics: tracker not initialized because account was not found", 1), null;
                var e = e || s(o),
                    c = "D" + I.appVersion;
                "undefined" != typeof e.tagContainerMarker ? e.tagContainerMarker = c : "string" == typeof e.version && e.version.substring(e.version.length - 5) !== "-" + c && (e.version += "-" + c), e.sa && this.settings.skipSetAccount !== !0 && this.settings.initTool !== !1 && e.sa(this.settings.account), this.applyVarBindingsOnTracker(e, a), r.length > 0 && (e.events = r.join(","));
                var u = I.getVisitorId();
                return u && (e.visitor = I.getVisitorId()), e
            },
            onSCodeLoaded: function (t) {
                this.initialized = !0, this.initializing = !1;
                var e = ["Adobe Analytics: loaded", t ? " (manual)" : "", "."];
                I.notify(e.join(""), 1), I.fireEvent(this.id + ".load", this.getS()), t || (this.flushQueueExceptTrackLink(), this.sendBeacon()), this.flushQueue()
            },
            getAccount: function (e) {
                return t.s_account ? t.s_account : e && this.settings.accountByHost ? this.settings.accountByHost[e] || this.settings.account : this.settings.account
            },
            getTrackingServer: function () {
                var e = this,
                    n = e.getS();
                if (n) {
                    if (n.ssl && n.trackingServerSecure) return n.trackingServerSecure;
                    if (n.trackingServer) return n.trackingServer
                }
                var i = e.getAccount(t.location.hostname);
                if (!i) return null;
                var a, r, o, s = "",
                    c = n && n.dc;
                return a = i, r = a.indexOf(","), r >= 0 && (a = a.gb(0, r)), a = a.replace(/[^A-Za-z0-9]/g, ""), s || (s = "2o7.net"), c = c ? ("" + c).toLowerCase() : "d1", "2o7.net" == s && ("d1" == c ? c = "112" : "d2" == c && (c = "122"), o = ""), r = a + "." + c + "." + o + s
            },
            sendBeacon: function () {
                var e = this.getS(t[this.settings.renameS || "s"]);
                return e ? this.settings.customInit && this.settings.customInit(e) === !1 ? void I.notify("Adobe Analytics: custom init suppressed beacon", 1) : (this.settings.executeCustomPageCodeFirst && this.applyVarBindingsOnTracker(e, this.varBindings), this.executeCustomSetupFuns(e), e.t(), this.clearVarBindings(), this.clearCustomSetup(), void I.notify("Adobe Analytics: tracked page view", 1)) : void I.notify("Adobe Analytics: page code not loaded", 1)
            },
            pollForSC: function () {
                I.poll(I.bind(function () {
                    return "function" == typeof t.s_gi ? (this.onSCodeLoaded(!0), !0) : void 0
                }, this))
            },
            flushQueueExceptTrackLink: function () {
                if (this.pending) {
                    for (var t = [], e = 0; e < this.pending.length; e++) {
                        var n = this.pending[e],
                            i = n[0];
                        "trackLink" === i.command ? t.push(n) : this.triggerCommand.apply(this, n)
                    }
                    this.pending = t
                }
            },
            isQueueAvailable: function () {
                return !this.initialized
            },
            substituteVariables: function (t, e) {
                var n = {};
                for (var i in t)
                    if (t.hasOwnProperty(i)) {
                        var a = t[i];
                        n[i] = I.replace(a, location, e)
                    }
                return n
            },
            $setVars: function (t, e, n) {
                for (var i in n)
                    if (n.hasOwnProperty(i)) {
                        var a = n[i];
                        "function" == typeof a && (a = a()), this.varBindings[i] = a
                    }
                I.notify("Adobe Analytics: set variables.", 2)
            },
            $customSetup: function (t, e, n) {
                this.customSetupFuns.push(function (i) {
                    n.call(t, e, i)
                })
            },
            isValidSCInstance: function (t) {
                return !!t && "function" == typeof t.t && "function" == typeof t.tl
            },
            concatWithToolVarBindings: function (t) {
                var e = this.settings.initVars || {};
                return I.map(["trackingServer", "trackingServerSecure"], function (n) {
                    e[n] && !t[n] && (t[n] = e[n])
                }), t
            },
            applyVarBindingsOnTracker: function (t, e) {
                for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n])
            },
            clearVarBindings: function () {
                this.varBindings = {}
            },
            clearCustomSetup: function () {
                this.customSetupFuns = []
            },
            executeCustomSetupFuns: function (e) {
                I.each(this.customSetupFuns, function (n) {
                    n.call(t, e)
                })
            },
            $trackLink: function (t, e, n) {
                n = n || {};
                var i = n.type,
                    a = n.linkName;
                !a && t && t.nodeName && "a" === t.nodeName.toLowerCase() && (a = t.innerHTML), a || (a = "link clicked");
                var r = n && n.setVars,
                    o = n && n.addEvent || [],
                    s = this.getS(null, {
                        setVars: r,
                        addEvent: o
                    });
                if (!s) return void I.notify("Adobe Analytics: page code not loaded", 1);
                var c = s.linkTrackVars,
                    u = s.linkTrackEvents,
                    l = this.definedVarNames(r);
                n && n.customSetup && n.customSetup.call(t, e, s), o.length > 0 && l.push("events"), s.products && l.push("products"), l = this.mergeTrackLinkVars(s.linkTrackVars, l), o = this.mergeTrackLinkVars(s.linkTrackEvents, o), s.linkTrackVars = this.getCustomLinkVarsList(l);
                var d = I.map(o, function (t) {
                    return t.split(":")[0]
                });
                s.linkTrackEvents = this.getCustomLinkVarsList(d), s.tl(!0, i || "o", a), I.notify(["Adobe Analytics: tracked link ", "using: linkTrackVars=", I.stringify(s.linkTrackVars), "; linkTrackEvents=", I.stringify(s.linkTrackEvents)].join(""), 1), s.linkTrackVars = c, s.linkTrackEvents = u
            },
            mergeTrackLinkVars: function (t, e) {
                return t && (e = t.split(",").concat(e)), e
            },
            getCustomLinkVarsList: function (t) {
                var e = I.indexOf(t, "None");
                return e > -1 && t.length > 1 && t.splice(e, 1), t.join(",")
            },
            definedVarNames: function (t) {
                t = t || this.varBindings;
                var e = [];
                for (var n in t) t.hasOwnProperty(n) && /^(eVar[0-9]+)|(prop[0-9]+)|(hier[0-9]+)|campaign|purchaseID|channel|server|state|zip|pageType$/.test(n) && e.push(n);
                return e
            },
            $trackPageView: function (t, e, n) {
                var i = n && n.setVars,
                    a = n && n.addEvent || [],
                    r = this.getS(null, {
                        setVars: i,
                        addEvent: a
                    });
                return r ? (r.linkTrackVars = "", r.linkTrackEvents = "", this.executeCustomSetupFuns(r), n && n.customSetup && n.customSetup.call(t, e, r), r.t(), this.clearVarBindings(), this.clearCustomSetup(), void I.notify("Adobe Analytics: tracked page view", 1)) : void I.notify("Adobe Analytics: page code not loaded", 1)
            },
            $postTransaction: function (e, n, i) {
                var a = I.data.transaction = t[i],
                    r = this.varBindings,
                    o = this.settings.fieldVarMapping;
                if (I.each(a.items, function (t) {
                        this.products.push(t)
                }, this), r.products = I.map(this.products, function (t) {
                        var e = [];
                        if (o && o.item)
                            for (var n in o.item)
                                if (o.item.hasOwnProperty(n)) {
                                    var i = o.item[n];
                                    e.push(i + "=" + t[n]), "event" === i.substring(0, 5) && this.events.push(i)
                }
                        var a = ["", t.product, t.quantity, t.unitPrice * t.quantity];
                        return e.length > 0 && a.push(e.join("|")), a.join(";")
                }, this).join(","), o && o.transaction) {
                    var s = [];
                    for (var c in o.transaction)
                        if (o.transaction.hasOwnProperty(c)) {
                            var i = o.transaction[c];
                            s.push(i + "=" + a[c]), "event" === i.substring(0, 5) && this.events.push(i)
                        }
                    r.products.length > 0 && (r.products += ","), r.products += ";;;;" + s.join("|")
                }
            },
            $addEvent: function (t, e) {
                for (var n = 2, i = arguments.length; i > n; n++) this.events.push(arguments[n])
            },
            $addProduct: function (t, e) {
                for (var n = 2, i = arguments.length; i > n; n++) this.products.push(arguments[n])
            }
        }), I.availableTools.sc = b, I.inherit(k, I.BaseTool), I.extend(k.prototype, {
            name: "Default",
            $loadIframe: function (e, n, i) {
                var a = i.pages,
                    r = i.loadOn,
                    o = I.bind(function () {
                        I.each(a, function (t) {
                            this.loadIframe(e, n, t)
                        }, this)
                    }, this);
                r || o(), "domready" === r && I.domReady(o), "load" === r && I.addEventHandler(t, "load", o)
            },
            loadIframe: function (t, n, i) {
                var a = e.createElement("iframe");
                a.style.display = "none";
                var r = I.data.host,
                    o = i.data,
                    s = this.scriptURL(i.src),
                    c = I.searchVariables(o, t, n);
                r && (s = I.basePath() + s), s += c, a.src = s;
                var u = e.getElementsByTagName("body")[0];
                u ? u.appendChild(a) : I.domReady(function () {
                    e.getElementsByTagName("body")[0].appendChild(a)
                })
            },
            scriptURL: function (t) {
                var e = I.settings.scriptDir || "";
                return e + t
            },
            $loadScript: function (e, n, i) {
                var a = i.scripts,
                    r = i.sequential,
                    o = i.loadOn,
                    s = I.bind(function () {
                        r ? this.loadScripts(e, n, a) : I.each(a, function (t) {
                            this.loadScripts(e, n, [t])
                        }, this)
                    }, this);
                o ? "domready" === o ? I.domReady(s) : "load" === o && I.addEventHandler(t, "load", s) : s()
            },
            loadScripts: function (t, e, n) {
                function i() {
                    if (r.length > 0 && a) {
                        var c = r.shift();
                        c.call(t, e, o)
                    }
                    var u = n.shift();
                    if (u) {
                        var l = I.data.host,
                            d = s.scriptURL(u.src);
                        l && (d = I.basePath() + d), a = u, I.loadScript(d, i)
                    }
                }
                try {
                    var a, n = n.slice(0),
                        r = this.asyncScriptCallbackQueue,
                        o = e.target || e.srcElement,
                        s = this
                } catch (c) {
                    console.error("scripts is", I.stringify(n))
                }
                i()
            },
            $loadBlockingScript: function (t, e, n) {
                var i = n.scripts,
                    a = (n.loadOn, I.bind(function () {
                        I.each(i, function (n) {
                            this.loadBlockingScript(t, e, n)
                        }, this)
                    }, this));
                a()
            },
            loadBlockingScript: function (t, e, n) {
                var i = this.scriptURL(n.src),
                    a = I.data.host,
                    r = e.target || e.srcElement;
                a && (i = I.basePath() + i), this.argsForBlockingScripts.push([t, e, r]), I.loadScriptSync(i)
            },
            pushAsyncScript: function (t) {
                this.asyncScriptCallbackQueue.push(t)
            },
            pushBlockingScript: function (t) {
                var e = this.argsForBlockingScripts.shift(),
                    n = e[0];
                t.apply(n, e.slice(1))
            },
            $writeHTML: I.escapeHtmlParams(function (t, n) {
                if (I.domReadyFired || !e.write) return void I.notify("Command writeHTML failed. You should try appending HTML using the async option.", 1);
                if ("pagebottom" !== n.type && "pagetop" !== n.type) return void I.notify("You can only use writeHTML on the `pagetop` and `pagebottom` events.", 1);
                for (var i = 2, a = arguments.length; a > i; i++) {
                    var r = arguments[i].html;
                    r = I.replace(r, t, n), e.write(r)
                }
            }),
            linkNeedsDelayActivate: function (e, n) {
                n = n || t;
                var i = e.tagName,
                    a = e.getAttribute("target"),
                    r = e.getAttribute("href");
                return i && "a" !== i.toLowerCase() ? !1 : r ? a ? "_blank" === a ? !1 : "_top" === a ? n.top === n : "_parent" === a ? !1 : "_self" === a ? !0 : n.name ? a === n.name : !0 : !0 : !1
            },
            $delayActivateLink: function (t, e) {
                if (this.linkNeedsDelayActivate(t)) {
                    I.preventDefault(e);
                    var n = I.settings.linkDelay || 100;
                    setTimeout(function () {
                        I.setLocation(t.href)
                    }, n)
                }
            },
            isQueueable: function (t) {
                return "writeHTML" !== t.command
            }
        }), I.availableTools["default"] = k, I.inherit(S, I.BaseTool), I.extend(S.prototype, {
            name: "Nielsen",
            endPLPhase: function (t) {
                switch (t) {
                    case "pagetop":
                        this.initialize();
                        break;
                    case "pagebottom":
                        this.enableTracking && (this.queueCommand({
                            command: "sendFirstBeacon",
                            arguments: []
                        }), this.flushQueueWhenReady())
                }
            },
            defineListeners: function () {
                this.onTabFocus = I.bind(function () {
                    this.notify("Tab visible, sending view beacon when ready", 1), this.tabEverVisible = !0, this.flushQueueWhenReady()
                }, this), this.onPageLeave = I.bind(function () {
                    this.notify("isHuman? : " + this.isHuman(), 1), this.isHuman() && this.sendDurationBeacon()
                }, this), this.onHumanDetectionChange = I.bind(function (t) {
                    this == t.target.target && (this.human = t.target.isHuman)
                }, this)
            },
            initialize: function () {
                this.initializeTracking(), this.initializeDataProviders(), this.initializeNonHumanDetection(), this.tabEverVisible = I.visibility.isVisible(), this.tabEverVisible ? this.notify("Tab visible, sending view beacon when ready", 1) : I.bindEventOnce("tabfocus", this.onTabFocus), this.initialized = !0
            },
            initializeTracking: function () {
                this.initialized || (this.notify("Initializing tracking", 1), this.addRemovePageLeaveEvent(this.enableTracking), this.addRemoveHumanDetectionChangeEvent(this.enableTracking), this.initialized = !0)
            },
            initializeDataProviders: function () {
                var t, e = this.getAnalyticsTool();
                this.dataProvider.register(new S.DataProvider.VisitorID(I.getVisitorId())), e ? (t = new S.DataProvider.Generic("rsid", function () {
                    return e.settings.account
                }), this.dataProvider.register(t)) : this.notify("Missing integration with Analytics: rsid will not be sent.")
            },
            initializeNonHumanDetection: function () {
                I.nonhumandetection ? (I.nonhumandetection.init(), this.setEnableNonHumanDetection(0 == this.settings.enableNonHumanDetection ? !1 : !0), this.settings.nonHumanDetectionDelay > 0 && this.setNonHumanDetectionDelay(1e3 * parseInt(this.settings.nonHumanDetectionDelay))) : this.notify("NHDM is not available.")
            },
            getAnalyticsTool: function () {
                return this.settings.integratesWith ? I.tools[this.settings.integratesWith] : void 0
            },
            flushQueueWhenReady: function () {
                this.enableTracking && this.tabEverVisible && I.poll(I.bind(function () {
                    return this.isReadyToTrack() ? (this.flushQueue(), !0) : void 0
                }, this), 100, 20)
            },
            isReadyToTrack: function () {
                return this.tabEverVisible && this.dataProvider.isReady()
            },
            $setVars: function (t, e, n) {
                for (var i in n) {
                    var a = n[i];
                    "function" == typeof a && (a = a()), this.settings[i] = a
                }
                this.notify("Set variables done", 2), this.prepareContextData()
            },
            $setEnableTracking: function (t, e, n) {
                this.notify("Will" + (n ? "" : " not") + " track time on page", 1), this.enableTracking != n && (this.addRemovePageLeaveEvent(n), this.addRemoveHumanDetectionChangeEvent(n), this.enableTracking = n)
            },
            $sendFirstBeacon: function (t, e, n) {
                this.sendViewBeacon()
            },
            setEnableNonHumanDetection: function (t) {
                t ? I.nonhumandetection.register(this) : I.nonhumandetection.unregister(this)
            },
            setNonHumanDetectionDelay: function (t) {
                I.nonhumandetection.register(this, t)
            },
            addRemovePageLeaveEvent: function (t) {
                this.notify((t ? "Attach onto" : "Detach from") + " page leave event", 1);
                var e = 0 == t ? "unbindEvent" : "bindEvent";
                I[e]("leave", this.onPageLeave)
            },
            addRemoveHumanDetectionChangeEvent: function (t) {
                this.notify((t ? "Attach onto" : "Detach from") + " human detection change event", 1);
                var e = 0 == t ? "unbindEvent" : "bindEvent";
                I[e]("humandetection.change", this.onHumanDetectionChange)
            },
            sendViewBeacon: function () {
                this.notify("Tracked page view.", 1), this.sendBeaconWith()
            },
            sendDurationBeacon: function () {
                if (!I.timetracking || "function" != typeof I.timetracking.timeOnPage || null == I.timetracking.timeOnPage()) return void this.notify("Could not track close due missing time on page", 5);
                this.notify("Tracked close", 1), this.sendBeaconWith({
                    timeOnPage: Math.round(I.timetracking.timeOnPage() / 1e3),
                    duration: "D",
                    timer: "timer"
                });
                var t, e = "";
                for (t = 0; t < this.magicConst; t++) e += "0"
            },
            sendBeaconWith: function (t) {
                this.enableTracking && this[this.beaconMethod].call(this, this.prepareUrl(t))
            },
            plainBeacon: function (t) {
                var e = new Image;
                e.src = t, e.width = 1, e.height = 1, e.alt = ""
            },
            navigatorSendBeacon: function (t) {
                navigator.sendBeacon(t)
            },
            prepareUrl: function (t) {
                var e = this.settings;
                return I.extend(e, this.dataProvider.provide()), I.extend(e, t), this.preparePrefix(this.settings.collectionServer) + this.adapt.convertToURI(this.adapt.toNielsen(this.substituteVariables(e)))
            },
            preparePrefix: function (t) {
                return "//" + encodeURIComponent(t) + ".imrworldwide.com/cgi-bin/gn?"
            },
            substituteVariables: function (t) {
                var e = {};
                for (var n in t) t.hasOwnProperty(n) && (e[n] = I.replace(t[n]));
                return e
            },
            prepareContextData: function () {
                if (!this.getAnalyticsTool()) return void this.notify("Adobe Analytics missing.");
                var t = this.settings;
                t.sdkVersion = _satellite.publishDate, this.getAnalyticsTool().$setVars(null, null, {
                    contextData: this.adapt.toAnalytics(this.substituteVariables(t))
                })
            },
            isHuman: function () {
                return this.human
            },
            onTabFocus: function () { },
            onPageLeave: function () { },
            onHumanDetectionChange: function () { },
            notify: function (t, e) {
                I.notify(this.logPrefix + t, e)
            },
            beaconMethod: "plainBeacon",
            adapt: null,
            enableTracking: !1,
            logPrefix: "Nielsen: ",
            tabEverVisible: !1,
            human: !0,
            magicConst: 2e6
        }), S.DataProvider = {}, S.DataProvider.Generic = function (t, e) {
            this.key = t, this.valueFn = e
        }, I.extend(S.DataProvider.Generic.prototype, {
            isReady: function () {
                return !0
            },
            getValue: function () {
                return this.valueFn()
            },
            provide: function () {
                this.isReady() || S.prototype.notify("Not yet ready to provide value for: " + this.key, 5);
                var t = {};
                return t[this.key] = this.getValue(), t
            }
        }), S.DataProvider.VisitorID = function (t, e, n) {
            this.key = e || "uuid", this.visitorInstance = t, this.visitorInstance && (this.visitorId = t.getMarketingCloudVisitorID([this, this._visitorIdCallback])), this.fallbackProvider = n || new S.UUID
        }, I.inherit(S.DataProvider.VisitorID, S.DataProvider.Generic), I.extend(S.DataProvider.VisitorID.prototype, {
            isReady: function () {
                return null === this.visitorInstance ? !0 : !!this.visitorId
            },
            getValue: function () {
                return this.visitorId || this.fallbackProvider.get()
            },
            _visitorIdCallback: function (t) {
                this.visitorId = t
            }
        }), S.DataProvider.Aggregate = function () {
            this.providers = [];
            for (var t = 0; t < arguments.length; t++) this.register(arguments[t])
        }, I.extend(S.DataProvider.Aggregate.prototype, {
            register: function (t) {
                this.providers.push(t)
            },
            isReady: function () {
                return I.every(this.providers, function (t) {
                    return t.isReady()
                })
            },
            provide: function () {
                var t = {};
                return I.each(this.providers, function (e) {
                    I.extend(t, e.provide())
                }), t
            }
        }), S.UUID = function () { }, I.extend(S.UUID.prototype, {
            generate: function () {
                return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (t) {
                    var e = 16 * Math.random() | 0,
                        n = "x" == t ? e : 3 & e | 8;
                    return n.toString(16)
                })
            },
            get: function () {
                var t = I.readCookie(this.key("uuid"));
                return t ? t : (t = this.generate(), I.setCookie(this.key("uuid"), t), t)
            },
            key: function (t) {
                return "_dtm_nielsen_" + t
            }
        }), S.DataAdapters = function () { }, I.extend(S.DataAdapters.prototype, {
            toNielsen: function (t) {
                var e = (new Date).getTime(),
                    i = {
                        c6: "vc,",
                        c13: "asid,",
                        c15: "apn,",
                        c27: "cln,",
                        c32: "segA,",
                        c33: "segB,",
                        c34: "segC,",
                        c35: "adrsid,",
                        c29: "plid,",
                        c30: "bldv,",
                        c40: "adbid,"
                    },
                    a = {
                        ci: t.clientId,
                        c6: t.vcid,
                        c13: t.appId,
                        c15: t.appName,
                        prv: 1,
                        forward: 0,
                        ad: 0,
                        cr: t.duration || "V",
                        rt: "text",
                        st: "dcr",
                        prd: "dcr",
                        r: e,
                        at: t.timer || "view",
                        c16: t.sdkVersion,
                        c27: t.timeOnPage || 0,
                        c40: t.uuid,
                        c35: t.rsid,
                        ti: e,
                        sup: 0,
                        c32: t.segmentA,
                        c33: t.segmentB,
                        c34: t.segmentC,
                        asn: t.assetName,
                        c29: t.playerID,
                        c30: t.buildVersion
                    };
                for (key in a)
                    if (a[key] !== n && null != a[key] && a[key] !== n && null != a && "" != a) {
                        var r = encodeURIComponent(a[key]);
                        i.hasOwnProperty(key) && r && (r = i[key] + r), a[key] = r
                    }
                return this.filterObject(a)
            },
            toAnalytics: function (t) {
                return this.filterObject({
                    "a.nielsen.clientid": t.clientId,
                    "a.nielsen.vcid": t.vcid,
                    "a.nielsen.appid": t.appId,
                    "a.nielsen.appname": t.appName,
                    "a.nielsen.accmethod": "0",
                    "a.nielsen.ctype": "text",
                    "a.nielsen.sega": t.segmentA,
                    "a.nielsen.segb": t.segmentB,
                    "a.nielsen.segc": t.segmentC,
                    "a.nielsen.asset": t.assetName
                })
            },
            convertToURI: function (t) {
                if (I.isObject(t) === !1) return "";
                var e = [];
                for (var n in t) t.hasOwnProperty(n) && e.push(n + "=" + t[n]);
                return e.join("&")
            },
            filterObject: function (t) {
                for (var e in t) !t.hasOwnProperty(e) || null != t[e] && t[e] !== n || delete t[e];
                return t
            }
        }), I.availableTools.nielsen = S, _satellite.init({
            tools: {
                "7a6e5ffce8293a2f9ddf48fa88932347": {
                    engine: "sc",
                    loadOn: "pagebottom",
                    euCookie: !1,
                    sCodeURL: "omniture-scode-2.0.6.js",
                    renameS: "s_omntr",
                    initVars: {
                        trackInlineStats: !0,
                        trackDownloadLinks: !0,
                        linkDownloadFileTypes: "avi,css,csv,doc,docx,eps,exe,fla,gif,jpg,js,m4v,mov,mp3,mpg,pdf,png,pps,ppt,pptx,rar,svg,tab,txt,vsd,vxd,wav,wma,wmv,xls,xlsx,xml,zip",
                        trackExternalLinks: !0,
                        linkInternalFilters: "hrbl.net,javascript:,mailto:,myherbalife,tel:",
                        linkLeaveQueryString: !1,
                        dynamicVariablePrefix: "D="
                    },
                    skipSetAccount: !0
                },
                "915a71a14948aad5318b7120c6657500ebfe57b2": {
                    engine: "tnt",
                    mboxURL: "mbox-2.0.4.js",
                    loadSync: !0,
                    pageParams: {
                        AFTitle: "%AFTitle%",
                        AFEncId: "%AFEncId%",
                        AFRoles: "%AFRoles%",
                        AFIsPresidentsTeam: "%AFIsPresidentsTeam%",
                        AFIsChairmanClub: "%AFIsChairmanClub%",
                        AFIsTabTeam: "%AFIsTabTeam%",
                        AFSSOProfileId: "%AFSSOProfileId%",
                        AFSearchTerms: "%AFSearchTerms%",
                        AFCategoryId: "%AFCategoryId%",
                        AFProductId: "%AFProductId%",
                        AFProductPrice: "%AFProductPrice%",
                        AFApfDue: "%AFApfDue%"
                    }
                },
                c56732f673c39bcf7bd835d7fe94e877a94bd7aa: {
                    engine: "visitor_id",
                    loadOn: "pagetop",
                    name: "VisitorID",
                    mcOrgId: "E1DC1042548EFE0F0A4C98A4@AdobeOrg",
                    autoRequest: !1,
                    initVars: {
                        trackingServer: "metrics.herbalife.com",
                        trackingServerSecure: "smetrics.herbalife.com",
                        marketingCloudServer: "metrics.herbalife.com",
                        marketingCloudServerSecure: "smetrics.herbalife.com",
                        loadTimeout: "1200"
                    },
                    customerIDs: {
                        userid: {
                            id: "%AFDSIDProfileId%",
                            authState: "AUTHENTICATED"
                        },
                        puuid: {
                            id: "%AFSSOProfileId%",
                            authState: "AUTHENTICATED"
                        }
                    }
                }
            },
            pageLoadRules: [{
                name: "Recommendations - Rules",
                trigger: [{
                    command: "loadScript",
                    arguments: [{
                        sequential: !1,
                        scripts: [{
                            src: "satellite-1.0.2.js"
                        }]
                    }]
                }],
                event: "pagebottom"
            }],
            rules: [],
            directCallRules: [],
            settings: {
                trackInternalLinks: !0,
                libraryName: "satelliteLib-1.0.8",
                isStaging: !1,
                allowGATTcalls: !1,
                downloadExtensions: /\.(?:doc|docx|eps|jpg|png|svg|xls|ppt|pptx|pdf|xlsx|tab|csv|zip|txt|vsd|vxd|xml|js|css|rar|exe|wma|mov|avi|wmv|mp3|wav|m4v)($|\&|\?)/i,
                notifications: !1,
                utilVisible: !1,
                domainList: ["myherbalife.com"],
                scriptDir: "",
                tagTimeout: 3e3
            },
            data: {
                URI: e.location.pathname + e.location.search,
                browser: {},
                cartItems: [],
                revenue: "",
                host: {
                    http: t.location.host + "/Scripts/dtm",
                    https: t.location.host + "/Scripts/dtm"
                }
            },
            dataElements: {
                AFApfDue: {
                    jsVariable: "_AnalyticsFacts_.ApfDue",
                    storeLength: "pageview"
                },
                AFCategoryId: {
                    jsVariable: "_AnalyticsFacts_.CategoryId",
                    storeLength: "pageview"
                },
                AFDSIDProfileId: {
                    jsVariable: "_AnalyticsFacts_.Id",
                    storeLength: "pageview"
                },
                AFEncId: {
                    jsVariable: "_AnalyticsFacts_.EncId",
                    storeLength: "pageview"
                },
                AFIsChairmanClub: {
                    jsVariable: "_AnalyticsFacts_.IsChairmanClub",
                    storeLength: "pageview"
                },
                AFIsPresidentsTeam: {
                    jsVariable: "_AnalyticsFacts_.IsPresidentsTeam",
                    storeLength: "pageview"
                },
                AFIsTabTeam: {
                    jsVariable: "_AnalyticsFacts_.IsTabTeam",
                    storeLength: "pageview"
                },
                AFProductId: {
                    jsVariable: "_AnalyticsFacts_.ProductId",
                    storeLength: "pageview"
                },
                AFProductPrice: {
                    jsVariable: "_AnalyticsFacts_.ProductPrice",
                    storeLength: "pageview"
                },
                AFRoles: {
                    jsVariable: "_AnalyticsFacts_.Roles",
                    storeLength: "pageview"
                },
                AFSearchTerms: {
                    jsVariable: "_AnalyticsFacts_.SearchTerms",
                    storeLength: "pageview"
                },
                AFSSOProfileId: {
                    jsVariable: "_AnalyticsFacts_.SSOProfileId",
                    storeLength: "pageview"
                },
                AFTitle: {
                    jsVariable: "_AnalyticsFacts_.Title",
                    storeLength: "pageview"
                }
            },
            appVersion: "7QN",
            buildDate: "2017-04-19 21:34:01 UTC",
            publishDate: "2017-04-19 21:34:01 UTC"
        })
    }(window, document);