using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


/*
Name: CloseTradesAtTime_cBot
Description: Bot closing pending orders and position at defined time. You can set close for all or specific currency pairs.
Author: GeorgeQuantAnalyst
Date: 15.5.2023
Version: 0.1.0
*/

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.CentralEuropeStandardTime, AccessRights = AccessRights.None)]
    public class CloseTradesAtTime_cBot : Robot
    {
        
        [Parameter(DefaultValue = 19)]
        public int Hours {get; set;}
    
        [Parameter(DefaultValue = 0)]
        public int Minutes {get; set;}
        
        [Parameter(DefaultValue = false)]
        public bool All {get; set;}
        
        [Parameter(DefaultValue = "USD")]
        public string Currency {get; set;}
        
        
        protected override void OnStart()
        {
            Print("Start timer");

            DateTime closeTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Hours, Minutes, 0);
            TimeSpan interval = closeTime.Subtract(DateTime.Now);
            Timer.Start(interval);    
        }
        
        protected override void OnTimer()
        {
             Print("Start closing orders and positions");

             if(All)
             {
                CancelOrdersAndPositionsForAllCurrencies();
             } 
             else
             {
                CancelOrdersAndPositions(Currency);
             }
             
             Timer.Stop();
             Stop();
        }
        
        private void CancelOrdersAndPositionsForAllCurrencies()
        {
            Print("Cancel all orders and positions for all currencies");
            var pendingOrders = PendingOrders;
            foreach (var order in pendingOrders)
            {
                CancelPendingOrder(order);
            }
            
            var openPositions = Positions;
            foreach (var position in openPositions)
            {
                 ClosePosition(position);
            }
        }
        
         private void CancelOrdersAndPositions(string currency)
        {
            Print("Cancel orders and positions for {} currency pairs", currency);
            var pendingOrders = PendingOrders;
            foreach (var order in pendingOrders)
            {
                if (order.SymbolName.Contains(currency))
                {
                    CancelPendingOrder(order);
                }       
            }
            
            var openPositions = Positions;
            foreach (var position in openPositions)
            {
                if (position.SymbolName.Contains(currency))
                {
                    ClosePosition(position);
                }           
            }
        }
        
    }
}
