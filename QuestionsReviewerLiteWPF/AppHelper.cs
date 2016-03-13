using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QuestionsReviewerLiteWPF
{
    public static class AppHelper
    {
        public static string LoadText(this string filename)
        {
            return File.ReadAllText(filename, Encoding.UTF8);
        }

        public static IEnumerable<string> ParseElements(this string raw)
        {
            var pattern = @"\d+、.+?(?=(\d+、)|Z{5})";
            var matches = Regex.Matches(raw, pattern, RegexOptions.Singleline | RegexOptions.Multiline);
            var results = from m in matches.Cast<Match>() select m.Groups[0].Value;
            return results;
        }

        public static IEnumerable<Question> InitializeQuestions(this string folder)
        {
            var global = new List<Question>();

            //target file pattern: Questions_1.txt
            var files = from f in Directory.GetFiles(folder)
                        where f.Contains("Questions")
                        select f;

            foreach (var file in files)
            {
                var ptnBatchID = @"Questions_(\d+)";
                var batchID = Regex.Match(file, ptnBatchID, RegexOptions.Singleline).Groups[1].Value;

                var rawQuestions = file.LoadText().ParseElements();

                var results = from q in rawQuestions
                              let length = q.IndexOf(@"、")
                              let questionID = q.Substring(0, length)
                              select new Question
                              {
                                  ID = questionID,
                                  BatchID = batchID,
                                  QuestionDesc = q
                              };

                global.AddRange(results);

            }

            return global;

        }

        public static IEnumerable<Question> EnrichAnswersOn(this string folder, IEnumerable<Question> questions)
        {
            var global = new List<Question>();

            //target file pattern: Answers_1.txt
            var files = from f in Directory.GetFiles(folder)
                        where f.Contains("Answers")
                        select f;

            foreach (var file in files)
            {
                var ptnBatchID = @"Answers_(\d+)";
                var batchID = Regex.Match(file, ptnBatchID, RegexOptions.Singleline).Groups[1].Value;

                var rawAnwsers = file.LoadText().ParseElements();

                var results = from a in rawAnwsers
                              let length = a.IndexOf(@"、")
                              let answerID = a.Substring(0, length)
                              let question = (from q in questions where q.BatchID == batchID && q.ID == answerID select q).FirstOrDefault()
                              select new Question
                              {
                                  ID = question.ID,
                                  BatchID = question.BatchID,
                                  QuestionDesc = question.QuestionDesc,
                                  AnswerDesc = a
                              };

                global.AddRange(results);

            }


            return global;
        }

        public static IEnumerable<Question> FilterBy(this IEnumerable<Question> questions,
            string filter)
        {
            var global = new List<Question>();

            if (filter.ToUpper().StartsWith("RE>"))//advanced mode
            {
                //limited version
                ////Re>b:[25];n:^1[1-9]$;q:meeting;a:meeting
                //var re_Patterns = filter.Substring(3).Split(";".ToCharArray());

                //var temp_Target = questions.ToList();

                //foreach (var re_Pattern in re_Patterns)
                //{
                //    if (re_Pattern.ToUpper().StartsWith("B:"))
                //    {
                //        var batch_Pattern = re_Pattern.Substring(2);
                //        var target = from q in temp_Target
                //                     where Regex.IsMatch(q.BatchID, batch_Pattern)
                //                     select q;
                //        temp_Target = target.ToList();
                //    }
                //    else if (re_Pattern.ToUpper().StartsWith("N:"))
                //    {
                //        var number_Pattern = re_Pattern.Substring(2);
                //        var target = from q in temp_Target
                //                     where Regex.IsMatch(q.ID, number_Pattern)
                //                     select q;
                //        temp_Target = target.ToList();
                //    }
                //    else if (re_Pattern.ToUpper().StartsWith("Q:"))
                //    {
                //        var question_Pattern = re_Pattern.Substring(2);
                //        var target = from q in temp_Target
                //                     where Regex.IsMatch(q.QuestionDesc, question_Pattern, RegexOptions.IgnoreCase)
                //                     select q;
                //        temp_Target = target.ToList();
                //    }
                //    else if (re_Pattern.ToUpper().StartsWith("A:"))
                //    {
                //        var answer_Pattern = re_Pattern.Substring(2);
                //        var target = from q in temp_Target
                //                     where Regex.IsMatch(q.AnswerDesc, answer_Pattern, RegexOptions.IgnoreCase)
                //                     select q;
                //        temp_Target = target.ToList();
                //    }
                //    else
                //    {
                //        temp_Target = questions.ToList();
                //    }
                //}

                //global.AddRange(temp_Target);

            }

            else//basic mode
            {
                //filter pattern
                //1:45,78-81,100;2:[23]0,1;3:67;
                var batches = filter.Split(";".ToCharArray());
                foreach (var b in batches)
                {
                    var elements = b.Split(":".ToCharArray());
                    var batchID = elements[0];
                    if (!String.IsNullOrEmpty(batchID))
                    {
                        var patterns = elements[1].Split(",".ToCharArray());

                        foreach (var p in patterns)
                        {
                            if (Regex.IsMatch(p, @"^\d+$"))
                            {
                                var target = from q in questions
                                             where q.BatchID == batchID && q.ID == p
                                             select q;

                                global.AddRange(target);
                            }

                            else if (Regex.IsMatch(p, @"^(\d+?)\-(\d+)$"))
                            {
                                var match = Regex.Match(p, @"^(\d+?)\-(\d+)$");
                                var start = Int32.Parse(match.Groups[1].Value);
                                var end = Int32.Parse(match.Groups[2].Value);

                                if (end > start)
                                {
                                    var target = from q in questions
                                                 let id = Int32.Parse(q.ID)
                                                 where q.BatchID == batchID && id >= start && id <= end
                                                 select q;

                                    global.AddRange(target);
                                }

                                if (end == start)
                                {
                                    var target = from q in questions
                                                 let id = Int32.Parse(q.ID)
                                                 where q.BatchID == batchID && id == start
                                                 select q;

                                    global.AddRange(target);
                                }
                            }


                            else //match Regex
                            {
                                var target = from q in questions
                                             where q.BatchID == batchID && Regex.IsMatch(q.ID, p)
                                             select q;

                                global.AddRange(target);
                            }


                        }

                    }

                }

            }



            return global;
        }
    }
}
