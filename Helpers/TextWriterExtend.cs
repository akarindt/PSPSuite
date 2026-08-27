using System;
using System.IO;
using System.Text;

namespace PSPSuite.Helpers;

public class TextWriterExtend : TextWriter
{
    private readonly Action<string> _writeAction;
    public TextWriterExtend(Action<string> writeAction) => _writeAction = writeAction;
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        if (value != null) _writeAction(value);
    }

    public override void WriteLine(string? value)
    {
        if (value != null) _writeAction(value + Environment.NewLine);
    }
}