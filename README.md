# Dynamic Group Chat Coding Task

This project demonstrates a dynamic group chat coding task that involves reading data from CSV files and processing it to produce specific output. The code is designed to be easily adaptable for reading data from a production Oracle database using Entity Framework and LINQ.

## Features

- Reads data from hardcoded CSV files.
- Processes the data to produce a formatted output.
- Designed to be easily adaptable for reading from an Oracle database.
- Uses record types for data structures.

## Prerequisites

- .NET SDK
- Visual Studio Code or any other C# compatible IDE

## Setup

1. Clone the repository:
    ```sh
    git clone https://github.com/your-repo/dynamic-group-chat-coding-task.git
    cd dynamic-group-chat-coding-task
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
