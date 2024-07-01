using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using ExcelDataReader;
using System.IO;
using System.Linq;
using ExcelDataReader.Log;

namespace PG_GUI.Forms
{
    public partial class FormProduct : Form
    {
        private List<(float Duration, double Load)> loadDurationCurve;
        private float connected_load;
        private float auc, auc_1year;
        private string excel_link;
        private float max_demand, demand_factor, average_load, load_factor, plant_capacity, plant_capacity_S, plant_capacity_H, plant_capacity_factor;


        // Constructor to initialize form and load data
        public FormProduct(List<(float Duration, double Load)> loadDurationCurve)
        {
            InitializeComponent();
            this.loadDurationCurve = loadDurationCurve; // Initialize the list


           /* // Load Excel data if not already loaded
            if (loadDurationCurve.Count == 0)
            {
                var excelData = LoadExcelData("C:\\Users\\Admin\\Documents\\GitHub\\Power_Generation_CEP_GUI\\PG_GUI\\load_profile.xlsx");
                var dataArray = excelData.ToArray();

                foreach (var dataPoint in dataArray)
                {
                    float duration = ProcessTimeInterval(dataPoint.Key);
                    loadDurationCurve.Add((duration, dataPoint.Value));
                }

                // Sort the loadDurationCurve by load in descending order initially
                loadDurationCurve = loadDurationCurve.OrderByDescending(item => item.Load).ToList();
            }
            

            // Print the sorted load duration curve (for debugging)
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }*/
        }

        private void LoadState()
        {
            loadDurationCurve = SharedState.LoadDurationCurve;
            connected_load = SharedState.ConnectedLoad;
            max_demand = SharedState.MaxDemand;
            demand_factor = SharedState.DemandFactor;
            average_load = SharedState.AverageLoad;
            load_factor = SharedState.LoadFactor;
            plant_capacity = SharedState.PlantCapacity;
            plant_capacity_factor = SharedState.PlantCapacityFactor;

            if (SharedState.LoadDurationCurveSeries != null)
            {
                chart1.Series.Clear();
                chart1.Series.Add(SharedState.LoadDurationCurveSeries);
            }
        }

        private void SaveState()
        {
            SharedState.LoadDurationCurve = loadDurationCurve;
            SharedState.ConnectedLoad = connected_load;
            SharedState.MaxDemand = max_demand;
            SharedState.DemandFactor = demand_factor;
            SharedState.AverageLoad = average_load;
            SharedState.LoadFactor = load_factor;
            SharedState.PlantCapacity = plant_capacity;
            SharedState.PlantCapacityFactor = plant_capacity_factor;

            if (chart1.Series.Count > 0)
            {
                SharedState.LoadDurationCurveSeries = chart1.Series["Voltage Regulation"];
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
            // Clear existing series points to avoid overlapping data
            chart1.Series.Clear();

            // Create a new chart series for the load duration curve
            Series timeDurationCurveSeries = new Series("Voltage Regulation");
            timeDurationCurveSeries.ChartType = SeriesChartType.Spline; // Set the chart type to Spline

            // Initialize cumulative duration
            float cumulativeDuration = 0.0f;

            // Ensure sorting happens only once
           
                // Sort the loadDurationCurve by load in descending order
                loadDurationCurve = loadDurationCurve.OrderByDescending(item => item.Load).ToList();
               

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

            // Save the state
            SaveState();
        }

        private void CalculateCostsSteam()
        {
            // Constants
           // const double steamCapitalCostPerKW = 1250;

            //const double steamInterestDepreciation = 0.12;

            //const double steamOperatingCostPerKWh = 0.05;

            double unitsGeneratedPerAnnum = 262.8 * 1e6; // 262.8 x 10^6 kWh
            double unitsGeneratedPerKgOfOil = 3.6;
            unitsGeneratedPerAnnum = auc_1year;


            float steamCapitalCostPerKW = 0.0f;
            float steamOperatingCostPerKWh = 0.0f;
            float steamInterestDepreciation = 0.0f;
            float steamTransmissionCostPerKWh = 0.0f;
            float SteamCostOfFuelPerMetricTon;
            float.TryParse(Capital.Text, out steamCapitalCostPerKW);
            float.TryParse(Intrest.Text, out steamInterestDepreciation);
            float.TryParse(Operating.Text, out steamOperatingCostPerKWh);
            float.TryParse(Transmission.Text, out steamTransmissionCostPerKWh);//CostOfFuel
            float.TryParse(CostOfFuel.Text, out SteamCostOfFuelPerMetricTon);
            steamInterestDepreciation /= 100;
            /*steamOperatingCostPerKWh /= 100;
            steamTransmissionCostPerKWh /= 100;*/
            // Steam Only
            double steamCapitalCostOnly = max_demand * steamCapitalCostPerKW;
            double steamAnnualInterestDepreciationOnly = steamInterestDepreciation * steamCapitalCostOnly;
            double steamOperatingCostOnly = steamOperatingCostPerKWh * unitsGeneratedPerAnnum;
            double steamTransmissionCostOnly = steamTransmissionCostPerKWh * unitsGeneratedPerAnnum;
            double steamTotalAnnualCostOnly = steamAnnualInterestDepreciationOnly + steamOperatingCostOnly + steamTransmissionCostOnly;
            double overallCostPerKWhSteamOnly = steamTotalAnnualCostOnly / unitsGeneratedPerAnnum;
            double anualOilConsumption = unitsGeneratedPerAnnum/unitsGeneratedPerKgOfOil;//for de
            SteamCostOfFuelPerMetricTon = SteamCostOfFuelPerMetricTon * (float)anualOilConsumption;

            double steamFixedCost = steamAnnualInterestDepreciationOnly + steamTransmissionCostOnly;
            double steamVariableCost = steamOperatingCostOnly + SteamCostOfFuelPerMetricTon;

            Console.WriteLine($"Steam Only Cost per kWh: {overallCostPerKWhSteamOnly} Rs");

            labelSteamOnlyCost.Text = $"{(int)overallCostPerKWhSteamOnly} Rs";
            labelSteamFuelCost.Text = $"{(int)steamFixedCost} Rs";
            labelSteamFixedCost.Text = $"{(int)steamVariableCost} Rs";
            labelSteamVariableCost.Text = $"{(int)SteamCostOfFuelPerMetricTon} Rs";
            GreenHousegas.Text = $"CO₂: 70-80% of total emissions.\r\nCH₄: <1% of total emissions.\r\nN₂O: <1% of total emissions.";
            Sustainable.Text = $"High CO₂ emissions, air pollution (SO₂, NOx, particulates),\n ecosystem damage from mining\n and high water consumption,\n resulting in low Sustainability.";
        }

        private void CalculateCostsHydro()
        {
            float hydroCapitalCostPerKW = 0.0f;
            float hydroOperatingCostPerKWh = 0.0f;
            float hydroInterestDepreciation = 0.0f;
            float hydroTransmissionCostPerKWh = 0.0f;

            float.TryParse(capital_1.Text, out hydroCapitalCostPerKW);
            float.TryParse(Intrest_2.Text, out hydroInterestDepreciation);
            float.TryParse(Operating_1.Text, out hydroOperatingCostPerKWh);
            float.TryParse(Transmission_1.Text, out hydroTransmissionCostPerKWh);

            hydroInterestDepreciation /= 100;
            /*hydroOperatingCostPerKWh /= 100;
            hydroTransmissionCostPerKWh /= 100;*/
            double unitsGeneratedPerAnnum = 262.8 * 1e6; // 262.8 x 10^6 kWh
            unitsGeneratedPerAnnum = auc_1year;
            // Hydro Only
            double hydroCapitalCostOnly = max_demand * hydroCapitalCostPerKW;
            double hydroAnnualInterestDepreciationOnly = hydroInterestDepreciation * hydroCapitalCostOnly;
            double hydroOperatingCostOnly = hydroOperatingCostPerKWh * unitsGeneratedPerAnnum;
            double hydroTransmissionCostOnly = hydroTransmissionCostPerKWh * unitsGeneratedPerAnnum;
            double hydroTotalAnnualCostOnly = hydroAnnualInterestDepreciationOnly + hydroOperatingCostOnly + hydroTransmissionCostOnly;
            double overallCostPerKWhHydroOnly = hydroTotalAnnualCostOnly / unitsGeneratedPerAnnum;
            Console.WriteLine($"Hydro Only Cost per kWh: {overallCostPerKWhHydroOnly} Rs");

            double HydroFixedCost = hydroAnnualInterestDepreciationOnly + hydroTransmissionCostOnly;
            double HydroVariableCost = hydroOperatingCostOnly;

            labelHydroOnlyCost1.Text = $"{(int)overallCostPerKWhHydroOnly} Rs";
            HfixedCost.Text = $"{(int)HydroFixedCost} Rs";
            HVariableCost.Text = $"{(int)HydroVariableCost} Rs";
            HGreenHouse.Text = $"Hydropower plants emit CO₂ and methane in low quantities.";
            HSustainability.Text = $"Renewable Nature: Hydropower is renewable and emits\n significantly fewer greenhouse gases than fossil fuel\n plants.\r\n\r\nEnvironmental Impacts: Concerns include habitat disruption \nand altered river flows due to dam construction and operation.\r\n\r\nClimate Change Mitigation: Hydropower contributes to mitigating climate \nchange by displacing more carbon-intensive forms of electricity generation.";

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Load Excel data if not already loaded
            if (loadDurationCurve.Count == 0)
            {
                excel_link = excel.Text;
                //C:\\Users\\Admin\\Documents\\GitHub\\Power_Generation_CEP_GUI\\PG_GUI\\load_profile.xlsx
                var excelData = LoadExcelData(excel_link);
                var dataArray = excelData.ToArray();

                foreach (var dataPoint in dataArray)
                {
                    float duration = ProcessTimeInterval(dataPoint.Key);
                    loadDurationCurve.Add((duration, dataPoint.Value));
                }

                // Sort the loadDurationCurve by load in descending order initially
                loadDurationCurve = loadDurationCurve.OrderByDescending(item => item.Load).ToList();
            }


            // Print the sorted load duration curve (for debugging)
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }


            // Parse CLoad.Text to get connected load
            if (float.TryParse(CLoad.Text, out connected_load))
            {
                // Calculate max demand
                max_demand = (float)loadDurationCurve.Max(item => item.Load);

                // Calculate demand factor
                demand_factor = max_demand / connected_load;

                // Calculate average load
                average_load = (float)loadDurationCurve.Average(item => item.Load);

                // Calculate load factor
                load_factor = average_load / max_demand;

                // Calculate plant capacity (assuming 80% factor as an example)
                plant_capacity = max_demand / (1000*0.8f);

                // Calculate plant capacity factor
                plant_capacity_factor = connected_load / plant_capacity;

                // Print results to console
                Console.WriteLine("Calculation Results:");
                Console.WriteLine($"Connected Load: {connected_load}");
                Console.WriteLine($"Max Demand: {max_demand}");
                Console.WriteLine($"Demand Factor: {demand_factor}");
                Console.WriteLine($"Average Load: {average_load}");
                Console.WriteLine($"Load Factor: {load_factor}");
                Console.WriteLine($"Plant Capacity: {plant_capacity} KW" );
                Console.WriteLine($"Plant Capacity Factor: {plant_capacity_factor}");
                //calculated_line_efficiency.Text = line_efficiency.ToString();
                // Calculate and display area under the curve
                // auc = CalculateAreaUnderCurve()/1000.0f;
                auc = load_factor * max_demand * 24.0f;
                auc = auc / 1000; 
                Console.WriteLine($"Area Under the Curve: {auc}");
                //MessageBox.Show($"Watt hour: {auc}", "KWH");
                // Display results in TextBoxes (optional, if needed)
                Loadfactor_1.Text = load_factor.ToString();
                DemandFcator_2.Text = demand_factor.ToString();
                PlantCapacity_1.Text = $"{plant_capacity} KW";
                PlantCapacityFactor_1.Text = plant_capacity_factor.ToString();
                Kwhenergy.Text = auc.ToString();

                auc_1year = auc * 356;
                Kwhinyear.Text = auc_1year.ToString();

                plant_capacity_S = plant_capacity + plant_capacity*0.65f;
                plantCapacity_steam.Text = plant_capacity_S.ToString();

                plant_capacity_H = plant_capacity + plant_capacity * 0.10f;
                plantCapacity_hydro.Text = plant_capacity_H.ToString();
            }
            else
            {
                // Handle case where parsing CLoad.Text fails
                MessageBox.Show("Invalid input for Connected Load.");
            }

            // Save state
           // SaveState();
        }

        private double CalculateAreaUnderCurve()
        {
            double area = 0.0;

            // Ensure the loadDurationCurve is sorted by duration
            loadDurationCurve = loadDurationCurve.OrderBy(item => item.Duration).ToList();

            for (int i = 0; i < loadDurationCurve.Count - 1; i++)
            {
                float x1 = loadDurationCurve[i].Duration;
                double y1 = loadDurationCurve[i].Load;
                float x2 = loadDurationCurve[i + 1].Duration;
                double y2 = loadDurationCurve[i + 1].Load;

                // Calculate the area of the trapezoid
                area += (x2 - x1) * (y1 + y2) / 2.0;
            }

            return area;
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            CalculateCostsSteam();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CalculateCostsHydro();
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void CostOfFuel_TextChanged(object sender, EventArgs e)
        {

        }

        private void fixedCost_TextChanged(object sender, EventArgs e)
        {

        }

        private void Sustainable_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelSteamOnlyCost_TextChanged(object sender, EventArgs e)
        {

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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
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