// ReSharper disable StyleCop.SA1310
namespace Net7MultiClientUnlocker.Framework.Win32
{
    public static class Win32Constants
    {
        public const uint WM_CHAR = 0x012;

        public const int MaxPath = 260;

        public const uint StatusInfoLengthMismatch = 0xC0000004;

        public const int DuplicateSameAccess = 0x2;

        public const int DuplicateCloseSource = 0x1;

        public const uint FlashWindowStop = 0;

        public const uint FlashWindowCaption = 1;

        public const uint FlashWindowTray = 2;

        public const uint FlashWindowAll = 3; // FLASHW_CAPTION | FLASHW_TRAY flags.

        public const uint FlashwTimer = 4;

        public const uint FlawhWindowTimerNoForeground = 12;

        public const uint SWP_NOSIZE = 0x0001;

        public const uint SWP_NOZORDER = 0x0004;

        public const uint SWP_FRAMECHANGED = 0x0020;

        public const uint SWP_NOACTIVATE = 0x0010;

        public const uint SWP_NOMOVE = 0x0002;

        public const uint SWP_NOREPOSITION = 0x0200;

        public const uint SWP_NOREDRAW = 0x008;

        public const uint BM_CLICK = 0x00F5;
        
        public const uint WM_SETTEXT = 0x000c;
        
        public const uint WM_KEYDOWN = 0x0100;
        
        public const uint WS_THICKFRAME = 0x40000;
        
        public const uint WS_CLIPCHILDREN = 0x2000000;
        
        public const uint WS_CAPTION = 0x00C00000;
        
        public const uint WS_VISIBLE = 0x10000000;
        
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        
        public const uint WS_SYSMENU = 0x00080000;
        
        public const uint WS_OVERLAPPED = 0x00000000;
        
        public const uint WS_MINIMIZEBOX = 0x00020000;
        
        public const uint WS_MAXIMIZEBOX = 0x00010000;

        public const uint WS_MAXIMIZE = 0x01000000;

        public const uint WS_EX_LEFT = 0x00000000;
        
        public const uint WS_EX_LTRREADING = 0x00000000;
        
        public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;
        
        public const uint WS_EX_WINDOWEDGE = 0x00000100;
        
        public const uint WS_EX_CONTROLPARENT = 0x00010000;
        
        public const uint WS_EX_APPWINDOW = 0x00040000;
        
        public const int GWL_STYLE = -16;

        public const long DS_3DLOOK = 0x4L;

        public const long DS_MODALFRAME = 0x80L;

        // Virtual Keys
        public const int VK_LBUTTON = 0x01; // Left mouse button

        public const int VK_RBUTTON = 0x02; // Right mouse button

        public const int VK_CANCEL = 0x03; // Control-break processing

        public const int VK_MBUTTON = 0x04; // Middle mouse button (three-button mouse)

        public const int VK_XBUTTON1 = 0x05; // X1 mouse button

        public const int VK_XBUTTON2 = 0x06; // X2 mouse button

        public const int VK_BACK = 0x08; // BACKSPACE key

        public const int VK_TAB = 0x09; // TAB key

        public const int VK_CLEAR = 0x0C; // CLEAR key

        public const int VK_RETURN = 0x0D; // ENTER key

        public const int VK_SHIFT = 0x10; // SHIFT key

        public const int VK_CONTROL = 0x11; // CTRL key

        public const int VK_MENU = 0x12; // ALT key

        public const int VK_PAUSE = 0x13; // PAUSE key

        public const int VK_CAPITAL = 0x14; // CAPS LOCK key

        public const int VK_KANA = 0x15; // IME Kana mode

        public const int VK_HANGUEL = 0x15; // IME Hanguel mode (maintained for compatibility; use VK_HANGUL)

        public const int VK_HANGUL = 0x15; // IME Hangul mode

        public const int VK_JUNJA = 0x17; // IME Junja mode

        public const int VK_FINAL = 0x18; // IME final mode

        public const int VK_HANJA = 0x19; // IME Hanja mode

        public const int VK_KANJI = 0x19; // IME Kanji mode

        public const int VK_ESCAPE = 0x1B; // ESC key

        public const int VK_CONVERT = 0x1C; // IME convert

        public const int VK_NONCONVERT = 0x1D; // IME nonconvert

        public const int VK_ACCEPT = 0x1E; // IME accept

        public const int VK_MODECHANGE = 0x1F; // IME mode change request

        public const int VK_SPACE = 0x20; // SPACEBAR

        public const int VK_PRIOR = 0x21; // PAGE UP key

        public const int VK_NEXT = 0x22; // PAGE DOWN key

        public const int VK_END = 0x23; // END key

        public const int VK_HOME = 0x24; // HOME key

        public const int VK_LEFT = 0x25; // LEFT ARROW key

        public const int VK_UP = 0x26; // UP ARROW key

        public const int VK_RIGHT = 0x27; // RIGHT ARROW key

        public const int VK_DOWN = 0x28; // DOWN ARROW key

        public const int VK_SELECT = 0x29; // SELECT key

        public const int VK_PRINT = 0x2A; // PRINT key

        public const int VK_EXECUTE = 0x2B; // EXECUTE key

        public const int VK_SNAPSHOT = 0x2C; // PRINT SCREEN key

        public const int VK_INSERT = 0x2D; // INS key

        public const int VK_DELETE = 0x2E; // DEL key

        public const int VK_HELP = 0x2F; // HELP key

        public const int VK_Key0 = 0x30; // 0 key

        public const int VK_Key1 = 0x31; // 1 key

        public const int VK_Key2 = 0x32; // 2 key

        public const int VK_Key3 = 0x33; // 3 key

        public const int VK_Key4 = 0x34; // 4 key

        public const int VK_Key5 = 0x35; // 5 key

        public const int VK_Key6 = 0x36; // 6 key

        public const int VK_Key7 = 0x37; // 7 key

        public const int VK_Key8 = 0x38; // 8 key

        public const int VK_Key9 = 0x39; // 9 key

        public const int VK_KeyA = 0x41; // A key

        public const int VK_KeyB = 0x42; // B key

        public const int VK_KeyC = 0x43; // C key

        public const int VK_KeyD = 0x44; // D key

        public const int VK_KeyE = 0x45; // E key

        public const int VK_KeyF = 0x46; // F key

        public const int VK_KeyG = 0x47; // G key

        public const int VK_KeyH = 0x48; // H key

        public const int VK_KeyI = 0x49; // I key

        public const int VK_KeyJ = 0x4A; // J key

        public const int VK_KeyK = 0x4B; // K key

        public const int VK_KeyL = 0x4C; // L key

        public const int VK_KeyM = 0x4D; // M key

        public const int VK_KeyN = 0x4E; // N key

        public const int VK_KeyO = 0x4F; // O key

        public const int VK_KeyP = 0x50; // P key

        public const int VK_KeyQ = 0x51; // Q key

        public const int VK_KeyR = 0x52; // R key

        public const int VK_KeyS = 0x53; // S key

        public const int VK_KeyT = 0x54; // T key

        public const int VK_KeyU = 0x55; // U key

        public const int VK_KeyV = 0x56; // V key

        public const int VK_KeyW = 0x57; // W key

        public const int VK_KeyX = 0x58; // X key

        public const int VK_KeyY = 0x59; // Y key

        public const int VK_KeyZ = 0x5A; // Z key

        public const int VK_LWIN = 0x5B; // Left Windows key (Natural keyboard)

        public const int VK_RWIN = 0x5C; // Right Windows key (Natural keyboard)

        public const int VK_APPS = 0x5D; // Applications key (Natural keyboard)

        public const int VK_SLEEP = 0x5F; // Computer Sleep key

        public const int VK_NUMPAD0 = 0x60; // Numeric keypad 0 key

        public const int VK_NUMPAD1 = 0x61; // Numeric keypad 1 key

        public const int VK_NUMPAD2 = 0x62; // Numeric keypad 2 key

        public const int VK_NUMPAD3 = 0x63; // Numeric keypad 3 key

        public const int VK_NUMPAD4 = 0x64; // Numeric keypad 4 key

        public const int VK_NUMPAD5 = 0x65; // Numeric keypad 5 key

        public const int VK_NUMPAD6 = 0x66; // Numeric keypad 6 key

        public const int VK_NUMPAD7 = 0x67; // Numeric keypad 7 key

        public const int VK_NUMPAD8 = 0x68; // Numeric keypad 8 key

        public const int VK_NUMPAD9 = 0x69; // Numeric keypad 9 key

        public const int VK_MULTIPLY = 0x6A; // Multiply key

        public const int VK_ADD = 0x6B; // Add key

        public const int VK_SEPARATOR = 0x6C; // Separator key

        public const int VK_SUBTRACT = 0x6D; // Subtract key

        public const int VK_DECIMAL = 0x6E; // Decimal key

        public const int VK_DIVIDE = 0x6F; // Divide key

        public const int VK_F1 = 0x70; // F1 key

        public const int VK_F2 = 0x71; // F2 key

        public const int VK_F3 = 0x72; // F3 key

        public const int VK_F4 = 0x73; // F4 key

        public const int VK_F5 = 0x74; // F5 key

        public const int VK_F6 = 0x75; // F6 key

        public const int VK_F7 = 0x76; // F7 key

        public const int VK_F8 = 0x77; // F8 key

        public const int VK_F9 = 0x78; // F9 key

        public const int VK_F10 = 0x79; // F10 key

        public const int VK_F11 = 0x7A; // F11 key

        public const int VK_F12 = 0x7B; // F12 key

        public const int VK_F13 = 0x7C; // F13 key

        public const int VK_F14 = 0x7D; // F14 key

        public const int VK_F15 = 0x7E; // F15 key

        public const int VK_F16 = 0x7F; // F16 key

        public const int VK_F17 = 0x80; // F17 key

        public const int VK_F18 = 0x81; // F18 key

        public const int VK_F19 = 0x82; // F19 key

        public const int VK_F20 = 0x83; // F20 key

        public const int VK_F21 = 0x84; // F21 key

        public const int VK_F22 = 0x85; // F22 key

        public const int VK_F23 = 0x86; // F23 key

        public const int VK_F24 = 0x87; // F24 key

        public const int VK_NUMLOCK = 0x90; // NUM LOCK key

        public const int VK_SCROLL = 0x91; // SCROLL LOCK key

        public const int VK_LSHIFT = 0xA0; // Left SHIFT key

        public const int VK_RSHIFT = 0xA1; // Right SHIFT key

        public const int VK_LCONTROL = 0xA2; // Left CONTROL key

        public const int VK_RCONTROL = 0xA3; // Right CONTROL key

        public const int VK_LMENU = 0xA4; // Left MENU key

        public const int VK_RMENU = 0xA5; // Right MENU key

        public const int VK_BROWSER_BACK = 0xA6; // Browser Back key

        public const int VK_BROWSER_FORWARD = 0xA7; // Browser Forward key

        public const int VK_BROWSER_REFRESH = 0xA8; // Browser Refresh key

        public const int VK_BROWSER_STOP = 0xA9; // Browser Stop key

        public const int VK_BROWSER_SEARCH = 0xAA; // Browser Search key

        public const int VK_BROWSER_FAVORITES = 0xAB; // Browser Favorites key

        public const int VK_BROWSER_HOME = 0xAC; // Browser Start and Home key

        public const int VK_VOLUME_MUTE = 0xAD; // Volume Mute key

        public const int VK_VOLUME_DOWN = 0xAE; // Volume Down key

        public const int VK_VOLUME_UP = 0xAF; // Volume Up key

        public const int VK_MEDIA_NEXT_TRACK = 0xB0; // Next Track key

        public const int VK_MEDIA_PREV_TRACK = 0xB1; // Previous Track key

        public const int VK_MEDIA_STOP = 0xB2; // Stop Media key

        public const int VK_MEDIA_PLAY_PAUSE = 0xB3; // Play/Pause Media key

        public const int VK_LAUNCH_MAIL = 0xB4; // Start Mail key

        public const int VK_LAUNCH_MEDIA_SELECT = 0xB5; // Select Media key

        public const int VK_LAUNCH_APP1 = 0xB6; // Start Application 1 key

        public const int VK_LAUNCH_APP2 = 0xB7; // Start Application 2 key

        public const int VK_OEM_1 = 0xBA; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the ';:' key

        public const int VK_OEM_PLUS = 0xBB; // For any country/region; the '+' key

        public const int VK_OEM_COMMA = 0xBC; // For any country/region; the ';' key

        public const int VK_OEM_MINUS = 0xBD; // For any country/region; the '-' key

        public const int VK_OEM_PERIOD = 0xBE; // For any country/region; the '.' key

        public const int VK_OEM_2 = 0xBF; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the '/?' key

        public const int VK_OEM_3 = 0xC0; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the '`~' key

        public const int VK_OEM_4 = 0xDB; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the '[{' key

        public const int VK_OEM_5 = 0xDC; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the '\|' key

        public const int VK_OEM_6 = 0xDD; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the ']}' key

        public const int VK_OEM_7 = 0xDE; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard; the 'single-quote/double-quote' key 

        public const int VK_OEM_8 = 0xDF; // Used for miscellaneous characters; it can vary by keyboard.

        public const int VK_OEM_102 = 0xE2; // Either the angle bracket key or the backslash key on the RT 102-key keyboard

        public const int VK_PROCESSKEY = 0xE5; // IME PROCESS key

        public const int VK_PACKET = 0xE7; // Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information; see Remark in KEYBDINPUT; SendInput; WM_KEYDOWN; and WM_KEYUP

        public const int VK_ATTN = 0xF6; // Attn key

        public const int VK_CRSEL = 0xF7; // CrSel key

        public const int VK_EXSEL = 0xF8; // ExSel key

        public const int VK_EREOF = 0xF9; // Erase EOF key

        public const int VK_PLAY = 0xFA; // Play key

        public const int VK_ZOOM = 0xFB; // Zoom key

        public const int VK_NONAME = 0xFC; // Reserved

        public const int VK_PA1 = 0xFD; // PA1 key

        public const int VK_OEM_CLEAR = 0xFE; // Clear key
    }
}