----dependency: TABLE ProfitAndLossFee.sql
IF  OBJECT_ID('proc_ProfitAndLossFee_Save') IS NULL
    EXEC('CREATE PROCEDURE proc_ProfitAndLossFee_Save AS SET NOCOUNT ON;')
GO

ALTER  procedure proc_ProfitAndLossFee_Save
(
	@Id bigint, -- the id 
	@Brand int,
	@FeeNameId int,
	@FeePercentage decimal(12, 4)=0,
	@FeeValue decimal(12, 4)=0
)
as
begin
	merge 
		into ProfitAndLossFee as target
		using (values (@id)) as source (id)
		on (target.id = source.id)
	-- insert new fees that are not found
	when not matched then
		insert (Brand, FeeNameId, FeePercentage, FeeValue)
		values (@Brand, @FeeNameId, @FeePercentage, @FeeValue)
	when matched then
		update
			set Brand = @Brand, FeeNameId = @FeeNameId, FeePercentage = @FeePercentage, FeeValue = @FeeValue ;

	
end

go


GRANT EXECUTE ON proc_ProfitAndLossFee_Save TO BIReporting_Configurator_user
GRANT EXECUTE ON proc_ProfitAndLossFee_Save TO [svc-Bireporting]
	

Go
--Select * from ProfitAndLossFee

--insert into ProfitAndLossFeeName (Name, Type)		values ('test', 1)