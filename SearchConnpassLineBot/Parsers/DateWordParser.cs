using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace SearchConnpassLineBot.Parsers
{
    public class DateWordParser
    {
        public DateWordParser()
        {
            _ymdRegex = new Regex(@"([０-９\d]{4}).([０-９\d]{1,2}).([０-９\d]{1,2})");
            _mdRegex = new Regex(@"([０-９\d]{1,2}).([０-９\d]{1,2})");
        }

        public void Parse(string input,
            out DateTime? eventDate, out List<string> keywords)
        {
            eventDate = null;
            keywords = new List<string>();

            try
            {
                string keyword = input;
                eventDate = ParseEvnetDate(input, ref keyword);
                if (eventDate != null)
                {
                    keywords = SplitKeyword(keyword);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private List<string> SplitKeyword(string keyword)
        {
            List<string> keywords = new List<string>();

            if (string.IsNullOrEmpty(keyword) == false)
            {
                keywords = keyword.Trim()
                    .Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => t != " ").Where(t => t != "　").ToList();
            }

            return keywords;
        }

        private DateTime? ParseEvnetDate(string input,
            ref string keyword)
        {
            DateTime? eventDate = CheckByDateKeywords(input, ref keyword);

            if (eventDate == null)
            {
                eventDate = CheckByRegex(input, ref keyword);
            }

            return eventDate;
        }

        private DateTime? CheckByDateKeywords(string input, ref string keyword)
        {
            DateTime? eventDate = null;
            Dictionary<int, List<string>> checkDictionary = new Dictionary<int, List<string>>();
            checkDictionary.Add(0, new List<string>() { "今日", "本日", "きょう", "本日" });
            checkDictionary.Add(1, new List<string>() { "明日", "あした" });
            checkDictionary.Add(2, new List<string>() { "明後日", "あさって" });

            foreach (int addDay in checkDictionary.Keys)
            {
                eventDate = CheckByDateKeyword(input, addDay, checkDictionary[addDay], ref keyword);
                if (eventDate != null)
                {
                    break;
                }
            }

            return eventDate;
        }

        private DateTime? CheckByDateKeyword(string input, int addDay, List<string> values,
            ref string keyword)
        {
            DateTime? eventDate = null;

            foreach (string value in values)
            {
                if (input.Contains(value))
                {
                    eventDate = JpNow().AddDays(addDay);
                    keyword = input.Replace(value, "");

                    break;
                }
            }

            return eventDate;
        }

        private DateTime? CheckByRegex(string input, ref string keyword)
        {
            DateTime? eventDate = null;

            if (_ymdRegex.IsMatch(input) == true)
            {
                Match match = _ymdRegex.Match(input);
                eventDate = ParseDateTime(match.Groups[1].Value,
                    match.Groups[2].Value,
                    match.Groups[3].Value);

                keyword = _ymdRegex.Replace(input, "");
            }
            else if (_mdRegex.IsMatch(input) == true)
            {
                Match match = _mdRegex.Match(input);
                eventDate = ParseDateTime(JpNow().Year.ToString(),
                    match.Groups[1].Value,
                    match.Groups[2].Value);

                keyword = _mdRegex.Replace(input, "");
            }

            return eventDate;
        }

        private DateTime JpNow()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TIMEZONE_JST).DateTime;
        }

        private DateTime ParseDateTime(string strYear, string strMonth, string strDay)
        {
            int year = ZenToHanNum(strYear);
            int month = ZenToHanNum(strMonth);
            int day = ZenToHanNum(strDay);

            return new DateTime(year, month, day);
        }

        private int ZenToHanNum(string s)
        {
            return int.Parse(Regex.Replace(s, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString()));
        }

        private readonly TimeZoneInfo TIMEZONE_JST =
            TZConvert.GetTimeZoneInfo("Tokyo Standard Time");

        private readonly Regex _ymdRegex;

        private readonly Regex _mdRegex;
    }
}
