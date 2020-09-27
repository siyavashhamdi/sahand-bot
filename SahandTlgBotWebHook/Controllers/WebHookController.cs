using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Telegram.Bot.Types.InputFiles;

namespace SahandTlgBotWebHook.Controllers
{
    public class WebhookController : ApiController
    {
        private static TelegramBotClient _botClient;
        private static string _botToken;

        public WebhookController() : base()
        {
            try
            {
                _botToken = ConfigurationManager.AppSettings["BotToken"];
                _botClient = new TelegramBotClient(_botToken);
            }
            catch (Exception ex)
            {
                Helper.AddLog("MethodName: WebhookController.WebhookController | Desc: Exception caught. ex: " + ex, _botClient);
            }
        }

        public async Task<IHttpActionResult> Post(Update update)
        {
            try
            {
                var message = update.Message;
                var botAdminGroupChatIds = ConfigurationManager.AppSettings["BotAdminGroupChatIds"].Split(';').Where(k => k != "").ToArray();
                var botLogGroupChatIds = ConfigurationManager.AppSettings["BotLogGroupChatIds"].Split(';').Where(k => k != "").ToArray();
                var jsonUpdate = new JavaScriptSerializer().Serialize(update);
                //var jsonUpdateBeautified = JsonConvert.SerializeObject(jsonUpdate, Formatting.Indented).Replace("\\\"", "");
                var jsonUpdateBeautified = Helper.BeautifytJson(jsonUpdate);

                Helper.AddLog("MethodName: WebhookController.Post | Update: " + jsonUpdateBeautified, _botClient);

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (message.Chat.Type == ChatType.Private)
                {
                    var calcTraceCode = DateTime.Now.ToJalaly();
                    calcTraceCode += (message.MessageId % 999 + 1).ToString("000");

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (message.Type)
                    {
                        case MessageType.Text:
                            var text = message.Text;

                            if (text == "/start")
                            {
                                await _botClient.SendTextMessageAsync(message.Chat.Id, $"سلام {message.Chat?.FirstName} {message.Chat?.LastName}\r\nبه بات ثبت نظر خوش آمدید.\r\nکافیه نظرت رو ارسال کنی تا بررسی بشه.", ParseMode.Markdown);
                                return Ok();
                            }

                            if (text.Length < 5)
                            {
                                await _botClient.SendTextMessageAsync(message.Chat.Id, $"نظر شما به ثبت نرسید. طول متن شما کمتر از حد مجاز است.", ParseMode.Markdown, false, false, message.MessageId);
                                return Ok();
                            }

                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"نظر شما با موفقیت به ثبت رسید.\r\n\r\n*کد پیگیری: {calcTraceCode}*", ParseMode.Markdown, false, false, message.MessageId);

                            try
                            {
                                foreach (var botForwardGroupChatId in botAdminGroupChatIds)
                                    await _botClient.SendTextMessageAsync(botForwardGroupChatId, $"نظر جدید با کد پیگیری {calcTraceCode} به ثبت رسید.\r\nمحتوای نظر:\r\n\r\n*«{text}»*\r\n\r\n`{calcTraceCode}_{message.Chat.Id}`", ParseMode.Markdown);
                            }
                            catch {/* Do nothing*/ }

                            break;

                        case MessageType.Photo:
                            var photo = message.Photo.LastOrDefault();

                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"تصویر شما با موفقیت به ثبت رسید.\r\n\r\n*کد پیگیری: {calcTraceCode}*", ParseMode.Markdown, false, false, message.MessageId);

                            try
                            {
                                foreach (var botForwardGroupChatId in botAdminGroupChatIds)
                                    await _botClient.SendPhotoAsync(botForwardGroupChatId, new InputOnlineFile(photo?.FileId), $"تصویر جدید با کد پیگیری {calcTraceCode} به ثبت رسید.{(string.IsNullOrEmpty(message.Caption) ? "" : $"\r\n\r\nکپشن تصویر:\r\n*«{message.Caption}»*")}\r\n\r\n`{calcTraceCode}_{message.Chat.Id}`", ParseMode.Markdown);
                            }
                            catch {/* Do nothing*/ }

                            break;

                        case MessageType.Video:
                            var video = message.Video;

                            if (video.Duration < 3)
                            {
                                await _botClient.SendTextMessageAsync(message.Chat.Id, $"ویدیوی شما به ثبت نرسید. طول کمتر از حد مجاز است.", ParseMode.Markdown, false, false, message.MessageId);
                                return Ok();
                            }

                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"ویدیوی شما با موفقیت به ثبت رسید.\r\n\r\n*کد پیگیری: {calcTraceCode}*", ParseMode.Markdown, false, false, message.MessageId);

                            try
                            {
                                foreach (var botForwardGroupChatId in botAdminGroupChatIds)
                                    await _botClient.SendVideoAsync(botForwardGroupChatId, new InputOnlineFile(video?.FileId), 0, 0, 0, $"ویدیوی جدید با کد پیگیری {calcTraceCode} به ثبت رسید.{(string.IsNullOrEmpty(message.Caption) ? "" : $"\r\n\r\nکپشن ویدیو:\r\n*«{message.Caption}»*")}\r\n\r\n`{calcTraceCode}_{message.Chat.Id}`", ParseMode.Markdown);
                            }
                            catch {/* Do nothing*/ }

                            break;

                        case MessageType.Voice:
                            var voice = message.Voice;

                            if (voice.Duration < 3)
                            {
                                await _botClient.SendTextMessageAsync(message.Chat.Id, $"وُیس شما به ثبت نرسید. طول صدا کمتر از حد مجاز است.", ParseMode.Markdown, false, false, message.MessageId);
                                return Ok();
                            }

                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"وُیس شما با موفقیت به ثبت رسید.\r\n\r\n*کد پیگیری: {calcTraceCode}*", ParseMode.Markdown, false, false, message.MessageId);

                            try
                            {
                                foreach (var botForwardGroupChatId in botAdminGroupChatIds)
                                    await _botClient.SendVoiceAsync(botForwardGroupChatId, new InputOnlineFile(voice?.FileId), $"وُیس جدید با طول {voice.Duration} ثانیه با کد پیگیری {calcTraceCode} به ثبت رسید.{(string.IsNullOrEmpty(message.Caption) ? "" : $"\r\n\r\nکپشن تصویر:\r\n*«{message.Caption}»*")}\r\n\r\n`{calcTraceCode}_{message.Chat.Id}`", ParseMode.Markdown);
                            }
                            catch {/* Do nothing*/ }

                            break;

                        default:
                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"نوع رسانه ارسالی مناسب نیست.", ParseMode.Markdown, false, false, message.MessageId);
                            break;
                    }
                }
                else if (message.Chat.Type == ChatType.Supergroup || message.Chat.Type == ChatType.Group)  // Group must be private with Id and access admin privilege to Bot
                {
                    var me = await _botClient.GetMeAsync();
                    var repliedMsg = message.ReplyToMessage;

                    if (repliedMsg == null || message.ReplyToMessage.From.Id != me.Id)
                        return Ok();

                    var repliedTextLastLine = "";
                    

                    switch (repliedMsg.Type)
                    {
                        case MessageType.Text:
                            repliedTextLastLine = repliedMsg.Text.Split('\n').Last();
                            break;

                        case MessageType.Photo:
                        case MessageType.Video:
                        case MessageType.Voice:
                            repliedTextLastLine = repliedMsg.Caption.Split('\n').Last();
                            break;

                        case MessageType.ChatMembersAdded:
                        case MessageType.ChatMemberLeft:
                            break;

                        default:
                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"نوع رسانه ارسالی مناسب نیست.", ParseMode.Markdown, false, false, message.MessageId);
                            return Ok();
                    }

                    var repliedTextInidicator = repliedTextLastLine.Split('_');
                    var repliedTraceCode = repliedTextInidicator[0];
                    var repliedChatId = repliedTextInidicator[1];

                    switch (message.Type)
                    {
                        case MessageType.Text:
                        case MessageType.Photo:
                        case MessageType.Video:
                        case MessageType.Voice:
                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"پاسخ شما برای کاربر با کد پیگیری {repliedTraceCode} به ثبت رسید و برای ایشان ارسال گردید.", ParseMode.Default, false, false, message.MessageId);
                            break;
                    }

                    var generalReplyMsg = $"کاربر گرامی،\r\nبه پیام ارسالی‌تان با کد پیگیری {repliedTraceCode} پاسخ داده شد.{(string.IsNullOrEmpty(message.Caption) ? "" : $"\r\n\r\nکپشن پاسخ:\r\n*«{message.Caption}»*")}{(string.IsNullOrEmpty(message.Text) ? "" : $"\r\n\r\nمتن پاسخ:\r\n*{message.Text}*")}";
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (message.Type)
                    {
                        case MessageType.Text:
                            await _botClient.SendTextMessageAsync(repliedChatId, generalReplyMsg, ParseMode.Markdown);
                            break;

                        case MessageType.Photo:
                            var photo = message.Photo.LastOrDefault();

                            await _botClient.SendPhotoAsync(repliedChatId, new InputOnlineFile(photo?.FileId), generalReplyMsg, ParseMode.Markdown);
                            break;

                        case MessageType.Video:
                            var video = message.Video;

                            await _botClient.SendVideoAsync(repliedChatId, new InputOnlineFile(video?.FileId), 0, 0, 0, generalReplyMsg, ParseMode.Markdown);
                            break;

                        case MessageType.Voice:
                            var voice = message.Voice;

                            await _botClient.SendVoiceAsync(repliedChatId, new InputOnlineFile(voice?.FileId), generalReplyMsg, ParseMode.Markdown);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.AddLog("MethodName: WebhookController.Post | Desc: Exception caught. ex: " + ex, _botClient);
            }

            return Ok();
        }

        public IHttpActionResult Get()
        {
            Helper.AddLog("MethodName: WebhookController.Get | Desc: Get got!: ", _botClient);
            return Ok("{ result: \"ok\"");
        }
    }
}
