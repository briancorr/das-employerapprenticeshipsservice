﻿using MediatR;

namespace SFA.DAS.EmployerApprenticeshipsService.Application.Commands.ResendInvitation
{
    public class ResendInvitationCommand : IAsyncRequest
    {
        public string Email { get; set; }
        public string AccountId { get; set; }
        public string ExternalUserId { get; set; }
    }
}