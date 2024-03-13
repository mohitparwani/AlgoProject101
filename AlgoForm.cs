using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;
using System.IO;
using AlgoProject101;
using WebSocketSharp.Server;
using WebSocketSharp;


namespace AlgoProject1
{
   
    public partial class AlgoForm : Form
    {
        OrdersAndFills oaf;
        //private Timer timer;
        //private string connectionString;
        WebSocketServer wssv;
        private TTAPI n_api = null;

        private bool n_isShutDown = false, n_shutdownInProcess = false;
        //private tt_net_sdk.WorkerDispatcher n_disp = null;
        public AlgoForm()
        {
            InitializeComponent();
            //InitializeTimer();
        }
        private void InitializeTimer()
        {
            
        }
        private void AlgoForm_Load(object sender, EventArgs e)
        {
            this.FormClosed += NewApp_FormClosed;
        }
        private void NewApp_FormClosed(object sender, FormClosedEventArgs e)
        {
            shutdownTTAPI();
            
        }

        public void ttNetApiInitHandler(TTAPI api, ApiCreationException e)
        {
            if (e == null)
            {
                n_api = api;
                n_api.TTAPIStatusUpdate += new EventHandler<TTAPIStatusUpdateEventArgs>(n_api_TTAPIStatusUpdate);
                wssv = new WebSocketServer("ws://10.136.25.45:5678");
                n_api.Start();
                wssv.Start();
            }
            else if (e.IsRecoverable)
            {

            }
            else
            {
                MessageBox.Show("Api Initialization failed:" + e.Message);
            }
        }
        private void n_api_TTAPIStatusUpdate(object sender, TTAPIStatusUpdateEventArgs e)
        {
            if (e.IsReady)
            {
                //MessageBox.Show("API Connection Established.", "Success");
                Console.WriteLine("Connection Established");
                initInstruments();
            }
            else if (e.IsDown)
            {
                MessageBox.Show("API Connection Down (Reconnecting...)", "Warning");
            }
            else
            {
                MessageBox.Show(String.Format("M_TTAPI_TTAPIStatusUpdate:{0}", e));
            }
        }
        public void shutdownTTAPI()
        {
            if (!n_shutdownInProcess)
            {
                TTAPI.ShutdownCompleted += new EventHandler(TTAPI_ShutdownCompleted);
                TTAPI.Shutdown();
                n_shutdownInProcess = true;
            }
        }

        public void TTAPI_ShutdownCompleted(object sender, EventArgs e)
        {
            n_isShutDown = true;
            wssv.Stop();
            Close();
        }

        private void Order_Data_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Algo_Orders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void initInstruments()
        {
            
            //CustomInstrument zw = new CustomInstrument(MarketId.CME, ProductType.Future, "CL",Order_Data);
            oaf = new OrdersAndFills(tt_net_sdk.Dispatcher.Current, LiveOrders,Order_Data,Algo_Orders,Account_Data,Connection_Message_Data,Account_Message_Data,wssv);
        }
    }
    
    //public class CustomInstrument
    //{
    //    Instrument instrument;
    //    InstrumentKey ik;
    //    InstrumentCatalog instr_cat;
    //    Product product;
    //    Price ltp;
    //    Price bidPrice;
    //    Price askPrice;
    //    Quantity bidQty;
    //    Quantity askQty;
    //    OrderProfile op;
    //    string contractIdentifier;
    //    PriceSubscriptionWorking prsubs;
    //    public CustomInstrument(MarketId mid, ProductType protyp, string prodName,ListBox Order_Data)
    //    {
    //        //ListBox Order_Data;
    //        //instr_cat = new InstrumentCatalog(mid, protyp, prodName, tt_net_sdk.Dispatcher.Current);
    //        //instr_cat.OnData += new EventHandler<InstrumentCatalogEventArgs>(instr_cat_OnData);
    //        //instr_cat.GetAsync();
    //        Console.WriteLine("custom instrument formed");
    //        OrdersAndFills oaf = new OrdersAndFills(tt_net_sdk.Dispatcher.Current);
            
    //        //FetchingSigleInstrument fsb = new FetchingSigleInstrument(tt_net_sdk.Dispatcher.Current, MarketId.CME, ProductType.Future, "CL", "CL Mar24");
    //    }

    //    private void instr_cat_OnData(object sender, InstrumentCatalogEventArgs e)
    //    {
    //        if (e.Event == ProductDataEvent.Found)
    //        {
    //            //string path = @"C:\Users\mohit.parwani";
    //            //Console.WriteLine(path);
    //            //string filePath = Path.Combine(path, "instrumentsxx.csv");
    //            //Console.WriteLine(filePath);
    //            //string outputFormat = "<CSV>";
    //            //StringBuilder csvData = new StringBuilder();
    //            //csvData.AppendLine("Sr.No., Instrumets");
    //            //int srn = 1;
    //            foreach (KeyValuePair<InstrumentKey, Instrument> kvp in e.InstrumentCatalog.Instruments)
    //            {
    //                //prsubs=new PriceSubscriptionWorking(tt_net_sdk.Dispatcher.Current,kvp.Key.ToString(),ProductType.Future);
    //                //Console.WriteLine(csvData);
    //                //string csvRow = string.Format("{0},{1}", srn, kvp.Key);
    //                //csvData.AppendLine(csvRow);
    //                Console.WriteLine("key={0} ", kvp.Key);
    //                Console.WriteLine("Value={0}", kvp.Value);
    //                //srn++;
    //            }
    //            //File.WriteAllText(filePath,csvData.ToString());
    //            //Console.WriteLine("jsflksjf");
    //        }
    //        else
    //        {
    //            Console.WriteLine("Cannot find instrument:{0}", e.Message);
    //        }
    //    }
    //}
}