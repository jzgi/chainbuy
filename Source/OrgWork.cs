using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Fabric.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainMart
{
    public abstract class OrgWork<V> : WebWork where V : OrgVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    public class PublyOrgWork : OrgWork<PublyOrgVarWork>
    {
    }

    [Ui("管理下级机构", "业务")]
    public class AdmlyOrgWork : OrgWork<AdmlyOrgVarWork>
    {
#if ZHNT
        [Ui("市场", group: 1), Tool(Anchor)]
#else
        [Ui("驿站", group: 1), Tool(Anchor)]
#endif
        public async Task @default(WebContext wc)
        {
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = ").T(Org.TYP_MKT).T(" ORDER BY regid, status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 1);

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else
                        h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/mktly/", o.Key, "/", icon: "link", disabled: !prin.CanDive(o))._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, false, 15);
        }

        [Ui("供区", group: 2), Tool(Anchor)]
        public async Task zon(WebContext wc)
        {
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ IN (").T(Org.TYP_ZON).T(",").T(Org.TYP_CTR).T(") ORDER BY typ, status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 2);

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/zonly/", o.Key, "/", icon: "link", disabled: !prin.CanDive(o))._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, false, 15);
        }

        [Ui("新建", "新建下级机构", icon: "plus", group: 7), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int cmd)
        {
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var orgs = Grab<int, Org>();

            if (wc.IsGet)
            {
                var m = new Org
                {
                    typ = cmd == 1 ? Org.TYP_MKT : Org.TYP_ZON,
                    created = DateTime.Now,
                    creator = prin.name,
                    state = Entity.STA_NORMAL
                };

                m.Read(wc.Query, 0);
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(cmd == 1
                        ?
#if ZHNT
                        "填写市场资料"
#else
                        "填写驿站资料"
#endif
                        : "填写供区资料"
                    );
                    if (cmd == 2)
                    {
                        h.LI_().SELECT("机构类型", nameof(m.typ), m.typ, Org.Typs, filter: (k, v) => k >= 10, required: true)._LI();
                    }
                    h.LI_().TEXT("常用名", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商登记名", nameof(m.fully), m.fully, max: 20, required: true)._LI();
                    h.LI_().SELECT(m.EqMarket ? "场区" : "省份", nameof(m.regid), m.regid, regs, filter: (k, v) => m.EqMarket ? v.IsSection : v.IsProvince, required: !m.EqZone)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 30)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().SELECT("关联中控", nameof(m.ctrid), m.ctrid, orgs, filter: (k, v) => v.EqCenter, required: true)._LI();
                    h.LI_().SELECT("状态", nameof(m.state), m.state, Entity.States, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var o = await wc.ReadObjectAsync(Entity.MSK_BORN, new Org
                {
                    typ = (short) (cmd == 1 ? Org.TYP_MKT : 0),
                    created = DateTime.Now,
                    creator = prin.name,
                });
                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, Entity.MSK_BORN)._VALUES_(Org.Empty, Entity.MSK_BORN);
                await dc.ExecuteAsync(p => o.Write(p, Entity.MSK_BORN));
                wc.GivePane(201); // created
            }
        }
    }

    [OrglyAuthorize(Org.TYP_ZON, 1)]
    [Ui("管理下级产源", "供区")]
    public class ZonlyOrgWork : OrgWork<ZonlyOrgVarWork>
    {
        [Ui("下级产源"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 ORDER BY status DESC, name");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));

            wc.GivePane(200, h =>
            {
                h.TOOLBAR();

                if (arr == null) return;

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/zonly/", o.Key, "/", icon: "link", disabled: !prin.CanDive(o))._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, false, 15);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("新建", "新建下级产源", icon: "plus"), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var zon = wc[-1].As<Org>();
            var prin = (User) wc.Principal;
            var regs = Grab<short, Reg>();
            var m = new Org
            {
                typ = Org.TYP_SRC,
                prtid = zon.id,
                created = DateTime.Now,
                creator = prin.name,
                state = Entity.STA_NORMAL
            };
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("下级产源属性");

                    h.LI_().TEXT("常用名", nameof(m.name), m.name, max: 12, required: true)._LI();
                    h.LI_().TEXT("工商登记名", nameof(m.fully), m.fully, max: 20, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().SELECT("省份", nameof(m.regid), m.regid, regs, filter: (k, v) => v.IsProvince, required: true)._LI();
                    h.LI_().TEXT("联系地址", nameof(m.addr), m.addr, max: 30)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.0000, max: 180.0000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().CHECKBOX("委托代办", nameof(m.trust), true, m.trust)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                var o = await wc.ReadObjectAsync(msk, instance: m);

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(201); // created
            }
        }
    }

    [OrglyAuthorize(Org.TYP_MKT, 1)]
#if ZHNT
    [Ui("管理下级商户", "市场")]
#else
    [Ui("管理下级商户", "驿站")]
#endif
    public class MktlyOrgWork : OrgWork<MktlyOrgVarWork>
    {
        [Ui("下属商户", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 ORDER BY id DESC LIMIT 20 OFFSET @2 * 20");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Org.TYP_SHP);

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/-", Org.TYP_SHP, MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    if (o.icon)
                    {
                        h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    }
                    else h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/mktly/", o.Key, "/", icon: "link", disabled: !prin.CanDive(o))._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });

                h.PAGINATION(arr?.Length == 20);
            }, false, 15);
        }

        [Ui(tip: "查询", icon: "search", group: 2), Tool(AnchorPrompt)]
        public async Task search(WebContext wc)
        {
            var regs = Grab<short, Reg>();
            var prin = (User) wc.Principal;

            bool inner = wc.Query[nameof(inner)];
            short regid = 0;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.RADIOSET(nameof(regid), regid, regs, filter: v => v.IsSection);
                    h._FORM();
                });
            }
            else // OUTER
            {
                regid = wc.Query[nameof(regid)];

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE regid = @1");
                var arr = await dc.QueryAsync<Org>(p => p.Set(regid));

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();

                    h.MAINGRID(arr, o =>
                    {
                        h.ADIALOG_(o.Key, "/-", o.typ, MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                        if (o.icon)
                        {
                            h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                        }
                        else h.PIC("/void.webp", css: "uk-width-1-5");

                        h.ASIDE_();
                        h.HEADER_().H5(o.name).SPAN("")._HEADER();
                        h.Q(o.tip, "uk-width-expand");
                        h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/mktly/", o.Key, "/", icon: "link", disabled: !prin.CanDive(o))._SPAN()._FOOTER();
                        h._ASIDE();

                        h._A();
                    });
                }, false, 15);
            }
        }

        [Ui(tip: "品牌链接", icon: "star", group: 4), Tool(Anchor)]
        public async Task star(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND typ = 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Org.TYP_BRD);

                h.MAINGRID(arr, o =>
                {
                    h.ADIALOG_(o.Key, "/-", Org.TYP_BRD, MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                    if (o.icon) h.PIC_("uk-width-1-5").T(MainApp.WwwUrl).T("/org/").T(o.id).T("/icon")._PIC();
                    else h.PIC("/void.webp", css: "uk-width-1-5");

                    h.ASIDE_();
                    h.HEADER_().H5(o.name).SPAN("")._HEADER();
                    h.Q(o.tip, "uk-width-expand");
                    h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/mktly/", o.Key, "/", icon: "link", disabled: !prin.CanDive(o))._SPAN()._FOOTER();
                    h._ASIDE();

                    h._A();
                });
            }, false, 15);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("新建", "新建下属商户", icon: "plus", group: 1 | 4), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User) wc.Principal;

            var regs = Grab<short, Reg>();
            var o = new Org
            {
                typ = (short) typ,
                created = DateTime.Now,
                creator = prin.name,
                state = Entity.STA_NORMAL,
                prtid = org.id,
                ctrid = org.ctrid,
            };

            if (wc.IsGet)
            {
                if (typ == Org.TYP_SHP)
                {
                    o.Read(wc.Query, 0);
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("填写商户资料");

                        h.LI_().TEXT("常用名", nameof(o.name), o.name, max: 12, required: true)._LI();
                        h.LI_().TEXT("工商登记名", nameof(o.fully), o.fully, max: 20, required: true)._LI();
                        h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                        h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                        h.LI_().SELECT("场区", nameof(o.regid), o.regid, regs, filter: (k, v) => v.IsSection)._LI();
                        h.LI_().CHECKBOX("委托办理", nameof(o.trust), true, o.trust)._LI();
#if ZHNT
                        h.LI_().TEXT("场地编号", nameof(o.addr), o.addr, max: 4)._LI();
#else
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
#endif
                        h._FIELDSUL()._FORM();
                    });
                }
                else // TYP_VTL
                {
                    o.Read(wc.Query, 0);
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("填写虚拟商户信息");

                        h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12, required: true)._LI();
                        h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                        h.LI_().TEXT("链接地址", nameof(o.addr), o.addr, max: 50)._LI();
                        h.LI_().SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k >= 0)._LI();

                        h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
                    });
                }
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                await wc.ReadObjectAsync(msk, instance: o);

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(201); // created
            }
        }
    }
}