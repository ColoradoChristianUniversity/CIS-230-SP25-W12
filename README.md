Here’s the updated file without the solution to the assignment:

# Assignment 01

![](Screenshot.gif)

## Universal Acceptance Criteria

This represents acceptance criteria that is true irrespective of assignment criteria.

1. You must understand every single line of your solution.
2. Your code must compile and run without errors.
3. You must turn in your repository URL in Brightspace.

## Assignment Acceptance Criteria

This represents acceptance criteria necessary for assignment completion.

1. Prompt the user for their first name using `Console.ReadLine()`.
2. Prompt the user for their last name using `Console.ReadLine()`.
3. Concatenate their first and last names: `Tim Valley` using string interpolation.
4. Prompt the user for their birth year using `Console.ReadLine()`.
5. Calculate the user's age from their birth year: `25`.
6. Update `SayHello()` to accept two parameters: `(string name, int age)`.
7. Write to the console a message like: `"Hello, Tim! Your age is 25"`.
8. Colorize the output with a blue background and yellow foreground using `ConsoleColor`.
9. After output, restart the question sequence in an easy-to-use loop. Allow the user to exit the loop by typing "exit".
10. You must use `SayHello()` in the `Logic` project to build your output string.

## Bonus Acceptance Criteria

This represents optional acceptance criteria available for additional learning and bonus.

1. Clear the console before final output using `Console.Clear()`.
2. Add error handling if the user enters a blank name.
3. Add error handling if the user enters a non-numeric value for their birth year.
4. Handle the singular/plural change in year/years when age is 1 and 2.
5. Hide the cursor in the console app using the built in command to do so. 

## Getting Started

### Clone Your Repository  
1. **Clone with Git**  
   Replace `<repository-url>` with your GitHub repository URL:  
   ```bash
   git clone <repository-url>
   cd <repository-folder>
   ```  

2. **Check Status**  
   Ensure your repository is clean:  
   ```bash
   git status
   ```  

### Create Your Project  

1. Create Solution & Project:
   ```
   dotnet new sln -n Client
   dotnet new console -n Client -o Client
   dotnet sln Client.sln add Client/Client.csproj
   ```

2. Add Configuration Files:
   ```
   dotnet new gitignore
   dotnet new editorconfig
   ```
   
### Test Your Project  

1. Restore, Build & Run:
   ```
   dotnet restore
   dotnet build
   dotnet run --project Client
   ```

### Debug in VSCode  

1. Open VSCode:
   Use the command: `code .`.

2. Configure Debugging:
   - Open Settings (`Ctrl+,`) and search for `csharp.debug.console`.
   - Set its value to `externalTerminal`.

3. Generate C# Debugging Assets:
   - Use the command palette (`Ctrl+Shift+P`) and select `.NET: Generate Assets for Build and Debug`.
   - Note the creation of `launch.json` and `tasks.json`.

4. Start Debugging:
   Use `F5`.

### Add Client.Library Class Library  

1. Create & Add Class Library:
   ```
   dotnet new classlib -n Client.Library -o Client.Library
   dotnet sln Client.sln add Client.Library/Client.Library.csproj
   dotnet add Client/Client.csproj reference Client.Library/Client.Library.csproj
   ```

2. Restore & Build:
   ```
   dotnet restore
   dotnet build
   ```

### Rename and Edit Class1.cs  
1. Rename `Client.Library/Class1.cs` to `Greeting.cs`.
3. Open `Greeting.cs` and replace the content with this new content:

   ```csharp
   namespace Client.Library;

   public class Greeting
   {
       public static string SayHello(string name)
       {
           return $"Hello, {name}!";
       }
   }
   ```  

### Update Program.cs to Use Library  

1. Open `Client/Program.cs`.
2. Replace the content with:  
   ```csharp
   using Client.Library;

   Console.WriteLine(Greeting.SayHello("World"));
   Console.ReadKey();
   ```  

### Commit Your Changes

1. Stage changes:
   ```
   git add .
   ```
2. Commit:
   ```
   git commit -m "Initial commit for Assignment 01"
   ```
3. Push:
   ```
   git push
   ```

3. **Run Updated Code**  
   ```bash
   dotnet run --project Client  
   ```  

### Now you can do your assignment

 * Read the *Acceptance Criteria*!
 * Keep committing your changes with git.
 * Remember to push your final work!
 * Turn in the URL to your repository.
