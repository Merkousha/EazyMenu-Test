using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Payments.VerifyPayment;

public sealed record VerifyPaymentCommand(Guid PaymentId, string Authority, string Status) : ICommand<VerifyPaymentResult>;
