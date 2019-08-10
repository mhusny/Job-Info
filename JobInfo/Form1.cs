using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;

namespace JobInfo
{
    public partial class Form1 : Form
    {
        class CellData
        {
           
            public DateTime Time;
        }

        private Thread _dataInputThread = null;
        private Thread _gridBlinkThread = null;
        private List<CellData> _blinkData = null;

        //public static DataGridViewRow DR;
        public Form1()
        {
            InitializeComponent();

            _blinkData = new List<CellData>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //WHView whv = new WHView();
            //whv.Show();

            timer1.Start();


            //blink rows
            _dataInputThread = new Thread(new ThreadStart(DataInputThreadFunc));
            _gridBlinkThread = new Thread(new ThreadStart(GridBlinkThreadFunc));

            _dataInputThread.IsBackground = true;
            _gridBlinkThread.IsBackground = true;

            _dataInputThread.Start();
            _gridBlinkThread.Start();
           
        }

        private void fillData()
        {
            string connectionString = "Data Source=umserver;Initial Catalog=dbKelaniya_new;User ID=sa;Password=Vx@7190;";
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            string queryString = " SET dateformat dmy SELECT   Distinct   _btblJCMaster.cJobCode AS JobCode, _btblJCMaster.cFinalInvoiceNo AS InvoiceNo, Client.Account,  LTRIM(Client.Title +' '+ Client.Name) AS Name, COUNT(_btblJCTxLines.cDescription) AS [No of Items], " +
            " CASE WHEN min(_btblJCTxLines.cDescription) like '*%' THEN 'ISSUED' WHEN min(_btblJCTxLines.cDescription) not like '*%'  THEN 'PENDING' END AS [Item Status], " +
            " CASE WHEN _btblJCMaster.iStatus = 1 THEN 'Work In Progress' WHEN  _btblJCMaster.iStatus = 2 THEN 'COMPLETED' END AS [Job Status] " +
            " FROM         _btblJCMaster INNER JOIN " +
            " _btblJCTxLines ON _btblJCMaster.IdJCMaster = _btblJCTxLines.iJCMasterID INNER JOIN " +
            " Client ON _btblJCMaster.iClientId = Client.DCLink " +
            " WHERE     (_btblJCMaster.dStartDate > '28/3/2016') AND _btblJCMaster.iStatus <> 3 " +
            " GROUP BY _btblJCMaster.cJobCode, _btblJCMaster.cFinalInvoiceNo, _btblJCTxLines.idJCTxLines, Client.Account, Client.Title, Name, _btblJCMaster.iStatus " +
            " ORDER BY [Item Status] desc, _btblJCMaster.cFinalInvoiceNo desc, [Job Status] desc ";


            //DateTime.Now.ToString("dd/MM/yyyy") + "') current date

            SqlDataAdapter adapter = new SqlDataAdapter(queryString, con);

            DataSet joblist = new DataSet();
            adapter.Fill(joblist, "UG1");

            con.Close();

            UG1.DataSource = joblist.Tables[0];
            UG1.Columns["No of Items"].Visible = false;
            UG1.Rows[0].Cells[0].Selected = false;
        }

        public void formatGrid()
        {
            UG1.DefaultCellStyle.Font = new Font("Tahoma", 16);
            foreach (DataGridViewRow row in UG1.Rows)
            {
                row.Height = 35;
                if (row.Cells[5].Value.ToString() == "ISSUED")
                {
                    row.Cells[0].Style.BackColor = Color.MediumSpringGreen;
                    row.Cells[1].Style.BackColor = Color.MediumSpringGreen;
                    row.Cells[2].Style.BackColor = Color.MediumSpringGreen;
                    row.Cells[3].Style.BackColor = Color.MediumSpringGreen;
                    row.Cells[4].Style.BackColor = Color.MediumSpringGreen;
                    row.Cells[5].Style.BackColor = Color.MediumSpringGreen;
                    row.Cells[6].Style.BackColor = Color.MediumSpringGreen;
                }
            }
        }

        private void DataInputThreadFunc()
        {
            while (true)
            {
                if (UG1.IsDisposed)
                    break;



                CellData data = new CellData();
                
                data.Time = DateTime.Now;



                    UG1.Invoke((MethodInvoker)delegate()
                    {
                        //UG1.Rows[data.Row].Cells[data.Col].Value = value;
                        foreach (DataGridViewRow DR in UG1.Rows)
                        {
                            if (true)
                            {
                                if (DR.Cells[5].Value.ToString() != "ISSUED" )
                                {
                                    DR.Cells[0].Style.BackColor = Color.Yellow;
                                    DR.Cells[1].Style.BackColor = Color.Yellow;
                                    DR.Cells[2].Style.BackColor = Color.Yellow;
                                    DR.Cells[3].Style.BackColor = Color.Yellow;
                                    DR.Cells[4].Style.BackColor = Color.Yellow;
                                    DR.Cells[5].Style.BackColor = Color.Yellow;
                                    DR.Cells[6].Style.BackColor = Color.Yellow;
                                }
                            }
                        }
                    });

                    lock (_blinkData)
                    {
                        _blinkData.Add(data);
                    }

                    Thread.Sleep(2000);
                }
            
        }

        private void GridBlinkThreadFunc()
        {
            while (true)
            {
                // Make a copy to avoid invalid operation exception while iterating through the map
                List<CellData> tempBlinkData;
                lock (_blinkData)
                {
                    tempBlinkData = new List<CellData>(_blinkData);
                }

                foreach (CellData data in tempBlinkData)
                {
                    TimeSpan elapsed = DateTime.Now - data.Time;
                    if (elapsed.TotalMilliseconds > 500) // 500 is the Blink delay
                    {
                        if (UG1.IsDisposed)
                            return;

                        UG1.Invoke((MethodInvoker)delegate()
                        {
                            foreach (DataGridViewRow DR in UG1.Rows)
                            {
                                if (DR.Cells[5].Value.ToString() != "ISSUED")
                                {
                                    DR.Cells[0].Style.BackColor = UG1.Columns[0].DefaultCellStyle.BackColor;
                                    DR.Cells[1].Style.BackColor = UG1.Columns[1].DefaultCellStyle.BackColor;
                                    DR.Cells[2].Style.BackColor = UG1.Columns[2].DefaultCellStyle.BackColor;
                                    DR.Cells[3].Style.BackColor = UG1.Columns[3].DefaultCellStyle.BackColor;
                                    DR.Cells[4].Style.BackColor = UG1.Columns[4].DefaultCellStyle.BackColor;
                                    DR.Cells[5].Style.BackColor = UG1.Columns[5].DefaultCellStyle.BackColor;
                                    DR.Cells[6].Style.BackColor = UG1.Columns[6].DefaultCellStyle.BackColor;
                                }
                            }
                        });

                        lock (_blinkData)
                        {
                            _blinkData.Remove(data);
                        }
                    }
                }

                Thread.Sleep(1000); // Blink frequency
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            fillData();
            formatGrid();
        }
    }
}
