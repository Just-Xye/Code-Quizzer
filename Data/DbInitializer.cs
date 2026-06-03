using Code_Quizzer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Code_Quizzer.Data
{
    public static class DbInitializer
    {
        public static void SeedData(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Wipe broken historical question records without choices for a clean re-seed
            if (context.Questions.Any() && !context.QuestionChoices.Any())
            {
                Console.WriteLine("--> Empty choice records detected. Wiping staging tables for clean re-seed...");
                context.Questions.ExecuteDelete();
            }
            else if (context.Questions.Any())
            {
                return; // Skipped if data already populated properly
            }

            string seedFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData");
            if (!Directory.Exists(seedFolderPath))
            {
                seedFolderPath = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData");
            }

            var seedFiles = Directory.GetFiles(seedFolderPath, "*.json", SearchOption.TopDirectoryOnly)
                                     .Where(f => f.EndsWith("Seed.json", StringComparison.OrdinalIgnoreCase))
                                     .ToList();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var file in seedFiles)
            {
                try
                {
                    var jsonString = File.ReadAllText(file);

                    // 2. FIX: Map the JSON data to your session-based 'Question' model 
                    // because its structural schema matches your JSON array layout!
                    var rawData = JsonSerializer.Deserialize<List<Question>>(jsonString, options);

                    if (rawData == null) continue;

                    foreach (var item in rawData)
                    {
                        // 3. Map values across to the SQL Table 'QuestionEntity'
                        var questionEntity = new QuestionEntity
                        {
                            Language = item.Language,
                            Difficulty = item.Difficulty,
                            Category = item.Category,
                            QuestionText = item.QuestionText,
                            CorrectAnswer = item.CorrectAnswer,
                            Explanation = item.Explanation,
                            Choices = new List<QuestionChoice>()
                        };

                        // 4. FIX: Safely parse out the native string array sequence directly!
                        if (item.Choices != null && item.Choices.Any())
                        {
                            foreach (var text in item.Choices)
                            {
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    questionEntity.Choices.Add(new QuestionChoice
                                    {
                                        ChoiceText = text
                                    });
                                }
                            }
                        }

                        context.Questions.Add(questionEntity);
                    }

                    int rowsSaved = context.SaveChanges();
                    Console.WriteLine($"--> [SUCCESS] Seeded file {Path.GetFileName(file)}. Added {rowsSaved} relational rows.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> [ERROR] Failed parsing file {Path.GetFileName(file)}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}