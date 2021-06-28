using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kalman2
{

    public partial class Form1 : Form
    {

        public string filePath;


        public Form1()
        {
            InitializeComponent();
        }
        // Фильтр Ботфорда
        //--------------------------------------------------------------------------
        // This function returns the data filtered. Converted to C# 2 July 2014.
        // Original source written in VBA for Microsoft Excel, 2000 by Sam Van
        // Wassenbergh (University of Antwerp), 6 june 2007.
        //--------------------------------------------------------------------------
        /**
         * Функция реализующая фильтр Баттерворта
         * Выходные данные - одномерный массив double
         * Входные данные:
         * indata - одномерные массив входных значений
         * deltaTimeinsec - интервал между измерениями
         * CutOff - частота отсечения принято за 0.1
         */
        public static double[] Butterworth(double[] indata, double deltaTimeinsec, double CutOff)
        {
            /// Проверка на пустоту
            if (indata == null) return null;
            if (CutOff == 0) return indata;

            /// Частота выборки
            double Samplingrate = 1 / deltaTimeinsec;
            /// Установка диапазона данных с помощью dF2 (The data range is set with dF2)
            long dF2 = indata.Length - 1;
            /// Массив с 4мя дополнительными ячейками сзади и спереди для промежуточных данных (Array with 4 extra points front and back)
            double[] Dat2 = new double[dF2 + 4];
            /// Массив результирующих значений (Ptr., changes passed data)
            double[] data = indata;

            /// Копирование исходных данных в расширенный массив (Copy indata to Dat2)
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = indata[r];
            }
            Dat2[1] = Dat2[0] = indata[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];

            /// Инициализация данных
            const double pi = 3.14159265358979;
            double wc = Math.Tan(CutOff * pi / Samplingrate);
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;

            /// RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            /// RECURSIVE TRIGGERS - выполняется фильтр ENABLE (константа первых, последних точек)
            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = indata[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                           + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            // FORWARD filter
            /// FORWARD filter
            double[] DatZt = new double[dF2 + 2];
            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                            + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
            }

            return data;
        }

        // Класс реализующий фильтр Калмана
        class KalmanFilterSimple1D
        {
            public double X0 { get; private set; } // predicted state
            public double P0 { get; private set; } // predicted covariance

            public double F { get; private set; } // factor of real value to previous real value
            public double Q { get; private set; } // measurement noise
            public double H { get; private set; } // factor of measured value to real value
            public double R { get; private set; } // environment noise

            public double State { get; private set; }
            public double Covariance { get; private set; }

            public KalmanFilterSimple1D(double q, double r, double f = 1, double h = 1)
            {
                Q = q;
                R = r;
                F = f;
                H = h;
            }

            public void SetState(double state, double covariance)
            {
                State = state;
                Covariance = covariance;
            }

            public void Correct(double data)
            {
                //time update - prediction
                X0 = F * State;
                P0 = F * Covariance * F + Q;

                //measurement update - correction
                var K = H * P0 / (H * P0 * H + R);
                State = X0 + K * (data - H * X0);
                Covariance = (1 - K * H) * P0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        double[] indata = new double[1000];
        Random rnd = new Random();
        List<double> sourseData = new List<double>();
        double otklK;
        double otklB;
        private void Button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Button2_Click(object sender, EventArgs e)
        {
           
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
        string[] fileLines;
        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "D:\\";
            openFileDialog1.Filter = "csv files (*.csv)|*.csv";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            listBox1.Items.Clear();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog1.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog1.OpenFile();
                var fileContent = string.Empty;
                // получаем содержимое файла
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }

                // Забираем все показания построчно
                fileLines = fileContent.Split('\n');
                List<string> uniqueLabelMas = new List<string>();

                // Найти оригинальные записи
                for (int i = 0; i < fileLines.Length - 1; i++)
                {
                    string[] temMas = fileLines[i].Split(';');
                    bool flag = true;
                    if (temMas[0].Substring(0, 2) == ", ")
                    {
                        temMas[0] = temMas[0].Substring(2, temMas[0].Length - 2);
                    }
                    for (int j = 0; j < uniqueLabelMas.Count; j++)
                        if (temMas[0] == uniqueLabelMas[j])
                        {
                            flag = false;
                        }
                    if (flag)
                        uniqueLabelMas.Add(temMas[0]);
                }
                for (int i = 0; i < uniqueLabelMas.Count; i++)
                    listBox1.Items.Add(uniqueLabelMas[i]);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart2.Series.Clear();
            chart2.Series.Add("Sourse");
            chart2.Series["Sourse"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series.Add("Batterworth");
            chart2.Series["Batterworth"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series["Batterworth"].BorderWidth = 5;
            chart2.Series.Add("Kalman");
            chart2.Series["Kalman"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series["Kalman"].BorderWidth = 5;
            // Когда выбираем строку
            if (listBox1.SelectedItem == "")
                return;
            // Ищем количество записей в файле с выбранным UUID
            int tCmax = 0;
            // Рассчитываем размер массивов
            for (int i = 0; i < fileLines.Length; i++)
            {
                string[] temMas = fileLines[i].Split(';');
                if (temMas[0].Length > 0)// Исключаем первую строку
                {
                    if (temMas[0].Substring(0, 2) == ", ")
                    {
                        temMas[0] = temMas[0].Substring(2, temMas[0].Length - 2);
                    }
                
                    // Выбираем 1 метку
                    if (temMas[0] == listBox1.SelectedItem.ToString())
                    {
                        tCmax++;
                    }
                }
            }
            richTextBox1.Text = "Количество показаний: " + tCmax.ToString() + "\r\n";

            double[] masMetka = new double[tCmax]; // Исходные данные
            double[] masMetkaMinAve = new double[tCmax]; // График шума
            int tC = 0; // Текущий счетчик
            for (int i = 0; i < fileLines.Length; i++)
            {
                string[] temMas = fileLines[i].Split(';');
                if (temMas[0].Length > 0)// Исключаем первую строку
                {
                    if (temMas[0].Substring(0, 2) == ", ")
                    {
                        temMas[0] = temMas[0].Substring(2, temMas[0].Length - 2);
                    }
                    if (temMas[0] == listBox1.SelectedItem.ToString())
                    {
                        masMetka[tC] = Convert.ToInt32(temMas[1]);
                        tC++;
                    }
                }
            }

            // Вывод исходных данных под Python
            richTextBox2.Clear();
            string s = "";
            for (int i = 0; i < masMetka.Length; i++)
                s += masMetka[i] + ";";
            richTextBox2.Text = s;

            // Расчет среднего
            int ave=0;
            double summ=0;
            for (int i = 0; i < masMetka.Length; i++)
            {
                summ+=masMetka[i];
            }
            ave = Convert.ToInt32(Math.Round(summ / masMetka.Length,1));
            richTextBox1.Text += "Среднее значение: "+ ave.ToString() + "\r\n";
            if (checkBox3.Checked) 
            {
                chart2.Series.Add("Average");
                chart2.Series["Average"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                //chart2.Series["Average"]
                for (int i = 0; i < masMetka.Length; i++)
                {
                    chart2.Series["Average"].Points.AddXY(i, ave);
                }
            }


            // Расчет графика шума
            chart3.Series.Clear();
            chart3.Series.Add("Noise");
            chart3.Series["Noise"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            for (int i = 0; i < masMetka.Length; i++)
            {
                masMetkaMinAve[i] = masMetka[i]- ave;
                chart3.Series["Noise"].Points.AddXY(i, masMetkaMinAve[i]);
            }

            // Гистограмма шума
            // Надо определить мин и макс и рассортировать среди них
            int maxValue = Convert.ToInt32(Math.Round(masMetka.Max<double>(),1));
            int minValue = Convert.ToInt32(Math.Round(masMetka.Min<double>(), 1));
            richTextBox1.Text += "Минимальное значение: " + minValue.ToString() + "\r\n";
            richTextBox1.Text += "Максимальное значение: " + maxValue.ToString() + "\r\n";
            
            int maxNoiseValue = Convert.ToInt32(Math.Round(masMetkaMinAve.Max<double>(), 1));
            int minNoiseValue = Convert.ToInt32(Math.Round(masMetkaMinAve.Min<double>(), 1));
            richTextBox1.Text += "Минимальное значение шума: " + minNoiseValue.ToString() + "\r\n";
            richTextBox1.Text += "Максимальное значение шума: " + maxNoiseValue.ToString() + "\r\n";
            // Вывод на диаграмму
            // Исходные данные
            for (int i = 0; i < masMetka.Length; i++)
            {
                //richTextBox1.Text += masMetka4[i] + "; ";
                chart2.Series["Sourse"].Points.AddXY(i, masMetka[i]);
            }
            
            // Считаем среднее отклонение от  среднего
            double deltaAve = 0.0;
            for (int i = 0; i < masMetka.Length; i++)
            {
                deltaAve += Math.Abs(ave - masMetka[i]);
            }
            // Построение гистограммы
            int range = maxNoiseValue - minNoiseValue;
            double[] masMetkaMinAveGraph = new double[range]; // График шума
            for (int i = 0; i < range; i++)
                for (int j = 0; j < masMetkaMinAve.Length-1; j++)
                {
                    if (masMetkaMinAve[j] == minNoiseValue + i)
                        masMetkaMinAveGraph[i]++;
                }

            //Вывод на график
            chart1.Series.Clear();
            chart1.Series.Add("GistNoise");
            //chart1.Series["GistNoise"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.;
            for (int i = 0; i < range; i++)
            {
                chart1.Series["GistNoise"].Points.AddXY(minNoiseValue + i, masMetkaMinAveGraph[i]);
            }

            
            // Фильтр Баттерворта
            double[] resmas=new double[masMetka.Length];
            if (checkBox2.Checked)
            { 
                resmas = Butterworth(masMetka, 1, 0.1);

                // Вывод на диаграмму
                for (int i = 0; i < masMetka.Length; i++)
                {
                    chart2.Series["Batterworth"].Points.AddXY(i, resmas[i]);
                }
            }
            // Вывод исходных данных под Python
            richTextBox3.Clear();
            s = "";
            for (int i = 0; i < resmas.Length; i++)
                s += resmas[i] + ";";
            richTextBox3.Text = s;

            // Фильтр Калмана
            var filtered = new List<double>();
            double[] resKalmanMas = new double[masMetka.Length];
            resKalmanMas = masMetka;
            if (checkBox1.Checked)
            {
                
                var kalman = new KalmanFilterSimple1D(f: 1, h: 1, q: 0.1, r: 15); // задаем F, H, Q и R
                kalman.SetState(resKalmanMas[0], 0.1); // Задаем начальные значение State и Covariance
                foreach (var d in resKalmanMas)
                {
                    kalman.Correct(d); // Применяем алгоритм

                    filtered.Add(kalman.State); // Сохраняем текущее состояние
                }


                // Вывод на диаграмму
                for (int i = 0; i < filtered.Count; i++)
                {
                    chart2.Series["Kalman"].Points.AddXY(i, filtered[i]);
                }
            }

            // Вывод исходных данных под Python
            richTextBox4.Clear();
            s = "";
            for (int i = 0; i < filtered.Count; i++)
                s += filtered[i] + ";";
            richTextBox4.Text = s;

            // Настраиваем оси графика

            chart2.ChartAreas[0].AxisY.Maximum = Convert.ToInt32(textBox1.Text);
            chart2.ChartAreas[0].AxisY.Minimum = Convert.ToInt32(textBox2.Text);



            // Расчет отклонения от среднего

            double deltaKalman = 0.0;
            double deltaBatter = 0.0;

            for (int i = 0; i < masMetka.Length; i++)
            {
                if (checkBox1.Checked)
                    deltaKalman += Math.Abs(filtered[i] - ave);
                if (checkBox2.Checked)
                    deltaBatter += Math.Abs(resmas[i] - ave);
            }
            richTextBox1.Text += "Суммарное отклоение от среднего: " + deltaAve.ToString() + "\r\n";
            richTextBox1.Text += "Среднее отклоение от среднего: " + (deltaAve / masMetka.Length).ToString() + "\r\n";
            if (checkBox1.Checked)
            {
                richTextBox1.Text += "Суммарное отклоение от Калмана: " + deltaKalman.ToString() + " (" + Math.Round((deltaKalman / deltaAve) * 100, 2) + "%)" + "\r\n";
                richTextBox1.Text += "Среднее отклоение от Калмана: " + (deltaKalman / masMetka.Length).ToString() + " (" + Math.Round((deltaKalman / deltaAve) * 100, 2) + "%)" + "\r\n";
                richTextBox1.Text += "СКО уменьшилась на: " + ((deltaKalman / masMetka.Length) - (deltaAve / masMetka.Length)) + " (" + Math.Round((deltaKalman / deltaAve) * 100, 2) + "%)" + "\r\n";
            }
                if (checkBox2.Checked)
                richTextBox1.Text += "Суммарное отклоение от Баттерворта: " + deltaBatter.ToString() + " (" + Math.Round((deltaBatter / deltaAve) * 100,2) + "%)" + "\r\n";

           
        }
    }
}