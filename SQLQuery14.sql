/****** Script for SelectTopNRows command from SSMS  ******/
INSERT INTO [dbo].[PortalParameters] VALUES ('Gold', 'TokenPack',50, 50);
INSERT INTO [dbo].[PortalParameters] VALUES ('Silver', 'TokenPack',10, 10);
INSERT INTO [dbo].[PortalParameters] VALUES ('Platinum', 'TokenPack',100, 100);
INSERT INTO [dbo].[PortalParameters] VALUES ('Page','PageSize',10, null);
INSERT INTO [dbo].[PortalParameters] VALUES ('USD', 'Currency', 1.21, 'USD');


drop table balance;
drop table bidauction;
drop table reservation;
drop table tokenorders;
drop table auction;
drop table bid;
