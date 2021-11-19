using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.Item;

namespace Revital
{
    public abstract class PostWork : WebWork
    {
    }

    public class PublyPostWork : PostWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyPostVarWork>();
        }

        public void @default(WebContext wc, int page)
        {
        }
    }

    public abstract class BizlyPostWork : PostWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyPostVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Post.Empty).T(" FROM posts WHERE orgid = @1 ORDER BY status DESC");
            var arr = await dc.QueryAsync<Post>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.name, o.tip)._TD();
                    h.TD(o.price, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [Ui("货架管理", forkie: TYP_AGRI)]
    public class AgriBizlyPostWork : BizlyPostWork
    {
    }

    [Ui("服务管理", forkie: TYP_DIET)]
    public class DietBizlyPostWork : BizlyPostWork
    {
    }
}