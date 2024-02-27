namespace AlgoProject1
{
    partial class AlgoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Order_Data = new System.Windows.Forms.DataGridView();
            this.ConnectionID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NumberOfOrders = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NumberOfUniqueAccounts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LiveOrders = new System.Windows.Forms.DataGridView();
            this.Connection_Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Instrument = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderSide = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OrderQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Account = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Algo_Orders = new System.Windows.Forms.DataGridView();
            this.Account_Data = new System.Windows.Forms.DataGridView();
            this.ordersAndFillsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ordersAndFillsBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.Order_Data)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LiveOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Algo_Orders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Account_Data)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ordersAndFillsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ordersAndFillsBindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // Order_Data
            // 
            this.Order_Data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Order_Data.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ConnectionID,
            this.NumberOfOrders,
            this.NumberOfUniqueAccounts});
            this.Order_Data.Location = new System.Drawing.Point(1, -2);
            this.Order_Data.Name = "Order_Data";
            this.Order_Data.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Order_Data.Size = new System.Drawing.Size(801, 174);
            this.Order_Data.TabIndex = 0;
            this.Order_Data.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Order_Data_CellContentClick);
            // 
            // ConnectionID
            // 
            this.ConnectionID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ConnectionID.HeaderText = "ConnectionID";
            this.ConnectionID.Name = "ConnectionID";
            this.ConnectionID.Width = 258;
            // 
            // NumberOfOrders
            // 
            this.NumberOfOrders.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.NumberOfOrders.HeaderText = "NumberOfOrders";
            this.NumberOfOrders.Name = "NumberOfOrders";
            this.NumberOfOrders.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.NumberOfOrders.Width = 250;
            // 
            // NumberOfUniqueAccounts
            // 
            this.NumberOfUniqueAccounts.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.NumberOfUniqueAccounts.HeaderText = "NumberOfUniqueAccounts";
            this.NumberOfUniqueAccounts.Name = "NumberOfUniqueAccounts";
            this.NumberOfUniqueAccounts.Width = 250;
            // 
            // LiveOrders
            // 
            this.LiveOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.LiveOrders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Connection_Id,
            this.Instrument,
            this.Exchange,
            this.OrderPrice,
            this.OrderSide,
            this.OrderQty,
            this.Account});
            this.LiveOrders.Location = new System.Drawing.Point(1, 178);
            this.LiveOrders.Name = "LiveOrders";
            this.LiveOrders.Size = new System.Drawing.Size(801, 323);
            this.LiveOrders.TabIndex = 1;
            // 
            // Connection_Id
            // 
            this.Connection_Id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Connection_Id.Frozen = true;
            this.Connection_Id.HeaderText = "Connection_Id";
            this.Connection_Id.Name = "Connection_Id";
            this.Connection_Id.Width = 108;
            // 
            // Instrument
            // 
            this.Instrument.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Instrument.Frozen = true;
            this.Instrument.HeaderText = "Instrument";
            this.Instrument.Name = "Instrument";
            this.Instrument.Width = 109;
            // 
            // Exchange
            // 
            this.Exchange.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Exchange.Frozen = true;
            this.Exchange.HeaderText = "Exchange";
            this.Exchange.Name = "Exchange";
            this.Exchange.Width = 108;
            // 
            // OrderPrice
            // 
            this.OrderPrice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.OrderPrice.Frozen = true;
            this.OrderPrice.HeaderText = "OrderPrice";
            this.OrderPrice.Name = "OrderPrice";
            this.OrderPrice.Width = 108;
            // 
            // OrderSide
            // 
            this.OrderSide.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.OrderSide.Frozen = true;
            this.OrderSide.HeaderText = "OrderSide";
            this.OrderSide.Name = "OrderSide";
            this.OrderSide.Width = 108;
            // 
            // OrderQty
            // 
            this.OrderQty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.OrderQty.Frozen = true;
            this.OrderQty.HeaderText = "OrderQty";
            this.OrderQty.Name = "OrderQty";
            this.OrderQty.Width = 109;
            // 
            // Account
            // 
            this.Account.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Account.Frozen = true;
            this.Account.HeaderText = "Account";
            this.Account.Name = "Account";
            this.Account.Width = 108;
            // 
            // Algo_Orders
            // 
            this.Algo_Orders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Algo_Orders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Algo_Orders.Location = new System.Drawing.Point(808, -2);
            this.Algo_Orders.Name = "Algo_Orders";
            this.Algo_Orders.Size = new System.Drawing.Size(396, 174);
            this.Algo_Orders.TabIndex = 2;
            this.Algo_Orders.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Algo_Orders_CellContentClick);
            // 
            // Account_Data
            // 
            this.Account_Data.AllowUserToOrderColumns = true;
            this.Account_Data.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.Account_Data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Account_Data.Location = new System.Drawing.Point(808, 178);
            this.Account_Data.Name = "Account_Data";
            this.Account_Data.Size = new System.Drawing.Size(396, 323);
            this.Account_Data.TabIndex = 3;
            // 
            // ordersAndFillsBindingSource
            // 
            this.ordersAndFillsBindingSource.DataSource = typeof(AlgoProject101.OrdersAndFills);
            // 
            // ordersAndFillsBindingSource1
            // 
            this.ordersAndFillsBindingSource1.DataSource = typeof(AlgoProject101.OrdersAndFills);
            // 
            // AlgoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1466, 827);
            this.Controls.Add(this.Account_Data);
            this.Controls.Add(this.Algo_Orders);
            this.Controls.Add(this.LiveOrders);
            this.Controls.Add(this.Order_Data);
            this.Name = "AlgoForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.Order_Data)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LiveOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Algo_Orders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Account_Data)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ordersAndFillsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ordersAndFillsBindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView Order_Data;
        private System.Windows.Forms.DataGridView LiveOrders;
        private System.Windows.Forms.DataGridViewTextBoxColumn Connection_Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn Instrument;
        private System.Windows.Forms.DataGridViewTextBoxColumn Exchange;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderSide;
        private System.Windows.Forms.DataGridViewTextBoxColumn OrderQty;
        private System.Windows.Forms.DataGridViewTextBoxColumn Account;
        private System.Windows.Forms.BindingSource ordersAndFillsBindingSource;
        private System.Windows.Forms.BindingSource ordersAndFillsBindingSource1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectionID;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumberOfOrders;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumberOfUniqueAccounts;
        private System.Windows.Forms.DataGridView Algo_Orders;
        private System.Windows.Forms.DataGridView Account_Data;
    }
}

