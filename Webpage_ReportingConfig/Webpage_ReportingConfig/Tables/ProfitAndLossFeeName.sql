--drop table ProfitAndLossFeeName
if not exists(select * from sys.tables where name = N'ProfitAndLossFeeName' and type = N'U')
BEGIN

	CREATE TABLE [ProfitAndLossFeeName](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] [nvarchar](255) NULL,
		[DateStamp] [datetime] NULL,
		[Type] int NULL
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
	) ON [PRIMARY] 
END;

GO


grant Select on [ProfitAndLossFeeName] to [svc-bireporting]
grant SELECT, INSERT, UPDATE, DELETE on ProfitAndLossFeeName to BIReporting_Configurator_user


go

-- add check constraint on UNIQUE FeeName 
if not exists (select  1 from INFORMATION_SCHEMA.TABLE_CONSTRAINTS  where constraint_name = 'DF_UNIQUE_ProfitAndLossFee_Name')
begin

	ALTER TABLE ProfitAndLossFeeName
	ADD CONSTRAINT DF_UNIQUE_ProfitAndLossFee_Name UNIQUE (Name);
END

GO


