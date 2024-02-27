using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tt_net_sdk;

namespace AlgoProject1
{
    internal class FetchingSigleInstrument
    {
        PriceSubscription psubs;
        public FetchingSigleInstrument(Dispatcher disp, MarketId mid, ProductType prodtyp, string prodName, string insName)
        {
            Console.WriteLine("infetchinginstrument");
            InstrumentLookup inslook = new InstrumentLookup(disp, mid, prodtyp, prodName, insName);
            inslook.OnData += new EventHandler<InstrumentLookupEventArgs>(inslook_OnData);
            inslook.GetAsync();
        }
        void inslook_OnData(object sender, InstrumentLookupEventArgs e)
        {
            
            if (e.Event == ProductDataEvent.Found)
            {
                Instrument ins = e.InstrumentLookup.Instrument;

                Console.WriteLine(ins);

                psubs = new PriceSubscription(ins, tt_net_sdk.Dispatcher.Current);
                psubs.Settings=new PriceSubscriptionSettings(PriceSubscriptionType.MarketDepth);
                psubs.FieldsUpdated += psubs_FieldsUpdated;
                psubs.Start();
            }
            else if (e.Event == ProductDataEvent.NotAllowed)
            {
                Console.WriteLine("Not Allowed : Please check your Token access");
            }
            else
            {
                Console.WriteLine("Cannot Find instrument:{0}", e.Message);
            }
        }
        void psubs_FieldsUpdated(object sender, FieldsUpdatedEventArgs e)
        {
            // Inside market fields
            
            foreach (FieldId id in e.Fields.GetFieldIds())
            {
                Field f = e.Fields[id];
                //Console.WriteLine(f.ToString());
                if (f is PriceField pf)
                {
                    Price p = pf.Value;
                }
                else if (f is QuantityField qf)
                {
                    Quantity q = qf.Value;
                }
            }
            Console.WriteLine("Ask Depth Snapshot");
            int askDepthLevels = e.Fields.GetLargestCurrentDepthLevel(FieldId.BestAskPrice);
            for (int i = askDepthLevels - 1; i >= 0; i--)
            {
                Quantity q = e.Fields.GetBestAskQuantityField(i).Value;
                if (Quantity.IsEmpty(q))
                    continue;
                Console.WriteLine(" Level=" + i + " Qty=" + q + " Price=" + e.Fields.GetBestAskPriceField(i).Value.ToString());
                int askDetailedDepthLevels = e.Fields.GetLargestCurrentDetailedDepthLevel(FieldId.AskDetailedDepthQuantity, i);
                for (int j = 0; j < askDetailedDepthLevels; j++)
                {
                    Quantity qu = e.Fields.GetAskDetailedDepthQuantityField(i, j).Value;
                    if (Quantity.IsEmpty(qu))
                        continue;
                    Console.WriteLine("  DETAILED DEPTH  Level=" + j + " Detailed Depth Qty=" + qu + " Price=" + e.Fields.GetBestAskPriceField(i).Value.ToString());
                }
            }
            Console.WriteLine("Bid Depth Snapshot");
            int bidDepthLevels = e.Fields.GetLargestCurrentDepthLevel(FieldId.BestBidPrice);
            for (int i = 0; i < bidDepthLevels; i++)
            {
                Quantity q = e.Fields.GetBestBidQuantityField(i).Value;
                if (Quantity.IsEmpty(q))
                    continue;
                Console.WriteLine(" Level=" + i + " Qty=" + q + " Price=" + e.Fields.GetBestBidPriceField(i).Value.ToString());
                int bidDetailedDepthLevels = e.Fields.GetLargestCurrentDetailedDepthLevel(FieldId.BidDetailedDepthQuantity, i);
                for (int j = 0; j < bidDetailedDepthLevels; j++)
                {
                    Quantity qu = e.Fields.GetBidDetailedDepthQuantityField(i, j).Value;
                    if (Quantity.IsEmpty(qu))
                        continue;
                    Console.WriteLine("  DETAILED DEPTH Level=" + j + " Detailed Depth Qty=" + qu + " Price=" + e.Fields.GetBestBidPriceField(i).Value.ToString());
                }
            }
        }
    }
}