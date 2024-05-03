using System.Text.RegularExpressions;
using vss.Utils;
using vss;

public class MainController
{
    private Form1 view;
    private List<WindowModel> windows;
    private List<WindowModel> filteredWindows;
    private string recent = "";
    private bool onlyShowVSCode = true;
    private WindowManager windowManager;

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
            if (magicSearches.TryGetValue(search, out string magicSearch))
            {
                search = magicSearch;
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
        }

        view.SelectFirstWindowInList();
    }

    public void SwitchWindow(int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < filteredWindows.Count)
        {
            var window = filteredWindows[selectedIndex];
            windowManager.SetTopWindow(window.Handle);
            recent = window.Title;
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