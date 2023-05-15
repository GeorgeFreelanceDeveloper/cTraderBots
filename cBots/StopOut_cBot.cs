using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


/*
Name: StopOut_cBot
Description: Bot checking for sufficient equity for trading, if the equity falls below the set limit, all positions and pending orders will be terminated.
Author: GeorgeQuantAnalyst
Date: 15.5.2023
Version: 0.1.0
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class StopOut_cBot : Robot
    {
        
        [Parameter(DefaultValue = 9800)]
        public double EquityStopOut { get; set; }

        protected override void OnStart()
        {
            Print("EquityStopOut is set on: {0}", EquityStopOut);
        }
        
        protected override void OnBar() 
        {
            Print("Check sufficient equity for trading");
            if (Account.Equity > EquityStopOut)
            {
                Print("Sufficient equity for trading. [Equity: {0}, EquityStopOut: {1}]", Account.Equity, EquityStopOut);
            }
            else
            {
                Print("Unsufficient equity for trading. [Equity: {0}, EquityStopOut: {1}]", Account.Equity, EquityStopOut);
                Print("Start close all pending orders and positions");
                
                foreach (var position in Positions)
                {
                    ClosePosition(position);
                }
                
                foreach (var order in PendingOrders)
                {
                    CancelPendingOrder(order);
                }
            
                Print("Finished close all pending orders and positions");
            }
            
        }
        

        protected override void OnStop()
        {

        }
    }
}
