using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SearchConnpassLineBot.Models
{
    public class WebHookRequestModel
    {
        [JsonProperty("events")]
        public List<Event> events { get; set; }
    }

    public class Event
    {
        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("replyToken")]
        public string ReplyToken { get; set; }
    }

    public class Message
    {
        /// <summary>
        /// text: テキスト
        /// flex: FlexMessage
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("altText")]
        public string AltText { get; set; }

        /// <summary>
        /// FlexMessageのコンテナ
        /// </summary>
        [JsonProperty("contents")]
        public FlexContainer Contents { get; set; }
    }

    public class FlexContainer
    {
        /// <summary>
        /// bubble: バブル
        /// carousel: カルーセル
        /// </summabubblery>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("contents")]
        public List<Content> Contents { get; set; }
    }

    public class Content
    {
        /// <summary>
        /// bubble
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }
    }

    public class Body
    {
        /// <summary>
        /// box
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// vertical
        /// </summary>
        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("contents")]
        public List<TextBox> Contents { get; set; }
    }

    public class TextBox
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("wrap")]
        public bool Wrap { get; set; }
    }
}
