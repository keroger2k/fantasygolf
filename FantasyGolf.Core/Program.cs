using FantasyGolf.Core.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FantasyGolf.Core
{
    public class Program
    {
        public const string TOP_5_PATTER = @"^T?[1-5]$";
        public const string TOP_10_PATTER = @"^T?[6-9]$|T?10$";
        public const string TOP_20_PATTER = @"^T?(1)[1-9]|T?20$";

        public static void Main(string[] args)
        {
            var db = new Database();
            var tournaments = db.Tournaments.OrderBy(t => t.OrderPlayed).ToList();
            var allTournamentResults = db.TournamentResults.ToList();

            foreach(var tournament in tournaments)
            {
                var tournamentResults = allTournamentResults
                    .Where(t => t.Year > 2013 && t.TournamentId == tournament.Id);

                var ratingService = new TournamentRatingService(tournament, tournamentResults);
                var results = ratingService.Predict();
            }

            #region OLD


            string top5pattern = @"^T?[1-5]$";
            string top10pattern = @"^T?[6-9]$|T?10$";
            string top20pattern = @"^T?(1)[1-9]|T?20$";

            foreach (var tournament in db.Tournaments.OrderBy(c => c.OrderPlayed).ToList())
            {

                var dataset = db.TournamentResults
                    .Where(t => 
                        t.TournamentId == tournament.Id && 
                        t.Year > 2013);

                var lookup = new Dictionary<int, int>();
                foreach (var item in dataset)
                {
                    if (!lookup.ContainsKey(item.PlayerId))
                    {
                        lookup.Add(item.PlayerId, 0);
                    }

                    if (item.Year == 2016 && item.Position == "1")
                    {
                        //won last year;
                        lookup[item.PlayerId] += -500;
                    }

                    if (item.Year == 2015 && item.Position == "1")
                    {
                        //won two years ago
                        lookup[item.PlayerId] += -100;
                    }

                    if (item.Year == 2014 && item.Position == "1")
                    {
                        //won three years ago
                        lookup[item.PlayerId] += -50;
                    }

                    if (new string[] { "CUT" }.Contains(item.Position))
                    {
                        //deduction for missing cut
                        lookup[item.PlayerId] += -100;
                    }

                    if (new Regex(top5pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2016)
                    {
                        lookup[item.PlayerId] += 200;
                    }

                    if (new Regex(top10pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2016)
                    {
                        lookup[item.PlayerId] += 100;
                    }

                    if (new Regex(top20pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2016)
                    {
                        lookup[item.PlayerId] += 50;
                    }

                    if (new Regex(top5pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2015)
                    {
                        lookup[item.PlayerId] += 100;
                    }

                    if (new Regex(top10pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2015)
                    {
                        lookup[item.PlayerId] += 50;
                    }

                    if (new Regex(top20pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2015)
                    {
                        lookup[item.PlayerId] += 25;
                    }


                    if (new Regex(top5pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2014)
                    {
                        lookup[item.PlayerId] += 50;
                    }

                    if (new Regex(top10pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2014)
                    {
                        lookup[item.PlayerId] += 25;
                    }

                    if (new Regex(top20pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2014)
                    {
                        lookup[item.PlayerId] += 15;
                    }


                }


                var resultList = lookup.Select(c => new Result { PlayerId = c.Key, Total = c.Value }).ToList();

                foreach (var item in resultList)
                {
                    var pointsForMultipleAppearances = db.TournamentResults
                        .Where(c => c.Year > 2013)
                        .Where(c => c.PlayerId == item.PlayerId &&
                    c.TournamentId == tournament.Id).Count();
                    item.Total += pointsForMultipleAppearances * 25;
                }


                foreach (var item in resultList.OrderByDescending(c => c.Total).Take(5))
                {
                    var player = db.Players.SingleOrDefault(c => c.Id == item.PlayerId);

                    Debug.WriteLine(string.Format("Result for {0}: {1} :: {2} {3}",
                        tournament.Name,
                        item.Total,
                        player.FirstName,
                        player.LastName
                        ));
                }
                Debug.WriteLine("\n");
            }
            #endregion

        }

    }

    public class Result
    {
        public int PlayerId { get; set; }
        public int Total { get; set; }
    }

    public class TournamentRatingService
    {
        private readonly Tournament _tournament;
        private readonly List<TournamentResult> _results;
        private Dictionary<int, TournamentRatingResults> stats = new Dictionary<int, TournamentRatingResults>();

        public const string TOP_5_PATTERN = @"^T?[1-5]$";
        public const string TOP_10_PATTERN = @"^T?[6-9]$|T?10$";
        public const string TOP_20_PATTERN = @"^T?(1)[1-9]|T?20$";

        public TournamentRatingService(Tournament tournament, IEnumerable<TournamentResult> results)
        {
            _tournament = tournament;
            _results = results.ToList();                    
        }

        private void EvaluatePlayersPerformance()
        {
            TournamentRatingResults _currentPlayer;

            foreach (var result in _results)
            {
                if (!stats.ContainsKey(result.PlayerId))
                {
                    stats.Add(result.PlayerId, new TournamentRatingResults());
                }

                _currentPlayer = stats.Single(c => c.Key == result.PlayerId).Value;

                if (result.Position == "1")
                {
                    //won last year;
                    _currentPlayer.YearsOfWins.Add(result.Year);
                }

                if (new string[] { "CUT" }.Contains(result.Position))
                {
                    //deduction for missing cut
                    _currentPlayer.YearsOfBeingCut.Add(result.Year);
                }

                if (new Regex(TOP_5_PATTERN, RegexOptions.IgnoreCase).Match(result.Position).Success)
                {
                    _currentPlayer.YearsOfTopFiveFinishes.Add(result.Year);
                }

                if (new Regex(TOP_10_PATTERN, RegexOptions.IgnoreCase).Match(result.Position).Success)
                {
                    _currentPlayer.YearsOfTopTenFinishes.Add(result.Year);
                }

                if (new Regex(TOP_20_PATTERN, RegexOptions.IgnoreCase).Match(result.Position).Success)
                {
                    _currentPlayer.YearsOfTopTwentyFinishes.Add(result.Year);
                }
            }
        }


        public TournamentRatingResults Predict()
        {
            throw new NotImplementedException();
        }

    }

    public class TournamentRatingResults
    {
        public List<int> YearsOfWins { get; set; }
        public List<int> YearsOfTopFiveFinishes { get; set; }
        public List<int> YearsOfTopTenFinishes { get; set; }
        public List<int> YearsOfTopTwentyFinishes { get; set; }
        public List<int> YearsOfBeingCut { get; set; }
    }

}
