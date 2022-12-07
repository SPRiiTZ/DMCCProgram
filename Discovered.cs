using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using System;
using System.Net;
using System.Windows.Forms;

namespace DataManApp
{
    public partial class Discovered : Form
    {

        //  Static variables used in the methods
        public static DataManSystem connector;
        private static EthSystemDiscoverer Disc;
        private static int Index = 0;
        private static readonly string[,] connectArray = new string[10, 3]; //  Array defined with 10 spaces (Object to change)
        public static string connectInfo;

        //  On Ethernet Discovery handler method
        //  For each discovered IP Address, run the eventhandler once and increase the index variable by 1
        private static void OnEthDiscovered(EthSystemDiscoverer.SystemInfo systemInfo)
        {
            //  Sets the data from the discovered system, into a 2D array
            //  Array has defined size, from static variables
            connectArray[Index, 0] = systemInfo.IPAddress.ToString();
            connectArray[Index, 1] = systemInfo.Type;
            connectArray[Index, 2] = systemInfo.Name;

            Index++;
        }

        //  Method for clearing the IP Address Array
        //  Public for use in "Discovered" form
        public static void ArrayClear()
        {
            //  Clears the array, from IP-Addresses first, to Type, to Name, and sets them all to NULL
            for (int j = 0; j < connectArray.GetLength(1); j++)
            {
                for (int i = 0; i < Index; i++)
                {
                    connectArray[i, j] = null;
                }
            }
            Index = 0;  //  Index reset
        }
        public Discovered()
        {
            InitializeComponent();

            //  Default variables for objects on start
            button1.Text = "Search for DataMan";
            button2.Text = "Connect to selected IP";
            button3.Text = "Exit Application";
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //  Default discovery button
            //  Clears the array to NULL values, along with text properties reset
            ArrayClear();

            comboBox1.Items.Clear();
            comboBox1.Text = "";
            label2.Text = "";
            label3.Text = "";

            //  Creates a new discovery variable, and handler for new IP Addresses found
            Disc = new EthSystemDiscoverer();
            Disc.SystemDiscovered += new EthSystemDiscoverer.SystemDiscoveredHandler(OnEthDiscovered);

            //  Start discovering
            Disc.Discover();

            //  Waits for discovery to finish (Creates a delay)
            while (Disc.IsDiscoveryInProgress)
            {
            }

            label1.Text = "Discovery done!";

            //  For each IP Address found, adds them to the DropDown object with IP information
            for (int i = 0; i < connectArray.GetLength(0); i++)
            {
                //  Breaks the cycle if it finds a NULL on IP Address
                if (connectArray[i,0] == null)
                {
                    break;
                }
                else
                {
                    comboBox1.Items.Add(connectArray[i, 0]);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //  Tries to connect to the selected IP Address, defaults to catch if no index selected
            //  Sends data to variables, for use later
            try
            {
                EthSystemConnector conn = new EthSystemConnector(IPAddress.Parse(connectArray[comboBox1.SelectedIndex,0]));
                connector = new DataManSystem(conn);

                connector.Connect();
                if (connector.State.ToString() == "Connected")
                {
                    connectInfo = "Connected to: " + conn.Address;
                    Connected connected = new Connected();
                    connected.Activate();
                    connected.Show();
                }
            }
            catch (Exception)
            {
                label1.Text = "Please select an IP address";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //  Exits the application
            Application.Exit();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //  Default labels for defining which DataMan you are selecting
            label2.Text = "Type: " + connectArray[comboBox1.SelectedIndex,1];
            label3.Text = "Name: " + connectArray[comboBox1.SelectedIndex,2];
        }
    }
}
