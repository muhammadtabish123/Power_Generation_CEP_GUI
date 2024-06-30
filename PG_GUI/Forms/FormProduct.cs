using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using ExcelDataReader;
using System.IO;
using System.Linq;

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

            // Load Excel data
            var excelData = LoadExcelData("C:\\Users\\Admin\\Documents\\GitHub\\Power_Generation_CEP_GUI\\PG_GUI\\load_profile.xlsx");

            // Convert to array if needed
            var dataArray = excelData.ToArray();

            // Example: Printing the data to the console
            foreach (var dataPoint in dataArray)
            {
                Console.WriteLine($"{dataPoint.Key}: {dataPoint.Value}");
                float duration = ProcessTimeInterval(dataPoint.Key);
                loadDurationCurve.Add((duration, dataPoint.Value));
            }

            /*// Optionally, print the load duration curve
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }*/

            // Sort the loadDurationCurve by load in ascending order
            loadDurationCurve = loadDurationCurve.OrderByDescending(item => item.Load).ToList();

            // Print the sorted load duration curve
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            // Handle label click event
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
            // Handle chart click event
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadDurationCurve = loadDurationCurve.OrderByDescending(item => item.Load).ToList();
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }
            // Clear existing series points to avoid overlapping data
            chart1.Series.Clear();

            // Create a new chart series for the load duration curve
            Series timeDurationCurveSeries = new Series("Voltage Regulation");
            timeDurationCurveSeries.ChartType = SeriesChartType.Spline; // Set the chart type to Spline

            // Initialize cumulative duration
            float cumulativeDuration = 0.0f;
            // Add points to the series from loadDurationCurve
            foreach (var item in loadDurationCurve)
            {
                // Add cumulative duration to the current duration
                cumulativeDuration += item.Duration;
                timeDurationCurveSeries.Points.AddXY((int)cumulativeDuration, item.Load);
            }
            // Add the series to the chart
            chart1.Series.Add(timeDurationCurveSeries);

            // Optionally, set chart properties
            chart1.ChartAreas[0].AxisX.Title = "Duration (hours)";
            chart1.ChartAreas[0].AxisY.Title = "Load (kW)";
            chart1.ChartAreas[0].RecalculateAxesScale();

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

        // Method to load Excel data
        private Dictionary<string, double> LoadExcelData(string filePath)
        {
            var data = new Dictionary<string, double>();

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                string key = reader.GetString(0);
                                double value = reader.GetDouble(1);
                                data.Add(key, value);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading row {reader.Depth + 1}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Excel file: {ex.Message}");
            }

            return data;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Handle button click event
        }
    }
}



/*
Duration: 0.8333333, Load: 1110
Duration: 2.833333, Load: 210

Duration: 2, Load: 270
Duration: 2, Load: 250
Duration: 3, Load: 330
Duration: 1, Load: 360
Duration: 3, Load: 300
Duration: 0.3333333, Load: 1590
Duration: 1.333333, Load: 650
Duration: 2, Load: 270


desire output must be 
Duration: 0.3333333, Load: 1590
Duration: 0.8333333, Load: 1110
Duration: 2, Load: 1210
Duration: 1.333333, Load: 650
Duration: 1, Load: 360
Duration: 3, Load: 330
Duration: 3, Load: 300
Duration: 2, Load: 270
Duration: 2, Load: 270
Duration: 2, Load: 250
Duration: 2.833333, Load: 210
*/