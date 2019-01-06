using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IEP_Auction.Startup))]
namespace IEP_Auction
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
