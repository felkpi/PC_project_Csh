using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace STAS_60
{
    public partial class View_Work : UserControl
    {
        Model model;
        public View_Work(Model model)
        {
            InitializeComponent();
            this.model = model;
        }

        private void View_Work_Paint(object sender, PaintEventArgs e)
        {
           // label1.Text = DateTime.Now.ToString("dd/mm/yyyy  hh:mm:ss");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
           // Invalidate();
            textBox1.Text = model.serial;
            textBox2.Text = model.type;
            textBox3.Text = model.block;
            textBox4.Text = model.separation;
            textBox5.Text = model.position;
            textBox6.Text = model.pSetup;
            textBox7.Text = model.rtm;
            textBox8.Text = model.user;

            textBox16.Text = model.FxcompressM.ToString();
            textBox9.Text = model.LcompressM.ToString();
            textBox10.Text = model.FxstretchingM.ToString();
            textBox11.Text = model.LstretchingM.ToString();
            textBox12.Text = model.FhcompressM.ToString();
            textBox13.Text = model.FhstretchingM.ToString();
            textBox14.Text = model.VcompressM.ToString();
            textBox15.Text = model.VstretchingM.ToString();           
            
            label22.Text = "= " + model.Fh.ToString();
            label23.Text = "≤ " + model.L.ToString("0.0");
            label26.Text = "≤ " + model.Fx.ToString();
            label29.Text = "от " + model.Vmin.ToString("0.0") + " до " + model.Vmax.ToString("0.0");
            label33.Text = "от " + model.Sxolmin.ToString() + " до " + model.Sxolmax.ToString();

            if (model.FxcompressM >=model.Fh)
            {
                textBox17.BackColor=Color.Red;
            }
            else
            {
                textBox17.BackColor=Color.White;
            }
        }

        // Инициализация событий по нажатию на кнопки
        public void checkBox_ManualChecked(EventHandler even)
        {
            this.checkBox_Manual.CheckedChanged += even;
        }
        public void checkBox_AutoChecked(EventHandler even)
        {
            this.checkBox_Auto.CheckedChanged += even;
        }
        public void checkBox_StartChecked(EventHandler even)
        {
            this.checkBox_Start.CheckedChanged += even;
        }

       
        public void buttonCompressingMouseDown(MouseEventHandler even)
        {
            buttonCompressing.MouseDown+=even;
        }
        public void buttonCompressingMouseUp(MouseEventHandler even)
        {
            buttonCompressing.MouseUp+=even;
        }
        public void buttonStreatchingMouseDown(MouseEventHandler even)
        {
            buttonStreatching.MouseDown+=even;
        }
        public void buttonStreatchingMouseUp(MouseEventHandler even)
        {
            buttonStreatching.MouseUp+=even;
        }
        public void buttonOK_SetupClick(EventHandler even)
        {
            buttonOK_Setup.Click+=even;
        }
        public void buttonCancel_Click (EventHandler even)
        {
            buttonCancel.Click+=even;
        }
        public void buttonCompressClick(EventHandler even)
        {
            buttonCompress.Click+=even;
        }
        public void buttonStreatchClick(EventHandler even)
        {
            buttonStreatch.Click+=even;
        }
        public void buttonCentrClick(EventHandler even)
        {
            buttonCentr.Click+=even;
        }
        public void buttonOK_AirClick(EventHandler even)
        {
            buttonOK_Air.Click+=even;
        }

        public void buttonRep_nnClick(EventHandler even)
        {
            buttonRep_nn.Click+=even;
        }
        public void buttonOK_nnClick(EventHandler even)
        {
            buttonOK_nn.Click+=even;
        }

        public void buttonOK_xxClick(EventHandler even)
        {
            buttonOK_xx.Click+=even;
        }

        public void buttonOK_SpeedClick(EventHandler even)
        {
            buttonOK_Speed.Click+=even;
        }

        public void buttonOK_SholClick(EventHandler even)
        {
            buttonOK_Shol.Click+=even;
        }

        public void buttonNextClick(EventHandler even)
        {
            this.buttonNext.Click+=even;
        }

        private void textBox25_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }
    }
}
