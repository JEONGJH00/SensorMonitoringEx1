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

        private int cnt; // 심박센서 버리지 않은 측정 횟수

        // 12시간을 1시간 단위로 나눠서 배열에 센서값을 넣음
        int[] HeartData = new int[10]; // 1시간 단위로 심박센서 값을 넣을 배열
        int[] PIRData = new int[10]; // 1시간 단위로 PIR센서 값을 넣을 배열


        public Form1()
        {
            InitializeComponent();


            // Combobox
            foreach (var port in SerialPort.GetPortNames())
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
            // 심박센서 차트
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add("draw");
            chart1.ChartAreas["draw"].AxisX.Minimum = 0;
            chart1.ChartAreas["draw"].AxisX.Maximum = 17;
            chart1.ChartAreas["draw"].AxisX.Interval = 1;
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
            chart1.Series.Add("HeartSensor");
            chart1.Series["HeartSensor"].ChartType = SeriesChartType.Line;
            chart1.Series["HeartSensor"].Color = Color.Black;
            chart1.Series["HeartSensor"].BorderWidth = 10;
            if (chart1.Legends.Count > 0)
                chart1.Legends.RemoveAt(0);

            // PIR 센서 차트
            chart2.ChartAreas.Clear();
            chart2.ChartAreas.Add("draw2");
            chart2.ChartAreas["draw2"].AxisX.Minimum = 0;
            chart2.ChartAreas["draw2"].AxisX.Maximum = 17;
            chart2.ChartAreas["draw2"].AxisX.Interval = 1;
            chart2.ChartAreas["draw2"].AxisX.MajorGrid.LineColor = Color.White;
            chart2.ChartAreas["draw2"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart2.ChartAreas["draw2"].AxisY.Minimum = 0;
            chart2.ChartAreas["draw2"].AxisY.Maximum = 50;
            chart2.ChartAreas["draw2"].AxisY.Interval = 10;
            chart2.ChartAreas["draw2"].AxisY.MajorGrid.LineColor = Color.White;
            chart2.ChartAreas["draw2"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            chart2.ChartAreas["draw2"].BackColor = Color.White;

            chart2.ChartAreas["draw2"].CursorX.AutoScroll = true;

            chart2.ChartAreas["draw2"].AxisX.ScaleView.Zoomable = true;
            chart2.ChartAreas["draw2"].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chart2.ChartAreas["draw2"].AxisX.ScrollBar.ButtonColor = Color.LightSteelBlue;

            chart2.Series.Clear();
            chart2.Series.Add("활동량");
            chart2.Series["활동량"].ChartType = SeriesChartType.Column;
            chart2.Series["활동량"].Color = Color.Black;
            chart2.Series["활동량"].BorderWidth = 10;
            if (chart2.Legends.Count > 0)
                chart2.Legends.RemoveAt(0);

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
            
            string s1 = sPort.ReadLine(); // 심박센서값을 불러와서 s1에 넣음
            string s2 = sPort.ReadLine(); // PIR센서값을 불러와서 s2에 넣음
            this.BeginInvoke(new Action(delegate { ShowValue1(s1); })); // 심박센서의 ShowValue를 호출
            this.BeginInvoke(new Action(delegate { ShowValue2(s2); })); // PIR센서의 ShowValue를 호출
        }

        // 심박 센서 ShowValue
        private void ShowValue1(string s1)
        {
            int v1 = Int32.Parse(s1); // 심박센서값을 INT값으로 변환해서 v1에 넣음

            try // 오류 점검 코드
            {
                v1 = Int32.Parse(s1);
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to convert {0}", s1);
                return;
            }


            // 심박 센서값이 0보다 작거나 120보다 크면 버린다.
            // 버리지 않은 값의 개수와 측정 횟수가 동일해야 평균값을 구할 수 있다.
            if (v1 < 0 || v1 > 120)
                return;
            else
            {
                if (DateTime.Now.Hour >=22 || DateTime.Now.Hour < 8 ) // 22시 이전, 8시 이후 불필요한 cnt값 증가 방지
                {
                    cnt++; // 심박 센서값이 0~120사이일 경우 측정 횟수 1씩 증가
                }
            }


            // 리스트박스에 실시간으로 넘어온 값 출력
            string item = DateTime.Now.ToString() + "\t" + s1;
            listBox1.Items.Add(item);
            listBox1.SelectedIndex = listBox1.Items.Count - 1; // 스크롤


            // 측정된 센서 값을 한시간 단위로 나눈다음 HeartData[] 배열에 저장
            if (DateTime.Now.Hour >= 22 && DateTime.Now.Hour < 23)
                HeartData[0] += v1;
            else if (DateTime.Now.Hour >= 23 && DateTime.Now.Hour < 24)
                HeartData[1] += v1;
            else if (DateTime.Now.Hour >= 24 && DateTime.Now.Hour < 1)
                HeartData[2] += v1;
            else if (DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 2)
                HeartData[3] += v1;
            else if (DateTime.Now.Hour >= 2 && DateTime.Now.Hour < 3)
                HeartData[4] += v1;
            else if (DateTime.Now.Hour >= 3 && DateTime.Now.Hour < 4)
                HeartData[5] += v1;
            else if (DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 5)
                HeartData[6] += v1;
            else if (DateTime.Now.Hour >= 5 && DateTime.Now.Hour < 6)
                HeartData[7] += v1;
            else if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 7)
                HeartData[8] += v1;
            else if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 8)
                HeartData[9] += v1;


            // HeartData[] 배열에 저장된 값을 cnt(측정횟수)로 나눠 평균값을 정각에 Chart로 나타냄
            // 다음 시간대에서 정확한 평균값을 구하기 위해 cnt 초기화
            // cnt 초기화를 안해줄 시 이전 시간대의 cnt값에서 cnt++이 시작됨.(센서값의 개수는 100개인데 cnt값은 100보다 큼)
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("22시", HeartData[0] / cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 24 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("23시", HeartData[1] / cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 1 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("24시", HeartData[2]/ cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 2 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("1시", HeartData[3]/ cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 3 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("2시", HeartData[4]/ cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 4 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("3시", HeartData[5]/ cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 5 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("4시", HeartData[6]/ cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 6 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("5시", HeartData[7]/ cnt);
                cnt = 0;

            }
            else if (DateTime.Now.Hour == 7 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("6시", HeartData[8]/ cnt);
                cnt = 0;
            }
            else if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0)
            {
                chart1.Series["HeartSensor"].Points.AddXY("7시", HeartData[9]/ cnt);
                cnt = 0;
            }



        }//

        // PIR 센서 ShowValue
        private void ShowValue2(string s2)
        {

            int v2 = Int32.Parse(s2); // PIR값을 INT값으로 변환해서 v2에 넣음
            try // 오류 점검 코드
            {
                v2 = Int32.Parse(s2);
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to convert {0}", s2);
                return;
            }

            // 센서값이 0보다 작거나 1보다 크면 버린다.
            if (v2 < 0 || v2 > 1)
                return;

            // 리스트박스에 실시간으로 넘어온 값 출력
            string item = DateTime.Now.ToString() + "\t" + s2;
            listBox1.Items.Add(item);
            listBox1.SelectedIndex = listBox1.Items.Count - 1; // 스크롤

            // 측정된 센서 값을 한시간 단위로 나눈다음 배열에 저장
            if (DateTime.Now.Hour>=22 && DateTime.Now.Hour < 23)
                    PIRData[0] += v2;
            else if (DateTime.Now.Hour >= 23 && DateTime.Now.Hour < 24)
                    PIRData[1] += v2;
            else if (DateTime.Now.Hour >= 24 && DateTime.Now.Hour < 1)
                    PIRData[2] += v2;
            else if (DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 2)
                    PIRData[3] += v2;
            else if (DateTime.Now.Hour >= 2 && DateTime.Now.Hour < 3)
                    PIRData[4] += v2;
            else if (DateTime.Now.Hour >= 3 && DateTime.Now.Hour < 4)
                    PIRData[5] += v2;
            else if (DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 5)
                    PIRData[6] += v2;
            else if (DateTime.Now.Hour >= 5 && DateTime.Now.Hour < 6)
                    PIRData[7] += v2;
            else if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour < 7)
                    PIRData[8] += v2;
            else if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 8)
                    PIRData[9] += v2;


            // 정각에 배열에 저장된 값을 Chart로 나타냄
            // PIR차트에서 cnt(측정횟수)를 안쓰는 이유는 센서값들이 모인 PIRData리스트 값 자체가 활동량임.
            // 예를 들어 PIRData[0] = 50일 경우 1시간동안 50번 움직였다는 의미이므로 굳이 cnt(측정횟수)로 연산을 하지않음.
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("22시", PIRData[0]);
            else if (DateTime.Now.Hour == 24 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("23시", PIRData[1]);
            else if (DateTime.Now.Hour == 1 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("24시", PIRData[2]);
            else if (DateTime.Now.Hour == 2 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("1시", PIRData[3]);
            else if (DateTime.Now.Hour == 3 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("2시", PIRData[4]);
            else if (DateTime.Now.Hour == 4 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("3시", PIRData[5]);
            else if (DateTime.Now.Hour == 5 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("4시", PIRData[6]);
            else if (DateTime.Now.Hour == 6 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("5시", PIRData[7]);
            else if (DateTime.Now.Hour == 7 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("6시", PIRData[8]);
            else if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0)
                chart2.Series["활동량"].Points.AddXY("7시", PIRData[9]);
            
       

        }


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
