using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class GameUi
{
    private readonly View _view;
    private const string SEPARATOR = "----------------------------------------";

    public GameUi(View view)
    {
        _view = view;
    }

    public void PrintLine()
    {
        _view.WriteLine(SEPARATOR);
    }

    public string ReadLine()
    {
        return _view.ReadLine();
    }

    public void WriteLine(string text)
    {
        _view.WriteLine(text);
    }

    public void ShowFiles(string[] files)
    {
        for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            string fileName = ExtractFileName(files[fileIndex]);
            _view.WriteLine($"{fileIndex}: {fileName}");
        }
    }

    private string ExtractFileName(string filePath)
    {
        return Path.GetFileName(filePath);
    }
}