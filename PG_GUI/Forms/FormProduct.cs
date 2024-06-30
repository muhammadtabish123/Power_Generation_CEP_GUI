using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace PG_GUI.Forms
{
    public partial class FormProduct : Form
    {
        private List<(float Duration, double Load)> loadDurationCurve;

        // Modify constructor to accept data
        public FormProduct(List<(float Duration, double Load)> loadDurationCurve)
        {
            InitializeComponent();
            this.loadDurationCurve = loadDurationCurve;

            // Optionally, print the load duration curve
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void FormProduct_Load(object sender, EventArgs e)
        {
            LoadTheme();
        }

        private void LoadTheme()
        {
            foreach (Control btns in this.Controls)
            {
                if (btns.GetType() == typeof(Button))
                {
                    Button btn = (Button)btns;
                    btn.BackColor = ThemeColor.PrimaryColor;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderColor = ThemeColor.SecondaryColor;
                }
            }
            label4.ForeColor = ThemeColor.SecondaryColor;
            label5.ForeColor = ThemeColor.PrimaryColor;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int x = 6, y = 9;
            // Create a new chart series for voltage regulation
            Series voltageRegulationSeries = new Series("Voltage Regulation");
            //voltageRegulationSeries.Points.Add(x);
            voltageRegulationSeries.Points.AddXY(x, y);
            chart1.Series.Add(voltageRegulationSeries);
            // Update the chart
            chart1.Update();
        }

        private float ProcessTimeInterval(string timeInterval)
        {
            string pattern = @"(\d{1,2}):(\d{2}) (AM|PM)";
            MatchCollection matches = Regex.Matches(timeInterval, pattern);

            if (matches.Count == 2)
            {
                int startHour = int.Parse(matches[0].Groups[1].Value);
                int startMinute = int.Parse(matches[0].Groups[2].Value);
                int endHour = int.Parse(matches[1].Groups[1].Value);
                int endMinute = int.Parse(matches[1].Groups[2].Value);

                float differenceHours = Math.Abs(startHour - endHour);
                float differenceMinutes = Math.Abs((startMinute - endMinute) / 60.0f);
                float totalDifference = differenceHours + differenceMinutes;

                Console.WriteLine($"Time Interval: {timeInterval}");
                Console.WriteLine($"Difference in hours: {totalDifference}");
                return totalDifference;
            }
            else
            {
                Console.WriteLine("Invalid time interval format.");
                return -1.0f;
            }
        }
    }
}
