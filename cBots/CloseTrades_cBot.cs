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
Version: 1.0.1
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class CloseTrades_cBot : Robot
    {   
        // User defined properties
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

        // Constants
        private TimeOnly CloseTime {get; set;}
        
        protected override void OnStart()
        {
            Print("Start CloseTrades_cBot");

            Print("User defined properties:");
            Print(String.Format("Day: {0}", Day));
            Print(String.Format("Hours: {0}", Hours));
            Print(String.Format("Minutes: {0}", Minutes));
            Print(String.Format("All: {0}", All));
            Print(String.Format("Market: {0}", Market));
            
            Print("Validation of User defined properties ...");
            List<String> inputErrorMessages = ValidateInputs();
            inputErrorMessages.ForEach(m => Print(m));
            if (inputErrorMessages.Any()){
                Print("App contains input validation errors and will be stop.");
                Stop();
                return;
            }
            Print("Finished validation of User defined properties ...");
            
            Print("Compute properties ...");
            CloseTime = new TimeOnly(Hours, Minutes);

            Print("Computed properties:");
            Print("CloseTime {0}", CloseTime);
        }

        protected override void OnBar()
        {
            Print(String.Format("{0}: Start onBar step", DateTime.Now));
             
            DateTime now = DateTime.Now;
             
            if(now.DayOfWeek == Day && TimeOnly.FromDateTime(now) >= CloseTime)
            {
               CancelOrdersAndPositions(All, Market);
            }
            
            Print(String.Format("{0}: Finished onBar step", DateTime.Now));
        }
        
        private void CancelOrdersAndPositions(bool all, string market)
        {
            Print("Start closing orders and positions");
            
            PendingOrders.Where(order => all || order.SymbolName == market)
                .ToList()
                .ForEach(order => CancelPendingOrder(order));
                
            Positions.Where(position => all || position.SymbolName == market)
                .ToList()
                .ForEach(position => ClosePosition(position));
                
            Print("Finished closing orders and positions");     
        }
        
        private List<String> ValidateInputs()
        {
            var errMessages = new List<String>();

            if (Hours < 0 || Hours > 24)
            {
                errMessages.Add(String.Format("WARNING: Hours must be defined and greater or equal to 0 and less or equal to 24. [Hours: {0}]", Hours));
            }
            
            if (Minutes < 0 || Minutes > 60)
            {
                 errMessages.Add(String.Format("WARNING: Minutes must be defined and greater or equal to 0 and less or equal to 60. [Minutes: {0}]", Minutes));
            }
            
            if (!All && (Market == null || Market == ""))
            {
                errMessages.Add(String.Format("WARNING: Market must be defined. [Market: {0}]", Market));
            }
            
            return errMessages;
        }
    }
}
