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
        private UdpClient client = null;
        private Stream stm = null;
        private string temp = "000";
        private string str = "07F7F"; //inital packet
        
        
        

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "Controller Settings: \r\nPress X to turn on blade \r\nPress Y to turn off Blade \r\nPress A to increase speed \r\nPress B to decrease speed";
        }   
      
        public void Client_Connect()
        {
            try
            {
                client = new UdpClient();
                client.Connect("192.168.4.1", 80);
            }
            catch(Exception e)
            {
                MessageBox.Show("Error.....Can't connect to server");
            }
        }
   
        public void SendPacket(string x)
        {
            string head = "&";
            string tail = "@";
            x = head + x + tail; ;
            stm = client.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] packet = asen.GetBytes(x);
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
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.A && stateNew.Gamepad.Buttons == GamepadButtonFlags.A)  //increase speed
            {
               // MessageBox.Show("a pressed");
                char[] arr = str.ToCharArray();
                string speed;
                string bit_1 = (arr[4]).ToString();
                string bit_2= (arr[5]).ToString();
                string val = bit_1 + bit_2;    //2 bit hex value to be added
               // MessageBox.Show(val);
                if (val == "F0")   //F0 = 240 last value without going over 255
                {
                    speed = "FF";
                    MessageBox.Show("Max speed has been reached");
                }
                else
                {
                    int intFromHex = int.Parse(val, System.Globalization.NumberStyles.HexNumber) + 20;  //increment hex value by 5
                    speed = intFromHex.ToString("X");
                }
                arr[1] = speed[0];
                arr[2] = speed[1];
                arr[3] = speed[0];
                arr[4] = speed[1];
                str = new string(arr);
                SendPacket(str);
                MessageBox.Show(str);
               

            }
             
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.B && stateNew.Gamepad.Buttons == GamepadButtonFlags.B)  //decrease speed
            {
                char[] arr = str.ToCharArray();
                string speed;
                string bit_1 = (arr[4]).ToString();
                string bit_2 = (arr[5]).ToString();
                string val = bit_1 + bit_2;    //2 bit hex value to be added
                if (val == "14")
                 {
                     speed = "00";
                    MessageBox.Show("Lowest speed has been reached");
                }
                if(val == "00")
                {
                    speed = "00";
                    MessageBox.Show("Lowest speed has been reached");
                }
                else
                {
                    int intFromHex = int.Parse(val, System.Globalization.NumberStyles.HexNumber)-20;  //decrement hex value by 20
                    speed = intFromHex.ToString("X");
                }
                arr[1] = speed[0];
                arr[2] = speed[1];
                arr[3] = speed[0];
                arr[4] = speed[1];
                str = new string(arr);
                SendPacket(str);
                MessageBox.Show(str);

            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.X && stateNew.Gamepad.Buttons == GamepadButtonFlags.X)  //turn on blade
            {
                if (temp[0] == '1')
                {
                    MessageBox.Show("The Blade is already on");
                }
                if (temp[0] != '1')
                {
                    temp = temp.Remove(0, 1).Insert(0, "1");  //100
                    //if (Convert.ToInt32(temp) >= 100)
                    if ((str.Length != 5) && (Convert.ToInt32(temp) >= 100))
                    {
                        MessageBox.Show(temp);
                        string hexValue = Convert.ToInt32(temp).ToString("X");
                        str = str.Remove(0, 2).Insert(0, hexValue);
                        MessageBox.Show(str);
                        //SendPacket(str);
                    }
                    if ((str.Length==5)&&(Convert.ToInt32(temp)>=100))
                    {
                        MessageBox.Show(temp);
                        string hexValue = Convert.ToInt32(temp).ToString("X");
                        str = str.Remove(0, 1).Insert(0, hexValue);
                        MessageBox.Show(str);
                        //SendPacket(str);

                    }


                }
               
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.Y && stateNew.Gamepad.Buttons == GamepadButtonFlags.Y)  //turn off blade
            {
                //MessageBox.Show(temp);
                if (temp[0] == '0')    
                {
                    MessageBox.Show("The Blade is already off");
                }
                
                if (temp[0] != '0')
                {
                    temp = temp.Remove(0, 1).Insert(0, "0");  //000
                    if (Convert.ToInt32(temp) <= 011)
                    {
                        string hexValue = Convert.ToInt32(temp).ToString("X");    //convert t
                        str = str.Remove(0, 2).Insert(0, "0"+hexValue);    //update packet with hex value 
                        MessageBox.Show(str);
                        //SendPacket(str);
                    }
                    if (Convert.ToInt32(temp) > 011)
                    {
                        string hexValue = Convert.ToInt32(temp).ToString("X");    //convert to hexvalue
                        MessageBox.Show(hexValue);
                        str = str.Remove(0, 2).Insert(0, hexValue);    //update packet with hex value 
                        MessageBox.Show(str);
                        //SendPacket(str);
                    }
                }
            
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.Start && stateNew.Gamepad.Buttons == GamepadButtonFlags.Start)
            {
                MessageBox.Show("Start pressed");
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadDown && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadDown)
            {
                
                MessageBox.Show("Down pressed");
                MessageBox.Show(temp);
                char[] arr = temp.ToCharArray();
                arr[1] = '0';   //
                arr[2] = '0';
                temp = new string(arr);
                MessageBox.Show(temp);

            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadUp && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadUp)
            {
                MessageBox.Show("Up pressed");
                
                if ((temp[1] == '1') && (temp[2] == '1'))
                {
                    MessageBox.Show("LawnMower is already going straight");
                }
                else
                {
                    char[] arr = temp.ToCharArray();
                    arr[1] = '1';   //
                    arr[2] = '1';
                    temp = new string(arr);
                    MessageBox.Show(temp);
                    if (Convert.ToInt32(temp) == 111)
                    {
                        
                        string hexValue = Convert.ToInt32(temp).ToString("X");
                        str = str.Remove(0, 2).Insert(0, hexValue);
                        MessageBox.Show(str);
                        //SendPacket(str);
                    }
                    if (Convert.ToInt32(temp) == 011)
                    {
                        string hexValue = Convert.ToInt32(temp).ToString("X");
                        str = str.Remove(0, 1).Insert(0, "0"+hexValue);
                        MessageBox.Show(str);
                        //SendPacket(str);
                    }
                }
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
            //
            
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
            SendPacket("hello");
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
           // Joystick();
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
