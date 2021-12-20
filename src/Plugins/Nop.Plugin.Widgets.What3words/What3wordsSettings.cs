using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.What3words
{
    public class What3wordsSettings : ISettings
    {
        public string ApiKey { get; set; }

        public bool Enabled { get; set; }

    }
}