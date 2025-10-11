namespace Bando.Core.SheetMusic.Rendering;
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class LibResvg
{
    private const string LibraryName = "resvg";
    public enum ResvgError : int
    {
        RESVG_OK = 0,
        RESVG_ERROR_NOT_AN_UTF8_STR,
        RESVG_ERROR_FILE_OPEN_FAILED,
        RESVG_ERROR_MALFORMED_GZIP,
        RESVG_ERROR_ELEMENTS_LIMIT_REACHED,
        RESVG_ERROR_INVALID_SIZE,
        RESVG_ERROR_PARSING_FAILED,
    }

    public enum ResvgImageRendering
    {
        RESVG_IMAGE_RENDERING_OPTIMIZE_QUALITY,
        RESVG_IMAGE_RENDERING_OPTIMIZE_SPEED,
    }

    public enum ResvgShapeRendering
    {
        RESVG_SHAPE_RENDERING_OPTIMIZE_SPEED,
        RESVG_SHAPE_RENDERING_CRISP_EDGES,
        RESVG_SHAPE_RENDERING_GEOMETRIC_PRECISION,
    }

    public enum ResvgTextRendering
    {
        RESVG_TEXT_RENDERING_OPTIMIZE_SPEED,
        RESVG_TEXT_RENDERING_OPTIMIZE_LEGIBILITY,
        RESVG_TEXT_RENDERING_GEOMETRIC_PRECISION,
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct ResvgTransform
    {
        public float a;
        public float b;
        public float c;
        public float d;
        public float e;
        public float f;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ResvgSize
    {
        public float width;
        public float height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ResvgRect
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }


    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ResvgTransform resvg_transform_identity();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_init_log();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint resvg_options_create();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_resources_dir(nint opt, string path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_set_dpi(nint opt, float dpi);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_stylesheet(nint opt, string content);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_font_family(nint opt, string family);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_set_font_size(nint opt, float size);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_serif_family(nint opt, string family);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_sans_serif_family(nint opt, string family);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_cursive_family(nint opt, string family);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_fantasy_family(nint opt, string family);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_monospace_family(nint opt, string family);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void resvg_options_set_languages(nint opt, string languages);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_set_shape_rendering_mode(nint opt, ResvgShapeRendering mode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_set_text_rendering_mode(nint opt, ResvgTextRendering mode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_set_image_rendering_mode(nint opt, ResvgImageRendering mode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_load_font_data(nint opt, byte[] data, uint len);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int resvg_options_load_font_file(nint opt, string file_path);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_load_system_fonts(nint opt);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_options_destroy(nint opt);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int resvg_parse_tree_from_file(string file_path, nint opt, out nint tree);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int resvg_parse_tree_from_data(byte[] data, uint len, nint opt, out nint tree);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_is_image_empty(nint tree);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ResvgSize resvg_get_image_size(nint tree);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_get_object_bbox(nint tree, out ResvgRect bbox);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_get_image_bbox(nint tree, out ResvgRect bbox);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_node_exists(nint tree, string id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_get_node_transform(nint tree, string id, out ResvgTransform transform);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_get_node_bbox(nint tree, string id, out ResvgRect bbox);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_get_node_stroke_bbox(nint tree, string id, out ResvgRect bbox);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_tree_destroy(nint tree);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void resvg_render(nint tree, ResvgTransform transform, uint width, uint height, byte[] pixmap);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool resvg_render_node(nint tree, string id, ResvgTransform transform, uint width, uint height, byte[] pixmap);
}

public class ResvgOptions : IDisposable
{
    private nint _handle;
    private bool _disposed;

    public ResvgOptions()
    {
        _handle = LibResvg.resvg_options_create();
        if (_handle == nint.Zero)
            throw new Exception("Failed to create resvg options");
        InitLog();
    }

    public nint Handle => _handle;

    public void SetResourcesDir(string path)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_resources_dir(_handle, path);
    }

    public void SetDpi(float dpi)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_dpi(_handle, dpi);
    }

    public void InitLog()
    {
        LibResvg.resvg_init_log();
    }
    public void SetStylesheet(string content)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_stylesheet(_handle, content);
    }

    public void SetFontFamily(string family)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_font_family(_handle, family);
    }

    public void SetFontSize(float size)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_font_size(_handle, size);
    }

    public void SetSerifFamily(string family)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_serif_family(_handle, family);
    }

    public void SetSansSerifFamily(string family)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_sans_serif_family(_handle, family);
    }

    public void SetCursiveFamily(string family)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_cursive_family(_handle, family);
    }

    public void SetFantasyFamily(string family)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_fantasy_family(_handle, family);
    }

    public void SetMonospaceFamily(string family)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_monospace_family(_handle, family);
    }

    public void SetLanguages(string languages)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_languages(_handle, languages);
    }

    public void SetShapeRenderingMode(LibResvg.ResvgShapeRendering mode)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_shape_rendering_mode(_handle, mode);
    }

    public void SetTextRenderingMode(LibResvg.ResvgTextRendering mode)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_text_rendering_mode(_handle, mode);
    }

    public void SetImageRenderingMode(LibResvg.ResvgImageRendering mode)
    {
        CheckDisposed();
        LibResvg.resvg_options_set_image_rendering_mode(_handle, mode);
    }

    public void LoadFontData(byte[] data)
    {
        CheckDisposed();
        LibResvg.resvg_options_load_font_data(_handle, data, (uint)data.Length);
    }

    public void LoadFontFile(string filePath)
    {
        CheckDisposed();
        int result = LibResvg.resvg_options_load_font_file(_handle, filePath);
        if (result != (int)LibResvg.ResvgError.RESVG_OK)
            throw new Exception($"Failed to load font file: {(LibResvg.ResvgError)result}");
    }

    public void LoadSystemFonts()
    {
        CheckDisposed();
        LibResvg.resvg_options_load_system_fonts(_handle);
    }

    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResvgOptions));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != nint.Zero)
            {
                LibResvg.resvg_options_destroy(_handle);
                _handle = nint.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ResvgOptions()
    {
        Dispose();
    }
}

public class ResvgRenderTree : IDisposable
{
    private nint _handle = nint.Zero;
    private ResvgOptions _opts;
    private bool _disposed;

    public ResvgRenderTree(ResvgOptions opts)
    {
        _opts = opts;
    }

    public nint Handle => _handle;

    public void ParseFromFile(string filePath)
    {
        int result = LibResvg.resvg_parse_tree_from_file(filePath, _opts.Handle, out nint tree);
        if (result != (int)LibResvg.ResvgError.RESVG_OK)
            throw new Exception($"Failed to parse SVG from file: {(LibResvg.ResvgError)result}");

        _handle = tree;
    }

    public void ParseFromData(byte[] data)
    {
        int result = LibResvg.resvg_parse_tree_from_data(data, (uint)data.Length, _opts.Handle, out nint tree);
        if (result != (int)LibResvg.ResvgError.RESVG_OK)
            throw new Exception($"Failed to parse SVG from data: {(LibResvg.ResvgError)result}");
        _handle = tree;
    }

    public void ParseFromString(string svgContent)
    {
        byte[] data = Encoding.UTF8.GetBytes(svgContent);
        ParseFromData(data);
    }

    public bool IsEmpty()
    {
        CheckDisposed();
        return LibResvg.resvg_is_image_empty(_handle);
    }

    public LibResvg.ResvgSize GetImageSize()
    {
        CheckDisposed();
        return LibResvg.resvg_get_image_size(_handle);
    }

    public bool TryGetObjectBBox(out LibResvg.ResvgRect bbox)
    {
        CheckDisposed();
        return LibResvg.resvg_get_object_bbox(_handle, out bbox);
    }

    public bool TryGetImageBBox(out LibResvg.ResvgRect bbox)
    {
        CheckDisposed();
        return LibResvg.resvg_get_image_bbox(_handle, out bbox);
    }

    public bool NodeExists(string id)
    {
        CheckDisposed();
        return LibResvg.resvg_node_exists(_handle, id);
    }

    public bool TryGetNodeTransform(string id, out LibResvg.ResvgTransform transform)
    {
        CheckDisposed();
        return LibResvg.resvg_get_node_transform(_handle, id, out transform);
    }

    public bool TryGetNodeBBox(string id, out LibResvg.ResvgRect bbox)
    {
        CheckDisposed();
        return LibResvg.resvg_get_node_bbox(_handle, id, out bbox);
    }

    public bool TryGetNodeStrokeBBox(string id, out LibResvg.ResvgRect bbox)
    {
        CheckDisposed();
        return LibResvg.resvg_get_node_stroke_bbox(_handle, id, out bbox);
    }

    public void Render(LibResvg.ResvgTransform transform, uint width, uint height, byte[] pixmap)
    {
        CheckDisposed();
        if (pixmap.Length < width * height * 4)
            throw new ArgumentException("Pixmap array is too small. Must be at least width*height*4 bytes.");

        LibResvg.resvg_render(_handle, transform, width, height, pixmap);
    }

    public void Render(uint width, uint height, byte[] pixmap)
    {
        Render(LibResvg.resvg_transform_identity(), width, height, pixmap);
    }

    public bool RenderNode(string id, LibResvg.ResvgTransform transform, uint width, uint height, byte[] pixmap)
    {
        CheckDisposed();
        if (pixmap.Length < width * height * 4)
            throw new ArgumentException("Pixmap array is too small. Must be at least width*height*4 bytes.");

        return LibResvg.resvg_render_node(_handle, id, transform, width, height, pixmap);
    }

    public bool RenderNode(string id, uint width, uint height, byte[] pixmap)
    {
        return RenderNode(id, LibResvg.resvg_transform_identity(), width, height, pixmap);
    }

    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResvgRenderTree));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != nint.Zero)
            {
                LibResvg.resvg_tree_destroy(_handle);
                _handle = nint.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ResvgRenderTree()
    {
        Dispose();
    }
}


