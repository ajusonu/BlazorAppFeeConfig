USE [BI_Reporting]
GO

CREATE OR ALTER PROCEDURE [dbo].[proc_extracts_Retail_fact_snapshot_Vs_Invoice_Summary_AsAtDate]
as

declare @snapshotDate datetime

Select @snapshotDate =max(COBDate) from RPT_HOT_Daily_Cumulative_RetailInvoiceCommission_VsTarget_By_COBDate_By_DestinationRegion_vw; 

with RetailSaleDestinationRegion as
(Select isNull(r.ModifiedRegion, 'Rest of World') as DestinationRegion, s.*  
  from fact_snapshot_YesterdaySale_ByRegion_Cumulative_vs_Target_vw s left join REF_HOT_Invoice_Sale_Commission_Region r
	on s.Region = r.Region
)
Select 
	DestinationRegion, 
	sum(YesterdaySales) as YesterdaySale ,
	sum(sellTarget) as Yesterday_Sale_Target 
into
	#MTD_Sale 
from
	 RetailSaleDestinationRegion
where 
	reportdate = @snapshotDate
Group by 
	DestinationRegion;

with RetailSaleDestinationRegion as
(Select isNull(r.ModifiedRegion, 'Rest of World') as DestinationRegion, s.*  
  from fact_snapshot_YesterdaySale_ByRegion_Cumulative_vs_Target_vw s left join REF_HOT_Invoice_Sale_Commission_Region r
	on s.Region = r.Region
)
Select 
	DestinationRegion, 
	sum(YesterdaySales) as MTD_Sale,
	sum(sellTarget) as MTD_Sale_Target  
into
	#YesterdaySale
from
	 RetailSaleDestinationRegion
where 
	Year(reportdate) = Year(@snapshotDate) and
	month(reportdate)= month(@snapshotDate)
Group by 
	DestinationRegion;


with RetailSaleDestinationRegion as
(Select isNull(ModifiedRegion, 'Rest of World') as DestinationRegion, 
	s.*  from RPT_HOT_Daily_Cumulative_RetailInvoiceCommission_VsTarget_By_COBDate_By_DestinationRegion_vw s
left join REF_HOT_Invoice_Sale_Commission_Region r
	on s.Region = r.Region
)
Select 
	DestinationRegion, 
	sum(SellExGST) as Yesterday_InvoiceSale, 
	sum(sellTarget) as Yesterday_InvoiceSale_Target, 
	sum(Commission) as Yesterday_InvoiceCommission, 
	sum(CommissionTarget) as Yesterday_InvoiceCommission_Target 
into
	#Yesterday_InvoiceSale
from 
	RetailSaleDestinationRegion
where
	 reportdate= @snapshotDate
Group by 
	DestinationRegion;


with RetailSaleDestinationRegion as
(Select isNull(ModifiedRegion, 'Rest of World') as DestinationRegion, 
	s.*  from RPT_HOT_Daily_Cumulative_RetailInvoiceCommission_VsTarget_By_COBDate_By_DestinationRegion_vw s
left join REF_HOT_Invoice_Sale_Commission_Region r
	on s.Region = r.Region
)
Select 
	DestinationRegion, 
	sum(SellExGST) as MTD_InvoiceSale, 
	sum(sellTarget) as MTD_InvoiceSale_Target, 
	sum(Commission) as MTD_InvoiceCommission, 
	sum(CommissionTarget) as MTD_InvoiceCommission_Target
into
	#MTD_InvoiceSale
from 
	RetailSaleDestinationRegion
where 
	year(reportdate)= Year(@snapshotDate) and
	month(reportdate)= month(@snapshotDate)
Group by 
	DestinationRegion;


delete RPT_Retail_fact_snapshot_Vs_Invoice_Summary_AsAtDate where AsAtDate = @snapshotDate
insert into RPT_Retail_fact_snapshot_Vs_Invoice_Summary_AsAtDate

Select
	@snapshotDate as AsAtDate,
	mi.DestinationRegion, 
	MTD_InvoiceSale, 
	MTD_InvoiceSale_Target, 
	MTD_InvoiceCommission, 
	MTD_InvoiceCommission_Target, 
	Yesterday_InvoiceSale, 
	Yesterday_InvoiceSale_Target, 
	Yesterday_InvoiceCommission, 
	Yesterday_InvoiceCommission_Target,
	YesterdaySale,
	Yesterday_Sale_Target,
	MTD_Sale,
	MTD_Sale_Target 

from 
	#MTD_InvoiceSale mi LEFT JOIN 
	#Yesterday_InvoiceSale yi on 
		mi.DestinationRegion = yi.DestinationRegion LEFT JOIN 
	#MTD_Sale ms on 
		mi.DestinationRegion = ms.DestinationRegion  LEFT JOIN 
	#YesterdaySale ys on 
		mi.DestinationRegion = ys.DestinationRegion