using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace JobInfo
{
    public partial class JobItems : Form
    {
        public static string connectionString = "Data Source=umserver;Initial Catalog=dbKelaniya_new;User ID=sa;Password=Vx@7190;";
        SqlConnection con = new SqlConnection(connectionString);
        SqlCommand cmd = new SqlCommand();

        public JobItems()
        {
            InitializeComponent();
        }

        private void JobItems_Load(object sender, EventArgs e)
        {

        }
    }
}
