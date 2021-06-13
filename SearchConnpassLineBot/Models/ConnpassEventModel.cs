using System;
namespace SearchConnpassLineBot.Models
{
    public class ConnpassEventModel
    {
        public string Title {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public DateTime StartedAt
        {
            get;
            set;
        }

        public DateTime EndedAt
        {
            get;
            set;
        }
    }
}
