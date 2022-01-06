using System;
using System.ComponentModel;
using System.Reflection;
/// <summary>
/// Fee Statuses used in Website to show state of fee items
/// </summary>
/// New         - When item for matching criteria created in Dolphin HOT_PendingFee into PendingFee ready for invoicing
/// Cancelled   - Outlet has Deleted the Fee Item and no longer wants to invoice
/// Pending     - When it has gone to Nexus message queue to be processed
/// FeeApplied  - When it is fee is successfully added
/// Failed      - When it is failed due reason such fee already exists or folder locked by user
/// UmMatched   - Fee item with no configured Fee 
public enum FeeStatus
{
    New,
    Cancelled,
    Pending,
    FeeApplied,
    Failed,
    UnMatched,
    AutoApplied
};
