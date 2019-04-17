using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STAS_60
{
    public partial class View_Archiv : UserControl
    {
        Model model;

        int counr;
        public View_Archiv(Model model)
        {
            InitializeComponent();
            this.model = model;

            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;
        }

        public void buttonArchivEnter_click (EventHandler even)
        {
            buttonArchivEnter.Click += even;
        }

        public void dateTimePicker1_ValueChanged(EventHandler even)
        {
            dateTimePicker1.ValueChanged += even;
        }
        public void dateTimePicker2_ValueChanged (EventHandler even)
        {
            dateTimePicker2.ValueChanged += even;
        }

        public void comboBoxSelectCerial_IndexChange (EventHandler even)
        {
            comboBoxSelectCerial.SelectedIndexChanged += even;
        }

        public void comboBoxSelectProtocol_IndexChange (EventHandler even)
        {
            comboBoxSelectProtocol.SelectedIndexChanged += even;
        }

        public void comboBoxBlock_IndexChange(EventHandler even)
        {
            comboBoxBlock.SelectedIndexChanged+=even;
        }

        public void comboBoxSeparation_IndexChange(EventHandler even)
        {
            comboBoxSeparation.SelectedIndexChanged+=even;
        }

        public void comboBoxRTM_IndexChange(EventHandler even)
        {
            comboBoxRTM.SelectedIndexChanged+=even;
        }

        public void buttonSave_Click (EventHandler even)
        {
            buttonSave.Click+=even;
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            counr=dataGridView1.CurrentRow.Index;
        }
    }
}
