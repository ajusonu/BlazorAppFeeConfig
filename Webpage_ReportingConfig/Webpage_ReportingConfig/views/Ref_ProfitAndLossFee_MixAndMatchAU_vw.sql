----dependency: VIEW Ref_ProfitAndLossFee_vw.sql
--Select * from Ref_ProfitAndLossFee_MixAndMatchAU_vw
create or alter view Ref_ProfitAndLossFee_MixAndMatchAU_vw as
Select * from Ref_ProfitAndLossFee_vw WHERE BrandName = 'MixAndMatchAU'
 