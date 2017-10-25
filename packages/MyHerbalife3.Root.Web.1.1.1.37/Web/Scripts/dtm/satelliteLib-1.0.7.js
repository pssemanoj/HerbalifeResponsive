/*
 ============== DO NOT ALTER ANYTHING BELOW THIS LINE ! ============

 Adobe Visitor API for JavaScript version: 1.10.0
 Copyright 1996-2015 Adobe, Inc. All Rights Reserved
 More info available at http://www.omniture.com
*/
function Visitor(t, e) {
    function n(t) {
        function e(t, e, n) {
            return n = n ? n += "|" : n, n + (t + "=" + encodeURIComponent(e))
        }
        for (var n = "", i = 0, a = t.length; a > i; i++) {
            var r = t[i],
                s = r[0],
                r = r[1];
            r != o && r !== I && (n = e(s, r, n))
        }
        return function (t) {
            var e = (new Date).getTime(),
                t = t ? t += "|" : t;
            return t + ("TS=" + e)
        }(n)
    }
    if (!t) throw "Visitor requires Adobe Marketing Cloud Org ID";
    var i = this;
    i.version = "1.10.0";
    var a = window,
        r = a.Visitor;
    r.version = i.version, a.s_c_in || (a.s_c_il = [], a.s_c_in = 0), i._c = "Visitor", i._il = a.s_c_il, i._in = a.s_c_in, i._il[i._in] = i, a.s_c_in++, i.ja = {
        Fa: []
    };
    var s = a.document,
        o = r.Cb;
    o || (o = null);
    var c = r.Db;
    c || (c = void 0);
    var u = r.Oa;
    u || (u = !0);
    var l = r.Ma;
    l || (l = !1), i.fa = function (t) {
        var e, n, i = 0;
        if (t)
            for (e = 0; e < t.length; e++) n = t.charCodeAt(e), i = (i << 5) - i + n, i &= i;
        return i
    }, i.s = function (t, e) {
        var n, i, a = "0123456789",
            r = "",
            s = "",
            o = 8,
            c = 10,
            l = 10;
        if (e === h && (L.isClientSideMarketingCloudVisitorID = u), 1 == t) {
            for (a += "ABCDEF", n = 0; 16 > n; n++) i = Math.floor(Math.random() * o), r += a.substring(i, i + 1), i = Math.floor(Math.random() * o), s += a.substring(i, i + 1), o = 16;
            return r + "-" + s
        }
        for (n = 0; 19 > n; n++) i = Math.floor(Math.random() * c), r += a.substring(i, i + 1), 0 == n && 9 == i ? c = 3 : (1 == n || 2 == n) && 10 != c && 2 > i ? c = 10 : n > 2 && (c = 10), i = Math.floor(Math.random() * l), s += a.substring(i, i + 1), 0 == n && 9 == i ? l = 3 : (1 == n || 2 == n) && 10 != l && 2 > i ? l = 10 : n > 2 && (l = 10);
        return r + s
    }, i.Ra = function () {
        var t;
        if (!t && a.location && (t = a.location.hostname), t)
            if (/^[0-9.]+$/.test(t)) t = "";
            else {
                var e = t.split("."),
                    n = e.length - 1,
                    i = n - 1;
                if (n > 1 && 2 >= e[n].length && (2 == e[n - 1].length || 0 > ",ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,ax,az,ba,bb,be,bf,bg,bh,bi,bj,bm,bo,br,bs,bt,bv,bw,by,bz,ca,cc,cd,cf,cg,ch,ci,cl,cm,cn,co,cr,cu,cv,cw,cx,cz,de,dj,dk,dm,do,dz,ec,ee,eg,es,et,eu,fi,fm,fo,fr,ga,gb,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gw,gy,hk,hm,hn,hr,ht,hu,id,ie,im,in,io,iq,ir,is,it,je,jo,jp,kg,ki,km,kn,kp,kr,ky,kz,la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly,ma,mc,md,me,mg,mh,mk,ml,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,na,nc,ne,nf,ng,nl,no,nr,nu,nz,om,pa,pe,pf,ph,pk,pl,pm,pn,pr,ps,pt,pw,py,qa,re,ro,rs,ru,rw,sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sx,sy,sz,tc,td,tf,tg,th,tj,tk,tl,tm,tn,to,tp,tr,tt,tv,tw,tz,ua,ug,uk,us,uy,uz,va,vc,ve,vg,vi,vn,vu,wf,ws,yt,".indexOf("," + e[n] + ",")) && i--, i > 0)
                    for (t = ""; n >= i;) t = e[n] + (t ? "." : "") + t, n--
            }
        return t
    }, i.cookieRead = function (t) {
        var t = encodeURIComponent(t),
            e = (";" + s.cookie).split(" ").join(";"),
            n = e.indexOf(";" + t + "="),
            i = 0 > n ? n : e.indexOf(";", n + 1);
        return 0 > n ? "" : decodeURIComponent(e.substring(n + 2 + t.length, 0 > i ? e.length : i))
    }, i.cookieWrite = function (t, e, n) {
        var a, r = i.cookieLifetime,
            e = "" + e,
            r = r ? ("" + r).toUpperCase() : "";
        return n && "SESSION" != r && "NONE" != r ? (a = "" != e ? parseInt(r ? r : 0, 10) : -60) ? (n = new Date, n.setTime(n.getTime() + 1e3 * a)) : 1 == n && (n = new Date, a = n.getYear(), n.setYear(a + 2 + (1900 > a ? 1900 : 0))) : n = 0, t && "NONE" != r ? (s.cookie = encodeURIComponent(t) + "=" + encodeURIComponent(e) + "; path=/;" + (n ? " expires=" + n.toGMTString() + ";" : "") + (i.cookieDomain ? " domain=" + i.cookieDomain + ";" : ""), i.cookieRead(t) == e) : 0
    }, i.h = o, i.J = function (t, e) {
        try {
            "function" == typeof t ? t.apply(a, e) : t[1].apply(t[0], e)
        } catch (n) { }
    }, i.Xa = function (t, e) {
        e && (i.h == o && (i.h = {}), i.h[t] == c && (i.h[t] = []), i.h[t].push(e))
    }, i.r = function (t, e) {
        if (i.h != o) {
            var n = i.h[t];
            if (n)
                for (; 0 < n.length;) i.J(n.shift(), e)
        }
    }, i.v = function (t, e, n, i) {
        if (n = encodeURIComponent(e) + "=" + encodeURIComponent(n), e = w.vb(t), t = w.mb(t), -1 === t.indexOf("?")) return t + "?" + n + e;
        var a = t.split("?"),
            t = a[0] + "?",
            i = w.$a(a[1], n, i);
        return t + i + e
    }, i.Qa = function (t, e) {
        var n = RegExp("[\\?&#]" + e + "=([^&#]*)").exec(t);
        return n && n.length ? decodeURIComponent(n[1]) : void 0
    }, i.Wa = function () {
        var t = o,
            e = a.location.href;
        try {
            var n = i.Qa(e, T.Z);
            if (n)
                for (var t = {}, r = n.split("|"), e = 0, s = r.length; s > e; e++) {
                    var c = r[e].split("=");
                    t[c[0]] = decodeURIComponent(c[1])
                }
            return t
        } catch (u) { }
    }, i.ba = function () {
        var e = i.Wa();
        if (e && e.TS && !(((new Date).getTime() - e.TS) / 6e4 > T.Ka || e[f] !== t)) {
            var n = e[h],
                a = i.setMarketingCloudVisitorID;
            n && n.match(T.u) && a(n), i.j(E, -1), e = e[S], n = i.setAnalyticsVisitorID, e && e.match(T.u) && n(e)
        }
    }, i.Va = function (t) {
        function e(t) {
            w.pb(t) && i.setCustomerIDs(t)
        }

        function n(t) {
            t = t || {}, i._supplementalDataIDCurrent = t.supplementalDataIDCurrent || "", i._supplementalDataIDCurrentConsumed = t.supplementalDataIDCurrentConsumed || {}, i._supplementalDataIDLast = t.supplementalDataIDLast || "", i._supplementalDataIDLastConsumed = t.supplementalDataIDLastConsumed || {}
        }
        t && t[i.marketingCloudOrgID] && (t = t[i.marketingCloudOrgID], e(t.customerIDs), n(t.sdid))
    }, i.l = o, i.Ta = function (t, e, n, a) {
        e = i.v(e, "d_fieldgroup", t, 1), a.url = i.v(a.url, "d_fieldgroup", t, 1), a.m = i.v(a.m, "d_fieldgroup", t, 1), L.d[t] = u, a === Object(a) && a.m && "XMLHttpRequest" === i.la.C.D ? i.la.ib(a, n, t) : i.useCORSOnly || i.ia(t, e, n)
    }, i.ia = function (t, e, n) {
        var a, r = 0,
            c = 0;
        if (e && s) {
            for (a = 0; !r && 2 > a;) {
                try {
                    r = (r = s.getElementsByTagName(a > 0 ? "HEAD" : "head")) && 0 < r.length ? r[0] : 0
                } catch (l) {
                    r = 0
                }
                a++
            }
            if (!r) try {
                s.body && (r = s.body)
            } catch (d) {
                r = 0
            }
            if (r)
                for (a = 0; !c && 2 > a;) {
                    try {
                        c = s.createElement(a > 0 ? "SCRIPT" : "script")
                    } catch (h) {
                        c = 0
                    }
                    a++
                }
        }
        e && r && c ? (c.type = "text/javascript", c.src = e, r.firstChild ? r.insertBefore(c, r.firstChild) : r.appendChild(c), r = i.loadTimeout, P.d[t] = {
            requestStart: P.o(),
            url: e,
            ta: r,
            ra: P.ya(),
            sa: 0
        }, n && (i.l == o && (i.l = {}), i.l[t] = setTimeout(function () {
            n(u)
        }, r)), i.ja.Fa.push(e)) : n && n()
    }, i.Pa = function (t) {
        i.l != o && i.l[t] && (clearTimeout(i.l[t]), i.l[t] = 0)
    }, i.ga = l, i.ha = l, i.isAllowed = function () {
        return !i.ga && (i.ga = u, i.cookieRead(i.cookieName) || i.cookieWrite(i.cookieName, "T", 1)) && (i.ha = u), i.ha
    }, i.b = o, i.c = o;
    var d = r.Ub;
    d || (d = "MC");
    var h = r.ac;
    h || (h = "MCMID");
    var f = r.Yb;
    f || (f = "MCORGID");
    var p = r.Vb;
    p || (p = "MCCIDH");
    var g = r.Zb;
    g || (g = "MCSYNCS");
    var v = r.$b;
    v || (v = "MCSYNCSOP");
    var m = r.Wb;
    m || (m = "MCIDTS");
    var y = r.Xb;
    y || (y = "MCOPTOUT");
    var b = r.Sb;
    b || (b = "A");
    var S = r.Pb;
    S || (S = "MCAID");
    var k = r.Tb;
    k || (k = "AAM");
    var C = r.Rb;
    C || (C = "MCAAMLH");
    var E = r.Qb;
    E || (E = "MCAAMB");
    var I = r.bc;
    I || (I = "NONE"), i.L = 0, i.ea = function () {
        if (!i.L) {
            var t = i.version;
            i.audienceManagerServer && (t += "|" + i.audienceManagerServer), i.audienceManagerServerSecure && (t += "|" + i.audienceManagerServerSecure), i.L = i.fa(t)
        }
        return i.L
    }, i.ka = l, i.f = function () {
        if (!i.ka) {
            i.ka = u;
            var t, e, n, a, r = i.ea(),
                s = l,
                c = i.cookieRead(i.cookieName),
                d = new Date;
            if (i.b == o && (i.b = {}), c && "T" != c)
                for (c = c.split("|"), c[0].match(/^[\-0-9]+$/) && (parseInt(c[0], 10) != r && (s = u), c.shift()), 1 == c.length % 2 && c.pop(), r = 0; r < c.length; r += 2) t = c[r].split("-"), e = t[0], n = c[r + 1], 1 < t.length ? (a = parseInt(t[1], 10), t = 0 < t[1].indexOf("s")) : (a = 0, t = l), s && (e == p && (n = ""), a > 0 && (a = d.getTime() / 1e3 - 60)), e && n && (i.e(e, n, 1), a > 0 && (i.b["expire" + e] = a + (t ? "s" : ""), d.getTime() >= 1e3 * a || t && !i.cookieRead(i.sessionCookieName))) && (i.c || (i.c = {}), i.c[e] = u);
            s = i.loadSSL ? !!i.trackingServerSecure : !!i.trackingServer, !i.a(S) && s && (c = i.cookieRead("s_vi")) && (c = c.split("|"), 1 < c.length && 0 <= c[0].indexOf("v1") && (n = c[1], r = n.indexOf("["), r >= 0 && (n = n.substring(0, r)), n && n.match(T.u) && i.e(S, n)))
        }
    }, i.Za = function () {
        var t, e, n = i.ea();
        for (t in i.b) !Object.prototype[t] && i.b[t] && "expire" != t.substring(0, 6) && (e = i.b[t], n += (n ? "|" : "") + t + (i.b["expire" + t] ? "-" + i.b["expire" + t] : "") + "|" + e);
        i.cookieWrite(i.cookieName, n, 1)
    }, i.a = function (t, e) {
        return i.b == o || !e && i.c && i.c[t] ? o : i.b[t]
    }, i.e = function (t, e, n) {
        i.b == o && (i.b = {}), i.b[t] = e, n || i.Za()
    }, i.Sa = function (t, e) {
        var n = i.a(t, e);
        return n ? n.split("*") : o
    }, i.Ya = function (t, e, n) {
        i.e(t, e ? e.join("*") : "", n)
    }, i.Jb = function (t, e) {
        var n = i.Sa(t, e);
        if (n) {
            var a, r = {};
            for (a = 0; a < n.length; a += 2) r[n[a]] = n[a + 1];
            return r
        }
        return o
    }, i.Lb = function (t, e, n) {
        var a, r = o;
        if (e)
            for (a in r = [], e) Object.prototype[a] || (r.push(a), r.push(e[a]));
        i.Ya(t, r, n)
    }, i.j = function (t, e, n) {
        var a = new Date;
        a.setTime(a.getTime() + 1e3 * e), i.b == o && (i.b = {}), i.b["expire" + t] = Math.floor(a.getTime() / 1e3) + (n ? "s" : ""), 0 > e ? (i.c || (i.c = {}), i.c[t] = u) : i.c && (i.c[t] = l), n && (i.cookieRead(i.sessionCookieName) || i.cookieWrite(i.sessionCookieName, "1"))
    }, i.da = function (t) {
        return t && ("object" == typeof t && (t = t.d_mid ? t.d_mid : t.visitorID ? t.visitorID : t.id ? t.id : t.uuid ? t.uuid : "" + t), t && (t = t.toUpperCase(), "NOTARGET" == t && (t = I)), !t || t != I && !t.match(T.u)) && (t = ""), t
    }, i.k = function (t, e) {
        if (i.Pa(t), i.i != o && (i.i[t] = l), P.d[t] && (P.d[t].Ab = P.o(), P.I(t)), L.d[t] && L.Ha(t, l), t == d) {
            L.isClientSideMarketingCloudVisitorID !== u && (L.isClientSideMarketingCloudVisitorID = l);
            var n = i.a(h);
            if (!n || i.overwriteCrossDomainMCIDAndAID) {
                if (n = "object" == typeof e && e.mid ? e.mid : i.da(e), !n) {
                    if (i.B) return void i.getAnalyticsVisitorID(o, l, u);
                    n = i.s(0, h)
                }
                i.e(h, n)
            }
            n && n != I || (n = ""), "object" == typeof e && ((e.d_region || e.dcs_region || e.d_blob || e.blob) && i.k(k, e), i.B && e.mid && i.k(b, {
                id: e.id
            })), i.r(h, [n])
        }
        if (t == k && "object" == typeof e) {
            n = 604800, e.id_sync_ttl != c && e.id_sync_ttl && (n = parseInt(e.id_sync_ttl, 10));
            var a = i.a(C);
            a || ((a = e.d_region) || (a = e.dcs_region), a && (i.j(C, n), i.e(C, a))), a || (a = ""), i.r(C, [a]), a = i.a(E), (e.d_blob || e.blob) && ((a = e.d_blob) || (a = e.blob), i.j(E, n), i.e(E, a)), a || (a = ""), i.r(E, [a]), !e.error_msg && i.A && i.e(p, i.A)
        }
        if (t == b && (n = i.a(S), (!n || i.overwriteCrossDomainMCIDAndAID) && ((n = i.da(e)) ? n !== I && i.j(E, -1) : n = I, i.e(S, n)), n && n != I || (n = ""), i.r(S, [n])), i.idSyncDisableSyncs ? A.za = u : (A.za = l, n = {}, n.ibs = e.ibs, n.subdomain = e.subdomain, A.wb(n)), e === Object(e)) {
            var r;
            i.isAllowed() && (r = i.a(y)), r || (r = I, e.d_optout && e.d_optout instanceof Array && (r = e.d_optout.join(",")), n = parseInt(e.d_ottl, 10), isNaN(n) && (n = 7200), i.j(y, n, u), i.e(y, r)), i.r(y, [r])
        }
    }, i.i = o, i.t = function (t, e, n, a, r) {
        var s, c = "",
            l = w.ob(t);
        return i.isAllowed() && (i.f(), c = i.a(t, _[t] === u), i.disableThirdPartyCalls && !c && (t === h ? (c = i.s(0, h), i.setMarketingCloudVisitorID(c)) : t === S && !l && (c = "", i.setAnalyticsVisitorID(c))), (!c || i.c && i.c[t]) && (!i.disableThirdPartyCalls || l)) && (t == h || t == y ? s = d : t == C || t == E ? s = k : t == S && (s = b), s) ? (!e || i.i != o && i.i[s] || (i.i == o && (i.i = {}), i.i[s] = u, i.Ta(s, e, function (e, n) {
            if (!i.a(t))
                if (P.d[s] && (P.d[s].timeout = P.o(), P.d[s].nb = !!e, P.I(s)), n !== Object(n) || i.useCORSOnly) {
                    e && L.Ha(s, u);
                    var a = "";
                    t == h ? a = i.s(0, h) : s == k && (a = {
                        error_msg: "timeout"
                    }), i.k(s, a)
                } else i.ia(s, n.url, n.G)
        }, r)), c ? c : (i.Xa(t, n), e || i.k(s, {
            id: I
        }), "")) : (t != h && t != S || c != I || (c = "", a = u), n && (a || i.disableThirdPartyCalls) && i.J(n, [c]), c)
    }, i._setMarketingCloudFields = function (t) {
        i.f(), i.k(d, t)
    }, i.setMarketingCloudVisitorID = function (t) {
        i._setMarketingCloudFields(t)
    }, i.B = l, i.getMarketingCloudVisitorID = function (t, e) {
        if (i.isAllowed()) {
            i.marketingCloudServer && 0 > i.marketingCloudServer.indexOf(".demdex.net") && (i.B = u);
            var n = i.z("_setMarketingCloudFields");
            return i.t(h, n.url, t, e, n)
        }
        return ""
    }, i.Ua = function () {
        i.getAudienceManagerBlob()
    }, r.AuthState = {
        UNKNOWN: 0,
        AUTHENTICATED: 1,
        LOGGED_OUT: 2
    }, i.w = {}, i.ca = l, i.A = "", i.setCustomerIDs = function (t) {
        if (i.isAllowed() && t) {
            i.f();
            var e, n;
            for (e in t)
                if (!Object.prototype[e] && (n = t[e]))
                    if ("object" == typeof n) {
                        var a = {};
                        n.id && (a.id = n.id), n.authState != c && (a.authState = n.authState), i.w[e] = a
                    } else i.w[e] = {
                        id: n
                    };
            var t = i.getCustomerIDs(),
                a = i.a(p),
                r = "";
            a || (a = 0);
            for (e in t) Object.prototype[e] || (n = t[e], r += (r ? "|" : "") + e + "|" + (n.id ? n.id : "") + (n.authState ? n.authState : ""));
            i.A = i.fa(r), i.A != a && (i.ca = u, i.Ua())
        }
    }, i.getCustomerIDs = function () {
        i.f();
        var t, e, n = {};
        for (t in i.w) Object.prototype[t] || (e = i.w[t], n[t] || (n[t] = {}), e.id && (n[t].id = e.id), n[t].authState = e.authState != c ? e.authState : r.AuthState.UNKNOWN);
        return n
    }, i._setAnalyticsFields = function (t) {
        i.f(), i.k(b, t)
    }, i.setAnalyticsVisitorID = function (t) {
        i._setAnalyticsFields(t)
    }, i.getAnalyticsVisitorID = function (t, e, n) {
        if (i.isAllowed()) {
            var a = "";
            if (n || (a = i.getMarketingCloudVisitorID(function () {
                    i.getAnalyticsVisitorID(t, u)
            })), a || n) {
                var r = n ? i.marketingCloudServer : i.trackingServer,
                    s = "";
                i.loadSSL && (n ? i.marketingCloudServerSecure && (r = i.marketingCloudServerSecure) : i.trackingServerSecure && (r = i.trackingServerSecure));
                var o = {};
                if (r) {
                    var r = "http" + (i.loadSSL ? "s" : "") + "://" + r + "/id",
                        a = "d_visid_ver=" + i.version + "&mcorgid=" + encodeURIComponent(i.marketingCloudOrgID) + (a ? "&mid=" + encodeURIComponent(a) : "") + (i.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : ""),
                        c = ["s_c_il", i._in, "_set" + (n ? "MarketingCloud" : "Analytics") + "Fields"],
                        s = r + "?" + a + "&callback=s_c_il%5B" + i._in + "%5D._set" + (n ? "MarketingCloud" : "Analytics") + "Fields";
                    o.m = r + "?" + a, o.oa = c
                }
                return o.url = s, i.t(n ? h : S, s, t, e, o)
            }
        }
        return ""
    }, i._setAudienceManagerFields = function (t) {
        i.f(), i.k(k, t)
    }, i.z = function (t) {
        var e = i.audienceManagerServer,
            n = "",
            a = i.a(h),
            r = i.a(E, u),
            s = i.a(S),
            s = s && s != I ? "&d_cid_ic=AVID%01" + encodeURIComponent(s) : "";
        if (i.loadSSL && i.audienceManagerServerSecure && (e = i.audienceManagerServerSecure), e) {
            var o, c, n = i.getCustomerIDs();
            if (n)
                for (o in n) Object.prototype[o] || (c = n[o], s += "&d_cid_ic=" + encodeURIComponent(o) + "%01" + encodeURIComponent(c.id ? c.id : "") + (c.authState ? "%01" + c.authState : ""));
            return t || (t = "_setAudienceManagerFields"), e = "http" + (i.loadSSL ? "s" : "") + "://" + e + "/id", a = "d_visid_ver=" + i.version + "&d_rtbd=json&d_ver=2" + (!a && i.B ? "&d_verify=1" : "") + "&d_orgid=" + encodeURIComponent(i.marketingCloudOrgID) + "&d_nsid=" + (i.idSyncContainerID || 0) + (a ? "&d_mid=" + encodeURIComponent(a) : "") + (i.idSyncDisable3rdPartySyncing ? "&d_coppa=true" : "") + (r ? "&d_blob=" + encodeURIComponent(r) : "") + s, r = ["s_c_il", i._in, t], n = e + "?" + a + "&d_cb=s_c_il%5B" + i._in + "%5D." + t, {
                url: n,
                m: e + "?" + a,
                oa: r
            }
        }
        return {
            url: n
        }
    }, i.getAudienceManagerLocationHint = function (t, e) {
        if (i.isAllowed() && i.getMarketingCloudVisitorID(function () {
                i.getAudienceManagerLocationHint(t, u)
        })) {
            var n = i.a(S);
            if (n || (n = i.getAnalyticsVisitorID(function () {
                    i.getAudienceManagerLocationHint(t, u)
            })), n) return n = i.z(), i.t(C, n.url, t, e, n)
        }
        return ""
    }, i.getLocationHint = i.getAudienceManagerLocationHint, i.getAudienceManagerBlob = function (t, e) {
        if (i.isAllowed() && i.getMarketingCloudVisitorID(function () {
                i.getAudienceManagerBlob(t, u)
        })) {
            var n = i.a(S);
            if (n || (n = i.getAnalyticsVisitorID(function () {
                    i.getAudienceManagerBlob(t, u)
            })), n) {
                var n = i.z(),
                    a = n.url;
                return i.ca && i.j(E, -1), i.t(E, a, t, e, n)
            }
        }
        return ""
    }, i._supplementalDataIDCurrent = "", i._supplementalDataIDCurrentConsumed = {}, i._supplementalDataIDLast = "", i._supplementalDataIDLastConsumed = {}, i.getSupplementalDataID = function (t, e) {
        !i._supplementalDataIDCurrent && !e && (i._supplementalDataIDCurrent = i.s(1));
        var n = i._supplementalDataIDCurrent;
        return i._supplementalDataIDLast && !i._supplementalDataIDLastConsumed[t] ? (n = i._supplementalDataIDLast, i._supplementalDataIDLastConsumed[t] = u) : n && (i._supplementalDataIDCurrentConsumed[t] && (i._supplementalDataIDLast = i._supplementalDataIDCurrent, i._supplementalDataIDLastConsumed = i._supplementalDataIDCurrentConsumed, i._supplementalDataIDCurrent = n = e ? "" : i.s(1), i._supplementalDataIDCurrentConsumed = {}), n && (i._supplementalDataIDCurrentConsumed[t] = u)), n
    }, r.OptOut = {
        GLOBAL: "global"
    }, i.getOptOut = function (t, e) {
        if (i.isAllowed()) {
            var n = i.z("_setMarketingCloudFields");
            return i.t(y, n.url, t, e, n)
        }
        return ""
    }, i.isOptedOut = function (t, e, n) {
        return i.isAllowed() ? (e || (e = r.OptOut.GLOBAL), (n = i.getOptOut(function (n) {
            i.J(t, [n == r.OptOut.GLOBAL || 0 <= n.indexOf(e)])
        }, n)) ? n == r.OptOut.GLOBAL || 0 <= n.indexOf(e) : o) : l
    }, i.appendVisitorIDsTo = function (t) {
        var e = T.Z,
            a = n([
                [h, i.a(h)],
                [S, i.a(S)],
                [f, i.marketingCloudOrgID]
            ]);
        try {
            return i.v(t, e, a)
        } catch (r) {
            return t
        }
    };
    var T = {
        q: !!a.postMessage,
        La: 1,
        aa: 864e5,
        Z: "adobe_mc",
        u: /^[0-9a-fA-F\-]+$/,
        Ka: 5
    };
    i.Eb = T, i.na = {
        postMessage: function (t, e, n) {
            var i = 1;
            e && (T.q ? n.postMessage(t, e.replace(/([^:]+:\/\/[^\/]+).*/, "$1")) : e && (n.location = e.replace(/#.*$/, "") + "#" + +new Date + i++ + "&" + t))
        },
        U: function (t, e) {
            var n;
            try {
                T.q && (t && (n = function (n) {
                    return "string" == typeof e && n.origin !== e || "[object Function]" === Object.prototype.toString.call(e) && !1 === e(n.origin) ? !1 : void t(n)
                }), window.addEventListener ? window[t ? "addEventListener" : "removeEventListener"]("message", n, !1) : window[t ? "attachEvent" : "detachEvent"]("onmessage", n))
            } catch (i) { }
        }
    };
    var w = {
        M: function () {
            return s.addEventListener ? function (t, e, n) {
                t.addEventListener(e, function (t) {
                    "function" == typeof n && n(t)
                }, l)
            } : s.attachEvent ? function (t, e, n) {
                t.attachEvent("on" + e, function (t) {
                    "function" == typeof n && n(t)
                })
            } : void 0
        }(),
        map: function (t, e) {
            if (Array.prototype.map) return t.map(e);
            if (void 0 === t || t === o) throw new TypeError;
            var n = Object(t),
                i = n.length >>> 0;
            if ("function" != typeof e) throw new TypeError;
            for (var a = Array(i), r = 0; i > r; r++) r in n && (a[r] = e.call(e, n[r], r, n));
            return a
        },
        va: function (t, e) {
            return this.map(t, function (t) {
                return encodeURIComponent(t)
            }).join(e)
        },
        vb: function (t) {
            var e = t.indexOf("#");
            return e > 0 ? t.substr(e) : ""
        },
        mb: function (t) {
            var e = t.indexOf("#");
            return e > 0 ? t.substr(0, e) : t
        },
        $a: function (t, e, n) {
            return t = t.split("&"), n = n != o ? n : t.length, t.splice(n, 0, e), t.join("&")
        },
        ob: function (t, e, n) {
            return t !== S ? l : (e || (e = i.trackingServer), n || (n = i.trackingServerSecure), t = i.loadSSL ? n : e, "string" == typeof t && t.length ? 0 > t.indexOf("2o7.net") && 0 > t.indexOf("omtrdc.net") : l)
        },
        pb: function (t) {
            return Boolean(t && t === Object(t))
        }
    };
    i.Kb = w;
    var D = {
        C: function () {
            var t = "none",
                e = u;
            return "undefined" != typeof XMLHttpRequest && XMLHttpRequest === Object(XMLHttpRequest) && ("withCredentials" in new XMLHttpRequest ? t = "XMLHttpRequest" : new Function("/*@cc_on return /^10/.test(@_jscript_version) @*/")() ? t = "XMLHttpRequest" : "undefined" != typeof XDomainRequest && XDomainRequest === Object(XDomainRequest) && (e = l), 0 < Object.prototype.toString.call(window.Bb).indexOf("Constructor") && (e = l)), {
                D: t,
                Nb: e
            }
        }(),
        jb: function () {
            return "none" === this.C.D ? o : new window[this.C.D]
        },
        ib: function (t, e, n) {
            var a = this;
            e && (t.G = e);
            try {
                var r = this.jb();
                r.open("get", t.m + "&ts=" + (new Date).getTime(), u), "XMLHttpRequest" === this.C.D && (r.withCredentials = u, r.timeout = i.loadTimeout, r.setRequestHeader("Content-Type", "application/x-www-form-urlencoded"), r.onreadystatechange = function () {
                    if (4 === this.readyState && 200 === this.status) t: {
                        var e;
                        try {
                            if (e = JSON.parse(this.responseText), e !== Object(e)) {
                                a.n(t, o, "Response is not JSON");
                                break t
                            }
                        } catch (n) {
                            a.n(t, n, "Error parsing response as JSON");
                            break t
                        }
                        try {
                            for (var i = t.oa, r = window, s = 0; s < i.length; s++) r = r[i[s]];
                            r(e)
                        } catch (c) {
                            a.n(t, c, "Error forming callback function")
                        }
                    }
                }), r.onerror = function (e) {
                    a.n(t, e, "onerror")
                }, r.ontimeout = function (e) {
                    a.n(t, e, "ontimeout")
                }, r.send(), P.d[n] = {
                    requestStart: P.o(),
                    url: t.m,
                    ta: r.timeout,
                    ra: P.ya(),
                    sa: 1
                }, i.ja.Fa.push(t.m)
            } catch (s) {
                this.n(t, s, "try-catch")
            }
        },
        n: function (t, e, n) {
            i.CORSErrors.push({
                Ob: t,
                error: e,
                description: n
            }), t.G && ("ontimeout" === n ? t.G(u) : t.G(l, t))
        }
    };
    i.la = D;
    var A = {
        Na: 3e4,
        $: 649,
        Ja: l,
        id: o,
        T: [],
        Q: o,
        xa: function (t) {
            return "string" == typeof t ? (t = t.split("/"), t[0] + "//" + t[2]) : void 0
        },
        g: o,
        url: o,
        kb: function () {
            var t = "http://fast.",
                e = "?d_nsid=" + i.idSyncContainerID + "#" + encodeURIComponent(s.location.href);
            return this.g || (this.g = "nosubdomainreturned"), i.loadSSL && (t = i.idSyncSSLUseAkamai ? "https://fast." : "https://"), t = t + this.g + ".demdex.net/dest5.html" + e, this.Q = this.xa(t), this.id = "destination_publishing_iframe_" + this.g + "_" + i.idSyncContainerID, t
        },
        cb: function () {
            var t = "?d_nsid=" + i.idSyncContainerID + "#" + encodeURIComponent(s.location.href);
            "string" == typeof i.K && i.K.length && (this.id = "destination_publishing_iframe_" + (new Date).getTime() + "_" + i.idSyncContainerID, this.Q = this.xa(i.K), this.url = i.K + t)
        },
        za: o,
        ua: l,
        W: l,
        F: o,
        cc: o,
        ub: o,
        dc: o,
        V: l,
        H: [],
        sb: [],
        tb: [],
        Ba: T.q ? 15 : 100,
        R: [],
        qb: [],
        pa: u,
        Ea: l,
        Da: function () {
            return !i.idSyncDisable3rdPartySyncing && (this.ua || i.Gb) && this.g && "nosubdomainreturned" !== this.g && this.url && !this.W
        },
        O: function () {
            function t() {
                i = document.createElement("iframe"), i.sandbox = "allow-scripts allow-same-origin", i.title = "Adobe ID Syncing iFrame", i.id = n.id, i.style.cssText = "display: none; width: 0; height: 0;", i.src = n.url, n.ub = u, e(), document.body.appendChild(i)
            }

            function e() {
                w.M(i, "load", function () {
                    i.className = "aamIframeLoaded", n.F = u, n.p()
                })
            }
            this.W = u;
            var n = this,
                i = document.getElementById(this.id);
            i ? "IFRAME" !== i.nodeName ? (this.id += "_2", t()) : "aamIframeLoaded" !== i.className ? e() : (this.F = u, this.Aa = i, this.p()) : t(), this.Aa = i
        },
        p: function (t) {
            var e = this;
            t === Object(t) && (this.R.push(t), this.xb(t)), (this.Ea || !T.q || this.F) && this.R.length && (this.I(this.R.shift()), this.p()), !i.idSyncDisableSyncs && this.F && this.H.length && !this.V && (this.Ja || (this.Ja = u, setTimeout(function () {
                e.Ba = T.q ? 15 : 150
            }, this.Na)), this.V = u, this.Ga())
        },
        xb: function (t) {
            var e, n, i;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (t = 0; n > t; t++) i = e[t], i.syncOnPage && this.qa(i, "", "syncOnPage")
        },
        I: function (t) {
            var e, n, i, a, r, s = encodeURIComponent;
            if ((e = t.ibs) && e instanceof Array && (n = e.length))
                for (i = 0; n > i; i++) a = e[i], r = [s("ibs"), s(a.id || ""), s(a.tag || ""), w.va(a.url || [], ","), s(a.ttl || ""), "", "", a.fireURLSync ? "true" : "false"], a.syncOnPage || (this.pa ? this.N(r.join("|")) : a.fireURLSync && this.qa(a, r.join("|")));
            this.qb.push(t)
        },
        qa: function (t, e, n) {
            var a = (n = "syncOnPage" === n ? u : l) ? v : g;
            i.f();
            var r = i.a(a),
                s = l,
                o = l,
                c = Math.ceil((new Date).getTime() / T.aa);
            r ? (r = r.split("*"), o = this.yb(r, t.id, c), s = o.gb, o = o.hb, (!s || !o) && this.wa(n, t, e, r, a, c)) : (r = [], this.wa(n, t, e, r, a, c))
        },
        yb: function (t, e, n) {
            var i, a, r, s = l,
                o = l;
            for (a = 0; a < t.length; a++) i = t[a], r = parseInt(i.split("-")[1], 10), i.match("^" + e + "-") ? (s = u, r > n ? o = u : (t.splice(a, 1), a--)) : n >= r && (t.splice(a, 1), a--);
            return {
                gb: s,
                hb: o
            }
        },
        rb: function (t) {
            if (t.join("*").length > this.$)
                for (t.sort(function (t, e) {
                        return parseInt(t.split("-")[1], 10) - parseInt(e.split("-")[1], 10)
                }) ; t.join("*").length > this.$;) t.shift()
        },
        wa: function (t, e, n, a, r, s) {
            var c = this;
            if (t) {
                if ("img" === e.tag) {
                    var u, l, d, t = e.url,
                        n = i.loadSSL ? "https:" : "http:";
                    for (a = 0, u = t.length; u > a; a++) {
                        l = t[a], d = /^\/\//.test(l);
                        var h = new Image;
                        w.M(h, "load", function (t, e, n, a) {
                            return function () {
                                c.T[t] = o, i.f();
                                var s = i.a(r),
                                    u = [];
                                if (s) {
                                    var l, d, h, s = s.split("*");
                                    for (l = 0, d = s.length; d > l; l++) h = s[l], h.match("^" + e.id + "-") || u.push(h)
                                }
                                c.Ia(u, e, n, a)
                            }
                        }(this.T.length, e, r, s)), h.src = (d ? n : "") + l, this.T.push(h)
                    }
                }
            } else this.N(n), this.Ia(a, e, r, s)
        },
        N: function (t) {
            var e = encodeURIComponent;
            this.H.push(e(i.Hb ? "---destpub-debug---" : "---destpub---") + t)
        },
        Ia: function (t, e, n, a) {
            t.push(e.id + "-" + (a + Math.ceil(e.ttl / 60 / 24))), this.rb(t), i.e(n, t.join("*"))
        },
        Ga: function () {
            var t, e = this;
            this.H.length ? (t = this.H.shift(), i.na.postMessage(t, this.url, this.Aa.contentWindow), this.sb.push(t), setTimeout(function () {
                e.Ga()
            }, this.Ba)) : this.V = l
        },
        U: function (t) {
            var e = /^---destpub-to-parent---/;
            "string" == typeof t && e.test(t) && (e = t.replace(e, "").split("|"), "canSetThirdPartyCookies" === e[0] && (this.pa = "true" === e[1] ? u : l, this.Ea = u, this.p()), this.tb.push(t))
        },
        wb: function (t) {
            (this.url === o || t.subdomain && "nosubdomainreturned" === this.g) && (this.g = "string" == typeof i.ma && i.ma.length ? i.ma : t.subdomain || "", this.url = this.kb()), t.ibs instanceof Array && t.ibs.length && (this.ua = u), this.Da() && (i.idSyncAttachIframeOnWindowLoad ? (r.Y || "complete" === s.readyState || "loaded" === s.readyState) && this.O() : this.ab()), "function" == typeof i.idSyncIDCallResult ? i.idSyncIDCallResult(t) : this.p(t), "function" == typeof i.idSyncAfterIDCallResult && i.idSyncAfterIDCallResult(t)
        },
        bb: function (t, e) {
            return i.Ib || !t || e - t > T.La
        },
        ab: function () {
            function t() {
                e.W || (document.body ? e.O() : setTimeout(t, 30))
            }
            var e = this;
            t()
        }
    };
    i.Fb = A, i.timeoutMetricsLog = [];
    var P = {
        fb: window.performance && window.performance.timing ? 1 : 0,
        Ca: window.performance && window.performance.timing ? window.performance.timing : o,
        X: o,
        P: o,
        d: {},
        S: [],
        send: function (t) {
            if (i.takeTimeoutMetrics && t === Object(t)) {
                var e, n = [],
                    a = encodeURIComponent;
                for (e in t) t.hasOwnProperty(e) && n.push(a(e) + "=" + a(t[e]));
                t = "http" + (i.loadSSL ? "s" : "") + "://dpm.demdex.net/event?d_visid_ver=" + i.version + "&d_visid_stg_timeout=" + i.loadTimeout + "&" + n.join("&") + "&d_orgid=" + a(i.marketingCloudOrgID) + "&d_timingapi=" + this.fb + "&d_winload=" + this.lb() + "&d_ld=" + this.o(), (new Image).src = t, i.timeoutMetricsLog.push(t)
            }
        },
        lb: function () {
            return this.P === o && (this.P = this.Ca ? this.X - this.Ca.navigationStart : this.X - r.eb), this.P
        },
        o: function () {
            return (new Date).getTime()
        },
        I: function (t) {
            var e = this.d[t],
                n = {};
            n.d_visid_stg_timeout_captured = e.ta, n.d_visid_cors = e.sa, n.d_fieldgroup = t, n.d_settimeout_overriden = e.ra, e.timeout ? e.nb ? (n.d_visid_timedout = 1, n.d_visid_timeout = e.timeout - e.requestStart, n.d_visid_response = -1) : (n.d_visid_timedout = "n/a", n.d_visid_timeout = "n/a", n.d_visid_response = "n/a") : (n.d_visid_timedout = 0, n.d_visid_timeout = -1, n.d_visid_response = e.Ab - e.requestStart), n.d_visid_url = e.url, r.Y ? this.send(n) : this.S.push(n), delete this.d[t]
        },
        zb: function () {
            for (var t = 0, e = this.S.length; e > t; t++) this.send(this.S[t])
        },
        ya: function () {
            return "function" == typeof setTimeout.toString ? -1 < setTimeout.toString().indexOf("[native code]") ? 0 : 1 : -1
        }
    };
    i.Mb = P;
    var L = {
        isClientSideMarketingCloudVisitorID: o,
        MCIDCallTimedOut: o,
        AnalyticsIDCallTimedOut: o,
        AAMIDCallTimedOut: o,
        d: {},
        Ha: function (t, e) {
            switch (t) {
                case d:
                    e === l ? this.MCIDCallTimedOut !== u && (this.MCIDCallTimedOut = l) : this.MCIDCallTimedOut = e;
                    break;
                case b:
                    e === l ? this.AnalyticsIDCallTimedOut !== u && (this.AnalyticsIDCallTimedOut = l) : this.AnalyticsIDCallTimedOut = e;
                    break;
                case k:
                    e === l ? this.AAMIDCallTimedOut !== u && (this.AAMIDCallTimedOut = l) : this.AAMIDCallTimedOut = e
            }
        }
    };
    i.isClientSideMarketingCloudVisitorID = function () {
        return L.isClientSideMarketingCloudVisitorID
    }, i.MCIDCallTimedOut = function () {
        return L.MCIDCallTimedOut
    }, i.AnalyticsIDCallTimedOut = function () {
        return L.AnalyticsIDCallTimedOut
    }, i.AAMIDCallTimedOut = function () {
        return L.AAMIDCallTimedOut
    }, i.idSyncGetOnPageSyncInfo = function () {
        return i.f(), i.a(v)
    }, i.idSyncByURL = function (t) {
        var e, n = t || {};
        e = n.minutesToLive;
        var a = "";
        if (i.idSyncDisableSyncs && (a = a ? a : "Error: id syncs have been disabled"), "string" == typeof n.dpid && n.dpid.length || (a = a ? a : "Error: config.dpid is empty"), "string" == typeof n.url && n.url.length || (a = a ? a : "Error: config.url is empty"), "undefined" == typeof e ? e = 20160 : (e = parseInt(e, 10), (isNaN(e) || 0 >= e) && (a = a ? a : "Error: config.minutesToLive needs to be a positive number")), e = {
            error: a,
            ec: e
        }, e.error) return e.error;
        var r, a = t.url,
            s = encodeURIComponent,
            n = A,
            a = a.replace(/^https:/, "").replace(/^http:/, "");
        return r = w.va(["", t.dpid, t.dpuuid || ""], ","), t = ["ibs", s(t.dpid), "img", s(a), e.ttl, "", r], n.N(t.join("|")), n.p(), "Successfully queued"
    }, i.idSyncByDataSource = function (t) {
        return t === Object(t) && "string" == typeof t.dpuuid && t.dpuuid.length ? (t.url = "//dpm.demdex.net/ibs:dpid=" + t.dpid + "&dpuuid=" + t.dpuuid, i.idSyncByURL(t)) : "Error: config or config.dpuuid is empty"
    }, 0 > t.indexOf("@") && (t += "@AdobeOrg"), i.marketingCloudOrgID = t, i.cookieName = "AMCV_" + t, i.sessionCookieName = "AMCVS_" + t, i.cookieDomain = i.Ra(), i.cookieDomain == a.location.hostname && (i.cookieDomain = ""), i.loadSSL = 0 <= a.location.protocol.toLowerCase().indexOf("https"), i.loadTimeout = 3e4, i.CORSErrors = [], i.marketingCloudServer = i.audienceManagerServer = "dpm.demdex.net";
    var _ = {};
    if (_[C] = u, _[E] = u, e && "object" == typeof e) {
        for (var O in e) !Object.prototype[O] && (i[O] = e[O]);
        i.idSyncContainerID = i.idSyncContainerID || 0, i.ba(), i.f(), D = i.a(m), O = Math.ceil((new Date).getTime() / T.aa), !i.idSyncDisableSyncs && A.bb(D, O) && (i.j(E, -1), i.e(m, O)), i.getMarketingCloudVisitorID(), i.getAudienceManagerLocationHint(), i.getAudienceManagerBlob(), i.Va(i.serverState)
    } else i.ba();
    if (!i.idSyncDisableSyncs) {
        A.cb(), w.M(window, "load", function () {
            r.Y = u, P.X = P.o(), P.zb();
            var t = A;
            t.Da() && t.O()
        });
        try {
            i.na.U(function (t) {
                A.U(t.data)
            }, A.Q)
        } catch (V) { }
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
            e.Y = n
        }
        var e = window.Visitor,
            n = e.Oa,
            i = e.Ma;
        n || (n = !0), i || (i = !1), window.addEventListener ? window.addEventListener("load", t) : window.attachEvent && window.attachEvent("onload", t), e.eb = (new Date).getTime()
    }(),
    // All code and conventions are protected by copyright
    function (t, e, n) {
        function i() {
            I.getToolsByType("nielsen").length > 0 && I.domReady(I.bind(this.initialize, this))
        }

        function a(e) {
            I.domReady(I.bind(function () {
                this.twttr = e || t.twttr, this.initialize()
            }, this))
        }

        function r() {
            this.lastURL = I.URL(), this._fireIfURIChanged = I.bind(this.fireIfURIChanged, this), this._onPopState = I.bind(this.onPopState, this), this._onHashChange = I.bind(this.onHashChange, this), this._pushState = I.bind(this.pushState, this), this._replaceState = I.bind(this.replaceState, this), this.initialize()
        }

        function s() {
            var t = I.filter(I.rules, function (t) {
                return 0 === t.event.indexOf("dataelementchange")
            });
            this.dataElementsNames = I.map(t, function (t) {
                var e = t.event.match(/dataelementchange\((.*)\)/i);
                return e[1]
            }, this), this.initPolling()
        }

        function o() {
            I.addEventHandler(t, "orientationchange", o.orientationChange)
        }

        function c() {
            this.rules = I.filter(I.rules, function (t) {
                return "videoplayed" === t.event.substring(0, 11)
            }), this.eventHandler = I.bind(this.onUpdateTime, this)
        }

        function u() {
            this.defineEvents(), this.visibilityApiHasPriority = !0, e.addEventListener ? this.setVisibilityApiPriority(!1) : this.attachDetachOlderEventListeners(!0, e, "focusout");
            I.bindEvent("aftertoolinit", function () {
                I.fireEvent(I.visibility.isHidden() ? "tabblur" : "tabfocus")
            })
        }

        function l(e) {
            e = e || I.rules, this.rules = I.filter(e, function (t) {
                return "inview" === t.event
            }), this.elements = [], this.eventHandler = I.bind(this.track, this), I.addEventHandler(t, "scroll", this.eventHandler), I.addEventHandler(t, "load", this.eventHandler)
        }

        function d() {
            this.rules = I.filter(I.rules, function (t) {
                return "elementexists" === t.event
            })
        }

        function h(t) {
            this.delay = 250, this.FB = t, I.domReady(I.bind(function () {
                I.poll(I.bind(this.initialize, this), this.delay, 8)
            }, this))
        }

        function f() {
            var t = this.eventRegex = /^hover\(([0-9]+)\)$/,
                e = this.rules = [];
            I.each(I.rules, function (n) {
                var i = n.event.match(t);
                i && e.push([Number(n.event.match(t)[1]), n.selector])
            })
        }

        function p(t) {
            I.BaseTool.call(this, t), this.defineListeners(), this.beaconMethod = "plainBeacon", this.adapt = new p.DataAdapters, this.dataProvider = new p.DataProvider.Aggregate
        }

        function g(t) {
            I.BaseTool.call(this, t), this.styleElements = {}, this.targetPageParamsStore = {}
        }

        function v() {
            I.BaseTool.call(this), this.asyncScriptCallbackQueue = [], this.argsForBlockingScripts = []
        }

        function m(t) {
            I.BaseTool.call(this, t), this.varBindings = {}, this.events = [], this.products = [], this.customSetupFuns = []
        }

        function y(t) {
            I.BaseTool.call(this, t), this.name = t.name || "Basic"
        }

        function b(t) {
            I.BaseTool.call(this, t)
        }

        function S(t) {
            I.BaseTool.call(this, t)
        }

        function k(t) {
            I.BaseTool.call(this, t), this.name = t.name || "VisitorID", this.initialize()
        }
        var C = Object.prototype.toString,
            E = t._satellite && t._satellite.override,
            I = {
                initialized: !1,
                $data: function (t, e, i) {
                    if (t) {
                        var a = "__satellite__",
                            r = I.dataCache,
                            s = t[a];
                        s || (s = t[a] = I.uuid++);
                        var o = r[s];
                        return o || (o = r[s] = {}), i === n ? o[e] : void (o[e] = i)
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
                        var s = t[a];
                        e.call(n, s, a, t) && i.push(s)
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
                        var s = t[a];
                        i = i && e.call(n, s, a, t)
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
                        for (h = 1; t = a.shift() ;) t()
                    }
                    var i, a = [],
                        r = !1,
                        s = e,
                        o = s.documentElement,
                        c = o.doScroll,
                        u = "DOMContentLoaded",
                        l = "addEventListener",
                        d = "onreadystatechange",
                        h = /^loade|^c/.test(s.readyState);
                    return s[l] && s[l](u, i = function () {
                        s.removeEventListener(u, i, r), n()
                    }, r), c && s.attachEvent(d, i = function () {
                        /^c/.test(s.readyState) && (s.detachEvent(d, i), n())
                    }), t = c ? function (e) {
                        self != top ? h ? e() : a.push(e) : function () {
                            try {
                                o.doScroll("left")
                            } catch (n) {
                                return setTimeout(function () {
                                    t(e)
                                }, 50)
                            }
                            e()
                        }()
                    } : function (t) {
                        h ? t() : a.push(t)
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
                        } catch (s) { }
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
                "default": new v
            },
                n = I.settings.euCookieName || "sat_track";
            for (var i in t)
                if (t.hasOwnProperty(i)) {
                    var a, r, s;
                    if (a = t[i], a.euCookie) {
                        var o = "true" !== I.readCookie(n);
                        if (o) continue
                    }
                    if (r = I.availableTools[a.engine], !r) {
                        var c = [];
                        for (var u in I.availableTools) I.availableTools.hasOwnProperty(u) && c.push(u);
                        throw new Error("No tool engine named " + a.engine + ", available: " + c.join(",") + ".")
                    }
                    s = new r(a), s.id = i, e[i] = s
                }
            return e
        }, I.preprocessArguments = function (t, e, n, i, a) {
            function r(t) {
                return i && I.isString(t) ? t.toLowerCase() : t
            }

            function s(t) {
                var c = {};
                for (var u in t)
                    if (t.hasOwnProperty(u)) {
                        var l = t[u];
                        I.isObject(l) ? c[u] = s(l) : I.isArray(l) ? c[u] = o(l, i) : c[u] = r(I.replace(l, e, n, a))
                    }
                return c
            }

            function o(t, i) {
                for (var a = [], o = 0, c = t.length; c > o; o++) {
                    var u = t[o];
                    I.isString(u) ? u = r(I.replace(u, e, n)) : u && u.constructor === Object && (u = s(u)), a.push(u)
                }
                return a
            }
            return t ? o(t, i) : t
        }, I.execute = function (t, e, n, i) {
            function a(a) {
                var r = i[a || "default"];
                if (r) try {
                    r.triggerCommand(t, e, n)
                } catch (s) {
                    I.logError(s)
                }
            }
            if (!_satellite.settings.hideActivity)
                if (i = i || I.tools, t.engine) {
                    var r = t.engine;
                    for (var s in i)
                        if (i.hasOwnProperty(s)) {
                            var o = i[s];
                            o.settings && o.settings.engine === r && a(s)
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
            for (var a, r = e.split("."), s = t, o = I.specialProperties, c = 0, u = r.length; u > c; c++) {
                if (null == s) return n;
                var l = r[c];
                if (i && "@" === l.charAt(0)) {
                    var d = l.slice(1);
                    s = o[d](s)
                } else if (s.getAttribute && (a = l.match(/^getAttribute\((.+)\)$/))) {
                    var h = a[1];
                    s = s.getAttribute(h)
                } else s = s[l]
            }
            return s
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
            var s, o, c = I.data.customVars,
                u = r ? r.target || r.srcElement : null,
                l = {
                    uri: I.URI(),
                    protocol: e.location.protocol,
                    hostname: e.location.hostname
                };
            if (I.dataElements && i in I.dataElements) return I.getDataElement(i);
            if (o = l[i.toLowerCase()], o === n)
                if ("this." === i.substring(0, 5)) i = i.slice(5), o = I.getObjectProperty(a, i, !0);
                else if ("event." === i.substring(0, 6)) i = i.slice(6), o = I.getObjectProperty(r, i);
                else if ("target." === i.substring(0, 7)) i = i.slice(7), o = I.getObjectProperty(u, i);
                else if ("window." === i.substring(0, 7)) i = i.slice(7), o = I.getObjectProperty(t, i);
                else if ("param." === i.substring(0, 6)) i = i.slice(6), o = I.getQueryParam(i);
                else if (s = i.match(/^rand([0-9]+)$/)) {
                    var d = Number(s[1]),
                        h = (Math.random() * (Math.pow(10, d) - 1)).toFixed(0);
                    o = Array(d - h.length + 1).join("0") + h
                } else o = I.getObjectProperty(c, i);
            return o
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
                var s = t[a],
                    o = I.getVar(s, e, n);
                i.push(s + "=" + escape(o))
            }
            return "?" + i.join("&")
        }, I.fireRule = function (t, e, n) {
            var i = t.trigger;
            if (i) {
                for (var a = 0, r = i.length; r > a; a++) {
                    var s = i[a];
                    I.execute(s, e, n)
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
            }, a = I.pageLoadRules, r = I.evtHandlers[i.type], s = a.length; s--;) {
                var o = a[s];
                I.ruleMatches(o, i, n) && (I.notify('Rule "' + o.name + '" fired.', 1), I.fireRule(o, n, i))
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
                for (var s = a[r];
                    " " == s.charAt(0) ;) s = s.substring(1, s.length);
                if (0 === s.indexOf(i)) return s.substring(i.length, s.length)
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
                s = t.property,
                o = e.type,
                c = t.value,
                u = e.target || e.srcElement,
                l = n === u;
            if (t.event !== o && ("custom" !== t.event || t.customEvent !== o)) return !1;
            if (!I.ruleInScope(t)) return !1;
            if ("click" === t.event && I.isRightClick(e)) return !1;
            if (t.isDefault && i > 0) return !1;
            if (t.expired) return !1;
            if ("inview" === o && e.inviewDelay !== t.inviewDelay) return !1;
            if (!l && (t.bubbleFireIfParent === !1 || 0 !== i && t.bubbleFireIfChildFired === !1)) return !1;
            if (t.selector && !I.matchesCss(t.selector, n)) return !1;
            if (!I.propertiesMatch(s, n)) return !1;
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
                var h = I.find(r, function (i) {
                    try {
                        return !i.call(n, e, u)
                    } catch (a) {
                        return I.notify('Condition for rule "' + t.name + '" not met. Error: ' + a.message, 1), !0
                    }
                });
                if (h) return I.notify("Condition " + h.toString() + ' for rule "' + t.name + '" not met.', 1), !1
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
                var s = n && n.nodeName;
                s ? I.notify("detected " + e + " on " + n.nodeName, 1) : I.notify("detected " + e, 1);
                for (var o = n; o; o = o.parentNode) {
                    var c = !1;
                    if (I.each(a, function (e) {
                            I.ruleMatches(e, t, o, i) && (I.notify('Rule "' + e.name + '" fired.', 1), I.fireRule(e, o, t), i++, e.bubbleStop && (c = !0))
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
                for (var a = t[i], r = e.getElementsByTagName(a), s = r.length - 1; s >= 0; s--) I.registerEvents(r[s], n)
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
                    hour: t[f](),
                    minute: t[p]()
                }), Math.floor(Math.abs((t.getTime() - e.getTime()) / 864e5))
            }

            function i(t, e) {
                function n(t) {
                    return 12 * t[d]() + t[h]()
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
                var n = t[f](),
                    i = t[p](),
                    a = e[f](),
                    r = e[p]();
                return 60 * n + i > 60 * a + r
            }

            function s(t, e) {
                var n = t[f](),
                    i = t[p](),
                    a = e[f](),
                    r = e[p]();
                return 60 * a + r > 60 * n + i
            }
            var o = t.schedule;
            if (!o) return !0;
            var c = o.utc,
                u = c ? "getUTCDate" : "getDate",
                l = c ? "getUTCDay" : "getDay",
                d = c ? "getUTCFullYear" : "getFullYear",
                h = c ? "getUTCMonth" : "getMonth",
                f = c ? "getUTCHours" : "getHours",
                p = c ? "getUTCMinutes" : "getMinutes",
                g = c ? "setUTCHours" : "setHours",
                v = c ? "setUTCMinutes" : "setMinutes",
                m = c ? "setUTCDate" : "setDate";
            if (e = e || new Date, o.repeat) {
                if (r(o.start, e)) return !1;
                if (s(o.end, e)) return !1;
                if (e < o.start) return !1;
                if (o.endRepeat && e >= o.endRepeat) return !1;
                if ("daily" === o.repeat) {
                    if (o.repeatEvery) {
                        var y = n(o.start, e);
                        if (y % o.repeatEvery !== 0) return !1
                    }
                } else if ("weekly" === o.repeat) {
                    if (o.days) {
                        if (!I.contains(o.days, e[l]())) return !1
                    } else if (o.start[l]() !== e[l]()) return !1;
                    if (o.repeatEvery) {
                        var b = n(o.start, e);
                        if (b % (7 * o.repeatEvery) !== 0) return !1
                    }
                } else if ("monthly" === o.repeat) {
                    if (o.repeatEvery) {
                        var S = i(o.start, e);
                        if (S % o.repeatEvery !== 0) return !1
                    }
                    if (o.nthWeek && o.mthDay) {
                        if (o.mthDay !== e[l]()) return !1;
                        var k = Math.floor((e[u]() - e[l]() + 1) / 7);
                        if (o.nthWeek !== k) return !1
                    } else if (o.start[u]() !== e[u]()) return !1
                } else if ("yearly" === o.repeat) {
                    if (o.start[h]() !== e[h]()) return !1;
                    if (o.start[u]() !== e[u]()) return !1;
                    if (o.repeatEvery) {
                        var b = Math.abs(o.start[d]() - e[d]());
                        if (b % o.repeatEvery !== 0) return !1
                    }
                }
            } else {
                if (o.start > e) return !1;
                if (o.end < e) return !1
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
            var s = r.URI,
                o = r.subdomains,
                c = r.domains,
                u = r.protocols,
                l = r.hashes;
            return s && i(s, n.pathname + n.search) ? !1 : o && i(o, n.hostname) ? !1 : c && a(c, n.hostname) ? !1 : u && a(u, n.protocol) ? !1 : l && i(l, n.hash) ? !1 : !0
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
                for (var n, i, a, r = e.getElementsByTagName("script"), s = /^(.*)satelliteLib-([a-f0-9]{40})\.js$/, o = /^(.*)satelliteLib-([a-f0-9]{40})-staging\.js$/, c = 0, u = r.length; u > c && (a = r[c].getAttribute("src"), !a || (n || (n = a.match(s)), i || (i = a.match(o)), !i)) ; c++);
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
            for (var s = ["bannerText", "headline", "introductoryText", "customCSS"], o = 0; o < s.length; o++) {
                var c = s[o],
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
                    r++, r === a.length && (n(), clearTimeout(s), t())
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
                var s = setTimeout(i, 5e3)
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
                    s = r ? r.escapeHtml : !1,
                    o = I.preprocessArguments(t.arguments, e, n, this.forceLowerCase, s);
                r ? r.apply(this, [e, n].concat(o)) : this.$missing$ ? this.$missing$(a, e, n, o) : I.notify("Failed to trigger " + a + " for tool " + this.id, 1)
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
        }, t._satellite = I, I.ecommerce = {
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
        }, I.visibility = {
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
        }, i.prototype = {
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
        }, I.availableEventEmitters.push(i), a.prototype = {
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
        }, I.availableEventEmitters.push(a), r.prototype = {
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
        }, I.availableEventEmitters.push(r), s.prototype.getStringifiedValue = t.JSON && t.JSON.stringify || I.stringify, s.prototype.initPolling = function () {
            0 !== this.dataElementsNames.length && (this.dataElementsStore = this.getDataElementsValues(), I.poll(I.bind(this.checkDataElementValues, this), 1e3))
        }, s.prototype.getDataElementsValues = function () {
            var t = {};
            return I.each(this.dataElementsNames, function (e) {
                var n = I.getVar(e);
                t[e] = this.getStringifiedValue(n)
            }, this), t
        }, s.prototype.checkDataElementValues = function () {
            I.each(this.dataElementsNames, I.bind(function (t) {
                var n = this.getStringifiedValue(I.getVar(t)),
                    i = this.dataElementsStore[t];
                n !== i && (this.dataElementsStore[t] = n, I.onEvent({
                    type: "dataelementchange(" + t + ")",
                    target: e
                }))
            }, this))
        }, I.availableEventEmitters.push(s), o.orientationChange = function (e) {
            var n = 0 === t.orientation ? "portrait" : "landscape";
            e.orientation = n, I.onEvent(e)
        }, I.availableEventEmitters.push(o), c.prototype = {
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
                    s = t.currentTime,
                    o = e.event.match(/^videoplayed\(([0-9]+)([s%])\)$/);
                if (o) {
                    var c = o[2],
                        u = Number(o[1]),
                        l = "%" === c ? function () {
                            return 100 * (s - a) / (r - a) >= u
                        } : function () {
                            return s - a >= u
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
        }, I.availableEventEmitters.push(c), u.prototype = {
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
        }, I.availableEventEmitters.push(u), l.offset = function (n) {
            var i = null,
                a = null;
            try {
                var r = n.getBoundingClientRect(),
                    s = e,
                    o = s.documentElement,
                    c = s.body,
                    u = t,
                    l = o.clientTop || c.clientTop || 0,
                    d = o.clientLeft || c.clientLeft || 0,
                    h = u.pageYOffset || o.scrollTop || c.scrollTop,
                    f = u.pageXOffset || o.scrollLeft || c.scrollLeft;
                i = r.top + h - l, a = r.left + f - d
            } catch (p) { }
            return {
                top: i,
                left: a
            }
        }, l.getViewportHeight = function () {
            var n = t.innerHeight,
                i = e.compatMode;
            return i && (n = "CSS1Compat" == i ? e.documentElement.clientHeight : e.body.clientHeight), n
        }, l.getScrollTop = function () {
            return e.documentElement.scrollTop ? e.documentElement.scrollTop : e.body.scrollTop
        }, l.isElementInDocument = function (t) {
            return e.body.contains(t)
        }, l.isElementInView = function (t) {
            if (!l.isElementInDocument(t)) return !1;
            var e = l.getViewportHeight(),
                n = l.getScrollTop(),
                i = l.offset(t).top,
                a = t.offsetHeight;
            return null !== i ? !(n > i + a || i > n + e) : !1
        }, l.prototype = {
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
                if (l.isElementInView(t)) {
                    i || I.$data(t, "inview", !0);
                    var a = this;
                    this.processRules(t, function (n, i, r) {
                        if (e || !n.inviewDelay) I.$data(t, i, !0), I.onEvent({
                            type: "inview",
                            target: t,
                            inviewDelay: n.inviewDelay
                        });
                        else if (n.inviewDelay) {
                            var s = I.$data(t, r);
                            s || (s = setTimeout(function () {
                                a.checkInView(t, !0, n.inviewDelay)
                            }, n.inviewDelay), I.$data(t, r, s))
                        }
                    }, n)
                } else {
                    if (!l.isElementInDocument(t)) {
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
        }, I.availableEventEmitters.push(l), d.prototype.backgroundTasks = function () {
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
        }, I.availableEventEmitters.push(d), h.prototype = {
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
        }, I.availableEventEmitters.push(h), f.prototype = {
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
        }, I.availableEventEmitters.push(f), I.inherit(p, I.BaseTool), I.extend(p.prototype, {
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
                this.dataProvider.register(new p.DataProvider.VisitorID(I.getVisitorId())), e ? (t = new p.DataProvider.Generic("rsid", function () {
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
        }), p.DataProvider = {}, p.DataProvider.Generic = function (t, e) {
            this.key = t, this.valueFn = e
        }, I.extend(p.DataProvider.Generic.prototype, {
            isReady: function () {
                return !0
            },
            getValue: function () {
                return this.valueFn()
            },
            provide: function () {
                this.isReady() || p.prototype.notify("Not yet ready to provide value for: " + this.key, 5);
                var t = {};
                return t[this.key] = this.getValue(), t
            }
        }), p.DataProvider.VisitorID = function (t, e, n) {
            this.key = e || "uuid", this.visitorInstance = t, this.visitorInstance && (this.visitorId = t.getMarketingCloudVisitorID([this, this._visitorIdCallback])), this.fallbackProvider = n || new p.UUID
        }, I.inherit(p.DataProvider.VisitorID, p.DataProvider.Generic), I.extend(p.DataProvider.VisitorID.prototype, {
            isReady: function () {
                return null === this.visitorInstance ? !0 : !!this.visitorId
            },
            getValue: function () {
                return this.visitorId || this.fallbackProvider.get()
            },
            _visitorIdCallback: function (t) {
                this.visitorId = t
            }
        }), p.DataProvider.Aggregate = function () {
            this.providers = [];
            for (var t = 0; t < arguments.length; t++) this.register(arguments[t])
        }, I.extend(p.DataProvider.Aggregate.prototype, {
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
        }), p.UUID = function () { }, I.extend(p.UUID.prototype, {
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
        }), p.DataAdapters = function () { }, I.extend(p.DataAdapters.prototype, {
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
        }), I.availableTools.nielsen = p, I.inherit(g, I.BaseTool), I.extend(g.prototype, {
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
                    s = this;
                I.addEventHandler(t, "load", I.bind(function () {
                    I.cssQuery(a.mboxGoesAround, function (n) {
                        var i = n[0];
                        if (i) {
                            var o = e.createElement("div");
                            o.id = r, i.parentNode.replaceChild(o, i), o.appendChild(i), t.mboxDefine(r, a.mboxName);
                            var c = [a.mboxName];
                            a.arguments && (c = c.concat(a.arguments)), t.mboxUpdate.apply(null, c), s.reappearWhenCallComesBack(i, r, a.timeout, a)
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
        }), I.availableTools.tnt = g, I.inherit(v, I.BaseTool), I.extend(v.prototype, {
            name: "Default",
            $loadIframe: function (e, n, i) {
                var a = i.pages,
                    r = i.loadOn,
                    s = I.bind(function () {
                        I.each(a, function (t) {
                            this.loadIframe(e, n, t)
                        }, this)
                    }, this);
                r || s(), "domready" === r && I.domReady(s), "load" === r && I.addEventHandler(t, "load", s)
            },
            loadIframe: function (t, n, i) {
                var a = e.createElement("iframe");
                a.style.display = "none";
                var r = I.data.host,
                    s = i.data,
                    o = this.scriptURL(i.src),
                    c = I.searchVariables(s, t, n);
                r && (o = I.basePath() + o), o += c, a.src = o;
                var u = e.getElementsByTagName("body")[0];
                u ? u.appendChild(a) : I.domReady(function () {
                    e.getElementsByTagName("body")[0].appendChild(a)
                })
            },
            scriptURL: function (t) {
                var e = I.settings.scriptDir || "";
                return t
            },
            $loadScript: function (e, n, i) {
                var a = i.scripts,
                    r = i.sequential,
                    s = i.loadOn,
                    o = I.bind(function () {
                        r ? this.loadScripts(e, n, a) : I.each(a, function (t) {
                            this.loadScripts(e, n, [t])
                        }, this)
                    }, this);
                s ? "domready" === s ? I.domReady(o) : "load" === s && I.addEventHandler(t, "load", o) : o()
            },
            loadScripts: function (t, e, n) {
                function i() {
                    if (r.length > 0 && a) {
                        var c = r.shift();
                        c.call(t, e, s)
                    }
                    var u = n.shift();
                    if (u) {
                        var l = I.data.host,
                            d = o.scriptURL(u.src);
                        l && (d = I.basePath() + d), a = u, I.loadScript(d, i)
                    }
                }
                try {
                    var a, n = n.slice(0),
                        r = this.asyncScriptCallbackQueue,
                        s = e.target || e.srcElement,
                        o = this
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
        }), I.availableTools["default"] = v, I.inherit(m, I.BaseTool), I.extend(m.prototype, {
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
                    s = this.getAccount(i),
                    o = t.s_gi;
                if (!o) return null;
                if (this.isValidSCInstance(e) || (e = null), !s && !e) return I.notify("Adobe Analytics: tracker not initialized because account was not found", 1), null;
                var e = e || o(s),
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
                var a, r, s, o = "",
                    c = n && n.dc;
                return a = i, r = a.indexOf(","), r >= 0 && (a = a.gb(0, r)), a = a.replace(/[^A-Za-z0-9]/g, ""), o || (o = "2o7.net"), c = c ? ("" + c).toLowerCase() : "d1", "2o7.net" == o && ("d1" == c ? c = "112" : "d2" == c && (c = "122"), s = ""), r = a + "." + c + "." + s + o
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
                    s = n && n.addEvent || [],
                    o = this.getS(null, {
                        setVars: r,
                        addEvent: s
                    });
                if (!o) return void I.notify("Adobe Analytics: page code not loaded", 1);
                var c = o.linkTrackVars,
                    u = o.linkTrackEvents,
                    l = this.definedVarNames(r);
                n && n.customSetup && n.customSetup.call(t, e, o), s.length > 0 && l.push("events"), o.products && l.push("products"), l = this.mergeTrackLinkVars(o.linkTrackVars, l), s = this.mergeTrackLinkVars(o.linkTrackEvents, s), o.linkTrackVars = this.getCustomLinkVarsList(l);
                var d = I.map(s, function (t) {
                    return t.split(":")[0]
                });
                o.linkTrackEvents = this.getCustomLinkVarsList(d), o.tl(!0, i || "o", a), I.notify(["Adobe Analytics: tracked link ", "using: linkTrackVars=", I.stringify(o.linkTrackVars), "; linkTrackEvents=", I.stringify(o.linkTrackEvents)].join(""), 1), o.linkTrackVars = c, o.linkTrackEvents = u
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
                    s = this.settings.fieldVarMapping;
                if (I.each(a.items, function (t) {
                        this.products.push(t)
                }, this), r.products = I.map(this.products, function (t) {
                        var e = [];
                        if (s && s.item)
                            for (var n in s.item)
                                if (s.item.hasOwnProperty(n)) {
                                    var i = s.item[n];
                                    e.push(i + "=" + t[n]), "event" === i.substring(0, 5) && this.events.push(i)
                }
                        var a = ["", t.product, t.quantity, t.unitPrice * t.quantity];
                        return e.length > 0 && a.push(e.join("|")), a.join(";")
                }, this).join(","), s && s.transaction) {
                    var o = [];
                    for (var c in s.transaction)
                        if (s.transaction.hasOwnProperty(c)) {
                            var i = s.transaction[c];
                            o.push(i + "=" + a[c]), "event" === i.substring(0, 5) && this.events.push(i)
                        }
                    r.products.length > 0 && (r.products += ","), r.products += ";;;;" + o.join("|")
                }
            },
            $addEvent: function (t, e) {
                for (var n = 2, i = arguments.length; i > n; n++) this.events.push(arguments[n])
            },
            $addProduct: function (t, e) {
                for (var n = 2, i = arguments.length; i > n; n++) this.products.push(arguments[n])
            }
        }), I.availableTools.sc = m, I.inherit(y, I.BaseTool), I.extend(y.prototype, {
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
        }), I.availableTools.am = y, I.availableTools.adlens = y, I.availableTools.aem = y, I.availableTools.__basic = y, I.inherit(b, I.BaseTool), I.extend(b.prototype, {
            name: "GA",
            initialize: function () {
                var e = this.settings,
                    n = t._gaq,
                    i = e.initCommands || [],
                    a = e.customInit;
                if (n || (_gaq = []), this.isSuppressed()) I.notify("GA: page code not loaded(suppressed).", 1);
                else {
                    if (!n && !b.scriptLoaded) {
                        var r = I.isHttps(),
                            s = (r ? "https://ssl" : "http://www") + ".google-analytics.com/ga.js";
                        e.url && (s = r ? e.url.https : e.url.http), I.loadScript(s), b.scriptLoaded = !0, I.notify("GA: page code loaded.", 1)
                    }
                    var o = (e.domain, e.trackerName),
                        c = T.allowLinker(),
                        u = I.replace(e.account, location);
                    I.settings.domainList || [];
                    _gaq.push([this.cmd("setAccount"), u]), c && _gaq.push([this.cmd("setAllowLinker"), c]), _gaq.push([this.cmd("setDomainName"), T.cookieDomain()]), I.each(i, function (t) {
                        var e = [this.cmd(t[0])].concat(I.preprocessArguments(t.slice(1), location, null, this.forceLowerCase));
                        _gaq.push(e)
                    }, this), a && (this.suppressInitialPageView = !1 === a(_gaq, o)), e.pageName && this.$overrideInitialPageView(null, null, e.pageName)
                }
                this.initialized = !0, I.fireEvent(this.id + ".configure", _gaq, o)
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
                if (T.allowLinker() && t.hostname.match(this.settings.linkerDomains) && !I.isSubdomainOf(t.hostname, location.hostname)) {
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
        }), I.availableTools.ga = b;
        var T = {
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
        I.inherit(S, I.BaseTool), I.extend(S.prototype, {
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
                if (T.allowLinker() && n.allowLinker !== !1 ? this.createAccountForLinker() : this.createAccount(), this.executeInitCommands(), n.customInit) {
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
                T.allowLinker() && (t.allowLinker = !0), this.create(t), this.call("require", "linker"), this.call("linker:autoLink", this.autoLinkDomains(), !1, !0)
            },
            create: function (t) {
                var e = this.settings.trackerSettings;
                e = I.preprocessArguments([e], location, null, this.forceLowerCase)[0], e.trackingId = I.replace(this.settings.trackerSettings.trackingId, location), e.cookieDomain || (e.cookieDomain = T.cookieDomain()), I.extend(e, t || {}), this.call("create", e)
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
        }), I.availableTools.ga_universal = S, I.extend(k.prototype, {
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
        }), I.availableTools.visitor_id = k, _satellite.init({
            tools: {
                "7a6e5ffce8293a2f9ddf48fa88932347": {
                    engine: "sc",
                    loadOn: "pagebottom",
                    euCookie: !1,
                    sCodeURL: "omniture-scode-2.0.5.js",
                    renameS: "s_omntr",
                    initVars: {
                        trackInlineStats: !0,
                        trackDownloadLinks: !1,
                        trackExternalLinks: !1,
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
                            src: "satellite-1.0.1.js"
                        }]
                    }]
                }],
                event: "pagebottom"
            }],
            rules: [{
                name: "content:download link",
                trigger: [{
                    engine: "sc",
                    command: "trackLink",
                    arguments: [{
                        type: "d",
                        linkName: "File Download",
                        customSetup: function (t, e) {
                            var n = this.href,
                                i = n.replace(/\?.*$/, "").replace(/.*\//, "");
                            e.prop60 = i, e.eVar7 = "D=c60", e.events = "event4", e.linkTrackVars = "prop10,eVar10,prop19,eVar22,prop25,eVar25,prop26,eVar8,prop28,eVar28,prop32,eVar32,prop33,eVar33,prop42,eVar42,prop50,eVar48,prop51,eVar49,prop52,eVar50,prop69,eVar69,prop60,eVar7,events", e.linkTrackEvents = "event4"
                        }
                    }]
                }, {
                    command: "delayActivateLink"
                }],
                selector: "a",
                property: {
                    href: /\.(?:avi|css|csv|doc|docx|eps|exe|fla|gif|jpg|js|m4v|mov|mp3|mpg|pdf|png|pps|ppt|pptx|rar|tab|txt|vsd|vxd|wav|wma|wmv||xls|xlsx|xml|zip)($|\&|\?)/i
                },
                event: "click",
                bubbleFireIfParent: !0,
                bubbleFireIfChildFired: !0,
                bubbleStop: !1
            }, {
                name: "content:exit link",
                trigger: [{
                    engine: "sc",
                    command: "trackLink",
                    arguments: [{
                        type: "e",
                        linkName: "Exit Link",
                        customSetup: function (t, e) {
                            var n = this.href;
                            e.prop56 = n, e.linkTrackVars = "prop10,eVar10,prop19,eVar22,prop25,eVar25,prop26,eVar8,prop28,eVar28,prop32,eVar32,prop33,eVar33,prop42,eVar42,prop50,eVar48,prop51,eVar49,prop52,eVar50,prop69,eVar69,prop56"
                        }
                    }]
                }, {
                    command: "delayActivateLink"
                }],
                conditions: [function (t, e) {
                    return _satellite.isOutboundLink(this)
                }],
                selector: "a",
                event: "click",
                bubbleFireIfParent: !0,
                bubbleFireIfChildFired: !0,
                bubbleStop: !1
            }],
            directCallRules: [],
            settings: {
                trackInternalLinks: !0,
                libraryName: "satelliteLib-1.0.7",
                isStaging: !1,
                allowGATTcalls: !1,
                downloadExtensions: /\.(?:doc|docx|eps|jpg|png|svg|xls|ppt|pptx|pdf|xlsx|tab|csv|zip|txt|vsd|vxd|xml|js|css|rar|exe|wma|mov|avi|wmv|mp3|wav|m4v)($|\&|\?)/i,
                notifications: !1,
                utilVisible: !1,
                domainList: ["myherbalife.com"],
                scriptDir: "/Scripts/dtm/",
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
            appVersion: "6ZS",
            buildDate: "2017-01-30 21:28:11 UTC",
            publishDate: "2017-01-30 21:28:10 UTC"
        })
    }(window, document);