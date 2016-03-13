using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuestionsReviewerLiteWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IEnumerable<Question> Questions;
        public List<Question> QuestionQueue;
        public Question Current;
        public int Count;
        public int CursorQ;

        private void InitializeUI()
        {
            var howtouse1 = "1. Basic mode: search with multiple batches and question ranges.\r\n" +
                            "For example, '1:45,78-81,100;2:[23]0,1;3:67'\r\n" +
                            "Of course, if you want to simply load Questions #1-#20 in Batch 1,\r\n" +
                            "you can use '1:1-20' in the search textbox.\r\n" +
                            "3 types of values are accepted for question ranges:\r\n" +
                            " - digit like 45\r\n" +
                            " - range like 78-81\r\n" +
                            " - patterns in Regular Expression like [23]0\r\n" +
                            "Values for question ranges should be separated by comma(,).\r\n";

            var howtouse2 = "2. Advanced mode: search with filter combination of batch, \r\nquestion number, question description and answer description\r\n" +
                            "via the support of patterns in Regular Expression.\r\n" +
                            "For example, 'Re>b:1;n:^4;q:osmotic'\r\n" +
                            "If you want to just load Questions with 'osmotic' in the question description,\r\n" +
                            "you can use 'Re>q:osmotic' in the search textbox.\r\n" +
                            "4 types of values are accepted for search filters:\r\n" +
                            " - b means Batch like 1\r\n" +
                            " - n means Question Number like ^4\r\n" +
                            " - q means Question Description like osmotic\r\n" +
                            " - a means Answer Description like osmotic\r\n" +
                            "Values for question ranges should be separated by semi-colon(;).\r\n";

            tbx_QuestionDesc.Text = howtouse1;
            tbx_AnswerDesc.Text = howtouse2;

            btn_Next.IsEnabled = false;
            btn_Previous.IsEnabled = false;
        }

        private void InitializeData()
        {
            var folder = @"Homework\";

            if (Questions == null)
            {
                var initialQuestions = folder.InitializeQuestions();
                Questions = folder.EnrichAnswersOn(initialQuestions).ToList();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            InitializeUI();
            
        }

        private void btn_Load_Click(object sender, RoutedEventArgs e)
        {
            InitializeData();
            var filter = tbx_QuestionRange.Text.Trim();
            var results = Questions.FilterBy(filter).ToList();
            if (chbx_Randomized.IsChecked == true)
                results = results.OrderBy(q => Guid.NewGuid()).ToList();

            if (results.Count > 0)
            {
                QuestionQueue = results;
                tb_Status.Text = QuestionQueue.Count.ToString() + " questions loaded.";

                Count = results.Count;                

                btn_Next.IsEnabled = true;             

                

            }
            else
            {
                
                QuestionQueue = null;
                tb_Status.Text = "0 questions loaded.";

                Count = 0;
            }


            CursorQ = 0;
            tbx_AnswerDesc.Text = "";
            tbx_QuestionDesc.Text = "";
            btn_Previous.IsEnabled = false;
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (Count > 0 && CursorQ < Count)
            {
                Current = QuestionQueue[CursorQ];

                CursorQ++;

                tbx_QuestionDesc.Text = Current.QuestionDesc;

                if (chbx_ViewAnswer.IsChecked == true) tbx_AnswerDesc.Text = Current.AnswerDesc;

                tb_Status.Text = CursorQ.ToString() + " of " + Count.ToString() + " Questions: Batch " + Current.BatchID + ", Number " + Current.ID;
            }
            else
            {
                btn_Next.IsEnabled = false;
            }

            if (CursorQ > 1)
            {
                btn_Previous.IsEnabled = true;
            }
        }

        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (CursorQ > 1)
            {           

                Current = QuestionQueue[CursorQ - 2];

                CursorQ--;


                tbx_QuestionDesc.Text = Current.QuestionDesc;

                if (chbx_ViewAnswer.IsChecked == true) tbx_AnswerDesc.Text = Current.AnswerDesc;

                tb_Status.Text = CursorQ.ToString() + " of " + Count.ToString() + " Questions: Batch " + Current.BatchID + ", Number " + Current.ID;

                btn_Next.IsEnabled = true;

                

            }


            if (CursorQ == 1)
            {
                btn_Previous.IsEnabled = false;
            }

        }

        private void chbx_ViewAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (chbx_ViewAnswer.IsChecked == true)
            {
                if (Current != null)
                    tbx_AnswerDesc.Text = Current.AnswerDesc;

            }
            else
            {
                tbx_AnswerDesc.Text = "";
            }
        }
    }
}
