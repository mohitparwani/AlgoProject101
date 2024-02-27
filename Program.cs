using AlgoProject101;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace AlgoProject1
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.

        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Dispatcher disp = Dispatcher.AttachUIDispatcher())
            {
                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);

                AlgoForm newForm = new AlgoForm();
                string appSecretKey = "c9b44114-d4b5-962f-ade9-d09e0f8f6c4b:f7f9d9c1-990a-54bb-b0aa-3ae60078f4dc";
                tt_net_sdk.ServiceEnvironment env = tt_net_sdk.ServiceEnvironment.ProdSim;
                tt_net_sdk.TTAPIOptions apiconfig = new tt_net_sdk.TTAPIOptions(env, appSecretKey, 5000);
                ApiInitializeHandler handler = new ApiInitializeHandler(newForm.ttNetApiInitHandler);
                TTAPI.CreateTTAPI(disp, apiconfig, handler);
                Application.Run(newForm);
            }
        }
    }
}