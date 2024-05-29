using System.Text.RegularExpressions;
using vss.Utils;
using vss;
using System.Diagnostics;
using System.Reflection;

namespace vss
{
    public class MainController
    {
        private Form1 view;
        private List<WindowModel> windows;
        private List<WindowModel> filteredWindows;
        private string recent = "";
        private bool onlyShowVSCode = true;
        private WindowManager windowManager;
        private string specialItem = "Open new VS Code window";

        public MainController(Form1 view)
        {
            this.view = view;
            windowManager = new WindowManager();
            UpdateWindowList();
        }

        public void UpdateWindowList()
        {
            var windowHandles = windowManager.GetWindows();
            windows = windowHandles.Select(hwnd => new WindowModel
            {
                Handle = hwnd,
                Title = windowManager.GetWindowText(hwnd)
            }).ToList();
            UpdateList();
        }
        public static string EncodeParameterArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
                return original;
            string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");
            return value;
        }
        public void UpdateList()
        {
            string search = view.SearchText;
            var magicSearches = ScoringLogic.LoadMagicSearches();
            filteredWindows = windows.Where(window => window.Title.Contains("Visual Studio Code")).ToList();
            var notVSCode = windows.Except(filteredWindows).ToList();

            if (onlyShowVSCode)
            {
                notVSCode.Clear();
            }

            if (string.IsNullOrEmpty(search))
            {
                var winTitles = (filteredWindows.Concat(notVSCode)).Select(window => window.Title).Where(title => !string.IsNullOrEmpty(title)).ToList();
                view.ClearWindowList();
                if (winTitles.Contains(recent))
                {
                    view.AddWindowToList(recent);
                }
                foreach (string winTitle in winTitles)
                {
                    if (winTitle != recent)
                    {
                        view.AddWindowToList(winTitle);
                    }
                }
            }
            else
            {
                if (magicSearches.TryGetValue(search, out MagicSearch magicSearch))
                {
                    search = magicSearch.ExpandedName;
                    filteredWindows = filteredWindows.Where(window => window.Title.Contains(search)).ToList();
                }

                string[] keywords = Regex.Split(search.Replace('[', ' ').Replace(']', ' '), @"\s+");
                foreach (var window in filteredWindows)
                {
                    int rcuScore = window.Title == recent ? 10 : 0;
                    string searchTitle = ScoringLogic.RemoveVSCodePostfix(window.Title);
                    int baseScore = keywords.Sum(keyword => Regex.IsMatch(searchTitle, keyword, RegexOptions.IgnoreCase) ? 1 : 0) * 100;
                    int acronymScore = ScoringLogic.GetCharacterMatchScore(search, searchTitle) * 10;
                    window.Score = baseScore + acronymScore + rcuScore;
                }

                filteredWindows = filteredWindows.OrderByDescending(window => window.Score).ToList();
                view.ClearWindowList();
                foreach (var window in filteredWindows)
                {
                    if (window.Score > 0)
                    {
                        view.AddWindowToList(window.Title);
                    }
                }

                if (!filteredWindows.Any() && magicSearches.TryGetValue(view.SearchText, out magicSearch))
                {
                    view.AddWindowToList(specialItem);
                }
            }

            view.SelectFirstWindowInList();
        }

        public void SwitchWindow(int selectedIndex, string window_name)
        {
            if (selectedIndex >= 0 && selectedIndex < filteredWindows.Count)
            {
                var window = filteredWindows[selectedIndex];
                windowManager.SetTopWindow(window.Handle);
                recent = window.Title;
            }
            else if (!filteredWindows.Any() && window_name == specialItem)
            {
                var magicSearches = ScoringLogic.LoadMagicSearches();
                if (magicSearches.TryGetValue(view.SearchText, out MagicSearch magicSearch))
                {
                    string arg = $"-w Hidden -nop -noni -nol -c  {EncodeParameterArgument(magicSearch.Command)}";
                    //arg = @"-c ""& 'C:\\Program Files\\Microsoft VS Code\\Code.exe'""";
                    Process.Start("pwsh", arg);
                }
            }
            view.ClearSearchText();
            view.HideForm();
        }

        public void ActivateWindow()
        {
            UpdateWindowList();
            view.ClearSearchText();
            view.ShowForm();
        }
    }
}