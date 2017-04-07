using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.XInput;

namespace xbox_3
{
    
    public partial class Form1 : Form
    {
        Controller controller;
        private State stateOld;
        public bool connected = false;
        private double normalizedLX, normalizedLY;
        private TcpClient client = null;
        private Stream stm = null;
        

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "Controller Settings: \r\nPress X to turn on blade \r\nPress Y to turn off Blade \r\nPress A to increase speed \r\nPress B to decrease speed";
        }   
        public void Client_Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect("134.88.143.211", 8001);
            }
            catch(Exception e)
            {
                MessageBox.Show("Error.....Can't connect to server");
            }
        }
   
        public void SendPacket(string str)
        {
            string head = "&";
            string tail = "@";
            str = head + str + tail; ;
            stm = client.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] packet = asen.GetBytes(str);
            stm.Write(packet, 0, packet.Length);   //sendd to server
        }
        public void Connect() //conect controller
        {
            controller = new Controller(UserIndex.One);
            connected = controller.IsConnected;
            if (connected)
                MessageBox.Show("Controller has successfully connected. Click Ok to continue");
            if (!connected)
            {
                MessageBox.Show("ERROR: Controller did not conncect.");
                
            }
        } 
        void Joystick()
        {
            State stick = controller.GetState();
            // Joystick controlls
            double x = (stick.Gamepad.LeftThumbX);
            double y = (stick.Gamepad.LeftThumbY);

           double magnitude = Math.Sqrt(x * x + y * y);
            //determine the direction the controller is pushed
            normalizedLX =( x / magnitude)*100;
            normalizedLY = (y / magnitude)*100;
            string xval = Convert.ToString(normalizedLX);
            string yval = Convert.ToString(normalizedLY);
            MessageBox.Show(xval+ "," + yval);
        }
        void GetInput()
        {
            State stateNew = controller.GetState(); 
            //buttons controlls
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.A && stateNew.Gamepad.Buttons == GamepadButtonFlags.A)
            {
                
                //SendPacket("hello");
            }
            
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.B && stateNew.Gamepad.Buttons == GamepadButtonFlags.B)
            {
                //SendPacket("hello");
                MessageBox.Show("B pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.X && stateNew.Gamepad.Buttons == GamepadButtonFlags.X)
            {
                MessageBox.Show("X pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.Y && stateNew.Gamepad.Buttons == GamepadButtonFlags.Y)
            {
                MessageBox.Show("Y pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.Start && stateNew.Gamepad.Buttons == GamepadButtonFlags.Start)
            {
                MessageBox.Show("Start pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadDown && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadDown)
            {
                MessageBox.Show("Down pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadUp && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadUp)
            {
                MessageBox.Show("Up pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadLeft && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadLeft)
            {
                MessageBox.Show("L pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadRight && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadRight)
            {
                MessageBox.Show("R pressed");
            }
                this.stateOld = stateNew;
            
        }
        private void button1_Click(object sender, EventArgs e)  //Connect button
        {
            Connect();
            //Client_Connect();
            timer1.Enabled = true;
            timer2.Enabled = true;
        }
        private void button2_Click(object sender, EventArgs e) //blade on button
        {
            SendPacket("hello");
        }
        private void button3_Click(object sender, EventArgs e) //blade off
        {
            //SendPacket("hello");
        }
        private void button4_Click(object sender, EventArgs e) //increase speed
        {
            //SendPacket("hello");
        }
        private void button5_Click(object sender, EventArgs e)  //decrease sppeed
        {
            //SendPacket("hello");
        }
        private void timer2_Tick(object sender, EventArgs e)  //timer for joystick updating
        {
            //clock started in button1()
            Joystick();
        }

        private void button2_Click_1(object sender, EventArgs e)   //disconnect button
        {
            stm.Close();
            client.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //clock started in button1()
            GetInput();
        }   
    }
}
