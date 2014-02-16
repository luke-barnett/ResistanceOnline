using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ResistanceOnline.Site.App_Start.SignalR))]

namespace ResistanceOnline.Site.App_Start
{
    public class SignalR
    {
        public void Configuration(IAppBuilder app)
        {
            // Map all hubs to "/signalr"
            app.MapSignalR();
            // Map the Echo PersistentConnection to "/echo"
            //app.MapSignalR<echoconnection>("/echo");
        }
    }
}