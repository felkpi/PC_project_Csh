using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Data.Sql;
using ADOX;

using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
//using System.Threading;
using Word = Microsoft.Office.Interop.Word;
namespace STAS_60
{

    public class Model
    {
        public UInt16 sendComandToPLC;

        static public UInt16 Compressing = 1;
        static public UInt16 Stretching = 2;
        static public UInt16 Compress = 3;
        static public UInt16 Stretch = 4;
        static public UInt16 Centr = 5;
        public enum Work { Auto, Manual, Stop };
        public Work work = Work.Stop;

        public enum SaveProtocolType { Work, Archiv };
        public SaveProtocolType saveProtocolType;

        OleDbConnection dbAbsorber;
        String dbAbsorberConnection = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Stas-60_500\\dbStas-60_500.accdb";

        OleDbConnection dbAction;
        String dbActionConnection = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Stas-60_500\\dbAction-60_500.accdb";

        OleDbConnection dbMeasure;
        String dbMeasureConnection = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Stas-60_500\\dbMeasure-60_500.accdb";

        public String absorberParametr;
        public String serial, type, block, separation, position, pSetup, rtm, Typegidcost;
        public String user;
        public Double So, Xo, Fx, L, Fh, Vmin, Vmax, Sxol, Sxolmin, Sxolmax, Vgidcost, preasure, rate;
        public int numberStep = 1;

        //Поля для занесення параметрів ГА і параметрів вимірювання в базу данних
        public DateTime timeExp;
        public Double Fhcompress, Lcompress , Fhstretching , Lstretching ;
        public Double Fxcompress , Fxstretching ;
        public Double Vcompress , Vstretching ;

        public Double FhcompressM = 0, LcompressM = 0, FhstretchingM = 0, LstretchingM = 0;
        public Double FxcompressM = 0, FxstretchingM = 0;
        public Double VcompressM = 0, VstretchingM = 0;

        public Double Shold = 0, WorkOil = 0;
        public int Cylinder = 0, Stok = 0;
        public String Comentc = "без комент.", RTI = "";

        //------------------------------поля для роботи з архівними даними-------------------------------------------
        public DateTime timeExpArch;
        public Double FhcompressArch = 0, LcompressArch = 0, FhstretchingArch = 0, LstretchingArch = 0;
        public Double FxcompressArch = 0, FxstretchingArch = 0;
        public Double VcompressArch = 0, VstretchingArch = 0;

        public Double SholdArch = 0, WorkOilArch = 0;
        public int ProtocolArch, CylinderArch = 0, StokArch = 0;
        public String TestUserArch, SerialArch, ComentcArch = "без комент.", RTIArch = "";

        public String serialArch, typeArch, blockArch, separationArch, positionArch, pSetupArch, rtmArch, TypegidcostArch, changeMasloArch;
        public Double SoArch, XoArch, FxArch, LArch, FhArch, VminArch, VmaxArch, SxolArch, SxolminArch, SxolmaxArch, VgidcostArch, preasureArch, rateArch;
        //--------------------------------------------------------------------------
        public String saveProtokolFolder;
        public int numberProtocol;
        public String comentsFor;


        public String[] ChangeRTI = new String[256];
        public String changeMaslo;



        public Model()
        {
            timeExp=DateTime.Today;
        }
        //-------------------------------------------------------------------------------------------------------------------------------
        public void restartAllParam()
        {
            absorberParametr="";
            serial=type=block=separation=position=pSetup=rtm=Typegidcost="";
            user="";
            So=Xo=Fx=L=Fh=Vmin=Vmax=Sxol=Sxolmin=Sxolmax=Vgidcost=preasure=rate=0;
            numberStep=1;
        }
        public void createFolder()// Створення папки на диску Д
        {
            Directory.CreateDirectory("D:\\Stas-60_500");
            createDBAbsorber();
        }
        public void createDBAbsorber()// Створення бази данних в папці на диску Д
        {
            String ON_dbAbsorber = @"D:\\Stas-60_500\\dbStas-60_500.accdb";

            if (File.Exists(ON_dbAbsorber)==false)
            {
                var create_dbAbsorber = new ADOX.Catalog();
                create_dbAbsorber.Create(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Stas-60_500\\dbStas-60_500.accdb");
                createTableAbsorbers();
                createTableUsers();
            }

        }
        public void createDBAction()
        {
            String ON_dbAction = @"D:\\Stas-60_500\\dbAction-60_500.accdb";
            if (File.Exists(ON_dbAction)==false)
            {
                var create_dbAction = new ADOX.Catalog();
                create_dbAction.Create(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Stas-60_500\\dbAction-60_500.accdb");
            }
        }//Створення бази для подій
        public void createMeasureDB()
        {
            String ON_dbAction = @"D:\\Stas-60_500\\dbMeasure-60_500.accdb";
            if (File.Exists(ON_dbAction)==false)
            {
                var create_dbMeasure = new ADOX.Catalog();
                create_dbMeasure.Create(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Stas-60_500\\dbMeasure-60_500.accdb");

                createTableMeasure();
            }
        }// Створення бази для занесення даних вимірювань
        //--------------------------------------------------------------------------------------------------------------------------------
        public DataTable createTableReport(DataTable measureAbsorberData)
        {
            DataTable tableForReport = new DataTable();
            DataRow dr;

            tableForReport.Columns.Add("Наименование оборудования");
            tableForReport.Columns.Add("РТМ");
            tableForReport.Columns.Add("Тип ГА");
            //tableForReport.Columns.Add("Номинальная нагрузка Fh, кг, доп.знач.");
            //tableForReport.Columns.Add("Номинальная нагрузка Fh, кг, изм.знач.");
            tableForReport.Columns.Add("Смещение поршня ГА, δ, мм, при Fh, доп.знач.");
            tableForReport.Columns.Add("Смещение поршня ГА, δ, мм, при Fh, изм.знач.");
            tableForReport.Columns.Add("Сопротивление холостого хода, Fxx, кгс, доп.знач.");
            tableForReport.Columns.Add("Сопротивление холостого хода, Fxx, кгс, изм.знач.");
            tableForReport.Columns.Add("Скорость закрытия калапана, V, см/с, доп.знач.");
            tableForReport.Columns.Add("Скорость закрытия калапана, V, см/с, изм.знач.");
            /*
            dr=tableForReport.NewRow();
            dr["Наименование оборудования"]="1";
            dr["РТМ"]="2";
            dr["Тип ГА"]="3";
            //dr["Номинальная нагрузка Fh, доп.знач."]="4";
            //dr["Номинальная нагрузка Fh, изм.знач."]="5";
            dr["Смещение поршня ГА, δ, мм, при Fh, доп.знач."]="6";
            dr["Смещение поршня ГА, δ, мм, при Fh, изм.знач."]="7";
            dr["Сопротивление холостого хода, Fxx, кгс, доп.знач."]="8";
            dr["Сопротивление холостого хода, Fxx, кгс, изм.знач."]="9";
            dr["Скорость закрытия калапана, V, см/с, доп.знач."]="10";
            dr["Скорость закрытия калапана, V, см/с, изм.знач."]="11";
            
            tableForReport.Rows.Add(dr);
            */
            foreach (DataRow row in measureAbsorberData.Rows)
            {
                dr=tableForReport.NewRow();
                selectAbsorberFromDBArchive(row["Serial"].ToString());
                dr["Наименование оборудования"]=pSetupArch;
                dr["РТМ"]=row["RTM"].ToString();
                dr["Тип ГА"]=typeArch;
                //dr["Номинальная нагрузка Fh, доп.знач."]=FhArch;
                //dr["Номинальная нагрузка Fh, изм.знач."]=row["Fhcompress"].ToString()+"/"+row["Fhstretching"].ToString();
                dr["Смещение поршня ГА, δ, мм, при Fh, доп.знач."]=LArch.ToString("0.0");
                dr["Смещение поршня ГА, δ, мм, при Fh, изм.знач."]=row["Lcompress"].ToString()+"/"+row["Lstretching"].ToString();
                dr["Сопротивление холостого хода, Fxx, кгс, доп.знач."]=FxArch.ToString("0.0");
                dr["Сопротивление холостого хода, Fxx, кгс, изм.знач."]=row["Fxcompress"].ToString()+"/"+row["Fxstretching"].ToString();
                dr["Скорость закрытия калапана, V, см/с, доп.знач."]=VminArch.ToString("0.0")+"-"+VmaxArch.ToString("0.0");
                dr["Скорость закрытия калапана, V, см/с, изм.знач."]=row["Vcompress"].ToString()+"/"+row["Vstretching"].ToString();
                tableForReport.Rows.Add(dr);
            }
            return tableForReport;
        } // Метод для створення групового звіту по проведеним випробуванням
        public void createTableAction()
        {
            dbAction=new OleDbConnection(dbActionConnection);
            dbAction.Open();

            String tableName = DateTime.Now.ToString("dd-MM-yyyy");

            String actionFolder = "CREATE TABLE ["+tableName+"]([ID] Counter Primary Key, [timeAction] string, [DBAction] string)";

            try
            {
                OleDbCommand createTableAction = new OleDbCommand(actionFolder, dbAction);
                createTableAction.ExecuteReader();
                dbAction.Close();
            }
            catch
            {
                //throw;
                dbAction.Close();
            }
        }// Створення таблички подій
        public void insertActioToDBAction(String DBAction)
        {
            dbAction=new OleDbConnection(dbActionConnection);
            dbAction.Open();
            String tableName = DateTime.Now.ToString("dd-MM-yyyy");
            String actionTime = DateTime.Now.ToString("HH:mm:ss");
            String insertCommand = "INSERT INTO ["+tableName+"] (date, DBAction) VALUES ('"+actionTime+"')";
            OleDbCommand insertAction = new OleDbCommand("INSERT INTO ["+tableName+"] (timeAction, DBAction) VALUES ('"+actionTime+"','"+DBAction+"')", dbAction);
            insertAction.ExecuteNonQuery();
            dbAction.Close();
        }// Занесення подій в базу данних
        //--------------------------------------------------------------------------------------------------------------------------------
        public void createTableAbsorbers()// Створеня таблиці ГА в базі данних
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            OleDbCommand createTAbsorbers = new OleDbCommand("CREATE TABLE [dbAbsorbers]([ID] Counter Primary Key,[Serial] string, [Type] string, [Block] string,"+
               " [Separation] string, [Posit] string, [pSetup] string, [RTM] string, [So] Double, [Xo] Double, [Fx] Double,[L] Double, [Fh] Double,[Vmin] Double, [Vmax] Double,"+
               "[Sxol] Double,[Sxolmin] Double,[Sxolmax] Double,[Vgidcost] Double, [Typegidcost] String,[Preasure] Double, [Rate] Double )", dbAbsorber);
            createTAbsorbers.ExecuteReader();
            dbAbsorber.Close();
        }
        public void addToTableAbsorbers(String parametrs)// Добавлення ГА в базу данних
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            OleDbCommand addAbsorbers = new OleDbCommand("INSERT INTO [dbAbsorbers] (Serial,Type,Block,Separation,Posit,pSetup, RTM, So, Xo, Fx, L, Fh, Vmin, Vmax, Sxol, Sxolmin, Sxolmax, Vgidcost , Typegidcost, Preasure, Rate) VALUES ('"+parametrs+"')", dbAbsorber);

            addAbsorbers.ExecuteNonQuery();
            dbAbsorber.Close();
        }
        public DataTable selectAbsorber() //Вибір амортизаторів з бази даних 
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            DataTable tableAbsorb = new DataTable();
            String selectFromTabke = @"SELECT * FROM [dbAbsorbers] ORDER BY ID ASC";
            OleDbDataAdapter chuseAbsorber = new OleDbDataAdapter(selectFromTabke, dbAbsorber);
            chuseAbsorber.Fill(tableAbsorb);
            dbAbsorber.Close();
            return tableAbsorb;
        }
        public DataTable selectZIP(String typeAbsorber)
        {
            DataTable slectZIPtd = new DataTable();
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            String slectZIPFromTabke = @"SELECT * FROM [dbZIP] WHERE Type='"+typeAbsorber+"'";
            OleDbDataAdapter slectZIPdb = new OleDbDataAdapter(slectZIPFromTabke, dbAbsorber);
            slectZIPdb.Fill(slectZIPtd);
            dbAbsorber.Close();
            return slectZIPtd;
        }
        public Boolean checkAbsorbers(String absorberName)// Перевірка на наявність ГА в базі данних
        {
            Boolean realist = new Boolean();

            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            //    String selectFromTable = @"SELECT * FROM [dbAbsorbers] WHERE Serial  = '"+ absorberName + "' ";
            String selectFromTable = String.Format(@"SELECT * FROM [dbAbsorbers] WHERE Serial  = '{0}'", absorberName);

            OleDbCommand selectFrom = new OleDbCommand(selectFromTable, dbAbsorber);
            OleDbDataReader readData = selectFrom.ExecuteReader();
            realist=readData.Read();

            dbAbsorber.Close();

            return realist;
        }
        public void selectAbsorberFromDB(String absorberName)//Вибір параметрів вибраного амортизатора з бази даних 
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            String selectFromTable = @"SELECT * FROM [dbAbsorbers] WHERE Serial  = '"+absorberName+"' ";

            OleDbCommand selectFrom = new OleDbCommand(selectFromTable, dbAbsorber);
            OleDbDataReader readData = selectFrom.ExecuteReader();

            try
            {
                while (readData.Read())
                {
                    serial=readData.GetString(1);
                    type=readData.GetString(2);
                    block=readData.GetString(3);
                    separation=readData.GetString(4);
                    position=readData.GetString(5);
                    pSetup=readData.GetString(6);
                    rtm=readData.GetString(7);
                    So=readData.GetDouble(8);
                    Xo=readData.GetDouble(9);
                    Fx=readData.GetDouble(10);
                    L=readData.GetDouble(11);
                    Fh=readData.GetDouble(12);
                    Vmin=readData.GetDouble(13);
                    Vmax=readData.GetDouble(14);
                    Sxol=readData.GetDouble(15);
                    Sxolmin=readData.GetDouble(16);
                    Sxolmax=readData.GetDouble(17);
                    Vgidcost=readData.GetDouble(18);
                    Typegidcost=readData.GetString(19);
                    preasure=readData.GetDouble(20);
                    rate=readData.GetDouble(21);
                }
            }
            catch
            {
                //throw;
                System.Windows.Forms.MessageBox.Show("Выбран ГА с неполными данными! Просьба внесите изменения в параметрах ГА!", "ВНИМАНИЕ");
            }
            dbAbsorber.Close();


        }
        public OleDbDataReader absorberBase(String absorberName)
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            String selectFromTable = @"SELECT * FROM [dbAbsorbers] WHERE Serial  = '"+absorberName+"' ";

            OleDbCommand selectFrom = new OleDbCommand(selectFromTable, dbAbsorber);
            OleDbDataReader readData = selectFrom.ExecuteReader();
            dbAbsorber.Close();
            return readData;
        }
        public void selectAbsorberFromDB_RTM(String absorberName)//Вибір параметрів вибраного амортизатора з бази даних 
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            String selectFromTable = @"SELECT * FROM [dbAbsorbers] WHERE RTM  = '"+absorberName+"' ";

            OleDbCommand selectFrom = new OleDbCommand(selectFromTable, dbAbsorber);
            OleDbDataReader readData = selectFrom.ExecuteReader();

            try
            {
                while (readData.Read())
                {
                    serial=readData.GetString(1);
                    type=readData.GetString(2);
                    block=readData.GetString(3);
                    separation=readData.GetString(4);
                    position=readData.GetString(5);
                    pSetup=readData.GetString(6);
                    rtm=readData.GetString(7);
                    So=readData.GetDouble(8);
                    Xo=readData.GetDouble(9);
                    Fx=readData.GetDouble(10);
                    L=readData.GetDouble(11);
                    Fh=readData.GetDouble(12);
                    Vmin=readData.GetDouble(13);
                    Vmax=readData.GetDouble(14);
                    Sxol=readData.GetDouble(15);
                    Sxolmin=readData.GetDouble(16);
                    Sxolmax=readData.GetDouble(17);
                    Vgidcost=readData.GetDouble(18);
                    Typegidcost=readData.GetString(19);
                    preasure=readData.GetDouble(20);
                    rate=readData.GetDouble(21);
                }
            }
            catch
            {
                //throw;
                System.Windows.Forms.MessageBox.Show("Выбран ГА с неполными данными! Просьба внесите изменения в параметрах ГА!", "ВНИМАНИЕ");
            }
            dbAbsorber.Close();


        }
        public void selectAbsorberFromDBArchive(String absorberName)
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            String selectFromTable = @"SELECT * FROM [dbAbsorbers] WHERE Serial  = '"+absorberName+"' ";

            OleDbCommand selectFrom = new OleDbCommand(selectFromTable, dbAbsorber);
            OleDbDataReader readData = selectFrom.ExecuteReader();

            try
            {
                while (readData.Read())
                {
                    serialArch=readData.GetString(1);
                    typeArch=readData.GetString(2);
                    blockArch=readData.GetString(3);
                    separationArch=readData.GetString(4);
                    positionArch=readData.GetString(5);
                    pSetupArch=readData.GetString(6);
                    rtmArch=readData.GetString(7);
                    SoArch=readData.GetDouble(8);
                    XoArch=readData.GetDouble(9);
                    FxArch=readData.GetDouble(10);
                    LArch=readData.GetDouble(11);
                    FhArch=readData.GetDouble(12);
                    VminArch=readData.GetDouble(13);
                    VmaxArch=readData.GetDouble(14);
                    SxolArch=readData.GetDouble(15);
                    SxolminArch=readData.GetDouble(16);
                    SxolmaxArch=readData.GetDouble(17);
                    VgidcostArch=readData.GetDouble(18);
                    TypegidcostArch=readData.GetString(19);
                    preasureArch=readData.GetDouble(20);
                    rateArch=readData.GetDouble(21);
                }
            }
            catch
            {
                //throw;
                //System.Windows.Forms.MessageBox.Show("Выбран ГА с неполными данными! Просьба внесите изменения в параметрах ГА!", "ВНИМАНИЕ");
            }
            dbAbsorber.Close();


        }
        public void deleteAbsorbers(String serialNumber)
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            String deleteFromTable = @"DELETE * FROM [dbAbsorbers] WHERE Serial  = '"+serialNumber+"' ";

            OleDbCommand addAbsorbers = new OleDbCommand(deleteFromTable, dbAbsorber);
            addAbsorbers.ExecuteNonQuery();
            dbAbsorber.Close();

        }
        public void updateAbsorber(String updateFromTable)
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            // String comand = "UPDATE dbAbsorbers SET Type = 'qwerty',Block = 'asdf',Separation = 'sdsd',Posit = 'cxxcv',pSetup = 'sdsdv', RTM = 'asdazxc', So='"+1234+ "', Xo='" + 654 + "', Preasure='" + 159753 + "', Rate='" + 951357 + "' WHERE Serial = '311' ";
            OleDbCommand updateAbsorb = new OleDbCommand(updateFromTable, dbAbsorber);
            updateAbsorb.ExecuteNonQuery();
            dbAbsorber.Close();
        }
        //---------------------------------------------------------------------------------------------------------------------------------
        public void createTableUsers()//створення бази данних користувачів
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            OleDbCommand createTAbsorbers = new OleDbCommand("CREATE TABLE [dbUsers]([ID] Counter Primary Key,[userName] string, [pass] Int )", dbAbsorber);
            createTAbsorbers.ExecuteReader();
            dbAbsorber.Close();
        }
        public void addToTableUsers(String users, int password)//Добавлення користувачів в бзу данни
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            OleDbCommand addUsers = new OleDbCommand("INSERT INTO [dbUsers] (userName, pass) VALUES ('"+users+"', '"+password+"')", dbAbsorber);
            addUsers.ExecuteNonQuery();

            dbAbsorber.Close();
        }
        public DataTable selectUser() // Вибір користувача з бази даних
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();
            DataTable tableUsers = new DataTable();
            String selectFromUsers = @"SELECT * FROM [dbUsers]";

            OleDbDataAdapter chuseUser = new OleDbDataAdapter(selectFromUsers, dbAbsorber);
            chuseUser.Fill(tableUsers);
            dbAbsorber.Close();
            return tableUsers;
        }
        public void deleteUser(String users)
        {
            dbAbsorber=new OleDbConnection(dbAbsorberConnection);
            dbAbsorber.Open();

            String selectFromUsers = @"DELETE * FROM [dbUsers] WHERE userName  = '"+users+"'";
            OleDbCommand deleteUser = new OleDbCommand(selectFromUsers, dbAbsorber);
            deleteUser.ExecuteNonQuery();

            dbAbsorber.Close();
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public void createTableMeasure() //Создание таблиц для проведения измерения
        {

            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String createTableAllMeasure = "CREATE TABLE [dbAllMeasure]([ID] Counter Primary Key, [timeExp] Date, [Serial] String,[Fhcompress] Double,[Lcompress] Double, "+
                "[Fhstretching] Double, [Lstretching] Double,[Fxcompress] Double, [Fxstretching] Double,[Vcompress] Double,[Vstretching] Double,[Shold] Double, "+
                "[WorkOil] Double, [Protocol] int, [TestUser] String, [RTI] LONGTEXT, [changeMaslo] LONGTEXT, [Comentc] LONGTEXT, [Block] String, [Separation] String, [RTM] String)";

            String createTableNormalLoadCompress = "CREATE TABLE [dbMeasureNormalLoadCompress]([ID] Counter Primary Key,[timeExp] datetime,[Serial] String, [Fhcompress] Double, [Lcompress] Double)";
            String createTableNormalLoadStretching = "CREATE TABLE [dbMeasureNormalLoadStretching]([ID] Counter Primary Key,[timeExp] datetime,[Serial] String, [Fhstretching] Double, [Lstretching] Double)";

            String createTableIdleCompress = "CREATE TABLE [dbMeasureIdleCompress]([ID] Counter Primary Key,[timeExp] datetime,[Serial] String,[Fxcompress] Double)";
            String createTableIdleStretching = "CREATE TABLE [dbMeasureIdleStretching]([ID] Counter Primary Key,[timeExp] datetime,[Serial] String,[Fxstretching] Double)";

            String createTableSpeedCloseCompress = "CREATE TABLE [dbMeasureSpeedCloseCompress]([ID] Counter Primary Key,[timeExp] datetime,[Serial] String,[Vcompress] Double)";
            String createTableSpeedCloseStretching = "CREATE TABLE [dbMeasureSpeedCloseStretching]([ID] Counter Primary Key,[timeExp] datetime,[Serial] String,[Vstretching] Double)";

            OleDbCommand createTMeasureAllMeasure = new OleDbCommand(createTableAllMeasure, dbMeasure);
            createTMeasureAllMeasure.ExecuteNonQuery();

            OleDbCommand createTMeasureCompress = new OleDbCommand(createTableNormalLoadCompress, dbMeasure);
            createTMeasureCompress.ExecuteNonQuery();
            OleDbCommand createTMeasureStretching = new OleDbCommand(createTableNormalLoadStretching, dbMeasure);
            createTMeasureStretching.ExecuteNonQuery();

            OleDbCommand createTMeasureIdleCompress = new OleDbCommand(createTableIdleCompress, dbMeasure);
            createTMeasureIdleCompress.ExecuteNonQuery();
            OleDbCommand createTMeasureIdleStretching = new OleDbCommand(createTableIdleStretching, dbMeasure);
            createTMeasureIdleStretching.ExecuteNonQuery();

            OleDbCommand createTMeasureSpeedCloseCompress = new OleDbCommand(createTableSpeedCloseCompress, dbMeasure);
            createTMeasureSpeedCloseCompress.ExecuteNonQuery();
            OleDbCommand createTMeasureSpeedCloseStretching = new OleDbCommand(createTableSpeedCloseStretching, dbMeasure);
            createTMeasureSpeedCloseStretching.ExecuteNonQuery();

            dbMeasure.Close();
        }
        public void addAllMeasure()
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String allParameters = "'"+DateTime.Now.Date+"','"+serial+"','"+Fhcompress+"','"+Lcompress+"','"+Fhstretching+"','"+Lstretching+
                "','"+Fxcompress+"','"+Fxstretching+"','"+Vcompress+"','"+Vstretching+"','"+Shold+
                "','"+WorkOil+"','"+numberProtocol+"','"+user+"','"+ChangeRTI[255]+"','"+changeMaslo+"','"+Comentc+"','"+block+"','"+separation+"','"+rtm+"'";

            String resulToDB = "INSERT INTO [dbAllMeasure] (timeExp,Serial,Fhcompress,Lcompress,Fhstretching,Lstretching,Fxcompress,Fxstretching,Vcompress,Vstretching, "+
                "Shold, WorkOil, Protocol, TestUser, RTI, changeMaslo, Comentc, Block, Separation, RTM)VALUES ("+allParameters+")";

            OleDbCommand addAllMeasureToTable = new OleDbCommand(resulToDB, dbMeasure);
            addAllMeasureToTable.ExecuteNonQuery();
            dbMeasure.Close();
        }
        public void addMeasureNormalLoadCompress(DateTime timeMeasure, String SerialAbs, Double Fhcomp, Double Lcomp) // Занисение данных при испытании на смещение при нормальной нагрузке при Сжатии
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String addMesureTabNormalLoadCompress = "INSERT INTO [dbMeasureNormalLoadCompress] (timeExp, Serial,Fhcompress,Lcompress )VALUES ('"+timeMeasure+"', '"+SerialAbs+"',  '"+Fhcomp+"', '"+Lcomp+"')";

            OleDbCommand addMeasureNormalLoadCompress = new OleDbCommand(addMesureTabNormalLoadCompress, dbMeasure);
            addMeasureNormalLoadCompress.ExecuteNonQuery();
            dbMeasure.Close();

        }
        public void addMeasureNormalLoadStretching(DateTime timeMeasure, String SerialAbs, Double FhStretch, Double LStretch)// Занисение данных при испытании на смещение при нормальной нагрузке при Растяжении
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String addMesureTabNormalLoadStretching = "INSERT INTO [dbMeasureNormalLoadStretching] (timeExp, Serial,Fhcompress,Lcompress )VALUES ('"+timeMeasure+"', '"+SerialAbs+"',  '"+FhStretch+"', '"+LStretch+"')";

            OleDbCommand addMeasureNormalLoadStretching = new OleDbCommand(addMesureTabNormalLoadStretching, dbMeasure);
            addMeasureNormalLoadStretching.ExecuteNonQuery();
            dbMeasure.Close();

        }

        public void addMeasureIdleCompress(DateTime timeMeasure, String SerialAbs, Double Fxcomp)// Занисение данных при испытании на сопротивление холостого хода при Сжатии
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String addMesureTabIdleCompress = "INSERT INTO [dbMeasureIdleCompress] (timeExp, Serial,Fxcompress )VALUES ('"+timeMeasure+"', '"+SerialAbs+"', '"+Fxcomp+"')";

            OleDbCommand addMeasureIdleCompress = new OleDbCommand(addMesureTabIdleCompress, dbMeasure);
            addMeasureIdleCompress.ExecuteNonQuery();
            dbMeasure.Close();
        }
        public void addMeasureIdleStretching(DateTime timeMeasure, String SerialAbs, Double FxStretc)// Занисение данных при испытании на сопротивление холостого хода при Растяжении
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String addMesureTabIdleStretching = "INSERT INTO [dbMeasureIdleStretching] (timeExp, Serial,Fxstretching )VALUES ('"+timeMeasure+"', '"+SerialAbs+"', '"+FxStretc+"')";

            OleDbCommand addMeasureIdleStretching = new OleDbCommand(addMesureTabIdleStretching, dbMeasure);
            addMeasureIdleStretching.ExecuteNonQuery();
            dbMeasure.Close();
        }
        public void addMeasureSpeedCloseCompress(DateTime timeMeasure, String SerialAbs, Double Vcomp)// Занисение данных при испытании на скорость закрытия клапана при Сжатии
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();
            String addMesureTabSpeedCloseCompress = "INSERT INTO [dbMeasureSpeedCloseCompress] (timeExp, Serial,Vcompress )VALUES ('"+timeMeasure+"', '"+SerialAbs+"', '"+Vcomp+"')";
            //String addMesureTabSpeedCloseCompress = String.Format( "INSERT INTO [dbMeasureSpeedCloseCompress] (timeExp, Serial,Vcompress )VALUES ('{0}', '{1}', '{2}')", timeMeasure, SerialAbs, Vcomp);

            OleDbCommand addMeasureSpeedCloseCompress = new OleDbCommand(addMesureTabSpeedCloseCompress, dbMeasure);
            addMeasureSpeedCloseCompress.ExecuteNonQuery();
            dbMeasure.Close();
        }
        public void addMeasureSpeedCloseStretching(DateTime timeMeasure, String SerialAbs, Double Vstretch)// Занисение данных при испытании на сопротивление холостого хода при Растяжении
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String addMesureTabSpeedCloseStretching = "INSERT INTO [dbMeasureSpeedCloseStretching] (timeExp, Serial,Vstretching )VALUES ('"+timeMeasure+"', '"+SerialAbs+"', '"+Vstretch+"')";

            OleDbCommand addMeasureSpeedCloseStretching = new OleDbCommand(addMesureTabSpeedCloseStretching, dbMeasure);
            addMeasureSpeedCloseStretching.ExecuteNonQuery();
            dbMeasure.Close();
        }

        //----Методи для фільтарції в Архіві. дані беруться з комбобоксів
        public DataTable selectAllMeasureByDate(DateTime DateTimeStart, DateTime DateTimeEnd)
        {
            DataTable MeasureByDate = new DataTable();
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String selectAllMeasureByDateFromTable = "SELECT *FROM [dbAllMeasure] WHERE [timeExp] between @DateStart AND @DateEnd ";

            OleDbDataAdapter chuseAbsorberMeasureByDate = new OleDbDataAdapter(selectAllMeasureByDateFromTable, dbMeasure);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateStart", DateTimeStart);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateEnd", DateTimeEnd);

            chuseAbsorberMeasureByDate.Fill(MeasureByDate);
            dbMeasure.Close();
            return MeasureByDate;
        }
        public DataTable selectAllMeasureByDateSerial(DateTime DateTimeStart, DateTime DateTimeEnd, String serial)
        {
            DataTable MeasureByDate = new DataTable();
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String selectAllMeasureByDateFromTable = "SELECT *FROM [dbAllMeasure] WHERE [timeExp] between @DateStart AND @DateEnd AND [Serial] = '"+serial+"' ";

            OleDbDataAdapter chuseAbsorberMeasureByDate = new OleDbDataAdapter(selectAllMeasureByDateFromTable, dbMeasure);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateStart", DateTimeStart);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateEnd", DateTimeEnd);
            // chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@serial", serial);

            chuseAbsorberMeasureByDate.Fill(MeasureByDate);
            dbMeasure.Close();
            return MeasureByDate;
        }
        public DataTable selectAllMeasureByDateBlock(DateTime DateTimeStart, DateTime DateTimeEnd, String Block)
        {
            DataTable MeasureByDate = new DataTable();
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String selectAllMeasureByDateFromTable = "SELECT *FROM [dbAllMeasure] WHERE [timeExp] between @DateStart AND @DateEnd AND [Block] = '"+Block+"' ";

            OleDbDataAdapter chuseAbsorberMeasureByDate = new OleDbDataAdapter(selectAllMeasureByDateFromTable, dbMeasure);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateStart", DateTimeStart);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateEnd", DateTimeEnd);
            // chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@serial", serial);

            chuseAbsorberMeasureByDate.Fill(MeasureByDate);
            dbMeasure.Close();
            return MeasureByDate;
        }
        public DataTable selectAllMeasureByDateBlockSeparation(DateTime DateTimeStart, DateTime DateTimeEnd, String Block, String Separation)
        {
            DataTable MeasureByDate = new DataTable();
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String selectAllMeasureByDateFromTable = "SELECT *FROM [dbAllMeasure] WHERE [timeExp] between @DateStart AND @DateEnd AND [Block] = '"+Block+"' AND [Separation] = '"+Separation+"' ";

            OleDbDataAdapter chuseAbsorberMeasureByDate = new OleDbDataAdapter(selectAllMeasureByDateFromTable, dbMeasure);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateStart", DateTimeStart);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateEnd", DateTimeEnd);
            // chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@serial", serial);

            chuseAbsorberMeasureByDate.Fill(MeasureByDate);
            dbMeasure.Close();
            return MeasureByDate;
        }
        public DataTable selectAllMeasureByDateBlockSeparationRTM(DateTime DateTimeStart, DateTime DateTimeEnd, String Block, String Separation, String RTM)
        {
            DataTable MeasureByDate = new DataTable();
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String selectAllMeasureByDateFromTable = "SELECT *FROM [dbAllMeasure] WHERE [timeExp] between @DateStart AND @DateEnd AND [Block] = '"+Block+"' AND [Separation] = '"+Separation+"' AND [RTM] = '"+RTM+"'";

            OleDbDataAdapter chuseAbsorberMeasureByDate = new OleDbDataAdapter(selectAllMeasureByDateFromTable, dbMeasure);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateStart", DateTimeStart);
            chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@DateEnd", DateTimeEnd);
            // chuseAbsorberMeasureByDate.SelectCommand.Parameters.AddWithValue("@serial", serial);

            chuseAbsorberMeasureByDate.Fill(MeasureByDate);
            dbMeasure.Close();
            return MeasureByDate;
        }
        public void selectAllMeasureByProtocol(Int32 protocol)
        {
            dbMeasure=new OleDbConnection(dbMeasureConnection);
            dbMeasure.Open();

            String selectAllMeasureByProtocolFromTable = @"SELECT *FROM [dbAllMeasure] WHERE Protocol="+protocol+" ";

            OleDbCommand selectFromArchiv = new OleDbCommand(selectAllMeasureByProtocolFromTable, dbMeasure);
            OleDbDataReader readArchiv = selectFromArchiv.ExecuteReader();

            try
            {
                while (readArchiv.Read())
                {
                    timeExpArch=readArchiv.GetDateTime(1);
                    SerialArch=readArchiv.GetString(2);
                    FhcompressArch=readArchiv.GetDouble(3);
                    LcompressArch=readArchiv.GetDouble(4);
                    FhstretchingArch=readArchiv.GetDouble(5);
                    LstretchingArch=readArchiv.GetDouble(6);
                    FxcompressArch=readArchiv.GetDouble(7);
                    FxstretchingArch=readArchiv.GetDouble(8);
                    VcompressArch=readArchiv.GetDouble(9);
                    VstretchingArch=readArchiv.GetDouble(10);
                    SholdArch=readArchiv.GetDouble(11);
                    WorkOilArch=readArchiv.GetDouble(12);
                    ProtocolArch=readArchiv.GetInt32(13);
                    TestUserArch=readArchiv.GetString(14);
                    RTIArch=readArchiv.GetString(15);
                    changeMasloArch=readArchiv.GetString(16);
                    ComentcArch=readArchiv.GetString(17);
                }

                selectAbsorberFromDBArchive(SerialArch);
            }
            catch
            {
                //throw;
            }

            dbMeasure.Close();
            // return MeasureByProtocol;
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public void insertDataMeasureDB()
        {

        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public void savePritokol(/*string name*/)
        {
            Word.Application application = new Word.Application();

            application.Visible=true;

            Object missing = Type.Missing;

            application.Documents.Add();

            application.Selection.Font.Size=20;
            application.Selection.TypeText("Протокол испытания №:"+numberProtocol.ToString()+"\n");
            application.Selection.Font.Size=14;

            application.Selection.TypeText("Параметры испытуемого ГА \n");


            String[] param = { "Серийный №:", "Тип:", "Энергоблок:", "Отделение:", "Позиция:", "Место установки:", "РТМ:", "So,мм:", "Ход поршня Хо,мм:", "Допустимая нагрузка Fh, кгс:",
                "Допустимое смещение l, мм:","Допустимая сопротивление Fxx, кгс:", "Скорость закрытия V, см/c:", "Холодное полодение Sхол, мм:"};

            String[] paramAbsorb = { serial,type,block, separation, position, pSetup, rtm,So.ToString(),Xo.ToString(), Fh.ToString(),
            L.ToString(),Fx.ToString(),"от " + Vmin.ToString()+ " до "+Vmax.ToString(), "от"+Sxolmin.ToString()+" до "+Sxolmax.ToString() };
            /*
                        application.ActiveDocument.Tables.Add(application.Selection.Range, 14, 2, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitContent);
                        for (int i = 1; i<=param.Length; i++)
                        {
                            application.ActiveDocument.Tables[1].Cell(i, 1).Range.InsertAfter(param[i-1]);
                            application.ActiveDocument.Tables[1].Cell(i, 2).Range.InsertAfter(paramAbsorb[i-1]);
                        }
                        application.Selection.MoveDown(Word.WdUnits.wdLine, 16, null);
            */

            application.Selection.Font.Size=12;

            application.Selection.Tables.Add(application.Selection.Range, 1, 4, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[1].Range.Bold=1;
            application.Selection.Tables[1].Cell(1, 1).Range.Text="Тип ГА ";
            application.Selection.Tables[1].Cell(1, 2).Range.InsertAfter("Серийный № ");
            application.Selection.Tables[1].Cell(1, 3).Range.InsertAfter("Позиция ");
            application.Selection.Tables[1].Cell(1, 4).Range.InsertAfter("РТМ ");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 4, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[2].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[2].Range.Bold=0;
            application.Selection.Tables[1].Rows[2].Range.Italic=1;
            application.Selection.Tables[1].Cell(2, 1).Range.InsertAfter(type);
            application.Selection.Tables[1].Cell(2, 2).Range.InsertAfter(serial);
            application.Selection.Tables[1].Cell(2, 3).Range.InsertAfter(position);
            application.Selection.Tables[1].Cell(2, 4).Range.InsertAfter(rtm);

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[3].Range.Bold=1;
            application.Selection.Tables[1].Cell(3, 1).Range.Text="Энергоблок №";
            application.Selection.Tables[1].Cell(3, 2).Range.InsertAfter("Отделение");
            application.Selection.Tables[1].Cell(3, 3).Range.InsertAfter("Месторасположение");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[4].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[4].Range.Bold=0;
            application.Selection.Tables[1].Rows[4].Range.Italic=1;
            application.Selection.Tables[1].Cell(4, 1).Range.InsertAfter(block);
            application.Selection.Tables[1].Cell(4, 2).Range.InsertAfter(separation);
            application.Selection.Tables[1].Cell(4, 3).Range.InsertAfter(pSetup);

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[5].Range.Bold=1;
            application.Selection.Tables[1].Cell(5, 1).Range.Text="Выполнена замена:";
            application.Selection.Tables[1].Cell(5, 2).Range.InsertAfter("РТИ");
            application.Selection.Tables[1].Cell(5, 3).Range.InsertAfter("Рабочая жидкость");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[6].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[6].Range.Bold=0;
            application.Selection.Tables[1].Rows[6].Range.Italic=1;
            application.Selection.Tables[1].Cell(6, 2).Range.Text=ChangeRTI[255];
            application.Selection.Tables[1].Cell(6, 3).Range.InsertAfter(changeMaslo.ToString());

            application.Selection.MoveDown(Word.WdUnits.wdLine, 20, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[7].Range.Bold=1;
            application.Selection.Tables[1].Cell(7, 1).Range.Text="Стенд для испытания";
            application.Selection.Tables[1].Cell(7, 2).Range.InsertAfter("Режим испытаний");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[8].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[8].Range.Bold=0;
            application.Selection.Tables[1].Rows[8].Range.Italic=1;
            application.Selection.Tables[1].Cell(8, 1).Range.Text="STAS-60";
            application.Selection.Tables[1].Cell(8, 2).Range.InsertAfter("Автоматический");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[9].Range.Bold=1;
            application.Selection.Tables[1].Cell(9, 2).Range.InsertAfter("Данные испытаний:");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[10].Range.Bold=1;
            application.Selection.Tables[1].Cell(10, 1).Range.Text="Испытание";
            application.Selection.Tables[1].Cell(10, 2).Range.InsertAfter("Паспортные данные");
            application.Selection.Tables[1].Cell(10, 3).Range.InsertAfter("Полученные данные \n сжатие/растяжение");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 4, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;

            application.Selection.Tables[1].Rows[11].Range.Italic=1;
            application.Selection.Tables[1].Cell(11, 1).Range.Text="Номинальная нагрузка, кгс \n (Fн)";
            application.Selection.Tables[1].Cell(11, 2).Range.Text=Fh.ToString();
            application.Selection.Tables[1].Cell(11, 3).Range.Text=Fhcompress.ToString()+"/"+Fhstretching.ToString();
            application.Selection.Tables[1].Cell(12, 1).Range.Text="Смещение поршня, мм \n (δ)";
            application.Selection.Tables[1].Cell(12, 2).Range.Text="≤"+L.ToString();
            application.Selection.Tables[1].Cell(12, 3).Range.Text=Lcompress.ToString()+"/"+Lstretching.ToString();
            application.Selection.Tables[1].Cell(13, 1).Range.Text="Скорость закрытия  \n клапана, см/с (V)";
            application.Selection.Tables[1].Cell(13, 2).Range.Text="от "+Vmin.ToString()+" до "+Vmax.ToString();
            application.Selection.Tables[1].Cell(13, 3).Range.Text=Vcompress.ToString()+" / "+Vstretching.ToString();
            application.Selection.Tables[1].Cell(14, 1).Range.Text="Нагрузка холостого хода,  \n кгс (Fхх)";
            application.Selection.Tables[1].Cell(14, 2).Range.Text="≤"+Fx.ToString();
            application.Selection.Tables[1].Cell(14, 3).Range.Text=Fxcompress.ToString()+" / "+Fxstretching.ToString();

            application.Selection.MoveDown(Word.WdUnits.wdLine, 14, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[15].Range.Bold=1;
            application.Selection.Tables[1].Cell(15, 1).Range.Text="Дата";
            application.Selection.Tables[1].Cell(15, 2).Range.InsertAfter("Испытатель");
            application.Selection.Tables[1].Cell(15, 3).Range.InsertAfter("Подпись");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[16].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[16].Range.Bold=0;
            application.Selection.Tables[1].Rows[16].Range.Italic=1;
            application.Selection.Tables[1].Cell(16, 1).Range.Text=timeExp.ToString("dd.MM.yyyy");
            application.Selection.Tables[1].Cell(16, 2).Range.InsertAfter(user);

            application.Selection.Tables[1].Range.ParagraphFormat.Alignment=Word.WdParagraphAlignment.wdAlignParagraphCenter;

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Font.Size=14;
            application.Selection.Font.Bold=1;
            application.Selection.Range.Text="Комментарии к испытанию: \n";

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);
            application.Selection.Font.Bold=0;
            application.Selection.TypeText(Comentc);

            //            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            /*
                        application.Selection.Font.Bold=1;
                        application.Selection.TypeText("\n");
                        application.Selection.TypeText("График. Нагрузка сжатия, кгс; Смещение сжатия, мм");
                        application.Selection.InlineShapes.AddPicture(@"d:\\chart1.bmp", ref missing, ref missing, ref missing);
                        application.Selection.TypeText("\t \n");

                        application.Selection.TypeText("График. Нагрузка растяжения, кгс; Смещение растяжения, мм");
                        application.Selection.InlineShapes.AddPicture(@"d:\\chart2.bmp", ref missing, ref missing, ref missing);
                        application.Selection.TypeText("\t \n");
               */
            /*
            
                        application.ActiveDocument.Tables.Add(application.Selection.Range, 1, 4, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
                        application.ActiveDocument.Tables[3].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
                        application.ActiveDocument.Tables[3].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
                        application.ActiveDocument.Tables[3].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
                        application.ActiveDocument.Tables[3].Cell(3, 1).Range.InsertAfter("Выполнена замена:\n");
                        application.ActiveDocument.Tables[3].Cell(3, 2).Range.InsertAfter("Выполнена замена:");
                        application.ActiveDocument.Tables[3].Cell(3, 3).Range.InsertAfter("Рабочая жидкость \n");
                        application.ActiveDocument.Tables[3].Cell(3, 2).Range.InsertAfter(ChangeRTI[255]);
                        application.ActiveDocument.Tables[3].Cell(3, 3).Range.InsertAfter(Typegidcost);
                        application.Selection.MoveDown(Word.WdUnits.wdLine, 4, null);


                        application.ActiveDocument.Tables.Add(application.Selection.Range, 1, 1, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
                        application.ActiveDocument.Tables[3].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
                        application.ActiveDocument.Tables[3].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
                        application.ActiveDocument.Tables[3].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
                        application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

                        */

            application.Documents.Save();

        }
        public void saveProtokolFromArchiv(/*string name, Bitmap bm*/)
        {

            /*
                        Word.Application application = new Word.Application();

                        application.Visible=true;

                        Object missing = Type.Missing;

                        application.Documents.Add();

                        application.Selection.Font.Size = 20;
                        application.Selection.TypeText("Протокол испытания №:"+ProtocolArch.ToString()+"\n");
                        application.Selection.Font.Size=14;

                        application.Selection.TypeText("Параметры испытуемого ГА №:"+SerialArch.ToString()+"\n");


                        String[] param = { "Серийный №:", "Тип:", "Энергоблок:", "Отделение:", "Позиция:", "Место установки:", "РТМ:", "So,мм:", "Ход поршня Хо,мм:", "Допустимая нагрузка Fh, кгс:",
                            "Допустимое смещение l, мм:","Допустимая сопротивление Fxx, кгс:", "Скорость закрытия V, см/c:", "Холодное полодение Sхол, мм:"};

                        String[] paramAbsorb = { SerialArch,typeArch,blockArch, separationArch, positionArch, pSetupArch, rtmArch,SoArch.ToString(),XoArch.ToString(), FhArch.ToString(),
                        LArch.ToString(),FxArch.ToString(),"от " + VminArch.ToString()+ " до "+VmaxArch.ToString(), "от"+SxolminArch.ToString()+" до "+SxolmaxArch.ToString() };

                        application.ActiveDocument.Tables.Add(application.Selection.Range,14,2,Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitContent);
                        for (int i=1; i<= param.Length; i++)
                        {
                            application.ActiveDocument.Tables[1].Cell(i, 1).Range.InsertAfter(param[i-1]);
                            application.ActiveDocument.Tables[1].Cell(i, 2).Range.InsertAfter(paramAbsorb[i-1]);
                        }


                        application.Selection.MoveDown(Word.WdUnits.wdLine, 15, null);
                        application.Selection.TypeText("\n");
                        application.Selection.Font.Size=16;
                        application.Selection.TypeText("Результаты испытания ГА №:"+SerialArch.ToString()+"\n");
                        application.Selection.Font.Size=14;
                        application.ActiveDocument.Tables.Add(application.Selection.Range, 14, 2, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitContent);


                        String[] resulParaeters = {"Время испытания", "Нагрузка сжатия Fh, кгс:", "Смещение сжатия L, мм:", "Нагрузка растяжения Fh, кгс:", "Смещение растяжени L, мм:",
                        "Сопротивление сжатия Fхх, кгс:","Сопротивление растяжения Fхх, кгс:", "Скорость закрытия сжатия, см/с:", "Скорость закрытия растяжения, см/с:","Испытатель" };

                        String[] resulMeasure = { timeExpArch.ToString("dd.MM.yyyy"), FhcompressArch.ToString(), LcompressArch.ToString(), FhstretchingArch.ToString(), LstretchingArch.ToString(),
                        FxcompressArch.ToString(), FxstretchingArch.ToString(), VcompressArch.ToString(), VstretchingArch.ToString(), TestUserArch};

                        for (int i = 1; i<=resulParaeters.Length; i++)
                        {
                            application.ActiveDocument.Tables[2].Cell(i, 1).Range.InsertAfter(resulParaeters[i-1]);
                            application.ActiveDocument.Tables[2].Cell(i, 2).Range.InsertAfter(resulMeasure[i-1]);
                        }

                        application.Selection.MoveDown(Word.WdUnits.wdLine, 15, null);
                        application.Selection.TypeText("\n");

                        application.Selection.Font.Size=14;
                        application.Selection.TypeText(RTIArch);
                        application.Selection.TypeText("\n");

                        application.Selection.Font.Size=16;
                        application.Selection.TypeText("Комментарии к испытанию:\t");
                        application.Selection.Font.Size=14;

                        application.Selection.TypeText("\n");
                        application.Selection.TypeText("График. Нагрузка сжатия, кгс; Смещение сжатия, мм");
                        application.Selection.InlineShapes.AddPicture(@"d:\\chart1.bmp", ref missing, ref missing, ref missing);
                        application.Selection.TypeText("\t \n");
                        */

            Word.Application application = new Word.Application();

            application.Visible=true;

            Object missing = Type.Missing;

            application.Documents.Add();

            application.Selection.Font.Size=16;
            application.Selection.TypeText("Протокол испытания №:"+ProtocolArch.ToString()+"\n");
            application.Selection.Font.Size=14;

            application.Selection.TypeText("Параметры испытуемого ГА \n");

            application.Selection.Font.Size=12;

            application.Selection.Tables.Add(application.Selection.Range, 1, 4, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[1].Range.Bold=1;
            application.Selection.Tables[1].Cell(1, 1).Range.Text="Тип ГА ";
            application.Selection.Tables[1].Cell(1, 2).Range.InsertAfter("Серийный № ");
            application.Selection.Tables[1].Cell(1, 3).Range.InsertAfter("Позиция ");
            application.Selection.Tables[1].Cell(1, 4).Range.InsertAfter("РТМ ");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 4, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[2].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[2].Range.Bold=0;
            application.Selection.Tables[1].Rows[2].Range.Italic=1;
            application.Selection.Tables[1].Cell(2, 1).Range.InsertAfter(typeArch);
            application.Selection.Tables[1].Cell(2, 2).Range.InsertAfter(serialArch);
            application.Selection.Tables[1].Cell(2, 3).Range.InsertAfter(positionArch);
            application.Selection.Tables[1].Cell(2, 4).Range.InsertAfter(rtmArch);

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[3].Range.Bold=1;
            application.Selection.Tables[1].Cell(3, 1).Range.Text="Энергоблок №";
            application.Selection.Tables[1].Cell(3, 2).Range.InsertAfter("Отделение");
            application.Selection.Tables[1].Cell(3, 3).Range.InsertAfter("Месторасположение");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[4].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[4].Range.Bold=0;
            application.Selection.Tables[1].Rows[4].Range.Italic=1;
            application.Selection.Tables[1].Cell(4, 1).Range.InsertAfter(blockArch);
            application.Selection.Tables[1].Cell(4, 2).Range.InsertAfter(separationArch);
            application.Selection.Tables[1].Cell(4, 3).Range.InsertAfter(pSetupArch);

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[5].Range.Bold=1;
            application.Selection.Tables[1].Cell(5, 1).Range.Text="Выполнена замена:";
            application.Selection.Tables[1].Cell(5, 2).Range.InsertAfter("РТИ");
            application.Selection.Tables[1].Cell(5, 3).Range.InsertAfter("Рабочая жидкость");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[6].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[6].Range.Bold=0;
            application.Selection.Tables[1].Rows[6].Range.Italic=1;
            application.Selection.Tables[1].Cell(6, 2).Range.Text=RTIArch;
            application.Selection.Tables[1].Cell(6, 3).Range.InsertAfter(changeMasloArch.ToString());

            application.Selection.MoveDown(Word.WdUnits.wdLine, 20, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[7].Range.Bold=1;
            application.Selection.Tables[1].Cell(7, 1).Range.Text="Стенд для испытания";
            application.Selection.Tables[1].Cell(7, 2).Range.InsertAfter("Режим испытаний");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[8].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[8].Range.Bold=0;
            application.Selection.Tables[1].Rows[8].Range.Italic=1;
            application.Selection.Tables[1].Cell(8, 1).Range.Text="STAS-60";
            application.Selection.Tables[1].Cell(8, 2).Range.InsertAfter("Автоматический");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[9].Range.Bold=1;
            application.Selection.Tables[1].Cell(9, 2).Range.InsertAfter("Данные испытаний:");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[10].Range.Bold=1;
            application.Selection.Tables[1].Cell(10, 1).Range.Text="Испытание";
            application.Selection.Tables[1].Cell(10, 2).Range.InsertAfter("Паспортные данные");
            application.Selection.Tables[1].Cell(10, 3).Range.InsertAfter("Полученные данные \n сжатие/растяжение");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            application.Selection.Tables.Add(application.Selection.Range, 4, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;

            application.Selection.Tables[1].Rows[11].Range.Italic=1;
            application.Selection.Tables[1].Cell(11, 1).Range.Text="Номинальная нагрузка, кгс \n (Fн)";
            application.Selection.Tables[1].Cell(11, 2).Range.Text=FhArch.ToString();
            application.Selection.Tables[1].Cell(11, 3).Range.Text=FhcompressArch.ToString()+"/"+FhstretchingArch.ToString();
            application.Selection.Tables[1].Cell(12, 1).Range.Text="Смещение поршня, мм \n (δ)";
            application.Selection.Tables[1].Cell(12, 2).Range.Text="≤"+LArch.ToString();
            application.Selection.Tables[1].Cell(12, 3).Range.Text=LcompressArch.ToString()+"/"+LstretchingArch.ToString();
            application.Selection.Tables[1].Cell(13, 1).Range.Text="Скорость закрытия  \n клапана, см/с (V)";
            application.Selection.Tables[1].Cell(13, 2).Range.Text="от "+VminArch.ToString()+" до "+VmaxArch.ToString();
            application.Selection.Tables[1].Cell(13, 3).Range.Text=VcompressArch.ToString()+" / "+VstretchingArch.ToString();
            application.Selection.Tables[1].Cell(14, 1).Range.Text="Нагрузка холостого хода,  \n кгс (Fхх)";
            application.Selection.Tables[1].Cell(14, 2).Range.Text="≤"+FxArch.ToString();
            application.Selection.Tables[1].Cell(14, 3).Range.Text=FxcompressArch.ToString()+" / "+FxstretchingArch.ToString();

            application.Selection.MoveDown(Word.WdUnits.wdLine, 14, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[15].Range.Bold=1;
            application.Selection.Tables[1].Cell(15, 1).Range.Text="Дата";
            application.Selection.Tables[1].Cell(15, 2).Range.InsertAfter("Испытатель");
            application.Selection.Tables[1].Cell(15, 3).Range.InsertAfter("Подпись");

            application.Selection.MoveDown(Word.WdUnits.wdLine, 2, null);

            application.Selection.Tables.Add(application.Selection.Range, 1, 3, Word.WdDefaultTableBehavior.wdWord9TableBehavior, Word.WdAutoFitBehavior.wdAutoFitWindow);
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[16].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleNone;
            application.Selection.Tables[1].Rows[16].Range.Bold=0;
            application.Selection.Tables[1].Rows[16].Range.Italic=1;
            application.Selection.Tables[1].Cell(16, 1).Range.Text=timeExpArch.ToString("dd.MM.yyyy");
            application.Selection.Tables[1].Cell(16, 2).Range.InsertAfter(TestUserArch);

            application.Selection.Tables[1].Range.ParagraphFormat.Alignment=Word.WdParagraphAlignment.wdAlignParagraphCenter;


            application.Selection.MoveDown(Word.WdUnits.wdLine, 2, null);

            application.Selection.Font.Size=14;
            application.Selection.Font.Bold=1;
            //application.Selection.Range.Text="Комментарии к испытанию:";
            application.Selection.TypeText("Комментарии к испытанию: ");

            //application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);
            application.Selection.Font.Bold=0;
            //application.Selection.Range.Text=ComentcArch;
            application.Selection.TypeText(ComentcArch);

            //  application.Selection.MoveDown(Word.WdUnits.wdLine, 1, null);

            /*
            application.Selection.Font.Bold=1;
            application.Selection.TypeText("\n");
            application.Selection.TypeText("График. Нагрузка сжатия, кгс; Смещение сжатия, мм");
            application.Selection.InlineShapes.AddPicture(@"d:\\chart1.bmp", ref missing, ref missing, ref missing);
            application.Selection.TypeText("\t \n");

            application.Selection.TypeText("График. Нагрузка растяжения, кгс; Смещение растяжения, мм");
            application.Selection.InlineShapes.AddPicture(@"d:\\chart2.bmp", ref missing, ref missing, ref missing);
            application.Selection.TypeText("\t \n");
            */
            application.Documents.Save();
        }
        public void saveDataGgridViewToWord(DataGridView DGV, string filename)
        {
            if (DGV.Rows.Count!=0)
            {
                int RowCount = DGV.Rows.Count;
                int ColumnCount = DGV.Columns.Count;
                Object[,] DataArray = new object[RowCount, ColumnCount+1];

                //add rows
                int r = 0;
                for (int c = 0; c<=ColumnCount-1; c++)
                {
                    for (r=0; r<RowCount-1; r++)
                    {
                        DataArray[r, c]=DGV.Rows[r].Cells[c].Value;
                    } //end row loop
                } //end column loop

                Word.Document oDoc = new Word.Document();
                oDoc.Application.Visible=true;

                //page orintation
                //oDoc.PageSetup.Orientation=Word.WdOrientation.wdOrientLandscape;

                oDoc.Content.Application.Selection.Range.Font.Name="Times New Roman";
                dynamic oRange = oDoc.Content.Application.Selection.Range;
                string oTemp = "";
                for (r=0; r<RowCount-1; r++)
                {
                    for (int c = 0; c<=ColumnCount-1; c++)
                    {
                        oTemp=oTemp+DataArray[r, c]+"\t";
                    }
                }

                //table format
                oRange.Text=oTemp;

                object Separator = Word.WdTableFieldSeparator.wdSeparateByTabs;
                object ApplyBorders = true;
                object AutoFit = true;
                object AutoFitBehavior = Word.WdAutoFitBehavior.wdAutoFitContent;

                oRange.ConvertToTable(ref Separator, ref RowCount, ref ColumnCount,
                                      Type.Missing, Type.Missing, ref ApplyBorders,
                                      Type.Missing, Type.Missing, Type.Missing,
                                      Type.Missing, Type.Missing, Type.Missing,
                                      Type.Missing, ref AutoFit, ref AutoFitBehavior, Type.Missing);

                oRange.Select();
                
                oDoc.Application.Selection.Tables[1].Select();
                oDoc.Application.Selection.Tables[1].Rows.AllowBreakAcrossPages=0;
                oDoc.Application.Selection.Tables[1].Rows.Alignment=0;
                oDoc.Application.Selection.Tables[1].Range.Font.Name="Times New Roman";
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.InsertRowsAbove(1);
                oDoc.Application.Selection.Tables[1].Rows[1].Select();

                //header row style
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Bold=0;
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Name="Times New Roman";
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Size=10;

                //add header row manually
                for (int c = 0; c<=ColumnCount-1; c++)
                {
                    oDoc.Application.Selection.Tables[1].Cell(1, c+1).Range.Text=DGV.Columns[c].HeaderText;
                }

                //table style 
                //oDoc.Application.Selection.Tables[1].set_Style("Grid Table 4 - Accent 5");
                oDoc.Application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderBottom].LineStyle=Word.WdLineStyle.wdLineStyleSingle;
                oDoc.Application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderTop].LineStyle=Word.WdLineStyle.wdLineStyleSingle;
                oDoc.Application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderVertical].LineStyle=Word.WdLineStyle.wdLineStyleSingle;
                oDoc.Application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderHorizontal].LineStyle=Word.WdLineStyle.wdLineStyleSingle;
                oDoc.Application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderLeft].LineStyle=Word.WdLineStyle.wdLineStyleSingle;
                oDoc.Application.Selection.Tables[1].Borders[Word.WdBorderType.wdBorderRight].LineStyle=Word.WdLineStyle.wdLineStyleSingle;
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.Cells.VerticalAlignment=Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                //header text
                foreach (Word.Section section in oDoc.Application.ActiveDocument.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, Word.WdFieldType.wdFieldPage);
                    //  headerRange.Text="your header text";
                    headerRange.Font.Size=10;
                    headerRange.Font.Name="TimesNewRoman";
                    headerRange.ParagraphFormat.Alignment=Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                oDoc.SaveAs2(filename);

            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public void changeRTI(Boolean teflonCylinder, Boolean rubberCylinder, Boolean teflonStok, Boolean rubberStok)
        {
            Cylinder=Convert.ToInt32(teflonCylinder)+2*Convert.ToInt32(rubberCylinder);
            Stok=Convert.ToInt32(teflonStok)+2*Convert.ToInt32(rubberStok);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------
        public Master MBmaster;
        //private byte[] data;

        public void conntectToPLC()
        {
            try
            {
                // Create new modbus master and add event functions
                MBmaster=new Master(Properties.Settings.Default.IPAdress, Properties.Settings.Default.Port);
                // MBmaster.OnResponseData += new Master.ResponseData(MBmaster_OnResponseData);
                MBmaster.OnException+=new Master.ExceptionData(MBmaster_OnException);
            }
            catch (SystemException error)
            {
                MessageBox.Show(error.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ------------------------------------------------------------------------
        // Event for response data
        // ------------------------------------------------------------------------
        /*     private void MBmaster_OnResponseData(ushort ID, byte unit, byte function, byte[] values)
        {
            // ------------------------------------------------------------------
            // Seperate calling threads
        //    if (this.InvokeRequired)
         //   {
         //       this.BeginInvoke(new Master.ResponseData(MBmaster_OnResponseData), new object[] { ID, unit, function, values });
          //      return;
          //  }

            // ------------------------------------------------------------------------
            // Identify requested data
            switch (ID)
            {
                case 1:
                    // grpData.Text = "Read coils";
                    data = values;
                    //  ShowAs(null, null);
                    break;
                case 2:
                    //grpData.Text = "Read discrete inputs";
                    data = values;
                    //ShowAs(null, null);
                    break;
                case 3:
                    // grpData.Text = "Read holding register";
                    data = values;
                    // ShowAs(null, null);
                    break;
                case 4:
                    // grpData.Text = "Read input register";
                    data = values;
                    //  ShowAs(null, null);
                    break;
                case 5:
                    // grpData.Text = "Write single coil";
                    break;
                case 6:
                    // grpData.Text = "Write multiple coils";
                    break;
                case 7:
                    // grpData.Text = "Write single register";
                    break;
                case 8:
                    // grpData.Text = "Write multiple register";
                    break;
            }
        }
*/
        // ------------------------------------------------------------------------
        // Modbus TCP slave exception
        // ------------------------------------------------------------------------
        private void MBmaster_OnException(ushort id, byte unit, byte function, byte exception)
        {
            string exc = "Modbus says error: ";
            switch (exception)
            {
                case Master.excIllegalFunction: exc+="Illegal function!"; break;
                case Master.excIllegalDataAdr: exc+="Illegal data adress!"; break;
                case Master.excIllegalDataVal: exc+="Illegal data value!"; break;
                case Master.excSlaveDeviceFailure: exc+="Slave device failure!"; break;
                case Master.excAck: exc+="Acknoledge!"; break;
                case Master.excGatePathUnavailable: exc+="Gateway path unavailbale!"; break;
                case Master.excExceptionTimeout: exc+="Slave timed out!"; break;
                case Master.excExceptionConnectionLost: exc+="Connection is lost!"; break;
                case Master.excExceptionNotConnected: exc+="Not connected!"; break;
            }

            MessageBox.Show(exc, "Modbus slave exception");
        }
        public Double[] ModBusTCP_ReadHold_Registers(Byte unite, ushort StartAddress, ushort numInputs)
        {
            Double[] HoldReg = new double[512];
            byte[] data_ReadHold_Registers = new byte[1024];
            try
            {
                MBmaster.ReadHoldingRegister(3, unite, StartAddress, numInputs, ref data_ReadHold_Registers);

                for (int i = 0; i<data_ReadHold_Registers.Length; i=i+2)
                {
                    HoldReg[i/2]=data_ReadHold_Registers[i]*256+data_ReadHold_Registers[i+1];
                }
            }
            catch
            {
                return HoldReg;
            }

            return HoldReg;
        }
        public void ModBusTCP_WriteSingleHold_Registers(Byte unit, ushort StartAddress, UInt16 dataSent)
        {
            byte[] dataSentTo = new byte[512];
            dataSentTo[0]=BitConverter.GetBytes(dataSent)[1];
            dataSentTo[1]=BitConverter.GetBytes(dataSent)[0];
            MBmaster.WriteSingleRegister(7, unit, StartAddress, dataSentTo);
        }

    }
}
