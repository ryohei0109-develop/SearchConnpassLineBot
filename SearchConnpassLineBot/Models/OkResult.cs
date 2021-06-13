using System;
using BasicExtension;

namespace SearchConnpassLineBot.Models
{
    public class OkResult
    {
        public OkResult()
        {
            statusCode = 200;
            body = new Test().ToJson();
        }

        public int statusCode
        {
            get;
            set;
        }

        public string headers
        {
            get;
            set;
        }

        public string body
        {
            get;
            set;
        }

        public class Test
        {
            public Test()
            {
                Result = "OK2";
            }

            public string Result
            {
                get;
                set;
            }
        }
    }
}
