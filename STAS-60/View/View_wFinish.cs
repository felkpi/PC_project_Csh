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
    public partial class View_wFinish : UserControl
    {
        Model model;
        
        public View_wFinish(Model model)
        {
            InitializeComponent();
            this.model = model;
        }
        public void buttonFinish_Click(EventHandler even)
        {
            this.buttonFinish.Click += even;
        }

        public void listView1_Click (MouseEventHandler even)
        {
            this.listView1.MouseMove += even;
        }

        public void textbox8_TextChange (EventHandler even)
        {
            this.textBox8.TextChanged += even;
        }

        public void textbox7_TextChange(EventHandler even)
        {
            this.textBox7.TextChanged+=even;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
            textBox13.Text = model.serial;
            textBox14.Text = model.type;
            textBox15.Text = model.block;
            textBox16.Text = model.separation;

            model.ChangeRTI[255] = "";
           // textBox7.Text = "";

            if (listView1.Items.Count != 0)
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].Checked == true)
                    {
                        //model.ChangeRTI[i] = listView1.Items[i].SubItems[1].Text + ", " + listView1.Items[i].SubItems[2].Text + ", " + listView1.Items[i].SubItems[3].Text + ".\n";
                        model.ChangeRTI[i]="- "+listView1.Items[i].SubItems[3].Text+"\n";
                    }
                    else
                    {
                        model.ChangeRTI[i] = "";
                    }
                    model.ChangeRTI[255] = model.ChangeRTI[255] + model.ChangeRTI[i];
                }             
            }

            if (checkBox1.Checked == true)
            {
                //model.changeMaslo = "Произведено замену рабочей жидкости: " + label16.Text + " на " + textBox8.Text + " литр";
                model.changeMaslo= label16.Text+" на \n"+textBox8.Text+" литр";
            }
            else
            {
                model.changeMaslo = "";
            }

           // textBox7.Text = "Произведенна замена РТИ : " + model.ChangeRTI[255] + ". \n" + model.changeMaslo;
        }

        private void textBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && e.KeyChar != 8 && e.KeyChar != 13 && e.KeyChar != 44)
            {
                e.Handled = true;
            }           
        }
    }
}
