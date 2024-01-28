::Parameters
set ctid="YOUR_EMAIL_ADDRESS"
set pwd-file="ctrader.pwd"
set account="YOUR_ACCOUNT_ID"
set symbol="US500"
set period="m1"

::Environment-variables
set CountPeriodForEntry1=20
set CountPeriodForEntry2=55
set CountPeriodForStop1=10
set CountPeriodForStop2=20
set EnableL1=True
set EnableL2=False
set RiskPercentage=2.5
set LongOnly=True

::ctrader bot
ctrader-cli.exe run C:\Users\Administrator\Documents\cAlgo\Sources\Robots\TurtleTrendFollow_cBot.algo --ctid=%ctid% --pwd-file=%pwd-file% --account=%account% --symbol=%symbol% --period=%period% --environment-variables --full-access --CountPeriodForEntry1=%CountPeriodForEntry1% --CountPeriodForEntry2=%CountPeriodForEntry2% --CountPeriodForStop1=%CountPeriodForStop1% --CountPeriodForStop2=%CountPeriodForStop2% --EnableL1=%EnableL1% --EnableL2=%EnableL2% --RiskPercentage=%RiskPercentage% --LongOnly=%LongOnly%
