using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.IO;


/*
Name: PlacePendingOrders_cBot
Description: Bot compute profit targes, amount orders and place pending ordes to trading platform.
Author: GeorgeQuantAnalyst
Date: 15.5.2023
Version: 0.1.0
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class PlacePendingOrders_cBot : Robot
    {
    
        [Parameter(DefaultValue = @"C:\Users\Administrator\Desktop\ForexSwingTrades.csv")]
        public string TradesFilePath { get; set; }
        
        [Parameter(DefaultValue = true)]
        public bool HasTrailingStop { get; set; }
        
        class PrepareOrder
        {
            public string Ticker { get; set; }
            public string Direction { get; set; }
            public double EntryPrice { get; set; }
            public double StopLossPrice { get; set; }
            public double ProfitTargetPrice { get; set; }
            public double Amount { get; set; }
        }

        protected override void OnStart()
        {
            List<string[]> dataList = Read_CSV_FromFile(TradesFilePath);
            List<PrepareOrder> prepareOrders = convertToListOfPrepareOrder(dataList);
            
            foreach (var prepareOrder in prepareOrders)
            {
                prepareOrder.ProfitTargetPrice = prepareOrder.EntryPrice + (prepareOrder.EntryPrice - prepareOrder.StopLossPrice);
                /*
                TODO: @Geoerge: Slozite nevim jak dal implementovat mena uctu se muze lisit, pip value...
                Pro výpočet hodnoty jednoho pipu pro měnový páru musíte nejprve zjistit aktuální kotaci tohoto měnového páru a hodnotu jednoho pipu v jednotkách měny vašeho obchodního účtu.
                Pip hodnota je definována jako nejmenší jednotka změny výměnného kurzu daného měnového páru, a proto se může lišit v závislosti na měnovém páru. Například pip hodnota pro měnový pár EUR/USD je obvykle 0,0001 USD, zatímco pip hodnota pro měnový pár USD/JPY je obvykle 0,01 JPY.
                */
                prepareOrder.Amount = 0;
                
            }
            
            Stop();
        }
        
        private List<string[]> Read_CSV_FromFile(string csvPath)
        {
            List<String[]> dataList = new List<string[]>();
            
            using (StreamReader reader = new StreamReader(csvPath))
            {   
                
                //Skip first line with header
                reader.ReadLine();
                
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(",");
                    
                    dataList.Add(values);
                }
            }
            
            return dataList;
        }
        
        private List<PrepareOrder> convertToListOfPrepareOrder(List<string[]> dataList)
        {
            List<PrepareOrder> result = new List<PrepareOrder>();
            
            foreach (var row in dataList)
            {
                PrepareOrder prepareOrder = new PrepareOrder();
                prepareOrder.Ticker = row[0];
                prepareOrder.Direction = row[1];
                prepareOrder.EntryPrice = Convert.ToDouble(row[2]);
                prepareOrder.StopLossPrice = Convert.ToDouble(row[3]);
                result.Add(prepareOrder);
            }
            
            return result;
        }


    }
}
