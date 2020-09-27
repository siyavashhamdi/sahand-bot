using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SahandTlgBotWebHook
{
    public static class Helper
    {
        public static void AddLog(string content, TelegramBotClient botClient = null)
        {
            try
            {
                var currPhysLoc = HostingEnvironment.MapPath("/");
                var logFilesDirPath = $"{currPhysLoc}{ConfigurationManager.AppSettings["LogFilesDir"]}";
                var logFileDate = DateTime.Now.ToJalaly();
                var logFilePath = $"{logFilesDirPath}/log-{logFileDate}.txt";

                if (!Directory.Exists(logFilesDirPath))
                    Directory.CreateDirectory(logFilesDirPath);

                var currentDateTime = DateTime.Now.ToJalaly("/", true);
                var modifiedContent = $"{currentDateTime} : {content}\r\n";

                File.AppendAllText(logFilePath, modifiedContent);

                if (botClient == null)
                    return;

                var botLogGroupChatIds = ConfigurationManager.AppSettings["BotLogGroupChatIds"].Split(';').Where(k => k != "").ToArray();

                foreach (var botLogGroupChatId in botLogGroupChatIds)
                    for (var i = 0; i < Math.Ceiling((decimal)modifiedContent.Length / 4000); i++)
                    {
                        var from = 4000 * i;
                        var to = 4000 * (i + 1);

                        botClient.SendTextMessageAsync(botLogGroupChatId, (i == 0 ? "" : ":CONTINUED:\r\n\r\n\r\n") +
                            (4000 * (i + 1) > modifiedContent.Length
                                ? modifiedContent.Substring(@from)
                                : modifiedContent.Substring(@from, to)));
                    }
            }
            catch (Exception ex)
            {
                // Do nothing
            }
        }

        public static string ToJalaly(this DateTime dt, string splitter = "", bool includesTime = false)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
            var pc = new PersianCalendar();

            dt = TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), cstZone);

            var year = pc.GetYear(dt).ToString("0000").Substring(2);
            var month = pc.GetMonth(dt).ToString("00");
            var day = pc.GetDayOfMonth(dt).ToString("00");
            var time = dt.ToString("HH:mm:ss");

            var res = $"{year}{splitter}{month}{splitter}{day}";
            if (includesTime)
                res += $" {time}";

            return res;
        }

        public static string BeautifytJson(string json, string indentString = "    ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(indentString, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(indentString, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(indentString, --indentation)) + ch : ch.ToString()

                select lineBreak ?? (openChar.Length > 1 ? openChar : closeChar);

            return string.Concat(result);
        }
    }
}
