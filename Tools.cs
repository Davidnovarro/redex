using System.Text;

namespace RedEx;

public static class Tools
{
    public static long CountLines(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 1024);

        const char CR = '\r';
        const char LF = '\n';
        const char NULL = (char)0;

        long lineCount = 0L;

        byte[] byteBuffer = new byte[1024 * 1024];
        char detectedEOL = NULL;
        char currentChar = NULL;

        int bytesRead;
        while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                currentChar = (char)byteBuffer[i];

                if (detectedEOL != NULL)
                {
                    if (currentChar == detectedEOL)
                    {
                        lineCount++;
                    }
                }
                else if (currentChar == LF || currentChar == CR)
                {
                    detectedEOL = currentChar;
                    lineCount++;
                }
            }
        }

        // We had a NON-EOL character at the end without a new line
        if (currentChar != LF && currentChar != CR && currentChar != NULL)
        {
            lineCount++;
        }

        return lineCount;
    }
}