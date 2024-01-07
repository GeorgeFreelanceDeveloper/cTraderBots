# cTraderBots
cTrader bots for automation tasks in trading (place pending orders and manage risk) on FX and commmodities market.

## What is cTrader Automate
cTrader Automate is a feature of cTrader desktop. It allows for developing and operating custom trading indicators and cBots.

## What is a cBot
Think of cBots as programs that run inside cTrader desktop and autonomously execute and manage trading operations. cBots can be designed to perform almost any trading activity such as opening positions or placing orders depending on certain conditions.

## Developed cBots
**Actively supported**
* **LevelTrader_cBot**: An automated bot for controlling trades. The bot helps reduce risk by adjusting positions when prices move favorably, cancel pending order when trade early reaction and eliminates open orders during sudden price spikes.
* **StopOut_cBot**: Bot for checking daily, weekly, monthly and overall PnL when PnL is above defined limits and if PnL is below defined limits, bot will close all pending orders and positions.
* **CloseTrades_cBot**: Bot closing pending orders and opened positions at defined date and time. You can set close for all trades or specific market.
* **GapFinder_cBot**: Bot for finding the top 20 largest daily gaps in the selected market.
* **TurtleTrendFollow_cBot**: An automated bot for trend following strategy Turtle.

## Turtle strategy
The Turtle Trading strategy was developed by Richard Dennis and William Eckhardt in the early 1980s. The story goes that Richard Dennis, a successful commodity trader, believed that trading could be taught to anyone, and he decided to conduct an experiment to prove his theory. He recruited a group of people, known as the "Turtles," and taught them his trading system.

Here's an overview of the Turtle Trading strategy:

* **Trend Following**: The core principle of the Turtle Trading strategy is trend following. The idea is to identify and ride strong trends in the financial markets. Turtles were taught to buy and hold futures contracts for markets that were in a strong uptrend and to sell short and hold contracts for markets in a strong downtrend.

* **Entry Signals**: Turtles used a breakout system to enter trades. The entry signal was based on the 20-day and 55-day price highs or lows. If the price exceeded the 20-day high, it was a signal to go long, and if it fell below the 20-day low, it was a signal to go short. Additionally, they used the 55-day high/low as a filter to confirm the strength of the trend.

* **Position Sizing**: Dennis and Eckhardt emphasized the importance of position sizing in their strategy. Turtles were taught to determine the size of their positions based on the volatility of the market. The more volatile the market, the smaller the position size, and vice versa. This risk management approach aimed to protect the traders from significant losses during adverse market conditions.

* **Exit Strategy**: The Turtles had a defined exit strategy to manage their trades. They used a 10-day low/high as a trailing stop to protect their profits and limit losses. If the market reversed and hit the 10-day low after a long position or the 10-day high after a short position, it was a signal to exit the trade and 20-days used for long-term entry break 50-days high or low.

* **Diversification**: The Turtle Trading system involved trading a diversified portfolio of markets, including commodities, currencies, and financial futures. This diversification was intended to spread risk and take advantage of trends in various markets.

## Development
Application is actively maintenance and develop.

## Prerequisites
* [Windows Server 2022 download link](https://www.microsoft.com/en-us/evalcenter/download-windows-server-2022)
* [.NET Framework download link](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
* [cTrader download link](https://ctrader.com/download/)

## C# base tutorials
* [C# introduction webinar - freeCodeCamp.org](https://youtu.be/GhQdlIFylQ8)
* [C# programming quide - microsoft.com](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/)
* [C# - learnxinyminutes.com](https://learnxinyminutes.com/docs/csharp/)

## Ctrader cBot documentation
* [ctrader.com - documentation](https://help.ctrader.com/ctrader-automate/)
* [ctrader.com - references](https://help.ctrader.com/ctrader-automate/references/)

## Contributors
<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
     <td align="center"><a href="https://github.com/GeorgeFreelanceDeveloper"><img src="https://avatars.githubusercontent.com/u/112611533?v=4" width="100px;" alt=""/><br /><sub><b>GeorgeFreelanceDeveloper</b></sub></a><br /><a href="https://github.com/GeorgeFreelanceDeveloper" title="Ideas">🤔</a></td>
    <td align="center"><a href="https://github.com/LucyFreelanceDeveloper"><img src="https://avatars.githubusercontent.com/u/115091833?v=4" width="100px;" alt=""/><br /><sub><b>LucyFreelanceDeveloper</b></sub></a><br /><a href="https://github.com/LucyFreelanceDeveloper" title="Code">💻</a></td>
  </tr>
</table>
