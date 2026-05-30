using Trackr.Api.Api.Accounts;
using Trackr.Api.Api.Categories;
using Trackr.Api.Api.Health;
using Trackr.Api.Api.RecurringTransactions;
using Trackr.Api.Api.Transactions;

namespace Trackr.Api;

public partial class TrackrApiClient
{
    public AccountsRequestBuilder Accounts => Api.Accounts;

    public CategoriesRequestBuilder Categories => Api.Categories;

    public HealthRequestBuilder Health => Api.Health;

    public TransactionsRequestBuilder Transactions => Api.Transactions;

    public RecurringTransactionsRequestBuilder RecurringTransactions => Api.RecurringTransactions;
}
