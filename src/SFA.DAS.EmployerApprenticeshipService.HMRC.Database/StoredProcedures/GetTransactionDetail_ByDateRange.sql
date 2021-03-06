﻿CREATE PROCEDURE [levy].[GetTransactionDetail_ByDateRange]
	@accountId bigint,
	@fromDate datetime,
	@toDate datetime

AS
select 
    tl.AccountId,
    tl.LevyDeclared as Amount,
    t.Amount as EnglishFraction,
    ldt.Amount as TopUp,
    tl.EmpRef,
	null as CourseName,
	null as ProviderName,
    tl.TransactionDate,
    tl.Amount as LineAmount,
    tl.TransactionType
from levy.TransactionLine tl
inner join levy.LevyDeclarationTopup ldt on ldt.SubmissionId = tl.SubmissionId
OUTER APPLY
(
	SELECT TOP 1 Amount
	FROM [levy].[EnglishFraction] ef
	WHERE ef.EmpRef = tl.empRef 
		AND ef.[DateCalculated] <= tl.TransactionDate
	ORDER BY [DateCalculated] DESC
) t
where    tl.TransactionDate >= @fromDate AND 
        tl.TransactionDate <= @toDate AND 
        tl.AccountId = @accountId

union all

select 
    tl.AccountId,
    null as Amount,
    null as EnglishFraction,
    null as TopUp,
    null as empref,
	meta.ApprenticeshipCourseName as CourseName,
	meta.ProviderName as ProviderName,
    (tl.TransactionDate) as transactiondate,
    (p.Amount) as LineAmount,
    tl.TransactionType
from levy.TransactionLine tl
inner join levy.Payment p on p.PeriodEnd = tl.PeriodEnd and p.AccountId = tl.AccountId
inner join levy.PaymentMetaData meta on p.PaymentMetaDataId = meta.Id
where   tl.TransactionDate >= @fromDate AND 
        tl.TransactionDate <= @toDate AND 
        tl.AccountId = @accountId