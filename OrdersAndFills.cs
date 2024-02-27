using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using tt.messaging.order.enums;
using tt_net_sdk;
//using System.Timers;
using System.Text;
namespace AlgoProject101
{
    internal class OrdersAndFills
    {

        TradeSubscription tsubs;
        private Timer timer;
        //Dictionary<ulong, Dictionary<string, Account>> orderMap = new Dictionary<ulong, Dictionary<string, Account>>();
        Dictionary<string, Order> orderMap = new Dictionary<string, Order>();
        DataTable Orders;
        DataTable SummaryTable;
        DataTable Algo_Table;
        DataTable Account_Table;
        public OrdersAndFills(Dispatcher disp, DataGridView LiveOrders, DataGridView Order_Data, DataGridView Algo_Orders, DataGridView Account_Data)
        {

            tsubs = new TradeSubscription(disp);
            tsubs.OrderBookDownload += new EventHandler<OrderBookDownloadEventArgs>(tsubs_OrderBookDownload);
            tsubs.OrderAdded += new EventHandler<OrderAddedEventArgs>(tsubs_OrderAdded);
            tsubs.OrderDeleted += new EventHandler<OrderDeletedEventArgs>(tsubs_OrderDeleted);
            tsubs.OrderFilled += new EventHandler<OrderFilledEventArgs>(tsubs_OrderFilled);
            tsubs.OrderRejected += new EventHandler<OrderRejectedEventArgs>(tsubs_OrderRejected);
            tsubs.OrderUpdated += new EventHandler<OrderUpdatedEventArgs>(tsubs_OrderUpdated);
            tsubs.OrderStatusUnknown += new EventHandler<OrderStatusUnknownEventArgs>(tsubs_OrderStatusUnknown);
            tsubs.OrderTimeout += new EventHandler<OrderTimeoutEventArgs>(tsubs_OrderTimeout);
            Order_Data.CellClick +=(sender,e)=> Order_Data_CellClick(sender,e,Order_Data,LiveOrders,Account_Data);
            Account_Data.CellClick += (sender, e) => Account_Data_CellClick(sender, e, Account_Data, LiveOrders);
            InitializeTimer();
            Orders = new DataTable();
            BindOrder_Data(Order_Data);
            BindAlgo_Orders(Algo_Orders);
            BindLiveOrders(LiveOrders);
            BindAccount_Data(Account_Data);
            tsubs.Start();
        }
        void Account_Data_CellClick(object sender, DataGridViewCellEventArgs e,DataGridView Account_Data,DataGridView LiveOrders)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = Account_Data.Rows[e.RowIndex];
                string account = row.Cells["Account"].Value.ToString();
                if ((LiveOrders.DataSource as DataTable).DefaultView.RowFilter == $"Account='{account}'")
                {
                    (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = "";
                }
                else
                    (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = $"Account='{account}'";
            }
        }
        void Order_Data_CellClick(object sender,DataGridViewCellEventArgs e,DataGridView Order_Data,DataGridView LiveOrders,DataGridView Account_Data)
        {
            if(e.RowIndex >= 0)
            {
                DataGridViewRow row = Order_Data.Rows[e.RowIndex];
                string connectionId = row.Cells["ConnectionID"].Value.ToString();
                if ((LiveOrders.DataSource as DataTable).DefaultView.RowFilter == $"Connection_Id='{connectionId}'")
                {
                    ShowAllData(LiveOrders,Account_Data);
                }
                else
                {
                    (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = $"Connection_Id='{connectionId}'";
                    (Account_Data.DataSource as DataTable).DefaultView.RowFilter = $"ConnectionId='{connectionId}'";
                }
            }
        }
        void ShowAllData(DataGridView LiveOrders,DataGridView Account_Data)
        {
            (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = "";
            (Account_Data.DataSource as DataTable).DefaultView.RowFilter = "";

        }
        void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 5 * 60 * 1000;
            timer.Tick += TimerElapse;
            timer.Start();
        }
        void TimerElapse(object sender, EventArgs e)
        {
            WriteDataTableToCsv("C:\\Users\\mohit.parwani\\Order_Summary.csv", SummaryTable);
            WriteDataTableToCsv("C:\\Users\\mohit.parwani\\AlgoSummary.csv", Algo_Table);
        }
        void WriteDataTableToCsv(string filepath, DataTable dt)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                if (!File.Exists(filepath))
                {
                    sb.Append("TimeStamp" + ",");
                    foreach (DataColumn col in dt.Columns)
                    {
                        sb.Append(col.ColumnName + ",");
                    }
                    sb.AppendLine();
                }
                foreach (DataRow row in dt.Rows)
                {
                    sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",");
                    foreach (object item in row.ItemArray)
                    {
                        sb.Append(item.ToString() + ",");
                    }
                    sb.AppendLine();
                }
                if (File.Exists(filepath))
                {
                    File.AppendAllText(filepath, sb.ToString());
                }
                else
                {
                    File.WriteAllText(filepath, sb.ToString());
                }
                Console.WriteLine($"Data from DataTable '{dt.TableName}' appended to '{filepath}'.");


            }
        }
        void BindAccount_Data(DataGridView Account_Data)
        {
            Account_Table= new DataTable();
            Account_Table.Columns.Add("ConnectionId");
            Account_Table.Columns.Add("Account");
            Account_Table.Columns.Add("NumberOfOrders");
            Account_Table.PrimaryKey=new DataColumn[] { Account_Table.Columns["ConnectionId"], Account_Table.Columns["Account"] };
            Account_Data.DataSource= Account_Table;
            

        }
        void BindAlgo_Orders(DataGridView Algo_Orders)
        {
            Algo_Table = new DataTable();
            Algo_Table.Columns.Add("Algo_Name");
            Algo_Table.Columns.Add("Status");
            Algo_Table.Columns.Add("NumberofOrders");
            Algo_Table.PrimaryKey = new DataColumn[] { Algo_Table.Columns["Algo_Name"], Algo_Table.Columns["Status"] };
            Algo_Orders.DataSource = Algo_Table;


        }
        void BindOrder_Data(DataGridView Order_Data)
        {
            SummaryTable = new DataTable();
            SummaryTable.Columns.Add("ConnectionID", typeof(string));
            SummaryTable.Columns.Add("NumberofOrders", typeof(string));
            SummaryTable.Columns.Add("NumberOfUniqueAccounts", typeof(string));
            SummaryTable.PrimaryKey = new DataColumn[] { SummaryTable.Columns["ConnectionID"] };
            //Order_Data.DataSource = null;
            Order_Data.DataSource = SummaryTable;
            Order_Data.Columns["ConnectionID"].DataPropertyName = "ConnectionID";
            Order_Data.Columns["NumberofOrders"].DataPropertyName = "NumberofOrders";
            Order_Data.Columns["NumberOfUniqueAccounts"].DataPropertyName = "NumberOfUniqueAccounts";
            //Console.WriteLine("me called");

            //Order_Data.Rows.Clear();


            Order_Data.Refresh();
        }
        void BindLiveOrders(DataGridView LiveOrders)
        {
            Orders.Columns.Add("SiteOrderKey", typeof(string));
            Orders.Columns.Add("Connection_Id", typeof(string));
            Orders.Columns.Add("Instrument", typeof(string));
            Orders.Columns.Add("Exchange", typeof(string));
            Orders.Columns.Add("OrderPrice", typeof(string));
            Orders.Columns.Add("OrderSide", typeof(string));
            Orders.Columns.Add("OrderQuantity", typeof(string));
            Orders.Columns.Add("Account", typeof(string));
            Orders.PrimaryKey = new DataColumn[] { Orders.Columns["SiteOrderKey"] };
            LiveOrders.DataSource = null;
            LiveOrders.DataSource = Orders;
            LiveOrders.Columns["Connection_Id"].DataPropertyName = "Connection_Id";
            LiveOrders.Columns["Instrument"].DataPropertyName = "Instrument";
            LiveOrders.Columns["Exchange"].DataPropertyName = "Exchange";
            LiveOrders.Columns["OrderPrice"].DataPropertyName = "OrderPrice";
            LiveOrders.Columns["OrderSide"].DataPropertyName = "OrderSide";
            LiveOrders.Columns["OrderQty"].DataPropertyName = "OrderQuantity";
            LiveOrders.Columns["Account"].DataPropertyName = "Account";

        }
        void UpdateOrder_Data()
        {
            SummaryTable.Clear();
            var groupedOrders = orderMap.Values.GroupBy(ord => ord.ConnectionId).Select(group => new
            {
                ConnectionId = group.Key,
                NumberOfOrders = group.Count(),
                NumberOfUniqueAccouts = group.Select(order => order.Account).Distinct().Count()
            });
            foreach (var item in groupedOrders)
            {
                SummaryTable.Rows.Add(item.ConnectionId, item.NumberOfOrders, item.NumberOfUniqueAccouts);
            }
        }
        void DeleteFromAlgo_TableandAccount_Table(Order ord)
        {
            DataRow AccountRowtoUpdate=Account_Table.Rows.Find(new object[] {ord.ConnectionId,ord.Account});
            if(AccountRowtoUpdate != null)
            {
                int entry = Convert.ToInt32(AccountRowtoUpdate["NumberofOrders"]);
                if (entry > 1)
                {
                    AccountRowtoUpdate["NumberofOrders"] = entry - 1;
                }
                else
                    Account_Table.Rows.Remove(AccountRowtoUpdate);
            }
            if (ord.SyntheticStatus != tt_net_sdk.SynthStatus.NotSet)
                Console.WriteLine(ord.SyntheticStatus + " " + ord.SyntheticType);
            if (ord.Algo != null)
            {
                DataRow AlgorowToUpdate = Algo_Table.Rows.Find(new object[] { ord.Algo.Alias, ord.SyntheticStatus });
                if (AlgorowToUpdate != null)
                {
                    int entry = Convert.ToInt32(AlgorowToUpdate["NumberofOrders"]);
                    if (entry == 1)
                    {
                        Algo_Table.Rows.Remove(AlgorowToUpdate);
                    }
                    else
                        AlgorowToUpdate["NumberofOrders"] = entry - 1;
                }

            }
        }
        void AddtoAlgo_TableandAccount_Table(Order ord)
        { 
            DataRow AccountrowToUpdate=Account_Table.Rows.Find(new object[] {ord.ConnectionId,ord.Account});
            if (AccountrowToUpdate != null)
            {
                AccountrowToUpdate["NumberofOrders"] = Convert.ToInt32(AccountrowToUpdate["NumberofOrders"]) + 1;
            }
            else
            {
                Account_Table.Rows.Add(ord.ConnectionId,ord.Account,"1");
            }
            if (ord.SyntheticStatus != tt_net_sdk.SynthStatus.NotSet)
                Console.WriteLine(ord.SyntheticStatus + " " + ord.SyntheticType);
            if (ord.Algo != null)
            {
                DataRow AlgorowToUpdate = Algo_Table.Rows.Find(new object[] { ord.Algo.Alias, ord.SyntheticStatus });
                if (AlgorowToUpdate != null)
                {
                    AlgorowToUpdate["NumberofOrders"] = Convert.ToInt32(AlgorowToUpdate["NumberofOrders"]) + 1;
                }
                else
                {
                    Algo_Table.Rows.Add(ord.Algo.Alias, ord.SyntheticStatus, "1");
                }

            }
        }
        void tsubs_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            Console.WriteLine("downloading order book");
            //Console.WriteLine(e.Orders);
            foreach (Order ord in e.Orders)
            {
                Console.WriteLine(ord);
                //Console.WriteLine("acct={0,-15}\tinstrument={3,-30}\tlimit price={1,-15}\tworking qty={2,-15}\tconnectionId={4,-15}", ord.Account, ord.LimitPrice, ord.WorkingQuantity, ord.InstrumentKey, ord.ConnectionId);
                Orders.Rows.Add(ord.SiteOrderKey, ord.ConnectionId, ord.InstrumentKey.Alias, ord.InstrumentKey.MarketId, ord.LimitPrice, ord.Side, ord.WorkingQuantity, ord.Account);
                orderMap.Add(ord.SiteOrderKey, ord);
                AddtoAlgo_TableandAccount_Table(ord);
            }
            UpdateOrder_Data();
            WriteDataTableToCsv("C:\\Users\\mohit.parwani\\Order_Summary.csv", SummaryTable);
            WriteDataTableToCsv("C:\\Users\\mohit.parwani\\AlgoSummary.csv", Algo_Table);
        }

        void tsubs_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            //Console.WriteLine("acct={0,-15}\tinstrument={3,-30}\tlimit price={1,-15}\tworking qty={2,-15}\tconnectionId={4,-15}", e.Order.Account, e.Order.LimitPrice, e.Order.WorkingQuantity, e.Order.InstrumentKey, e.Order.ConnectionId);
            Console.WriteLine("added={0}", e.ToString());
            Orders.Rows.Add(e.Order.SiteOrderKey, e.Order.ConnectionId, e.Order.InstrumentKey.Alias, e.Order.InstrumentKey.MarketId, e.Order.LimitPrice, e.Order.Side, e.Order.WorkingQuantity, e.Order.Account);
            orderMap.Add(e.Order.SiteOrderKey, e.Order);
            UpdateOrder_Data();
            AddtoAlgo_TableandAccount_Table(e.Order);
        }
        void tsubs_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            Console.WriteLine("deleted={0}", e.ToString());
            //string key = e.DeletedUpdate.SiteOrderKey;
            //Console.WriteLine(orderMap.ContainsKey(key) ? "true" : "false");
            Orders.Rows.Remove(Orders.Rows.Find(e.DeletedUpdate.SiteOrderKey));
            orderMap.Remove(e.DeletedUpdate.SiteOrderKey);
            DeleteFromAlgo_TableandAccount_Table(e.DeletedUpdate);
            UpdateOrder_Data();
        }
        void tsubs_OrderFilled(object sender, OrderFilledEventArgs e)
        {
            Console.WriteLine("filled={0}", e.ToString());
            Orders.Rows.Remove(Orders.Rows.Find(e.OldOrder.SiteOrderKey));
            orderMap.Remove(e.OldOrder.SiteOrderKey);
            DeleteFromAlgo_TableandAccount_Table(e.OldOrder);
            Orders.Rows.Add(e.NewOrder.SiteOrderKey, e.NewOrder.ConnectionId, e.NewOrder.InstrumentKey.Alias, e.NewOrder.InstrumentKey.MarketId, e.NewOrder.LimitPrice, e.NewOrder.Side, e.NewOrder.WorkingQuantity, e.NewOrder.Account);
            orderMap.Add(e.NewOrder.SiteOrderKey, e.NewOrder);
            UpdateOrder_Data();
            AddtoAlgo_TableandAccount_Table(e.NewOrder);
        }
        void tsubs_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            Orders.Rows.Remove(Orders.Rows.Find(e.Order.SiteOrderKey));
            orderMap.Remove(e.Order.SiteOrderKey);
            Console.WriteLine("rejected={0}", e.ToString());
            UpdateOrder_Data();
            DeleteFromAlgo_TableandAccount_Table(e.Order);
        }
        void tsubs_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            Console.WriteLine("updated={0}", e.ToString());
            Orders.Rows.Remove(Orders.Rows.Find(e.OldOrder.SiteOrderKey));
            orderMap.Remove(e.OldOrder.SiteOrderKey);
            DeleteFromAlgo_TableandAccount_Table(e.OldOrder);
            Orders.Rows.Add(e.NewOrder.SiteOrderKey, e.NewOrder.ConnectionId, e.NewOrder.InstrumentKey.Alias, e.NewOrder.InstrumentKey.MarketId, e.NewOrder.LimitPrice, e.NewOrder.Side, e.NewOrder.WorkingQuantity, e.NewOrder.Account);
            orderMap.Add(e.NewOrder.SiteOrderKey, e.NewOrder);
            UpdateOrder_Data();
            AddtoAlgo_TableandAccount_Table(e.NewOrder);
        }
        void tsubs_OrderStatusUnknown(object sender, OrderStatusUnknownEventArgs e)
        {
            Console.WriteLine("unknown={0}", e.ToString());
        }
        void tsubs_OrderTimeout(object sender, OrderTimeoutEventArgs e)
        {
            Orders.Rows.Remove(Orders.Rows.Find(e.Order.SiteOrderKey));
            orderMap.Remove(e.Order.SiteOrderKey);
            Console.WriteLine("timeout={0}", e.ToString());
            UpdateOrder_Data();
            DeleteFromAlgo_TableandAccount_Table(e.Order);
        }
    }
}

