using Cognex.DataMan.SDK;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataManApp
{
    public partial class Calibrate : Form
    {
        private static System.Windows.Forms.Timer timer;
        private static double focusSet = 0, gainSet = 0, exposureSet = 0;
        private void TimerTick(object sender, EventArgs args)
        {
            pictureBox1.Image = Discovered.connector.GetLiveImage(ImageFormat.bitmap, ImageSize.Full, ImageQuality.High);
        }
        private void GetItems()
        {
            DmccResponse fSet = Discovered.connector.SendCommand("get focus.power");
            focusSet = Double.Parse(fSet.PayLoad) / 100;
            DmccResponse eSet = Discovered.connector.SendCommand("get camera.exposure");
            exposureSet = Double.Parse(eSet.PayLoad);
            DmccResponse gSet = Discovered.connector.SendCommand("get camera.gain");
            gainSet = Double.Parse(gSet.PayLoad) / 100;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            GetItems();

            if (comboBox1.SelectedIndex == 0)
            {
                if (focusSet >= 14.98)
                {
                    label1.Text = "Kan ikke gå højere op";
                }
                else
                {
                    focusSet += 0.05;
                    string focusSetString = focusSet.ToString().Replace(",", ".");
                    Discovered.connector.SendCommand("set focus.power " + focusSetString);
                    label1.Text = "Focus er sat til " + focusSetString;
                }
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                if (gainSet >= 125.54)
                {
                    label1.Text = "Kan ikke gå højere op";
                }
                else
                {
                    gainSet += 0.05;
                    string gainSetString = gainSet.ToString().Replace(",", ".");
                    Discovered.connector.SendCommand("set camera.gain " + gainSetString);
                    label1.Text = "Gain er sat til " + gainSetString;
                }
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                if (exposureSet >= 190000)
                {
                    label1.Text = "Kan ikke gå højere op";
                }
                else
                {
                    exposureSet += 1000;
                    string expoSetString = exposureSet.ToString().Replace(",", ".");
                    Discovered.connector.SendCommand("set camera.exposure " + expoSetString);
                    label1.Text = "Exposure er sat til " + expoSetString;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetItems();

            if (comboBox1.SelectedIndex == 0)
            {
                if (focusSet <= -6.19)
                {
                    label1.Text = "Kan ikke gå længere ned";
                }
                else
                {
                    focusSet -= 0.05;
                    string focusSetString = focusSet.ToString().Replace(",", ".");
                    Discovered.connector.SendCommand("set focus.power " + focusSetString);
                    label1.Text = "Focus er sat til " + focusSetString;
                }
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                if (gainSet <= 1.05)
                {
                    label1.Text = "Kan ikke gå længere ned";
                }
                else
                {
                    gainSet -= 0.05;
                    string gainSetString = gainSet.ToString().Replace(",", ".");
                    Discovered.connector.SendCommand("set camera.gain " + gainSetString);
                    label1.Text = "Gain er sat til " + gainSetString;
                }
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                if (exposureSet <= 1000)
                {
                    label1.Text = "Kan ikke gå længere ned";
                }
                else
                {
                    exposureSet -= 1000;
                    string expoSetString = exposureSet.ToString().Replace(",", ".");
                    Discovered.connector.SendCommand("set camera.exposure " + expoSetString);
                    label1.Text = "Exposure er sat til " + expoSetString;
                }
            }
            else
            {
                label1.Text = "Vælg venligst en metode";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GetItems();

            if (comboBox1.SelectedIndex == 0)
            {
                Discovered.connector.SendCommand("set focus.power " + textBox1.Text);
                label1.Text = "Focus er sat til " + textBox1.Text;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                Discovered.connector.SendCommand("set camera.gain " + textBox1.Text);
                label1.Text = "Gain er sat til " + textBox1.Text;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                Discovered.connector.SendCommand("set camera.exposure " + textBox1.Text);
                label1.Text = "Exposure er sat til " + textBox1.Text;
            }
            else
            {
                label1.Text = "Vælg venligst en metode";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetItems();

            if (comboBox1.SelectedIndex == 0)
            {
                label1.Text = "Focus er sat til " + focusSet;
            }
            else if (comboBox1.SelectedIndex == 1)
            {
                label1.Text = "Gain er sat til " + gainSet;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                label1.Text = "Exposure er sat til " + exposureSet;
            }
            else
            {
                label1.Text = "Vælg venligst en metode";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer.Stop();
            Discovered.connector.SendCommand("set liveimg.mode 0");
            Discovered.connector.SendCommand("tune.start");
            DmccResponse dmccTuneActive = Discovered.connector.SendCommand("get Tune.status");
            MessageBox.Show("Tuning in progress");
            while (dmccTuneActive.PayLoad == "ON")
            {
                dmccTuneActive = Discovered.connector.SendCommand("get Tune.status");
            }
            MessageBox.Show("Tuning Complete");
            Discovered.connector.SendCommand("set liveimg.mode 2");
            timer.Start();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    Symbologies symbologies = new Symbologies();
                    symbologies.Activate();
                    symbologies.Show();
                    break;
                default:
                    break;
            }
        }

        public Calibrate()
        {
            InitializeComponent();

            button1.Text = "Close";

            button4.Text = "Set";

            button5.Text = "Tune";

            button6.Text = "Setup";

            label1.Text = "";

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            DmccResponse dmccExpoAuto = Discovered.connector.SendCommand("get MONITOR-MODE.AUTOEXPOSURE");
            if (dmccExpoAuto.PayLoad == "ON")
                Discovered.connector.SendCommand("set MONITOR-MODE.AUTOEXPOSURE off");


            Discovered.connector.SendCommand("set liveimg.mode 2");


            comboBox1.Items.Clear();
            comboBox1.Items.Add("Set focus");
            comboBox1.Items.Add("Set gain");
            comboBox1.Items.Add("Set exposure");

            comboBox2.Items.Clear();
            comboBox2.Items.Add("Symbologies");

            timer = new System.Windows.Forms.Timer()
            {
                Interval = 500
            };

            timer.Tick += new EventHandler(TimerTick);

            timer.Start();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Stop();
            Discovered.connector.SendCommand("set liveimg.mode 0");
            Discovered.connector.SendCommand("trigger on");
            this.Close();
        }
    }
}
