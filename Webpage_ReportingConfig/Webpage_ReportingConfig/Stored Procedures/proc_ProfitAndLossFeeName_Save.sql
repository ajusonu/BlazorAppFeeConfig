----dependency: TABLE ProfitAndLossFeeName.sql
IF  OBJECT_ID('proc_ProfitAndLossFeeName_Save') IS NULL
    EXEC('CREATE PROCEDURE proc_ProfitAndLossFeeName_Save AS SET NOCOUNT ON;')
GO

ALTER  procedure proc_ProfitAndLossFeeName_Save
(
	@Id bigint, -- the id 
	@Name nvarchar(255),
	@Type int NULL
)
as
begin
	merge 
		into ProfitAndLossFeeName as target
		using (values (@id)) as source (id)
		on (target.id = source.id)
	-- insert new fees that are not found
	when not matched then
		insert (Name, Type, DateStamp)
		values (@Name, @Type, getdate())
	when matched then
		update
			set Name = @Name, Type = @Type, DateStamp = getdate() ;

	
end

go


GRANT EXECUTE ON proc_ProfitAndLossFeeName_Save TO BIReporting_Configurator_user
GRANT EXECUTE ON proc_ProfitAndLossFeeName_Save TO [svc-Bireporting]
	

Go
--Select * from ProfitAndLossFeeName

--insert into ProfitAndLossFeeName (Name, Type)		values ('test', 1)