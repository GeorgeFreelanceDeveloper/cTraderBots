using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

/*
Name: CloseTrades_cBot
Description: Bot closing pending orders and opened positions at defined date and time. You can set close for all trades or specific market.
Author: LucyQuantAnalyst
Date: 29.10.2023
Version: 1.0.0
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class CloseTrades_cBot : Robot
    {   
        [Parameter(DefaultValue = DayOfWeek.Friday)]
        public DayOfWeek Day {get; set;}

        [Parameter(DefaultValue = 20)]
        public int Hours {get; set;}

        [Parameter(DefaultValue = 0)]
        public int Minutes {get; set;}

        [Parameter(DefaultValue = true)]
        public bool All {get; set;}

        [Parameter(DefaultValue = "Copper")]
        public string Market {get; set;}

        private TimeOnly CloseTime {get; set;}
        
        protected override void OnStart()
        {
            Print("Start CloseTrades_cBot");
        }

        protected override void OnBar()
        {
            Print("Start onBar step");

            CloseTime = new TimeOnly(Hours, Minutes);
             
            Print("Close time: {0}", CloseTime);
            Print("Day time now: {0}", DateTime.Now);
             
            DateTime now = DateTime.Now;
             
            if(now.DayOfWeek == Day)
            {
               
               Print("Start closing orders and positions");
               
               if(TimeOnly.FromDateTime(now) >= CloseTime)
               {
                    if(All)
                    {
                        CancelOrdersAndPositionsForAllMarkets();
                    }
                    else
                    {
                        CancelOrdersAndPositions(Market);
                    }
               }
               
               Print("Finished closing orders and positions");
            }
            
            Print("Finished onBar step"); 
        }
        
        private void CancelOrdersAndPositionsForAllMarkets()
        {
            Print("Cancel all orders and positions for all markets");
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
        
        private void CancelOrdersAndPositions(string market)
        {
            Print("Cancel orders and positions for {0} market.", market);
            var pendingOrders = PendingOrders;
            foreach (var order in pendingOrders)
            {
                if (order.SymbolName.SequenceEqual(market))
                {
                    CancelPendingOrder(order);
                }
            }
            
            var openPositions = Positions;
            foreach (var position in openPositions)
            {
                if (position.SymbolName.SequenceEqual(market))
                {
                    ClosePosition(position);
                }
            }
         }
    }
}