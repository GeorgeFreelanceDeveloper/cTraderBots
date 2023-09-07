using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


/*
Name: StopOut_cBot
Description: Bot for checking daily, weekly, monthly and overall PnL when PnL is above defined limits and if PnL is below defined limits, bot will close all pending orders and positions.
Author: GeorgeQuantAnalyst
CreateDate: 15.5.2023
UpdateDate: 1.9.2023
Version: 1.1.0
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class StopOut_cBot : Robot
    {

        // User defined properties
        [Parameter(DefaultValue = 10)]
        public double RiskPerTrade { get; set; }

        [Parameter(DefaultValue = 2)]
        public int MaxDailyDrawDownMultiplier { get; set; }

        [Parameter(DefaultValue = 3)]
        public int MaxWeeklyDrawDownMultiplier { get; set; }

        [Parameter(DefaultValue = 5)]
        public int MaxMonthlyDrawDownMultiplier { get; set; }

        [Parameter(DefaultValue = 10)]
        public int MaxDrawDownMultiplier { get; set; }

        // Computed properties
        private double MaxDailyDrawDownAmount { get; set; }
        private double MaxWeeklyDrawDownAmount { get; set; }
        private double MaxMonthlyDrawDownAmount { get; set; }
        private double MaxDrawDownAmount { get; set; }

        //Constants
        private readonly bool enableTrace = false;


        protected override void OnStart()
        {
            Print("Start StopOut_cBot");
            
            Print("User defined properties:");
            Print("RiskPerTrade: {0}", RiskPerTrade);
            Print("MaxDailyDrawDownMultiplier: {0}", MaxDailyDrawDownMultiplier);
            Print("MaxWeeklyDrawDownMultiplier: {0}", MaxWeeklyDrawDownMultiplier);
            Print("MaxMonthlyDrawDownMultiplier: {0}", MaxMonthlyDrawDownMultiplier);
            Print("MaxDrawDownMultiplier: {0}", MaxDrawDownMultiplier);
            
            Print("Validation of User defined properties ...");
            List<String> inputErrorMessages = ValidateInputs();
            inputErrorMessages.ForEach(m => Print(m));
            if (inputErrorMessages.Any()){
                Print("App contains input validation errors and will be stop.");
                Stop();
                return;
            }

            Print("Compute properties ...");
            MaxDailyDrawDownAmount = MaxDailyDrawDownMultiplier * RiskPerTrade * -1;
            MaxWeeklyDrawDownAmount = MaxWeeklyDrawDownMultiplier * RiskPerTrade * -1;
            MaxMonthlyDrawDownAmount = MaxMonthlyDrawDownMultiplier * RiskPerTrade * -1;
            MaxDrawDownAmount = MaxDrawDownMultiplier * RiskPerTrade * -1;

            Print("Computed properties:");
            Print("MaxDailyDrawDownAmount: {0}", MaxDailyDrawDownAmount);
            Print("MaxWeeklyDrawDownAmount: {0}", MaxWeeklyDrawDownAmount);
            Print("MaxMonthlyDrawDownAmount: {0}", MaxMonthlyDrawDownAmount);
            Print("MaxDrawDownAmount: {0}", MaxDrawDownAmount);
        }

        protected override void OnBar()
        {
            Print("Start to check if there is sufficient equity for trading");
            
            var dailyPnL = ComputeDailyPnL();
            var weeklyPnL = ComputeWeeklyPnL();
            var monthlyPnL = ComputeMonthlyPnL();
            var overallPnL = ComputeOverallPnL();

            if (dailyPnL < MaxDailyDrawDownAmount)
            {
                Print("Daily loss limit reached. [dailyPnL: {0}, MaxDailyDrawDownAmount: {1}]", dailyPnL, MaxDailyDrawDownAmount);
                Print("Start close all pending orders and positions");
                CloseAllPositionsAndPendingOrders();
                LocalStorage.SetObject("MaxDailyDrawDownReach",true, LocalStorageScope.Device);
                return;
            }

            if (weeklyPnL < MaxWeeklyDrawDownAmount)
            {
                Print("Weekly loss limit reached. [weeklyPnL: {0}, MaxWeeklyDrawDownAmount: {1}]", weeklyPnL, MaxWeeklyDrawDownAmount);
                Print("Start close all pending orders and positions");
                CloseAllPositionsAndPendingOrders();
                LocalStorage.SetObject("MaxWeeklyDrawDownReach",true, LocalStorageScope.Device);
                return;
            }

            if (monthlyPnL < MaxMonthlyDrawDownAmount)
            {
                Print("Monthly loss limit reached. [monthlyPnL: {0}, MaxMonthlyDrawDownAmount: {1}]", monthlyPnL, MaxMonthlyDrawDownAmount);
                Print("Start close all pending orders and positions");
                CloseAllPositionsAndPendingOrders();
                LocalStorage.SetObject("MaxMonthlyDrawDownReach",true, LocalStorageScope.Device);
                return;
            }

            if (overallPnL < MaxDrawDownAmount)
            {
                Print("Max drawdown limit reached. [overallPnL: {0}, MaxDrawDownAmount: {1}]", overallPnL, MaxDrawDownAmount);
                Print("Start close all pending orders and positions");
                CloseAllPositionsAndPendingOrders();
                LocalStorage.SetObject("MaxDrawDownReach",true, LocalStorageScope.Device);
                return;
            }

            if(enableTrace)
            {
                Print("PnLs:");
                Print("Daily PnL: {0}", dailyPnL);
                Print("Weekly PnL: {0}", weeklyPnL);
                Print("Monthly PnL: {0}", monthlyPnL);    
                Print("Overall PnL: {0}", overallPnL);
            }
            
            LocalStorage.SetObject("MaxDailyDrawDownReach",false, LocalStorageScope.Device);
            LocalStorage.SetObject("MaxWeeklyDrawDownReach",false, LocalStorageScope.Device);
            LocalStorage.SetObject("MaxMonthlyDrawDownReach",false, LocalStorageScope.Device);
            LocalStorage.SetObject("MaxDrawDownAmount",false, LocalStorageScope.Device);
            
            Print("Finished to check if there is sufficient equity for trading");
        }

        private void CloseAllPositionsAndPendingOrders()
        {
            Print("Start close all pending orders and positions");
            Positions.ToList().ForEach(p => ClosePosition(p));
            PendingOrders.ToList().ForEach(o => CancelPendingOrder(o));
            Print("Finished close all positions and pending orders");
        }

        private double ComputeDailyPnL()
        {
            DateTime startOfDay = DateTime.Now.Date;
            var dailyTrades = History.ToList().Where(trade => trade.ClosingTime >= startOfDay);
            return dailyTrades.Sum(trade => trade.NetProfit);
        }

        private double ComputeWeeklyPnL()
        {
            var today = DateTime.Now;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var weeklyTrades = History.ToList().Where(trade => trade.ClosingTime >= startOfWeek);
            return weeklyTrades.Sum(trade => trade.NetProfit);
        }

        private double ComputeMonthlyPnL()
        {
            var today = DateTime.Now;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var monthlyTrades = History.ToList().Where(trade => trade.ClosingTime >= startOfMonth);
            return monthlyTrades.Sum(trade => trade.NetProfit);
        }

        private double ComputeOverallPnL()
        {
            return History.Sum(trade => trade.NetProfit);
        }

        protected override void OnStop()
        {
            Print("Finished StopOut_cBot");
        }
        
        private List<String> ValidateInputs()
        {
            var errMessages = new List<String>();
            
            if (RiskPerTrade <= 0)
            {
                errMessages.Add(String.Format("WARNING: RiskPerTrade must be greater than 0. [RiskPerTrade: {0}]", RiskPerTrade));
            }
            
            if (MaxDailyDrawDownMultiplier <= 0)
            {
                errMessages.Add(String.Format("WARNING: MaxDailyDrawDownMultiplier must be greater than 0. [MaxDailyDrawDownMultiplier: {0}]", MaxDailyDrawDownMultiplier));
            }
            
            if (MaxWeeklyDrawDownMultiplier <= 0)
            {
                 errMessages.Add(String.Format("WARNING: MaxWeeklyDrawDownMultiplier must be greater than 0. [MaxWeeklyDrawDownMultiplier: {0}]", MaxWeeklyDrawDownMultiplier));
            }
            
            if (MaxMonthlyDrawDownMultiplier <= 0)
            {
                errMessages.Add(String.Format("WARNING: MaxMonthlyDrawDownMultiplier must be greater than 0. [MaxMonthlyDrawDownMultiplier: {0}]", MaxMonthlyDrawDownMultiplier));
            }

            if (MaxDrawDownMultiplier <= 0)
            {
                errMessages.Add(String.Format("WARNING: MaxDrawDownMultiplier must be greater than 0. [MaxDrawDownMultiplier: {0}]", MaxDrawDownMultiplier));
            }
            
            return errMessages;
        }
        
    }
}
