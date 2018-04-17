using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace Wox.Plugin.PirateBay
{
    public class Main : IPlugin
    {
        private const string APISEARCH = "https://www.thepiratebay.org/search/";
        private const int MAXRESULTS = 20;

        public void Init(PluginInitContext context)
        {
        }

        public List<Result> Query(Query query)
        {
            List<Result> mainList = new List<Result>();
            if (query.FirstSearch.Length > 0)
            {
                string address = APISEARCH + query.Search;
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        int num1 = 0;
                        string pgsrc = webClient.DownloadString(APISEARCH + query.Search);
                        pgsrc = pgsrc.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                        MatchCollection results = Regex.Matches(pgsrc, "<tr.*?title=\"Details for (.*?)\".*?<a href=\"magnet:\\?(.*?)\".*?, Size (.*?),.*?<td align=\"right\">(.*?)<\\/td><td align=\"right\">(.*?)<\\/td>.*?<\\/tr>");
                        foreach (Match m in results)
                        {
                            if (num1 < MAXRESULTS)
                            {
                                string itemName = m.Groups[1].ToString();
                                string itemMagnet = "magnet:?" + m.Groups[2].ToString();

                                string itemSize = m.Groups[3].ToString().Replace("&nbsp;","");

                                string itemSeeds = m.Groups[4].ToString();
                                string itemLeech = m.Groups[5].ToString();

                                string subString = String.Format("Seeders: {0} Leechers: {1} Size: {2}", itemSeeds, itemLeech, itemSize);

                                List<Result> tempList = mainList;
                                Result torrent = new Result();
                                torrent.Title = itemName;
                                torrent.SubTitle = subString;
                                torrent.IcoPath = "Images\\magnet.png";
                                torrent.Action= ((Func<ActionContext, bool>)(e =>
                                {
                                    Process.Start(itemMagnet);
                                    return true;
                                }));

                                Result result2 = torrent;
                                tempList.Add(result2);
                                ++num1;
                            }
                        }

                        if (num1 == 0)
                        {
                            List<Result> list2 = mainList;
                            Result result1 = new Result();
                            result1.Title=("No results");
                            result1.SubTitle=("");
                            result1.IcoPath=("Images\\logo.png");
                            result1.Action=((Func<ActionContext, bool>)(e => false));
                            Result result2 = result1;
                            list2.Add(result2);
                        }
                    }
                    catch (Exception ex)
                    {
                        List<Result> list2 = mainList;
                        Result result1 = new Result();
                        result1.Title=("Error");
                        result1.SubTitle=(ex.ToString());
                        result1.IcoPath=("Images\\logo.png");
                        result1.Action=((Func<ActionContext, bool>)(e => false));
                        Result result2 = result1;
                        list2.Add(result2);
                    }
                }
            }
            return mainList;
        }
    }
}
