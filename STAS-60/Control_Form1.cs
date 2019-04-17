using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;
using System.Threading;


namespace STAS_60
{
    public partial class Control_Form1 : Form
    {
        Viev_wStart view_wStart; // окно вноса реквизитов испытания
        View_Alarm view_alarm; // окно диагностики, предуприждений
        View_Archiv view_archiv; // окно работы с архивом
        View_Setings view_seting; // окно настроек
        View_wFinish view_wFinish; // окно окночания проверги ГА
        View_Work view_work; // окно основной проверки
        View_Graphik view_graphik; // графики


        /// <summary> Экземпляр класса model в котором описываются все методы которые используются в программе (шаблон MVC)</summary>
        Model model;
        /// <summary> Экземпляр класса tcp_ip для работы с ПЛК по протоколу ModBusTCP</summary>
        TCP_IP tcp_ip;
        /// <summary> Экземпляр класса для считывания регистров в паралельном потоке</summary>
        Thread readReg;

        Bitmap bm;
        MemoryStream ms;

        public Control_Form1()
        {
            InitializeComponent();

            tcp_ip = new TCP_IP();

            model = new Model();
            model.createFolder();
            model.createDBAction();
            model.createTableAction();
            model.createMeasureDB();

            view_wStart = new Viev_wStart(model);
            panel1.Controls.Add(view_wStart);
            view_wStart.Hide();

            view_alarm = new View_Alarm(model);
            panel1.Controls.Add(view_alarm);
            view_alarm.Hide();

            view_archiv = new View_Archiv(model);
            panel1.Controls.Add(view_archiv);
            view_archiv.Hide();

            view_seting = new View_Setings(model);
            panel1.Controls.Add(view_seting);
            view_seting.Hide();

            view_wFinish = new View_wFinish(model);
            panel1.Controls.Add(view_wFinish);
            view_wFinish.Hide();

            view_work = new View_Work(model);
            panel1.Controls.Add(view_work);
            view_work.Hide();

            view_graphik = new View_Graphik(model);
            panel1.Controls.Add(view_graphik);
            view_graphik.Hide();

            // Инициализация событий
            //-----------------------------------------------------------------------------------------
            view_work.checkBox_ManualChecked(new EventHandler(checkBox_ManualChecked));
            view_work.checkBox_AutoChecked(new EventHandler(checkBox_AutoChecked));
            view_work.buttonNextClick(new EventHandler(button_View_Work_Next));
            view_work.buttonCompressingMouseDown(new MouseEventHandler (buttonViewWorkCompressingMouseDown));
            view_work.buttonCompressingMouseUp(new MouseEventHandler(buttonViewWorkCompressingMouseUp));
            view_work.buttonStreatchingMouseDown(new MouseEventHandler(buttonViewWorkStreatchingMouseDown));
            view_work.buttonStreatchingMouseUp(new MouseEventHandler(buttonViewWorkStreatchingMouseUp));
            view_work.buttonOK_SetupClick(new EventHandler(buttonViewWorkOK_Setup));
            view_work.buttonCancel_Click(new EventHandler (buttonCancelClick));
            //------------------------------------------------------------------------------------------
            view_wStart.button_Next(new EventHandler(button_Next));
            view_wStart.comboBox1_SelectedIndexChanged(new EventHandler(comboBox1SelectedIndexChanged));
            //------------------------------------------------------------------------------------------
            view_wFinish.buttonFinish_Click(new EventHandler(buttonFinish));
            //------------------------------------------------------------------------------------------
            view_seting.button_Add(new EventHandler(buttonAdd));
            view_seting.button_AddUser(new EventHandler(buttonAddUser));
            view_seting.comboBoxSSAbsorbers_SelectedIndexChanged(new EventHandler(comboBoxSSAbsorbersIndexChanged));
            view_seting.comboBoxSSAbsorbers_Click(new EventHandler(comboBoxSSAbsorbersClick));
            view_seting.button_Delete_Absorber(new EventHandler(buttonDeleteAbsorber));
            view_seting.button_Delete_User(new EventHandler(buttonDeleteUsers));
            view_seting.button_Change_Absorber(new EventHandler(buttonUpdateAbsorber));
            view_seting.comboBoxRTMAbsorbers_SelectedIndexChange(new EventHandler (comboBoxRTMAbsorbersSelectedIndexChange));
            view_seting.comboBoxRTMAbsorbers_Click(new EventHandler(comboBoxRTMAbsorbersClick));
            //------------------------------------------------------------------------------------------

            view_archiv.buttonArchivEnter_click(new EventHandler(buttonArchivEnter));
            view_archiv.dateTimePicker1_ValueChanged(new EventHandler(dateTimePicker1ValueChanged));
            view_archiv.dateTimePicker2_ValueChanged(new EventHandler(dateTimePicker2ValueChanged));
            view_archiv.comboBoxSelectCerial_IndexChange(new EventHandler(comboBoxSelectCerialIndexChange));
            view_archiv.comboBoxSelectProtocol_IndexChange(new EventHandler(comboBoxSelectProtocolIndexChange));
            view_archiv.buttonSave_Click(new EventHandler(buttonArchivSave));
            view_archiv.comboBoxBlock_IndexChange(new EventHandler (comboBoxBlockIndexChange));
            view_archiv.comboBoxSeparation_IndexChange(new EventHandler (comboBoxSeparationIndexChange));
            view_archiv.comboBoxRTM_IndexChange(new EventHandler (comboBoxRTMIndexChange));
            //-------------------------------------------------------------------------------------------
            // Properties.Settings.Default.IPAdress = "127.0.0.1";
            // Properties.Settings.Default.Save();

            model.numberProtocol = Properties.Settings.Default.NumberProtocol;

            model.conntectToPLC();

            readReg = new Thread(readHolding);
            readReg.IsBackground = true;
            readReg.Start();
        }

        private void виходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            model.insertActioToDBAction("Выход из програми");
            if (MessageBox.Show("Вы действительно хотите выйти из программи?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }
        private void настройкиПринтераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            model.insertActioToDBAction("Подпрограма настройки принтера");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = Convert.ToString(DateTime.Now);

            DateTime forArchiv = Convert.ToDateTime(view_archiv.dateTimePicker1.Value);
            view_archiv.label2.Text = forArchiv.ToString("");

        }

        // Переключения между рабочими окнами
        private void toolStripButton1_Click(object sender, EventArgs e) // Вікно підготовки перевірки і вибору ГА
        {
            cleareView_wStart_Controls();

            if (model.numberStep == 1)
            {
                model.insertActioToDBAction("Вход в подпрограму выбора ГА");
                view_wStart.Show();
                view_wFinish.Hide();
                view_work.Hide();
                view_graphik.Hide();
            }
            else if (model.numberStep == 2)
            {
                model.insertActioToDBAction("Вход в испытание выбраного ГА");
                view_work.Show();
                view_wFinish.Hide();
                view_wStart.Hide();
                view_graphik.Hide();
            }
            else if (model.numberStep == 3)
            {
                model.insertActioToDBAction("Вход в подпрограму создания протокола испытания ГА");
                view_wFinish.Show();
                view_wStart.Hide();
                view_work.Hide();
                view_graphik.Hide();
            }

            view_alarm.Hide();
            view_archiv.Hide();
            view_seting.Hide();
            view_graphik.Hide();


            view_wStart.comboBox2.Items.Clear();
            view_wStart.comboBox1.Items.Clear();

            foreach (DataRow row in model.selectAbsorber().Rows) //Вибір і занесення в комбобокс амортизаторів з бази даних 
            {
                String absorberName = row["Serial"].ToString();
                view_wStart.comboBox1.Items.Add(absorberName);
            }

            foreach (DataRow row in model.selectUser().Rows)
            {
                String usersName = row["userName"].ToString();
                view_wStart.comboBox2.Items.Add(usersName);
            }
        }
        private void toolStripButton2_Click(object sender, EventArgs e) // Вікно несправностей/діагностики
        {
            view_alarm.Show();
            model.insertActioToDBAction("Вход в Диагностику");
            view_wStart.Hide();
            view_archiv.Hide();
            view_seting.Hide();
            view_wFinish.Hide();
            view_work.Hide();
            view_graphik.Hide();
        }
        private void toolStripButton3_Click(object sender, EventArgs e) // Вікно роботи з архівом вимірювань параметрів ГА
        {
            view_archiv.Show();
            model.insertActioToDBAction("Вход в Архив");
            view_wStart.Hide();
            view_alarm.Hide();
            view_seting.Hide();
            view_wFinish.Hide();
            view_work.Hide();
            view_graphik.Hide();
        }
        private void toolStripButton1_Click_1(object sender, EventArgs e) // Вікно налаштувань
        {
            if (model.numberStep == 1)
            {
                view_seting.Show();
                model.insertActioToDBAction("Вход в Настройки");
                view_wStart.Hide();
                view_archiv.Hide();
                view_archiv.Hide();
                view_wFinish.Hide();
                view_work.Hide();
                view_graphik.Hide();

                view_seting.comboBoxSSAbsorbers.Items.Clear();

                view_seting.comboBoxRTMAbsorbers.Items.Clear();
                view_seting.comboBox2.Items.Clear();

                foreach (DataRow row in model.selectAbsorber().Rows) //Вибір і занесення в комбобокс амортизаторів з бази даних 
                {
                    String absorberName = row["Serial"].ToString();
                    view_seting.comboBoxSSAbsorbers.Items.Add(absorberName);
                }

                foreach(DataRow row in model.selectAbsorber().Rows) //Вибір і занесення в комбобокс амортизаторів з бази даних 
                {
                    String absorberName = row["RTM"].ToString();
                    view_seting.comboBoxRTMAbsorbers.Items.Add(absorberName);
                }

                foreach (DataRow row in model.selectUser().Rows)
                {
                    String usersName = row["userName"].ToString();
                    view_seting.comboBox2.Items.Add(usersName);
                }
            }
            else
            {
                MessageBox.Show("Идет провергка ГА. Изменения в настройках запрещены! ", "Внимани");
            }
        }
        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            view_graphik.Show();
            view_alarm.Hide();
            model.insertActioToDBAction("Вход в Графики");
            view_wStart.Hide();
            view_archiv.Hide();
            view_seting.Hide();
            view_wFinish.Hide();
            view_work.Hide();
        }

        // Собития по нажатию на кнопок контекстного меню
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            view_work.groupBox1.Visible = view_work.groupBox2.Visible = view_work.groupBox3.Visible = false;
        }


        // Собития по нажатию на кнопок в View_Viev_wStart
        private void button_Next(object sender, EventArgs e)
        {
            if (view_wStart.comboBox1.Text != "" && view_wStart.textBox1.Text != "" && view_wStart.textBox2.Text != "" && view_wStart.textBox3.Text != ""
                && view_wStart.textBox4.Text != "" && view_wStart.textBox5.Text != "" && view_wStart.textBox6.Text != "" && view_wStart.comboBox2.Text != "")
            {
                model.insertActioToDBAction("Вход в испытание выбраного ГА");
                view_work.Show();
                view_wStart.Hide();
                view_archiv.Hide();
                view_archiv.Hide();
                view_seting.Hide();
                view_wFinish.Hide();
                model.numberStep = 2;

                view_work.textBox25.Text = model.Sxol.ToString();
            }
            else
            {
                MessageBox.Show("Введите всю ОСНОВНУЮ информация по испытанию ГА", "ВНИМАНИЕ");
            }
        }
        private void comboBox1SelectedIndexChanged(object sender, EventArgs e) //Вибір параметрів вибраного амортизатора з бази даних
        {
            if (view_wStart.comboBox1.Text != "")
            {
                model.selectAbsorberFromDB(view_wStart.comboBox1.Text);
            }
            view_wStart.show();
            view_wStart.Invalidate();

        }
        private void comboBox2SelectedIndexChanged(object sender, EventArgs e)
        {
            view_wStart.show();
            view_wStart.Invalidate();
        }
        private void cleareView_wStart_Controls ()
        {
            foreach (Control c in view_wStart.groupBox1.Controls)
                if (c is TextBox)
                {
                    ((TextBox) c).Text=null;
                }
        }

        //Собития по нажатию на кнопки в View_Work
        private void checkBox_ManualChecked(object sender, EventArgs e)
        {
            if (view_work.checkBox_Manual.Checked==true)
            {
                view_work.checkBox_Auto.Enabled=false;
                model.work=Model.Work.Manual;
                view_work.groupBox1.Enabled=true;
            }

            if (view_work.checkBox_Manual.Checked==false)
            {
                view_work.checkBox_Auto.Enabled=true;
                model.work=Model.Work.Stop;
                view_work.groupBox1.Enabled=false;
                view_work.groupBox1.Enabled=view_work.groupBox2.Enabled=view_work.groupBox3.Enabled=
                view_work.groupBox4.Enabled=view_work.groupBox5.Enabled=view_work.groupBox8.Enabled=false;
            }
        }
        private void checkBox_AutoChecked(object sender, EventArgs e)
        {
            if (view_work.checkBox_Auto.Checked==true)
            {
                view_work.checkBox_Manual.Enabled=false;
                model.work=Model.Work.Auto;
                view_work.groupBox1.Enabled=true;
            }

            if (view_work.checkBox_Auto.Checked==false)
            {
                view_work.checkBox_Manual.Enabled=true;
                model.work=Model.Work.Stop;
                view_work.groupBox1.Enabled=false;
                view_work.groupBox1.Enabled=view_work.groupBox2.Enabled=view_work.groupBox3.Enabled=
                view_work.groupBox4.Enabled=view_work.groupBox5.Enabled=view_work.groupBox8.Enabled=false;
            }
        }       
        private void comboBoxViewWorkStartChecked(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkCompressingMouseDown(object sender, EventArgs e)
        {
            view_work.label1.Text="Идет сжатие";
        }
        private void buttonViewWorkCompressingMouseUp(object sender, EventArgs e)
        {
            view_work.label1.Text="Останов сжатия";
        }
        private void buttonViewWorkStreatchingMouseDown(object sender, EventArgs e)
        {
            view_work.label1.Text="Идет растяжение";
        }
        private void buttonViewWorkStreatchingMouseUp(object sender, EventArgs e)
        {
            view_work.label1.Text="Останов растяжения";
        }
        private void buttonViewWorkOK_Setup(object sender, EventArgs e)
        {
            view_work.groupBox1.Enabled=false;
            Thread.Sleep(1000);
            view_work.groupBox4.Enabled=true;
        }

        private void buttonViewWorkCompress(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkStreatch(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkCentr(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkOK_Air(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkRep_nn(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkOK_nn(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkOK_xx(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkOK_Speed(object sender, EventArgs e)
        {

        }
        private void buttonViewWorkOK_Shol(object sender, EventArgs e)
        {

        }
        private void button_View_Work_Next(object sender, EventArgs e)
        {
            model.insertActioToDBAction("Вход в подпрограму создания протокола испытания ГА");
            view_wFinish.Show();
            view_wStart.Hide();
            view_archiv.Hide();
            view_archiv.Hide();
            view_seting.Hide();
            view_work.Hide();
            model.numberStep=3;
            model.numberProtocol++;

            model.Fxcompress=model.FxcompressM;
            model.Lcompress=model.LcompressM;
            model.Fxstretching=model.FxstretchingM;
            model.Lstretching=model.LstretchingM;
            model.Fhcompress=model.FhcompressM;
            model.Fhstretching=model.FhstretchingM;
            model.Vcompress=model.VcompressM;
            model.Vstretching=model.VstretchingM;

            Properties.Settings.Default.NumberProtocol=model.numberProtocol;
            Properties.Settings.Default.Save();

            view_wFinish.textBox2.Text=model.numberProtocol.ToString();

            view_wFinish.textBox3.Text=model.Fxcompress.ToString("0,0");
            view_wFinish.textBox4.Text=model.Lcompress.ToString("0,0");
            view_wFinish.textBox5.Text=model.Fhcompress.ToString("0,0");
            view_wFinish.textBox6.Text=model.Vcompress.ToString("0,0");
            view_wFinish.textBox9.Text=model.Fxstretching.ToString("0,0");
            view_wFinish.textBox8.Text=model.Vgidcost.ToString();
            view_wFinish.textBox10.Text=model.Lstretching.ToString("0,0");
            view_wFinish.textBox11.Text=model.Fhstretching.ToString("0,0");
            view_wFinish.textBox12.Text=model.Vstretching.ToString("0,0");

            model.Shold=Convert.ToDouble(view_work.textBox25.Text);

            DataTable selectZIP = model.selectZIP(model.type);

            view_wFinish.listView1.Items.Clear();
            foreach (DataRow row in selectZIP.Rows)
            {
                ListViewItem item = new ListViewItem();
                item.SubItems.Add(row["MestoUstanovki"].ToString());
                item.SubItems.Add(row["NaimenKolca"].ToString());
                item.SubItems.Add(row["Chertezh"].ToString());
                item.SubItems.Add(row["Typegidcost"].ToString());
                view_wFinish.listView1.Items.Add(item);
                view_wFinish.label16.Text=row["Typegidcost"].ToString();
            }


        }
        
        private void buttonCancelClick (object sender, EventArgs e)
        {
            model.insertActioToDBAction("Отмена проведения испытания");
            view_wStart.Hide();
            view_alarm.Hide();
            view_archiv.Hide();
            view_seting.Hide();
            view_wFinish.Hide();
            view_work.Hide();

            model.restartAllParam();
            view_wStart.show();
            view_wStart.Invalidate();

            model.numberStep=1;
        }

        //Собития по нажатию на кнопки в View_wFinish
        private void buttonFinish(object sender, EventArgs e)
        {
            model.WorkOil = Convert.ToDouble(view_wFinish.textBox8.Text);
            model.Comentc = view_wFinish.textBox7.Text;
            model.saveProtocolType=Model.SaveProtocolType.Work;
           // view_graphik.chart1.SaveImage(@"d:\\chart1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
           // view_graphik.chart2.SaveImage(@"d:\\chart2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
           // view_graphik.chart3.SaveImage(@"d:\\chart3.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
           // view_graphik.chart4.SaveImage(@"d:\\chart4.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
           // view_graphik.chart5.SaveImage(@"d:\\chart5.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
           // view_graphik.chart6.SaveImage(@"d:\\chart6.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            if (view_wFinish.textBox2.Text != "")
            {
                
                model.insertActioToDBAction("Сохраниение испытания №: " + model.numberProtocol + "");
                model.addAllMeasure();
                view_wStart.Hide();
                view_alarm.Hide();
                view_archiv.Hide();
                view_seting.Hide();
                view_wFinish.Hide();
                view_work.Hide();
                model.numberStep = 1;
                model.Comentc = view_wFinish.textBox7.Text;
                //model.savePritokol();
                model.Typegidcost=model.TypegidcostArch="";
                view_wFinish.label16.Text=" ";
            }
            else
            {
                MessageBox.Show("Введите номер протокола", "ВНИМАНИЕ");
            }

            model.restartAllParam();
            view_wStart.show();
            view_wStart.Invalidate();
            view_wStart.comboBox1.Text="";
            view_wFinish.checkBox1.Checked=false;
            view_wFinish.textBox7.Text="";
        }
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string name = saveFileDialog1.FileName;
            if (model.saveProtocolType==Model.SaveProtocolType.Work)
            { model.savePritokol(); }
            if (model.saveProtocolType==Model.SaveProtocolType.Archiv)
            { model.saveProtokolFromArchiv();  }
        }
         

        //Собития по нажатию кнопки в View_Seting
        private void buttonAdd(object sender, EventArgs e) // кнопка добавлення ГА в базу данних
        {
            if (view_seting.textBox1.Text != "" && view_seting.textBox2.Text != "" && view_seting.textBox3.Text != "" && view_seting.textBox4.Text != "" &&
                view_seting.textBox5.Text != "" && view_seting.textBox6.Text != "" && view_seting.textBox7.Text != "" && view_seting.textBox8.Text != "" &&
                view_seting.textBox9.Text != "" && view_seting.textBox10.Text != "" && view_seting.textBox11.Text != "" && view_seting.textBox12.Text != "" &&
                view_seting.textBox13.Text != "" && view_seting.textBox14.Text != "" && view_seting.textBox15.Text != "" && view_seting.textBox16.Text != "" &&
                view_seting.textBox17.Text != "" && view_seting.textBox18.Text != "" && view_seting.textBox19.Text != "")
            {
                if (model.checkAbsorbers(view_seting.textBox1.Text) == false)
                {
                    model.absorberParametr = view_seting.textBox1.Text + "', '" + view_seting.textBox2.Text + "', '" + view_seting.textBox3.Text + "', '" + view_seting.textBox4.Text +
                        "', '" + view_seting.textBox5.Text + "', '" + view_seting.textBox6.Text + "', '" + view_seting.textBox7.Text + "', '" + Convert.ToDouble(view_seting.textBox8.Text) +
                        "', '" + Convert.ToDouble(view_seting.textBox9.Text) + "', '" + Convert.ToDouble(view_seting.textBox10.Text) + "', '" + Convert.ToDouble(view_seting.textBox11.Text) +
                        "', '" + Convert.ToDouble(view_seting.textBox12.Text) + "', '" + Convert.ToDouble(view_seting.textBox13.Text) + "', '" + Convert.ToDouble(view_seting.textBox14.Text) +
                        "', '" + Convert.ToDouble(view_seting.textBox15.Text) + "', '" + Convert.ToDouble(view_seting.textBox16.Text) + "', '" + Convert.ToDouble(view_seting.textBox17.Text) +
                        "', '" + Convert.ToDouble(view_seting.textBox18.Text) + "', '" + view_seting.textBox19.Text + "', '" + Convert.ToDouble(view_seting.textBox20.Text) + 
                        "', '" + Convert.ToDouble(view_seting.textBox21.Text);

                    model.addToTableAbsorbers(model.absorberParametr);
                    MessageBox.Show("ГА cерийный номер № " + view_seting.textBox1.Text + " сохранен в базу", "Внимание");
                    model.insertActioToDBAction("ГА cерийный номер № " + view_seting.textBox1.Text + " сохранен в базу ");
                }
                else
                {
                    MessageBox.Show("ГА с таким серийным номером уже существует", "Внимание");
                    model.insertActioToDBAction("Сохранение ГА в базу. Ошибка! ГА с таким серийным номером уже существует!");
                }
            }
            else
            {
                MessageBox.Show("ВВедите все параметры ГА", "Внимание");
                model.insertActioToDBAction("Сохранение ГА в базу. Ошибка!");
            }
        }
        private void buttonDeleteAbsorber(object sender, EventArgs e)
        {
            if (view_seting.textBox1.Text != "")
            {
                model.insertActioToDBAction("Удаление ГА с базы данных. Серийный номер № " + view_seting.textBox1.Text + "");
                if (MessageBox.Show("Вы действительно хотите удалить ГА с серийным № " + view_seting.textBox1.Text + "?", "УДАЛЕНИЕ ГА С БАЗЫ ДАННЫХ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    model.deleteAbsorbers(view_seting.textBox1.Text);
                    model.insertActioToDBAction("ГА с серийным № " + view_seting.textBox1.Text + " был удален с базы данных.");
                }
            }
            else
            {
                MessageBox.Show("Выберитите ГА", "Внимание");
                model.insertActioToDBAction("Ошибка! Не выбран ГА при попытке удаления с базы данных");
            }
        }
        private void buttonUpdateAbsorber(object sender, EventArgs e)
        {
            if (view_seting.textBox1.Text != "")
            {
                String comand = "UPDATE dbAbsorbers SET Type = '" + view_seting.textBox2.Text + "',Block = '" + view_seting.textBox3.Text + "',Separation = '" + view_seting.textBox4.Text +
                    "',Posit = '" + view_seting.textBox5.Text + "',pSetup = '" + view_seting.textBox6.Text + "', RTM = '" + view_seting.textBox7.Text +
                    "', So='" + Convert.ToInt32(view_seting.textBox8.Text) + "', Xo='" + Convert.ToInt32(view_seting.textBox9.Text) +
                    "', Fxx='" + Convert.ToInt32(view_seting.textBox10.Text) + "', L='" + Convert.ToInt32(view_seting.textBox11.Text) + "', Fhe='" + Convert.ToInt32(view_seting.textBox12.Text) +
                    "', Vmin='" + Convert.ToInt32(view_seting.textBox13.Text) + "', Vmax='" + Convert.ToInt32(view_seting.textBox14.Text) +
                    "', Sxol='" + Convert.ToInt32(view_seting.textBox15.Text) + "',Sxolmin='" + Convert.ToInt32(view_seting.textBox16.Text) + "', Sxolmax='" + Convert.ToInt32(view_seting.textBox17.Text) +
                    "', Preasure='" + Convert.ToInt32(view_seting.textBox18.Text) + "', Rate='" + Convert.ToInt32(view_seting.textBox19.Text) + "' WHERE Serial = '" + view_seting.textBox1.Text + "' ";
                model.updateAbsorber(comand);
                MessageBox.Show("ГА, серийныйый номер № " + view_seting.textBox1.Text + " был изменен", "ИЗМЕНЕНИЯ");
                model.insertActioToDBAction("ГА, серийныйый номер № " + view_seting.textBox1.Text + " был изменен");
            }
            else
            {
                MessageBox.Show("При изменении ГА возникла ошибка. Не выбран серийный номер ГА", "ВНИМАНИЕ");
                model.insertActioToDBAction("При изменении ГА возникла ошибка. Не выбран серийный номер ГА");
            }

        }// Нужно дописать
        private void buttonAddUser(object sender, EventArgs e) // Кнопка добавлення користувача
        {
            if (view_seting.comboBox2.Text != "" && view_seting.textBox20.Text != "")
            {
                model.addToTableUsers(view_seting.comboBox2.Text, Convert.ToInt32(view_seting.textBox22.Text));
                model.insertActioToDBAction("Добавление пользователя в базу данных. Имя пользователя : " + view_seting.comboBox2.Text + "");
            }
            else
            {
                MessageBox.Show("ВВедите все параметры пользователя", "Внимание");
            }
        }
        private void buttonDeleteUsers(object sender, EventArgs e)
        {
            if (view_seting.comboBox2.Text != "")
            {
                model.insertActioToDBAction("Удаление пользователя с базы данных. Имя пользователя : " + view_seting.comboBox2.Text + "");

                if (MessageBox.Show("Вы действительно хотите удалить пользователя с именем: " + view_seting.comboBox2.Text + "?", "УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЯ С БАЗЫ ДАННЫХ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    model.deleteUser(view_seting.comboBox2.Text);
                    model.insertActioToDBAction("Пользователь с именем " + view_seting.comboBox2.Text + " был удален с базы данных.");
                }
            }
            else
            {
                MessageBox.Show("Выберитите пользователя", "Внимание");
                model.insertActioToDBAction("Ошибка! Не выбран пользователь при попытке удаления с базы данных");
            }

        }
        private void comboBoxSSAbsorbersIndexChanged(object sender, EventArgs e) // Вибір ГА з бази данних
        {
            if (view_seting.comboBoxSSAbsorbers.Text!="")
            {
                model.selectAbsorberFromDB(view_seting.comboBoxSSAbsorbers.Text);
            }
            view_seting.comboBoxRTMAbsorbers.Text="";
            view_seting.showTextBox();
        }
        private void comboBoxSSAbsorbersClick(object sender, EventArgs e)
        {
            view_seting.comboBoxSSAbsorbers.Items.Clear();
            foreach (DataRow row in model.selectAbsorber().Rows) //Вибір і занесення в комбобокс амортизаторів з бази даних 
            {
                String absorberName = row["Serial"].ToString();
                view_seting.comboBoxSSAbsorbers.Items.Add(absorberName);
            }
        }
        private void comboBoxRTMAbsorbersClick (object sender, EventArgs e)
        {
            view_seting.comboBoxRTMAbsorbers.Items.Clear();
            foreach (DataRow row in model.selectAbsorber().Rows) //Вибір і занесення в комбобокс амортизаторів з бази даних 
            {
                String absorberName = row["RTM"].ToString();
                view_seting.comboBoxRTMAbsorbers.Items.Add(absorberName);
            }
        }

        private void comboBoxRTMAbsorbersSelectedIndexChange (object sender, EventArgs e)
        {
            if (view_seting.comboBoxRTMAbsorbers.Text != "")
            {
                model.selectAbsorberFromDB_RTM(view_seting.comboBoxRTMAbsorbers.Text);
            }
            view_seting.comboBoxSSAbsorbers.Text="";
            view_seting.showTextBox();
        }
        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void связьСPLCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tcp_ip = new TCP_IP();
            tcp_ip.Show();
        }

        //Собития по нажатию кнопки в View_Archiv
        private void dateTimePicker1ValueChanged (object sender, EventArgs e)
        {
            view_archiv.dataGridView1.Columns.Clear();

            DataTable qwerty = model.selectAllMeasureByDate(view_archiv.dateTimePicker1.Value, view_archiv.dateTimePicker2.Value);
            view_archiv.dataGridView1.DataSource=model.createTableReport(qwerty);

            view_archiv.comboBoxSelectCerial.Items.Clear();
            view_archiv.comboBoxSelectCerial.Text = "";

            view_archiv.comboBoxSelectProtocol.Items.Clear();
            view_archiv.comboBoxSelectProtocol.Text = "";

            view_archiv.comboBoxBlock.Items.Clear();
            view_archiv.comboBoxBlock.Text="";

            view_archiv.comboBoxSeparation.Items.Clear();
            view_archiv.comboBoxSeparation.Text="";

            view_archiv.comboBoxRTM.Items.Clear();
            view_archiv.comboBoxRTM.Text="";

            foreach (DataRow row in qwerty.Rows)
            {
                String Block = row["Block"].ToString();
                view_archiv.comboBoxBlock.Items.Add(Block);
               
                object[] itemsBlock = view_archiv.comboBoxBlock.Items.OfType<String>().Distinct().ToArray();
                view_archiv.comboBoxBlock.Items.Clear();
                view_archiv.comboBoxBlock.Items.AddRange(itemsBlock);

            }
            
            view_archiv.Invalidate();
        }
        private void dateTimePicker2ValueChanged(object sender, EventArgs e)
        {
            view_archiv.dataGridView1.Columns.Clear();

            DataTable qwerty = model.selectAllMeasureByDate(view_archiv.dateTimePicker1.Value, view_archiv.dateTimePicker2.Value);
            view_archiv.dataGridView1.DataSource=model.createTableReport(qwerty);

            view_archiv.comboBoxSelectCerial.Items.Clear();
            view_archiv.comboBoxSelectCerial.Text="";

            view_archiv.comboBoxSelectProtocol.Items.Clear();
            view_archiv.comboBoxSelectProtocol.Text="";

            view_archiv.comboBoxBlock.Items.Clear();
            view_archiv.comboBoxBlock.Text="";

            view_archiv.comboBoxSeparation.Items.Clear();
            view_archiv.comboBoxSeparation.Text="";

            view_archiv.comboBoxRTM.Items.Clear();
            view_archiv.comboBoxRTM.Text="";



            foreach (DataRow row in qwerty.Rows)
            {
                String Block = row["Block"].ToString();
                view_archiv.comboBoxBlock.Items.Add(Block);

                object[] itemsBlock = view_archiv.comboBoxBlock.Items.OfType<String>().Distinct().ToArray();
                view_archiv.comboBoxBlock.Items.Clear();
                view_archiv.comboBoxBlock.Items.AddRange(itemsBlock);
            }    
            
                view_archiv.Invalidate();
        }
      
        private void comboBoxBlockIndexChange (object sender, EventArgs e)
        {
            DataTable qwertyBlock = model.selectAllMeasureByDateBlock(view_archiv.dateTimePicker1.Value, view_archiv.dateTimePicker2.Value, view_archiv.comboBoxBlock.Text);

            view_archiv.dataGridView1.DataSource=model.createTableReport(qwertyBlock);

            view_archiv.comboBoxSeparation.Items.Clear();
            view_archiv.comboBoxSeparation.Text="";

            view_archiv.comboBoxRTM.Items.Clear();
            view_archiv.comboBoxRTM.Text="";

            view_archiv.comboBoxSelectProtocol.Items.Clear();
            view_archiv.comboBoxSelectProtocol.Text="";

            foreach (DataRow row1 in qwertyBlock.Rows)
            {
                String Protocol = row1["Separation"].ToString();
                view_archiv.comboBoxSeparation.Items.Add(Protocol);

                object[] itemsSeparation = view_archiv.comboBoxSeparation.Items.OfType<String>().Distinct().ToArray();
                view_archiv.comboBoxSeparation.Items.Clear();
                view_archiv.comboBoxSeparation.Items.AddRange(itemsSeparation);
            }

            view_archiv.Invalidate();
        }
        private void comboBoxSeparationIndexChange(object sender, EventArgs e)
        {
            DataTable qwertySeparation = model.selectAllMeasureByDateBlockSeparation(view_archiv.dateTimePicker1.Value, view_archiv.dateTimePicker2.Value, view_archiv.comboBoxBlock.Text, view_archiv.comboBoxSeparation.Text);

            view_archiv.dataGridView1.DataSource=model.createTableReport(qwertySeparation);
            view_archiv.comboBoxRTM.Items.Clear();
            view_archiv.comboBoxRTM.Text="";

            view_archiv.comboBoxSelectProtocol.Items.Clear();
            view_archiv.comboBoxSelectProtocol.Text="";

            foreach (DataRow row1 in qwertySeparation.Rows)
            {
                String Protocol = row1["RTM"].ToString();
                view_archiv.comboBoxRTM.Items.Add(Protocol);

                object[] itemsRTM = view_archiv.comboBoxRTM.Items.OfType<String>().Distinct().ToArray();
                view_archiv.comboBoxRTM.Items.Clear();
                view_archiv.comboBoxRTM.Items.AddRange(itemsRTM);
            }

            view_archiv.Invalidate();
        }
        private void comboBoxRTMIndexChange (object sender, EventArgs e)
        {
            DataTable qwertyRTM = model.selectAllMeasureByDateBlockSeparationRTM(view_archiv.dateTimePicker1.Value, view_archiv.dateTimePicker2.Value, view_archiv.comboBoxBlock.Text
                , view_archiv.comboBoxSeparation.Text, view_archiv.comboBoxRTM.Text);
            view_archiv.dataGridView1.DataSource=model.createTableReport(qwertyRTM);

            view_archiv.comboBoxSelectProtocol.Items.Clear();
            view_archiv.comboBoxSelectProtocol.Text="";

            foreach (DataRow row1 in qwertyRTM.Rows)
            {
                String Protocol = row1["Protocol"].ToString();
                view_archiv.comboBoxSelectProtocol.Items.Add(Protocol);

            }

            view_archiv.Invalidate();
        }
        private void comboBoxSelectProtocolIndexChange (object sender, EventArgs e)
        {
            model.selectAllMeasureByProtocol(Convert.ToInt32( view_archiv.comboBoxSelectProtocol.Text));

            view_archiv.textBox1.Text = model.timeExpArch.ToString("dd.MM.yyyy");
            view_archiv.textBox2.Text = model.SerialArch.ToString();
            view_archiv.textBox3.Text = model.FhcompressArch.ToString()+" / "+model.FhstretchingArch.ToString();
            view_archiv.textBox4.Text = model.LcompressArch.ToString()+" / "+ model.LstretchingArch.ToString();
            view_archiv.textBox5.Text ="= "+model.FhArch.ToString();
            view_archiv.textBox6.Text ="≤ " + model.LArch.ToString("0.0");
            view_archiv.textBox7.Text = model.FxcompressArch.ToString()+" / "+model.FxstretchingArch.ToString();
            view_archiv.textBox8.Text = "≤ " + model.FxArch.ToString();
            view_archiv.textBox9.Text = model.VcompressArch.ToString()+" / "+model.VstretchingArch.ToString();
            view_archiv.textBox10.Text = model.VminArch.ToString("0.0") + "..." + model.VmaxArch.ToString("0.0");
            view_archiv.textBox11.Text = model.SholdArch.ToString();
            view_archiv.textBox14.Text = model.WorkOilArch.ToString();
            view_archiv.textBox15.Text = model.ProtocolArch.ToString();
            view_archiv.textBox16.Text = model.TestUserArch.ToString();
           // view_archiv.textBox17.Text = model.ComentcArch.ToString();

            view_archiv.textBox18.Text = model.typeArch.ToString();
            view_archiv.textBox19.Text = model.blockArch.ToString();
            view_archiv.textBox20.Text = model.separationArch.ToString();
            view_archiv.textBox21.Text = model.positionArch.ToString();
            view_archiv.textBox22.Text = model.pSetupArch.ToString();
            view_archiv.textBox23.Text = model.rtmArch.ToString();
            view_archiv.textBox24.Text = model.SoArch.ToString();

            view_archiv.Invalidate();
        }

        private void comboBoxSelectCerialIndexChange(object sender, EventArgs e)
                {
                    DataTable qwertySerial = model.selectAllMeasureByDateSerial(view_archiv.dateTimePicker1.Value, view_archiv.dateTimePicker2.Value, view_archiv.comboBoxSelectCerial.Text);
                    //view_archiv.dataGridView1.DataSource=qwertySerial;
                    view_archiv.dataGridView1.DataSource=model.createTableReport(qwertySerial);
                    view_archiv.comboBoxSelectProtocol.Items.Clear();
                    view_archiv.comboBoxSelectProtocol.Text = "";

                        foreach (DataRow row1 in qwertySerial.Rows)
                        {
                            String Protocol = row1["Protocol"].ToString();
                            view_archiv.comboBoxSelectProtocol.Items.Add(Protocol);
                        }

                    view_archiv.Invalidate();
                }
        private void buttonArchivEnter (object sender, EventArgs e)
        {
            if (view_archiv.comboBoxSelectProtocol.Text!="")
            {
                model.saveProtocolType=Model.SaveProtocolType.Archiv;
                model.saveProtokolFromArchiv();
            }
            else
            {
                MessageBox.Show("Не выбран протокол", "Внимание");
            }
        }
        private void buttonArchivSave(object sender, EventArgs e)
        {
            model.saveProtocolType=Model.SaveProtocolType.Archiv;
            // model.saveProtokolFromArchiv();
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter="Word Documents (*.docx)|*.docx";

            sfd.FileName= DateTime.Now.ToString("dd/MM/yyyy hh-mm-ss") + ".docx";

            if (sfd.ShowDialog()==DialogResult.OK)
            {
                model.saveDataGgridViewToWord(view_archiv.dataGridView1, sfd.FileName);
            }
        }

        //private Master MBmaster;
        private byte[] data;
        private byte[] dataSent = new byte [512];
        private Double[] dataNe = new double[256];
        Random rnd = new Random();

        public void readHolding ()
        {
            while (true)
            {
                dataNe = model.ModBusTCP_ReadHold_Registers(1,0,50);

                /*
                for (int i=0; i<= 50; i++)
                {
                    dataNe[i] = rnd.Next(0,100000);
                }
                */
                
                Thread.Sleep(500);
                model.FxcompressM = dataNe[25];
                model.LcompressM = dataNe[26];
                model.FxstretchingM = dataNe[27];
                model.LstretchingM = dataNe[28];
                model.FhcompressM = dataNe[29];
                model.FhstretchingM = dataNe[30];
                model.VcompressM = dataNe[31];
                model.VstretchingM = dataNe[32];
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            model.conntectToPLC();
        }
        private void button2_Click(object sender, EventArgs e)
        { 
          model.ModBusTCP_WriteSingleHold_Registers(1,0,12345);
        }
    }
}
