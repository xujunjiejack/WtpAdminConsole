# WTP_Admin_Console

## Developed in: **C# 7.0**

## Description:
This small application is developed to migrate generated data in wtp_collab.db, SQLite database to our main MySQL database "wtp_data". Only the data files the collaborators make will be migrated back and get archived based on the table "user_table_trackers". Inside contains all of the meta information about who and when collaborators make the table. No other tables outside the collaborator-made tables will be modified during this process. After migration succeeds, the "user_table_trackers" table will be updated in the wtp_collab.db. It is scheduled to collect data from wtp_collab.db every week. 

## Dependency:
* log4net (A logging package for C#)
* System.Data.SQLite (ADO.NET package for using System.Data on SQLite)

## Usage:
Just double-click on "WtpAdminConsole.exe" or type in `WtpAdminConsole.exe` in cmd.

## Notice:
This script involves two different SQL implementation, MySQL and SQLite, so be prepared to switch between those two. This script also uses primarily ADO.NET. If you don't remember, check out the online tutorial. https://www.tutorialspoint.com/asp.net/asp.net_ado_net.htm

## Future:
It needs to have sort of automated testing script in NightWatch to detect whether the scripts have run successfully. 

> *Updated at 6/12 2018, by JJ*


