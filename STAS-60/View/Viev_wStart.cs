using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace STAS_60
{
    public partial class Viev_wStart : UserControl
    {
        Model model;
        public Viev_wStart(Model model)
        {
            InitializeComponent();
            this.model = model;
  
        }
        public void button_Next(EventHandler even) // нажатие кнопки "Далее"
        {
            this.buttonNext.Click += even;
        }
        public void comboBox1_SelectedIndexChanged(EventHandler even)
        {
            this.comboBox1.SelectedIndexChanged += even;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Invalidate();
        }
        public void show ()
        {
            textBox1.Text=model.type;
            textBox2.Text=model.block;
            textBox3.Text=model.separation;
            textBox4.Text=model.position;
            textBox5.Text=model.pSetup;
            textBox6.Text=model.rtm;
            textBox7.Text=Convert.ToString(model.So);
            textBox8.Text=Convert.ToString(model.Xo);
            textBox9.Text=Convert.ToString(model.Fx);
            textBox10.Text=Convert.ToString(model.L);
            textBox11.Text=Convert.ToString(model.Fh);
            textBox12.Text=Convert.ToString(model.Vmin);
            textBox13.Text=Convert.ToString(model.Vmax);
            textBox14.Text=Convert.ToString(model.Sxol);
            textBox15.Text=Convert.ToString(model.Sxolmin);
            textBox16.Text=Convert.ToString(model.Sxolmax);

            model.user=comboBox2.Text;
        }
        private void Viev_wStart_Paint(object sender, PaintEventArgs e)
        {
            /*if (comboBox2.Text != "" && comboBox1.Text != "" && textBox1.Text != "" && textBox2.Text != "" &&
                textBox3.Text != "" && textBox4.Text != "" && textBox5.Text != "" && textBox6.Text != "")
            {
                buttonNext.Enabled = true;
            }
            else
            {
                buttonNext.Enabled = false;
            }*/
        }
    }
}
