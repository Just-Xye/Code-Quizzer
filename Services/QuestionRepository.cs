using Code_Quizzer.Models;

namespace Code_Quizzer.Services
{
    public static class QuestionRepository
    {
        public static List<Question> Questions = new()
    {
        new Question
        {
            Id = 1,
            Language = "Python",
            Difficulty = "Easy",
            QuestionText = "What keyword defines a function in Python?",
            Choices = new()
            {
                "function",
                "func",
                "def",
                "define"
            },
            CorrectAnswer = "def",
            Explanation = "Python uses the 'def' keyword to define functions."
        },

        new Question
        {
            Id = 2,
            Language = "Python",
            Difficulty = "Easy",
            QuestionText = "Which data type stores multiple values?",
            Choices = new()
            {
                "int",
                "list",
                "bool",
                "float"
            },
            CorrectAnswer = "list",
            Explanation = "Lists store collections of multiple values."
        },

        new Question
        {
            Id = 3,
            Language = "Python",
            Difficulty = "Easy",
            QuestionText = "What is the output of print(2 ** 3)?",
            Choices  = new()
            {
                "5",
                "6",
                "8",
                "9"
            },
            CorrectAnswer = "8",
            Explanation = "The '**' operator in Python is the exponentiation operator, so 2 ** 3 means 2 raised to the power of 3, which equals 8."
        },

        new Question
        {
            Id = 4,
            Language = "Python",
            Difficulty = "Easy",
            QuestionText = "Which of the following is a valid variable name in Python?",
            Choices = new()
            {
                "1variable",
                "variable-name",
                "variable_name",
                "variable name"
            },
            CorrectAnswer = "variable_name",
            Explanation = "In Python, variable names must start with a letter or an underscore and can only contain letters, numbers, and underscores. 'variable_name' is the only valid option among the choices."
        },

        new Question
        {
            Id = 5,
            Language = "Python",
            Difficulty = "Easy",
            QuestionText = "What is the correct way to create a list in Python?",
            Choices = new()
            {
                "my_list = ()",
                "my_list = []",
                "my_list = {}",
                "my_list = <>"
            },
            CorrectAnswer = "my_list = []",
            Explanation = "In Python, lists are created using square brackets []. The other options represent different data structures: () for tuples, {} for dictionaries, and <> is not a valid syntax for any data structure."
        }
    };
    }
}
