using CustomerFeedback.BuildingBlocks.Messaging;
using MassTransit;
using Reporting.Application.UseCases;

namespace Reporting.Infrastructure.Messaging;

public sealed class FeedbackSubmittedConsumer(ProjectSubmittedFeedbackHandler handler) : IConsumer<FeedbackSubmitted>
{
    public Task Consume(ConsumeContext<FeedbackSubmitted> context) =>
        handler.Handle(context.Message, context.CancellationToken);
}

public sealed class FeedbackUpdatedConsumer(ProjectUpdatedFeedbackHandler handler) : IConsumer<FeedbackUpdated>
{
    public Task Consume(ConsumeContext<FeedbackUpdated> context) =>
        handler.Handle(context.Message, context.CancellationToken);
}
