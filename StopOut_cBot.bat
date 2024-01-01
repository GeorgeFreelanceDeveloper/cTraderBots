::Parameters
set ctid="YOUR_EMAIL_ADDRESS"
set pwd-file="ctrader.pwd"
set account="YOUR_ACCOUNT_ID"
set symbol="EURUSD"
set period="m1"

::Environment-variables
set RiskPerTradePercentage="5"
set MaxDailyDrawDownMultiplier="2"
set MaxWeeklyDrawDownMultiplier="3"
set MaxMonthlyDrawDownMultiplier="5"
set MaxDrawDownMultiplier="10"

::ctrader bot
ctrader-cli.exe run C:\Users\Administrator\Documents\cAlgo\Sources\Robots\StopOut_cBot.algo --ctid=%ctid% --pwd-file=%pwd-file% --account=%account% --symbol=%symbol% --period=%period% --environment-variables --full-access --RiskPerTradePercentage=%RiskPerTradePercentage% --MaxDailyDrawDownMultiplier=%MaxDailyDrawDownMultiplier% --MaxWeeklyDrawDownMultiplier=%MaxWeeklyDrawDownMultiplier% --MaxMonthlyDrawDownMultiplier=%MaxMonthlyDrawDownMultiplier% --MaxDrawDownMultiplier=%MaxDrawDownMultiplier%
