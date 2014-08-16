using Spira.Core;

namespace Spira.UI
{
    public static class UiWindowFactory
    {
        public static UiWindow Create(string title)
        {
            Exceptions.CheckArgumentNullOrEmprty(title, "title");

            UiWindow window = new UiWindow {Title = title};

            return window;
        }
    }
}