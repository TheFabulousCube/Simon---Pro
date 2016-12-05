using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Simon
{
    public class TopScore
    {
        private string _name;
        private int _level; // sequence.count(), number of levels completed
        private int _round; // playerSequence.count(), number of 'clicks' completed sucessfuly in this level

        public TopScore(string name, int level, int round)
        {
            this.Name = name;
            this.Level = level;
            this.Round = round;
        }

        public string Name
        
        { get; set; }
        

        public int Level
        { get; set; }

        public int Round
        { get; set; }

        public static bool checkLocal()
        {

            return false;
        }
    }
}
