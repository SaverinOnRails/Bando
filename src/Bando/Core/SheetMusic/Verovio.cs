using System;
using System.IO;
using System.Runtime.InteropServices;
namespace Bando.Core.SheetMusic;
public static class LibVerovio
{
#if LINUX
    private const string DllName = "libverovio.so";
#elif MACOS 
    private const string DllName = "libverovio.dylib";
#else
    private const string DllName = "libverovio.dll";
#endif

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void enableLog(bool value);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void enableLogToBuffer(bool value);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_constructor();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_constructorResourcePath(
        [MarshalAs(UnmanagedType.LPStr)] string resourcePath);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_constructorNoResource();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void vrvToolkit_destructor(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_edit(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string editorAction);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_editInfo(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getAvailableOptions(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getDefaultOptions(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getDescriptiveFeatures(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string options);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getElementAttr(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getElementsAtTime(nint tkPtr, int millisec);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getExpansionIdsForElement(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getHumdrum(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_getHumdrumFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getID(nint tkPtr);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_convertHumdrumToHumdrum(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string humdrumData);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_convertHumdrumToMIDI(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string humdrumData);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_convertMEIToHumdrum(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string meiData);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getLog(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getMEI(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string options);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getMIDIValuesForElement(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getNotatedIdForElement(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getOptions(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getOptionUsageString(nint tkPtr);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int vrvToolkit_getPageCount(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int vrvToolkit_getPageWithElement(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getResourcePath(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int vrvToolkit_getScale(nint tkPtr);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern double vrvToolkit_getTimeForElement(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_getTimesForElement(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string xmlId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_getVersion(nint tkPtr);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_loadData(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string data);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_loadFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_loadZipDataBase64(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string data);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_loadZipDataBuffer(
        nint tkPtr,
        byte[] data,
        int length);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void vrvToolkit_redoLayout(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string c_options);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void vrvToolkit_redoPagePitchPosLayout(nint tkPtr);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_renderData(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string data,
        [MarshalAs(UnmanagedType.LPStr)] string options);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_renderToExpansionMap(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_renderToExpansionMapFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_renderToMIDI(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_renderToMIDIFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint vrvToolkit_renderToPAE(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_renderToPAEFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_renderToSVG(
        nint tkPtr,
        int page_no,
        [MarshalAs(UnmanagedType.I1)] bool xmlDeclaration);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_renderToSVGFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename,
        int pageNo);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_renderToTimemap(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string c_options);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_renderToTimemapFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename,
        [MarshalAs(UnmanagedType.LPStr)] string c_options);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void vrvToolkit_resetOptions(nint tkPtr);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void vrvToolkit_resetXmlIdSeed(nint tkPtr, int seed);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_saveFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename,
        [MarshalAs(UnmanagedType.LPStr)] string c_options);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_select(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string selection);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_setInputFrom(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string inputFrom);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_setOptions(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string options);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_setOutputTo(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string outputTo);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_setResourcePath(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool vrvToolkit_setScale(nint tkPtr, int scale);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_validatePAE(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string data);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern nint vrvToolkit_validatePAEFile(
        nint tkPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename);
}

public class Verovio : IDisposable
{
    private nint _tkPtr = nint.Zero;
    private bool _disposed = false;

    public Verovio()
    {
        _tkPtr = LibVerovio.vrvToolkit_constructor();
        if (_tkPtr == nint.Zero)
        {
            throw new Exception("Failed to create Verovio toolkit instance");
        }
        LibVerovio.vrvToolkit_setResourcePath(_tkPtr, Path.Combine(AppContext.BaseDirectory, "VerovioData"));
        SetOption("scaleToPageSize", "true");
        SetOption("svgViewBox","true");
        SetOption("adjustPageHeight","true");
    }

    // For setting multiple options at once (more efficient)
    public void SetOption(string key, string value)
    {
        // Verovio expects a JSON object with options
        var jsonOptions = $"{{\"{key}\": \"{value}\"}}";
        bool success = LibVerovio.vrvToolkit_setOptions(_tkPtr, jsonOptions);

        if (!success)
        {
            Console.WriteLine($"Warning: Failed to set option {key}={value}");
        }
    }
    // For numeric values (no quotes around the value)
    public void SetOptionNumeric(string key, int value)
    {
        var jsonOptions = $"{{\"{key}\": {value}}}";
        bool success = LibVerovio.vrvToolkit_setOptions(_tkPtr, jsonOptions);

        if (!success)
        {
            Console.WriteLine($"Warning: Failed to set option {key}={value}");
        }
    }
    public bool LoadFile(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            throw new ArgumentNullException(nameof(filename));
        }
        return LibVerovio.vrvToolkit_loadFile(_tkPtr, filename);
    }

    public byte[] RenderToSvg(int pageNumber = 1, bool includeXmlDeclaration = false)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be >= 1");
        }

        nint svgPtr = LibVerovio.vrvToolkit_renderToSVG(_tkPtr, pageNumber, includeXmlDeclaration);

        if (svgPtr == nint.Zero)
        {
            return Array.Empty<byte>();
        }

        string? svgString = Marshal.PtrToStringAnsi(svgPtr);
        if (string.IsNullOrEmpty(svgString))
        {
            return Array.Empty<byte>();
        }
        return System.Text.Encoding.UTF8.GetBytes(svgString);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_tkPtr != nint.Zero)
            {
                LibVerovio.vrvToolkit_destructor(_tkPtr);
                _tkPtr = nint.Zero;
            }
            _disposed = true;
        }
    }

    internal object GetPageCount()
    {
        if (_tkPtr == nint.Zero) throw new Exception();
        return LibVerovio.vrvToolkit_getPageCount(_tkPtr);
    }
}
