# Dynamic Group Chat Coding Task

This project demonstrates a dynamic group chat coding task that involves converting Oracle PL/SQL code to .NET C# code. The code is designed to be easily adaptable for reading data from a production Oracle database using Entity Framework and LINQ.

## Features

- Reads data from hardcoded CSV files for testing.
- Breaks down the converting for PL/SQL to C# into logical tasks.
- Uses agents for orchestration, coding, review and running of code.
- Verifies that output of testdata of produced logic matches expected input.

## Prerequisites

- .NET SDK
- Visual Studio Code or any other C# compatible IDE
- At point of testing only GPT-4 model was working correctly. Issues with both 3.5-turbo and GPT-4o.
  Configure endpoints by setting enviroment variables:
  - Azure: AZURE_OPENAI_API_KEY and AZURE_OPENAI_ENDPOINT
  - OpenAI: OPENAI_API_KEY
     

## Setup

1. Clone the repository:
    ```sh
    git clone https://github.com/your-repo/multiagent-demo-dotnet.git
    cd multiagent-demo-dotnet
    ```

2. Open the project in Visual Studio Code:
    ```sh
    code .
    ```

3. Restore the dependencies:
    ```sh
    dotnet restore
    ```

## Running the Project

1. Build the project:
    ```sh
    dotnet build
    ```

2. Run the project:
    ```sh
    dotnet run
    ```

## Project Structure

- `Dynamic_GroupChat_Coding_Task.cs`: Main file containing the logic for reading CSV data and processing it.
- `testdata-sql-plsql.sql`: SQL script for PL/SQL.
- `testdata-sql-tables.sql`: SQL script for table definitions.
- `testdata-csv-departments.csv`: CSV file containing department data.
- `testdata-csv-employees.csv`: CSV file containing employee data.
- `testdata-csv-salaries.csv`: CSV file containing salary data.

## Example Output

For Department 1, the expected output is:
Emp ID: 201, Name: John Doe, Salary: 50000, Bonus: 5000
Emp ID: 202, Name: Jane Smith, Salary: 55000, Bonus: 5500 
Total Salary for Department 1: 115500


## Future Enhancements

- Implement logic to read data from a production Oracle database using Entity Framework.
- Use LINQ to query the data from the Oracle database.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.
