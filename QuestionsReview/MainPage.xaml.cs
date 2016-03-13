﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuestionsReview
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public IEnumerable<Question> Questions;
        public List<Question> QuestionQueue;
        public Question Current;
        public int Count;
        public int Cursor;

        public MainPage()
        {
            this.InitializeComponent();

            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Black;
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Colors.DarkGreen;
            //titleBar.ButtonBackgroundColor = Colors.DarkCyan;
            titleBar.ButtonForegroundColor = Colors.White;

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

            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            //Window.Current.SetTitleBar(@"ACP Questions Reviewer Lite");

            var folder = @"Homework\";

            if(Questions == null)
            {
                var initialQuestions = folder.InitializeQuestions();
                Questions = folder.EnrichAnswersOn(initialQuestions).ToList();
            }

            btn_Next.IsEnabled = false;
            btn_Previous.IsEnabled = false;

            
        }

        private void btn_Load_Click(object sender, RoutedEventArgs e)
        {
                    

            var filter = tbx_QuestionRange.Text.Trim();
            var results = Questions.FilterBy(filter).ToList();
            if (chbx_Randomized.IsChecked == true)
                results = results.OrderBy(q => Guid.NewGuid()).ToList();
            

            //tbx_QuestionDesc.Text = QuestionQueue.Count.ToString() + " questions loaded.";
            

            if (results.Count > 0)
            {
                btn_Next.IsEnabled = true;
                //btn_Previous.IsEnabled = false;

                Count = results.Count;
                Cursor = 0;

                QuestionQueue = results;

                tb_Status.Text = QuestionQueue.Count.ToString() + " questions loaded.";

            }
            else
            {
                Count = 0;
                QuestionQueue = null;

                tb_Status.Text = "0 questions loaded.";
            }

            tbx_AnswerDesc.Text = "";
            tbx_QuestionDesc.Text = "";
            btn_Previous.IsEnabled = false;
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if(Count > 0 && Cursor < Count)
            {
                Current = QuestionQueue[Cursor];

                Cursor++;

                tbx_QuestionDesc.Text = Current.QuestionDesc;

                if (chbx_ViewAnswer.IsChecked == true) tbx_AnswerDesc.Text = Current.AnswerDesc;

                tb_Status.Text = $"{Cursor} of {Count} Questions: Batch {Current.BatchID}, Number {Current.ID}";
            }
            else
            {
                btn_Next.IsEnabled = false;
            }

            if(Cursor > 1)
            {
                btn_Previous.IsEnabled = true;
            }
        }

        

        private void chbx_ViewAnswer_Tapped(object sender, TappedRoutedEventArgs e)
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

        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            

            if (Cursor > 1)
            {           

                Current = QuestionQueue[Cursor - 2];

                Cursor--;


                tbx_QuestionDesc.Text = Current.QuestionDesc;

                if (chbx_ViewAnswer.IsChecked == true) tbx_AnswerDesc.Text = Current.AnswerDesc;

                tb_Status.Text = $"{Cursor} of {Count} Questions: Batch {Current.BatchID}, Number {Current.ID}";

                btn_Next.IsEnabled = true;

                

            }


            if (Cursor == 1)
            {
                btn_Previous.IsEnabled = false;
            }


        }
    }
}
