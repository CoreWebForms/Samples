# Samples
## WingtipToys (Linux)
1. To run WingtipToys in Linux, please attach the ```wingtiptoys.mdf``` file in the ```App_Data``` folder to SQL Server instance [(reference)](https://learn.microsoft.com/en-us/sql/relational-databases/databases/attach-a-database?view=sql-server-ver16#SSMSProcedure). 
2. Uncomment the line ```SetDefaultConnectionFactory``` under ```Model/ProductDbConfiguration.cs``` and provide the correct connection string for above SQL Server instance.