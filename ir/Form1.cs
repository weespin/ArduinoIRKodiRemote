using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Ports;

namespace ir
{
    public partial class Form1 : Form
    {
        const int SENDKEY = 1;
        const int READKEY = 2;
        private static string keystrokeToSave;
        private static SerialPort serial;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SerialReading(SENDKEY);
        }


        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void sendMessage(string message)
        {
            string appName = "Kodi";
            IntPtr calculatorHandle = FindWindow(null, appName);
            if (calculatorHandle == IntPtr.Zero)
            {
                if (message == "FFFFEA15\r\n")
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Kodi\Kodi.exe");
                }
            }

            SetForegroundWindow(calculatorHandle);

            translateMessage(message);
        }

        private static void translateMessage(string message)
        {
            string key = "";
            switch (message)
            {
                case "6897\r\n":
                    key = "{LEFT}";
                    break;
                case "708F\r\n":
                    key = "{UP}";
                    break;
                case "7887\r\n":
                    key = "{RIGHT}";
                    break;
                case "28D7\r\n":
                    key = "{DOWN}";
                    break;
                case "48B7\r\n":
                    key = "{ENTER}";
                    break;
                case "58A7\r\n":
                    key = "{BACKSPACE}";
                    break;
                case "FFFF827D\r\n":
                    key = "{F10}";
                    break;
                case "42BD\r\n":
                    key = "{F9}";
                    break;
                case "8F7\r\n":
                    key = " ";
                    break;
                case "38C7\r\n":
                    key = "x";
                    break;
                case "FFFFFA05\r\n":
                    key = "{F8}";
                    break;
                case "3AC5\r\n":
                    key = "ű";
                    break;       
                case "18E7\r\n":
                    key = "r";
                    break;
                case "FFFF8877\r\n":
                    key = "f";
                    break;
                case "1AE5\r\n":
                    key = "c";
                    break;
                case "AF5\r\n":
                    key = "i";
                    break;
                case "2AD5\r\n":
                    key = "m";
                    break;
                case "FFFF9867\r\n":
                    key = "{TAB}";
                    break;
                case "FFFF847B\r\n":
                    key = "{PGUP}";
                    break;
                case "4FB\r\n":
                    key = "{PGDN}";
                    break;

                case "FFFFEA15\r\n":
                    key = "s";
                    break;
                case "54AB\r\n":
                    key = "e";
                    break;


                case "FFFFDA25\r\n":
                    key = "1";
                    break;
                case "FFFFF20D\r\n":
                    key = "2";
                    break;
                case "FFFFCA35\r\n":
                    key = "3";
                    break;
                case "5AA5\r\n":
                    key = "4";
                    break;
                case "FFFFF00F\r\n":
                    key = "5";
                    break;
                case "7A85\r\n":
                    key = "6";
                    break;
                case "6A95\r\n":
                    key = "7";
                    break;
                case "728D\r\n":
                    key = "8";
                    break;
                case "4AB5\r\n":
                    key = "9";
                    break;
                case "FFFFAA55\r\n":
                    key = "0";
                    break;

            }

            SendKeys.SendWait(key);
        }

        public static void SerialReading(int mode)
        {
            string comPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\com.conf";
            string port = "";
            try
            {
                port = System.IO.File.ReadAllText(comPath);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(comPath, "COM3");

                MessageBox.Show("And now this app will be exit. Please configure COM port settings in: " + comPath);
                Application.Exit();

                return;
            }
            
            serial = new SerialPort(port);

            serial.BaudRate = 115200;
            serial.Parity = Parity.None;
            serial.StopBits = StopBits.One;
            serial.DataBits = 8;
            serial.Handshake = Handshake.None;

            if (mode == SENDKEY)
            {
                serial.DataReceived += new SerialDataReceivedEventHandler(sendKeyStroke);
            }
            else if (mode == READKEY)
            {
                serial.DataReceived += new SerialDataReceivedEventHandler(saveKeyStroke);
            }

            try
            {
                serial.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't open the port: " + port + "\n\n" + ex.ToString());
                Application.Exit();
            }
            
        }

        private static void sendKeyStroke(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            sendMessage(indata);
        }

        private static void saveKeyStroke(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            keystrokeToSave = sp.ReadExisting();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                {
                    serial.Open();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
