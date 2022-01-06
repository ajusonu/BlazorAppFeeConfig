----dependency: TABLE ProfitAndLossFee.sql
----dependency: TABLE ProfitAndLossFeeName.sql
IF  OBJECT_ID('proc_ProfitAndLossFee_Get') IS NULL
    EXEC('CREATE PROCEDURE proc_ProfitAndLossFee_Get AS SET NOCOUNT ON;')
GO

ALTER procedure proc_ProfitAndLossFee_Get
(
	@Id bigint=0,
	@SearchText varchar(50)=''
)
as
begin

	Select p.*, n.name, n.type from ProfitAndLossFee p  join ProfitAndLossFeeName n on p.FeeNameId = n.id  where ( nullif(@Id, 0) is null OR p.Id = @Id)
	 and (rtrim(@SearchText) = '' or patindex('%'+rtrim(@SearchText)+'%', n.name) > 0)
	 order by n.name
	
end

go

GRANT EXECUTE ON proc_ProfitAndLossFee_Get TO BIReporting_Configurator_user
GRANT EXECUTE ON proc_ProfitAndLossFee_Get TO [svc-Bireporting]
	

GO 
--exec proc_ProfitAndLossFee_Get null