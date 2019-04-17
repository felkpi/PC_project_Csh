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
    public partial class View_Setings : UserControl
    {
        Model model;
        public View_Setings(Model model)
        {
            InitializeComponent();
            this.model = model;
        }

        public void button_Add (EventHandler even)
        {
            this.buttonAdd.Click += even;
        }
        public void button_Change_Absorber (EventHandler even)
        {
            this.buttonChange.Click += even;
        }
        public void button_Delete_Absorber (EventHandler even)
        {
            this.buttonDelete.Click += even;
        }

        public void button_AddUser (EventHandler even)
        {
            this.buttonAddUser.Click += even;
        }
        public void button_Delete_User (EventHandler even)
        {
            this.buttonDeleteUser.Click += even;
        }

        public void comboBoxSSAbsorbers_SelectedIndexChanged (EventHandler even)
        {
            comboBoxSSAbsorbers.SelectedIndexChanged += even;
        }
        public void comboBoxSSAbsorbers_Click (EventHandler even)
        {
            this.comboBoxSSAbsorbers.Click += even;
        }

        public void comboBoxRTMAbsorbers_Click (EventHandler even)
        {
            comboBoxRTMAbsorbers.Click+=even;
        }

        public void comboBoxRTMAbsorbers_SelectedIndexChange(EventHandler even)
        {
            comboBoxRTMAbsorbers.SelectedIndexChanged+=even;
        }
        public void showTextBox ()
        {
            textBox1.Text = model.serial;
            textBox2.Text = model.type;
            textBox3.Text = model.block;
            textBox4.Text = model.separation;
            textBox5.Text = model.position;
            textBox6.Text = model.pSetup;
            textBox7.Text = model.rtm;
            textBox8.Text = model.So.ToString();
            textBox9.Text = model.Xo.ToString();
            textBox10.Text = model.Fx.ToString();
            textBox11.Text = model.L.ToString();
            textBox12.Text = model.Fh.ToString();
            textBox13.Text = model.Vmin.ToString();
            textBox14.Text = model.Vmax.ToString();
            textBox15.Text = model.Sxol.ToString();
            textBox16.Text = model.Sxolmin.ToString();
            textBox17.Text = model.Sxolmax.ToString();
            textBox18.Text = model.Vgidcost.ToString();
            textBox19.Text = model.Typegidcost.ToString();
            textBox20.Text = model.preasure.ToString();
            textBox21.Text = model.rate.ToString();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          //  Invalidate();            
        }

        private void textBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox9_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox10_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox11_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox12_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox13_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox14_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox15_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox16_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox17_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox18_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }

        private void textBox19_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }
        }
    }
}
