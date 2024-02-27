using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tt_net_sdk;

namespace AlgoProject1
{
    internal class PriceSubscriptionWorking
    {
        PriceSubscription psubsx;
        public PriceSubscriptionWorking(Instrument inst, Dispatcher dis)
        {

            psubsx = new PriceSubscription(inst, dis);
            psubsx.Settings = new PriceSubscriptionSettings(PriceSubscriptionType.MarketDepth);
            psubsx.FieldsUpdated += psubs_FieldsUpdated;
            psubsx.Start();
        }
        void psubs_FieldsUpdated(object sender, FieldsUpdatedEventArgs e)
        {
            if (e.Error == null)
            {
                Console.WriteLine("inpricesubscription");
            foreach (FieldId id in e.Fields.GetFieldIds())
            {
                Field f = e.Fields[id];
                string s = f.FormattedValue;
                Console.WriteLine(f.ToString());
                //Console.Write("s=");
                Console.WriteLine(s);
                if (f is PriceField pf)
                {
                    Price p = pf.Value;
                //Console.Write("p=");
                //Console.Write(p);
                }
                else if (f is QuantityField qf)
                {
                    Quantity q = qf.Value;
                //Console.Write("q=");
                //Console.WriteLine(q);
                }
            }
            Console.WriteLine("Ask Depth SnapShot");
            int askDepthLevels = e.Fields.GetLargestCurrentDepthLevel(FieldId.BestAskPrice);
            Console.WriteLine(askDepthLevels);
            for (int i = askDepthLevels - 1; i >= 0; i--)
            {
                Quantity q = e.Fields.GetBestAskQuantityField(i).Value;
                Console.WriteLine(q.ToString());
                if (Quantity.IsEmpty(q))
                    continue;
                Console.WriteLine("Level=" + i + "qty=" + q + "price=" + e.Fields.GetBestAskPriceField(i).Value.ToString());
                int askDetailedDepthLevels = e.Fields.GetLargestCurrentDetailedDepthLevel(FieldId.AskDetailedDepthQuantity, i);
                for (int j = 0; j < askDetailedDepthLevels; j++)
                {
                    Quantity qu = e.Fields.GetAskDetailedDepthQuantityField(i, j).Value;
                    if (Quantity.IsEmpty(qu))
                        continue;
                    Console.WriteLine("Detailed Depth Level=" + j + "Detailed Depth Qty=" + qu + "price=" + e.Fields.GetBestAskPriceField(i).Value.ToString());
                }
            }
            Console.WriteLine("Bid Depth Snapshot");
            int bidDepthLevels = e.Fields.GetLargestCurrentDepthLevel(FieldId.BestBidPrice);
            Console.WriteLine(bidDepthLevels);
            for (int i = 0; i < bidDepthLevels; i++)
            {
                Quantity q = e.Fields.GetBestBidQuantityField(i).Value;
                Console.WriteLine(q.ToString());
                if (Quantity.IsEmpty(q)) continue;
                Console.WriteLine("level=" + i + "qty=" + q + "price=" + e.Fields.GetBestBidPriceField(i).Value.ToString());
                int bidDetailedDepthLevels = e.Fields.GetLargestCurrentDetailedDepthLevel(FieldId.BidDetailedDepthQuantity, i);
                for (int j = 0; j < bidDetailedDepthLevels; j++)
                {
                    Quantity qu = e.Fields.GetBidDetailedDepthQuantityField(i, j).Value;
                    if (Quantity.IsEmpty(qu)) continue;
                    Console.WriteLine("Detailed Depth Level=" + j + "Detailed Depth Qty=" + qu + "price=" + e.Fields.GetBestBidPriceField(i).Value.ToString());
                }
            }
                Price lastP = e.Fields.GetLastTradedPriceField().Value;
                if (lastP.IsValid)
                {
                    Console.WriteLine(lastP.ToString() + " " + lastP.ToTicks());
                }
            }
            else
            {
                Console.WriteLine(e.Error.Message);
            }
            if (e.Error == null)
            {
                Console.WriteLine("inpricesubscription");
                QuantityField ltq = e.Fields.GetLastTradedQuantityField();
                Quantity lastQ = ltq.Value;
                if (lastQ.IsValid)
                {
                    // Extract the quantity in flow
                    int lastQinFlow = lastQ.ToFlow();
                    Console.WriteLine(lastQinFlow);
                }
            }
        }

    }
}