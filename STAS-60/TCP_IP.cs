using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace STAS_60
{
    public partial class TCP_IP : Form
    {
        Model model;
        public TCP_IP()
        {
            InitializeComponent();
            textBox1.Text = Properties.Settings.Default.IPAdress.ToString();
            textBox2.Text = Properties.Settings.Default.Port.ToString();
            textBox3.Text = Properties.Settings.Default.ModbusAdress.ToString();
            model = new Model();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IPAdress = textBox1.Text;
            Properties.Settings.Default.Port = Convert.ToUInt16(textBox2.Text);
            Properties.Settings.Default.ModbusAdress = Convert.ToUInt16(textBox3.Text);
            Properties.Settings.Default.Save();

            model.conntectToPLC();
        }
    }
}
