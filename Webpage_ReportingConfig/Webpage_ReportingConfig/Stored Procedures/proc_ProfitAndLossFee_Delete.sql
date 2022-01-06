----dependency: TABLE ProfitAndLossFee.sql
IF  OBJECT_ID('proc_ProfitAndLossFee_Delete') IS NULL
    EXEC('CREATE PROCEDURE proc_ProfitAndLossFee_Delete AS SET NOCOUNT ON;')
GO

ALTER  procedure proc_ProfitAndLossFee_Delete
(
	@Ids varchar(1000)
)
as
begin
	Delete p from ProfitAndLossFee p  join [dbo].[SplitStringWithId](@ids, ',') i on p.id = i.items
	
end

go


GRANT EXECUTE ON proc_ProfitAndLossFee_Delete TO BIReporting_Configurator_user
GRANT EXECUTE ON proc_ProfitAndLossFee_Delete TO [svc-Bireporting]
	