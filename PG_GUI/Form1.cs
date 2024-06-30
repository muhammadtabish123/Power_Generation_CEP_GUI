using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ExcelDataReader;

namespace PG_GUI
{
    public partial class Form1 : Form
    {
        private Button currentButton;
        private Random random;
        private int tempIndex;
        private Form activeForm;
        private List<(float Duration, double Load)> loadDurationCurve;

        public Form1()
        {
            InitializeComponent();
            random = new Random();
            btnCloseChildForm.Visible = false;
            this.Text = string.Empty;
            this.ControlBox = false;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            this.panelTitleBar.MouseDown += new MouseEventHandler(this.panelTitleBar_MouseDown);

            // Initialize the list
            loadDurationCurve = new List<(float, double)>();

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

            // Optionally, print the load duration curve
            foreach (var item in loadDurationCurve)
            {
                Console.WriteLine($"Duration: {item.Duration}, Load: {item.Load}");
            }
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private Color SelectThemeColor()
        {
            int index = random.Next(ThemeColor.ColorList.Count);
            while (tempIndex == index)
            {
                index = random.Next(ThemeColor.ColorList.Count);
            }
            tempIndex = index;
            string color = ThemeColor.ColorList[index];
            return ColorTranslator.FromHtml(color);
        }

        private void ActivateButton(object btnSender)
        {
            if (btnSender != null)
            {
                if (currentButton != (Button)btnSender)
                {
                    DisableButton();
                    Color color = SelectThemeColor();
                    currentButton = (Button)btnSender;
                    currentButton.BackColor = color;
                    currentButton.ForeColor = Color.White;
                    currentButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    panelTitleBar.BackColor = color;
                    panelLogo.BackColor = ThemeColor.ChangeColorBrightness(color, -0.3);
                    ThemeColor.PrimaryColor = color;
                    ThemeColor.SecondaryColor = ThemeColor.ChangeColorBrightness(color, -0.3);
                    btnCloseChildForm.Visible = true;
                }
            }
        }

        private void DisableButton()
        {
            foreach (Control previousBtn in panelMenu.Controls)
            {
                if (previousBtn.GetType() == typeof(Button))
                {
                    previousBtn.BackColor = Color.FromArgb(51, 51, 76);
                    previousBtn.ForeColor = Color.Gainsboro;
                    previousBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                }
            }
        }

        private void OpenChildForm(Form childForm, object btnSender)
        {
            if (activeForm != null)
                activeForm.Close();
            ActivateButton(btnSender);
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            this.panelDesktopPane.Controls.Add(childForm);
            this.panelDesktopPane.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
            lblTitle.Text = childForm.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            // Pass loadDurationCurve when creating FormProduct
            OpenChildForm(new Forms.FormProduct(loadDurationCurve), sender);
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Forms.FormOrders(), sender);
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Forms.FormCustomers(), sender);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Forms.FormReporting(), sender);
        }

        private void btnNotifications_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Forms.FormNotifications(), sender);
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Forms.FormSetting(), sender);
        }

        private void btnCloseChildForm_Click(object sender, EventArgs e)
        {
            if (activeForm != null)
                activeForm.Close();
            Reset();
        }

        private void Reset()
        {
            DisableButton();
            lblTitle.Text = "HOME";
            panelTitleBar.BackColor = Color.FromArgb(0, 150, 136);
            panelLogo.BackColor = Color.FromArgb(39, 39, 58);
            currentButton = null;
            btnCloseChildForm.Visible = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void bntMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void panelDesktopPane_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

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
