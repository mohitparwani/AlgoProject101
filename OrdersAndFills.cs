﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using tt.messaging.order.enums;
using tt_net_sdk;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using System.Diagnostics;
using tt_net_sdk.util;
using System.Security.Cryptography;
using static tt.messaging.ttus.Connection.Types;


namespace AlgoProject101
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public string ClientId { get; }
        public ClientConnectedEventArgs(string clientId)
        {
            //Console.WriteLine("75  client connected");
            ClientId = clientId;
        }
    }
    public class DATA : WebSocketBehavior
    {
        private static HashSet<string> connectedSessionIds = new HashSet<string>();
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("Received message from client" + e.Data);
            //Send(e.Data);
        }
        protected override void OnOpen()
        {
            //connectedSessionIds.Add(ID);
            //Console.WriteLine("added_____" + ID);
            //Console.WriteLine("ws open");
            OnClientConnected(new ClientConnectedEventArgs(ID));
            base.OnOpen();
        }
        protected virtual void OnClientConnected(ClientConnectedEventArgs e)
        {
            //Console.WriteLine("client connected");
            ClientConnected?.Invoke(this, e);
        }
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            //Console.WriteLine("removed_____"+ID);
            connectedSessionIds.Remove(ID);
        }
    }

    internal class OrdersAndFills
    {
        private readonly object _locker = new object();
        private readonly object sm_locker = new object();
        TradeSubscription tsubs;
        private Timer Coordinatetimer;
        private Timer updateTimer;
        private Timer updateOrderTimer;
        public static WebSocketServer OAFwssv;
        //HashSet<string> orders;
        //DataTable Orders;
        //DataTable SummaryTable;
        //DataTable Algo_Table;
        //DataTable Account_Table;
        //DataTable Connection_Message_Table;
        //DataTable Account_Message_Table;
        Dictionary<string, int> actionIndexPair;
        Dictionary<string, decimal[]> ConnectionInfo;
        Dictionary<string, decimal[]> ConnectionMessages;
        Dictionary<string, decimal[]> AccountMessages;
        Dictionary<string, Order> OrderBook;
        Dictionary<string, Dictionary<string, List<object>>> CoordinateChart;
        private readonly DATA datainstance;
        public OrdersAndFills(Dispatcher disp, DataGridView LiveOrders, DataGridView Order_Data, DataGridView Algo_Orders, DataGridView Account_Data, DataGridView Connection_Message_Data, DataGridView Account_Message_Data, WebSocketServer wssv)
        {
            Initialize_WebSocket(wssv);
            tsubs = new TradeSubscription(disp);
            tsubs.OrderBookDownload += new EventHandler<OrderBookDownloadEventArgs>(tsubs_OrderBookDownload);
            tsubs.OrderAdded += new EventHandler<OrderAddedEventArgs>(tsubs_OrderAdded);
            tsubs.OrderDeleted += new EventHandler<OrderDeletedEventArgs>(tsubs_OrderDeleted);
            tsubs.OrderFilled += new EventHandler<OrderFilledEventArgs>(tsubs_OrderFilled);
            tsubs.OrderRejected += new EventHandler<OrderRejectedEventArgs>(tsubs_OrderRejected);
            tsubs.OrderUpdated += new EventHandler<OrderUpdatedEventArgs>(tsubs_OrderUpdated);
            tsubs.OrderStatusUnknown += new EventHandler<OrderStatusUnknownEventArgs>(tsubs_OrderStatusUnknown);
            tsubs.OrderTimeout += new EventHandler<OrderTimeoutEventArgs>(tsubs_OrderTimeout);
            OrderBook = new Dictionary<string, Order>();
            CoordinateChart = new Dictionary<string, Dictionary<string, List<object>>>();
            ConnectionInfo = new Dictionary<string, decimal[]>();
            ConnectionMessages = new Dictionary<string, decimal[]>();
            AccountMessages = new Dictionary<string, decimal[]>();
            InitialilzeIndexes();
            tsubs.Start();
            //Order_Data.CellClick += (sender, e) => Order_Data_CellClick(sender, e, Order_Data, LiveOrders, Account_Data);
            //Account_Data.CellClick += (sender, e) => Account_Data_CellClick(sender, e, Account_Data, LiveOrders);
            //orders = new HashSet<string>();
            //BindOrder_Data(Order_Data);
            //BindAlgo_Orders(Algo_Orders);
            //BindLiveOrders(LiveOrders);
            //BindAccount_Data(Account_Data);
            //BindMessage_Table(Connection_Message_Data, Account_Message_Data);
        }
        void InitialilzeIndexes()
        {
            actionIndexPair = new Dictionary<string, int>();
            actionIndexPair.Add("Add", 0);
            actionIndexPair.Add("Delete", 1);
            actionIndexPair.Add("Update", 2);
            actionIndexPair.Add("Messages", 3);
            actionIndexPair.Add("Volume", 4);
            actionIndexPair.Add("MVR", 5);
            //actionIndexPair.Add("NumberofOrders", 0);
            //actionIndexPair.Add("NumberOfUniqueAccounts", 1);
        }
        void Initialize_WebSocket(WebSocketServer wssv)
        {
            OAFwssv = wssv;
            Console.WriteLine("server started on ws://localhost:1234");
            subscribeToclientConnectedEvent("/ConnectionMessageData");
            subscribeToclientConnectedEvent("/AccountMessageData");
            subscribeToclientConnectedEvent("/ConnectionIdData");
            OAFwssv.AddWebSocketService<DATA>("/ChartData");
            //OAFwssv.AddWebSocketService<DATA>("/ConnectionMessageData",()=>new DATA());
            //OAFwssv.AddWebSocketService<DATA>("/AccountMessageData", () => new DATA());
            //OAFwssv.AddWebSocketService<DATA>("/ConnectionIdData", () => new DATA());
            //OAFwssv.AddWebSocketService<DATA>("/AccountData", () => new DATA());
            //OAFwssv.AddWebSocketService<DATA>("/AlgoData", () => new DATA());
        }
        void subscribeToclientConnectedEvent(string path)
        {
            OAFwssv.AddWebSocketService<DATA>(path, () =>
            {
                var instace = new DATA();
                instace.ClientConnected += FirstBroadCastData;
                return instace;
            });
        }
        //void Account_Data_CellClick(object sender, DataGridViewCellEventArgs e, DataGridView Account_Data, DataGridView LiveOrders)
        //{
        //    if (e.RowIndex >= 0)
        //    {
        //        DataGridViewRow row = Account_Data.Rows[e.RowIndex];
        //        string account = row.Cells["Account"].Value.ToString();
        //        if ((LiveOrders.DataSource as DataTable).DefaultView.RowFilter == $"Account='{account}'")
        //        {
        //            (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = "";
        //        }
        //        else
        //            (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = $"Account='{account}'";
        //    }
        //}
        //void Order_Data_CellClick(object sender, DataGridViewCellEventArgs e, DataGridView Order_Data, DataGridView LiveOrders, DataGridView Account_Data)
        //{
        //    if (e.RowIndex >= 0)
        //    {
        //        DataGridViewRow row = Order_Data.Rows[e.RowIndex];
        //        string connectionId = row.Cells["ConnectionID"].Value.ToString();
        //        if ((LiveOrders.DataSource as DataTable).DefaultView.RowFilter == $"Connection_Id='{connectionId}'")
        //        {
        //            ShowAllData(LiveOrders, Account_Data);
        //        }
        //        else
        //        {
        //            (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = $"Connection_Id='{connectionId}'";
        //            (Account_Data.DataSource as DataTable).DefaultView.RowFilter = $"ConnectionId='{connectionId}'";
        //        }
        //    }
        //}
        //void ShowAllData(DataGridView LiveOrders, DataGridView Account_Data)
        //{
        //    (LiveOrders.DataSource as DataTable).DefaultView.RowFilter = "";
        //    (Account_Data.DataSource as DataTable).DefaultView.RowFilter = "";

        //}
        void InitializeTimer()
        {
            //timer = new Timer();
            //timer.Interval = 5 * 60 * 1000;
            //timer.Tick += TimerElapse;
            //timer.Start();

            updateTimer = new Timer();
            updateTimer.Interval = 10 * 1000;
            updateTimer.Tick += BroadCastData;
            updateTimer.Start();

            updateOrderTimer = new Timer();
            updateOrderTimer.Interval = 15 * 1000;
            updateOrderTimer.Tick += UpdateOrderDataFromOrderBook;
            updateOrderTimer.Start();

            Coordinatetimer = new Timer();
            Coordinatetimer.Interval = 5 * 60 * 1000;
            Coordinatetimer.Tick += UpdateCoordinateChart;
            Coordinatetimer.Start();
        }
        void UpdateCoordinateChart(object sender, EventArgs e)
        {
            string currentTime = DateTime.Now.ToString("HH:mm");
            foreach (var acc in AccountMessages)
            {

                var accountColumns = CoordinateChart.ContainsKey(acc.Key) ? CoordinateChart[acc.Key] : new Dictionary<string, List<object>>();
                foreach (var each in actionIndexPair)
                {
                    object coordinate = new { x = currentTime, y = acc.Value[each.Value] };
                    var ActionCoordinates = accountColumns.ContainsKey(each.Key) ? accountColumns[each.Key] : new List<object>();
                    ActionCoordinates.Add(coordinate);
                    accountColumns[each.Key] = ActionCoordinates;
                }
                CoordinateChart[acc.Key] = accountColumns;

            }
            Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");

            BroadCastCoordinateChart();

        }
        void BroadCastCoordinateChart()
        {
            Console.WriteLine("_____________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________");
            try
            {
                string json = JsonConvert.SerializeObject(CoordinateChart, Formatting.Indented);
                OAFwssv.WebSocketServices["/ChartData"].Sessions.Broadcast(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to charData client: {ex}");
                Console.WriteLine("178");
            }
        }
        void FirstBroadCastData(Object sender, ClientConnectedEventArgs e)
        {
            //Console.WriteLine("____________________"+e.ClientId);
            BroadCastDataToWebSocket("/ConnectionIdData", ConnectionInfo, "ConnectionIdData", e.ClientId);
            BroadCastDataToWebSocket("/AccountMessageData", AccountMessages, "AccountMessageData", e.ClientId);
            BroadCastDataToWebSocket("/ConnectionMessageData", ConnectionMessages, "ConnectionMessageData", e.ClientId);
            BroadCastCoordinateChart();
        }
        void BroadCastData(Object sender, EventArgs e)
        {
            BroadCastDataToWebSocket("/ConnectionIdData", ConnectionInfo, "ConnectionIdData");
            BroadCastDataToWebSocket("/AccountMessageData", AccountMessages, "AccountMessageData");
            BroadCastDataToWebSocket("/ConnectionMessageData", ConnectionMessages, "ConnectionMessageData");
        }

        void BroadCastDataToWebSocket(string path, Dictionary<string, decimal[]> data, string errorMessage, string id = null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                OAFwssv.WebSocketServices[path].Sessions.Broadcast(json);
                //DATA.BrodCastData(json, path, OAFwssv.WebSocketServices[path].Sessions);
                //if (id == null)
                //else
                //{
                //    Console.WriteLine("print from");
                //    OAFwssv.WebSocketServices[path].Sessions.SendTo(json, id);
                //}//Console.WriteLine(OAFwssv.WebSocketServices[path].Sessions.Count+" "+ OAFwssv.WebSocketServices[path].Sessions.ActiveIDs);
                //Console.WriteLine(path+"     "+json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to {path} client: {errorMessage} {ex}");
                Console.WriteLine("178");
            }
        }



        //void BroadCastData(Object sender, EventArgs e)
        //{
        //    BroadcastWebSocketData(OAFwssv, "/ConnectionIdData", SummaryTable, "ConnectionIdData");
        //    BroadcastWebSocketData(OAFwssv, "/AccountMessageData", Account_Message_Table, "AccountMessageData");
        //    BroadcastWebSocketData(OAFwssv, "/ConnectionMessageData", Connection_Message_Table, "ConnectionMessageData");
        //    BroadcastWebSocketData(OAFwssv, "/AccountData", Account_Table, "AccountData");
        //    BroadcastWebSocketData(OAFwssv, "/AlgoData", Algo_Table, "AlgoData");
        //}
        //void BroadcastWebSocketData(WebSocketServer OAFwssv, string path, DataTable data, string errorMessage)
        //{
        //    try
        //    {
        //        string json = JsonConvert.SerializeObject(data);
        //        OAFwssv.WebSocketServices[path].Sessions.Broadcast(json);
        //        Console.WriteLine(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error connecting to {path} client: {errorMessage} {ex}");
        //        Console.WriteLine("160");
        //    }
        //}


        //async void TimerElapse(object sender, EventArgs e)
        //{
        //    Task task1 = WriteDataTableToCsv("C:\\Users\\mohit.parwani\\Order_Summary.csv", SummaryTable);
        //    Task task2 = WriteDataTableToCsv("C:\\Users\\mohit.parwani\\AlgoSummary.csv", Algo_Table);
        //    Task task3 = WriteDataTableToCsv("C:\\Users\\mohit.parwani\\AccountSummary.csv", Account_Table);

        //    await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
        //}
        //StringBuilder sb = new StringBuilder();
        //async Task WriteDataTableToCsv(string filepath, DataTable dt)
        //{
        //    try
        //    {
        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            Console.WriteLine(filepath + "__" + dt.Rows.Count);
        //            sb.Clear();
        //            if (!File.Exists(filepath))
        //            {
        //                sb.Append("TimeStamp" + ",");
        //                foreach (DataColumn col in dt.Columns)
        //                {
        //                    sb.Append(col.ColumnName + ",");
        //                }
        //                sb.AppendLine();
        //            }
        //            foreach (DataRow row in dt.Rows)
        //            {
        //                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",");
        //                foreach (object item in row.ItemArray)
        //                {
        //                    sb.Append(item.ToString() + ",");
        //                }
        //                sb.AppendLine();
        //            }
        //            using (StreamWriter writer = new StreamWriter(filepath, true))
        //            {
        //                await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        //            }
        //            //Console.WriteLine($"Data from DataTable '{dt.TableName}' appended to '{filepath}'.");


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        Console.WriteLine("212");
        //    }
        //}
        //void BindMessage_Table(DataGridView Connection_Message_Data, DataGridView Account_Message_Data)
        //{
        //    Connection_Message_Table = new DataTable();
        //    Account_Message_Table = new DataTable();
        //    Connection_Message_Table.Columns.Add("ConnectionId");
        //    Connection_Message_Table.Columns.Add("Add");
        //    Connection_Message_Table.Columns.Add("Delete");
        //    Connection_Message_Table.Columns.Add("Update");
        //    Connection_Message_Table.Columns.Add("Messages");
        //    Connection_Message_Table.Columns.Add("Volume");
        //    Connection_Message_Table.Columns.Add("MVR");
        //    Connection_Message_Table.PrimaryKey = new DataColumn[] { Connection_Message_Table.Columns["ConnectionId"] };
        //    Connection_Message_Data.DataSource = Connection_Message_Table;

        //    Account_Message_Table.Columns.Add("Account");
        //    Account_Message_Table.Columns.Add("Add");
        //    Account_Message_Table.Columns.Add("Delete");
        //    Account_Message_Table.Columns.Add("Update");
        //    Account_Message_Table.Columns.Add("Messages");
        //    Account_Message_Table.Columns.Add("Volume");
        //    Account_Message_Table.Columns.Add("MVR");
        //    Account_Message_Table.PrimaryKey = new DataColumn[] { Account_Message_Table.Columns["Account"] };
        //    Account_Message_Data.DataSource = Account_Message_Table;


        //}
        //void BindAccount_Data(DataGridView Account_Data)
        //{
        //    Account_Table = new DataTable();
        //    Account_Table.Columns.Add("ConnectionId");
        //    Account_Table.Columns.Add("Account");
        //    Account_Table.Columns.Add("NumberOfOrders");
        //    Account_Table.PrimaryKey = new DataColumn[] { Account_Table.Columns["ConnectionId"], Account_Table.Columns["Account"] };
        //    Account_Data.DataSource = Account_Table;


        //}
        //void BindAlgo_Orders(DataGridView Algo_Orders)
        //{
        //    Algo_Table = new DataTable();
        //    Algo_Table.Columns.Add("Algo_Name");
        //    Algo_Table.Columns.Add("Status");
        //    Algo_Table.Columns.Add("Account");
        //    Algo_Table.Columns.Add("NumberofOrders");
        //    Algo_Table.PrimaryKey = new DataColumn[] { Algo_Table.Columns["Algo_Name"], Algo_Table.Columns["Status"], Algo_Table.Columns["Account"] };
        //    Algo_Orders.DataSource = Algo_Table;


        //}
        //void BindOrder_Data(DataGridView Order_Data)
        //{
        //    SummaryTable = new DataTable();
        //    SummaryTable.Columns.Add("ConnectionID", typeof(string));
        //    SummaryTable.Columns.Add("NumberofOrders", typeof(string));
        //    SummaryTable.Columns.Add("NumberOfUniqueAccounts", typeof(string));
        //    SummaryTable.PrimaryKey = new DataColumn[] { SummaryTable.Columns["ConnectionID"] };
        //    //Order_Data.DataSource = null;
        //    Order_Data.DataSource = SummaryTable;
        //    Order_Data.Columns["ConnectionID"].DataPropertyName = "ConnectionID";
        //    Order_Data.Columns["NumberofOrders"].DataPropertyName = "NumberofOrders";
        //    Order_Data.Columns["NumberOfUniqueAccounts"].DataPropertyName = "NumberOfUniqueAccounts";
        //    //Console.WriteLine("me called");

        //    //Order_Data.Rows.Clear();


        //    Order_Data.Refresh();
        //}
        //void BindLiveOrders(DataGridView LiveOrders)
        //{
        //    Orders = new DataTable();
        //    Orders.Columns.Add("SiteOrderKey", typeof(string));
        //    Orders.Columns.Add("Connection_Id", typeof(string));
        //    Orders.Columns.Add("Instrument", typeof(string));
        //    Orders.Columns.Add("Exchange", typeof(string));
        //    Orders.Columns.Add("OrderPrice", typeof(string));
        //    Orders.Columns.Add("OrderSide", typeof(string));
        //    Orders.Columns.Add("OrderQuantity", typeof(string));
        //    Orders.Columns.Add("Account", typeof(string));
        //    Orders.PrimaryKey = new DataColumn[] { Orders.Columns["SiteOrderKey"] };
        //    LiveOrders.DataSource = Orders;
        //    LiveOrders.Columns["Connection_Id"].DataPropertyName = "Connection_Id";
        //    LiveOrders.Columns["Instrument"].DataPropertyName = "Instrument";
        //    LiveOrders.Columns["Exchange"].DataPropertyName = "Exchange";
        //    LiveOrders.Columns["OrderPrice"].DataPropertyName = "OrderPrice";
        //    LiveOrders.Columns["OrderSide"].DataPropertyName = "OrderSide";
        //    LiveOrders.Columns["OrderQty"].DataPropertyName = "OrderQuantity";
        //    LiveOrders.Columns["Account"].DataPropertyName = "Account";

        //}
        //void UpdateOrder_Data(Object sender, EventArgs e)
        //{
        //    SummaryTable.Clear();
        //    var groupedOrders = from row in Orders.AsEnumerable()
        //                        group row by row.Field<string>("Connection_Id") into grp
        //                        select new
        //                        {
        //                            ConnectionId = grp.Key,
        //                            NumberofOrders = grp.Count(),
        //                            NumberofUniqueAccounts = grp.Select(r => r.Field<string>("Account")).Distinct().Count()
        //                        };


        //    foreach (var item in groupedOrders)
        //    {
        //        SummaryTable.Rows.Add(item.ConnectionId, item.NumberofOrders, item.NumberofUniqueAccounts);
        //    }
        //}
        //    string json = JsonConvert.SerializeObject(SummaryTable);
        //wssv.WebSocketServices["/Data"].Sessions.Broadcast(json);
        void UpdateOrderDataFromOrderBook(Object Sender, EventArgs e)
        {
            ConnectionInfo = OrderBook.GroupBy(order => order.Value.ConnectionId.ToString()).ToDictionary(group => group.Key,
                group => new decimal[]
            {
                    group.Count(),
                    group.Select(order => order.Value.Account).Distinct().Count()
            });
        }
        void UpdateMessagesfromOrderBook(string action, Order ord, Order ord_ = null)
        {
            //lock (sm_locker)
            //{
            decimal[] connectionRow = ConnectionMessages.TryGetValue(ord.ConnectionId.ToString(), out var Crow) ? Crow : new decimal[6];
            decimal[] accountRow = AccountMessages.TryGetValue(ord.Account.ToString(), out var Arow) ? Arow : new decimal[6];
            if (action == "Filled")
            {
                connectionRow[4] += ((ord_ != null ? (ord.FillQuantity - ord_.FillQuantity) : ord.FillQuantity).ToDecimal());

                accountRow[4] += ((ord_ != null ? (ord.FillQuantity - ord_.FillQuantity) : ord.FillQuantity).ToDecimal());
            }
            else
            {
                connectionRow[actionIndexPair[action]] += 1;
                accountRow[actionIndexPair[action]] += 1;
            }
            connectionRow[3] = connectionRow[0] + connectionRow[1] + connectionRow[2];
            accountRow[3] = accountRow[0] + accountRow[1] + accountRow[2];
            connectionRow[5] = connectionRow[4] > 0 ? Math.Round((connectionRow[3] / connectionRow[4]), 2) : 0;
            accountRow[5] = accountRow[4] > 0 ? Math.Round((accountRow[3] / accountRow[4]), 2) : 0;
            ConnectionMessages[ord.ConnectionId.ToString()] = connectionRow;
            AccountMessages[ord.Account.ToString()] = accountRow;
            //}
        }
        //void Update_Message_Data(Order ord, string action, Order ord2 = null)
        //{

        //    DataRow ConnectionRowtoUpdate = Connection_Message_Table.Rows.Find(ord.ConnectionId);
        //    DataRow AccountRowtoUpdate = Account_Message_Table.Rows.Find(ord.Account);
        //    if (ConnectionRowtoUpdate == null)
        //    {
        //        ConnectionRowtoUpdate = Connection_Message_Table.Rows.Add(ord.ConnectionId, 0, 0, 0, 0, 0, "NA");
        //    }
        //    if (AccountRowtoUpdate == null)
        //    {
        //        AccountRowtoUpdate = Account_Message_Table.Rows.Add(ord.Account, 0, 0, 0, 0, 0, "NA");
        //    }
        //    if (action == "Filled")
        //    {
        //        //Console.WriteLine($"{ord.Instrument.GetLegs().Count} && {ord2.Instrument.GetLegs().Count}");
        //        if (ord.Instrument.GetLegs().Count == 0 && ord2.Instrument.GetLegs().Count == 0)
        //        {
        //            Console.WriteLine($"in it{ord.Instrument.GetLegs().Count} && {ord2.Instrument.GetLegs().Count}");
        //            Quantity volumeToAdd = (ord2 != null ? (ord.FillQuantity - ord2.FillQuantity) : ord.FillQuantity);
        //            ConnectionRowtoUpdate["Volume"] = Convert.ToInt32(ConnectionRowtoUpdate["Volume"]) + volumeToAdd;
        //            AccountRowtoUpdate["Volume"] = Convert.ToInt32(AccountRowtoUpdate["Volume"]) + volumeToAdd;
        //        }
        //    }
        //    else
        //    {
        //        ConnectionRowtoUpdate[$"{action}"] = Convert.ToInt32(ConnectionRowtoUpdate[$"{action}"]) + 1;
        //        ConnectionRowtoUpdate["Messages"] = Convert.ToInt32(ConnectionRowtoUpdate["Messages"]) + 1;
        //        AccountRowtoUpdate[$"{action}"] = Convert.ToInt32(AccountRowtoUpdate[$"{action}"]) + 1;
        //        AccountRowtoUpdate["Messages"] = Convert.ToInt32(AccountRowtoUpdate["Messages"]) + 1;
        //    }

        //    double volume = Convert.ToDouble(ConnectionRowtoUpdate["Volume"]);
        //    if (volume == 0)
        //    {
        //        ConnectionRowtoUpdate["MVR"] = "NA";
        //    }
        //    else
        //    {
        //        double messages = Convert.ToDouble(ConnectionRowtoUpdate["Messages"]);
        //        double mvr = Math.Round((messages / volume), 2);
        //        ConnectionRowtoUpdate["MVR"] = mvr;
        //    }
        //    volume = Convert.ToDouble(AccountRowtoUpdate["Volume"]);
        //    if (volume == 0)
        //    {
        //        AccountRowtoUpdate["MVR"] = "NA";
        //    }
        //    else
        //    {
        //        double messages = Convert.ToDouble(AccountRowtoUpdate["Messages"]);
        //        double mvr = Math.Round((messages / volume), 2);
        //        AccountRowtoUpdate["MVR"] = mvr;
        //    }


        //}

        //void DeleteFromAlgo_TableandAccount_Table(Order ord)
        //{
        //    Action updateAccountTable = () =>
        //    {
        //        DataRow AccountRowtoUpdate = Account_Table.Rows.Find(new object[] { ord.ConnectionId, ord.Account });
        //        if (AccountRowtoUpdate != null)
        //        {
        //            int entry = Convert.ToInt32(AccountRowtoUpdate["NumberofOrders"]);
        //            if (entry > 1)
        //            {
        //                AccountRowtoUpdate["NumberofOrders"] = entry - 1;
        //            }
        //            else
        //                Account_Table.Rows.Remove(AccountRowtoUpdate);
        //        }
        //    };
        //    //if (ord.SyntheticStatus != tt_net_sdk.SynthStatus.NotSet)
        //    //Console.WriteLine(ord.SyntheticStatus + " " + ord.SyntheticType);
        //    Action updateAlgoTable = () =>
        //    {
        //        if (ord.Algo != null)
        //        {
        //            DataRow AlgorowToUpdate = Algo_Table.Rows.Find(new object[] { ord.Algo.Alias, ord.SyntheticStatus, ord.Account });
        //            if (AlgorowToUpdate != null)
        //            {
        //                int entry = Convert.ToInt32(AlgorowToUpdate["NumberofOrders"]);
        //                if (entry <= 1)
        //                {
        //                    Algo_Table.Rows.Remove(AlgorowToUpdate);
        //                }
        //                else
        //                    AlgorowToUpdate["NumberofOrders"] = entry - 1;
        //            }

        //        }
        //    };
        //    Parallel.Invoke(updateAccountTable, updateAlgoTable);
        //}

        //void AddtoAlgo_TableandAccount_Table(Order ord)
        //{
        //    Action updateAccountTable = () =>
        //    {
        //        DataRow accountRowToUpdate = Account_Table.Rows.Find(new object[] { ord.ConnectionId, ord.Account });
        //        if (accountRowToUpdate != null)
        //        {
        //            accountRowToUpdate["NumberofOrders"] = Convert.ToInt32(accountRowToUpdate["NumberofOrders"]) + 1;
        //        }
        //        else
        //        {
        //            Account_Table.Rows.Add(ord.ConnectionId, ord.Account, "1");
        //        }
        //    };
        //    if (ord.SyntheticStatus != tt_net_sdk.SynthStatus.NotSet)
        //    {
        //        foreach (Leg leg in ord.Instrument.GetLegs())
        //        {
        //            Console.Write("leg.Instrument.GetLegs().Count=" + leg.Instrument.GetLegs().Count + " " + "leg.QuantityRatio=" + leg.QuantityRatio + "____");
        //            Console.WriteLine(leg.Instrument);
        //            foreach (var ran in leg.Instrument.GetLegs())
        //                Console.Write("ran.Instrument.GetLegs().Count=" + ran.Instrument.GetLegs().Count + "__" + "ran.QuantityRatio=" + ran.QuantityRatio + "\t");
        //        }

        //        Console.WriteLine(ord.Instrument + "__" + ord.WorkingQuantity);
        //        if (ord.Instrument.GetSpreadDetails() != null)
        //        {
        //            try
        //            {
        //                for (int i = 0; i < ord.Instrument.GetSpreadDetails().LegCount(); i++)
        //                {
        //                    Console.WriteLine(ord.Instrument.GetSpreadDetails().GetLeg(i).Instrument);
        //                }
        //                for (int i = 0; i < ord.Instrument.GetSpreadDetails().RuleCount(); i++)
        //                {
        //                    Console.WriteLine(ord.Instrument.GetSpreadDetails().GetRule(i).Name);
        //                    foreach (var x in ord.Instrument.GetSpreadDetails().GetRule(i).CustomVariables)
        //                    {
        //                        Console.WriteLine(x.Name + "__" + x.Value + "__" + x.Type);
        //                    }
        //                }
        //                Console.WriteLine(ord.SyntheticStatus + " " + ord.SyntheticType + "\n\n");
        //            }
        //            catch (Exception e) { Console.WriteLine(ord); }
        //        }
        //    }
        //    Action updateAlgoTable = () =>
        //    {
        //        if (ord.Algo != null)
        //        {
        //            DataRow algoRowToUpdate = Algo_Table.Rows.Find(new object[] { ord.Algo.Alias, ord.SyntheticStatus, ord.Account });
        //            if (algoRowToUpdate != null)
        //            {
        //                algoRowToUpdate["NumberofOrders"] = Convert.ToInt32(algoRowToUpdate["NumberofOrders"]) + 1;
        //            }
        //            else
        //            {
        //                Algo_Table.Rows.Add(ord.Algo.Alias, ord.SyntheticStatus, ord.Account, "1");
        //            }
        //        }
        //    };

        //    Parallel.Invoke(updateAccountTable, updateAlgoTable);
        //}
        void PrintOrderDetails(Order ord)
        {

            Console.WriteLine("\n" + ord + "  " + ord.OrderType);
            //Console.WriteLine("acct={0,-15}\tinstrument={3,-30}\tlimit price={1,-15}\tworking qty={2,-15}\tconnectionId={4,-15}", ord.Account, ord.LimitPrice, ord.WorkingQuantity, ord.InstrumentKey, ord.ConnectionId);
        }
        void AddtoOrderBook(Order ord)
        {
            if (OrderBook.ContainsKey(ord.SiteOrderKey))
            {
                OrderBook.Remove(ord.SiteOrderKey);
            }
            OrderBook.Add(ord.SiteOrderKey, ord);
        }
        void RemoveFromOrderBook(Order ord)
        {
            OrderBook.Remove(ord.SiteOrderKey);
        }

        //void AddOrder(Order ord)
        //{

        //    if (orders.Contains(ord.SiteOrderKey))
        //    {
        //        DataRow rowToRemove = Orders.Rows.Find(ord.SiteOrderKey);
        //        if (rowToRemove != null)
        //        {
        //            Console.WriteLine(rowToRemove + "\n" + ord);
        //            Orders.Rows.Remove(rowToRemove);
        //        }
        //        orders.Remove(ord.SiteOrderKey);
        //    }
        //    else
        //    {
        //        AddtoAlgo_TableandAccount_Table(ord);
        //    }
        //    try
        //    {
        //        orders.Add(ord.SiteOrderKey);
        //        Orders.Rows.Add(ord.SiteOrderKey, ord.ConnectionId, ord.InstrumentKey.Alias, ord.InstrumentKey.MarketId, ord.LimitPrice, ord.Side, ord.WorkingQuantity, ord.Account);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine("486");
        //    }
        //}
        //void RemoveOrder(Order ord)
        //{
        //    if (ord != null && orders.Contains(ord.SiteOrderKey))
        //    {
        //        DataRow rowToRemove = Orders.Rows.Find(ord.SiteOrderKey);
        //        try
        //        {
        //            Orders.Rows.Remove(rowToRemove);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("510", ex.Message);
        //        }
        //        orders.Remove(ord.SiteOrderKey);
        //        DeleteFromAlgo_TableandAccount_Table(ord);
        //    }
        //}
        void tsubs_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            Console.WriteLine("downloading order book");

            lock (_locker)
            {

                foreach (Order ord in e.Orders)
                {
                    //AddOrder(ord);
                    AddtoOrderBook(ord);
                }
                //UpdateOrder_Data(null, EventArgs.Empty);
                UpdateOrderDataFromOrderBook(null, EventArgs.Empty);
                //BroadCastData(null, EventArgs.Empty);
                //TimerElapse(null, EventArgs.Empty);
                InitializeTimer();
            }
        }

        void tsubs_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            lock (_locker)
            {
                Console.WriteLine("Added");
                //PrintOrderDetails(e.Order
                //AddOrder(e.Order);
                AddtoOrderBook(e.Order);
                //Update_Message_Data(e.Order, "Add");
                UpdateMessagesfromOrderBook("Add", e.Order);

            }
        }
        void tsubs_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            lock (_locker)
            {
                Console.WriteLine("Delete");
                //RemoveOrder(e.DeletedUpdate);
                RemoveFromOrderBook(e.DeletedUpdate);
                //Update_Message_Data(e.DeletedUpdate, "Delete");
                UpdateMessagesfromOrderBook("Delete", e.DeletedUpdate);
                //orderMap.Remove(e.DeletedUpdate.SiteOrderKey);
                //UpdateOrder_Data();

            }
        }
        void tsubs_OrderFilled(object sender, OrderFilledEventArgs e)
        {
            lock (_locker)
            {
                Console.WriteLine("filled={0}\n", e.ToString());
                //RemoveOrder(e.OldOrder);
                AddtoOrderBook(e.NewOrder);
                UpdateMessagesfromOrderBook("Filled", e.NewOrder, e.OldOrder);
                //Update_Message_Data(e.NewOrder, "Filled", e.OldOrder);
                //Console.WriteLine("e.Fill.Instrument" + e.Fill.Instrument + "e.NewOrder.Instrument" + e.NewOrder.Instrument);
                //orderMap.Add(e.NewOrder.SiteOrderKey, e.NewOrder);
                //UpdateOrder_Data();
            }
        }
        void tsubs_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            //RemoveOrder(e.Order);
            RemoveFromOrderBook(e.Order);
            //orderMap.Remove(e.Order.SiteOrderKey);
            //Console.WriteLine("rejected={0}", e.ToString());
        }
        void tsubs_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            lock (_locker)
            {
                //RemoveOrder(e.OldOrder);
                //orderMap.Remove(e.OldOrder.SiteOrderKey);
                //AddOrder(e.NewOrder);
                AddtoOrderBook(e.NewOrder);
                //Update_Message_Data(e.NewOrder, "Update");
                UpdateMessagesfromOrderBook("Update", e.NewOrder, e.OldOrder);
            }
        }
        void tsubs_OrderStatusUnknown(object sender, OrderStatusUnknownEventArgs e)
        {
            //Console.WriteLine("unknown={0}", e.ToString());
        }
        void tsubs_OrderTimeout(object sender, OrderTimeoutEventArgs e)
        {
            //RemoveOrder(e.Order);
            RemoveFromOrderBook(e.Order);
        }
    }
}

