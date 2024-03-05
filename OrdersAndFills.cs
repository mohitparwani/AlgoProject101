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
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace AlgoProject101
{
    internal class OrdersAndFills
    {
        private readonly object _locker = new object();
        private readonly object sm_locker = new object();
        TradeSubscription tsubs;
        private Timer timer;
        HashSet<string> uniqueFills;
        //Dictionary<ulong, Dictionary<string, Account>> orderMap = new Dictionary<ulong, Dictionary<string, Account>>();
        //Dictionary<string, Order> orderMap = new Dictionary<string, Order>();
        DataTable Orders;
        DataTable SummaryTable;
        DataTable Algo_Table;
        DataTable Account_Table;
        DataTable Connection_Message_Table;
        DataTable Account_Message_Table;
        public OrdersAndFills(Dispatcher disp, DataGridView LiveOrders, DataGridView Order_Data, DataGridView Algo_Orders, DataGridView Account_Data, DataGridView Connection_Message_Data, DataGridView Account_Message_Data)
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
            Order_Data.CellClick += (sender, e) => Order_Data_CellClick(sender, e, Order_Data, LiveOrders, Account_Data);
            Account_Data.CellClick += (sender, e) => Account_Data_CellClick(sender, e, Account_Data, LiveOrders);
            uniqueFills = new HashSet<string>();
            BindOrder_Data(Order_Data);
            BindAlgo_Orders(Algo_Orders);
            BindLiveOrders(LiveOrders);
            BindAccount_Data(Account_Data);
            BindMessage_Table(Connection_Message_Data, Account_Message_Data);
            tsubs.Start();
        }
        void Account_Data_CellClick(object sender, DataGridViewCellEventArgs e, DataGridView Account_Data, DataGridView LiveOrders)
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
        void Order_Data_CellClick(object sender, DataGridViewCellEventArgs e, DataGridView Order_Data, DataGridView LiveOrders, DataGridView Account_Data)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = Order_Data.Rows[e.RowIndex];
                string connectionId = row.Cells["ConnectionID"].Value.ToString();
                if ((LiveOrders.DataSource as DataTable).DefaultView.RowFilter == $"Connection_Id='{connectionId}'")
                {
                    ShowAllData(LiveOrders, Account_Data);
                }
                else
                {
                    (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = $"Connection_Id='{connectionId}'";
                    (Account_Data.DataSource as DataTable).DefaultView.RowFilter = $"ConnectionId='{connectionId}'";
                }
            }
        }
        void ShowAllData(DataGridView LiveOrders, DataGridView Account_Data)
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
        async void TimerElapse(object sender, EventArgs e)
        {
            Task task1 = WriteDataTableToCsv("C:\\Users\\mohit.parwani\\Order_Summary.csv", SummaryTable);
            Task task2 = WriteDataTableToCsv("C:\\Users\\mohit.parwani\\AlgoSummary.csv", Algo_Table);
            Task task3 = WriteDataTableToCsv("C:\\Users\\mohit.parwani\\AccountSummary.csv", Account_Table);

            await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
        }
        StringBuilder sb = new StringBuilder();
        async Task WriteDataTableToCsv(string filepath, DataTable dt)
        {
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    Console.WriteLine(filepath + "__" + dt.Rows.Count);
                    sb.Clear();
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
                    using (StreamWriter writer = new StreamWriter(filepath, true))
                    {
                        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
                    }
                    //Console.WriteLine($"Data from DataTable '{dt.TableName}' appended to '{filepath}'.");


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        void BindMessage_Table(DataGridView Connection_Message_Data, DataGridView Account_Message_Data)
        {
            Connection_Message_Table = new DataTable();
            Account_Message_Table = new DataTable();
            Connection_Message_Table.Columns.Add("ConnectionId");
            Connection_Message_Table.Columns.Add("Add");
            Connection_Message_Table.Columns.Add("Delete");
            Connection_Message_Table.Columns.Add("Update");
            Connection_Message_Table.Columns.Add("Messages");
            Connection_Message_Table.Columns.Add("Volume");
            Connection_Message_Table.Columns.Add("MVR");
            Connection_Message_Table.PrimaryKey = new DataColumn[] { Connection_Message_Table.Columns["ConnectionId"] };
            Connection_Message_Data.DataSource = Connection_Message_Table;

            Account_Message_Table.Columns.Add("Account");
            Account_Message_Table.Columns.Add("Add");
            Account_Message_Table.Columns.Add("Delete");
            Account_Message_Table.Columns.Add("Update");
            Account_Message_Table.Columns.Add("Messages");
            Account_Message_Table.Columns.Add("Volume");
            Account_Message_Table.Columns.Add("MVR");
            Account_Message_Table.PrimaryKey = new DataColumn[] { Account_Message_Table.Columns["Account"] };
            Account_Message_Data.DataSource = Account_Message_Table;


        }
        void BindAccount_Data(DataGridView Account_Data)
        {
            Account_Table = new DataTable();
            Account_Table.Columns.Add("ConnectionId");
            Account_Table.Columns.Add("Account");
            Account_Table.Columns.Add("NumberOfOrders");
            Account_Table.PrimaryKey = new DataColumn[] { Account_Table.Columns["ConnectionId"], Account_Table.Columns["Account"] };
            Account_Data.DataSource = Account_Table;


        }
        void BindAlgo_Orders(DataGridView Algo_Orders)
        {
            Algo_Table = new DataTable();
            Algo_Table.Columns.Add("Algo_Name");
            Algo_Table.Columns.Add("Status");
            Algo_Table.Columns.Add("Account");
            Algo_Table.Columns.Add("NumberofOrders");
            Algo_Table.PrimaryKey = new DataColumn[] { Algo_Table.Columns["Algo_Name"], Algo_Table.Columns["Status"], Algo_Table.Columns["Account"] };
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
            Orders = new DataTable();
            Orders.Columns.Add("SiteOrderKey", typeof(string));
            Orders.Columns.Add("Connection_Id", typeof(string));
            Orders.Columns.Add("Instrument", typeof(string));
            Orders.Columns.Add("Exchange", typeof(string));
            Orders.Columns.Add("OrderPrice", typeof(string));
            Orders.Columns.Add("OrderSide", typeof(string));
            Orders.Columns.Add("OrderQuantity", typeof(string));
            Orders.Columns.Add("Account", typeof(string));
            Orders.PrimaryKey = new DataColumn[] { Orders.Columns["SiteOrderKey"] };
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
            var groupedOrders = from row in Orders.AsEnumerable()
                                group row by row.Field<string>("Connection_Id") into grp
                                select new
                                {
                                    ConnectionId = grp.Key,
                                    NumberofOrders = grp.Count(),
                                    NumberofUniqueAccounts = grp.Select(r => r.Field<string>("Account")).Distinct().Count()
                                };
            //var groupedOrders = orderMap.Values.GroupBy(ord => ord.ConnectionId).Select(group => new
            //{
            //    ConnectionId = group.Key,
            //    NumberOfOrders = group.Count(),
            //    NumberOfUniqueAccouts = group.Select(order => order.Account).Distinct().Count()
            //});
            ConcurrentDictionary<object, DataRow> summaryRows = new ConcurrentDictionary<object, DataRow>();

            Parallel.ForEach(groupedOrders, item =>
            {
                DataRow newRow = SummaryTable.NewRow();
                newRow["ConnectionId"] = item.ConnectionId;
                newRow["NumberofOrders"] = item.NumberofOrders;
                newRow["NumberofUniqueAccounts"] = item.NumberofUniqueAccounts;
                summaryRows.TryAdd(item.ConnectionId, newRow);
            });

            foreach (var row in summaryRows)
            {
                SummaryTable.Rows.Add(row.Value);
            }

        }
        void Update_Message_Data(Order ord, string action, Order ord2 = null)
        {

            DataRow ConnectionRowtoUpdate = Connection_Message_Table.Rows.Find(ord.ConnectionId);
            DataRow AccountRowtoUpdate = Account_Message_Table.Rows.Find(ord.Account);
            if (ConnectionRowtoUpdate == null)
            {
                ConnectionRowtoUpdate = Connection_Message_Table.Rows.Add(ord.ConnectionId, 0, 0, 0, 0, 0, "NA");
            }
            if (AccountRowtoUpdate == null)
            {
                AccountRowtoUpdate = Account_Message_Table.Rows.Add(ord.Account, 0, 0, 0, 0, 0, "NA");
            }
            if (ord2 != null && action == "Filled")
            {
                Console.WriteLine($"{ord.Instrument.GetLegs().Count} && {ord2.Instrument.GetLegs().Count}");
                if (ord.Instrument.GetLegs().Count == 0 && ord2.Instrument.GetLegs().Count == 0)
                {
                    Console.WriteLine($"in it{ord.Instrument.GetLegs().Count} && {ord2.Instrument.GetLegs().Count}");
                    ConnectionRowtoUpdate["Volume"] = Convert.ToInt32(ConnectionRowtoUpdate["Volume"]) + ord.FillQuantity - ord2.FillQuantity;
                    AccountRowtoUpdate["Volume"] = Convert.ToInt32(AccountRowtoUpdate["Volume"]) + ord.FillQuantity - ord2.FillQuantity;
                }
            }
            else
            {
                ConnectionRowtoUpdate[$"{action}"] = Convert.ToInt32(ConnectionRowtoUpdate[$"{action}"]) + 1;
                ConnectionRowtoUpdate["Messages"] = Convert.ToInt32(ConnectionRowtoUpdate["Messages"]) + 1;
                AccountRowtoUpdate[$"{action}"] = Convert.ToInt32(AccountRowtoUpdate[$"{action}"]) + 1;
                AccountRowtoUpdate["Messages"] = Convert.ToInt32(AccountRowtoUpdate["Messages"]) + 1;
            }

            double volume = Convert.ToDouble(ConnectionRowtoUpdate["Volume"]);
            if (volume == 0)
            {
                ConnectionRowtoUpdate["MVR"] = "NA";
            }
            else
            {
                double messages = Convert.ToDouble(ConnectionRowtoUpdate["Messages"]);
                double mvr = Math.Round((messages / volume), 2);
                ConnectionRowtoUpdate["MVR"] = mvr;
            }
            volume = Convert.ToDouble(AccountRowtoUpdate["Volume"]);
            if (volume == 0)
            {
                AccountRowtoUpdate["MVR"] = "NA";
            }
            else
            {
                double messages = Convert.ToDouble(AccountRowtoUpdate["Messages"]);
                double mvr = Math.Round((messages / volume), 2);
                AccountRowtoUpdate["MVR"] = mvr;
            }


        }

        void DeleteFromAlgo_TableandAccount_Table(Order ord)
        {
            DataRow AccountRowtoUpdate = Account_Table.Rows.Find(new object[] { ord.ConnectionId, ord.Account });
            if (AccountRowtoUpdate != null)
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
                //Console.WriteLine(ord.SyntheticStatus + " " + ord.SyntheticType);
                if (ord.Algo != null)
                {
                    DataRow AlgorowToUpdate = Algo_Table.Rows.Find(new object[] { ord.Algo.Alias, ord.SyntheticStatus, ord.Account });
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
            Action updateAccountTable = () =>
            {
                DataRow accountRowToUpdate = Account_Table.Rows.Find(new object[] { ord.ConnectionId, ord.Account });
                if (accountRowToUpdate != null)
                {
                    accountRowToUpdate["NumberofOrders"] = Convert.ToInt32(accountRowToUpdate["NumberofOrders"]) + 1;
                }
                else
                {
                    Account_Table.Rows.Add(ord.ConnectionId, ord.Account, "1");
                }
            };
            //if (ord.SyntheticStatus != tt_net_sdk.SynthStatus.NotSet)
            //{
            //    foreach (Leg leg in ord.Instrument.GetLegs())
            //    {
            //        Console.Write("leg.Instrument.GetLegs().Count=" + leg.Instrument.GetLegs().Count + " " + "leg.QuantityRatio=" + leg.QuantityRatio + "____");
            //        foreach (var ran in leg.Instrument.GetLegs())
            //            Console.Write("ran.Instrument.GetLegs().Count=" + ran.Instrument.GetLegs().Count + "__" + "ran.QuantityRatio=" + ran.QuantityRatio + "\t");
            //    }
            //    Console.WriteLine(ord.Instrument + "__" + ord.WorkingQuantity);
            //    Console.WriteLine(ord.SyntheticStatus + " " + ord.SyntheticType + "\n\n");
            //}
            Action updateAlgoTable = () =>
            {
                if (ord.Algo != null)
                {
                    DataRow algoRowToUpdate = Algo_Table.Rows.Find(new object[] { ord.Algo.Alias, ord.SyntheticStatus, ord.Account });
                    if (algoRowToUpdate != null)
                    {
                        algoRowToUpdate["NumberofOrders"] = Convert.ToInt32(algoRowToUpdate["NumberofOrders"]) + 1;
                    }
                    else
                    {
                        Algo_Table.Rows.Add(ord.Algo.Alias, ord.SyntheticStatus, ord.Account, "1");
                    }
                }
            };

            Parallel.Invoke(updateAccountTable, updateAlgoTable);
        }
        void PrintOrderDetails(Order ord)
        {

            Console.WriteLine("\n" + ord + "  " + ord.OrderType);
            //Console.WriteLine("acct={0,-15}\tinstrument={3,-30}\tlimit price={1,-15}\tworking qty={2,-15}\tconnectionId={4,-15}", ord.Account, ord.LimitPrice, ord.WorkingQuantity, ord.InstrumentKey, ord.ConnectionId);
        }
        void tsubs_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            //Console.WriteLine("downloading order book");
            //Console.WriteLine(e.Orders);

            lock (_locker)
            {
                Task.Run(() =>
                {
                    foreach (Order ord in e.Orders)
                    {
                        //PrintOrderDetails(ord);
                        Orders.Rows.Add(ord.SiteOrderKey, ord.ConnectionId, ord.InstrumentKey.Alias, ord.InstrumentKey.MarketId, ord.LimitPrice, ord.Side, ord.WorkingQuantity, ord.Account);
                        //orderMap.Add(ord.SiteOrderKey, ord);
                        AddtoAlgo_TableandAccount_Table(ord);
                    }
                    UpdateOrder_Data();
                    TimerElapse(null, EventArgs.Empty);
                    InitializeTimer();
                });
            }
        }

        void tsubs_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            lock (_locker)
            {
                Console.WriteLine("Added");
                //PrintOrderDetails(e.Order);
                Task.Run(() =>
                {
                    DataRow checkOrderExist = Orders.Rows.Find(e.Order.SiteOrderKey);
                    if (checkOrderExist != null)
                    {
                        Console.WriteLine(checkOrderExist + "\n" + e.Order);
                    }
                    else
                    {
                        Orders.Rows.Add(e.Order.SiteOrderKey, e.Order.ConnectionId, e.Order.InstrumentKey.Alias, e.Order.InstrumentKey.MarketId, e.Order.LimitPrice, e.Order.Side, e.Order.WorkingQuantity, e.Order.Account);
                    }//orderMap.Add(e.Order.SiteOrderKey, e.Order);
                    UpdateOrder_Data();
                    Update_Message_Data(e.Order, "Add");
                    AddtoAlgo_TableandAccount_Table(e.Order);
                });
            }
        }
        void tsubs_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            lock (_locker)
            {
                //Console.WriteLine("deleted={0}", e.ToString());
                //Console.WriteLine("Delete");
                //string key = e.DeletedUpdate.SiteOrderKey;
                //Console.WriteLine(orderMap.ContainsKey(key) ? "true" : "false");
                //Console.WriteLine(e.Message);
                //PrintOrderDetails(e.DeletedUpdate);
                //PrintOrderDetails(e.OldOrder);
                Task.Run(() =>
                {
                    DataRow checkOrderExist = Orders.Rows.Find(e.DeletedUpdate.SiteOrderKey);
                    if (checkOrderExist != null)
                    {
                        Orders.Rows.Remove(Orders.Rows.Find(e.DeletedUpdate.SiteOrderKey));
                    }
                    //orderMap.Remove(e.DeletedUpdate.SiteOrderKey);
                    DeleteFromAlgo_TableandAccount_Table(e.DeletedUpdate);
                    Update_Message_Data(e.DeletedUpdate, "Delete");
                    UpdateOrder_Data();
                });
            }
        }
        void tsubs_OrderFilled(object sender, OrderFilledEventArgs e)
        {
            lock (_locker)
            {
                Console.WriteLine("filled={0}", e.ToString());
                Console.WriteLine("OldOrder.WorkingQuantity=" + e.OldOrder.WorkingQuantity + " " + "NewOrder.WorkingQuantity=" + e.NewOrder.WorkingQuantity + " " + "OldOrder.FillQuantity=" + e.OldOrder.FillQuantity + " " + "NewOrder.FillQuantity=" + e.NewOrder.FillQuantity);
                Console.WriteLine(e.NewOrder.SyntheticType);
                Task.Run(() =>
                {
                    Orders.Rows.Remove(Orders.Rows.Find(e.OldOrder.SiteOrderKey));
                    //orderMap.Remove(e.OldOrder.SiteOrderKey);
                    DeleteFromAlgo_TableandAccount_Table(e.OldOrder);
                    Orders.Rows.Add(e.NewOrder.SiteOrderKey, e.NewOrder.ConnectionId, e.NewOrder.InstrumentKey.Alias, e.NewOrder.InstrumentKey.MarketId, e.NewOrder.LimitPrice, e.NewOrder.Side, e.NewOrder.WorkingQuantity, e.NewOrder.Account);
                    //orderMap.Add(e.NewOrder.SiteOrderKey, e.NewOrder);
                    Update_Message_Data(e.NewOrder, "Filled", e.OldOrder);
                    Console.WriteLine("e.Fill.Instrument" + e.Fill.Instrument + "e.NewOrder.Instrument" + e.NewOrder.Instrument);
                    UpdateOrder_Data();
                    AddtoAlgo_TableandAccount_Table(e.NewOrder);
                });
            }
        }
        void tsubs_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            Orders.Rows.Remove(Orders.Rows.Find(e.Order.SiteOrderKey));
            //orderMap.Remove(e.Order.SiteOrderKey);
            //Console.WriteLine("rejected={0}", e.ToString());
        }
        void tsubs_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            lock (_locker)
            {
                //Console.WriteLine(e.Message);
                //PrintOrderDetails(e.NewOrder);
                //PrintOrderDetails(e.NewOrder);
                //Console.WriteLine(e.OldOrder.);
                //Console.WriteLine(e.NewOrder);
                Task.Run(() =>
                {
                    try
                    {
                        DataRow rowUpdated = Orders.Rows.Find(e.OldOrder.SiteOrderKey);
                        if (rowUpdated != null)
                        {
                            //rowUpdated["Connection_Id"] = e.NewOrder.ConnectionId;
                            //rowUpdated["Instrument"] = e.NewOrder.InstrumentKey.Alias;
                            //rowUpdated["Exchange"] = e.NewOrder.InstrumentKey.MarketId;
                            //rowUpdated["OrderPrice"] = e.NewOrder.LimitPrice;
                            //rowUpdated["OrderSide"] = e.NewOrder.Side;
                            //rowUpdated["OrderQuantity"] = e.NewOrder.WorkingQuantity;
                            //rowUpdated["Account"] = e.NewOrder.Account;
                            Orders.Rows.Remove(Orders.Rows.Find(e.OldOrder.SiteOrderKey));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("old row not found and still giving the error" + ex.Message);
                    }
                    //orderMap.Remove(e.OldOrder.SiteOrderKey);
                    DeleteFromAlgo_TableandAccount_Table(e.OldOrder);
                    try
                    {
                        Orders.Rows.Add(e.NewOrder.SiteOrderKey, e.NewOrder.ConnectionId, e.NewOrder.InstrumentKey.Alias, e.NewOrder.InstrumentKey.MarketId, e.NewOrder.LimitPrice, e.NewOrder.Side, e.NewOrder.WorkingQuantity, e.NewOrder.Account);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("new row not getting added" + ex.Message);
                    }
                    //orderMap.Add(e.NewOrder.SiteOrderKey, e.NewOrder);
                    UpdateOrder_Data();
                    Update_Message_Data(e.NewOrder, "Update");
                    AddtoAlgo_TableandAccount_Table(e.NewOrder);
                });

            }
        }
        void tsubs_OrderStatusUnknown(object sender, OrderStatusUnknownEventArgs e)
        {
            //Console.WriteLine("unknown={0}", e.ToString());
        }
        void tsubs_OrderTimeout(object sender, OrderTimeoutEventArgs e)
        {
            Task.Run(() =>
            {
                Orders.Rows.Remove(Orders.Rows.Find(e.Order.SiteOrderKey));
                //orderMap.Remove(e.Order.SiteOrderKey);
                //Console.WriteLine("timeout={0}", e.ToString());
                UpdateOrder_Data();
                DeleteFromAlgo_TableandAccount_Table(e.Order);
            });
        }
    }
}

