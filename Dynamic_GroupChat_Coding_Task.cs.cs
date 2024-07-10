﻿using AutoGen;
using AutoGen.Core;
using AutoGen.BasicSample;
using AutoGen.DotnetInteractive;
using AutoGen.OpenAI;

public partial class Dynamic_GroupChat_Coding_Task
{
    public static async Task RunAsync()
    {
        var instance = new Dynamic_GroupChat_Coding_Task();

        var sqlFolderPath = Directory.GetCurrentDirectory();
        var plsqlFilePath = Path.Combine(sqlFolderPath, "testdata-sql-plsql.sql");
        var tablesFilePath = Path.Combine(sqlFolderPath, "testdata-sql-tables.sql");

        var plsqlScript = await File.ReadAllTextAsync(plsqlFilePath);
        var tablesScript = await File.ReadAllTextAsync(tablesFilePath);

        var departmentsFilePath = Path.Combine(sqlFolderPath, "testdata-csv-departments.csv");
        var employeesFilePath = Path.Combine(sqlFolderPath, "testdata-csv-employees.csv");
        var salariesFilePath = Path.Combine(sqlFolderPath, "testdata-csv-salaries.csv");

        var departmentsData = await File.ReadAllTextAsync(departmentsFilePath);
        var employeesData = await File.ReadAllTextAsync(employeesFilePath);
        var salariesData = await File.ReadAllTextAsync(salariesFilePath);

        // setup dotnet interactive
        var workDir = Path.Combine(Path.GetTempPath(), "InteractiveService");
        if (!Directory.Exists(workDir))
            Directory.CreateDirectory(workDir);

        using var service = new InteractiveService(workDir);
        var dotnetInteractiveFunctions = new DotnetInteractiveFunction(service);

        await service.StartAsync(workDir, default);

        var gptConfig = LLMConfiguration.GetAzureOpenAIGPT4();

        var groupAdmin = new GPTAgent(
            name: "groupAdmin",
            systemMessage: "You are the admin of the group chat",
            temperature: 0f,
            config: gptConfig);

        var userProxy = new UserProxyAgent(name: "user", defaultReply: GroupChatExtension.TERMINATE, humanInputMode: HumanInputMode.NEVER)
            .RegisterPrintMessage();

        // Create admin agent
        var admin = new AssistantAgent(
            name: "admin",
            systemMessage: """
            You are a manager who takes coding problem from user and resolve problem by splitting them into small tasks and assign each task to the most appropriate agent.
            Here's available agents who you can assign task to:
            - coder: write dotnet code to resolve task
            - runner: run dotnet code from coder

            The workflow is as follows:
            - You take the coding problem from user
            - You break the problem into small tasks. For each tasks you first ask coder to write code to resolve the task. Once the code is written, you ask runner to run the code. If code contains unit tests you can ask runner to run the unit tests successfully before running the code.
            - Once a small task is resolved, you summarize the completed steps and create the next step.
            - Don't pass code to coder in context, coder should hold the code for each task and just incrementally add code to resolve the task.
            - You repeat the above steps until the coding problem is resolved.

            You can use the following json format to assign task to agents:
            ```task
            {
                "to": "{agent_name}",
                "task": "{a short description of the task}",
                "context": "{previous context from scratchpad}"
            }
            ```

            If you need to ask user for extra information, you can use the following format:
            ```ask
            {
                "question": "{question}"
            }
            ```

            Once the coding problem is resolved, summarize each steps and results and send the summary to the user using the following format:
            ```summary
            {
                "problem": "{coding problem}",
                "steps": [
                    {
                        "step": "{step}",
                        "result": "{result}"
                    }
                ]
            }
            ```

            Your reply must contain one of [task|ask|summary] to indicate the type of your message.
            """,
            llmConfig: new ConversableAgentConfig
            {
                Temperature = 0,
                ConfigList = [gptConfig],
            })
            .RegisterPrintMessage();

        // create coder agent
        var coderAgent = new GPTAgent(
            name: "coder",
            systemMessage: """
            You act as dotnet coder, you write dotnet code to resolve task. Once you finish writing code, ask runner to run the code for you.
            Here're some rules to follow on writing dotnet code:
            - put code between ```csharp and ```
            - Try to use `var` instead of explicit type.
            - Try avoid using external library, use .NET Core library instead.
            - Use top level statement to write code.
            - Always print out the result to console. Don't write code that doesn't print out anything.
            - Use standarized strict code style.
            - Write DRY code. Don't repeat yourself.
            - For any function written, write XML documentation comments for the function.
            - For any function written, make sure it is testable. Write unit tests for any functions you write.
            - Use Xunit + FluentAssertions for unit tests.

            If you need to install nuget packages, put nuget packages in the following format:
            ```nuget
            nuget_package_name
            ```

            If your code is incorrect, Fix the error and send the code again.
            """,
            config: gptConfig,
            temperature: 0.4f)
            .RegisterPrintMessage();

        // code reviewer agent will review if code block from coder's reply satisfy the following conditions:
        var codeReviewAgent = new GPTAgent(
            name: "reviewer",
            systemMessage: """
            You are a code reviewer who reviews code from coder. You need to check if the code satisfy the following conditions:
            - The reply from coder contains at least one code block, e.g ```csharp and ```
            - There's only one code block and it's csharp code block
            - The code block is not inside a main function. a.k.a top level statement
            - Review code style. Use standarized strict code style.
            - Any function shall have XML documentation comments for the function.
            - Member names shall not be the same as their enclosing type.

            Put your comment between ```review and ```, if the code satisfies all conditions, put APPROVED in review.result field. Otherwise, put REJECTED along with comments. make sure your comment is clear and easy to understand.
            
            ## Example 1 ##
            ```review
            comment: The code satisfies all conditions.
            result: APPROVED
            ```

            ## Example 2 ##
            ```review
            comment: The code is inside main function. Please rewrite the code in top level statement.
            result: REJECTED
            ```

            """,
            config: gptConfig,
            temperature: 0f)
            .RegisterPrintMessage();

        // create runner agent
        var runner = new AssistantAgent(
            name: "runner",
            defaultReply: "No code available, coder, write code please")
            .RegisterDotnetCodeBlockExectionHook(interactiveService: service)
            .RegisterMiddleware(async (msgs, option, agent, ct) =>
            {
                var mostRecentCoderMessage = msgs.LastOrDefault(x => x.From == "coder") ?? throw new Exception("No coder message found");
                return await agent.GenerateReplyAsync([mostRecentCoderMessage], option, ct);
            })
            .RegisterPrintMessage();

        var adminToCoderTransition = Transition.Create(admin, coderAgent, async (from, to, messages) =>
        {
            // the last message should be from admin
            var lastMessage = messages.Last();
            if (lastMessage.From != admin.Name)
            {
                return false;
            }

            return true;
        });
        var coderToReviewerTransition = Transition.Create(coderAgent, codeReviewAgent);
        
        var adminToRunnerTransition = Transition.Create(admin, runner, async (from, to, messages) =>
        {
            // the last message should be from admin
            var lastMessage = messages.Last();
            if (lastMessage.From != admin.Name)
            {
                return false;
            }

            // the previous messages should contain a message from coder
            var coderMessage = messages.FirstOrDefault(x => x.From == coderAgent.Name);
            if (coderMessage is null)
            {
                return false;
            }

            return true;
        });

        var runnerToAdminTransition = Transition.Create(runner, admin);

        var reviewerToAdminTransition = Transition.Create(codeReviewAgent, admin);

        var adminToUserTransition = Transition.Create(admin, userProxy, async (from, to, messages) =>
        {
            // the last message should be from admin
            var lastMessage = messages.Last();
            if (lastMessage.From != admin.Name)
            {
                return false;
            }

            return true;
        });

        var userToAdminTransition = Transition.Create(userProxy, admin);

        var workflow = new Graph(
            [
                adminToCoderTransition,
                coderToReviewerTransition,
                reviewerToAdminTransition,
                adminToRunnerTransition,
                runnerToAdminTransition,
                adminToUserTransition,
                userToAdminTransition,
            ]);

        // create group chat
        var groupChat = new GroupChat(
            admin: groupAdmin,
            members: [admin, coderAgent, runner, codeReviewAgent, userProxy],
            workflow: workflow);

        // task 1: retrieve the most recent pr from mlnet and save it in result.txt
        var groupChatManager = new GroupChatManager(groupChat);
        await userProxy.SendAsync(groupChatManager, $$"""
            Please rewerite this Oracle PL/SQL function into a dotnet code function. Code should be written as a single program.
            The function should return the results as a logical structure. Then print it similar to the PL/SQL output.

            PL/SQL function:
             ```
            {{plsqlScript}}
             ```

            For context, here's the tables used in the PL/SQL function:
            ```
            {{tablesScript}}
            ```

            This is for testing purpose, so you don't need to make code for connecting to Oracle database. Instead use the data from the following csv data when testing logic. The csv data contains the following data:
            departments.csv:
            ```
            {{departmentsData}}
            ```
            employees.csv:
            ```
            {{employeesData}}
            ```
            salaries.csv:
            ```
            {{salariesData}}
            ```

            When you are satisfied with the code, send the code to runner to run the code and present the result to user. For the test use hardcoded CSV data provided above. No need for logic reading for CSV-files.
            Logic should be written so it can easiliy be changed to read from the production Oracle database. That solution will use entity framework to read data from Oracle database and LINQ to query the data.
            Use record type instead of class for the data structure. This needs to be included in a single program.

            The expected output for Department 1 should be:
            ```
            Emp ID: 201, Name: John Doe, Salary: 50000, Bonus: 5000
            Emp ID: 202, Name: Jane Smith, Salary: 55000, Bonus: 5500
            Total Salary for Department 1: 115500
            ```

            """, maxRound: 30);
    }
}