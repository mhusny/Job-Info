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
using System.Net;
using System.IO;

namespace JobInfo
{
    public partial class WHView : Form
    {
        public static string connectionString = "Data Source=umserver;Initial Catalog=dbKelaniya_new;User ID=sage;Password=FGT%35;";
        SqlConnection con = new SqlConnection(connectionString);
        SqlCommand cmd = new SqlCommand();

        class CellData
        {
            
            public DateTime Time;
        }

        //private Thread _dataInputThread = null;
        //private Thread _gridBlinkThread = null;
        
        private List<CellData> _blinkData = null;
            
        public WHView()
        {
            InitializeComponent();
            _blinkData = new List<CellData>();
        }

        private void WHView_Load(object sender, EventArgs e)
        {

            //change pending to processing
            //add customr name
            //slow blink frequency
            //dont blink in WH


            cmd.Connection = con;
            timer1.Start();

            //blink rows
            //_dataInputThread = new Thread(new ThreadStart(DataInputThreadFunc));
            //_gridBlinkThread = new Thread(new ThreadStart(GridBlinkThreadFunc));

            //_dataInputThread.IsBackground = true;
            //_gridBlinkThread.IsBackground = true;

            //_dataInputThread.Start();
            //_gridBlinkThread.Start();
        }

        private void fillData()
        {


            string queryString = " SET dateformat dmy SELECT   Distinct   _btblJCMaster.cJobCode AS JobCode, _btblJCMaster.cFinalInvoiceNo AS InvoiceNo, Client.Account,  COUNT(_btblJCTxLines.idJCTxLines) AS [No of Items], " +
            " CASE WHEN min(_btblJCTxLines.cDescription) like '*%' THEN 'ISSUED' WHEN min(_btblJCTxLines.cDescription) not like '*%'  THEN 'PENDING' END AS [Item Status], " +
            " CASE WHEN _btblJCMaster.iStatus = 1 THEN 'ACTIVE' WHEN _btblJCMaster.iStatus = 2  THEN 'COMPLETE' END AS [Job Status], " +
            " _btblJCMaster.IdJCMaster, _btblJCMaster.iStatus " +
            " FROM         _btblJCMaster INNER JOIN " +
            " _btblJCTxLines ON _btblJCMaster.IdJCMaster = _btblJCTxLines.iJCMasterID INNER JOIN " +
            " Client ON _btblJCMaster.iClientId = Client.DCLink " +
            " WHERE     (_btblJCMaster.dStartDate > '28/3/2016') AND _btblJCMaster.iStatus <> 3 " +
            " GROUP BY _btblJCMaster.cJobCode, _btblJCMaster.cFinalInvoiceNo, _btblJCTxLines.idJCTxLines, Client.Account, _btblJCMaster.IdJCMaster, _btblJCMaster.iStatus " +
            " ORDER BY [Item Status] desc, _btblJCMaster.cFinalInvoiceNo desc ";

            SqlDataAdapter adapter = new SqlDataAdapter(queryString, con);

            DataSet joblist = new DataSet();
            adapter.Fill(joblist, "UG1");

            con.Close();

            UG1.DataSource = joblist.Tables[0];
            UG1.Columns["IdJCMaster"].Visible = false;
            UG1.Columns["No of Items"].Visible = false;
            UG1.Columns["iStatus"].Visible = false;
            UG1.Rows[0].Cells[0].Selected = false;

            formatGrid();
        }

        public void formatGrid()
        {
            //UG1.DefaultCellStyle.Font = new Font("Tahoma", 20);
            //foreach (DataGridViewRow row in UG1.Rows)
            //{
            //    row.Height = 60;
            //    row.Cells[0].Style.BackColor = Color.Green;
            //}


            foreach (DataGridViewRow DR in UG1.Rows)
            {
                DR.Cells[0].Style.BackColor = UG1.Columns[0].DefaultCellStyle.BackColor;
                DR.Cells[1].Style.BackColor = UG1.Columns[1].DefaultCellStyle.BackColor;
                DR.Cells[2].Style.BackColor = UG1.Columns[2].DefaultCellStyle.BackColor;
                DR.Cells[3].Style.BackColor = UG1.Columns[3].DefaultCellStyle.BackColor;
                DR.Cells[4].Style.BackColor = UG1.Columns[4].DefaultCellStyle.BackColor;
            }


            foreach (DataGridViewRow DR in UG1.Rows)
            {
                if (true)
                {
                    if (DR.Cells[4].Value.ToString() != "ISSUED")
                    {
                        DR.Cells[0].Style.BackColor = Color.Yellow;
                        DR.Cells[1].Style.BackColor = Color.Yellow;
                        DR.Cells[2].Style.BackColor = Color.Yellow;
                        DR.Cells[3].Style.BackColor = Color.Yellow;
                        DR.Cells[4].Style.BackColor = Color.Yellow;
                    }
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
                            if (DR.Cells[4].Value.ToString() != "ISSUED")
                            {
                                DR.Cells[0].Style.BackColor = Color.Yellow;
                                DR.Cells[1].Style.BackColor = Color.Yellow;
                                DR.Cells[2].Style.BackColor = Color.Yellow;
                                DR.Cells[3].Style.BackColor = Color.Yellow;
                                DR.Cells[4].Style.BackColor = Color.Yellow;
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
                                DR.Cells[0].Style.BackColor = UG1.Columns[0].DefaultCellStyle.BackColor;
                                DR.Cells[1].Style.BackColor = UG1.Columns[1].DefaultCellStyle.BackColor;
                                DR.Cells[2].Style.BackColor = UG1.Columns[2].DefaultCellStyle.BackColor;
                                DR.Cells[3].Style.BackColor = UG1.Columns[3].DefaultCellStyle.BackColor;
                                DR.Cells[4].Style.BackColor = UG1.Columns[4].DefaultCellStyle.BackColor;
                            }
                        });

                        lock (_blinkData)
                        {
                            _blinkData.Remove(data);
                        }
                    }
                }

                Thread.Sleep(250); // Blink frequency
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (UG1.SelectedCells.Count > 0)
            {
                int selectedrowindex = UG1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = UG1.Rows[selectedrowindex];
                string a = Convert.ToString(selectedRow.Cells["JobCode"].Value);

                if ( Convert.ToInt32(selectedRow.Cells["iStatus"].Value) > 1)
                {
                    //string queryString = " UPDATE _btblJCMaster SET cFinalCheck = 'OK' WHERE cJobCode = '" + a + "' ";
                    string queryString = " UPDATE _btblJCTxLines SET dEndDate = GETDATE(), cDescription = '*' + cDescription WHERE iJCMasterID = " + selectedRow.Cells["IdJCMaster"].Value + " AND SUBSTRING(cDescription,1,1) <> '*' ";

                    con.Open();
                    cmd.CommandText = queryString;
                    cmd.ExecuteNonQuery();
                    con.Close();

                    fillData();
                }
            }
             
        }

        private void sendSMS()
        {
            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            string baseurl = "https://cpsolutions.dialog.lk/index.php/cbs/sms/send?destination=94779962198&q=14181869886758&message=hi";
            Stream data = client.OpenRead(baseurl);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            fillData();
        }

        private void UG1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void UG1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            JobItems jobitems = new JobItems();
            jobitems.UG1.DataSource = null;

            int selectedrowindex = UG1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = UG1.Rows[selectedrowindex];
            string queryString = " SELECT cDescription FROM _btblJCTxLines WHERE iJCMasterID = " + selectedRow.Cells["IdJCMaster"].Value + " ";

            SqlDataAdapter adapter = new SqlDataAdapter(queryString, con);

            DataSet jobitem = new DataSet();
            adapter.Fill(jobitem, "UG1");

            con.Close();

            jobitems.UG1.DataSource = jobitem.Tables[0];
            jobitems.Show();
        }
    }
}
