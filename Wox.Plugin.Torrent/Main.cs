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
                        int numTorrents = 0;
                        //Get the HTML of pirate bay... we're going to parse it with regex...
                        string pbHTML = webClient.DownloadString(APISEARCH + query.Search);
                        //Fix some formatting
                        pbHTML = pbHTML.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                        //Collection of results, it takes the title, magnet, and details about activity.
                        MatchCollection results = Regex.Matches(pbHTML, 
                            "<tr.*?title=\"Details for (.*?)\".*?<a href=\"magnet:\\?(.*?)\".*?, Size (.*?),.*?<td align=\"right\">(.*?)<\\/td><td align=\"right\">(.*?)<\\/td>.*?<\\/tr>");
                        foreach (Match m in results)
                        {
                            //only show MAXRESULTS number of torrents. TODO: Make this a Wox based setting
                            if (numTorrents < MAXRESULTS)
                            {
                                string itemName = m.Groups[1].ToString();
                                //We ruined the formating of the magnet, to get our torrent client to open we need to reformat it.
                                string itemMagnet = "magnet:?" + m.Groups[2].ToString();

                                string itemSize = m.Groups[3].ToString().Replace("&nbsp;","");

                                string itemSeeds = m.Groups[4].ToString();
                                string itemLeech = m.Groups[5].ToString();

                                //the final information
                                string subString = String.Format("Seeders: {0} Leechers: {1} Size: {2}", itemSeeds, itemLeech, itemSize);

                                //not sure why this is so complicated
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

                                //why do I make a new result? take this out and double check it works....
                                Result result2 = torrent;
                                tempList.Add(result2);
                                ++numTorrents;
                            }
                        }

                        //There wasn't anything to be found (This isn't an error! It should not be treated as such)
                        if (numTorrents == 0)
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
                    //Display the error if we got one (Usually this means you had trouble with connecting to TPB)
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
