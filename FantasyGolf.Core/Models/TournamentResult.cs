using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyGolf.Core.Models
{
    public class TournamentResult
    {
        public int TournamentId { get; set; }
        public int Year { get; set; }
        public int PlayerId { get; set; }

        public string Position { get; set; }
        public int R1 { get; set; }
        public int R2 { get; set; }
        public int R3 { get; set; }
        public int R4 { get; set; }
        public int Total { get; set; }
        public decimal Money { get; set; }

        public Tournament Tournament { get; set; }
        public Player Player { get; set; }
    }
}
