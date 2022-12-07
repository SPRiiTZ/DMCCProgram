using Cognex.DataMan.SDK;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataManApp
{
    public partial class Connected : Form
    {
        //  Static variables used in the methods
        private static string path;
        private static Timer timer;
        private static bool saveStarted = false;

        // Timer Method
        // Used for checking if a new picture has arrived in the buffer
        // Saves the picture as well, in a user defined folder
        private void timerTick(object sender, EventArgs eventArgs)
        {
            //  Stops timer temporarily
            timer.Stop();
            if (Discovered.connector.State.ToString() == "Connected")
            {
                //  Information from last good read, formatted
                pictureBox1.Image = Discovered.connector.GetLastReadImage();
                DmccResponse dmccPictureInfo = Discovered.connector.SendCommand("get result 0");
                label1.Text = "Last good read details\n\n" + dmccPictureInfo.PayLoad.Replace("|", "\n").TrimEnd();

                //  Checks how many pictures are located in the DataMan buffer
                DmccResponse dmcc = Discovered.connector.SendCommand("get imagebuffer.num");
                int n = Int32.Parse(dmcc.PayLoad);

                //  If one or more pictures are buffered, saves all the pictures
                if (n > 0 && path != null)
                {
                    while (n > 0)
                    {
                        try     //  Tries to get all pictures, added for timing correction issues
                        {
                            Discovered.connector.GetBufferedImage(n - 1, ImageFormat.bitmap, ImageSize.Full, ImageQuality.High).Save(@path + @"\Picture_" + DateTime.Now.ToString("yyyy_MM_dd.HH_mm_ss_ff") + ".bmp");
                            n--;
                        }
                        catch (Exception)
                        {
                            break;  //  Breaks if timing is corrupted for some reason
                        }
                    }
                }

                //  Starts timer again
                timer.Start();
            }
        }

        public Connected()
        {
            InitializeComponent();

            //  On connection, checks the current state of the DataMan (Master / Slave / Off)
            DmccResponse dmcc = Discovered.connector.SendCommand("GET MASTER-SLAVE.MODE");
            Discovered.connector.SendCommand("trigger on");     //  Triggers once to get a picture for start picture

            string msMode = "";

            //  Correctly labes the Master/Slave mode
            switch (dmcc.PayLoad)
            {
                case "0":
                    msMode = "Not Configured";
                    break;
                case "1":
                    msMode = "Slave Mode";
                    break;
                case "2":
                    msMode = "Master Mode";
                    break;
                default:
                    break;
            }

            //  Default values for objects
            toolStripStatusLabel1.Text = Discovered.connectInfo + " | Master Slave Mode: " + msMode;

            button1.Text = "Get last picture";

            button2.BackColor = Color.LightBlue;
            button2.Text = "Select Save Path\nAnd Start Application";

            button3.Text = "Exit";

            button4.Text = "Send Command";

            button5.Text = "Calibrate";

            //  Timer creation, and interval settings
            timer = new Timer
            {
                Interval = 100
            };

            //  Eventhandler for the timer method
            timer.Tick += new EventHandler(timerTick);

            comboBox1.Items.Clear();
            comboBox1.Items.Add("No images");
            comboBox1.Items.Add("All images");
            comboBox1.Items.Add("Only reads");
            comboBox1.Items.Add("Only noReads");
            comboBox1.Items.Add("Validation Failure");
            comboBox1.Items.Add("Validation Failure + noReads");
            comboBox1.SelectedIndex = 0;

            DmccResponse dmccTune = Discovered.connector.SendCommand("get tune.train-code");
            DmccResponse dmccBuffSet = Discovered.connector.SendCommand("get imagebuffer.record-type");

            if (dmccTune.PayLoad == "ON")
            {
                Discovered.connector.SendCommand("set tune.train-code off");
            }
            if (dmccBuffSet.PayLoad != "0")
            {
                Discovered.connector.SendCommand("set imagebuffer.record-type 0");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //  On button press, check if there is a good image on the reader

            if (Discovered.connector.GetLastReadImage() == null)
            {
                Discovered.connector.SendCommand("trigger on");     // If for some reason there isn't a piccture, trigger once
            }

            pictureBox1.Image = Discovered.connector.GetLastReadImage();
            DmccResponse dmcc = Discovered.connector.SendCommand("get result 0");
            label1.Text = dmcc.PayLoad.Replace("|", "\n").TrimEnd();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //  On button press, check if there is a path selected, for saving pictures
            //  If no path (or shift hold when pressed), select a new path
            if (path == null || Control.ModifierKeys == Keys.Shift)
            {
                FolderBrowserDialog folder = new FolderBrowserDialog();

                if (folder.ShowDialog() == DialogResult.OK)
                {
                    path = folder.SelectedPath;

                }
            }

            //  Toggles the saveStarted variable, to start or stop the timer, and change button values
            if (!saveStarted)
            {
                saveStarted = !saveStarted;
                button2.BackColor = Color.Green;
                button2.Text = "Saving pictures";
                timer.Start();
            }
            else if (saveStarted)
            {
                saveStarted = !saveStarted;
                button2.BackColor = Color.LightBlue;
                button2.Text = "Select Save Path\nAnd Start Application";
                path = null;
                timer.Stop();
            }

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    Discovered.connector.SendCommand("set imagebuffer.record-type 0");
                    break;
                case 1:
                    Discovered.connector.SendCommand("set imagebuffer.record-type 3");
                    break;
                case 2:
                    Discovered.connector.SendCommand("set imagebuffer.record-type 2");
                    break;
                case 3:
                    Discovered.connector.SendCommand("set imagebuffer.record-type 1");
                    break;
                case 4:
                    Discovered.connector.SendCommand("set imagebuffer.record-type 4");
                    break;
                case 5:
                    Discovered.connector.SendCommand("set imagebuffer.record-type 5");
                    break;
                default:
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //  Disconnects the current session, and closes the current form
            Discovered.connector.Disconnect();
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //  Tries to send DMCC commands to the DataMan, with error catching
            try
            {
                DmccResponse dmcc = Discovered.connector.SendCommand(textBox1.Text);
                label1.Text = dmcc.PayLoad.ToString();
            }
            catch (Exception a)
            {
                MessageBox.Show(a.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (saveStarted)
            {
                saveStarted = !saveStarted;
                button2.BackColor = Color.LightBlue;
                button2.Text = "Select Save Path\nAnd Start Application";
                path = null;
                timer.Stop();
            }
            Calibrate calibrate = new Calibrate();
            calibrate.Activate();
            calibrate.Show();
        }
    }
}
