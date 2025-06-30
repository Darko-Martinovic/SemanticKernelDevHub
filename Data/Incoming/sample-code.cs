using System;

public class SampleClass
{
    // Bad practice: public field
    public string name;
    
    // Missing error handling
    public void ProcessData(string input)
    {
        var result = input.ToUpper();
        Console.WriteLine(result);
    }
    
    // No input validation
    public int Calculate(int a, int b)
    {
        return a / b; // Division by zero risk
    }
}
