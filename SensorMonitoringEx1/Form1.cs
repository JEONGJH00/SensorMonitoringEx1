using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SqlClient;

namespace SensorMonitoringEx1
{
    public partial class Form1 : Form
    {
        private SerialPort sPort;
        private int xCount = 200; //
        List<SensorData> myData = new List<SensorData>();
        SqlConnection conn;
//        string connString = @"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\정재형\\source\\repos\\SensorMonitoringEx1\\SensorMonitoringEx1\\SensorData.mdf;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();

            // Combobox
            foreach(var port in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(port);
            }
            comboBox1.Text = "Select Port";

            

            // Chart 세팅
            ChartSetting();

            // txtCount
            txtCount.TextAlign = HorizontalAlignment.Center;

            // Connect, DistConnect 버튼
            btnConnect.Enabled = false;
            btnDisConnect.Enabled = false;
        }

        private void ChartSetting()
        {
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add("draw");
            chart1.ChartAreas["draw"].AxisX.Minimum = 0;
            chart1.ChartAreas["draw"].AxisX.Maximum = xCount;
            chart1.ChartAreas["draw"].AxisX.Interval = xCount / 4;
            chart1.ChartAreas["draw"].AxisX.MajorGrid.LineColor = Color.White;
            chart1.ChartAreas["draw"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart1.ChartAreas["draw"].AxisY.Minimum = 0;
            chart1.ChartAreas["draw"].AxisY.Maximum = 120;
            chart1.ChartAreas["draw"].AxisY.Interval = 10;
            chart1.ChartAreas["draw"].AxisY.MajorGrid.LineColor = Color.White;
            chart1.ChartAreas["draw"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart1.ChartAreas["draw"].BackColor = Color.White;

            chart1.ChartAreas["draw"].CursorX.AutoScroll = true;

            chart1.ChartAreas["draw"].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas["draw"].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chart1.ChartAreas["draw"].AxisX.ScrollBar.ButtonColor = Color.LightSteelBlue;

            chart1.Series.Clear();
            chart1.Series.Add("PhotoCell");
            chart1.Series["PhotoCell"].ChartType = SeriesChartType.Line;
            chart1.Series["PhotoCell"].Color = Color.Black;
            chart1.Series["PhotoCell"].BorderWidth = 6;
            if (chart1.Legends.Count > 0)
                chart1.Legends.RemoveAt(0);

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            sPort = new SerialPort(cb.SelectedItem.ToString());
            sPort.Open();
            sPort.DataReceived += SPort_DataReceived;

            lblConnectTime.Text = "Connection Time : " + DateTime.Now.ToString();
            btnDisConnect.Enabled = true;
        }

        private void SPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string s = sPort.ReadLine();
            this.BeginInvoke(new Action(delegate { ShowValue(s); }));
        }

        private void ShowValue(string s)
        {
            int v = Int32.Parse(s);
            try
            {
                v = Int32.Parse(s);
            }
            catch(FormatException)
            {
                Console.WriteLine("Unable to convert {0}", s);
                return;
            }
            if (v < 0 || v > 120) // 센서값이 0보다 작거나 120보다 크면 버린다.
                return;

            SensorData data = new SensorData(
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToString("HH:mm:ss"),
                v);

            myData.Add(data);
            //DBInsert(data);

            txtCount.Text = myData.Count.ToString();
            //progressBar1.Value = v;

            string item = DateTime.Now.ToString() + "\t" + s;
            listBox1.Items.Add(item);
            listBox1.SelectedIndex = listBox1.Items.Count - 1; // 스크롤

            // Chart
            chart1.Series["PhotoCell"].Points.Add(v);

            // 차트 스크롤 하는 부분
            chart1.ChartAreas["draw"].AxisX.Minimum = 0;
            chart1.ChartAreas["draw"].AxisX.Maximum =
                (myData.Count >= 200) ? myData.Count : xCount;
            if (myData.Count > xCount)
                chart1.ChartAreas["draw"].AxisX.ScaleView.Zoom
                    (myData.Count - xCount, myData.Count);
            else
                chart1.ChartAreas["draw"].AxisX.ScaleView.Zoom(0, xCount);

  
        }

        /*
        private void DBInsert(SensorData data)
        {
            string sql = string.Format(
                "INSERT INTO SenSorTable(Date, Time, Value) Valus{'{0}','{1}',{2})",
                data.Date, data.Time, data.Value);

            try
            {
                SqlConnection conn = new SqlConnection(connString);
                SqlCommand comm = new SqlCommand(sql, conn);
                {
                    conn.Open();
                    comm.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        */

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            sPort.Close();
            btnConnect.Enabled = true;
            btnDisConnect.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            sPort.Open();
            btnDisConnect.Enabled = true;
            btnConnect.Enabled = false;
        }
    }
}
