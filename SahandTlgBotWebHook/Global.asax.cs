using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Routing;
using Telegram.Bot;

namespace SahandTlgBotWebHook
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                Helper.AddLog("MethodName: WebApiApplication.Application_Start | Desc: Start of application");

                GlobalConfiguration.Configure(WebApiConfig.Register);
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                var botToken = ConfigurationManager.AppSettings["BotToken"];
                var webHookUrl = ConfigurationManager.AppSettings["WebHookUrl"];
                var _botClient = new TelegramBotClient(botToken);

                _botClient.SetWebhookAsync(webHookUrl).Wait();

                Helper.AddLog("MethodName: WebApiApplication.Application_Start | Desc: Start of application is successful", _botClient);
            }
            catch (Exception ex)
            {
                Helper.AddLog("MethodName: WebApiApplication.Application_Start | Desc: Exception caught. ex: " + ex);
            }
        }
    }
}
