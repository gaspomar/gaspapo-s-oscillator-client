using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime;

namespace OscillatorClient
{
    enum Signal
    {
        Sine        = 48,
        Triangle    = 49,
        Pulse       = 50
    }

    enum Command
    {
        WAVEFORM1 = 48,   // '0'
        FREQUENCY1 = 49,   // '1'
        AMP1 = 50,
        PARAM1 = 51,
        OUTPUT = 52,
        WAVEFORM2 = 53,
        FREQUENCY2 = 54,
        PARAM2 = 55,
        MOD_DEPTH = 56
    }

    public partial class Form1 : Form
    {
        SerialPort serialPort;

        private Signal signal1;
        private short freq1;
        private short fillFactor1;
        private short riseTime1;
        private double amp;

        private Signal signal2;
        private short freq2;
        private short fillFactor2;
        private short riseTime2;
        private double modDepth;


        private Label paramLabel1;
        private ComboBox paramList1;

        private Label paramLabel2;
        private ComboBox paramList2;
        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort("COM16", 9600, Parity.None, 8, StopBits.One);
        }

        private void waveformList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (waveformList1.SelectedItem.ToString() == "Sine")
            {
                signal1 = Signal.Sine;
                if (groupBox1.Controls.Contains(paramList1))
                {
                    groupBox1.Controls.Remove(paramList1);
                    groupBox1.Controls.Remove(paramLabel1);
                    paramList1 = null;
                    paramLabel1 = null;
                }
                groupBox1.Refresh();
            }

            if (waveformList1.SelectedItem.ToString() == "Pulse")
            {
                signal1 = Signal.Pulse;
                if (groupBox1.Controls.Contains(paramList1))
                {
                    groupBox1.Controls.Remove(paramList1);
                    groupBox1.Controls.Remove(paramLabel1);
                    paramList1 = null;
                    paramLabel1 = null;
                }
                paramLabel1 = new Label();
                paramLabel1.AutoSize = true;
                paramLabel1.Location = new System.Drawing.Point(58, 185);
                paramLabel1.Name = "paramLabel";
                paramLabel1.Size = new System.Drawing.Size(66, 17);
                paramLabel1.TabIndex = 10;
                paramLabel1.Text = "Fill factor";
                groupBox1.Controls.Add(paramLabel1);
                groupBox1.Refresh();

                paramList1 = new ComboBox();
                paramList1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                paramList1.FormattingEnabled = true;
                paramList1.Items.AddRange(new object[] {
                    "10 %",
                    "20 %",
                    "30 %",
                    "40 %",
                    "50 %",
                    "60 %",
                    "70 %",
                    "80 %",
                    "90 %",
                    "100 %"});
                paramList1.Location = new System.Drawing.Point(160, 185);
                paramList1.Name = "fillFactorList";
                paramList1.Size = new System.Drawing.Size(121, 24);
                paramList1.TabIndex = 11;
                paramList1.SelectedIndexChanged += new System.EventHandler(this.fillFactorList1_SelectedIndexChanged);
                groupBox1.Controls.Add(paramList1);
                groupBox1.Refresh();
            }

            if (waveformList1.SelectedItem.ToString() == "Triangle")
            {
                signal1 = Signal.Triangle;
                if (groupBox1.Controls.Contains(paramList1))
                {
                    groupBox1.Controls.Remove(paramList1);
                    groupBox1.Controls.Remove(paramLabel1);
                    paramList1 = null;
                    paramLabel1 = null;
                }
                paramLabel1 = new Label();
                paramLabel1.AutoSize = true;
                paramLabel1.Location = new System.Drawing.Point(58, 185);
                paramLabel1.Name = "paramLabel";
                paramLabel1.Size = new System.Drawing.Size(66, 17);
                paramLabel1.TabIndex = 10;
                paramLabel1.Text = "Rise time";
                groupBox1.Controls.Add(paramLabel1);
                groupBox1.Refresh();

                paramList1 = new ComboBox();
                paramList1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                paramList1.FormattingEnabled = true;
                paramList1.Items.AddRange(new object[] {
                    "10 %",
                    "20 %",
                    "30 %",
                    "40 %",
                    "50 %",
                    "60 %",
                    "70 %",
                    "80 %",
                    "90 %",
                    "100 %"});
                paramList1.Location = new System.Drawing.Point(160, 185);
                paramList1.Name = "riseTimeList";
                paramList1.Size = new System.Drawing.Size(121, 24);
                paramList1.TabIndex = 11;
                paramList1.SelectedIndexChanged += new System.EventHandler(this.riseTimeList1_SelectedIndexChanged);
                groupBox1.Controls.Add(paramList1);
                groupBox1.Refresh();
            }
            Trace.WriteLine($"signal1 set to {signal1.ToString()}");
        }

        private void freqTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (freqTextBox1.Text != "")
            {
                try
                {
                    if (Int16.Parse(freqTextBox1.Text) > 20000)
                    {
                        MessageBox.Show("Given value too big!\nValid range: 1 - 20000 Hz");
                        freqTextBox1.Text = "";
                    }
                    else if (Int16.Parse(freqTextBox1.Text) < 1)
                    {
                        MessageBox.Show("Given value too small!\nValid range: 1 - 20000 Hz");
                        freqTextBox1.Text = "";
                    }
                    else
                    {
                        freq1 = Int16.Parse(freqTextBox1.Text);
                        Trace.WriteLine($"freq1 set to {freq1}");
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid format!");
                    freqTextBox1.Text = "";
                }
            }
        }

        private void ampTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ampTextBox.Text != "")
            {
                try
                {
                    if (Double.Parse(ampTextBox.Text) > 3.3)
                    {
                        MessageBox.Show("Given value too big!\nValid range: 0 - 3.3 V");
                        ampTextBox.Text = "";
                    }
                    else if (Double.Parse(ampTextBox.Text) < 0)
                    {
                        MessageBox.Show("Negative values not supported!\nValid range: 0 - 3.3 V");
                        ampTextBox.Text = "";
                    }
                    else
                    {
                        amp = Double.Parse(ampTextBox.Text);
                        Trace.WriteLine($"Amp set to {amp}");
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid format!");
                    ampTextBox.Text = "";
                }
            }
        }

        private void fillFactorList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (paramList1.SelectedItem.ToString())
            {
                case "0 %":
                    fillFactor1 = 0;
                    break;

                case "10 %":
                    fillFactor1 = 10;
                    break;

                case "20 %":
                    fillFactor1 = 20;
                    break;

                case "30 %":
                    fillFactor1 = 30;
                    break;

                case "40 %":
                    fillFactor1 = 40;
                    break;

                case "50 %":
                    fillFactor1 = 50;
                    break;

                case "60 %":
                    fillFactor1 = 60;
                    break;

                case "70 %":
                    fillFactor1 = 70;
                    break;

                case "80 %":
                    fillFactor1 = 80;
                    break;

                case "90 %":
                    fillFactor1 = 90;
                    break;

                case "100 %":
                    fillFactor1 = 100;
                    break;
            }
            Trace.WriteLine($"fillFactor1 set to {fillFactor1}");
        }

        private void riseTimeList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (paramList1.SelectedItem.ToString())
            {
                case "0 %":
                    riseTime1 = 0;
                    break;

                case "10 %":
                    riseTime1 = 10;
                    break;

                case "20 %":
                    riseTime1 = 20;
                    break;

                case "30 %":
                    riseTime1 = 30;
                    break;

                case "40 %":
                    riseTime1 = 40;
                    break;

                case "50 %":
                    riseTime1 = 50;
                    break;

                case "60 %":
                    riseTime1 = 60;
                    break;

                case "70 %":
                    riseTime1 = 70;
                    break;

                case "80 %":
                    riseTime1 = 80;
                    break;

                case "90 %":
                    riseTime1 = 90;
                    break;

                case "100 %":
                    riseTime1 = 100;
                    break;
            }
            Trace.WriteLine($"riseTime1 set to {riseTime1}");
        }



        /**************************************************************************************
         *      Modulator signal
         **************************************************************************************/
        private void waveformList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (waveformList2.SelectedItem.ToString() == "Sine")
            {
                signal2 = Signal.Sine;
                if (groupBox2.Controls.Contains(paramList2))
                {
                    groupBox2.Controls.Remove(paramList2);
                    groupBox2.Controls.Remove(paramLabel2);
                    paramList2 = null;
                    paramLabel2 = null;
                }
                groupBox2.Refresh();
            }

            if (waveformList2.SelectedItem.ToString() == "Pulse")
            {
                signal2 = Signal.Pulse;
                if (groupBox2.Controls.Contains(paramList2))
                {
                    groupBox2.Controls.Remove(paramList2);
                    groupBox2.Controls.Remove(paramLabel2);
                    paramList2 = null;
                    paramLabel2 = null;
                }
                paramLabel2 = new Label();
                paramLabel2.AutoSize = true;
                paramLabel2.Location = new System.Drawing.Point(58, 185);
                paramLabel2.Name = "paramLabel";
                paramLabel2.Size = new System.Drawing.Size(66, 17);
                paramLabel2.TabIndex = 10;
                paramLabel2.Text = "Fill factor";
                groupBox2.Controls.Add(paramLabel2);
                groupBox2.Refresh();

                paramList2 = new ComboBox();
                paramList2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                paramList2.FormattingEnabled = true;
                paramList2.Items.AddRange(new object[] {
                    "10 %",
                    "20 %",
                    "30 %",
                    "40 %",
                    "50 %",
                    "60 %",
                    "70 %",
                    "80 %",
                    "90 %",
                    "100 %"});
                paramList2.Location = new System.Drawing.Point(160, 185);
                paramList2.Name = "fillFactorList";
                paramList2.Size = new System.Drawing.Size(121, 24);
                paramList2.TabIndex = 11;
                paramList2.SelectedIndexChanged += new System.EventHandler(this.fillFactorList2_SelectedIndexChanged);
                groupBox2.Controls.Add(paramList2);
                groupBox2.Refresh();
            }

            if (waveformList2.SelectedItem.ToString() == "Triangle")
            {
                signal2 = Signal.Triangle;
                if (groupBox2.Controls.Contains(paramList2))
                {
                    groupBox2.Controls.Remove(paramList2);
                    groupBox2.Controls.Remove(paramLabel2);
                    paramList2 = null;
                    paramLabel2 = null;
                }
                paramLabel2 = new Label();
                paramLabel2.AutoSize = true;
                paramLabel2.Location = new System.Drawing.Point(58, 185);
                paramLabel2.Name = "paramLabel";
                paramLabel2.Size = new System.Drawing.Size(66, 17);
                paramLabel2.TabIndex = 10;
                paramLabel2.Text = "Rise time";
                groupBox2.Controls.Add(paramLabel2);
                groupBox2.Refresh();

                paramList2 = new ComboBox();
                paramList2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                paramList2.FormattingEnabled = true;
                paramList2.Items.AddRange(new object[] {
                    "10 %",
                    "20 %",
                    "30 %",
                    "40 %",
                    "50 %",
                    "60 %",
                    "70 %",
                    "80 %",
                    "90 %",
                    "100 %"});
                paramList2.Location = new System.Drawing.Point(160, 185);
                paramList2.Name = "riseTimeList";
                paramList2.Size = new System.Drawing.Size(121, 24);
                paramList2.TabIndex = 11;
                paramList2.SelectedIndexChanged += new System.EventHandler(this.riseTimeList2_SelectedIndexChanged);
                groupBox2.Controls.Add(paramList2);
                groupBox2.Refresh();
            }
            Trace.WriteLine($"signal2 set to {signal2.ToString()}");
        }

        private void fillFactorList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (paramList2.SelectedItem.ToString())
            {
                case "0 %":
                    fillFactor2 = 0;
                    break;

                case "10 %":
                    fillFactor2 = 10;
                    break;

                case "20 %":
                    fillFactor2 = 20;
                    break;

                case "30 %":
                    fillFactor2 = 30;
                    break;

                case "40 %":
                    fillFactor2 = 40;
                    break;

                case "50 %":
                    fillFactor2 = 50;
                    break;

                case "60 %":
                    fillFactor2 = 60;
                    break;

                case "70 %":
                    fillFactor2 = 70;
                    break;

                case "80 %":
                    fillFactor2 = 80;
                    break;

                case "90 %":
                    fillFactor2 = 90;
                    break;

                case "100 %":
                    fillFactor2 = 100;
                    break;
            }
            Trace.WriteLine($"fillFactor2 set to {fillFactor2}");
        }

        private void riseTimeList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (paramList2.SelectedItem.ToString())
            {
                case "0 %":
                    riseTime2 = 0;
                    break;

                case "10 %":
                    riseTime2 = 10;
                    break;

                case "20 %":
                    riseTime2 = 20;
                    break;

                case "30 %":
                    riseTime2 = 30;
                    break;

                case "40 %":
                    riseTime2 = 40;
                    break;

                case "50 %":
                    riseTime2 = 50;
                    break;

                case "60 %":
                    riseTime2 = 60;
                    break;

                case "70 %":
                    riseTime2 = 70;
                    break;

                case "80 %":
                    riseTime2 = 80;
                    break;

                case "90 %":
                    riseTime2 = 90;
                    break;

                case "100 %":
                    riseTime2 = 100;
                    break;
            }
            Trace.WriteLine($"riseTime2 set to {riseTime2}");
        }

        private void freqTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (freqTextBox2.Text != "")
            {
                try
                {
                    if (Int16.Parse(freqTextBox2.Text) > 20000)
                    {
                        MessageBox.Show("Given value too big!\nValid range: 1 - 20000 Hz");
                        freqTextBox2.Text = "";
                    }
                    else if (Int32.Parse(freqTextBox2.Text) < 1)
                    {
                        MessageBox.Show("Given value too small!\nValid range: 1 - 20000 Hz");
                        freqTextBox2.Text = "";
                    }
                    else
                    {
                        freq2 = Int16.Parse(freqTextBox2.Text);
                        Trace.WriteLine($"freq2 set to {freq2}");
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid format!");
                    freqTextBox2.Text = "";
                }
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen == false) { return; }

            byte[] txBuffer = new byte[3];

            // Sending waveform1
            txBuffer[0] = (byte)Command.WAVEFORM1;
            txBuffer[1] = (byte)signal1;
            serialPort.Write(txBuffer, 0, 3);

            // Sending frequency1
            txBuffer[0] = (byte)Command.FREQUENCY1;
            txBuffer[1] = (byte)freq1;
            txBuffer[2] = (byte)(freq1 >> 8);
            serialPort.Write(txBuffer, 0, 3);

            // Sending amp
            txBuffer[0] = (byte)Command.AMP1;
            txBuffer[1] = (byte)((int)(amp+0.05));
            var tmp = (amp + 0.05) - (int)(amp + 0.05);
            tmp *= 10;
            txBuffer[2] = (byte)tmp;
            serialPort.Write(txBuffer, 0, 3);

            // Sending param1
            txBuffer[0] = (byte)Command.PARAM1;
            if (signal1 == Signal.Triangle)
            {
                txBuffer[1] = (byte)(riseTime1);
                serialPort.Write(txBuffer, 0, 3);
            }
            if (signal1 == Signal.Pulse)
            {
                txBuffer[1] = (byte)(fillFactor1);
                serialPort.Write(txBuffer, 0, 3);
            }

            // Sending waveform2
            txBuffer[0] = (byte)Command.WAVEFORM2;
            txBuffer[1] = (byte)signal2;
            serialPort.Write(txBuffer, 0, 3);

            // Sending frequency2
            txBuffer[0] = (byte)Command.FREQUENCY2;
            txBuffer[1] = (byte)freq2;
            txBuffer[2] = (byte)(freq2 >> 8);
            serialPort.Write(txBuffer, 0, 3);

            // Sending mod depth
            txBuffer[0] = (byte)Command.MOD_DEPTH;
            txBuffer[1] = (byte)((int)(modDepth + 0.05));
            tmp = (modDepth + 0.05) - (int)(modDepth + 0.05);
            tmp *= 10;
            txBuffer[2] = (byte)tmp;
            serialPort.Write(txBuffer, 0, 3);
            serialPort.Write(txBuffer, 0, 3);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if(serialPort.IsOpen == true)
            {
                try { serialPort.Close(); }
                catch 
                {
                    MessageBox.Show("Cannot disconnect from port!");
                    return;
                }
                connectButton.ForeColor = Color.Coral;
                connectButton.Text = "Connect";
            }

            else
            {
                try { serialPort.PortName = textBox1.Text; }
                catch { return; }

                try { serialPort.Open(); }
                catch
                {
                    MessageBox.Show("Port unavailable!");
                    textBox1.Text = "";
                    return;
                }
                connectButton.ForeColor = Color.LimeGreen;
                connectButton.Text = "Connected";
            }
            
            Invalidate();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                try
                {
                    if (Double.Parse(textBox2.Text) > 1.0)
                    {
                        MessageBox.Show("Given value too big!\nValid range: 0 - 1.0");
                        ampTextBox.Text = "";
                    }
                    else if (Double.Parse(textBox2.Text) < 0)
                    {
                        MessageBox.Show("Negative values not supported!\nValid range: 0 - 1.0");
                        ampTextBox.Text = "";
                    }
                    else
                    {
                        modDepth = Double.Parse(textBox2.Text);
                        Trace.WriteLine($"Amp set to {modDepth}");
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid format!");
                    ampTextBox.Text = "";
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen == false) { return; }

            byte[] txBuffer = new byte[3];

            // Sending output
            txBuffer[0] = (byte)Command.OUTPUT;
            txBuffer[1] = (byte)1;
            serialPort.Write(txBuffer, 0, 3);

            button2.Text = "Output ON";
            button2.ForeColor = Color.LimeGreen;
            Invalidate();
        }
    }
}
