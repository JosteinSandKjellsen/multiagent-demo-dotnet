CREATE OR REPLACE PROCEDURE get_department_employees (
    p_dept_id IN departments.dept_id%TYPE,
    p_total_salary OUT NUMBER
) IS
    -- Deklarere en cursor for å hente data fra employees og salaries
    CURSOR emp_cur IS
        SELECT e.emp_id, e.first_name, e.last_name, s.salary, s.bonus
        FROM employees e
        JOIN salaries s ON e.emp_id = s.emp_id
        WHERE e.dept_id = p_dept_id;

    -- Variabler for å holde data fra cursor
    v_emp_id employees.emp_id%TYPE;
    v_first_name employees.first_name%TYPE;
    v_last_name employees.last_name%TYPE;
    v_salary salaries.salary%TYPE;
    v_bonus salaries.bonus%TYPE;
    
    -- Variabel for å holde total lønn
    v_total_salary NUMBER := 0;
BEGIN
    -- Åpne cursor
    OPEN emp_cur;
    
    -- Hente data fra cursor
    LOOP
        FETCH emp_cur INTO v_emp_id, v_first_name, v_last_name, v_salary, v_bonus;
        EXIT WHEN emp_cur%NOTFOUND;
        
        -- Utskrift av data
        DBMS_OUTPUT.PUT_LINE('Emp ID: ' || v_emp_id || ', Name: ' || v_first_name || ' ' || v_last_name || ', Salary: ' || v_salary || ', Bonus: ' || v_bonus);
        
        -- Legge til lønn i total lønn
        v_total_salary := v_total_salary + v_salary + NVL(v_bonus, 0);
    END LOOP;
    
    -- Lukke cursor
    CLOSE emp_cur;
    
    -- Returnere total lønn
    p_total_salary := v_total_salary;
    
    -- Utskrift av total lønn
    DBMS_OUTPUT.PUT_LINE('Total Salary for Department ' || p_dept_id || ': ' || v_total_salary);
END;
/