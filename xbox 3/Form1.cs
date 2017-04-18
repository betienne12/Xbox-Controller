﻿using System;
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
       // private UdpClient client = null;
       // private Stream stm = null;
        private string temp = "000";
        private string str = "87F7F"; //inital packet 7F
        
        
        

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "Controller Settings: \r\nPress Start Button to start the lawnmower \r\nPress X to turn on blade \r\nPress Y to turn off Blade \r\nPress A to increase speed \r\nPress B to decrease speed";
        }   
    
      
        /*public void Client_Connect()
        {
            try
            {
                client = new UdpClient();
                //string p = "hello";
                //client.Send(p, p.Length, "192.168.4.1", 4210);
                //IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.4.1"), 4210);
                //client.Connect(ep);
                client.Connect("192.168.4.1", 4210);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }*/
   
        public void SendPacket(string x)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
            IPAddress serverAddr = IPAddress.Parse("192.168.4.1");
            IPEndPoint endPoint = new IPEndPoint(serverAddr, 4210);
            string head = "&";
            string tail = "@";
            x = head + x + tail; ;
            byte[] packet = Encoding.ASCII.GetBytes(x);
            sock.SendTo(packet, endPoint);
            
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
                string bit_1 = (arr[1]).ToString();
                string bit_2 = (arr[2]).ToString();
                string val = bit_1 + bit_2;    //2 bit hex value to be added
                                               // MessageBox.Show(val);
                if (val == "F0")   //F0 = 240 last value without going over 255
                {
                    speed = "FF";
                    MessageBox.Show("Max speed has been reached");
                }
                else
                {
                    int intFromHex = int.Parse(val, System.Globalization.NumberStyles.HexNumber) + 20;  //increment hex value by 20
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
                string bit_1 = (arr[1]).ToString();
                string bit_2 = (arr[2]).ToString();
                string val = bit_1 + bit_2;    //2 bit hex value to be added
                if (val == "1B")
                {
                    speed = "00";
                    MessageBox.Show("Lowest speed has been reached");
                }
                if (val == "14")
                {
                    speed = "00";
                    MessageBox.Show("Lowest speed has been reached");
                }
                else if (val == "00")
                {
                    speed = "00";
                    MessageBox.Show("Lowest speed has been reached");
                }
                else
                {
                    int intFromHex = int.Parse(val, System.Globalization.NumberStyles.HexNumber) - 20;  //decrement hex value by 20
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
                    String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                    MessageBox.Show(binary);
                    str = str.Remove(0, 1).Insert(0, binary);
                    SendPacket(str);
                    MessageBox.Show(str);
                }

            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.Y && stateNew.Gamepad.Buttons == GamepadButtonFlags.Y)  //turn off blade
            {
                if (temp[0] == '0')
                {
                    MessageBox.Show("The Blade is already off");
                }

                if (temp[0] != '0')
                {
                    temp = temp.Remove(0, 1).Insert(0, "0");  //000
                    String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                    str = str.Remove(0, 1).Insert(0, binary);
                    SendPacket(str);
                    MessageBox.Show(str);
                }

            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.Start && stateNew.Gamepad.Buttons == GamepadButtonFlags.Start)
            {
                SendPacket(str);
                MessageBox.Show(str);
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadDown && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadDown)
            {

                if ((temp[1] == '0') && (temp[2] == '0'))
                {
                    MessageBox.Show("LawnMower is already going backwards");
                }
                else
                {
                    char[] arr = temp.ToCharArray();
                    arr[1] = '0';   //assuming 0 is backwards
                    arr[2] = '0';
                    temp = new string(arr);
                    String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);  
                    str = str.Remove(0, 1).Insert(0, binary);
                    SendPacket(str);
                    MessageBox.Show(str);

                }
            }
                if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadUp && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadUp)
                {

                    if ((temp[1] == '1') && (temp[2] == '1'))
                    {
                        MessageBox.Show("LawnMower is already going straight");
                    }
                    else
                    {
                        char[] arr = temp.ToCharArray();
                        arr[1] = '1';   //assuming one is forward
                        arr[2] = '1';
                        temp = new string(arr);
                        String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                        str = str.Remove(0, 1).Insert(0, binary);
                        SendPacket(str);
                        MessageBox.Show(str);

                    }
                }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadLeft && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadLeft)
            {
                if ((temp[1] == '0') && (temp[2] == '1'))   //assuming dir1 is left
                {
                    MessageBox.Show("LawnMower is turning left");
                }
                else
                {
                    char[] arr = temp.ToCharArray();
                    arr[1] = '0';   //assuming one is forward
                    arr[2] = '1';
                    temp = new string(arr);
                    String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                    str = str.Remove(0, 1).Insert(0, binary);
                    SendPacket(str);
                    MessageBox.Show(str);
                }
            }
            if (this.stateOld.Gamepad.Buttons == GamepadButtonFlags.DPadRight && stateNew.Gamepad.Buttons == GamepadButtonFlags.DPadRight)
            {
                if ((temp[1] == '1') && (temp[2] == '0'))   //assuming dir2 is right
                {
                    MessageBox.Show("LawnMower is turning right");
                }
                else
                {
                    char[] arr = temp.ToCharArray();
                    arr[1] = '1';   //assuming one is forward
                    arr[2] = '0';  //assuming is backward
                    temp = new string(arr);
                    String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                    str = str.Remove(0, 1).Insert(0, binary);
                    SendPacket(str);
                    MessageBox.Show(str);
                }
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
            if (temp[0] == '1')
            {
                MessageBox.Show("The Blade is already on");
            }
            if (temp[0] != '1')
            {
                temp = temp.Remove(0, 1).Insert(0, "1");  //100
                String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                str = str.Remove(0, 1).Insert(0, binary);
                SendPacket(str);
                MessageBox.Show(str);
            }
        }
        private void button3_Click(object sender, EventArgs e) //blade off
        {
            //MessageBox.Show(temp);
            if (temp[0] == '0')
            {
                MessageBox.Show("The Blade is already off");
            }

            if (temp[0] != '0')
            {
                temp = temp.Remove(0, 1).Insert(0, "0");  //000
                String binary = Convert.ToString(Convert.ToInt32(temp, 2), 10);
                str = str.Remove(0, 1).Insert(0, binary);
                SendPacket(str);
                MessageBox.Show(str);
            }
        }
        private void button4_Click(object sender, EventArgs e) //increase speed
        {
            char[] arr = str.ToCharArray();
            string speed;
            string bit_1 = (arr[1]).ToString();
            string bit_2 = (arr[2]).ToString();
            string val = bit_1 + bit_2;    //2 bit hex value to be added
                                           // MessageBox.Show(val);
            if (val == "F0")   //F0 = 240 last value without going over 255
            {
                speed = "FF";
                MessageBox.Show("Max speed has been reached");
            }
            else
            {
                int intFromHex = int.Parse(val, System.Globalization.NumberStyles.HexNumber) + 20;  //increment hex value by 20
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
        private void button5_Click(object sender, EventArgs e)  //decrease sppeed
        {
            char[] arr = str.ToCharArray();
            string speed;
            string bit_1 = (arr[1]).ToString();
            string bit_2 = (arr[2]).ToString();
            string val = bit_1 + bit_2;    //2 bit hex value to be added
            if (val == "1B")
            {
                speed = "00";
                MessageBox.Show("Lowest speed has been reached");
            }
            if (val == "14")
            {
                speed = "00";
                MessageBox.Show("Lowest speed has been reached");
            }
            else if (val == "00")
            {
                speed = "00";
                MessageBox.Show("Lowest speed has been reached");
            }
            else
            {
                int intFromHex = int.Parse(val, System.Globalization.NumberStyles.HexNumber) - 20;  //decrement hex value by 20
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
        private void timer2_Tick(object sender, EventArgs e)  //timer for joystick updating
        {
            //clock started in button1()
           // Joystick();
        }

        private void button2_Click_1(object sender, EventArgs e)  //stop button
        {
            char[] arr = str.ToCharArray();
            arr[0] = '0';
            arr[1] = '0';
            arr[2] = '0';
            arr[3] = '0';
            arr[4] = '0';
            str = new string(arr);
            SendPacket(str);
            MessageBox.Show(str);
        }

        private void button6_Click(object sender, EventArgs e)  //start button
        {
            SendPacket(str);
            MessageBox.Show(str);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //clock started in button1()
            GetInput();
        }   
    }
}
