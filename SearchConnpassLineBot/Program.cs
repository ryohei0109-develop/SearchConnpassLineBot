using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using BasicExtension;
using SearchConnpassLineBot.Models;

namespace SearchConnpassLineBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // test code
                // RunTest();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void RunTest()
        {
            WebHookRequestModel webHookRequestModel = new WebHookRequestModel();
            webHookRequestModel.events = new List<Event>()
                {
                    new Event()
                    {
                        Message = new Message()
                        {
                            Type = "",
                            Text = "今日"
                        },
                    }
                };

            APIGatewayProxyRequest aPIGatewayProxyRequest = new APIGatewayProxyRequest();
            aPIGatewayProxyRequest.Body = webHookRequestModel.ToJson();

            var hoge = aPIGatewayProxyRequest.Body;
            Console.WriteLine(aPIGatewayProxyRequest.Body.ToJson());

            Console.WriteLine(webHookRequestModel.ToJson());

            Function function = new Function();
            function.FunctionHandler(aPIGatewayProxyRequest, null);
        }
    }
}
