using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using SmartCadFeedback.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCadFeedback
{
    public class GithubIssue
    {
        public string Title { get; }
        public string Body { get; }
        public ID ID { get; }
        public int Number { get; }

        public bool IsClosed { get; }

        public List<CustomLabel> Labels { get; }

        public GithubIssue(string title, string body, ID iD, int number, List<CustomLabel> lb, bool is_closed)
        {
            Title = title;
            Body = body;
            ID = iD;
            Number = number;
            Labels = lb;
            IsClosed = is_closed;
        }
    }

    public class CustomLabel
    {
        public string Name { get; }

        public CustomLabel(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private Connection GithubApi;
        public const string LabelCrashLog = "crash_log";

        IList<GithubIssue> CurrentListSource;

        public MainPage()
        {
            this.InitializeComponent();
            GithubApi = new Connection(new ProductHeaderValue("MuziburRahman"), "beaf49c8cc9e64ff42e7bb07ac6b7c6ba865ba35");
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RefreshButtonClick(null, null);
        }

        private async void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WaitUI.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var progress_incr = 90.0 / IssueList.SelectedItems.Count;
            DeleteProgressBar.Value = 0;
            DeleteProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            foreach (GithubIssue item in IssueList.SelectedItems)
            {
                var mt = new Mutation()
                                .DeleteIssue(new DeleteIssueInput { IssueId = item.ID, ClientMutationId = "MuziburRahman" })
                                .Select(i => i.ClientMutationId);

                await GithubApi.Run(mt);
                DeleteProgressBar.Value += progress_incr;
            }

            await Task.Delay(1);
            RefreshButtonClick(null, null);

            DeleteProgressBar.Visibility =
            WaitUI.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void RefreshButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WaitUI.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var query = new Query()
            .Repository("SmartCadFeedback", "MuziburRahman")
            .Select(r => new
            {
                Issues = r.Issues(100, null, null, null, null, null, null, null)
                          .Nodes
                          .Select(i => new GithubIssue
                          (
                              i.Title,
                              i.Body,
                              i.Id,
                              i.Number,
                              i.Labels(4, null, null, null).Nodes.Select(l => new CustomLabel(l.Name)).ToList(),
                              i.Closed
                          ))
                          .ToList(),
            });

            var result = await GithubApi.Run(query);
            CurrentListSource = result.Issues.Where(i => !i.IsClosed).Reverse().ToArray();
            IssueList.ItemsSource = CurrentListSource;

            WaitUI.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void SearchTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;


            if (CurrentListSource is null || CurrentListSource.Count == 0)
                return;

            string str = SearchTextBox.Text.ToLower();

            var global_result = CurrentListSource.Where(sc => sc.Title.ToLower().Contains(str)).ToArray();

            if (global_result.Length == 0)
            {
                global_result = CurrentListSource
                                .Select(sc => (sc, str.SequenceMatch(sc.Title.ToLower())))
                                .Where(res => res.Item2.MatchPercentage >= -0.25f || res.Item2.MatchCount > 2)
                                .OrderByDescending(res => res.Item2.MatchCount)
                                .Select(iss => iss.sc)
                                .ToArray();

                if (global_result.Length > 3)
                {
                    IssueList.ItemsSource = global_result.Take(3);
                }
                else
                {
                    IssueList.ItemsSource = global_result;
                }
            }
            else
            {
                IssueList.ItemsSource = global_result;
            }
        }
    }
}
