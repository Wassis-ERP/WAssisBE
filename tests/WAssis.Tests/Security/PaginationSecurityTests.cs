using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Customers.Interfaces;
using WAssis.Application.Modules.Customers.Queries;
using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;
using WAssis.Application.Modules.WhatsAppSupport.Queries;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;

namespace WAssis.Tests.Security;

public sealed class PaginationSecurityTests
{
    [Fact]
    public async Task CustomerListClampsInvalidPageAndExcessivePageSize()
    {
        var repository = new CustomerRepository();
        var result = await new ListInsuredPeopleQueryHandler(repository)
            .Handle(new ListInsuredPeopleQuery(null, null, -5, 10_000), default);

        Assert.Equal(1, repository.Page);
        Assert.Equal(100, repository.PageSize);
        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task WhatsAppQueueClampsServerSidePageSize()
    {
        var repository = new WhatsAppRepository();
        await new GetWhatsAppQueueQueryHandler(repository)
            .Handle(new GetWhatsAppQueueQuery(0, int.MaxValue), default);

        Assert.Equal(1, repository.Page);
        Assert.Equal(100, repository.PageSize);
    }

    private sealed class CustomerRepository : IInsuredPersonReadRepository
    {
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public Task<InsuredPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<InsuredPerson?>(null);
        public Task<PagedResult<InsuredPerson>> ListAsync(string? search, string? status, int page, int pageSize, CancellationToken cancellationToken)
        {
            Page = page;
            PageSize = pageSize;
            return Task.FromResult(new PagedResult<InsuredPerson>([], page, pageSize, 0));
        }
    }

    private sealed class WhatsAppRepository : IWhatsAppConversationRepository
    {
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public Task AddAsync(WhatsAppConversation conversation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<WhatsAppConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<WhatsAppConversation?>(null);
        public Task<int> CountWaitingHumanHandoffsAsync(CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<int> CountHumanActiveAsync(CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<PagedResult<WhatsAppConversationDto>> GetWaitingQueueAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            Page = page;
            PageSize = pageSize;
            return Task.FromResult(new PagedResult<WhatsAppConversationDto>([], page, pageSize, 0));
        }
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
