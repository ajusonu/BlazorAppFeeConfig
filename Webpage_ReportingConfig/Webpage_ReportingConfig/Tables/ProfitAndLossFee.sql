--drop table ProfitAndLossFee
if not exists(select * from sys.tables where name = N'ProfitAndLossFee' and type = N'U')
BEGIN

CREATE TABLE [dbo].[ProfitAndLossFee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Brand] [int] NOT NULL,
	[FeeValue] [decimal](12, 4) NOT NULL,
	[FeePercentage] [decimal](12, 4) NOT NULL,
	[FeeNameId] [int] NOT NULL,
 CONSTRAINT [PK_ProfitAndLossFee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [DF_UNIQUE_ProfitAndLossFee_Brand_FeeNameId] UNIQUE NONCLUSTERED 
(
	[Brand] ASC,
	[FeeNameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

END;

GO


grant Select on [ProfitAndLossFee] to [svc-bireporting]
grant SELECT, INSERT, UPDATE, DELETE on ProfitAndLossFee to BIReporting_Configurator_user


go

-- add check constraint on UNIQUE FeeName 
if not exists (select  1 from INFORMATION_SCHEMA.TABLE_CONSTRAINTS  where constraint_name = 'DF_UNIQUE_ProfitAndLossFee_Name')
begin

	ALTER TABLE ProfitAndLossFee
	ADD CONSTRAINT DF_UNIQUE_ProfitAndLossFee_Name UNIQUE (
				[Brand] ASC,
				[FeeNameId] ASC
					);
END

GO


