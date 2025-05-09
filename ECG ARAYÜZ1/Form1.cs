﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;

namespace ECG_ARAYÜZ1
{
    public partial class Form2 : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private string apiUrl = "http://172.20.10.6:5000/bpm";  // Flask API URL'si

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // Zamanlayıcı aralığını ayarla (500 ms)
            dataTimer.Interval = 4000; // Veri alımını yarım saniyeye düşür
        }

        private async void dataTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonData = JsonConvert.DeserializeObject<dynamic>(responseBody);
                double bpm = jsonData.bpm;
                var signal = jsonData.signal.ToObject<List<double>>();
                var peaks = jsonData.peaks.ToObject<List<int>>();

                lblBPM.Text = $"Current BPM: {bpm:F2}";

                // Mevcut grafik veri noktasını sınırla (ör. 100 nokta)
                if (ecgChart.Series["ECG"].Points.Count > 100)
                {
                    ecgChart.Series["ECG"].Points.RemoveAt(0);
                }

                // Yeni verileri ekle
                for (int i = 0; i < signal.Count; i++)
                {
                    ecgChart.Series["ECG"].Points.AddY(signal[i]);
                }

                // Peakleri işaretle
                foreach (var peakIndex in peaks)
                {
                    if (peakIndex < signal.Count)
                    {
                        var point = ecgChart.Series["ECG"].Points[peakIndex];
                        point.Color = System.Drawing.Color.Red;
                        point.MarkerStyle = MarkerStyle.Circle;
                    }
                }

                // Y ekseni değerlerini ayarla
                ecgChart.ChartAreas[0].AxisY.Minimum = 50;
                ecgChart.ChartAreas[0].AxisY.Maximum = 250;
                ecgChart.ChartAreas[0].AxisY.Interval = 20;

                // X ekseni için aralık belirle
                ecgChart.ChartAreas[0].AxisX.Minimum = 0;
                ecgChart.ChartAreas[0].AxisX.Maximum = 30; // Gösterilecek veri sayısı (örneğin 100 nokta)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Bağlantı Başlatıldı";
            dataTimer.Interval = 2000;
            dataTimer.Start();
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "Bağlantı Kesildi";
            dataTimer.Stop();
            ecgChart.Series["ECG"].Points.Clear();
        }
    }
}
