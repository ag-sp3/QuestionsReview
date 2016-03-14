using System;

namespace QuestionsReviewerLiteWPF
{
    [Serializable]
    public class Question
    {
        
        public string ID { get; set; }
        public string BatchID { get; set; }
        public string QuestionDesc { get; set; }
        public string AnswerDesc { get; set; }
    }
}
