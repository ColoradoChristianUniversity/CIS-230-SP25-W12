# Assignment 12

## Universal Acceptance Criteria

1. You must understand every single line of your solution.
2. Your code must compile and run without errors.
3. You must submit your repository URL in Brightspace.

## Assignment Requirements

1. Copy the `Console App` into a `Web App`. 

    1. Use `dotnet new webapp` to create a minimal Razor Pages web UI.
    2. Your new project should reference the `Bank.App.Shared` project.
    3. Your new project should reference the `Bank.Logic` project.
    4. Ensure you put your web app in the `/Bank.App.Web` folder.
    5. Ensure your csproj file is named `Bank.App.Web.csproj`.
    6. Ensure your new project is added to the solution with `dotnet sln add`.
    7. Update `Properties/launchSettings.json` to use your custom port `2345`.
    8. Update `Properties/launchSettings.json` to delete the `https` configuration.

2. Ensure basic functionality (at least):

    - List Accounts
        - List Accounts with Balances
        - Select & View an Account
        - Create a New Account
    - View Account
        - Always Show: Name & Balance
        - Allow Deposit
            - Ask Dollar Value
        - Allow Withdraw
            - Ask Dollar Value
        - Allow Delete Account
            - Prompt the user: "Are you sure?"
        - View All Transactions
            - Show Table of Transactions

**Good luck.**