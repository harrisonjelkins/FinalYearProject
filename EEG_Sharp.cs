//using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.WindowsAPICodePack.Dialogs;
using FFTransform;

namespace EEG_Sharp
{
    public partial class eEEG : Form
    {
        Read rr;
        public static int sampleNumber = 2500; //1500; // initial sampling number 
        //public static Complex[] samples0f = new Complex[sampleNumber];
        //static int sampleRate = 972;
        int blockSize = 10000;
        const int streamBufferSize = 4096;
        class Read
        {
            private string[] header;
            private float[,] data;
            private int nLines;
            private int nColumns;
            DateTime startTime = DateTime.Now;
            public Read(Stream myStream)
            {
                string aux;
                string[] pieces;


                //read the file line by line
                //  BufferedStream bs = new BufferedStream(myStream, streamBufferSize);
                Console.WriteLine("Process started at " + startTime.ToString("HH-mm-ss"));
                StreamReader sr = new StreamReader(myStream);
                aux = sr.ReadLine();
                header = aux.Split(',');
                nColumns = header.Length;
                nLines = 0;
                while ((aux = sr.ReadLine()) != null)
                {
                    if (aux.Length > 0) nLines++;
                }

                //StreamWriter sw = new StreamWriter(myStream);

                //read the numerical data from file in an array
                data = new float[nLines, nColumns];
                sr.BaseStream.Seek(0, 0);
                sr.ReadLine();
                for (int i = 0; i < nLines; i++)
                {
                    aux = sr.ReadLine();
                    pieces = aux.Split(',');
                    for (int j = 0; j < nColumns; j++)
                    {
                        data[i, j] = float.Parse(pieces[j]);
                    }

                }
                string msg = string.Format("Loaded in {0} lines and {1} columns of data!", nLines, nColumns);
                Console.WriteLine(msg);
                DateTime finishTime = DateTime.Now;
                Console.WriteLine("Process finished at " + finishTime.ToString("HH-mm-ss"));

                sr.Close();

            }

            //functions used for retrieving the data
            public int get_nLines()
            {
                return nLines;
            }

            public int get_nColumns()
            {
                return nColumns;
            }

            public float[,] get_Data()
            {
                return data;
            }

            public string[] get_Header()
            {
                return header;
            }


            // Function used to create data blocks
            public void CreateDataBlocks(int nLines, int nColumns, float[,] data)
            {
                const int blockSize = 4096;
                int i = 0;
                int j = 0;
                int n = 0;
                //int progress = 0;

                int nBlocks = (nLines / blockSize) + 1;
                //Console.WriteLine("Number of Blocks Needed: %d", nBlocks);
                float[,] dataBlock = new float[blockSize, nColumns];
                string[] dataHeader = new string[nColumns];
                FileStream fs;

                DateTime startTime = DateTime.Now;

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                try
                {
                    dialog.InitialDirectory = "C:\\Users";
                    dialog.IsFolderPicker = true;
                }
                catch
                {
                    MessageBox.Show("Unkown Error. Please Try Again");
                }

                Console.WriteLine("Processs started at" + startTime.ToString("HH:mm:ss"));

                for (n = 0; n < nBlocks; n++)
                {
                    string fileName = @"C:\Users\Harri\Documents\Thesis Bin\" + startTime.ToString("dd-MM-yyyy-HH-mm");
                    fileName = String.Concat(fileName, "-", n, ".txt");
                    Console.WriteLine(fileName);

                    fs = File.Create(fileName);
                    //File.WriteAllLines(fileName,dataHeader);
                    StreamWriter sw = new StreamWriter(fs);

                    for (i = 0; i < blockSize; i++)
                    {
                        string dataString = i.ToString();

                        if ((blockSize * n) + i < nLines)
                        {
                            for (j = 0; j < nColumns; j++)
                            {
                                dataBlock[i, j] = data[(blockSize * n) + i, j];
                                if (j != 0)
                                {
                                    dataString = string.Concat(dataString, ",", dataBlock[i, j]);
                                }

                            }
                            sw.WriteLine(dataString);
                            //Console.WriteLine(dataString);
                        }

                        //sw.WriteLine(dataString);
                        //Console.WriteLine(dataString);

                    }
                    sw.Flush();
                    sw.Close();
                    Console.WriteLine("Block {0} of {1} Completed", (n + 1), nBlocks);
                }
                string message = string.Format("Successfully Saved {0} Blocks", nBlocks);
                DateTime finishTime = DateTime.Now;
                Console.WriteLine(message + finishTime.ToString("HH:mm:ss"));
                MessageBox.Show(message);

                //DateTime elapsed = DateTime.Now.Subtract(startTime);
            }

            /*public string findFile(string fileName, int block)
            {
                //const int block = 0;
                string newName;
                string[] nameParts = fileName.Split('-');
                newName = nameParts[0];
                for (int i = 1; i < (nameParts.Length - 1); i++)
                {
                    newName = string.Concat(newName + '-' + nameParts[i]);
                    Console.WriteLine(nameParts[i]);
                }

                Console.WriteLine(newName.Length);

                //nameParts = nameParts[0].Split('.');

                newName = string.Concat(newName, '-', block);
                newName = string.Concat(newName, ".txt");

                Console.WriteLine(newName);
                return newName;
            }*/

            public void DataBlockFFT()
            {
                //Import (ANY) Data Block
                float[] input;
                Complex[] output;

                OpenFileDialog ff = new OpenFileDialog();

                Stream readFile;
                //Stream writeFile;
                Read rr = null;
                //string fileName;

                ff.InitialDirectory = "c:\\";
                ff.Filter = "txt files (*.txt)|*.txt |All files (*.*)|*.*";
                ff.FilterIndex = 1;
                ff.RestoreDirectory = true;

                if (ff.ShowDialog() == DialogResult.OK)
                {
                    //MessageBox.Show(ff.FileName + " Opended Successfully!");

                    readFile = new FileStream(ff.FileName, FileMode.Open);
                    rr = new Read(readFile);
                    if (rr.get_nLines() < 4096 || rr.get_nColumns() <= 2)
                    {
                        float[,] data = rr.get_Data();
                        int L = rr.get_nLines();
                        input = new float[L];

                        for (int i = 0; i < L; i++)
                        {
                            input[i] = data[i, 1];
                        }

                        output = Fourier.FloatFFT(input);

                        Fourier.FFTResults(output, 100);

                        MessageBox.Show("Process completed");
                    }
                    else
                        MessageBox.Show("File too Big");
                }

                else
                {
                    //fileName = ff.FileName;
                    MessageBox.Show(ff.FileName + "Does NOT exist!");
                }

                return;

            }

            public void DataBlockFIR()
            {
                return;
            }

        }
        public eEEG()
        {
            InitializeComponent();
            ((ISupportInitialize)(chart1)).BeginInit();
            Bitmap field1 = new Bitmap(chart1.Width, chart1.Height);
            chart1.DrawToBitmap(field1, new Rectangle(chart1.Location, chart1.Size));

            ChartArea ca1 = chart1.ChartAreas[0];
            ChartArea ca2 = chart1.ChartAreas[1];

            Axis ax1 = ca1.AxisX;
            Axis ax2 = ca2.AxisX;

            Axis ay1 = ca1.AxisY;
            Axis ay2 = ca2.AxisY;

            ax1.ScaleView.Zoomable = true;
            ax2.ScaleView.Zoomable = true;
            ay1.ScaleView.Zoomable = true;
            ay2.ScaleView.Zoomable = true;

            ca1.CursorX.IsUserSelectionEnabled = true;
            ca2.CursorX.IsUserSelectionEnabled = true;



            // chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            // chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
             this.chart1.MouseWheel += Chart_MouseWheel;
            // chart1.ChartAreas[0].CursorX.AutoScroll = true;
            // chart1.ChartAreas[1].CursorX.AutoScroll = true;
            // chart.ChartAreas[0].AxisX.ScaleView.SizeType = DateTimeIntervalType.Number;
             int position = 0;
             int size = blockSize;
             ca1.AxisX.ScaleView.Zoom(position, size);
             ca2.AxisX.ScaleView.Zoom(position, size);
            

            // disable zoom-reset button (only scrollbar's arrows are available)
            ca1.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
             ca2.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;


            // set scrollbar small change to blockSize (e.g. 100)
            ca1.AxisX.ScaleView.SmallScrollSize = blockSize;
            ca2.AxisX.ScaleView.SmallScrollSize = blockSize;
            ((ISupportInitialize)(chart1)).EndInit();

        }

        private void dataPlotting()
        {
             
            int indX = xBox.SelectedIndex;
            int indY = yBox.SelectedIndex;
            int indY2 = yBox2.SelectedIndex;
            float[,] data = rr.get_Data();
            int nLines = rr.get_nLines();
            int nColumns = rr.get_nColumns();
            string[] header = rr.get_Header();
            chart1.Series["CH1_data"].ChartArea = "CH1";
            chart1.Series["FCH1_data"].ChartArea = "FCH1";
            chart1.Series["CH1_data"].Points.Clear(); //ensure that the chart is empty
            chart1.Series["FCH1_data"].Points.Clear();
            
            //chart1.Series.Add("Series0");
            chart1.Series[0].ChartType = SeriesChartType.FastLine;
           // chart1.ChartAreas[0].AxisX.LabelStyle.Format = "{F2}";
            //chart1.ChartAreas[0].AxisX.Title = header[indX];
            //chart1.ChartAreas[0].AxisY.Title = header[indY];           
            chart1.Legends.Clear();
            for (int j = 0; j < nLines; j++)
            {
                chart1.Series["CH1_data"].Points.AddXY(data[j, indX], data[j, indY]);
                chart1.Series["FCH1_data"].Points.AddXY(data[j, indX], data[j, indY2]);
            }
        }

        public static double[] NotchFilter(double[] Input, double[] Output, double f, double BW)
        {
            double x_2 = 0.0;
            double x_1 = 0.0;
            double y_2 = 0.0;
            double y_1 = 0.0;

            double R = 1 - 3 * BW;
            double K = (1 - 2 * R * Math.Cos(2 * Math.PI * f) + Math.Pow(R, 2)) / (2 - 2 * Math.Cos(2 * Math.PI * f));
            double a0 = K;
            double a1 = -2 * K * Math.Cos(2 * Math.PI * f);
            double a2 = K;
            double b1 = 2 * R * Math.Cos(2 * Math.PI * f);
            double b2 = -Math.Pow(R, 2);

            for (int i = 0; i < sampleNumber; ++i)
            {
                Output[i] = a0 * Input[i] + a1 * x_1 + a2 * x_2 + b1 * y_1 + b2 * y_2;
                x_2 = x_1;
                x_1 = Input[i];
                y_2 = y_1;
                y_1 = Output[i];
            }
            return Output;
        }

        /*public void FFTPlotting()
        {
           // samples0f = new Complex(Convert.ToDouble(d / 65.0), 0); //(d, 0); //
            //FFTplot2.Series["FrequencyCh0"].Points.Clear();
            

            Fourier.Forward(eEEG.samples0f, FourierOptions.Default);
            

            //FFTplot2.ChartAreas[0].AxisX.Maximum = 100;
            //FFTplot2.ChartAreas[0].AxisX.Minimum = 0;
            //FFTplot2.ChartAreas[0].AxisX.Interval = 10;
            //FFTplot2.ChartAreas["ChartArea1"].AxisX.MinorTickMark.Enabled = true;
            //FFTplot2.ChartAreas[0].AxisY.Maximum = 10000000000;
            //FFTplot2.ChartAreas[0].AxisY.Minimum = 0;
            //FFTplot2.ChartAreas[0].AxisY.Interval = 10000000000 / 2;


            double Correction_Factor = 0.5;

            for (int i = 1; i < eEEG.sampleNumber; i++)
            {
                double magf1 = (eEEG.sampleNumber) * (Math.Abs(Math.Pow(eEEG.samples0f[i].Real, 2) + Math.Pow(eEEG.samples0f[i].Imaginary, 2)));
                

                double hzPerSamplef1 = ((1 * Correction_Factor) / eEEG.sampleNumber);
                
                double iDummy = Convert.ToDouble(i);
                double time = iDummy;

                //FFTplot2.Series["FrequencyCh0"].Points.AddXY(hzPerSamplef1 * time, magf1);
               
            }

            return;
        }*/
        

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog ff = new OpenFileDialog();

            ff.InitialDirectory = "c:\\";
            ff.Filter = "csv files (*.csv)|*.csv| txt files (*.txt)|*.txt |All files (*.*)|*.*";
            ff.FilterIndex = 1;
            ff.RestoreDirectory = true;

            if (ff.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = ff.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            rr = null;
                            rr = new Read(myStream);
                            string[] header = rr.get_Header();
                            List<string> lX = new List<string>();
                            List<string> lY = new List<string>();
                            List<string> lY1 = new List<string>();

                            lX.Add(header[0]);
                          //  lY.Add(header[1]);
                          //  lY1.Add(header[2]);

                           for (int i = 0; i < header.Length; i++)
                            {
                                lY.Add(header[i]); // lX.Add(header[0]);
                                lY1.Add(header[i]);
                            }
                            //Populate the ComboBoxes
                            xBox.DataSource = lX;
                            yBox.DataSource = lY;
                            yBox2.DataSource = lY1;

                            // Close the stream
                            myStream.Close();
                        }
                    }
                }
                catch (Exception err)
                {
                    //Inform the user if we can't read the file
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog ff = new SaveFileDialog();

            ff.Filter = "jpg files (*.jpg)|*.jpg |All files (*.*)|*.*";
            ff.FilterIndex = 1;
            ff.RestoreDirectory = true;

            if (ff.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = ff.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        chart1.SaveImage(myStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }
        }

        private void btnPlot_Click(object sender, EventArgs e)
        {
            if (rr != null)
            {
                chart1.Series["CH1_data"].Points.Clear();
                chart1.Series["FCH1_data"].Points.Clear();
                // Plot pl = new Plot(rr, xBox, yBox);
                dataPlotting();                  
                
            }
            else
            {
                MessageBox.Show("Error, no data to plot! Please load csv file");
                return;
            }
        }

        private void Chart_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;
            var yAxis = chart.ChartAreas[0].AxisY;
            var xAxis1 = chart.ChartAreas[1].AxisX;
            var yAxis1 = chart.ChartAreas[1].AxisY;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {

                    // xAxis.ScaleView.ZoomReset();
                    //  yAxis.ScaleView.ZoomReset();
                    int position = 0;
                    int size = blockSize;
                    chart.ChartAreas[0].AxisX.ScaleView.Zoom(position, size);
                    chart.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
                    chart.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = blockSize;
                    chart.ChartAreas[1].AxisX.ScaleView.Zoom(position, size);
                    chart.ChartAreas[1].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
                    chart.ChartAreas[1].AxisX.ScaleView.SmallScrollSize = blockSize;
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;
                    var xMin1 = xAxis1.ScaleView.ViewMinimum;
                    var xMax1 = xAxis1.ScaleView.ViewMaximum;
                    var yMin1 = yAxis1.ScaleView.ViewMinimum;
                    var yMax1 = yAxis1.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 2.5;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 2.5;
                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 2.5;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 2.5;
                    var posXStart1 = xAxis1.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 2.5;
                    var posXFinish1 = xAxis1.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 2.5;
                    var posYStart1 = yAxis1.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 2.5;
                    var posYFinish1 = yAxis1.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 2.5;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);
                    xAxis1.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis1.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { }
        }


        private void GraphDemo_Load(object sender, EventArgs e)
        {
            chart1.Legends.Clear();
        }

        private void chart1_AxisViewChanged(object sender, ViewEventArgs e)
        {
            ChartArea ca1 = chart1.ChartAreas[0];
            ChartArea ca2 = chart1.ChartAreas[1];
            Axis ax1 = ca1.AxisX;
            Axis ax2 = ca2.AxisX;

            if (e.Axis == ax1)
            {
                ax2.ScaleView.Size = ax1.ScaleView.Size;
                ax2.ScaleView.Position = ax1.ScaleView.Position;
            }
            if (e.Axis == ax2)
            {
                ax1.ScaleView.Size = ax2.ScaleView.Size;
                ax1.ScaleView.Position = ax2.ScaleView.Position;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int lines = rr.get_nLines();
                int columns = rr.get_nColumns();
                float[,] data = rr.get_Data();
                //int blockSize = lines / 100;

                dataBlocksExport.Text = "Export Data Blocks";
                rr.CreateDataBlocks(lines, columns, data);
            }
            catch
            {
                MessageBox.Show("No File Selected");
            }
            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if(rr == null)
            {
                MessageBox.Show("Error. Load some Data plz");
            }
            else
            {
                rr.DataBlockFFT();
                //MessageBox.Show("Process Successfully Completed?");
            }
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }
    }
}

   
    
   

