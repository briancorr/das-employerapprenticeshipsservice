﻿using System;

namespace SFA.DAS.EmployerApprenticeshipsService.Domain
{
    public class EmployerAgreementView
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public EmployerAgreementStatus Status { get; set; }
        public string SignedByName { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LegalEntityRegisteredAddress { get; set; }
        public int TemplateId { get; set; }
        public string TemplateText { get; set; }
    }
}