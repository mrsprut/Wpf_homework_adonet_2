using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Xml.Linq;

namespace Wpf_homework_adonet_2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataTable my_data;
        private SqlDataAdapter my_adapter;
        private SqlConnection my_conn;
        private XDocument my_xml;

        public MainWindow()
        {
            InitializeComponent();
            my_conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString);
        }

        private void Run_qr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCommand my_select_comand;
                my_select_comand = my_conn.CreateCommand();
                my_select_comand.CommandText = TB_input.Text;
                my_data = new DataTable();
                my_adapter = new SqlDataAdapter();
                my_adapter.SelectCommand = my_select_comand;
                my_adapter.Fill(my_data);
                DG_panel.ItemsSource = my_data.AsDataView();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (my_data != null)
            {
                my_data.Dispose();
            }
            if (my_adapter != null)
            {
                my_adapter.Dispose();
            }
            if (my_conn != null)
            {
                my_conn.Dispose();
            }
            base.OnClosed(e);
        }

        private void BTN_getXML_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand get_xml = my_conn.CreateCommand();
            get_xml.CommandType = CommandType.StoredProcedure;
            get_xml.CommandText = "Get_XML_proc";
            get_xml.Parameters.Add("@Date_begin", SqlDbType.Date).Value = TB_Date_beg.Text;
            if (TB_Date_end.Text.Length == 0)
            {
                DateTime curr_date = DateTime.Now;
                get_xml.Parameters.Add("@Date_end", SqlDbType.Date).Value = curr_date.Date;
            }
            else
            {
                get_xml.Parameters.Add("@Date_end", SqlDbType.Date).Value = TB_Date_end.Text;
            }
            get_xml.Parameters.Add("@Output_par", SqlDbType.NVarChar, 6000).Direction = ParameterDirection.Output;
            my_xml = new XDocument();
            try
            {
                my_conn.Open();
                get_xml.ExecuteNonQuery();
                my_xml = XDocument.Parse((String)get_xml.Parameters["@Output_par"].Value);
                my_xml.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sales_report.xml"));
                //my_conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                my_conn.Close();
            }
        }

        private void BT_DG_to_xml_Click(object sender, RoutedEventArgs e)
        {
            if (DG_panel.Items.Count >0)
            {
                DataSet my_dataset = new DataSet();
                my_dataset.Tables.Add(my_data);
                my_dataset.WriteXml(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DataGrid_table.xml"));
            }
        }
    }
}