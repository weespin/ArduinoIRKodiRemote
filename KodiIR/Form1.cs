using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace KodiIR
{
    public partial class Form1 : Form
    {
        private static SerialPort serial;
        private static string Port;
        private static int btnindex;
        private static bool Mapping;
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);
        private static Form1 Instance;
        private static Dictionary<Buttons,string> MappingDict = new Dictionary<Buttons, string>();
        public static bool IsLocal() => Instance.textBox1.Text == "127.0.0.1";
        
        public enum Buttons
        {
            Up,
            Down,
            Left,
            Right,
            Select,
            ContextMenu,
            Home,
            Info,
            ShowCodec,
            ShowOSD,
            ShowPlayerProcessInfo,
            Back,
            Quit
        }

        public static string[] GetBtnArr => Enum.GetNames(typeof(Buttons));
        public Form1()
        {
            InitializeComponent();
            Instance = this;
            button1_Click(null, null);
            StartSerialListener();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("config.json"))
            {
                MappingDict =
                    JsonConvert.DeserializeObject<Dictionary<Buttons, string>>(File.ReadAllText("config.json"));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var serials = SerialPort.GetPortNames();
            cmb_serial.Items.Clear();
            if (serials.Length > 0)
            {
                cmb_serial.Text = serials[0];
                Port = serials[0];
                for (var index = 0; index < serials.Length; index++)
                {
                    cmb_serial.Items.Add(serials[index]);
                }
            }
            else
            {
                MessageBox.Show("Can't find any serial ports", "Error");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MappingDict.Clear();
            label1.Visible = true;
            button5.Visible = true;
            Mapping = true;
            btnindex = 0;
            var btn = GetBtnArr[btnindex];
            Instance.label1.Text = $"Press {btn} button";
        }

        void StartSerialListener()
        {
            serial = new SerialPort(Port);

            serial.BaudRate = 115200;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.DataBits = 8;
            serial.Handshake = Handshake.None;
            serial.DataReceived += new SerialDataReceivedEventHandler(ProcessData);
            try
            {
                serial.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't open {Port}, maybe you forgot to close Arduino port monitor?");
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        private static void ProcessData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = sender as SerialPort;
            string data = sp.ReadExisting();
            Action action3 = () => Instance.label4.Text = $"Last pressed: {data}";
            Instance.label4.Invoke(action3);
            if (Mapping)
            {
                if (Enum.GetNames(typeof(Buttons)).Length - 1 >= btnindex)
                {
                    var btns = (Buttons) Enum.Parse(typeof(Buttons), GetBtnArr[btnindex]);
                    MappingDict.Add(btns, data);
                    btnindex++;
                   
                    var en = Enum.GetNames(typeof(Buttons)).Length;
                    if (en <= btnindex)
                    {
                        Instance.FinishedMapping();
                    }
                    else
                    {
                        var btn = GetBtnArr[btnindex];
                        Action action = () => Instance.label1.Text = $"Press {btn} button";
                        Instance.label1.Invoke(action);
                    }
                }
            }
            else
            {
                if (MappingDict.Count == 0)
                {
                    MessageBox.Show("Please map keys");
                    return;
                }

                var btn = MappingDict.FirstOrDefault(n => n.Value == data);
                if (!btn.Equals(null)&&btn.Value!=null)
                {
                    string k = "";
                    var btnn = btn.Key;
                    switch (btnn)
                    {
                        case Buttons.Left:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Left\"}";
                            break;
                        case Buttons.Up:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Up\"}";
                            break;
                        case Buttons.Right:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Right\"}";
                            break;
                        case Buttons.Down:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Down\"}";
                            break;
                        case Buttons.Select:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Select\"}";
                            break;
                        case Buttons.Info:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Info\"}";
                            break;
                        case Buttons.ContextMenu:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.ContextMenu\"}";
                            break;
                        case Buttons.Home:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Home\"}";
                            break;
             
                        case Buttons.Back:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.Back\"}";
                            break;
                        case Buttons.ShowOSD:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.ShowOSD\"}";
                            break;
                        case Buttons.ShowCodec:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.ShowCodec\"}";
                            break;
                        case Buttons.ShowPlayerProcessInfo:
                            k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Input.ShowPlayerProcessInfo\"}";
                            break;
                        case Buttons.Quit:
                            string appName = "Kodi";
                            IntPtr calculatorHandle = FindWindow(null, appName);
                            if (calculatorHandle == IntPtr.Zero)
                            {
                                if (IsLocal())
                                {
                                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Kodi\Kodi.exe");
                                }
                            }
                            else
                            {
                                if (IsLocal())
                                {
                                    try
                                    {


                                        System.Diagnostics.Process[] pname =
                                            System.Diagnostics.Process.GetProcessesByName("Kodi");

                                        if (pname.Length != 0)
                                        {
                                            foreach (var process in pname)
                                            {
                                                process.Kill();
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //Skipped
                                    }
                                }
                                else
                                {
                                    k = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"Application.Quit\"}";
                                }
                            }

                            break;
                       
                    }

                    if (k != "")
                    {
                        try
                        {

                            var p =
                                $"http://{Instance.textBox1.Text.Trim()}:{Instance.textBox2.Text.Trim()}/jsonrpc?request={k}";
                            new WebClient().DownloadString(
                                p);
                        }
                        catch (Exception)
                        {
                            if (IsLocal())
                            {
                                System.Diagnostics.Process[] pname =
                                    System.Diagnostics.Process.GetProcessesByName("Kodi");

                                if (pname.Length != 0)
                                {
                                    MessageBox.Show(
                                        "Can't reach server, enable remote control in kodi -> settings -> service");
                                }
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Can't reach server, enable remote control in kodi -> settings -> service");

                            }
                        }
                    }
                }



                //sendMessage(indata);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartSerialListener();
        }

        private void FinishedMapping()
        {
            Action action = () => Instance.label1.Visible = false;
            Instance.label1.Invoke(action);
            Action action2 = () => Instance.button5.Visible = false;
            Instance.button5.Invoke(action2);
            btnindex = 0;
            // SAVE
            var str = JsonConvert.SerializeObject(MappingDict);
            File.WriteAllText("config.json",str);
            Mapping = false;


        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (Mapping)
            {
                var btns = (Buttons)Enum.Parse(typeof(Buttons), GetBtnArr[btnindex]);
                MappingDict.Add(btns, "NULL");
                btnindex++;

                var en = Enum.GetNames(typeof(Buttons)).Length;
                if (en <= btnindex)
                {
                    FinishedMapping();
                }
                else
                {
                    var btn = GetBtnArr[btnindex];
                    Action action = () => Instance.label1.Text = $"Press {btn} button";
                    Instance.label1.Invoke(action);
                }
            }
           
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
