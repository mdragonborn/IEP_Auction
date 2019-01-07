USE IepDb;

CREATE TABLE dbo.Bid
(
	Id bigint IDENTITY(1,1) PRIMARY KEY NOT NULL,
	Time timestamp NOT NULL,
	Amount int,
	UserId nvarchar(128) NOT NULL,
	FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id)
);

CREATE TABLE dbo.Auction
	(Id uniqueidentifier PRIMARY KEY NOT NULL,
	CreatorId nvarchar(128) NOT NULL,
	Status nvarchar(10) NOT NULL DEFAULT 'OPENED',
	TimeStart timestamp NOT NULL,
	TimeEnd time NOT NULL,
	LastBidId bigint,
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
	Status nvarchar(10),
	UserId nvarchar(128) NOT NULL,
	Amount bigint NOT NULL,
	Time timestamp NOT NULL,
	FOREIGN KEY(UserId) REFERENCES dbo.AspNetUsers(Id)
)

