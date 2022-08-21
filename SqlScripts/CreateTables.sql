USE [DealDB] 
GO

CREATE TABLE Company
(
    Id integer IDENTITY(1, 1) PRIMARY KEY, 
	INN nvarchar(50),
	Name nvarchar(1000),
	IsRussianINN bit -- 1 - is correct, 0 - is incorrect
);

INSERT INTO Company (INN, Name, IsRussianINN)
VALUES ('', N'Физ лицо', 0)

CREATE TABLE Deal
(
    Id uniqueidentifier PRIMARY KEY, 
	DealNumber nvarchar(40) NOT NULL,
	SellerId integer FOREIGN KEY REFERENCES Company(Id) NOT NULL,
	BuyerId integer FOREIGN KEY REFERENCES Company(Id) NOT NULL,
	WoodVolumeBuyer decimal(15, 4) NOT NULL,
	WoodVolumeSeller decimal(15, 4) NOT NULL,
	DealDate DateTime2 NULL,
	IsDealCorrect bit
);

CREATE TABLE NameShortage
(
	FullName nvarchar(100) PRIMARY KEY,
	ShortName nvarchar(10)
);


INSERT INTO NameShortage (FullName, ShortName)
VALUES
(N'ФЕДЕРАЛЬНОЕ ГОСУДАРСТВЕННОЕ БЮДЖЕТНОЕ УЧРЕЖДЕНИЕ ', N'ФГБУ'),
(N'АВТОНОМНОЕ УЧРЕЖДЕНИЕ', N'АУ'),
(N'МУНИЦИПАЛЬНОЕ КАЗЕННОЕ УЧРЕЖДЕНИЕ', N'МКУ'),
(N'МУНИЦИПАЛЬНОЕ АВТОНОМНОЕ УЧРЕЖДЕНИЕ', N'МАУ'),
(N'ФЕДЕРАЛЬНОЕ КАЗЕННОЕ УЧРЕЖДЕНИЕ', N'ФКУ'),
(N'ТОРГОВО-ЭКОНОМИЧЕСКАЯ КОМПАНИЯ', N'ТЭК'),
(N'Государственное автономное учреждение', N'ГАУ'),
(N'АКЦИОНЕРНОЕ ОБЩЕСТВО', N'АО'),
(N'ИНДИВИДУАЛЬНЫЙ ПРЕДПРИНИМАТЕЛЬ', N'ИП'),
(N'закрытое акционерное общество', N'ЗАО'),
(N'Специализированное государственное бюджетное учреждение', N'СГБУ'),
(N'Государственное бюджетное учреждение', N'ГБУ'),
(N'государственное учреждение', N'ГУ'),
(N'МУНИЦИПАЛЬНОЕ БЮДЖЕТНОЕ ДОШКОЛЬНОЕ ОБРАЗОВАТЕЛЬНОЕ УЧРЕЖДЕНИЕ', N'МБДШОУ'),
(N'ОБЩЕСТВО С ОГРАНИЧЕННОЙ ОТВЕТСТВЕННОСТЬЮ', N'ООО'),
(N'ОТКРЫТОЕ АКЦИОНЕРНОЕ ОБЩЕСТВО', N'ОАО'),
(N'Физическое лицо', N'Физ лицо')