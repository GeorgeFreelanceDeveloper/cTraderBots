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
Description: Bot checking for sufficient equity for trading, if the equity falls below the set daily, weekly, monthly and all limits, all positions and pending orders will be terminated.
Author: GeorgeQuantAnalyst
Date: 15.5.2023
Update: 1.9.2023
Version: 1.1.0-SNAPSHOT
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
        public int MaxyDrawDownMultiplier { get; set; }

        // Computed properties
        private double MaxDailyDrawDownAmount { get; set; }
        private double MaxWeeklyDrawDownAmount { get; set; }
        private double MaxMonthlyDrawDownAmount { get; set; }
        private double MaxDrawDownAmount { get; set; }


        protected override void OnStart()
        {
            Print("Start CommoditiesLevelTrader_cBot");

            Print("User defined properties:");
            Print("RiskPerTrade: {0}", RiskPerTrade);
            Print("MaxDailyDrawDownMultiplier: {0}", MaxDailyDrawDownMultiplier);
            Print("MaxWeeklyDrawDownMultiplier: {0}", MaxWeeklyDrawDownMultiplier);
            Print("MaxMonthlyDrawDownMultiplier: {0}", MaxMonthlyDrawDownMultiplier);
            Print("MaxyDrawDownMultiplier: {0}", MaxyDrawDownMultiplier);

            Print("Compute properties ...");
            MaxDailyDrawDownAmount = MaxDailyDrawDownMultiplier * RiskPerTrade * -1;
            MaxWeeklyDrawDownAmount = MaxWeeklyDrawDownMultiplier * RiskPerTrade * -1;
            MaxMonthlyDrawDownAmount = MaxMonthlyDrawDownMultiplier * RiskPerTrade * -1;
            MaxDrawDownAmount = MaxyDrawDownMultiplier * RiskPerTrade * -1;

            Print("Computed properties:");
            Print("MaxDailyDrawDownAmount: {0}", MaxDailyDrawDownAmount);
            Print("MaxWeeklyDrawDownAmount: {0}", MaxWeeklyDrawDownAmount);
            Print("MaxMonthlyDrawDownAmount: {0}", MaxMonthlyDrawDownAmount);
            Print("MaxDrawDownAmount: {0}", MaxDrawDownAmount);

            Print("PnLs:");
            Print(History.Sum(x=>x.NetProfit));
            Print("Daily PnL: {0}", ComputeDailyPnL());
            Print("Weekly PnL: {0}", ComputeWeeklyPnL());
            Print("Monthly PnL: {0}", ComputeMonthlyPnL());    
            Print("Overall PnL: {0}", ComputeOverallPnL());
            
        }

        protected override void OnBar()
        {
            Print("Start check sufficient equity for trading");
            
            var dailyPnL = ComputeDailyPnL();
            var weeklyPnL = ComputeWeeklyPnL();
            var monthlyPnL = ComputeMonthlyPnL();
            var overallPnL = ComputeOverallPnL();

            if (dailyPnL < MaxDailyDrawDownAmount)
            {
                Print("Daily loss limit reached. [dailyPnL: {0}, MaxDailyDrawDownAmount: {1}]", dailyPnL, MaxDailyDrawDownAmount);
                Print("Start close all pending orders and positions");
                CloseAllPositionsAndPendingOrders();
                return;
            }

            if (weeklyPnL < MaxWeeklyDrawDownAmount)
            {
                Print("Weekly loss limit reached. [weeklyPnL: {0}, MaxDailyDrawDownAmount: {1}]", weeklyPnL, MaxWeeklyDrawDownAmount);
                Print("Start close all pending orders and positions");
                CloseAllPositionsAndPendingOrders();
                return;
            }

            //TODO: Lucka check monthly and overall PnL



            Print("Finished check sufficient equity for trading");
        }

        private void CloseAllPositionsAndPendingOrders()
        {
            Print("Start close all pending orders and positions");
            Positions.ToList().ForEach(p => ClosePosition(p));
            PendingOrders.ToList().ForEach(o => CancelPendingOrder(o));
            Print("Finished close all pending orders and positions");
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
            Print("Finished CommoditiesLevelTrader_cBot");
        }
    }
}
