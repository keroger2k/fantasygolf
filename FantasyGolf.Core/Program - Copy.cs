//using FantasyGolf.Core.Models;
//using HtmlAgilityPack;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace FantasyGolf.Core
//{
//    public class Program
//    {
//        public static int[] YEARS = { 2016, 2015, 2014, 2013, 2012 };


//        public static void Main(string[] args)
//        {
//            var db = new Database();
//            foreach (var year in YEARS)
//            {
//                foreach (var tournament in db.Tournaments)
//                {
//                    var client = new HttpClient();
//                    var doc = new HtmlDocument();
//                    var playerList = new List<TournamentScrape>();
//                    var tournamentResultList = new List<TournamentResult>();
//                    string currentUrl = string.Format(tournament.Url, year);

//                    var response = client.GetAsync(currentUrl).Result;
//                    response.EnsureSuccessStatusCode();

//                    doc.Load(response.Content.ReadAsStreamAsync().Result);
//                    var playerRows = doc.DocumentNode.SelectNodes("//tr");
//                    for (var i = 3; i < playerRows.Count; i++)
//                    {
//                        var playerRow = playerRows.ElementAt(i);
//                        var ts = new TournamentScrape();
//                        ts.Player = playerRow.ChildNodes.ElementAt(1).InnerText;
//                        ts.Position = playerRow.ChildNodes.ElementAt(3).InnerText;
//                        ts.Total = playerRow.ChildNodes.ElementAt(playerRow.ChildNodes.Count - 6).InnerText;
//                        ts.Money = playerRow.ChildNodes.ElementAt(playerRow.ChildNodes.Count - 4).InnerText;

//                        if (playerRow.ChildNodes.Count == 19)  //full 4 rounds
//                        {
//                            ts.R1 = playerRow.ChildNodes.ElementAt(5).InnerText;
//                            ts.R2 = playerRow.ChildNodes.ElementAt(7).InnerText;
//                            ts.R3 = playerRow.ChildNodes.ElementAt(9).InnerText;
//                            ts.R4 = playerRow.ChildNodes.ElementAt(11).InnerText;
//                        }
//                        else if (playerRow.ChildNodes.Count == 17)  //only 3 rounds
//                        {
//                            ts.R1 = playerRow.ChildNodes.ElementAt(5).InnerText;
//                            ts.R2 = playerRow.ChildNodes.ElementAt(7).InnerText;
//                            ts.R3 = playerRow.ChildNodes.ElementAt(9).InnerText;

//                        }
//                        playerList.Add(ts);
//                    }

//                    foreach (var player in playerList)
//                    {
//                        var tr = new TournamentResult();
//                        var tmp = player.Player.Split(' ');
//                        var first = tmp[0];
//                        var last = tmp[tmp.Length - 1]; //might be III or some suffix.
//                        var db1 = new Database();
//                        var p = db1.Players.SingleOrDefault(c => c.FirstName.Contains(first) && c.LastName.Contains(last));
//                        if (p != null)
//                        {
//                            tr.TournamentId = tournament.Id;
//                            tr.Year = year;
//                            tr.Position = player.Position;
//                            tr.PlayerId = p.Id;
//                            tr.R1 = string.IsNullOrEmpty(player.R1) ? 0 : Int32.Parse(player.R1);
//                            tr.R2 = string.IsNullOrEmpty(player.R2) ? 0 : Int32.Parse(player.R2);
//                            tr.R3 = string.IsNullOrEmpty(player.R3) ? 0 : Int32.Parse(player.R3);
//                            tr.R4 = string.IsNullOrEmpty(player.R4) ? 0 : Int32.Parse(player.R4);
//                            tr.Total = string.IsNullOrEmpty(player.Total) ? 0 : Int32.Parse(player.Total);
//                            tr.Money = string.IsNullOrEmpty(player.Money) ? 0 : decimal.Parse(player.Money.Split('$')[1]);
//                            tournamentResultList.Add(tr);
//                        }
//                        else
//                        {
//                            db1.Players.Add(new Player
//                            {
//                                FirstName = first,
//                                LastName = last
//                            });
//                            db1.SaveChanges();
//                        }
//                    }
//                    db.TournamentResults.AddRange(tournamentResultList);

//                }

//            }
//            db.SaveChanges();


//        }

//    }
//}
