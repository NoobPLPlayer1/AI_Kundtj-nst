using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BotClient
{

    public class Root
    {
        public string channelId { get; set; } = "emulator";
        public Conversation Conversation { get; set; }
        public string ID { get; set; }
        public string InputHint { get; set; }
        public DateTime LocalTimestamp { get; set; }
        public string Locale { get; set; } = "en-US";
        public string ReplyToId { get; set; }
        public string ServiceUrl { get; set; }
        public string Text { get; set; }
        public DateTime timestamp { get; set; } = DateTime.Now;
        public string Type { get; set; } = "message";
    }

    public struct Conversation
    {
        public string ID { get; set; }
    }
}
