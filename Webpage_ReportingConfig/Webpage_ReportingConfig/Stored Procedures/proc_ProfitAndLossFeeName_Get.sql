----dependency: TABLE ProfitAndLossFee.sql
IF  OBJECT_ID('proc_ProfitAndLossFeeName_Get') IS NULL
    EXEC('CREATE PROCEDURE proc_ProfitAndLossFeeName_Get AS SET NOCOUNT ON;')
GO

ALTER procedure proc_ProfitAndLossFeeName_Get
(
	@Id bigint=0
)
as
begin

	Select * from ProfitAndLossFeeName where ( nullif(@Id, 0) is null OR Id = @Id) order by Name
	
end

go

GRANT EXECUTE ON proc_ProfitAndLossFeeName_Get TO BIReporting_Configurator_user
GRANT EXECUTE ON proc_ProfitAndLossFeeName_Get TO [svc-Bireporting]
	

GO 
--exec proc_ProfitAndLossFeeName_Get null