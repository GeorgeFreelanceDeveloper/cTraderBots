using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

/*
Name: MaxOpenPositions_cBot
Description: Bot controlling the maximum number of open positions, if more than allowed limit, the newer position will be closed. 
Author: GeorgeQuantAnalyst
Date: 15.5.2023
Version: 0.1.0
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MaxOpenPositions_cBot : Robot
    {
        [Parameter(DefaultValue = 2)]
        public int MaxAllowedOpenPositions { get; set; }

        
        protected override void OnStart()
        {

        }

        protected override void OnBar()
        {
            Print("Check open more positions than max allowed position: {0}", MaxAllowedOpenPositions);
            
            if (Positions.Count > MaxAllowedOpenPositions)
            {
                Print("Open more positions than max allowed positions. [OpenPositions: {0}, MaxAllowedOpenPositions: {1}]", 
                Positions.Count, MaxAllowedOpenPositions);
                
                List<Position> positions = Positions.ToList();
                
                // Sort positions by EntryTime desc
                positions.Sort((item1, item2) => item1.EntryTime.CompareTo(item2.EntryTime));
                positions.RemoveRange(0, MaxAllowedOpenPositions);
                
                Print("Close other positions");
                foreach (var position in positions)
                {
                    ClosePosition(position);
                }
            }
        }

        protected override void OnStop()
        {
        
        }
        
    }
}
