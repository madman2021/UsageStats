﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsageStats
{
    public class KeyboardStatistics : Observable
    {
        public KeyboardStatistics(ActiveTime total, TimePerHour activityPerHour)
        {
            KeyUsage = new Dictionary<string, int>();
            KeyboardActivity = new ActiveTime(total);
            KeyCountPerHour = new CountPerHour(activityPerHour);
            TypingSpeed = new Histogram(25);
        }

        public int KeyStrokes { get; set; }
        public Dictionary<string, int> KeyUsage { get; set; }
        public ActiveTime KeyboardActivity { get; set; }
        public CountPerHour KeyCountPerHour { get; set; }
        public Histogram TypingSpeed { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format(" Keystrokes:    {0}", KeyStrokes));
            sb.AppendLine(String.Format(" Activity:      {0}", KeyboardActivity));
            sb.AppendLine();
            //sb.AppendLine(" Typing speed (keystrokes per minute):");
            //sb.AppendLine(TypingSpeed.Report());
            //sb.AppendLine();
            sb.AppendLine(" Keystrokes per hour:");
            sb.AppendLine(KeyCountPerHour.Report(false));
            sb.AppendLine();
            IOrderedEnumerable<KeyValuePair<string, int>> list = KeyUsage.ToList().OrderByDescending(kvp => kvp.Value);
            if (list.Count() > 0)
            {
                int longest = list.Max(kvp => kvp.Key.ToString().Length);
                foreach (var kvp in list)
                {
                    double p = KeyStrokes > 0 ? 1.0*kvp.Value/KeyStrokes : 0;
                    sb.AppendLine(String.Format(" {0} {1:####} {2:###%}", kvp.Key.PadRight(longest), kvp.Value, p));
                }
            }
            return sb.ToString();
        }

        public void KeyDown(string key)
        {
            AddKeyUsage(key);

            double sec = KeyboardActivity.Update(Statistics.InactivityThreshold);

            if (sec < 4 && sec > 0.1)
            {
                double perMinute = 60/sec;
                TypingSpeed.Add(perMinute);
            }

            KeyStrokes++;
            KeyCountPerHour.Increase();
        }

        private void AddKeyUsage(string key)
        {
            if (KeyUsage.ContainsKey(key))
            {
                KeyUsage[key] = KeyUsage[key] + 1;
            }
            else
            {
                KeyUsage.Add(key, 1);
            }
        }
    }
}