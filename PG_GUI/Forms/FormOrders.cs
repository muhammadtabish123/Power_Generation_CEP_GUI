using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PG_GUI.Forms
{
    
    public partial class FormOrders : Form
    {
        private float LoadFactor;
        private float MaxDemand;
        public FormOrders()
        {
            InitializeComponent();
            LoadState();
            CalculateCosts();
        }
        private void LoadState()
        {
            // Load data from SharedState if needed
            LoadFactor = SharedState.LoadFactor;
            MaxDemand = SharedState.MaxDemand;

            Console.WriteLine($"Max Demand: {MaxDemand} KW");
            Console.WriteLine($"LoadFactor: {LoadFactor}");
            // ... load other variables and update the form accordingly
        }

        private void FormOrders_Load(object sender, EventArgs e)
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
        private void CalculateCosts()
        {
            // Constants
            const double steamCapitalCostPerKW = 1250;
            const double hydroCapitalCostPerKW = 2500;
            const double steamInterestDepreciation = 0.12;
            const double hydroInterestDepreciation = 0.10;
            const double steamOperatingCostPerKWh = 0.05;

            const double hydroOperatingCostPerKWh = 0.015;
            const double hydroTransmissionCostPerKWh = 0.002;
            const double unitsGeneratedPerAnnum = 262.8 * 1e6; // 262.8 x 10^6 kWh
            const double hydroUnitsGenerated = 100 * 1e6; // 100 x 10^6 kWh
            const double hydroMaxOutputMW = 40; // 40 MW

            // Steam and Hydro in Conjunction
            double steamUnitsGenerated = unitsGeneratedPerAnnum - hydroUnitsGenerated;
            double steamCapitalCost = 60 * 1e3 * steamCapitalCostPerKW;
            double steamAnnualInterestDepreciation = steamInterestDepreciation * steamCapitalCost;
            double steamOperatingCost = steamOperatingCostPerKWh * steamUnitsGenerated;
            double steamTotalAnnualCost = steamAnnualInterestDepreciation + steamOperatingCost;

            double hydroCapitalCost = hydroMaxOutputMW * 1e3 * hydroCapitalCostPerKW;
            double hydroAnnualInterestDepreciation = hydroInterestDepreciation * hydroCapitalCost;
            double hydroOperatingCost = hydroOperatingCostPerKWh * hydroUnitsGenerated;
            double hydroTransmissionCost = hydroTransmissionCostPerKWh * hydroUnitsGenerated;
            double hydroTotalAnnualCost = hydroAnnualInterestDepreciation + hydroOperatingCost + hydroTransmissionCost;

            double totalAnnualCostConjunction = steamTotalAnnualCost + hydroTotalAnnualCost;
            double overallCostPerKWhConjunction = totalAnnualCostConjunction / unitsGeneratedPerAnnum;

            // Steam Only
            double steamCapitalCostOnly = 100 * 1e3 * steamCapitalCostPerKW;
            double steamAnnualInterestDepreciationOnly = steamInterestDepreciation * steamCapitalCostOnly;
            double steamOperatingCostOnly = steamOperatingCostPerKWh * unitsGeneratedPerAnnum;
            double steamTotalAnnualCostOnly = steamAnnualInterestDepreciationOnly + steamOperatingCostOnly;
            double overallCostPerKWhSteamOnly = steamTotalAnnualCostOnly / unitsGeneratedPerAnnum;

            // Hydro Only
            double hydroCapitalCostOnly = 100 * 1e3 * hydroCapitalCostPerKW;
            double hydroAnnualInterestDepreciationOnly = hydroInterestDepreciation * hydroCapitalCostOnly;
            double hydroOperatingCostOnly = hydroOperatingCostPerKWh * unitsGeneratedPerAnnum;
            double hydroTransmissionCostOnly = hydroTransmissionCostPerKWh * unitsGeneratedPerAnnum;
            double hydroTotalAnnualCostOnly = hydroAnnualInterestDepreciationOnly + hydroOperatingCostOnly + hydroTransmissionCostOnly;
            double overallCostPerKWhHydroOnly = hydroTotalAnnualCostOnly / unitsGeneratedPerAnnum;

            // Display Results
            Console.WriteLine($"Steam and Hydro Conjunction Cost per kWh: {overallCostPerKWhConjunction * 100} paise");
            Console.WriteLine($"Steam Only Cost per kWh: {overallCostPerKWhSteamOnly * 100} paise");
            Console.WriteLine($"Hydro Only Cost per kWh: {overallCostPerKWhHydroOnly * 100} paise");

            // Optionally, display these results in the form's labels or textboxes
            // Example:
            // labelConjunctionCost.Text = $"{overallCostPerKWhConjunction * 100} paise";
            // labelSteamOnlyCost.Text = $"{overallCostPerKWhSteamOnly * 100} paise";
            // labelHydroOnlyCost.Text = $"{overallCostPerKWhHydroOnly * 100} paise";
        }
    }
}
