﻿using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// An organizational unit.
    /// </summary>
    public class Org : Entity, IKeyable<int>
    {
        public static readonly Org Empty = new Org();

        public const short
            TYP_VTL = 0b00000, // virtual
            TYP_PRT = 0b01000, // parent
            TYP_SHP = 0b00001, // shop
            TYP_SRC = 0b00010, // source
            TYP_DST = 0b00100, // distributor
            TYP_MKT = TYP_PRT | TYP_SHP, // market
            TYP_ZON = TYP_PRT | TYP_SRC, // zone
            TYP_CTR = TYP_PRT | TYP_SRC | TYP_DST; // center

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_SHP, "商户"},
            {TYP_SRC, "产源"},
#if ZHNT
            {TYP_MKT, "市场"},
#else
            {TYP_SHP, "驿站"},
#endif
            {TYP_ZON, "供区"},
            {TYP_CTR, "控运"},
        };

        // id
        internal int id;

        // parent id, only if shop or source
        internal int prtid;

        // center id, only if market or shop
        internal int ctrid;

        internal string license;
        internal short regid;
        internal string addr;
        internal double x;
        internal double y;

        internal string tel;
        internal string link;
        internal bool trust;

        internal int mgrid; // supervisor id
        internal string mgrname;
        internal string mgrtel;
        internal string mgrim;
        internal string alias;
        internal bool icon;
        internal JObj specs;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(prtid), ref prtid);
                s.Get(nameof(ctrid), ref ctrid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(alias), ref alias);
                s.Get(nameof(license), ref license);
                s.Get(nameof(regid), ref regid);
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(link), ref link);
                s.Get(nameof(trust), ref trust);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(mgrid), ref mgrid);
                s.Get(nameof(mgrname), ref mgrname);
                s.Get(nameof(mgrtel), ref mgrtel);
                s.Get(nameof(mgrim), ref mgrim);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(specs), ref specs);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                if (prtid > 0) s.Put(nameof(prtid), prtid);
                else s.PutNull(nameof(prtid));

                if (ctrid > 0) s.Put(nameof(ctrid), ctrid);
                else s.PutNull(nameof(ctrid));
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(alias), alias);
                s.Put(nameof(license), license);
                if (regid > 0) s.Put(nameof(regid), regid);
                else s.PutNull(nameof(regid));
                s.Put(nameof(addr), addr);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
                s.Put(nameof(tel), tel);
                s.Put(nameof(link), link);
                s.Put(nameof(trust), trust);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(mgrid), mgrid);
                s.Put(nameof(mgrname), mgrname);
                s.Put(nameof(mgrtel), mgrtel);
                s.Put(nameof(mgrim), mgrim);
                s.Put(nameof(icon), icon);
                s.Put(nameof(specs), specs);
            }
        }


        public int Key => id;

        public string Tel => tel;

        public string Im => mgrim;

        public int MarketId => IsMarket ? id : IsOfShop ? prtid : 0;

        public int ZoneId => IsZone ? id : IsOfSource ? prtid : 0;

        public bool IsParentCapable => (typ & TYP_PRT) == TYP_PRT;

        public bool IsLink => typ == TYP_VTL;

        public bool IsZone => typ == TYP_ZON;

        public bool IsOfZone => (typ & TYP_ZON) == TYP_ZON;

        public bool IsSource => typ == TYP_SRC;

        public bool IsOfSource => (typ & TYP_SRC) == TYP_SRC;

        public bool IsShop => typ == TYP_SHP;

        public bool IsOfShop => (typ & TYP_SHP) == TYP_SHP;

        public bool IsMarket => typ == TYP_MKT;

        public bool IsCenter => typ == TYP_CTR;

        public bool HasXy => IsMarket || IsSource || IsCenter;

        public bool HasCtr => IsOfShop;

        public string ShopName => IsMarket ? alias : name;

        public override string ToString() => name;
    }
}