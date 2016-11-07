﻿using System.Collections.Generic;
using SFA.DAS.EAS.Domain.Entities.Account;

namespace SFA.DAS.EAS.Application.Queries.GetAllEmployerAccounts
{
    public class GetAllEmployerAccountsResponse
    {
        public List<Account> Accounts { get; set; }
    }
}