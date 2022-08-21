USE DealDB

CREATE TYPE UpdateDealType AS TABLE
(
	Id uniqueidentifier,
	WoodVolumeBuyer decimal(15, 4),
	WoodVolumeSeller decimal(15, 4),
	DealDate DateTime2(7),
	IsDealCorrect bit
);

GO
CREATE PROCEDURE UpdateDeals(@Deals UpdateDealType readonly)
AS
BEGIN TRANSACTION
	DECLARE
	@Id uniqueidentifier,
	@WoodVolumeBuyer decimal(15, 4),
	@WoodVolumeSeller decimal(15, 4),
	@DealDate DateTime2(7),
	@IsDealCorrect bit;

	DECLARE dealCursor CURSOR LOCAL FORWARD_ONLY 
    FOR
    SELECT Id, WoodVolumeBuyer, WoodVolumeSeller, DealDate, IsDealCorrect
    FROM @Deals;

    OPEN dealCursor;
    FETCH dealCursor INTO @Id, @WoodVolumeBuyer, @WoodVolumeSeller, @DealDate, @IsDealCorrect;
    WHILE @@FETCH_STATUS = 0
    BEGIN
	    UPDATE Deal
	    SET WoodVolumeBuyer = @WoodVolumeBuyer, WoodVolumeSeller = @WoodVolumeSeller,
			IsDealCorrect = @IsDealCorrect, DealDate = @DealDate
	    WHERE Id = @Id;
		
	    FETCH NEXT FROM dealCursor INTO @Id, @WoodVolumeBuyer, @WoodVolumeSeller, @DealDate, @IsDealCorrect;
    END;
    CLOSE dealCursor;
    DEALLOCATE dealCursor;
COMMIT;

CREATE TYPE UpdateCompanyType AS TABLE
(
	Id int,
	CompanyName nvarchar(1000)
);

GO
CREATE PROCEDURE UpdateCompanies(@Companies UpdateCompanyType readonly)
AS
BEGIN TRANSACTION
	DECLARE
	@Id int,
	@CompanyName nvarchar(1000);

	DECLARE dealCursor CURSOR LOCAL FORWARD_ONLY 
    FOR
    SELECT Id, CompanyName
    FROM @Companies;

    OPEN dealCursor;
    FETCH dealCursor INTO @Id, @CompanyName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
	    UPDATE Company
	    SET Name = @CompanyName
	    WHERE Id = @Id;
		
	    FETCH NEXT FROM dealCursor INTO @Id, @CompanyName;
    END;
    CLOSE dealCursor;
    DEALLOCATE dealCursor;
COMMIT;