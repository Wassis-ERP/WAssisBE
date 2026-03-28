using WAssis.Domain.Modules.WhatsAppSupport.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Tests.Modules.WhatsAppSupport;

public sealed class WhatsAppConversationTests
{
    [Fact]
    public void Assign_ShouldMoveConversationToHumanActive_AndSetAssignment()
    {
        var conversation = WhatsAppConversation.Create(
            "tenant-wa",
            "corr-wa-1",
            "5511999999999",
            "cliente pediu ajuda",
            WhatsAppConversationPriority.High);

        conversation.RequestHumanHandoff();
        conversation.Assign("user-1", "Atendente 1");

        Assert.Equal(WhatsAppConversationStatus.HumanActive, conversation.Status);
        Assert.Equal("user-1", conversation.AssignedToUserId);
        Assert.Equal("Atendente 1", conversation.AssignedToDisplayName);
        Assert.NotNull(conversation.StartedHumanAtUtc);
    }

    [Fact]
    public void Close_ShouldFinalizeConversation()
    {
        var conversation = WhatsAppConversation.Create(
            "tenant-wa",
            "corr-wa-2",
            "5511888888888",
            "ultima mensagem",
            WhatsAppConversationPriority.Normal);

        conversation.Close("encerrado pelo operador");

        Assert.Equal(WhatsAppConversationStatus.Closed, conversation.Status);
        Assert.Equal("encerrado pelo operador", conversation.LastMessagePreview);
        Assert.NotNull(conversation.ClosedAtUtc);
    }
}
