using System;
using System.Collections.Generic;
using System.Text;

namespace QuestionsReview
{
    public class Question
    {
        public string ID { get; set; }
        public string BatchID { get; set; }
        public string QuestionDesc { get; set; }
        public string AnswerDesc { get; set; }
        public string AnswerRef { get; set; }
    }

    public class ReviewItem
    {
        public string BatchID { get; set; }
        public string QuestionID { get; set; }
        public string Answer { get; set; }

        public override string ToString()
        {
            return $"{BatchID}-{QuestionID}: {Answer}";  
        }
    }

    public class ReviewLog
    {
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public string ReviewPattern { get; set; }
        public List<ReviewItem> ReviewItems { get; set; }
        public string ReviewSummary { get; set; }        
        public string ReviewDetail { get; set; }
        public string IncorrectItemsPattern { get; set; }
        public string UnfinishedItemsPattern { get; set; }
        public string UncertainItemsPattern { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Review Log from {StartTime.ToString("yyyy-MM-dd HH:mm:ss")} to {FinishTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"Review Pattern: {ReviewPattern}");
            sb.AppendLine($"Review Summary: ");
            sb.AppendLine($"{ReviewSummary}");
            if (!ReviewSummary.StartsWith("A"))
            {
                sb.AppendLine($"Review Detail:");
                sb.AppendLine(ReviewDetail);

                
            }
            
            return sb.ToString();
        }
    }

   
}
