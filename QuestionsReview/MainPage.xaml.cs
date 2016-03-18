using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Storage;
using Windows.UI.Popups;

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
        public ReviewLog CurrentReviewLog;
        public List<Question> UncertainItems;

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

            if (CurrentReviewLog == null) CurrentReviewLog = new ReviewLog();

            if (UncertainItems == null) UncertainItems = new List<Question>();

            btn_Next.IsEnabled = false;
            btn_Previous.IsEnabled = false;
            btn_Submit.IsEnabled = false;
            rtbn_A.IsEnabled = rtbn_B.IsEnabled = rtbn_C.IsEnabled = rtbn_D.IsEnabled = false;
            chbx_Uncertain.IsEnabled = false;


        }

        private void btn_Load_Click(object sender, RoutedEventArgs e)
        {
                    

            var filter = tbx_QuestionRange.Text.Trim();
            var results = Questions.FilterBy(filter).ToList();
            if (chbx_Randomized.IsChecked == true)
                results = results.OrderBy(q => Guid.NewGuid()).ToList();

            if (CurrentReviewLog != null) CurrentReviewLog = null;
            if (UncertainItems != null) UncertainItems = null;
            

            //tbx_QuestionDesc.Text = QuestionQueue.Count.ToString() + " questions loaded.";
            

            if (results.Count > 0)
            {
                btn_Next.IsEnabled = true;
                //btn_Previous.IsEnabled = false;

                Count = results.Count;
                Cursor = 0;
                Current = null;

                QuestionQueue = results;

                CurrentReviewLog = new ReviewLog
                {
                    StartTime = DateTime.Now,
                    ReviewPattern = tbx_QuestionRange.Text,
                    ReviewItems = (from q in QuestionQueue
                                   select new ReviewItem
                                   {
                                       QuestionID = q.ID,
                                       BatchID = q.BatchID,
                                       Answer = "X"
                                   }).ToList()
                };

                UncertainItems = new List<Question>();

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
            btn_Submit.IsEnabled = false;
            rtbn_A.IsEnabled = rtbn_B.IsEnabled = rtbn_C.IsEnabled = rtbn_D.IsEnabled = false;
            rtbn_A.IsChecked = rtbn_B.IsChecked = rtbn_C.IsChecked = rtbn_D.IsChecked = false;
            chbx_Uncertain.IsChecked = false;
            chbx_Uncertain.IsEnabled = false;

        }

       

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            

            if(Count > 0 && Cursor < Count)
            {
                if(Current == null)
                {
                    chbx_Uncertain.IsEnabled = true;
                }
                else //current is the previous one
                {
                    var batchID = Current.BatchID;
                    var questionID = Current.ID;

                    for(int i = 0; i < CurrentReviewLog.ReviewItems.Count; i++)
                    {
                        if(CurrentReviewLog.ReviewItems[i].BatchID == batchID && CurrentReviewLog.ReviewItems[i].QuestionID == questionID)
                        {
                            if (rtbn_A.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "A";
                            }
                            else if (rtbn_B.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "B";
                            }
                            else if (rtbn_C.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "C";
                            }
                            else if (rtbn_D.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "D";
                            }
                            else
                            {
                               
                                CurrentReviewLog.ReviewItems[i].Answer = "X";
                               
                            }
                        }
                    }

                    if(chbx_Uncertain.IsChecked == true)
                    {
                        UncertainItems.Add(Current);
                        UncertainItems = UncertainItems.Distinct().ToList();

                    }
                    else
                    {
                        if(UncertainItems.Contains(Current))
                        {
                            UncertainItems.Remove(Current);
                        }
                    }
                    

                    

                }

                Current = QuestionQueue[Cursor];

                Cursor++;

                tbx_QuestionDesc.Text = Current.QuestionDesc;

                if (chbx_ViewAnswer.IsChecked == true) tbx_AnswerDesc.Text = Current.AnswerDesc;

                rtbn_A.IsEnabled = rtbn_B.IsEnabled = rtbn_C.IsEnabled = rtbn_D.IsEnabled = true;

                var tempAnswer = (from i in CurrentReviewLog.ReviewItems
                                  where i.BatchID == Current.BatchID && i.QuestionID == Current.ID
                                  select i.Answer).FirstOrDefault();

                if (tempAnswer == "A")
                {
                    rtbn_A.IsChecked = true;
                }
                else if (tempAnswer == "B")
                {
                    rtbn_B.IsChecked = true;

                }
                else if (tempAnswer == "C")
                {
                    rtbn_C.IsChecked = true;

                }
                else if (tempAnswer == "D")
                {
                    rtbn_D.IsChecked = true;

                }
                else
                {
                    rtbn_A.IsChecked = rtbn_B.IsChecked = rtbn_C.IsChecked = rtbn_D.IsChecked = false;
                }

                if(UncertainItems.Contains(Current))
                {
                    chbx_Uncertain.IsChecked = true;
                }
                else
                {
                    chbx_Uncertain.IsChecked = false;
                }






                tb_Status.Text = $"{Cursor} of {Count} Questions: {Current.BatchID}-{Current.ID}";
            }
            else
            {
                if (Cursor == Count)
                {
                    var batchID = Current.BatchID;
                    var questionID = Current.ID;

                    for (int i = 0; i < CurrentReviewLog.ReviewItems.Count; i++)
                    {
                        if (CurrentReviewLog.ReviewItems[i].BatchID == batchID && CurrentReviewLog.ReviewItems[i].QuestionID == questionID)
                        {
                            if (rtbn_A.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "A";
                            }
                            else if (rtbn_B.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "B";
                            }
                            else if (rtbn_C.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "C";
                            }
                            else if (rtbn_D.IsChecked == true)
                            {
                                CurrentReviewLog.ReviewItems[i].Answer = "D";
                            }
                            else
                            {

                                CurrentReviewLog.ReviewItems[i].Answer = "X";

                            }
                        }
                    }

                    if (chbx_Uncertain.IsChecked == true)
                    {
                        UncertainItems.Add(Current);
                        UncertainItems = UncertainItems.Distinct().ToList();
                    }
                    else
                    {
                        if (UncertainItems.Contains(Current))
                        {
                            UncertainItems.Remove(Current);
                        }
                    }


                    btn_Submit.IsEnabled = true;
                }
                

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

                var tempAnswer = (from i in CurrentReviewLog.ReviewItems
                                  where i.BatchID == Current.BatchID && i.QuestionID == Current.ID
                                  select i.Answer).FirstOrDefault();

                if (tempAnswer == "A")
                {
                    rtbn_A.IsChecked = true;
                }
                else if (tempAnswer == "B")
                {
                    rtbn_B.IsChecked = true;

                }
                else if (tempAnswer == "C")
                {
                    rtbn_C.IsChecked = true;

                }
                else if (tempAnswer == "D")
                {
                    rtbn_D.IsChecked = true;

                }
                else
                {
                    rtbn_A.IsChecked = rtbn_B.IsChecked = rtbn_C.IsChecked = rtbn_D.IsChecked = false;
                }

                if (UncertainItems.Contains(Current))
                {
                    chbx_Uncertain.IsChecked = true;
                }
                else
                {
                    chbx_Uncertain.IsChecked = false;
                }



                tb_Status.Text = $"{Cursor} of {Count} Questions: {Current.BatchID}-{Current.ID}";

                btn_Next.IsEnabled = true;

                

            }


            if (Cursor == 1)
            {
                if(Current != null)
                {
                    var tempAnswer = (from i in CurrentReviewLog.ReviewItems
                                      where i.BatchID == Current.BatchID && i.QuestionID == Current.ID
                                      select i.Answer).FirstOrDefault();

                    if (tempAnswer == "A")
                    {
                        rtbn_A.IsChecked = true;
                    }
                    else if (tempAnswer == "B")
                    {
                        rtbn_B.IsChecked = true;

                    }
                    else if (tempAnswer == "C")
                    {
                        rtbn_C.IsChecked = true;

                    }
                    else if (tempAnswer == "D")
                    {
                        rtbn_D.IsChecked = true;

                    }
                    else
                    {
                        rtbn_A.IsChecked = rtbn_B.IsChecked = rtbn_C.IsChecked = rtbn_D.IsChecked = false;
                    }

                    if (UncertainItems.Contains(Current))
                    {
                        chbx_Uncertain.IsChecked = true;
                    }
                    else
                    {
                        chbx_Uncertain.IsChecked = false;
                    }
                }

                btn_Previous.IsEnabled = false;
            }


        }

        private async void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentReviewLog == null || CurrentReviewLog.ReviewItems.Count == 0)
                return;

            btn_Next.IsEnabled = btn_Previous.IsEnabled = false;

            CurrentReviewLog.FinishTime = DateTime.Now;

            var sb_IncorrectItemsPattern = new StringBuilder();
            var sb_UnfinishedItemsPattern = new StringBuilder();
            var sb_ReviewSummary = new StringBuilder();
            var sb_ReviewDetail = new StringBuilder();
            var sb_UncertainItemsPattern = new StringBuilder();

            var sb_Patterns = new StringBuilder();

            //uncertain items
            if (UncertainItems.Count > 0)
            {
                sb_ReviewSummary.AppendLine($"{UncertainItems.Count} of {CurrentReviewLog.ReviewItems.Count} questions are uncertain.");
                foreach (var uit in UncertainItems)
                {
                    sb_UncertainItemsPattern.Append($"{uit.BatchID}:{uit.ID};");

                    sb_ReviewDetail.AppendLine($"{uit.BatchID}-{uit.ID}: Uncertain;");
                }
            }

            //unfinished items

            var unfinished = (from i in CurrentReviewLog.ReviewItems where i.Answer == "X" select i).ToList();

            if (unfinished.Count > 0)
            {
                var diag = new MessageDialog($"You still have {unfinished.Count} questions without actual answers!");
                await diag.ShowAsync();

                sb_ReviewSummary.AppendLine($"{unfinished.Count} of {CurrentReviewLog.ReviewItems.Count} questions are unfinished.");
                foreach (var ui in unfinished)
                {
                    sb_UnfinishedItemsPattern.Append($"{ui.BatchID}:{ui.QuestionID};");

                    sb_ReviewDetail.AppendLine($"{ui.BatchID}-{ui.QuestionID}: Unfinished;");
                }

            }

            

            //incorrectItems
            var incorrectItems = from i in CurrentReviewLog.ReviewItems
                                 let batchID = i.BatchID
                                 let questionID = i.QuestionID
                                 let answer = i.Answer
                                 let standard = (from q in QuestionQueue where q.BatchID == batchID && q.ID == questionID select q).FirstOrDefault()
                                 where answer != standard.AnswerRef && answer != "X"
                                 select new
                                 {
                                     BatchID = batchID,
                                     QuestionID = questionID,
                                     Answer = answer,
                                     AnswerRef = standard.AnswerRef
                                 };

            

            var c = incorrectItems.Count();
            if (c == 0 && unfinished.Count == 0)
            {
                sb_ReviewSummary.AppendLine($"All your actual answers of {CurrentReviewLog.ReviewItems.Count} questions in this review are expected.");
            }
            else
            {
                sb_ReviewSummary.AppendLine($"{c} of {CurrentReviewLog.ReviewItems.Count} questions are with unexpected answers.");

                foreach (var ii in incorrectItems)
                {
                    sb_IncorrectItemsPattern.Append($"{ii.BatchID}:{ii.QuestionID};");

                    sb_ReviewDetail.AppendLine($"{ii.BatchID}-{ii.QuestionID}: Actual Answer {ii.Answer}, Expected Answer {ii.AnswerRef};");
                }

            }




            CurrentReviewLog.ReviewPattern = tbx_QuestionRange.Text;
            CurrentReviewLog.IncorrectItemsPattern = sb_IncorrectItemsPattern.ToString();
            CurrentReviewLog.ReviewSummary = sb_ReviewSummary.ToString();
            CurrentReviewLog.ReviewDetail = sb_ReviewDetail.ToString();
            CurrentReviewLog.UnfinishedItemsPattern = sb_UnfinishedItemsPattern.ToString();
            CurrentReviewLog.UncertainItemsPattern = sb_UncertainItemsPattern.ToString();

            sb_Patterns.AppendLine($"Additional Patterns: ");
            if(!string.IsNullOrEmpty(CurrentReviewLog.UncertainItemsPattern))
                sb_Patterns.AppendLine($"Uncertain Items: {CurrentReviewLog.UncertainItemsPattern}");
            if (!string.IsNullOrEmpty(CurrentReviewLog.UnfinishedItemsPattern))
                sb_Patterns.AppendLine($"Unfinished Items: {CurrentReviewLog.UnfinishedItemsPattern}");
            if (!string.IsNullOrEmpty(CurrentReviewLog.IncorrectItemsPattern))
                sb_Patterns.AppendLine($"Incorrect Items: {CurrentReviewLog.IncorrectItemsPattern}");

            if(string.IsNullOrEmpty(CurrentReviewLog.UncertainItemsPattern)
                && string.IsNullOrEmpty(CurrentReviewLog.UnfinishedItemsPattern)
                && string.IsNullOrEmpty(CurrentReviewLog.IncorrectItemsPattern))
            {
                sb_Patterns.Clear();
            }

            var basic = CurrentReviewLog.ToString();
            var additional = sb_Patterns.ToString();

            //output to UI
            tbx_QuestionDesc.Text = basic ;
            tbx_AnswerDesc.Text = additional;


            //output to file
            
            var file = $"ReviewLog_{CurrentReviewLog.StartTime.ToString("yyyyMMddhhmmss")}-{CurrentReviewLog.FinishTime.ToString("yyyyMMddhhmmss")}.txt";
            

            var localFolder = ApplicationData.Current.LocalFolder;
            //var dialog2 = new MessageDialog(localFolder.Path);
            //await dialog2.ShowAsync();
            var r = localFolder.CreateFolderAsync("ReviewLogs", CreationCollisionOption.OpenIfExists);
            //var dialog3 = new MessageDialog((r.ErrorCode == null).ToString());
            //await dialog3.ShowAsync();

            var sf = await localFolder.CreateFileAsync(@"ReviewLogs\" +file, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sf, basic + additional);

            


            CurrentReviewLog = null;
            rtbn_A.IsChecked = rtbn_B.IsChecked = rtbn_C.IsChecked = rtbn_D.IsChecked = false;
            rtbn_A.IsEnabled = rtbn_B.IsEnabled = rtbn_C.IsEnabled = rtbn_D.IsEnabled = false;
            chbx_Uncertain.IsEnabled = false;
            chbx_Uncertain.IsChecked = false;
            tb_Status.Text = "Please load your questions.";
            btn_Submit.IsEnabled = false;


        }
    }
}
