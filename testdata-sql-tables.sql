CREATE TABLE employees (
    emp_id NUMBER PRIMARY KEY,
    first_name VARCHAR2(50),
    last_name VARCHAR2(50),
    dept_id NUMBER
);

CREATE TABLE departments (
    dept_id NUMBER PRIMARY KEY,
    dept_name VARCHAR2(50),
    manager_id NUMBER
);

CREATE TABLE salaries (
    emp_id NUMBER PRIMARY KEY,
    salary NUMBER,
    bonus NUMBER,
    CONSTRAINT fk_emp_id FOREIGN KEY (emp_id) REFERENCES employees(emp_id)
);