using FantasyGolf.Core.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            var allTournamentResults = db.TournamentResults
                .Include(c => c.Player)
                .ToList();

            foreach(var tournament in tournaments)
            {
                var tournamentResults = allTournamentResults
                    .Where(t => t.Year > 2013 && t.TournamentId == tournament.Id);

                var ratingService = new TournamentRatingService(tournament, tournamentResults);
                var results = ratingService.Predict();
            }

            #region OLD


            //string top5pattern = @"^T?[1-5]$";
            //string top10pattern = @"^T?[6-9]$|T?10$";
            //string top20pattern = @"^T?(1)[1-9]|T?20$";

            //foreach (var tournament in db.Tournaments.OrderBy(c => c.OrderPlayed).ToList())
            //{

            //    var dataset = db.TournamentResults
            //        .Where(t => 
            //            t.TournamentId == tournament.Id && 
            //            t.Year > 2013);

            //    var lookup = new Dictionary<int, int>();
            //    foreach (var item in dataset)
            //    {
            //        if (!lookup.ContainsKey(item.PlayerId))
            //        {
            //            lookup.Add(item.PlayerId, 0);
            //        }

            //        if (item.Year == 2016 && item.Position == "1")
            //        {
            //            //won last year;
            //            lookup[item.PlayerId] += -500;
            //        }

            //        if (item.Year == 2015 && item.Position == "1")
            //        {
            //            //won two years ago
            //            lookup[item.PlayerId] += -100;
            //        }

            //        if (item.Year == 2014 && item.Position == "1")
            //        {
            //            //won three years ago
            //            lookup[item.PlayerId] += -50;
            //        }

            //        if (new string[] { "CUT" }.Contains(item.Position))
            //        {
            //            //deduction for missing cut
            //            lookup[item.PlayerId] += -100;
            //        }

            //        if (new Regex(top5pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2016)
            //        {
            //            lookup[item.PlayerId] += 200;
            //        }

            //        if (new Regex(top10pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2016)
            //        {
            //            lookup[item.PlayerId] += 100;
            //        }

            //        if (new Regex(top20pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2016)
            //        {
            //            lookup[item.PlayerId] += 50;
            //        }

            //        if (new Regex(top5pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2015)
            //        {
            //            lookup[item.PlayerId] += 100;
            //        }

            //        if (new Regex(top10pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2015)
            //        {
            //            lookup[item.PlayerId] += 50;
            //        }

            //        if (new Regex(top20pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2015)
            //        {
            //            lookup[item.PlayerId] += 25;
            //        }


            //        if (new Regex(top5pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2014)
            //        {
            //            lookup[item.PlayerId] += 50;
            //        }

            //        if (new Regex(top10pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2014)
            //        {
            //            lookup[item.PlayerId] += 25;
            //        }

            //        if (new Regex(top20pattern, RegexOptions.IgnoreCase).Match(item.Position).Success && item.Year == 2014)
            //        {
            //            lookup[item.PlayerId] += 15;
            //        }


            //    }


            //    var resultList = lookup.Select(c => new Result { PlayerId = c.Key, Total = c.Value }).ToList();

            //    foreach (var item in resultList)
            //    {
            //        var pointsForMultipleAppearances = db.TournamentResults
            //            .Where(c => c.Year > 2013)
            //            .Where(c => c.PlayerId == item.PlayerId &&
            //        c.TournamentId == tournament.Id).Count();
            //        item.Total += pointsForMultipleAppearances * 25;
            //    }


            //    foreach (var item in resultList.OrderByDescending(c => c.Total).Take(5))
            //    {
            //        var player = db.Players.SingleOrDefault(c => c.Id == item.PlayerId);

            //        Debug.WriteLine(string.Format("Result for {0}: {1} :: {2} {3}",
            //            tournament.Name,
            //            item.Total,
            //            player.FirstName,
            //            player.LastName
            //            ));
            //    }
            //    Debug.WriteLine("\n");
            //}
            #endregion

        }

    }

    public class TournamentRatingService
    {
        private readonly Tournament _tournament;
        private readonly List<TournamentResult> _results;
        
        public const string TOP_5_PATTERN = @"^T?[1-5]$";
        public const string TOP_10_PATTERN = @"^T?[6-9]$|T?10$";
        public const string TOP_20_PATTERN = @"^T?(1)[1-9]|T?20$";

        public TournamentRatingService(Tournament tournament, IEnumerable<TournamentResult> results)
        {
            _tournament = tournament;
            _results = results.ToList();                    
        }

        private List<TournamentRatingResults> EvaluatePlayersPerformance()
        {
            TournamentRatingResults _currentPlayer;
            Dictionary<Player, TournamentRatingResults> stats = new Dictionary<Player, TournamentRatingResults>();

            foreach (var result in _results)
            {
                if (!stats.ContainsKey(result.Player))
                {
                    stats.Add(result.Player, new TournamentRatingResults());
                }

                _currentPlayer = stats.Single(c => c.Key == result.Player).Value;
                _currentPlayer.Player = result.Player;
                _currentPlayer.YearCompeted.Add(result.Year);

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
                else if (new Regex(TOP_10_PATTERN, RegexOptions.IgnoreCase).Match(result.Position).Success)
                {
                    _currentPlayer.YearsOfTopTenFinishes.Add(result.Year);
                }
                else if (new Regex(TOP_20_PATTERN, RegexOptions.IgnoreCase).Match(result.Position).Success)
                {
                    _currentPlayer.YearsOfTopTwentyFinishes.Add(result.Year);
                }
                
            }
            return stats.Select(c => c.Value).ToList();
        }


        public List<TournamentRatingResults> Predict()
        {
            var playerResults = EvaluatePlayersPerformance().OrderByDescending(c => c.Score);
          
            return null;
        }

}

    public class TournamentRatingResults
    {
        public TournamentRatingResults()
        {
            this.YearsOfTopTwentyFinishes = new List<int>();
            this.YearsOfTopFiveFinishes = new List<int>();
            this.YearsOfTopTenFinishes = new List<int>();
            this.YearCompeted = new List<int>();
            this.YearsOfWins = new List<int>();
            this.YearsOfBeingCut = new List<int>();
        }

        public Player Player { get; set; }
        public int Score
        {
            get
            {
               return CalculateScore();
            }
        }
        public List<int> YearsOfWins { get; set; }
        public List<int> YearsOfTopFiveFinishes { get; set; }
        public List<int> YearsOfTopTenFinishes { get; set; }
        public List<int> YearsOfTopTwentyFinishes { get; set; }
        public List<int> YearCompeted { get; set; }
        public List<int> YearsOfBeingCut { get; set; }

        public int CalculateScore()
        {
            int score = 0;

            score += YearsOfWins.Contains(2016) ? -500 : 0;
            score += YearsOfWins.Contains(2015) ? -100 : 0;
            score += YearsOfWins.Contains(2014) ? -50 : 0;

            score += YearsOfTopFiveFinishes.Count(c => c == 2016) * 200;
            score += YearsOfTopTenFinishes.Count(c => c == 2016) * 100;
            score += YearsOfTopTwentyFinishes.Count(c => c == 2016) * 50;

            score += YearsOfTopFiveFinishes.Count(c => c == 2015) * 100;
            score += YearsOfTopTenFinishes.Count(c => c == 2015) * 50;
            score += YearsOfTopTwentyFinishes.Count(c => c == 2015) * 25;

            score += YearsOfTopFiveFinishes.Count(c => c == 2014) * 50;
            score += YearsOfTopTenFinishes.Count(c => c == 2014) * 25;
            score += YearsOfTopTwentyFinishes.Count(c => c == 2014) * 15;

            score += YearsOfBeingCut.Count() * -50;
            score += YearCompeted.Count() * 25;
            return score;
        }

        public override string ToString()
        {
            return string.Format("Player: {0} {1}, Score {2}", Player.FirstName, Player.LastName, CalculateScore().ToString());
        }

    }

}
