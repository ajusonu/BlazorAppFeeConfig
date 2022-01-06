USE [BI_Reporting]
GO


CREATE OR ALTER   PROCEDURE [dbo].[Proc_MMAU_BookingProfitAndLossReport_PBI]
AS

-- ============================================================================================================================================================================
-- Author: Ajay
-- Create date: 08/09/2020 3:04:38 PM
-- VSO Case: 42150 -- MMAU Profit & Loss Report
-- Purpose: Breakdown the components of a booking to determine overall booking profit and loss, note there are a number of complexities around elements which are not sourced from withing the BMM booking itself
--          Also a key fundamental fact is the booking elements (such as a credit card fee), are distributed equally over all tickets (as we don't know which ticket any booking level element really applies to)
-- Version 1.0
-- ============================================================================================================================================================================

Declare @StartYear as Int = Year(GetDate()) - 2

-- ============================================================================================================================================================================
-- Reference
-- ============================================================================================================================================================================
-- Collation in BMM = collate SQL_Latin1_General_CP1_CI_AS
-- Row number Fn    = Row_Number() over (Partition by [field1], [field2 etc]  order by [field1],'[field2 etc]) as RankNo
-- LAG (field to return [,offset] [,default if null]) OVER ( [ partition_by_clause ] order_by_clause )
-- ============================================================================================================================================================================
-- Plan
-- ============================================================================================================================================================================
-- A: Get base data
-- B: Get online payments
-- C: Calculate folder level items
-- C2: Calculate monthly values for search engines
-- D: Distribute folder level values across all tickets
-- E: Results

Print 'ODS Item Info';
----------------------------------------------------------------------------------
--**********************************************************************************
--ODS Item Info
--**********************************************************************************
--drop table #ODSItemInfo
Select 
	fi.strBBranchCode_FD + cast(fi.lFFoldNo_FD as varchar(20))+ '-'+ cast(fi.nFiFoldItemID_FD as varchar(20)) as ItemId, 
	fi.lFFoldNo_FD, fi.strBBranchCode_FD, fi.nFiFoldItemID_FD,
	fp.lffpcustfinalcustpayableamt_fd,  fp.lffpcusttotbranchreceivablefromcustamt_fd,
	fi.strFiAirLineCode_FD,
	fi.strFiType_FD, 
	fi.lffitotpayabletoanysellamt_fd,
	Left(fi.strFiSuppFOPInfo_FD, 14) TPFormOfPayment,
	bFiRefundItem_FD
 into
	#ODSItemInfo
 from 
	 [DolphinAUSERVER].[DolphinAU].[DBA].[FoldsAndFIsAll_VW]  fi with (nolock) join
	 [DolphinAUSERVER].[DolphinAU].[DBA].FoldPricingsDec_VW  fp with (nolock)
	 on fp.lFFoldNo_FD  = fi.lFFoldNo_FD and
		 fp.strbbranchcode_fd = fi.strBBranchCode_FD and
		 fp.nfifolditemid_fd = fi.nFiFoldItemID_FD
	
	where 
		--fi.lFFoldNo_FD in ( '388917', '472021', '364629', '450240', '364629', '471416', '471399') and 
		fi.strBBranchCode_FD ='au';

--Select * from #ODSItemInfo
-- ============================================================================================================================================================================
-- A: Get base data
-- ============================================================================================================================================================================
--drop table #BMM_Folders
select 
	o.ParentOutletCode as Outlet,
	'BMM' as [Source],
	f.strBBranchCode_FD as BranchCode,
	f.lFFoldNo_FD as FolderNo,
	f.dtFCreateDate_FD as CreationDate,
	f.dtFDepDate_FD as DepDate,
	case when  f.strCCompanyName_FD = '' then 'No Company' else f.strCCompanyName_FD end as CompanyName,
	f.strFLeadPaxLastName_FD as LastName,
	f.strFLeadPaxFirstName_FD as FirstName,
	f.strFLeadPaxTitle_FD as Title,
	f.strFOwnedByStaffName_FD,
	f.strFProjectNo_FD,
	case when f.strCCompanyName_FD <> '' and f.bFisCompanyTravel_fd = 1 then 'CORPORATE'
		 when f.strCCompanyName_FD <> '' and f.bFisCompanyTravel_fd = 0 then 'CORPLEISURE'
		 else 'LEISURE' end as TravelType,
	f.strFArrivalPointCode_FD as FolderArrivalPoint,
	f.strFDestinationName_FD as FolderDestCityCode,
	f.strFDestinationCountryName_FD as FolderDestCountryName,
	f.strFDestinationCountryCode_FD as FolderDestCountryCode,
	f.nFiSortOrder_FD as SegmentNo,
	f.nFiFoldItemID_FD as ItemId,
	case 
		when f.strFiType_FD = 'TKT' Then 'Airfare'
		when f.strFiType_FD = 'HOT' Then 'Hotel'
		when f.strFiType_FD = 'OTH' Then 'Other'
		else
			f.strFiType_FD
	end SegmentType,
	f.strFiAirlineCode_FD as Airline,
	case when f.strFiType_FD IN  ('AIR', 'TKT') then --f.strFiFlightNo_FD 
		substring(f.strFiFlightNo_FD, 3, LEN(f.strFiFlightNo_fd))
	 else cast('' as varchar(10)) end as FlightNo,
	case when f.strFiType_FD IN  ('AIR', 'TKT') then f.strFiStartPointCode_FD else f.strFiItinVendCityCode_FD end as DepCity,
	case when f.strFiType_FD IN  ('AIR', 'TKT') then f.strFiEndPointCode_FD   else f.strFiItinVendCityCode_FD end as ArrCity,
	case when f.strFiType_FD IN  ('AIR', 'TKT') then f.strFiStartPointCountryCode_FD else  f.strFiItinVendCountryCode_FD end as DepCountry,
	case when f.strFiType_FD IN  ('AIR', 'TKT') then f.strFiEndPointCountryCode_FD else  f.strFiItinVendCountryCode_FD end as ArrCountry,
	f.dtFiStartDateTime_FD as SegDepDateTime,
	f.dtFiEndDateTime_FD as SegArrDateTime,
	f.nFiNumDay_FD as NoNights,
	f.strFiStatus_FD as [Status],
	f.lFiItinVendID_FD as VendorCode,
	f.strFiItinVendName_FD as VendorName,
	case when f.strPcProductCode_FD IN  ('AIR', 'TKT') then f.strFiStartPointCountryCode_FD else  f.strFiItinVendCountryCode_FD end as SegmentDepCountryCode,
	case when f.strPcProductCode_FD IN  ('AIR', 'TKT') then f.strFiEndPointCountryCode_FD else  f.strFiItinVendCountryCode_FD end as SegmentArrCountryCode,
	cast('' as varchar(500)) as SegmentDepCountryName,
	cast('' as varchar(500)) as SegmentArrCountryName,
	case when f.strFDestinationCountryCode_FD = 'AU' then 'D' else 'I' end as IntDom,
	f.strFDestinationCountryCode_FD as bkg_dest_country_code,
	cast('' as varchar(250)) as ArrCityName,
	cast('' as varchar(250)) as DepCityName,
	f.strFOwnedByStaffCode_FD,
	f.strFItineraryEmail_FD strCEmail_FD,
	f.strCMobilePhoneNo_FD,
	f.lFClientID_FD,
	f.lFCompanyID_FD,    
	f.strFiType_FD,
	f.strCCountryCode_FD,
	f.strFCostCentre_FD,
	f.strCDepartment_FD,
	br.DefDepCity_FD strFCustomerBranchCode_FD,
	CASE WHEN f.strFiType_FD IN  ('TKT') THEN f.strFiVendDocNo_FD ELSE strFiConfNo_FD END strFiConfNo_FD,
	f.strFiVendDocNo_FD,
	f.strFiTktType_FD,
	f.strCClientMobilePhoneNo_FD,
	f.lfFCustTotPayableToBranchSellAmt_FD,
	f.lfFCustTotBranchReceivableFromCustAmt_FD,
	f.lfFCustTotPayableToBranchSellVATAmt_FD, 
	f.lfFiCustTotPayableToAnySellAmt_FD,
	f.lfFiCustTotBranchReceivableFromCustAmt_FD,
	f.lfFCustPayableToAnyFinalCustPayableAmt_FD,
	f.lfFiCustTotFinalCustPayableAmt_FD,
	f.strFiDesc_FD,
	f.strFiProductName_FD,
	f.strFpDesc_FD,
	f.strFBookedByName_FD,
	case when f.dtFCreateDate_FD is NULL then 0 
		else DATEDIFF(Day, f.dtFCreateDate_FD, f.dtFDepDate_FD )  end as days_booked_in_advance,
	f.lfFpCustFinalCustPayableAmt_FD,
	f.strFTravelPurpose_FD,
	f.strFiLinkedPaxName_FD,
	case 
      when co.region = 'AU' then 'D'
      when co.region = 'AU' then 'T'
      else 'I' 
   end  as [Market Type],
   f.strFDestinationCountryCode_FD,
   f.lfFCustTotPayableToAnySellVATAmt_FD,
   f.lfFiCustTotPayableToAnySellVATAmt_FD,
   f.lfFpCustTotInvCrdSplCommVATAmt_FD,
   f.lfFiTotPayableToAnyCommAmt_FD,
   f.lfFpCustTotTktTaxAmt_FD,
   f.lfFpTotCommAmt_FD,
   f.nFNumBum_FD,
   f.dtFiCreateDate_FD item_creation_date,
   f.strFiStatus_FD FolderStatus,
   f.lfFpTotNetAmt_FD + f.lfFpTotNetVATAmt_FD as BuyPrice,
   f.lfFpCustFinalCustPayableAmt_FD as SellPrice,
   f.bFiRefundItem_FD,
   f.lfFpTotNetVATAmt_FD,
    Row_Number() over (Partition by 	f.strBBranchCode_FD, f.lFFoldNo_FD  
			order by case when  f.dtFiStartDateTime_FD = '1980-01-01 00:00:00' OR f.strFiType_FD <> 'AIR' then dateadd(year, 20, getdate()) else 
			f.dtFiStartDateTime_FD end ) as SegDepartFromRankNo,
	cast('' as varchar(50)) as DepartureCountryCode

INTO
	#BMM_Folders
from 
	[DolphinAUSERVER].[DolphinAU].[DBA].FoldsAndFIsAllAndFPs_VW f JOIN
	[DolphinAUSERVER].[DolphinAU].[DBA].HOT_BranchDetails o
		ON f.strBBranchCode_FD = o.BranchCode JOIN
	[DolphinAUSERVER].[DolphinAU].[DBA].BranchDetails_TB br
		ON f.strBBranchCode_FD= br.BranchCode_FD join
   [DolphinAUSERVER].[DolphinAU].[DBA].HOT_country co
      on f.strFDestinationCountryCode_FD collate SQL_Latin1_General_CP1_CI_AS  = co.countrycodetwo 
where 
	--f.strFiType_FD NOT IN ('INS', 'FEE') AND
	f.strFiStatus_FD not in  ('--','XX') AND
	f.bFIsCostQuote_FD = 0 AND
	f.dtFDepDate_FD > dateadd(YEAR, -1, getdate()) AND
	f.strFiRouteNo_FD <> 'ARUNK' AND
	--f.lFFoldNo_FD IN (605488, 601915) and 
	Year(f.dtFCreateDate_FD) > 2017 and 
	f.strBBranchCode_FD = 'AU'

order by 
	FolderNo, SegmentType 
	

--Select Distinct strFiVendDocNo_FD from #BMM_Folders where FolderNo = '1430'

update 
	#BMM_Folders
set 
	SegmentDepCountryName = c.strCountryName_FD
from
	#BMM_Folders b join
	[DolphinAUSERVER].[DolphinAU].[DBA].COUNTRIES_TB c
		on c.strCountryCode_FD = SegmentDepCountryCode
		
update 
	#BMM_Folders
set 
	SegmentArrCountryName = c.strCountryName_FD
from
	#BMM_Folders b join
	[DolphinAUSERVER].[DolphinAU].[DBA].COUNTRIES_TB c
		on c.strCountryCode_FD = SegmentArrCountryCode		

--Update City Names

update 
	#BMM_Folders
set 
	ArrCityName = la.strArrivalPointName_FD
from
	#BMM_Folders b join
	[DolphinAUSERVER].[DolphinAU].[DBA].ArrivalPoints_TB la
		on	la.strArrivalPointCode_FD COLLATE Latin1_General_CI_AS = ArrCity
where
	isNull(ArrCity, '') <> ''
				

update 
	#BMM_Folders
set 
	DepCityName = ld.strArrivalPointName_FD
from
	#BMM_Folders b join
	[DolphinAUSERVER].[DolphinAU].[DBA].ArrivalPoints_TB ld
		on	ld.strArrivalPointCode_FD COLLATE Latin1_General_CI_AS = DepCity
where
	isNull(DepCity, '') <> ''

update 
	#BMM_Folders
set
	DepartureCountryCode = d.DepCountry
from 
	#BMM_Folders f join
	(Select df.FolderNo, df.BranchCode, DepCountry from #BMM_Folders df where SegDepartFromRankNo = 1) d
		on d.FolderNo = f.FolderNo and
			d.BranchCode = f.BranchCode


 update 
	#BMM_Folders
set
	DepartureCountryCode = d.SegmentDepCountryCode
from 
	#BMM_Folders f join
	(Select df.FolderNo, df.BranchCode, SegmentDepCountryCode from #BMM_Folders df where SegDepartFromRankNo = 1) d
		on d.FolderNo = f.FolderNo and
			d.BranchCode = f.BranchCode
where
	DepartureCountryCode = '' 

--drop table #MainTable
Select distinct m.*,
	departReg.booking_departure_datamine_region,
	destReg.booking_departure_datamine_region as bkg_dest_datamine_region
	
into #MainTable 
	from #BMM_Folders m left join
	 (Select distinct booking_departure_country_code, booking_departure_datamine_region 
		from EDW_Reporting.dbo.dim_booking_departure_location) departReg
		on departReg.booking_departure_country_code = m.DepartureCountryCode
	 left join
	 (Select distinct booking_departure_country_code, booking_departure_datamine_region 
		from EDW_Reporting.dbo.dim_booking_departure_location) destReg
		on destReg.booking_departure_country_code = m.FolderDestCountryCode
--SELECT * from #MainTable where folderno = 1164

--drop table #BaseDataItem
Select 
	(BranchCode + cast(FolderNo as varchar(20))) as BookingKey,
	(BranchCode + cast(FolderNo as varchar(20)) + '-'+ cast(itemid as varchar(20)) ) as dim_price_item_key,
	'WEB' channel,
	itemid,
	Airline plated_carrier_reported,
	item_creation_date,
	FolderStatus,
	BranchCode store_id,
	BranchCode AS DolphinBranch,
	strFProjectNo_FD as OnlineBookingID,
	FolderNo AS booking_id,
	strFiTktType_FD,
	strFiLinkedPaxName_FD AS [Traveller Name],
	SegmentType,
	CreationDate as booking_creation_date,
	DepDate as booking_departure_date,
	FolderDestCountryCode as booking_departure_country_code,
	bkg_dest_country_code,
	FolderArrivalPoint as bkg_dest_airport_code,
	FolderDestCountryName as bkg_dest_country,
	nFNumBum_FD as  BookingPaxCount,
	LastName,
	FirstName,
	Title,
	strFOwnedByStaffName_FD, 
	TravelType,
	FolderDestCityCode,
	IntDom,
	strCEmail_FD,
	lfFCustPayableToAnyFinalCustPayableAmt_FD Amount,
	lfFCustTotPayableToBranchSellVATAmt_FD as Total_retail_sell_gst,
	lfFpTotNetVATAmt_FD retail_sell_gst,  
	strFBookedByName_FD AS BookedBy,
	strFpDesc_FD AS item_details,
	strFpDesc_FD AS item_category,
	lfFpCustFinalCustPayableAmt_FD AS retail_sell_price_inc_gst_inc_tax,
	#MainTable.lfFpCustTotTktTaxAmt_FD grossTax,
	#MainTable.lfFpTotCommAmt_FD as retail_commission_ex_gst_ex_tax,
	#MainTable.BuyPrice,
	#MainTable.SellPrice,
	#MainTable.bFiRefundItem_FD,
	#MainTable.booking_departure_datamine_region,
	DepCity+'-'+ ArrCity  flight_city_pair,
	CASE 
		WHEN days_booked_in_advance <= 5 THEN '1 TO 5'
		WHEN days_booked_in_advance BETWEEN 6 AND 10 THEN '6 TO 10'
		WHEN days_booked_in_advance BETWEEN 11 AND 20 THEN '11 TO 20'
		ELSE '> 20' END
		  AS DaysInAdvance,

	ArrCity,
	DepCity,
	CASE WHEN SegmentType IN ('AIR', 'TKT', 'HOT', 'CAR') THEN VendorName ELSE '' END VendorName, 
	CASE WHEN SegmentType = 'AIR' THEN Airline ELSE '' END Airline, 
	CASE WHEN SegmentType = 'AIR' THEN FlightNo ELSE '' END FlightNo, 
	CASE WHEN SegmentType = 'AIR' THEN SegDepDateTime ELSE '' END StartDateTime, 
	CASE WHEN SegmentType = 'AIR' THEN SegArrDateTime ELSE '' END EndDateTime, 
	CASE WHEN SegmentType = 'AIR' THEN ArrCityName ELSE '' END ArrCityName, 
	CASE WHEN SegmentType = 'AIR' THEN DepCityName ELSE '' END DepCityName, 
	CASE WHEN SegmentType = 'AIR' THEN SegmentDepCountryCode ELSE '' END DepartCountry, 
	CASE WHEN SegmentType = 'AIR' THEN SegmentDepCountryName ELSE '' END DepartCountryName, 
	CASE WHEN SegmentType = 'AIR' THEN SegmentArrCountryCode ELSE '' END ArriveCountry, 
	CASE WHEN SegmentType = 'AIR' THEN SegmentArrCountryName ELSE '' END ArriveCountryName,
	case when SegmentType IN ('Airfare') then null else SegDepDateTime END StartDate,
	case when SegmentType IN ('Airfare') then null else SegArrDateTime END EndDate,
	case when SegmentType IN ('HOT', 'CAR') then NoNights else null end NoOfDays,
	case when SegmentType IN ('HOT', 'CAR') then ArrCityName else null end Location,
	case when SegmentType IN ('Airfare') then ArrCityName else null end TkTArrCityName,
	case when SegmentType IN ('Airfare') then DepCityName else null end TkTDepCityName,
	Format(CreationDate,'yyyy MMM') as YearMonthNameOfBookingDeparture,
	Format(CreationDate,'yyyy MM') as YearMonthOfBookingDeparture,
	Format(CreationDate,'yyyy MMM') as YearMonthNameOfBookingCreation,
	Format(CreationDate,'yyyy MM') as YearMonthOfBookingCreation,
	
	Case	
		When booking_departure_datamine_region <> 'AU' and  bkg_dest_datamine_region = 'AU'
		Then 1
		Else 0
	End as BookingIsInbound,
	Case	
		When booking_departure_datamine_region <> 'AU' and  bkg_dest_datamine_region = 'AU'
		Then booking_departure_datamine_region
		Else bkg_dest_datamine_region
	End as bkg_dest_datamine_region_modified, --This is the same logic as BookingIsInbound, this field is a destination but modified if inbound and used to assign many of the fees
	Case	
		When FolderDestCountryCode <> 'AU' and  bkg_dest_country_code = 'AU'
		Then FolderDestCountryCode
		Else bkg_dest_country_code
	End as bkg_dest_country_code_modified,
	case when SegmentType IN ('Airfare') then strFiVendDocNo_FD ELSE '' END document,
	Cast('Unknown' as VarChar(50)) as BookingPayment,	--Updated next
	0 as BookingPaymentType,							--Updated next
	0 as BookingTicketCount,							--Updated next
	Cast('Unknown' as VarChar(50)) as ReportingFee,		--Updated next, this is used to reallocate many different fee types into master categories for reporting
	--Following are folder level items, for clarity these are shown both at booking level and then apportioned by ticket
	Cast(0 as Money) as AmadeusRebate_Booking,
	Cast(0 as Money)  as AmadeusRebate_Apportioned,
	Cast(0 as Int)  as SearchEngineFeeID_Booking,
	Cast(0 as Money)  as SkyScannerFee_Apportioned,
	Cast(0 as Money)  as CreditCardFeeIncome_Booking,
	Cast(0 as Money)  as CreditCardFeeIncome_Apportioned,
	Cast(0 as Money)  as CreditCardFeeExpence_Apportioned,
	Cast(0 as Money)  as BookingFee_Booking,
	Cast(0 as Money)  as BookingFee_Apportioned,
	Cast(0 as Money)  as NegativeMargin_Booking,
	Cast(0 as Money)  as NegativeMargin_Apportioned,
	Cast(0 as Money)  as PositiveMargin_Booking,
	Cast(0 as Money)  as PositiveMargin_Apportioned,
	Cast(0 as Money)  as BookingFeeDiscount_Booking,
	Cast(0 as Money)  as BookingFeeDiscount_Apportioned,
	Cast(0 as Money)  as LostBookingFee_Booking,
	Cast(0 as Money)  as LostBookingFee_Apportioned,
	Cast(0 as Money)  as Rounding_Booking,
	Cast(0 as Money)  as Rounding_Apportioned,
	Cast(0 as Money)  as GeneralDiscount_Booking,
	Cast(0 as Money)  as GeneralDiscount_Apportioned,
	Cast(0 as Money)  as AmendmentFee_Booking,
	Cast(0 as Money)  as AmendmentFee_Apportioned,
	Cast(0 as Money)  as OnlineSeatOnlyDiscount_Booking,
	Cast(0 as Money)  as OnlineSeatOnlyDiscount_Apportioned,
	Cast(0 as Money)  as Other_Booking,
	Cast(0 as Money)  as Other_Apportioned,
	Cast(0 as Money)  as Evoucher_Booking,
	Cast(0 as Money)  as Evoucher_Apportioned,
	Cast(0 as Money)  as KayakFee_Apportioned,	
	Cast(0 as Money)  as CheapFlightsFee_Apportioned,
	Cast(0 as Money)  as Other_International_Apportioned,
	Cast(0 as Money)  as Other_Domestic_Apportioned,
	Cast(0 as Money)  as Consultant_International_Apportioned,
	Cast(0 as Money)  as Consultant_Domestic_Apportioned,
	--End of booking level items
	GetDate() as Updated
Into
	#BaseDataItem
	 
from 
	#MainTable 
order by 
	CreationDate,
	BranchCode,
	FolderNo

--Select retail_sell_gst, * from #BaseDataItem where bFiRefundItem_FD =1

--Select BookingIsInbound,  bkg_dest_datamine_region, bkg_dest_datamine_region_modified from #BaseData
--Select count(*) from #BaseDataItem --691325
--Get payments
--Testing
--Drop Table #Payments
--Declare @StartYear as int = Year(GetDate()) - 1 
--End Testing
Declare @SQLString as nVarChar(4000)

Create Table #Payments(BookingID Int, SuccessfulTransaction Int, SettlementTransactionType VarChar(10), ResponseText VarChar(255), SettlementTransactionAmount Decimal (18,12), SettlementCurrencyCode VarChar(3), CardHolderName VarChar(255), CardType VarChar(50));

Select @SQLString = 'Select * From OpenQuery([MIXANDMATCHAU_HOT_BOOKINGSERVER],' +CHAR(39) +  --Start format for the Open Query
	'Select
		BookingID,
		SuccessfulTransaction,
		SettlementTransactionType,
		ResponseText,
		SettlementTransactionAmount,
		SettlementCurrencyCode,
		CardHolderName,
		CardType
	From
		[MixAndMatchAU_HOT_Booking].dbo.BookingPaymentDPS
	Where
		Year(Inserted) >= ' + Cast(@StartYear as Char(4)) --Year must be a string in the SQLString
+CHAR(39) +')' --End format for the open query

--Select @SQLString
Insert Into #Payments EXECUTE sp_executesql @SQLString

--Select * from #Payments where successfultransaction = 1 and cardtype like '%LB%'
Update
	#Payments
Set
	CardType = 'Laybuy'
Where
	CardType = 'LB'
--------------------------------------------------------------------------------------------------------
--Item Level 
--------------------------------------------------------------------------------------------------------

Update
	#BaseDataItem
Set
	BookingPayment = p.CardType
From
	#BaseDataItem b
	Join #Payments p on
		b.OnlineBookingID = p.bookingid
Where
	p.SuccessfulTransaction = 1 and
	PATINDEX('%[^0-9]%',b.OnlineBookingID) = 0

--BookingPaymentType
Update
	#BaseDataItem
Set
	BookingPaymentType = 
	Case 
		When BookingPayment in ('Visa','Amex','Diners','Discover','JCB','Mastercard','QCard','Trurewrd') Then 1
		When BookingPayment = 'Laybuy' Then 2
		Else 0
	End

	--Select * from #BaseDataItem where Patindex('%Booking Fee%',item_details) > 0
--ReportingFee, because fees can be entered in many different formats, this will reallocate them into reporting categories, this allows us to check to see if any fees remain afater the allocation
Update
	#BaseDataItem
Set
	ReportingFee = 
		--Warning if you change any of the output text in the case statement, you also need to change the mapping in the section following
		Case	
			When SegmentType = 'Fee' and Patindex('Credit%',item_category) > 0 Then 'Credit card fee income'
			When SegmentType = 'Fee' and Patindex('%Booking Fee%',item_category) > 0 and retail_sell_price_inc_gst_inc_tax >= 0 Then 'Booking fee'
			When SegmentType = 'Fee' and Patindex('%Booking Fee%',item_category) > 0 and retail_sell_price_inc_gst_inc_tax < 0 Then 'Booking fee discount'
			When SegmentType = 'Fee' and (Patindex('%Consultant Fee%',item_category) > 0 or Patindex('%Amendment Fee%',item_category) > 0) Then 'Amendment fee'
			When SegmentType = 'Discount' and Patindex('Online seat%',item_category) > 0 Then 'Online seat only discount'
			When SegmentType = 'Discount' and Patindex('%Rounding%',item_category) = 0 and retail_sell_price_inc_gst_inc_tax < 0 Then 'General discount'
			When SegmentType in ('Fee','Discount') and Patindex('Rounding%',item_category) > 0 Then 'Rounding'
			When SegmentType in ('Fee','Discount') Then  'Other' --Catch all for other fees and discounts not defined (don't use 'else' as this will catch all segment types)
			When SegmentType = 'Other' and ((Patindex('%Discount%',item_category) > 0 OR  PatIndex('%Campaign%',Item_Details) > 0)) and retail_sell_price_inc_gst_inc_tax < 0 Then 'Negative margin'
			When SegmentType = 'Other' and ((Patindex('%Discount%',item_category) > 0 OR  PatIndex('%Campaign%',Item_Details) > 0)) and retail_sell_price_inc_gst_inc_tax > 0 Then 'Positive margin'
			When SegmentType = 'Other' and ((Patindex('%evoucher%',item_category) > 0 OR  Patindex('%e voucher%',item_category) > 0 OR Patindex('%e-voucher%',item_category) > 0) OR (PatIndex('%evoucher%',Item_Details) > 0 OR PatIndex('%e voucher%',Item_Details) > 0 OR PatIndex('%e-voucher%',Item_Details) > 0)) and retail_sell_price_inc_gst_inc_tax < 0 Then 'Evoucher'
		End

--Select * from #BaseDataItem
Update
	#BaseDataItem
Set
	AmendmentFee_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseDataItem a
	Join (Select BookingKey, dim_price_item_key, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax
			From #BaseDataItem Where ReportingFee = 'Amendment fee' Group By BookingKey, dim_price_item_key) b on
		a.BookingKey = b.BookingKey and
		a.dim_price_item_key = b.dim_price_item_key

		--AmendmentFee_Apportioned
Update #BaseDataItem Set AmendmentFee_Apportioned = AmendmentFee_Booking / 
case when BookingTicketCount =0 then 1 else BookingTicketCount end -- Where SegmentType = 'Airfare'

--Consultant_International_Apportioned
Update #BaseDataItem Set Consultant_International_Apportioned = AmendmentFee_Booking / case when BookingTicketCount =0 then 1 else BookingTicketCount end 
Where Patindex('%Consultant Fee%Inter%',item_category) > 0 

--Select * from #BaseDataItem where Consultant_International_Apportioned <> 0
--Select * from #BaseDataItem where Consultant_International_Apportioned <> 0 and BookingKey = 'HO398216'

--Select  AmendmentFee_Booking , BookingTicketCount, * from #BaseData where item_category like '%Consultant%Fee%International%' and BookingTicketCount =0
--Consultant_Domestic_Apportioned
Update #BaseDataItem Set Consultant_Domestic_Apportioned = AmendmentFee_Booking /
case when BookingTicketCount =0 then 1 else BookingTicketCount end  
Where Patindex('%Consultant Fee%Inter%',item_category) = 0

--Other_Booking,
Update
	#BaseDataItem
Set
	Other_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseDataItem a
	Join (Select BookingKey, dim_price_item_key, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax 
		From #BaseDataItem Where ReportingFee = 'Other' Group By BookingKey, dim_price_item_key) b on
		a.BookingKey = b.BookingKey and
		a.dim_price_item_key = b.dim_price_item_key

Update #BaseDataItem Set Other_Apportioned = Other_Booking / 
case when BookingTicketCount =0 then 1 else BookingTicketCount end   Where SegmentType = 'Airfare'


Update #BaseDataItem Set Other_Domestic_Apportioned = Other_Booking / 
case when BookingTicketCount =0 then 1 else BookingTicketCount end   Where SegmentType = 'Airfare' and 
booking_departure_country_code = 'AU' and bkg_dest_country_code = 'AU'

Update #BaseDataItem Set Other_International_Apportioned = Other_Booking / 
case when BookingTicketCount =0 then 1 else BookingTicketCount end   Where SegmentType = 'Airfare'  and 
NOT (booking_departure_country_code = 'AU' and bkg_dest_country_code = 'AU') 

--drop table #BaseData
Select * into #BaseData from #BaseDataItem
--Select * from #BaseData
--------------------------------------------------------------------------------------------------------
--Bookings 
--Drop table #Bookings
Select Distinct
	BookingKey,
	Airline,
	ArrCity,
	DepCity
Into
	#Bookings
From
	#BaseData

--drop table #Flights

--default fee 
Declare @CreditCardFeePercent as Money = .012
Declare @LaybuyFeePercent as Money = .035
Declare @AmadeusRebate_NZDom as Money = 4.27
Declare @AmadeusRebate_NZInt as Money = 4.27
Declare @AmadeusRebate_NZSH as Money = 4.27 --Shorthaul, introduced 05Aug19, Shannon (VSO 34382)
Declare @AmadeusRebate_JQ as Money = 0.0
Declare @AmadeusRebate_VA as Money = 1.0
Declare @AmadeusRebate_QF as Money = 1.5
Declare @AmadeusRebate_YY as Money = 4.27
Declare @SkyScannerFixedPercent_Asia as Money = .02
Declare @SkyScannerFixedPercent_Au as Money = .02
Declare @SkyScannerFixedPercent_NZ as Money = .02
Declare @SkyScannerFixedPercent_Oth as Money = .02
Declare @SkyScannerFixedPercent_SPAC as Money = .02
Declare @SkyScannerFixedPercent_UKEUR as Money = .02
Declare @SkyScannerFixedPercent_USCA as Money = .02
Declare @KayakFixedPercent as Money = 0
Declare @CheapFlightsFixedPercent as Money = 1.75
Declare @LostBookingFee_Asia as Money = 24.95
Declare @LostBookingFee_Au as Money = 14.95
Declare @LostBookingFee_NZ as Money = 9.95
Declare @LostBookingFee_Oth as Money = 24.95
Declare @LostBookingFee_SPAC as Money = 14.95
Declare @LostBookingFee_UKEUR as Money = 24.95
Declare @LostBookingFee_USCA as Money = 24.95
Declare @LostBookingFee_ID as Money = 14.95

--Override fee value if exists in ProfitAndLossFee table
--Getting Fee for View Select * from Ref_ProfitAndLossFee_MixAndMatchAU_vw
--This can be configure using UI http://webpage_reportingconfig.hotdev01.hot.co.nz/
SELECT @LostBookingFee_NZ = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_NZ' 
SELECT @CreditCardFeePercent = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='CreditCardFeePercent' 

SELECT @LaybuyFeePercent = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LaybuyFeePercent' 
SELECT @LostBookingFee_Asia = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_Asia' 
SELECT @LostBookingFee_Au = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_Au' 

SELECT @LostBookingFee_Oth = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_Oth' 
SELECT @LostBookingFee_SPAC = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_SPAC' 
SELECT @LostBookingFee_UKEUR = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_UKEUR' 
SELECT @LostBookingFee_USCA = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_USCA' 
SELECT @LostBookingFee_ID = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='LostBookingFee_ID' 
SELECT @AmadeusRebate_NZDom = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_NZDom' 
SELECT @AmadeusRebate_NZInt = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_NZInt' 
SELECT @AmadeusRebate_NZSH = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_NZSH' 
SELECT @AmadeusRebate_JQ = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_JQ' 
SELECT @AmadeusRebate_VA = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_VA' 
SELECT @AmadeusRebate_QF = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_QF' 
SELECT @AmadeusRebate_YY = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='AmadeusRebate_YY' 
SELECT @SkyScannerFixedPercent_Asia = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_Asia' 
SELECT @SkyScannerFixedPercent_Au = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_Au' 
SELECT @SkyScannerFixedPercent_NZ = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_NZ' 
SELECT @SkyScannerFixedPercent_Oth = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_Oth' 
SELECT @SkyScannerFixedPercent_SPAC = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_SPAC' 
SELECT @SkyScannerFixedPercent_UKEUR = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_UKEUR' 
SELECT @SkyScannerFixedPercent_USCA = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='SkyScannerFixedPercent_USCA' 
SELECT @KayakFixedPercent = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='KayakFixedPercent' 
SELECT @CheapFlightsFixedPercent = FEE FROM Ref_ProfitAndLossFee_MixAndMatchAU_vw WHERE Name ='CheapFlightsFixedPercent' 


--Flights
Select
	distinct 
	b.BookingKey,
	Airline as airline_code,
	b.DepCity as dep_airport_code,
	dep.country_code as dep_country,
	b.ArrCity arr_airport_code,
	arr.country_code as arr_country,
	Case
		When (Airline = 'NZ' and dep.country_code = 'AU' and (arr.datamine_region in ('SPAC','AU') or arr.airport_code in ('DPS','HNL'))) or (Airline = 'NZ' and arr.country_code = 'AU' and (dep.datamine_region in ('SPAC','AU') or dep.airport_code in ('DPS','HNL')))  Then @AmadeusRebate_NZSH --shorthaul (Shannon VSO 34382)
		When Airline = 'NZ' and (dep.country_code <> 'AU' or arr.country_code <> 'AU') Then @AmadeusRebate_NZInt --longhaul international (test for shorthaul first)
		When Airline = 'NZ' and dep.country_code = 'AU' and arr.country_code = 'AU' Then @AmadeusRebate_NZDom --domestic
		When Airline = 'JQ' Then @AmadeusRebate_JQ
		When Airline = 'VA' Then @AmadeusRebate_VA
		When Airline = 'QF' Then @AmadeusRebate_QF
		Else @AmadeusRebate_YY
	End as AmadeusRebate
Into
	#Flights
From
	#Bookings b

		Join EDW_Reporting.dbo.dim_Location dep on
			dep.airport_code = b.DepCity
		Join EDW_Reporting.dbo.dim_Location arr on
			arr.airport_code = b.ArrCity
where
	Airline <> ''

--Select * from #flights where airline_code = 'NZ' and arr_country <> 'AU' and amadeusrebate = .26
--Select * from EDW_Reporting.dbo.dim_itinerary_flight where store_id = 'HO' and Booking_ID = 310428
--Select * from #Bookings where bookingkey = 'ho310428'
--Select * from #Flights where bookingkey = 'ho310428'
-- ============================================================================================================================================================================
-- B: Match with Ecom database and get payment types
-- ============================================================================================================================================================================

--Set the payment type (note multiple payment types are very abnormal, but we will only accept one for reporting so the last one found will be used)

--select * from #Payments where SuccessfulTransaction = 1

Update
	#BaseData
Set
	BookingPayment = p.CardType
From
	#BaseData b
	Join #Payments p on
		b.OnlineBookingID = p.bookingid
Where
	p.SuccessfulTransaction = 1 and
	PATINDEX('%[^0-9]%',b.OnlineBookingID) = 0

--BookingPaymentType
Update
	#BaseData
Set
	BookingPaymentType = 
	Case 
		When BookingPayment in ('Visa','Amex','Diners','Discover','JCB','Mastercard','QCard','Trurewrd') Then 1
		When BookingPayment = 'Laybuy' Then 2
		Else 0
	End
--Select * from #BaseData
	
-- ============================================================================================================================================================================
-- C: Calculate folder level items
-- ============================================================================================================================================================================
--ReportingFee, because fees can be entered in many different formats, this will reallocate them into reporting categories, this allows us to check to see if any fees remain afater the allocation
Update
	#BaseData
Set
	ReportingFee = 
		--Warning if you change any of the output text in the case statement, you also need to change the mapping in the section following
		Case	
			When SegmentType = 'Fee' and Patindex('Credit%',item_category) > 0 Then 'Credit card fee income'
			When SegmentType = 'Fee' and Patindex('%Booking Fee%',item_category) > 0 and retail_sell_price_inc_gst_inc_tax >= 0 Then 'Booking fee'
			When SegmentType = 'Fee' and Patindex('%Booking Fee%',item_category) > 0 and retail_sell_price_inc_gst_inc_tax < 0 Then 'Booking fee discount'
			When SegmentType = 'Fee' and (Patindex('%Consultant Fee%',item_category) > 0 or Patindex('%Amendment Fee%',item_category) > 0) Then 'Amendment fee'
			When SegmentType = 'Discount' and Patindex('Online seat%',item_category) > 0 Then 'Online seat only discount'
			When SegmentType = 'Discount' and Patindex('%Rounding%',item_category) = 0 and retail_sell_price_inc_gst_inc_tax < 0 Then 'General discount'
			When SegmentType in ('Fee','Discount') and Patindex('Rounding%',item_category) > 0 Then 'Rounding'
			When SegmentType in ('Fee','Discount') Then  'Other' --Catch all for other fees and discounts not defined (don't use 'else' as this will catch all segment types)
			When SegmentType = 'Other' and ((Patindex('%Discount%',item_category) > 0 OR  PatIndex('%Campaign%',Item_Details) > 0)) and retail_sell_price_inc_gst_inc_tax < 0 Then 'Negative margin'
			When SegmentType = 'Other' and ((Patindex('%Discount%',item_category) > 0 OR  PatIndex('%Campaign%',Item_Details) > 0)) and retail_sell_price_inc_gst_inc_tax > 0 Then 'Positive margin'
			When SegmentType = 'Other' and ((Patindex('%evoucher%',item_category) > 0 OR  Patindex('%e voucher%',item_category) > 0 OR Patindex('%e-voucher%',item_category) > 0) OR (PatIndex('%evoucher%',Item_Details) > 0 OR PatIndex('%e voucher%',Item_Details) > 0 OR PatIndex('%e-voucher%',Item_Details) > 0)) and retail_sell_price_inc_gst_inc_tax < 0 Then 'Evoucher'
		End
--Select * from #BaseData Where	booking_id = 398216

--Select * from #BaseData Where Patindex('%Consultant Fee%Inter%',item_category) > 0 

--BookingTicketCount
Update
	#BaseData
Set
	BookingTicketCount = b.BookingTicketCount
From
	#BaseData a
	Join (Select BookingKey, Count(*) as BookingTicketCount From #BaseData Where SegmentType = 'Airfare' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--AmadeusRebate_Booking
Update
	#BaseData
Set
	AmadeusRebate_Booking = b.TotalAmadeusRebate
From
	#BaseData a
	Join (Select BookingKey,Sum(AmadeusRebate) as TotalAmadeusRebate From #Flights Group By BookingKey) b on
		a.BookingKey = b.BookingKey


--CreditCardFeeIncome_Booking
Update
	#BaseData
Set
	CreditCardFeeIncome_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Credit card fee income' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--BookingFee_Booking
Update
	#BaseData
Set
	BookingFee_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Booking fee' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--NegativeMargin_Booking
Update
	#BaseData
Set
	NegativeMargin_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where  ReportingFee = 'Negative margin' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--PositiveMargin_Booking
Update
	#BaseData
Set
	PositiveMargin_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData
	Where  ReportingFee = 'Positive margin' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--BookingFeeDiscount_Booking
Update
	#BaseData
Set
	BookingFeeDiscount_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData 
	Where ReportingFee = 'Booking fee discount' Group By BookingKey) b on
		a.BookingKey = b.BookingKey
	

--LostBookingFee_Booking
Update
	#BaseData
Set
	LostBookingFee_Booking = 
	Case
		When bkg_dest_country_code_modified = 'ID' Then BookingPaxCount * @LostBookingFee_ID --Test for specific country first, then progress to test for regions if specific country not found (Shannon VSO 34382)
		When bkg_dest_datamine_region_modified = 'Asia' Then BookingPaxCount * @LostBookingFee_Asia
		When bkg_dest_datamine_region_modified = 'AU' Then BookingPaxCount * @LostBookingFee_Au
		When bkg_dest_datamine_region_modified = 'NZ' Then BookingPaxCount * @LostBookingFee_NZ
		When bkg_dest_datamine_region_modified = 'SPAC' Then BookingPaxCount * @LostBookingFee_SPAC
		When bkg_dest_datamine_region_modified = 'UK/EUR' Then BookingPaxCount * @LostBookingFee_UKEUR
		When bkg_dest_datamine_region_modified = 'US/CA' Then BookingPaxCount * @LostBookingFee_USCA
		Else BookingPaxCount * @LostBookingFee_Oth
	End
From
	#BaseData
Where
	BookingFee_Booking <= 0

--Rounding_Booking,
Update
	#BaseData
Set
	Rounding_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Rounding' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--GeneralDiscount_Booking,
Update
	#BaseData
Set
	GeneralDiscount_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'General discount' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--AmendmentFee_Booking,
Update
	#BaseData
Set
	AmendmentFee_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Amendment fee' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--OnlineSeatOnlyDiscount_Booking,
Update
	#BaseData
Set
	OnlineSeatOnlyDiscount_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Online seat only discount' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--Other_Booking,
Update
	#BaseData
Set
	Other_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Other' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

--Evoucher_Booking,
Update
	#BaseData
Set
	Evoucher_Booking = b.total_retail_sell_price_inc_gst_inc_tax
From
	#BaseData a
	Join (Select BookingKey, Sum(retail_sell_price_inc_gst_inc_tax) as total_retail_sell_price_inc_gst_inc_tax From #BaseData Where ReportingFee = 'Evoucher' Group By BookingKey) b on
		a.BookingKey = b.BookingKey

-- ============================================================================================================================================================================
-- C2: Calculate monthly values for search engines
-- ============================================================================================================================================================================
Select
	bkg_dest_datamine_region_modified as region,
	DATEADD(m, DATEDIFF(m, 0, booking_creation_date), 0) as BookingCreationMonth,
	Sum(
		Case When SegmentType = 'Airfare' Then retail_sell_price_inc_gst_inc_tax Else 0 End
		) as SumOf_retail_sell_price_inc_gst_inc_tax,
	Count(Distinct BookingKey) as SumOfBookings,
	Sum(
		Case When SegmentType = 'Airfare' Then 1 ELse 0 End
		) as SumOfTickets,
	Sum(CreditCardFeeIncome_Booking) as SumOfCreditCardFeeIncome_Booking,
	Sum(BookingFee_Booking) as SumOfBookingFee_Booking,
	Sum(NegativeMargin_Booking) as SumOfNegativeMargin_Booking,
	Sum(PositiveMargin_Booking) as SumOfPositiveMargin_Booking,
	Sum(BookingFeeDiscount_Booking) as SumOfBookingFeeDiscount_Booking
Into
	#MonthlyValues
From
	#BaseData
Group By
	bkg_dest_datamine_region_modified,
	DATEADD(m, DATEDIFF(m, 0, booking_creation_date), 0)

--Select distinct BookingKey from #BaseData where booking_creation_date between '2018-07-01' and '2018-07-31'
--Select * from #MonthlyValues where BookingCreationMonth between '2018-07-01' and '2018-07-31'

--Get search engine values from the reference table based on the creation month and region of the booking
Select
	s.ID,
	s.Region,
	s.DateFrom,
	s.SkyScanner,
	s.Kayak,
	s.CheapFlights,
	m.SumOf_retail_sell_price_inc_gst_inc_tax,
	m.SumOfBookings,
	m.SumOfTickets,
	m.SumOfCreditCardFeeIncome_Booking,
	m.SumOfBookingFee_Booking,
	m.SumOfNegativeMargin_Booking,
	m.SumOfPositiveMargin_Booking,
	m.SumOfBookingFeeDiscount_Booking,
	(m.SumOf_retail_sell_price_inc_gst_inc_tax + m.SumOfCreditCardFeeIncome_Booking + m.SumOfBookingFee_Booking + m.SumOfNegativeMargin_Booking + m.SumOfPositiveMargin_Booking + m.SumOfBookingFeeDiscount_Booking) * s.SkyScanner as SkyScannerPercentageOfRetailSell, --note all added as the negative margin is already a negative vale so adding it will actually deduct it
	(m.SumOf_retail_sell_price_inc_gst_inc_tax + m.SumOfCreditCardFeeIncome_Booking + m.SumOfBookingFee_Booking + m.SumOfNegativeMargin_Booking + m.SumOfPositiveMargin_Booking + m.SumOfBookingFeeDiscount_Booking) * s.SkyScanner / m.SumOfTickets as SkyScannerPercentageOfRetailSellDividedByTickets
Into
	#SearchEngines
From
	[EDWSERVER].BI_Reporting.dbo.Ecom_BookingProfitAndLossReport_SearchEngines s
	Join #MonthlyValues m on
		s.region = m.region and
		s.DateFrom = m.BookingCreationMonth
where
	m.SumOfTickets <> 0
		
--Select * from #SearchEngines	


--Update the base data with the applicable ID from the reference table, used later when distributing values across tickets
Update
	#BaseData
Set
	SearchEngineFeeID_Booking = IsNull(s.ID,0)
From
	#BaseData b
	Left Join #SearchEngines s on
		b.bkg_dest_datamine_region_modified = s.Region and
		s.DateFrom <= b.booking_creation_date and
		s.DateFrom = (Select max(DateFrom) from #SearchEngines Where b.bkg_dest_datamine_region_modified = Region and DateFrom <= b.booking_creation_date)
Where
	SegmentType = 'Airfare'

--Select * from #basedata
-- ============================================================================================================================================================================
-- D: Distribute folder level values across all tickets
-- ============================================================================================================================================================================

--AmadeusRebate_Apportioned
Update #BaseData Set AmadeusRebate_Apportioned = AmadeusRebate_Booking / BookingTicketCount 
	Where SegmentType = 'Airfare' AND
			BookingTicketCount <> 0

--SkyScannerFee_Apportioned
Update
	#BaseData
Set
	SkyScannerFee_Apportioned = 0-(SkyScannerPercentageOfRetailSellDividedByTickets *
	Case
		When bkg_dest_datamine_region_modified = 'Asia' Then @SkyScannerFixedPercent_Asia
		When bkg_dest_datamine_region_modified = 'AU' Then @SkyScannerFixedPercent_AU
		When bkg_dest_datamine_region_modified = 'NZ' Then @SkyScannerFixedPercent_NZ
		When bkg_dest_datamine_region_modified = 'SPAC' Then @SkyScannerFixedPercent_SPAC
		When bkg_dest_datamine_region_modified = 'UK/EUR' Then @SkyScannerFixedPercent_UKEUR
		When bkg_dest_datamine_region_modified = 'US/CA' Then @SkyScannerFixedPercent_USCA
		Else @SkyScannerFixedPercent_Oth
	End)
From
	#BaseData b
	Join #SearchEngines s on
		b.SearchEngineFeeID_Booking = s.ID

--KayakFee_Apportioned
Update
	#BaseData
Set
	KayakFee_Apportioned = 0-((s.SumOfBookings * Kayak / SumOfTickets * @KayakFixedPercent) / BookingTicketCount)
From
	#BaseData b
	Join #SearchEngines s on
		b.SearchEngineFeeID_Booking = s.ID

--CheapFlightsFee_Apportioned
Update
	#BaseData
Set
	CheapFlightsFee_Apportioned = 0-((s.SumOfBookings * CheapFlights / SumOfTickets * @KayakFixedPercent) / BookingTicketCount)
From
	#BaseData b
	Join #SearchEngines s on
		b.SearchEngineFeeID_Booking = s.ID

--CreditCardFeeIncome_Apportioned
Update #BaseData Set CreditCardFeeIncome_Apportioned = CreditCardFeeIncome_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--CreditCardFeeExpence_Apportioned
Update
	#BaseData
Set
	CreditCardFeeExpence_Apportioned = 
	Case
		When BookingPaymentType = 1 Then 0-(retail_sell_price_inc_gst_inc_tax * @CreditCardFeePercent)
		When BookingPaymentType = 2 Then 0-(retail_sell_price_inc_gst_inc_tax * @LayBuyFeePercent)
		Else 0
	End
Where
	SegmentType = 'Airfare'

--BookingFee_Apportioned
Update #BaseData Set BookingFee_Apportioned = BookingFee_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--NegativeMargin_Apportioned
Update #BaseData Set NegativeMargin_Apportioned = NegativeMargin_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--PositiveMargin_Apportioned
Update #BaseData Set PositiveMargin_Apportioned = PositiveMargin_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--BookingFeeDiscount_Apportioned
Update #BaseData Set BookingFeeDiscount_Apportioned = BookingFeeDiscount_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--LostBookingFee_Apportioned
Update #BaseData Set LostBookingFee_Apportioned = LostBookingFee_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--Rounding_Apportioned
Update #BaseData Set Rounding_Apportioned = Rounding_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--GeneralDiscount_Apportioned
Update #BaseData Set GeneralDiscount_Apportioned = GeneralDiscount_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--OnlineSeatOnlyDiscount
Update #BaseData Set OnlineSeatOnlyDiscount_Apportioned = OnlineSeatOnlyDiscount_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--Other_Apportioned
Update #BaseData Set Other_Apportioned = Other_Booking / BookingTicketCount Where SegmentType = 'Airfare'


Update #BaseData Set Other_Domestic_Apportioned = Other_Booking / BookingTicketCount Where SegmentType = 'Airfare' and 
booking_departure_country_code = 'AU' and bkg_dest_country_code = 'AU'

Update #BaseData Set Other_International_Apportioned = Other_Booking / BookingTicketCount Where SegmentType = 'Airfare'  and 
NOT (booking_departure_country_code = 'AU' and bkg_dest_country_code = 'AU') 

--AmendmentFee_Apportioned
Update #BaseData Set AmendmentFee_Apportioned = AmendmentFee_Booking / BookingTicketCount Where SegmentType = 'Airfare'

--Consultant_International_Apportioned
Update #BaseData Set Consultant_International_Apportioned = AmendmentFee_Booking / case when BookingTicketCount =0 then 1 else BookingTicketCount end 
Where Patindex('%Consultant Fee%Inter%',item_category) > 0 

--Select * from #BaseData where Consultant_International_Apportioned <> 0

--Select  AmendmentFee_Booking , BookingTicketCount, * from #BaseData where item_category like '%Consultant%Fee%International%' and BookingTicketCount =0
--Consultant_Domestic_Apportioned
Update #BaseData Set Consultant_Domestic_Apportioned = AmendmentFee_Booking / case when BookingTicketCount =0 then 1 else BookingTicketCount end  
Where Patindex('%Consultant Fee%Inter%',item_category) = 0

--Evoucher_Apportioned
Update #BaseData Set Evoucher_Apportioned = Evoucher_Booking / BookingTicketCount Where SegmentType = 'Airfare'
-- ============================================================================================================================================================================
-- E: Results
-- ============================================================================================================================================================================
Print 'Result'
Select
	*,
	Case
		When LostBookingFee_Apportioned <> 0 Then 'True'
		Else 'False'
	End as HasLostBookingFee,
	Case
		When NegativeMargin_Apportioned <> 0 Then 'True'
		Else 'False'
	End as HasNegativeMargin,
	Case
		When PositiveMargin_Apportioned <> 0 Then 'True'
		Else 'False'
	End as HasPositiveMargin,
	Case
		When Evoucher_Apportioned <> 0 Then 'True'
		Else 'False'
	End as HasEvoucher,
	Case
		When BookingFee_Apportioned <> 0 and LostBookingFee_Apportioned <> 0 Then 'Booking fee and lost booking fee'
		When BookingFee_Apportioned = 0 and LostBookingFee_Apportioned = 0 Then 'No booking fee or lost booking fee'
		When BookingFee_Apportioned <> 0 and LostBookingFee_Apportioned = 0 Then 'Booking fee'
		When BookingFee_Apportioned = 0 and LostBookingFee_Apportioned <> 0 Then 'Lost booking fee'
	End as BookingFeeLostBookingFeeStatus
into
	#FinalData
From
	#BaseData
Where
	SegmentType IN ('Airfare', 'Fee');


Print 'Refund Processed';
---------------------------------------------------------------------------------
Select  vp.strbbranchcode_fd, vp.lFFoldNo_FD, vp.nFiFoldItemID_FD, vp.dtVtReconcileDate_FD dtVtReconcileDate_FD 
			
into #VendPmtsMade
from  [DolphinAUSERVER].[DolphinAU].[DBA].[VendPmtsMade_VW] vp 
where vp.dtVtReconcileDate_FD > year(getdate()) -1
--Select distinct * from #RefundProcessed --3619
Print 'Refund Processed logic Starting'
--drop table #RefundProcessed
Select
	distinct
    store_id,
    booking_id,
	booking_departure_date,
	bkg_dest_airport_code,
    bkg_dest_country,
    bkg_dest_country_code,
    retail_sell_price_inc_gst_inc_tax as retail_sell_price_ex_gst_ex_tax,
    retail_sell_price_inc_gst_inc_tax,
	BuyPrice buyprice,
    item_details,
    item_category,
 	item_creation_date,
	document  ticket_number,
	plated_carrier_reported as plated_carrier,
	plated_carrier_reported as carrier_code,
	cast(v.dtVtReconcileDate_FD as date) RefundsProcessedDate,
	Format(v.dtVtReconcileDate_FD,'yyyyMMdd') as dim_date_key,
	1 as TicketCount,
	v.dtvtreconciledate_fd,
	retail_sell_gst,
	BookingKey,
	SegmentType
into
	#RefundProcessed
From
    #BaseDataItem db
	--Join Vendor Reconcile 
	LEFT JOIN (Select  vp.strbbranchcode_fd, vp.lFFoldNo_FD, vp.nFiFoldItemID_FD, max(vp.dtVtReconcileDate_FD) dtVtReconcileDate_FD 
			from  #VendPmtsMade vp 
			group by vp.strbbranchcode_fd, vp.lFFoldNo_FD, vp.nFiFoldItemID_FD) v
		ON 	db.store_id =  v.strbbranchcode_fd and 
			db.booking_id = v.lFFoldNo_FD and
			db.itemid = v.nFiFoldItemID_FD
	
Where
	bFiRefundItem_FD = 1
--

--drop table if exists #RefundProcessedRebates
Select 
	b.BookingKey,
	RefundsProcessedDate,
	item_creation_date,
	SegmentType ,
	Case
		When (#bookings.Airline = 'NZ' and dep.country_code = 'AU' and (arr.datamine_region in ('SPAC','AU') or arr.airport_code in ('DPS','HNL'))) or
		(#bookings.Airline = 'NZ' and arr.country_code = 'AU' and (dep.datamine_region in ('SPAC','AU') or dep.airport_code in ('DPS','HNL')))  Then @AmadeusRebate_NZSH --shorthaul (Shannon VSO 34382)
		When #bookings.Airline = 'NZ' and (dep.country_code <> 'AU' or arr.country_code <> 'AU') Then @AmadeusRebate_NZInt --longhaul international (test for shorthaul first)
		When #bookings.Airline = 'NZ' and dep.country_code = 'AU' and arr.country_code = 'AU' Then @AmadeusRebate_NZDom --domestic
		When #bookings.Airline = 'JQ' Then @AmadeusRebate_JQ
		When #bookings.Airline = 'VA' Then @AmadeusRebate_VA
		When #bookings.Airline = 'QF' Then @AmadeusRebate_QF
		Else @AmadeusRebate_YY
	End as AmadeusRebate
Into
	#RefundProcessedRebates
From
( select distinct r.BookingKey, r.RefundsProcessedDate, r.item_creation_date, r.SegmentType  from #RefundProcessed r) b
		Join #bookings on
			b.BookingKey = #bookings.BookingKey
		Join EDW_Reporting.dbo.dim_Location dep on
			#bookings.DepCity = dep.airport_code
		Join EDW_Reporting.dbo.dim_Location arr on
			#bookings.ArrCity = arr.airport_code
where
	#bookings.Airline <> ''

--Select * from #RefundProcessedRebates
--Select * from #bookings


Print 'Refund Processed logic end'
--Select top 10 * from #BaseDataItem
--delete #BaseDataItem from #BaseDataItem where bFiRefundItem_FD = 1

--delete #BaseDataItem from #BaseDataItem where bFiRefundItem_FD = 1


drop table if exists #ReportData
Print 'ReportData'
Select 
	cast('General' as varchar(50)) as MainGroup, 
	cast('Number of Pax' as varchar(50)) Category, 
	min(BookingPaxCount) as ActualValue, 
	BookingKey,
	min(item_creation_Date) booking_creation_date,
	cast(3 as int) SortBy
into
	#ReportData
 from 	
	#BaseDataItem b
 where
	SegmentType IN ('Airfare', 'HOTEL', 'Insurance')
 group by 
	BookingKey, 
	booking_creation_date

Union

Select 
	'General' as MainGroup, 
	'Number of Bookings' Category, 
	1 as ActualValue,
	BookingKey , 
	min(item_creation_Date),
	cast(6 as int) SortBy

 from 	
	#BaseDataItem b
 where
	SegmentType IN ('Airfare', 'HOTEL', 'Insurance')	
 group by 
	BookingKey
UNION
Select 
	cast('General' as varchar(50)) as MainGroup, 
	cast('Number of Ticket Pax' as varchar(50)) Category, 
	min(BookingPaxCount) as ActualValue, 
	BookingKey,
	min(item_creation_Date) booking_creation_date,
	cast(9 as int) SortBy

 from 	
	#BaseDataItem b
 where
	SegmentType IN ('Airfare')	
 group by 
	BookingKey, 
	booking_creation_date

Union

Select 
	'General' as MainGroup, 
	'Number of Tickets' Category, 
	1 as ActualValue,
	BookingKey , 
	min(item_creation_Date),
	cast(12 as int) SortBy

 from 	
	#BaseDataItem b
 where
	SegmentType = 'Airfare'	
 group by 
	BookingKey

Union

Select 
	cast('General' as varchar(50)) as MainGroup, 
	cast('Number of Insurance Pax' as varchar(50)) Category, 
	min(BookingPaxCount) as ActualValue, 
	BookingKey,
	min(item_creation_Date) booking_creation_date,
	cast(15 as int) SortBy
 from 	
	#BaseDataItem b
 where
	SegmentType IN ('Insurance')
 group by 
	BookingKey, 
	booking_creation_date

union

Select 
	'General' as MainGroup, 
	'Number of Insurance Bookings' Category, 
	1 as ActualValue,
	BookingKey , 
	min(item_creation_Date),
	cast(18 as int) SortBy

 from 	
	#BaseDataItem b
 where
	SegmentType IN ('Insurance')	
 group by 
	BookingKey

Union

Select 
	cast('General' as varchar(50)) as MainGroup, 
	cast('Number of Hotel Pax' as varchar(50)) Category, 
	min(BookingPaxCount) as ActualValue, 
	BookingKey,
	min(item_creation_Date) booking_creation_date,
	cast(23 as int) SortBy
 from 	
	#BaseDataItem b
 where
	SegmentType IN ('HOTEL')
 group by 
	BookingKey, 
	booking_creation_date

union

Select 
	'General' as MainGroup, 
	'Number of Hotel Bookings' Category, 
	1 as ActualValue,
	BookingKey , 
	min(item_creation_Date),
	cast(25 as int) SortBy

 from 	
	#BaseDataItem b
 where
	SegmentType IN ('HOTEL')	
 group by 
	BookingKey

union

Select 
	'General' as MainGroup, 
	'Lost Booking Fee' Category, 
	sum(LostBookingFee_Apportioned), 
	BookingKey,
	 booking_creation_date,
	cast(30 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey , booking_creation_date
union


Select 
	'General' as MainGroup, 
	'Refund Requested (Gross)' Category, 
	sum(retail_sell_price_inc_gst_inc_tax), 
	BookingKey, 
	item_creation_date,
	cast(33 as int) SortBy
 from 	
	#RefundProcessed b
 where
	SegmentType = 'Airfare'	
	--and BookingKey = 'ho398216'
 group by
	 BookingKey, 
	 item_creation_date


Union

Select 
	'General' as MainGroup, 
	'Total Ticket Sell Price' Category, 
	sum(retail_sell_price_inc_gst_inc_tax), 
	BookingKey, 
	item_creation_date,
	cast(35 as int) SortBy
 from 	
	#BaseDataItem b 
 where
	SegmentType = 'Airfare'	
	--and BookingKey = 'ho398216'
 group by
	 BookingKey, 
	 item_creation_date



union
Select 
	'General' as MainGroup, 
	'GST on Ticket' Category, 
	sum(retail_sell_gst), 
	BookingKey, 
	item_creation_date,
	cast(40 as int) SortBy
 from 	
	#BaseDataItem b
 where
	retail_sell_gst <> 0 and
	SegmentType = 'Airfare'	
 group by
	 BookingKey, 
	 item_creation_date

union

Select 
	'General' as MainGroup, 
	'Total Nett Ticket Sales' Category, 
	sum(retail_sell_price_inc_gst_inc_tax) -sum(retail_sell_gst), 
	BookingKey, 
	item_creation_date,
	cast(43 as int) SortBy
 from 	
	#BaseDataItem b
 where
	SegmentType = 'Airfare'	
 group by
	 BookingKey, 
	 item_creation_date
------ Refund Processed
union
Select 
	'General' as MainGroup, 
	'Refund Processed' Category, 
	sum(buyprice), 
	BookingKey, 
	dtVtReconcileDate_FD,
	cast(45 as int) SortBy
 from 	
	#RefundProcessed b
 where
	SegmentType = 'Airfare' AND
	RefundsProcessedDate is not null
 group by
	 BookingKey, 
	 dtVtReconcileDate_FD
union
Select 
	'General' as MainGroup, 
	'GST on Refund Processed' Category, 
	sum(retail_sell_gst), 
	BookingKey, 
	dtVtReconcileDate_FD,
	cast(47 as int) SortBy
 from 	
	#RefundProcessed b
 where
	SegmentType = 'Airfare' AND
	RefundsProcessedDate is not null
 group by
	 BookingKey, 
	 dtVtReconcileDate_FD

union

Select 
	'General' as MainGroup, 
	'Total Nett Refund Processed' Category, 
	sum(buyprice) -sum(retail_sell_gst), 
	BookingKey, 
	dtVtReconcileDate_FD,
	cast(50 as int) SortBy
 from 	
	#RefundProcessed b
 where
	SegmentType = 'Airfare' AND
	RefundsProcessedDate is not null
 group by
	 BookingKey, 
	 dtVtReconcileDate_FD



union
-- Insuracnce 
Select 
	'General' as MainGroup, 
	'Insurance Sales' Category, 
	sum(retail_sell_price_inc_gst_inc_tax),  
	BookingKey, 
	item_creation_date,
	cast(55 as int) SortBy
 from 	
	#BaseDataItem b
 where
	retail_sell_price_inc_gst_inc_tax <> 0 and
	SegmentType = 'Insurance'	
 group by
	 BookingKey, 
	 item_creation_date

union
Select 
	'General' as MainGroup, 
	'GST on Insurance' Category, 
	sum(retail_sell_gst), 
	BookingKey, 
	item_creation_date,
	cast(60 as int) SortBy
 from 	
	#BaseDataItem b
 where
	retail_sell_gst <> 0 and
	SegmentType = 'Insurance'	
 group by
	 BookingKey, 
	 item_creation_date

union
Select 
	'General' as MainGroup, 
	'Total Nett Insurance' Category, 
	sum(retail_sell_price_inc_gst_inc_tax) -  sum(retail_sell_gst), 
	BookingKey, 
	item_creation_date,
	cast(65 as int) SortBy
 from 	
	#BaseDataItem b
 where
	SegmentType = 'Insurance'	
 group by
	 BookingKey, 
	 item_creation_date

union
-- Hotel 
Select 
	'General' as MainGroup, 
	'Hotel Sales' Category, 
	sum(retail_sell_price_inc_gst_inc_tax),  
	BookingKey, 
	item_creation_date,
	cast(70 as int) SortBy
 from 	
	#BaseDataItem b
 where
	retail_sell_price_inc_gst_inc_tax <> 0 and
	SegmentType = 'Hotel'	
 group by
	 BookingKey, 
	 item_creation_date

union
Select 
	'General' as MainGroup, 
	'GST on Hotel' Category, 
	sum(retail_sell_gst), 
	BookingKey, 
	item_creation_date,
	cast(75 as int) SortBy
 from 	
	#BaseDataItem b
 where
	retail_sell_gst <> 0 and
	SegmentType = 'Hotel'	
 group by
	 BookingKey, 
	 item_creation_date

union
Select 
	'General' as MainGroup, 
	'Total Nett Hotel' Category, 
	sum(retail_sell_price_inc_gst_inc_tax) -  sum(retail_sell_gst), 
	BookingKey, 
	item_creation_date,
	cast(80 as int) SortBy
 from 	
	#BaseDataItem b
 where
	SegmentType = 'Hotel'	
 group by
	 BookingKey, 
	 item_creation_date

----------------------------------
Union

Select 
	'Income' as MainGroup, 
	'Amadeus Hotel Rebate' Category, 
	4 as ActualValue, -- $4 per booking hardcoded 
	BookingKey , 
	min(item_creation_Date),
	cast(107 as int) SortBy

 from 	
	#BaseDataItem b
 where
	SegmentType IN ('HOTEL')	
 group by 
	BookingKey

Union

Select 
	'Income' as MainGroup, 
	'Amadeus Ticket Rebate' Category, 
	sum(AmadeusRebate_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(115 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date


Union
Select 
	'Income' as MainGroup, 
	'Amendment Fee' Category, 
	sum(AmendmentFee_Apportioned), 
	BookingKey, 
	item_creation_Date booking_creation_date,
	cast(120 as int) SortBy
 from 	
	#BaseDataItem b
 group by
	 BookingKey, 
	 item_creation_Date


Union

Select 
	'Income' as MainGroup, 
	'Booking Fee' Category, 
	sum(BookingFee_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(130 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Income' as MainGroup, 
	'Consultant Fee Domestic' Category, 
	Sum(Consultant_Domestic_Apportioned), 
	BookingKey, 
	item_creation_Date booking_creation_date,
	cast(140 as int) SortBy
 from 	
	#BaseDataItem b
 group by 
	BookingKey, 
	item_creation_Date

Union

Select 
	'Income' as MainGroup, 
	'Consultant Fee International' Category, 
	sum(Consultant_International_Apportioned), 
	BookingKey, 
	item_creation_Date booking_creation_date,
	cast(150 as int) SortBy
 from 	
	#BaseDataItem b
 group by
	 BookingKey, 
	 item_creation_Date

Union

Select 
	'Income' as MainGroup, 
	'Credit Card Fee Hotel' Category, 
	sum(CreditCardFeeIncome_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(160 as int) SortBy
 from 	
	#FinalData b
where
	b.SegmentType = 'Hotel'
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Income' as MainGroup, 
	'Credit Card Fee Ticket' Category, 
	sum(CreditCardFeeIncome_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(170 as int) SortBy
 from 	
	#FinalData b
where
	b.SegmentType <> 'Hotel'
 group by
	 BookingKey, 
	 booking_creation_date



Union
--Hotel Comm
Select 
	'Income' as MainGroup, 
	'Hotel Commission' Category, 
	sum(retail_commission_ex_gst_ex_tax), 
	BookingKey, 
	item_creation_date booking_creation_date,
	cast(180 as int) SortBy
from 	
	#BaseDataItem b
 where
	SegmentType = 'Hotel' and
	retail_commission_ex_gst_ex_tax <> 0
	--and BookingKey = 'ho398216'
 group by
	 BookingKey, 
	 item_creation_date

union

Select 
	'Income' as MainGroup, 
	'Insurance Commission' Category, 
	sum(retail_sell_price_inc_gst_inc_tax)*.25,  
	BookingKey, 
	item_creation_date,
	cast(190 as int) SortBy
 from 	
	#BaseDataItem b
 where
	retail_sell_price_inc_gst_inc_tax <> 0 and
	SegmentType = 'Insurance'	
 group by
	 BookingKey, 
	 item_creation_date

Union

Select 
	'Income' as MainGroup, 
	'Ticket Commission' Category, 
	sum(retail_commission_ex_gst_ex_tax), 
	BookingKey, 
	item_creation_date booking_creation_date,
	cast(200 as int) SortBy
from 	
	#BaseDataItem b
 where
	SegmentType = 'Airfare' 
	--and BookingKey = 'ho398216'
 group by
	 BookingKey, 
	 item_creation_date

Union

Select 
	'Income' as MainGroup, 
	'Positive Margin' Category, 
	sum(PositiveMargin_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(220 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Income' as MainGroup, 
	'Other Fee' Category, 
	sum(Other_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(240 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date

Union
Select 
	'Income' as MainGroup, 
	'Other Fee Domestic' Category, 
	Sum(Other_Domestic_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(260 as int) SortBy
 from 	
	#FinalData b
 group by 
	BookingKey, 
	booking_creation_date

Union

Select 
	'Income' as MainGroup, 
	'Other Fee International' Category, 
	sum(Other_international_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(280 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date

--------------------------Income End

--------------------------Expense Start

--union
--Select 
--	'Expenses' as MainGroup, 
--	'Kayak Fee' Category, 
--	sum(KayakFee_Apportioned), 
--	BookingKey, 
--	booking_creation_date,
--	cast(300 as int) SortBy
-- from 	
--	#FinalData b
-- group by
--	 BookingKey, 
--	 booking_creation_date

--Union

--Select 
--	'Expenses' as MainGroup, 
--	'Sky Scanner Fee' Category, 
--	sum(SkyScannerFee_Apportioned), 
--	BookingKey, 
--	booking_creation_date,
--	cast(310 as int) SortBy
-- from 	
--	#FinalData b
-- group by
--	 BookingKey, 
--	 booking_creation_date
--Union

--Select 
--	'Expenses' as MainGroup, 
--	'Cheap Flights Fee' Category, 
--	sum(CheapFlightsFee_Apportioned), 
--	BookingKey, 
--	booking_creation_date,
--	cast(315 as int) SortBy
-- from 	
--	#FinalData b
-- group by
--	 BookingKey, 
--	 booking_creation_date

--------------------------Expense Start

Union

Select 
	'Expenses' as MainGroup, 
	'Booking Fee Discount' Category, 
	sum(BookingFeeDiscount_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(303 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'Credit Card Fee Hotel- Expenses' Category, 
	sum(CreditCardFeeExpence_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(310 as int) SortBy
 from 	
	#FinalData b
 where
	b.SegmentType = 'Hotel' and
	CreditCardFeeExpence_Apportioned <> 0
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'Credit Card Fee Tickets- Expenses' Category, 
	sum(CreditCardFeeExpence_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(320 as int) SortBy
 from 	
	#FinalData b
 where
	b.SegmentType <> 'Hotel' and
	CreditCardFeeExpence_Apportioned <> 0
 group by
	 BookingKey, 
	 booking_creation_date

union

Select 
	'Expenses' as MainGroup, 
	'Evoucher Hotel' Category, 
	sum(Evoucher_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(330 as int) SortBy
 from 	
	#FinalData b
 where
	b.SegmentType = 'Hotel'
 group by
	 BookingKey, 
	 booking_creation_date

Union

--NegativeMargin_Apportioned
Select 
	'Expenses' as MainGroup, 
	'Evoucher Ticket' Category, 
	sum(Evoucher_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(340 as int) SortBy
 from 	
	#FinalData b
 where
	b.SegmentType <> 'Hotel'
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'General Discount' Category, 
	sum(GeneralDiscount_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(350 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date


Union

Select 
	'Expenses' as MainGroup, 
	'Hotel Only Discount' Category, 
	sum(retail_sell_price_inc_gst_inc_tax), 
	BookingKey, 
	booking_creation_date,
	cast(360 as int) SortBy
 from 	
	#BaseData b
 where
	b.BookingKey in (Select distinct BookingKey from #BaseData h where h.SegmentType = 'Hotel') AND
	b.BookingKey not in (Select distinct BookingKey from #BaseData h where h.SegmentType =  'Airfare' ) AND
	item_details = 'HOTO Campaigns' AND
	retail_sell_price_inc_gst_inc_tax < 0
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'Negative Margin Hotel' Category, 
	sum(NegativeMargin_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(370 as int) SortBy
 from 	
	#FinalData b 
 where
	b.SegmentType = 'Hotel'
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'Negative Margin Ticket' Category, 
	sum(NegativeMargin_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(380 as int) SortBy
 from 	
	#FinalData b 
 where
	b.SegmentType <> 'Hotel'
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'Online Seat Only Discount' Category, 
	sum(OnlineSeatOnlyDiscount_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(390 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date


	 Union

Select 
	'Expenses' as MainGroup, 
	'Rounding' Category, 
	sum(Rounding_Apportioned), 
	BookingKey, 
	booking_creation_date,
	cast(400 as int) SortBy
 from 	
	#FinalData b
 group by
	 BookingKey, 
	 booking_creation_date

union

Select 
	'Profit/Loss' as MainGroup, 
	'Refund Rebate - Hotel' Category, 
	-1*sum(AmadeusRebate), 
	BookingKey, 
	item_creation_date,
	--RefundsProcessedDate,
	cast(9920 as int) SortBy
 from 	
	#RefundProcessedRebates b
 where
	RefundsProcessedDate is not null AND 
	SegmentType = 'Hotel'

 group by
	 BookingKey, 
	 item_creation_date--RefundsProcessedDate

union

Select 
	'Profit/Loss' as MainGroup, 
	'Refund Rebate - Ticket' Category, 
	-1*sum(AmadeusRebate), 
	BookingKey, 
	item_creation_date,
	--RefundsProcessedDate,
	cast(9950 as int) SortBy
 from 	
	#RefundProcessedRebates b
 where
	RefundsProcessedDate is not null AND 
	SegmentType = 'Airfare'

 group by
	 BookingKey, 
	 item_creation_date--RefundsProcessedDate




-------------------------------------Kayak Fee And SkyScanner Fee Based on 
--Select top 1000 * from #ReportData where category like '%sell p%'
Select
	Max(r.MainGroup) as MainCategory,
	Category, 
	ActualValue, 
	SortBy,
	r.booking_creation_date,
	r.BookingKey,
	Max(bkg_dest_datamine_region_modified) bkg_dest_datamine_region,
	Max(plated_carrier_reported) plated_carrier_reported,
	cast(case 
		when PATINDEX('%[^0-9]%', Max(rtrim(ltrim(OnlineBookingID)))) =0 and 
			 isnumeric(Max(rtrim(ltrim(OnlineBookingID)))) =1
	
		 then Max(rtrim(ltrim(OnlineBookingID))) else 0 end as bigint) OnlineBookingID,
	cast('' as varchar(255)) as	Source,
	cast(0 as Money) as KayakSkyscannerPercentage,
	cast(0 as Money) as KayakSkyscannerFee
into
	#calcKayakSkyScanner

from 
	#ReportData r join
	#FinalData b
		on r.BookingKey = b.BookingKey and
			b.SegmentType = 'Airfare' 
where
	Category = 'Total Ticket Sell Price'
group by
	Category, 
	ActualValue, 
	SortBy,
	r.booking_creation_date,
	r.BookingKey

drop table if exists [#Searches]

--Select OnlineBookingID, * from #calcKayakSkyScanner f where f.OnlineBookingID =  '2712370' 
Declare @SqlSearchesString as nVarChar(4000)

CREATE TABLE [dbo].[#Searches](
	[bookingid] [bigint] NULL,
	[sessionid] [uniqueidentifier] NOT NULL,
	[searchkey] [int] NOT NULL,
	[eventtypedescription] [varchar](100) NOT NULL,
	[pagepseudonym] [varchar](50) NOT NULL,
	[eventaction] [varchar](50) NULL,
	[insertedtime] [datetime2](7) NULL,
	[notes] [varchar](500) NULL,
	[validationmessage] [varchar](6750) NULL,
	[info1] [varchar](100) NULL,
	[info2] [varchar](100) NULL,
	[info3] [varchar](100) NULL,
	[bookingfee] [numeric](18, 2) NULL,
	[region] [varchar](20) NULL,
	[origin] [char](3) NOT NULL,
	[destination] [char](3) NOT NULL,
	[departing] [datetime2](7) NOT NULL,
	[returning] [datetime2](7) NULL,
	[ipaddress] [varchar](20) NULL,
	[clientbrowser] [varchar](255) NULL,
	[domainname] [varchar](128) NULL,
	[Source] [varchar](100)  NULL
) 



Set @SqlSearchesString = 'Select * From OpenQuery(Hot_log_allServer,''
			select distinct
			bi.bookingid,
			 sa.sessionid,sa.searchkey,e.eventtypedescription,p.pagepseudonym,sa.eventaction,
						  sa.insertedtime,sa.notes,sa.validationmessage,sa.info1,sa.info2,sa.info3,
						  sa.bookingfee,sa.region,sd.origin,sd.destination,sd.departing,sd.returning
						  ,sd.ipaddress,sd.clientbrowser,sd.domainname,
			  sd.source
			from hot_log_all.[dbo].sessionactivity sa
			inner join hot_log_all.[dbo].bookingitem bi on bi.sessionid = sa.sessionid
			inner join hot_log_all.[dbo].pagepseudonym p on p.pageid = sa.pageid
			inner join hot_log_all.[dbo].eventtype e on e.eventtypeid = sa.eventtypeid
			inner join hot_log_all.[dbo].searchdetails sd on sa.sessionid = sd.sessionid and sa.searchkey = sd.searchkey
			where sa.origin = bi.origincode --and bookingid = @bookingid
			order by sa.insertedtime desc
		'')'

--SELECT @SqlSearchesString
Insert Into #Searches 
EXECUTE sp_executesql @SqlSearchesString

--Select * from #Searches where  Source like '%cheap%'



update 
	#calcKayakSkyScanner
set 
	Source = s.Source
from
	#calcKayakSkyScanner o JOIN
	#Searches s
		On o.OnlineBookingID = s.bookingid
where
	s.Source Like '%skyscanner%' or
	s.Source Like '%Kayak%' 

	/*
	Select * from #Output where nullif(Source, '') is not null

Declare @SkyScannerFixedPercent_Asia as Money = .02
Declare @SkyScannerFixedPercent_Au as Money = .0125
Declare @SkyScannerFixedPercent_NZ as Money = .0125
Declare @SkyScannerFixedPercent_Oth as Money = .02
Declare @SkyScannerFixedPercent_SPAC as Money = .02
Declare @SkyScannerFixedPercent_UKEUR as Money = .02
Declare @SkyScannerFixedPercent_USCA as Money = .02
Declare @KayakFixedPercent as Money = .0175

--*/

update 
	#calcKayakSkyScanner
set 
	KayakSkyscannerPercentage = CASE 
									WHEN Source = 'skyscanner' AND bkg_dest_datamine_region = 'NZ' THEN @SkyScannerFixedPercent_NZ
									WHEN Source = 'skyscanner' AND bkg_dest_datamine_region = 'AU' THEN @SkyScannerFixedPercent_Au
									WHEN Source = 'skyscanner' AND bkg_dest_datamine_region = 'Asia' THEN @SkyScannerFixedPercent_Asia
									WHEN Source = 'skyscanner' AND bkg_dest_datamine_region = 'SPAC' THEN @SkyScannerFixedPercent_SPAC
									WHEN Source = 'skyscanner' AND bkg_dest_datamine_region = 'UKEUR' THEN @SkyScannerFixedPercent_UKEUR
									WHEN Source = 'skyscanner' AND bkg_dest_datamine_region = 'USCA' THEN @SkyScannerFixedPercent_USCA
									WHEN Source = 'Kayak' THEN @KayakFixedPercent
									ELSE @SkyScannerFixedPercent_Oth
								END
from
	#calcKayakSkyScanner o 
 where
	 nullif(Source, '') is not null


update 
	#calcKayakSkyScanner
set 
	KayakSkyscannerFee = KayakSkyscannerPercentage* ActualValue
from
	#calcKayakSkyScanner o 
 where
	 nullif(Source, '') is not null



delete from #ReportData where Category in ('Kayak Fee', 'Sky Scanner Fee') AND MainGroup = 'Expenses'

insert into #ReportData
Select 
	'Expenses' as MainGroup, 
	'Kayak Fee' Category, 
	-1*sum(KayakSkyscannerFee), 
	BookingKey, 
	booking_creation_date,
	cast(300 as int) SortBy
 from 	
	#calcKayakSkyScanner b
 where
	Source = 'Kayak'
 group by
	 BookingKey, 
	 booking_creation_date

Union

Select 
	'Expenses' as MainGroup, 
	'Sky Scanner Fee' Category, 
	-1*sum(KayakSkyscannerFee), 
	BookingKey, 
	booking_creation_date,
	cast(310 as int) SortBy
 from 	
	#calcKayakSkyScanner b
 where
	Source = 'skyscanner'
 group by
	 BookingKey, 
	 booking_creation_date

---*********************************************Totals
--Total Nett Air

insert into #ReportData

Select 
	'General' as MainGroup, 
	'Total Nett Air' Category, 
	sum(ActualValue), 
	BookingKey, 
	booking_creation_date,
	(Select max(SortBy) from #ReportData where category IN ('Total Nett Ticket Sales', 'Total Nett Refund Processed')) + 1
from 	
	#ReportData b
where
	MainGroup = 'general' and
	category IN ('Total Nett Ticket Sales', 'Total Nett Refund Processed')
	--and booking_creation_date between '2020-07-01' and '2020-09-17'
	--and b.BookingKey = 'HO433538'
 group by
	 BookingKey, 
	 booking_creation_date

--Total Income

insert into #ReportData

Select 
	'Total Income' as MainGroup, 
	'Total Income' Category, 
	sum(ActualValue), 
	BookingKey, 
	booking_creation_date,
	(Select max(SortBy) from #ReportData where MainGroup = 'income') + 1
from 	
	#ReportData b
where
	MainGroup = 'Income' and
	category NOT IN ('Other Fee Domestic', 'Other Fee International', 'Consultant Fee Domestic', 'Consultant Fee International')
	--and booking_creation_date between '2020-07-01' and '2020-09-17'
	--and b.BookingKey = 'HO433538'
 group by
	 BookingKey, 
	 booking_creation_date
--Select distinct Category from #ReportData

--Total Expenses

insert into #ReportData
Select 
	'Total Expenses' as MainGroup, 
	'Total Expenses' Category, 
	sum(ActualValue), 
	BookingKey, 
	booking_creation_date,
	(Select max(SortBy) from #ReportData where MainGroup = 'Expenses') + 1

from 	
	#ReportData b

where
	MainGroup = 'Expenses'  --and
	--category NOT IN ('Sky Scanner Fee', 'Kayak Fee')
	--and booking_creation_date between '2019-07-01' and '2020-09-17'
	--and b.BookingKey = 'HO433538'
 group by
	 BookingKey, 
	 booking_creation_date

--Profit/Loss

insert into #ReportData
Select 
	'Profit/Loss' as MainGroup, 
	'Profit/Loss' Category, 
	sum(ActualValue), 
	BookingKey, 
	booking_creation_date,
	9999
from
	(
	Select 
		sum(ActualValue) ActualValue, 
		BookingKey, 
		booking_creation_date
	from 	
		#ReportData b
	where
		Category = 'Total Income'
		--and booking_creation_date between '2020-07-01' and '2020-09-17'
	 group by
		 BookingKey, 
		 booking_creation_date

	UNION ALL
	
	Select 
		sum(ActualValue), 
		BookingKey, 
		booking_creation_date
	from 	
		#ReportData b
	where
		Category = 'Total Expenses'

	 group by
		 BookingKey, 
		 booking_creation_date
		 ) pp
--where booking_creation_date between '2019-07-01' and '2020-09-17'	and pp.BookingKey = 'HO433538'
group by
		 BookingKey, 
		 booking_creation_date



--drop table #FinalOutput
--4095263
Select
	Max(r.MainGroup) as MainCategory,
	case when r.SortBy = MinSortBy then r.MainGroup else '' end MainGroup, 
	Category, 
	ActualValue, 
	SortBy,
	r.booking_creation_date,
	r.BookingKey,
	Min(SegmentType) as FirstSegmentType,
	Max(bkg_dest_datamine_region_modified) bkg_dest_datamine_region,
	Max(plated_carrier_reported) plated_carrier_reported,
	Max(flight_city_pair) flight_city_pair,
	Max(BookingIsInbound) BookingIsInbound,
	Max(BookingPayment) BookingPayment,
	Max(BookingPaymentType) BookingPaymentType,
	Max(BookingPaxCount) as BookingPaxCount,
	Max(b.Booking_Departure_date) as Booking_Departure_date,
	Max(booking_departure_datamine_region) booking_departure_datamine_region,
	cast(Max(case when Category like '%Total %' or Category='Profit/Loss' then 'Black' else 'White' end) as varchar(50)) as Color,
	cast(Max(case when Category like '%Total %' or Category='Profit/Loss' then 'White' else 'Black' end) as varchar(50)) as ForeColor,
	Max(b.BookingFeeLostBookingFeeStatus) BookingFeeLostBookingFeeStatus,
	cast(case 
		when PATINDEX('%[^0-9]%', Max(rtrim(ltrim(OnlineBookingID)))) =0 and 
			 isnumeric(Max(rtrim(ltrim(OnlineBookingID)))) =1
	
		 then Max(rtrim(ltrim(OnlineBookingID))) else 0 end as bigint) OnlineBookingID,
	cast('' as varchar(255)) as	Source,
	cast(0 as Money) as KayakSkyscannerPercentage,
	cast(0 as Money) as KayakSkyscannerFee,
	cast('' as varchar(50)) as CategorySubGroup
into
	#FinalOutput

from 
	#ReportData r left join
	#FinalData b
		on r.BookingKey = b.BookingKey and
			b.SegmentType IN ('Airfare', 'HOTEL') join
	(Select MainGroup, min(SortBy) MinSortBy from #ReportData group by MainGroup) m
		on m.MainGroup = r.MainGroup 
--where
	--r.BookingKey = 'HO444984'
group by 
	case when r.SortBy = MinSortBy then r.MainGroup else '' end, 
	Category, 
	ActualValue, 
	SortBy,
	r.booking_creation_date,
	r.BookingKey
order by 
	case when r.SortBy = MinSortBy then r.MainGroup else '' end, 
	Category


--Select * from #FinalData where BookingKey = 'HO475668'
update 
	#FinalOutput
set 
	Source = o.Source,
	KayakSkyscannerPercentage = o.KayakSkyscannerPercentage,
	KayakSkyscannerFee = o.KayakSkyscannerFee
from
	#calcKayakSkyScanner o JOIN
	#FinalOutput s
		On s.OnlineBookingID = o.OnlineBookingID
where
	o.Source Like '%skyscanner%' or
	o.Source Like '%Kayak%'

--Select * from #Searches
--Select top 10 * from #FinalOutput

update 
	#FinalOutput
set 
	[Source] = s.[Source]
--Select top 10 o.*, s.*
from
	#FinalOutput o JOIN
	#Searches s
		On o.OnlineBookingID = s.bookingid
where
	nullif( o.[Source], '') is null and
	eventtypedescription = 'Book'
	


update 
	#FinalOutput
set 
	Source = s.Source
from
	#FinalOutput o JOIN
	#Searches s
		On o.OnlineBookingID = s.bookingid
where
	nullif( o.Source, '') is null 

update 
	#FinalOutput
set 
	Source = 'Other'
where
	nullif(Source, '') is null 


update 
	#FinalOutput
set 
	CategorySubGroup  ='Hotel',
	Color = '#33F9FF'
from
	#FinalOutput s
		
where
	s.Category Like '%Hotel%'


update 
	#FinalOutput
set 
	CategorySubGroup  ='Insurance',
	Color = '#FFD133'
from
	#FinalOutput s
		
where
	s.Category Like '%Insurance%'

update 
	#FinalOutput
set 
	CategorySubGroup  ='Airfare',
	Color = '#3386FF'
from
	#FinalOutput s
		
where
	s.Category Like '%Airfare%' OR
	s.Category Like '%Ticket%'

update 
	#FinalOutput
set 
	Color = 'Black',
	ForeColor = 'White'
from
	#FinalOutput s
		
where
	s.Category Like '%Total%' OR
	Category='Profit/Loss'

Select
	distinct
	MainCategory, 	
	MainGroup, 	
	Category, 	
	ActualValue, 	
	SortBy, 	
	cast(booking_creation_date as DATE) as booking_creation_date, 
	BookingKey, 	
	bkg_dest_datamine_region, 	
	plated_carrier_reported, 	
	flight_city_pair, 	
	BookingIsInbound, 	
	BookingPayment, 	
	BookingPaymentType, 	
	BookingPaxCount, 	
	Booking_Departure_date, 	
	booking_departure_datamine_region, 
	Color, 	
	ForeColor, 	
	BookingFeeLostBookingFeeStatus, 	
	OnlineBookingID,
	Source,
	KayakSkyscannerPercentage,
	KayakSkyscannerFee,
	FirstSegmentType,
	CategorySubGroup 
 from 
	#FinalOutput
 WHERE
	(OnlineBookingID <> 0 OR 
		ActualValue <> 0.00
		)
	--and BookingKey = 'HO475668'

Print 'ReportData end'

--Select * from #MainTable where FolderNo ='1430'
--Select * from #BMM_Folders where FolderNo= '1491'
	--Select retail_sell_price_inc_gst_inc_tax, retail_sell_gst, * from #BaseData where BookingKey ='AU1862'
--LostBookingFee_Apportioned
	 --retail_sell_price_inc_gst_inc_tax
	 --retail_commission_ex_gst_ex_tax
	 --AmadeusRebate_Apportioned
	 --CreditCardFeeIncome_Apportioned
	 --PositiveMargin_Apportioned
	 --BookingFee_Apportioned
	 --AmendmentFee_Apportioned
	 --AmendmentFee_Apportioned
/*
Select 
	sum(ActualValue) ActualValue, 
	Category,
	MainGroup,
	sortBy
from 
	#ReportData f 
where 
	f.booking_creation_date between '2020-06-01' and  '2020-06-30'
group by 
	MainGroup, Category, sortBy
order by 
	 sortBy


Select 
	sum(ActualValue) ActualValue, 
	Category,
	MainGroup,
	sortBy
from 
	#ReportData f 
where 
	f.item_creation_date between '2020-06-01' and  '2020-06-30'
group by 
	MainGroup, Category, sortBy
order by 
	MainGroup, sortBy


Cast(0 as Money) as AmadeusRebate_Booking,
	Cast(0 as Money)  as AmadeusRebate_Apportioned,
	Cast(0 as Int)  as SearchEngineFeeID_Booking,
	Cast(0 as Money)  as SkyScannerFee_Apportioned,
	Cast(0 as Money)  as CreditCardFeeIncome_Booking,
	Cast(0 as Money)  as CreditCardFeeIncome_Apportioned,
	Cast(0 as Money)  as CreditCardFeeExpence_Apportioned,
	Cast(0 as Money)  as BookingFee_Booking,
	Cast(0 as Money)  as BookingFee_Apportioned,
	Cast(0 as Money)  as NegativeMargin_Booking,
	Cast(0 as Money)  as NegativeMargin_Apportioned,
	Cast(0 as Money)  as PositiveMargin_Booking,
	Cast(0 as Money)  as PositiveMargin_Apportioned,
	Cast(0 as Money)  as BookingFeeDiscount_Booking,
	Cast(0 as Money)  as BookingFeeDiscount_Apportioned,
	Cast(0 as Money)  as LostBookingFee_Booking,
	Cast(0 as Money)  as LostBookingFee_Apportioned,
	Cast(0 as Money)  as Rounding_Booking,
	Cast(0 as Money)  as Rounding_Apportioned,
	Cast(0 as Money)  as GeneralDiscount_Booking,
	Cast(0 as Money)  as GeneralDiscount_Apportioned,
	Cast(0 as Money)  as AmendmentFee_Booking,
	Cast(0 as Money)  as AmendmentFee_Apportioned,
	Cast(0 as Money)  as OnlineSeatOnlyDiscount_Booking,
	Cast(0 as Money)  as OnlineSeatOnlyDiscount_Apportioned,
	Cast(0 as Money)  as Other_Booking,
	Cast(0 as Money)  as Other_Apportioned,
	Cast(0 as Money)  as Evoucher_Booking,
	Cast(0 as Money)  as Evoucher_Apportioned,
	Cast(0 as Money)  as KayakFee_Apportioned,	
	Cast(0 as Money)  as CheapFlightsFee_Apportioned


	
Select 
	sum(ActualValue) ActualValue, 
	Category,
	MainGroup,
	sortBy
from 
	#ReportData f 
where 
	f.booking_creation_date between '2019-09-30' and  '2020-02-03'
group by 
	MainGroup, Category, sortBy
order by 
	MainGroup, sortBy
--exec Proc_Ecom_BookingProfitAndLossReport_ByItemCreationDate_PBI
*/

--Select 
--	'General' as MainGroup, 
--	'Number of Pax' Category, 
--	avg(BookingPaxCount) as ActualValue, 
--	BookingKey,
--	item_creation_date,
--	Max(bkg_dest_datamine_region) bkg_dest_datamine_region,
--	Max(plated_carrier_reported) plated_carrier_reported,
--	Max(flight_city_pair) flight_city_pair,
--	Max(BookingIsInbound) BookingIsInbound,
--	Max(BookingPayment) BookingPayment,
--	Max(BookingPaymentType) BookingPaymentType,
--	Max(booking_departure_datamine_region) booking_departure_datamine_region,
--	case when Category like '%Total%' then 'Black' else 'blue' end Color
-- from 	
--	#BaseDataItem b
-- where
--	SegmentType = 'Airfare' --and
--	--BookingKey = 'ho398216'
-- group by 
--	BookingKey,
--	item_creation_date, 
--	booking_creation_date,
--	bkg_dest_datamine_region,
--	plated_carrier_reported,
--	flight_city_pair,
--	BookingIsInbound,
--	BookingPayment,
--	BookingPaymentType


--Exec Proc_MMAU_BookingProfitAndLossReport_PBI
--Select * from rpt_ECOM_PowerBI_ProfitAndLossReportData

