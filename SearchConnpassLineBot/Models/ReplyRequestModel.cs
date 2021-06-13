using System;
using System.Collections.Generic;

namespace SearchConnpassLineBot.Models
{
    public class ReplyRequestModel
    {
        public string replyToken
        {
            get;
            set;
        }

        public List<Message> messages
        {
            get;
            set;
        }

        /// <summary>
        /// true: ユーザに通知されない(デフォルト)
        /// false: ユーザに通知される
        /// </summary>
        public bool notificationDisabled
        {
            get;
            set;
        }
    }
}
