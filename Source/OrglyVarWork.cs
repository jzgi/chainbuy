﻿using ChainFx.Web;

namespace ChainBuy
{
    public abstract class OrglyVarWork : WebWork
    {
        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBARXL_();

                bool astack = wc.Query[nameof(astack)];
                if (astack)
                {
                    h.T("<a class=\"uk-icon-button\" href=\"javascript: window.parent.closeUp(false);\" uk-icon=\"icon: chevron-left; ratio: 1.75\"></a>");
                }

                string rol = wc.Dive ? "代" + User.Orgly[wc.Role] : User.Orgly[wc.Role];

                h.HEADER_("uk-width-expand uk-col uk-padding-left");
                h.H2(org.name);
                if (org.IsParent) h.H4(org.Ext);
                h.P2(prin.name, rol, brace: true);
                h._HEADER();

                if (org.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/org/", org.id, "/icon", circle: true, css: "uk-width-small");
                }
                else
                    h.PIC(org.IsShop ? "/shp.webp" : org.EqCenter ? "/ctr.webp" : "/src.webp", circle: true, css: "uk-width-small");

                h._TOPBARXL();

                h.WORKBOARD(notice: org.id);

                // qrcode
                if (org.IsShop)
                {
                    h.NAV_(css: "uk-col uk-margin uk-flex-middle").QRCODE(MainApp.WwwUrl + "/" + org.MarketId + "/", css: " uk-width-small").SPAN("推荐市场")._NAV();
                }
            }, false, 30, title: org.name);
        }
    }


    [OrglyAuthorize(Org.TYP_SRC, 1)]
    [Ui("供区产源操作")]
    public class SrclyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access", false); // false = source

            CreateWork<OrglyAssetWork>("asset");

            CreateWork<OrglyCreditWork>("credit");

            CreateWork<PtylyBookClearWork>("bookclr", state: true); // true = is org

            // src

            CreateWork<SrclyLotWork>("slot");

            CreateWork<SrclyBookWork>("sbook");

            CreateWork<SrclyBookAggWork>("sbookldg");

            // zon

            CreateWork<ZonlyOrgWork>("zorg");

            CreateWork<ZonlyTestWork>("ztest");

            // ctr

            CreateWork<CtrlyLotWork>("clot");

            CreateWork<CtrlyBookAggWork>("cbookrpt");

            CreateWork<CtrlyBookWork>("cbook");
        }
    }


    [OrglyAuthorize(Org.TYP_SHP, 1)]
    [Ui("市场操作")]
    public class ShplyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // org

            CreateWork<OrglySetgWork>("setg");

            CreateWork<OrglyAccessWork>("access", true); // true = shop

            CreateWork<OrglyAssetWork>("asset");

            CreateWork<OrglyCreditWork>("credit");

            CreateWork<PtylyBuyClearWork>("buyclr", state: true);

            // shp

            CreateWork<ShplyItemWork>("sitem");

            CreateWork<ShplyVipWork>("svip");

            CreateWork<ShplyBuyWork>("sbuy");

            CreateWork<ShplyPosWork>("spos");

            CreateWork<ShplyBuyAggWork>("sbuyagg");

            CreateWork<ShplyBookWork>("sbook");


            // mkt

            CreateWork<MktlyOrgWork>("morg");

            CreateWork<MktlyTestWork>("mtest");

            CreateWork<MktlyBuyWork>("mbuy");

            CreateWork<MktlyBookWork>("mbook");
        }
    }
}