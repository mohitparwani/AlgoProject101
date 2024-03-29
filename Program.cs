﻿using AlgoProject101;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;
using WebSocketSharp.Server;

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
                //string appSecretKey = "c9b44114-d4b5-962f-ade9-d09e0f8f6c4b:f7f9d9c1-990a-54bb-b0aa-3ae60078f4dc";
                string appLiveKey = "84d5cfba-2018-d536-30eb-c4001fdbd132:5beecf50-5e4b-b68c-f9c4-17a7ed5b985a";
                tt_net_sdk.ServiceEnvironment env = tt_net_sdk.ServiceEnvironment.ProdLive;
                //tt_net_sdk.ServiceEnvironment env = tt_net_sdk.ServiceEnvironment.ProdSim;
                tt_net_sdk.TTAPIOptions apiconfig = new tt_net_sdk.TTAPIOptions(env, appLiveKey, 5000);
                ApiInitializeHandler handler = new ApiInitializeHandler(newForm.ttNetApiInitHandler);
                TTAPI.CreateTTAPI(disp, apiconfig, handler);
                Application.Run(newForm);
                
            }
        }
    }
}