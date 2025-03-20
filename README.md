# Assignment 10

## Universal Acceptance Criteria

These criteria apply to all assignments, regardless of specific requirements:

1. You must understand every single line of your solution.
2. Your code must compile and run without errors.
3. You must submit your repository URL in Brightspace.

## Assignment Requirements

1. Implement `EndpointHandler.cs`.
    - Look for `NotImplementedException` and replace that with your correct code.
2. Write tests in `EndpointHandlerTests.cs`.
    - Ensure all tests pass.
    - Achieve at least `80%` code coverage.
3. Implement API endpoints in `Program.cs` according to the specification below.
    - Look for `NotImplementedException` and replace that with your correct code.
4. Write tests in `MinimalApiTests.cs`.
    - Ensure all tests pass.
    - Achieve at least `80%` code coverage.

## Bonus Requirements

1. Improve `EndpointHandlerTests.cs` to achieve at least `95%` code coverage.

## Information

### Class Diagram of `Bank.API`

```mermaid
classDiagram
    class Program {
        +Main(string args)
    }

    class IEndpointHandler {
        <<interface>>
        +CreateAccountAsync() IResult
        +DeleteAccountAsync(int accountId) IResult
        +GetAccountAsync(int accountId) IResult
        +GetDefaultSettingsAsync() IResult
        +WithdrawAsync(int accountId, double amount) IResult
        +DepositAsync(int accountId, double amount) IResult
        +GetTransactionHistoryAsync(int accountId) IResult
        +AddTransactionAsync(int accountId, string type, double amount) IResult
        +ListAccountsAsync() IResult
    }

    class EndpointHandler {
        +Storage Storage
        +CreateAccountAsync() IResult
        +DeleteAccountAsync(int accountId) IResult
        +ListAccountsAsync() IResult
        -WrapperAsync(Func<IResult> action) IResult
    }

    class Storage {
        +ListAccounts() int
        +AddAccount() Account
        +GetAccount(int id) Account?
        +RemoveAccount(int id)
        -SaveChanges()
    }

    EndpointHandler ..|> IEndpointHandler
```

### API Endpoints

Use the existing endpoints as a guide and pattern to follow. This means using `EndpointHandler` and inside `EndpointHandler` methods, copy the patterns already there illustrating how to do this correctly.

#### **Create an account** → `POST /account`

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API (Program.cs)
    participant Handler as EndpointHandler
    participant Storage as Storage

    Client->>API: POST /account
    API->>Handler: CreateAccountAsync()
    Handler->>Storage: AddAccount()
    Storage->>Storage: Generate new account ID
    Storage->>Storage: SaveChanges()
    Storage-->>Handler: Returns new Account
    Handler-->>API: Returns HTTP 200 with Account
    API-->>Client: HTTP 200 OK (Account JSON)
```

Creates a new account with the specified settings.
```csharp
var client = new HttpClient();
var url = "https://api.example.com/account";
var payload = new Account { Id = 123, Settings = new AccountSettings { OverdraftFee = 25.00 } };
var response = await client.PostAsJsonAsync(url, payload);
```

#### **Get account details** → `GET /account/{accountId}`

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API (Program.cs)
    participant Handler as EndpointHandler
    participant Storage as Storage

    Client->>API: GET /account
    API->>Handler: GetAccountAsync(accountId)
    Handler->>Storage: GetAccount(accountId)
    Storage-->>Handler: Return Account
    Handler-->>API: Return HTTP 200 with Account JSON
    API-->>Client: HTTP 200 OK (Account JSON)
```
##### Endpoint Workflow


```mermaid
flowchart TD
    A(GET /account) -->|Valid accountId| B(Fetch Account from Storage)
    B -->|Account Found| C(Return HTTP 200 with Account JSON)
    B -->|Account Not Found| D(Return HTTP 404 Not Found)
    A -->|Invalid accountId| E(Return HTTP 400 Bad Request)
```

Retrieves details for the specified account.
```csharp
var client = new HttpClient();
var url = "https://api.example.com/account/123";
var response = await client.GetAsync(url);
var account = await response.Content.ReadFromJsonAsync<Account>();
```

#### **Withdraw funds** → `POST /withdraw/{accountId}/{amount}`

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API (Program.cs)
    participant Handler as EndpointHandler
    participant Storage as Storage

    Client->>API: POST /withdraw
    API->>Handler: WithdrawAsync(accountId, amount)
    Handler->>Storage: GetAccount(accountId)
    Storage-->>Handler: Return Account
    Handler->>Storage: Check Sufficient Funds
    Storage-->>Handler: Funds Available?
    Handler-->>API: Return HTTP 200 OK or 400 Bad Request
    API-->>Client: HTTP 200 OK or 400 Bad Request
```
##### Endpoint Workflow


```mermaid
flowchart TD
    A(POST /withdraw) -->|Valid Input| B(Fetch Account from Storage)
    B -->|Account Found| C(Check Sufficient Funds)
    C -->|Sufficient Funds| D(Update Balance & Save Changes)
    D --> E(Return HTTP 200 OK)
    C -->|Insufficient Funds| F(Return HTTP 400 Bad Request)
    B -->|Account Not Found| G(Return HTTP 404 Not Found)
    A -->|Invalid Input| H(Return HTTP 400 Bad Request)
```

Withdraws a specified amount from an account if funds allow.
```csharp
var client = new HttpClient();
var url = "https://api.example.com/withdraw/123/50.00";
var payload = new Transaction { Id = 123, Type = TransactionType.Withdraw, Amount = 50.00 };
var response = await client.PostAsJsonAsync(url, payload);
```

#### **Deposit funds** → `POST /deposit/{accountId}/{amount}`

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API (Program.cs)
    participant Handler as EndpointHandler
    participant Storage as Storage

    Client->>API: POST /deposit
    API->>Handler: DepositAsync(accountId, amount)
    Handler->>Storage: GetAccount(accountId)
    Storage-->>Handler: Return Account
    Handler->>Storage: Update Balance & Save Changes
    Storage-->>Handler: Return Updated Account
    Handler-->>API: Return HTTP 200 OK
    API-->>Client: HTTP 200 OK
```
##### Endpoint Workflow


```mermaid
flowchart TD
    A(POST /deposit) -->|Valid Input| B(Fetch Account from Storage)
    B -->|Account Found| C(Update Balance & Save Changes)
    C --> D(Return HTTP 200 OK)
    B -->|Account Not Found| E(Return HTTP 404 Not Found)
    A -->|Invalid Input| F(Return HTTP 400 Bad Request)
```

Deposits a specified amount into an account.
```csharp
var client = new HttpClient();
var url = "https://api.example.com/deposit/123/100.00";
var payload = new Transaction { Id = 123, Type = TransactionType.Deposit, Amount = 100.00 };
var response = await client.PostAsJsonAsync(url, payload);
```

#### **Get transaction history** → `GET /transactions/{accountId}`

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API (Program.cs)
    participant Handler as EndpointHandler
    participant Storage as Storage

    Client->>API: GET /transactions/{accountId}
    API->>Handler: GetTransactionHistoryAsync(accountId)
    Handler->>Storage: Get Transactions(accountId)
    Storage-->>Handler: Return Transactions
    Handler-->>API: Return HTTP 200 with Transactions
    API-->>Client: HTTP 200 OK (Transactions JSON)
```
##### Endpoint Workflow

```mermaid
flowchart TD
    A(GET /transactions) -->|Valid accountId| B(Fetch Transactions from Storage)
    B -->|Transactions Found| C(Return HTTP 200 with Transactions JSON)
    B -->|No Transactions| D(Return HTTP 200 with Empty List)
    A -->|Invalid accountId| E(Return HTTP 400 Bad Request)
    A -->|Account Not Found| F(Return HTTP 404 Not Found)
```

Retrieves a list of transactions for an account.
```csharp
var client = new HttpClient();
var url = "https://api.example.com/transactions/123";
var response = await client.GetAsync(url);
var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>();
```

#### **Add a specialty transaction** → `POST /transaction/{accountId}/{type}/{amount}`

```mermaid
sequenceDiagram
    participant Client
    participant API as Minimal API (Program.cs)
    participant Handler as EndpointHandler
    participant Storage as Storage

    Client->>API: POST /transaction
    API->>Handler: AddTransactionAsync(accountId, type, amount)
    Handler->>Storage: Validate Account
    Storage-->>Handler: Return Account
    Handler->>Storage: Validate Transaction Type
    Storage-->>Handler: Type Valid?
    Handler-->>API: Return HTTP 200 OK or 400 Bad Request
    API-->>Client: HTTP 200 OK or 400 Bad Request
```
##### Endpoint Workflow

```mermaid
flowchart TD
    A(POST /transaction) -->|Valid Input| B(Fetch Account from Storage)
    B -->|Account Found| C(Validate Transaction Type)
    C -->|Valid Type| D(Add Transaction & Save Changes)
    D --> E(Return HTTP 200 OK)
    C -->|Invalid Type| F(Return HTTP 400 Bad Request)
    B -->|Account Not Found| G(Return HTTP 404 Not Found)
    A -->|Invalid Input| H(Return HTTP 400 Bad Request)
```

Adds a transaction type (e.g., overdraft fee or interest) to an account.
```csharp
var client = new HttpClient();
var url = "https://api.example.com/transaction/123/Fee_Overdraft/15.00";
var payload = new Transaction { Id = 123, Type = TransactionType.Fee_Overdraft, Amount = 15.00 };
var response = await client.PostAsJsonAsync(url, payload);
```

## Running and Debugging Tests

To verify your implementation, use the C# Dev Kit **Unit Test Runner** and **Code Coverage** button. If tests do not run correctly, try these steps:

```bash
dotnet clean
dotnet build
dotnet test --collect:"Coverage"
```

If issues persist, restart VS Code and ensure your dependencies are installed correctly.

