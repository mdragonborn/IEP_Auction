USE IepDb;

DROP TABLE IF EXISTS dbo.Balance;
DROP TABLE IF EXISTS dbo.BidAuction;
DROP TABLE IF EXISTS dbo.Reservation;
DROP TABLE IF EXISTS dbo.TokenOrders;
DROP TABLE IF EXISTS dbo.Auction;
DROP TABLE IF EXISTS dbo.Bid;
DROP TABLE IF EXISTS dbo.PortalParameters;

CREATE TABLE dbo.Bid
(
	Id bigint IDENTITY(1,1) PRIMARY KEY NOT NULL,
	Time datetime NOT NULL,
	Amount int,
	UserId nvarchar(128) NOT NULL,
	FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id)
);

CREATE TABLE dbo.Auction
	(Id uniqueidentifier PRIMARY KEY NOT NULL,
	CreatorId nvarchar(128) NOT NULL,
	Status nvarchar(10) NOT NULL DEFAULT 'READY',
	DurationMinutes int NOT NULL,
	TimeStart datetime,
	TimeEnd datetime,
	LastBidId bigint,
	Name nvarchar(128) NOT NULL,
	Description text NOT NULL,
	ImagePath nvarchar(256) NOT NULL,
	FOREIGN KEY(CreatorId) REFERENCES dbo.AspNetUsers(Id),
	FOREIGN KEY(LastBidId) REFERENCES Bid(Id));

CREATE TABLE dbo.BidAuction
(
	BidId bigint PRIMARY KEY NOT NULL,
	AuctionId uniqueidentifier
	FOREIGN KEY(BidId) REFERENCES Bid(Id),
	FOREIGN KEY(AuctionId) REFERENCES Auction(Id)
);

CREATE TABLE dbo.Balance
(
	Id nvarchar(128) PRIMARY KEY NOT NULL,
	Tokens bigint DEFAULT 0,
	FOREIGN KEY(Id) REFERENCES dbo.AspNetUsers(Id)
);

CREATE TABLE dbo.Reservation
(
	BidId bigint NOT NULL,
	UserId nvarchar(128) NOT NULL,
	Amount bigint DEFAULT 0,
	PRIMARY KEY(BidId, UserId),
	FOREIGN KEY(BidId) REFERENCES Bid(Id),
	FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id)
);

CREATE TABLE dbo.TokenOrders
(
	Id uniqueidentifier PRIMARY KEY,
	Status nvarchar(10) DEFAULT 'SUBMITTED',
	UserId nvarchar(128) NOT NULL,
	Amount bigint NOT NULL,
	Value float NOT NULL,
	Currency nvarchar(3) NOT NULL,
	Time datetime NOT NULL,
	FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id)
);

CREATE TABLE dbo.PortalParameters
(
	Name nvarchar(128) PRIMARY KEY,
	Type nvarchar(128),
	NumValue float,
	StrValue nvarchar(128),
);

