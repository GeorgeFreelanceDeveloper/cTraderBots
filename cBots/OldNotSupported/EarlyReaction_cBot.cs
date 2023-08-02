using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


/*
Name: EarlyReaction_cBot
Description: Bot checks whether there was a reaction to the trading level earlier, if so, it cancels the given trade.
Author: GeorgeQuantAnalyst
Date: 15.5.2023
Version: 1.0.0
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class EarlyReaction_cBot : Robot
    {
        
        [Parameter(DefaultValue = 0.33)]
        public double PercentageBeforeEntry { get; set; }
        
        private List<int> BeforeEntryIds { get; set;}

        protected override void OnStart()
        {
            BeforeEntryIds = new List<int>();
        }

        protected override void OnBar() 
        {
            Print("Start check early reaction.");
            
            RemoveNotExistsIds();
            
            foreach (var order in PendingOrders)
            {
                if (order.StopLoss == null || order.TakeProfit == null)
                {
                    Print("Order {0} does not have bracked orders (profit target and stop loss), continue to next order.", order.Id);
                    continue;
                }
                double beforeEntryPrice = order.TargetPrice + (((double) order.TargetPrice - (double) order.StopLoss) * PercentageBeforeEntry);
                
                Bar lastBar = MarketData.GetBars(TimeFrame.Minute, order.SymbolName).Last();

                if (!BeforeEntryIds.Contains(order.Id) && 
                (((order.TradeType == TradeType.Buy && beforeEntryPrice >= lastBar.Low)) || (order.TradeType == TradeType.Sell && beforeEntryPrice <= lastBar.High)))
                {
                    Print("Price arrived before entry: [OrderId: {0}, BeforeEntryPrice: {1}, LastBarLow: {2}, lastBarHigh: {3}]", order.Id, beforeEntryPrice, lastBar.Low, lastBar.High);
                    BeforeEntryIds.Add(order.Id);
                    continue;
                }
                
                if(BeforeEntryIds.Contains(order.Id) && 
                ((order.TradeType == TradeType.Buy && order.TakeProfit <= lastBar.High) || (order.TradeType == TradeType.Sell && order.TakeProfit >= lastBar.Low)))
                {
                    Print("Price arrived to TakeProfit price early after arrived BeforeEntryPrice, pending order will be cancel. [OrderId: {0}, TakeProfit: {1}, LastBarLow: {2}, lastBarHigh: {3}]", order.Id, order.TakeProfit, lastBar.Low, lastBar.High);
                    CancelPendingOrder(order);
                    BeforeEntryIds.Remove(order.Id);
                }
                
            }
            
            Print("Finished check early reaction.");
        }

        protected override void OnStop()
        {
        
        }
        
        private void RemoveNotExistsIds()
        {
            List<int> ids = new List<int>();
            List<int> notExistsIds = new List<int>();
            
            foreach (var order in PendingOrders)
            {
                ids.Add(order.Id);
            }
            
            foreach (var id in BeforeEntryIds)
            {
                if (!ids.Contains(id))
                {
                    notExistsIds.Add(id);
                }
            }
            
            foreach(var id in notExistsIds)
            {
                BeforeEntryIds.Remove(id);
            }
            
        }
    }
}