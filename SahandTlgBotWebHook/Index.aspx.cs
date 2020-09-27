using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SahandTlgBotWebHook.Controllers;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace SahandTlgBotWebHook
{
    public partial class Index : System.Web.UI.Page
    {
        private static TelegramBotClient _botClient;
        private static bool _isOnUpdateEventAdded;

        public Index()
        {
            return;

            Helper.AddLog("MethodName: Index.Index | Desc: Index.aspx page is called. ");

            var botToken = ConfigurationManager.AppSettings["BotToken"];

            _botClient = new TelegramBotClient(botToken);
            _isOnUpdateEventAdded = false;

            _botClient.DeleteWebhookAsync();

            Helper.AddLog("MethodName: Index.Index | Desc: Index.aspx page is called successfully. ", _botClient);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            return;

            var threadOnUpdateReceived = new Thread(DoWork);
            threadOnUpdateReceived.Start();
        }

        private static void DoWork()
        {
            if (_isOnUpdateEventAdded)
                return;

            _botClient.OnUpdate += BotOnUpdateReceived;
            _botClient.StartReceiving();
            _isOnUpdateEventAdded = true;
        }

        private static async void BotOnUpdateReceived(object sender, UpdateEventArgs e)
        {
            return;

            await new WebhookController().Post(e.Update);
            return;

            var botForwardGroupChatIds = ConfigurationManager.AppSettings["BotAdminGroupChatIds"].Split(';').ToArray();
            var message = e.Update.Message;

            // 355250101
            var me = await _botClient.GetMeAsync();
            await _botClient.SendTextMessageAsync(message.Chat.Id, $"به ییام کاربر با کد پیگیری {message.ReplyToMessage.From.Id}_{me.Id}", ParseMode.Markdown, false, false);



            if (message == null || message.Type != MessageType.Text)
                return;

            var text = message.Text;

            await _botClient.SendTextMessageAsync(message.Chat.Id, "_Recieved Update._: " + text, ParseMode.Markdown);
        }
    }
}
