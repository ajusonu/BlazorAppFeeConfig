----dependency: TABLE ProfitAndLossFee.sql
----dependency: TABLE ProfitAndLossFeeName.sql
--Select * from Ref_ProfitAndLossFee_vw
create or alter view Ref_ProfitAndLossFee_vw as
Select 
	n.name, 
	CASE
		WHEN TYPE = 2 THEN p.FeeValue ELSE p.FeePercentage END as Fee,
			CASE WHEN n.type =1 THEN 'Percentage' ELSE 'Value' END FeeType, 
			CASE WHEN Brand = 0 THEN 'MixAndMatchNZ'
				WHEN Brand = 1 THEN 'MixAndMatchAU'
				WHEN Brand = 2 THEN 'MixAndMatchUK'
				ELSE ''
			END BrandName
	from 
		ProfitAndLossFee p  join ProfitAndLossFeeName n on p.FeeNameId = n.id 