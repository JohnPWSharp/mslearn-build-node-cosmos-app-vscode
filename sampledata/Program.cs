using System;
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace GenerateData
{
    class Program
    {
        static string[] courseNames = { "Applied Mathematics", "Introduction to Computer Science", "Statistics", "Data Processing", "Systems Architecture", "Graphics and Video Processing", "Machine Learning", "Advanced Programming Techniques", "C# Programming", "Distributed Systems" };
        private static readonly string endpointUri = "COSMOS DB URI GOES HERE";
        private static readonly string primaryKey = "PRIMARY KEY GOES HERE";
        private static CosmosClient cosmosClient;
        private static Database database;
        private static Container collection;

        static void Main(string[] args)
        {
            cosmosClient = new CosmosClient(endpointUri, primaryKey);
            database = cosmosClient.GetDatabase("SchoolDB");
            collection = database.GetContainer("StudentCourseGrades");

            for (int i = 2010; i <= 2020; i++)
            {
                GenerateCourseData(i);
                GenerateSudentData(i);
            }
        }

        private static void GenerateSudentData(int i)
        {
            int studentRootCode = i - 2009;
            int numStudents = new Random().Next(2000) + 100;

            for (int studentNum = 1; studentNum < numStudents; studentNum++)
            {
                StudentRecord student = new StudentRecord
                {
                    id = $"SU{studentRootCode}{studentNum.ToString("0000")}",
                    AcademicYear = i.ToString(),
                    Name = new StudentName
                    {
                        Forename = $"{RandomString(1)}{RandomString(9).ToLower()}",
                        Lastname = $"{RandomString(1)}{RandomString(9).ToLower()}"
                    },
                };

                int numCoursesTaken = new Random().Next(courseNames.Length) + 1;
                Grades[] courseGrades = new Grades[numCoursesTaken];

                for (int courses = 0; courses < numCoursesTaken; courses++)
                {
                    courseGrades[courses] = new Grades
                    {                      
                        Course = courseNames[courses],
                        Grade = RandomGrade()
                    };
                }
                student.CourseGrades = courseGrades;

                collection.CreateItemAsync(student).Wait();
            }
        }
        
        private static void GenerateCourseData(int i)
        {
            int courseRootCode = i - 2009;

            for (int numCourses = 1; numCourses <= courseNames.Length; numCourses++)
            {
                CourseRecord course = new CourseRecord
                {
                    id = $"C{courseRootCode}{numCourses.ToString("00")}",
                    AcademicYear = i.ToString(),
                    CourseName = courseNames[numCourses - 1]
                };

                collection.CreateItemAsync(course).Wait();
            }
        }

        private static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string RandomGrade()
        {
            Random random = new Random();
            const string chars = "ABCDEF";
            return chars[random.Next() % 6].ToString();
        }
    }

    public struct CourseRecord
    {
        public string id;
        public string AcademicYear;
        public string CourseName;
    }

    public struct Grades
    {
        public string Course;
        public string Grade;
    };

    public struct StudentName
    {
        public string Forename;
        public string Lastname;
    };

    public struct StudentRecord
    {
        public string id;
        public string AcademicYear;
        public StudentName Name;
        public Grades [] CourseGrades;
    }
}
