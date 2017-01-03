using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Profile;

namespace Simon
{
    public class TopScoreEntity : TableEntity
    {
        public TopScoreEntity(string name, int level, int round, int buttons)
        {
            this.PartitionKey = "TopScores";
            this.RowKey = name;
            Name = name;
            Level = level;
            Round = round;
            Buttons = buttons;
            Device = AnalyticsInfo.VersionInfo.DeviceFamily.Replace("Windows.", null);
        }

        public TopScoreEntity(string name, int level, int round, int buttons, string device)
        {
            this.PartitionKey = "TopScores";
            this.RowKey = name;
            Name = name;
            Level = level;
            Round = round;
            Buttons = buttons;
            Device = device.Replace("Windows.", null);
        }

        public TopScoreEntity() { }

        public string Name
        { get; set; }

        public int Level
        { get; set; }

        public int Round
        { get; set; }

        public int Buttons
        { get; set; }

        public string Device
        { get; set; }

        public bool Beats(TopScoreEntity thatScore)
        {

            if ((this.Level > thatScore.Level || (this.Level == thatScore.Level && this.Round > thatScore.Round)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }


    }

    public struct TopScore
    {

        public string Name
        { get; set; }

        public int Level
        { get; set; }

        public int Round
        { get; set; }

        public int Buttons
        { get; set; }

        public string Device
        { get; set; }

        public static bool checkLocal()
        {
            return false;
        }

        public TopScore(string name, int level, int round, int buttons)
        {
            Name = name;
            Level = level;
            Round = round;
            Buttons = buttons;
            Device = AnalyticsInfo.VersionInfo.DeviceFamily.Replace("Windows.", null);
        }


        public TopScore(string name, int level, int round, int buttons, string device)
        {
            Name = name;
            Level = level;
            Round = round;
            Buttons = buttons;
            Device = device;
        }

        public bool Beats(TopScore thatScore)
        {

            if ((this.Level > thatScore.Level || (this.Level == thatScore.Level && this.Round > thatScore.Round)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
