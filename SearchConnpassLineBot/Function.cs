using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using BasicExtension;
using SearchConnpassLineBot.Models;
using SearchConnpassLineBot.Parsers;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SearchConnpassLineBot
{
    public class Function
    {
        public Function()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                Environment.GetEnvironmentVariable("CHANNEL_ACCESS_TOKEN"));
        }

        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            try
            {
                if (input != null)
                {
                    LambdaLogger.Log(input.ToJson());

                    WebHookRequestModel webHookRequestModel = input.Body.ToObject<WebHookRequestModel>();

                    if ((webHookRequestModel != null) && (webHookRequestModel.events != null)
                        && (webHookRequestModel.events.Count > 0))
                    {
                        string replyToken = webHookRequestModel.events[0].ReplyToken;
                        Message message = webHookRequestModel.events[0].Message;
                        string type = message.Type;
                        string id = message.Id;
                        string inputText = message.Text;

                        ReplyToLine(replyToken, inputText);
                    }
                }
            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.Message);
            }

            return CreateResponse();
        }

        private void ReplyToLine(string replyToken, string inputText)
        {
            string stringRequest = CreateReplyRequestModel(replyToken, inputText).ToJson();
            StringContent stringContent = new StringContent(stringRequest, Encoding.UTF8, "application/json");
            _httpClient.PostAsync(_lineMessagingApiUrl, stringContent).Wait();
        }

        private ReplyRequestModel CreateReplyRequestModel(string replyToken, string inputText)
        {
            ReplyRequestModel replyRequestModel = new ReplyRequestModel();
            replyRequestModel.replyToken = replyToken;
            replyRequestModel.messages = CreateMessages(inputText);
            replyRequestModel.notificationDisabled = false;

            return replyRequestModel;
        }

        private List<Message> CreateMessages(string inputText)
        {
            List<Message> messages = new List<Message>();
            List<ConnpassEventModel> eventModels =
                CreateConnpassEventModels(inputText);

            if (eventModels.Count > 0)
            {
                while (eventModels.Count > 0)
                {
                    List<ConnpassEventModel> targetEventModels =
                        eventModels.Take(33).ToList();
                    Message message = CreateMessageModelByEventModels(targetEventModels);
                    messages.Add(message);

                    eventModels = eventModels.Skip(33).ToList();
                }
            }
            else
            {
                Message errorMessage = CreateMessageModelByText("検索結果は0件です。");
                messages.Add(errorMessage);
            }

            return messages;
        }

        private List<ConnpassEventModel> CreateConnpassEventModels(string inputText)
        {
            List<ConnpassEventModel> connpassEventModels = new List<ConnpassEventModel>();

            try
            {
                DateWordParser dateWordParser = new DateWordParser();
                dateWordParser.Parse(inputText,
                    out DateTime? eventDate, out List<string> keywords);

                if (eventDate != null)
                {
                    ConnpassParser connpassParser = new ConnpassParser();
                    connpassEventModels = connpassParser.Parse((DateTime)eventDate, keywords);
                }
            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.Message);
            }

            return connpassEventModels;
        }

        public Message CreateMessageModelByEventModels(List<ConnpassEventModel> eventModels)
        {
            string textMessage = CreateReplyTextByEventModels(eventModels);
            Message message = CreateMessageModelByText(textMessage);

            return message;
        }

        public Message CreateMessageModelByText(string textMessage)
        {
            Message message = new Message();
            message.Type = "text";
            message.Text = textMessage;

            Console.WriteLine(message.Text);

            return message;
        }

        private string CreateReplyTextByEventModels(List<ConnpassEventModel> eventModels)
        {
            StringBuilder sb = new StringBuilder("");

            foreach (ConnpassEventModel eventModel in eventModels)
            {
                sb.Append(eventModel.Title);
                sb.Append("\n");

                sb.Append(eventModel.StartedAt.ToString("yyyy年MM月dd日 HH時mm分〜"));
                sb.Append("\n");

                sb.Append(eventModel.EndedAt.ToString("yyyy年MM月dd日 HH時mm分"));
                sb.Append("\n");

                sb.Append(eventModel.Url);
                sb.Append("\n\n");
                sb.Append("----\n");
            }

            return sb.ToString();
        }

        private APIGatewayProxyResponse CreateResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = null,
                /*
                Body = System.Text.Json.JsonSerializer.Serialize(
                    input, typeof(APIGatewayProxyRequest),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
                */
                IsBase64Encoded = false,
                Headers = new Dictionary<string, string>() { { "Content-Type", "application/json" } }
            };
        }

        // test code
        private List<Message> CreateCoruselMessages(string inputText)
        {
            Message message = new Message();
            message.Type = "text";

            DateWordParser dateWordParser = new DateWordParser();
            dateWordParser.Parse(inputText,
                out DateTime? eventDate, out List<string> keywords);

            if (eventDate != null)
            {
                ConnpassParser connpassParser = new ConnpassParser();
                List<ConnpassEventModel> connpassEventModels =
                    connpassParser.Parse((DateTime)eventDate, keywords);
                if (connpassEventModels.Count > 0)
                {
                    message = CreateCoruselMessage(connpassEventModels);
                }
                else
                {
                    message.Text = "対象のイベントは存在しません。";
                }
            }
            else
            {
                message.Text = "申し訳ございません。入力内容を読み取れませんでした。";
            }

            return new List<Message>() { message };
        }

        // test code
        private Message CreateCoruselMessage(List<ConnpassEventModel> connpassEventModels)
        {
            List<Content> contents = new List<Content>();

            foreach (ConnpassEventModel connpassEventModel in connpassEventModels)
            {
                Content content = new Content()
                {
                    Type = "bubble",
                    Body = new Body()
                    {
                        Type = "box",
                        Layout = "vertical",
                        Contents = new List<TextBox>()
                        {
                            new TextBox()
                            {
                                Type = "text",
                                Text = $"{connpassEventModel.Title}",
                                Wrap = true
                            }
                        }
                    }
                };

                contents.Add(content);
            }

            FlexContainer container = new FlexContainer();
            container.Type = "carousel";
            container.Contents = contents;

            Message message = new Message();
            message.Type = "flex";
            message.AltText = "Alt Text です";
            message.Contents = container;

            return message;
        }

        private readonly HttpClient _httpClient;

        private const string _lineMessagingApiUrl = "https://api.line.me/v2/bot/message/reply";
    }
}
