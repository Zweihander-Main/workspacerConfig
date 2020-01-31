#r "C:\Program Files\workspacer\workspacer.Shared.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.Bar\workspacer.Bar.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.ActionMenu\workspacer.ActionMenu.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.FocusIndicator\workspacer.FocusIndicator.dll"


using System;
using System.Runtime.InteropServices;
using workspacer;
using workspacer.Bar;
using workspacer.ActionMenu;
using workspacer.FocusIndicator;
using workspacer.Bar.Widgets;

// https://stackoverflow.com/questions/19022789/hide-taskbar-in-winforms-application#19024531
public class Taskbar
{
    [DllImport("user32.dll")]
    private static extern int FindWindow(string className, string windowText);

    [DllImport("user32.dll")]
    private static extern int ShowWindow(int hwnd, int command);

    [DllImport("user32.dll")]
    public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

    [DllImport("user32.dll")]
    private static extern int GetDesktopWindow();

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 1;

    private static int currentState = 0;

    protected static int Handle
    {
        get
        {
            return FindWindow("Shell_TrayWnd", "");
        }
    }

    protected static int HandleOfStartButton
    {
        get
        {
            int handleOfDesktop = GetDesktopWindow();
            int handleOfStartButton = FindWindowEx(handleOfDesktop, 0, "button", 0);
            return handleOfStartButton;
        }
    }

    private Taskbar()
    {
        // hide ctor
    }

    public static void Show()
    {
        ShowWindow(Handle, SW_SHOW);
        ShowWindow(HandleOfStartButton, SW_SHOW);
    }

    public static void Hide()
    {
        ShowWindow(Handle, SW_HIDE);
        ShowWindow(HandleOfStartButton, SW_HIDE);
    }

    public static void Toggle()
    {
        if (currentState == 0)
        {
            Hide();
            currentState = 1;
        }
        else
        {
            Show();
            currentState = 0;
        }
    }
}



Action<IConfigContext> doConfig = (context) =>
{
    var monitors = context.MonitorContainer.GetAllMonitors();

    var titleWidget = new TitleWidget();
    titleWidget.MonitorHasFocusColor = Color.Red;

    context.AddBar(new BarPluginConfig()
    {
        BarTitle = "workspacer.Bar",
        BarHeight = 18,
        FontSize = 10,
        DefaultWidgetForeground = Color.White,
        DefaultWidgetBackground = Color.Black,
        Background = Color.Black,
        LeftWidgets = () => new IBarWidget[] { new WorkspaceWidget(),  new ActiveLayoutWidget(), new TextWidget("    "), titleWidget },
        RightWidgets = () => new IBarWidget[] { new TimeWidget(1000, "ddd, M/dd/yyyy | h:mm tt") },
    });
    context.AddFocusIndicator();
    var actionMenu = context.AddActionMenu();

    context.DefaultLayouts = () => new ILayoutEngine[] { new FullLayoutEngine(), new TallLayoutEngine() };

    var sticky = new StickyWorkspaceContainer(context, StickyWorkspaceIndexMode.Local);
    context.WorkspaceContainer = sticky;
    sticky.CreateWorkspaces(monitors[0], "5|main", "5|code", "5|learn I", "5|learn II", "5|media", "5|chat");
    if (monitors.Length > 1)
    {
        sticky.CreateWorkspaces(monitors[1], "6|main", "6|code", "6|learn I", "6|learn II", "6|media", "6|chat");
    }
    if (monitors.Length > 2)
    {
        sticky.CreateWorkspaces(monitors[2], "8|main", "8|code", "8|learn I", "8|learn II", "8|media", "8|chat");
    }
    if (monitors.Length > 3)
    {
        sticky.CreateWorkspaces(monitors[3], "4|main", "4|code", "4|learn I", "4|learn II", "4|media", "4|chat");
    }

    // Ignore Program Filters
    context.WindowRouter.AddFilter((window) => !window.Title.Equals("Wox"));
    context.WindowRouter.AddFilter((window) => !window.Title.Equals("Everything"));
    context.WindowRouter.AddFilter((window) => !window.Title.Equals("Cmder"));
    context.WindowRouter.AddFilter((window) => !window.Class.Equals("ApplicationFrameWindow"));
    context.WindowRouter.AddFilter((window) => !window.Title.Equals("MasterStartupHotkeys.ahk"));
    context.WindowRouter.AddFilter((window) => !window.Class.Equals("#32770")); // Deletion dialog
    context.WindowRouter.AddFilter((window) => !window.Class.Equals("OperationStatusWindow")); // Copying dialog
    context.WindowRouter.AddFilter((window) => !window.ProcessName.Equals("pinentry")); // Yubikey GPG 
    
    
    // Router Program Filters
    context.WindowRouter.AddRoute((window) => window.ProcessName.Equals("thunderbird") ? context.WorkspaceContainer["4|media"] : null);
    context.WindowRouter.AddRoute((window) => window.ProcessName.Equals("hexchat") ? context.WorkspaceContainer["4|chat"] : null);

    // show keybinds
    // override win keybindds
    // Router program filters doesn't work if program already exists

    // Keybindings
    context.Keybinds.UnsubscribeAll();

    // context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LShift, Keys.F,
    //     () => context.Enabled = !context.Enabled, "toggle enable/disable");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.C,
        () => context.Workspaces.FocusedWorkspace.CloseFocusedWindow(), "close focused window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.T,
        () => context.Workspaces.FocusedWorkspace.NextLayoutEngine(), "next layout");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.T,
        () => context.Workspaces.FocusedWorkspace.PreviousLayoutEngine(), "previous layout");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LControl, Keys.Back,
        () => context.Workspaces.FocusedWorkspace.ResetLayout(), "reset layout");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.Down,
        () => context.Workspaces.FocusedWorkspace.FocusNextWindow(), "focus next window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.Up,
        () => context.Workspaces.FocusedWorkspace.FocusPreviousWindow(), "focus previous window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.M,
        () => context.Workspaces.FocusedWorkspace.FocusPrimaryWindow(), "focus primary window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.Enter,
        () => context.Workspaces.FocusedWorkspace.SwapFocusAndPrimaryWindow(), "swap focus and primary window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.Down,
        () => context.Workspaces.FocusedWorkspace.SwapFocusAndNextWindow(), "swap focus and next window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.Up,
        () => context.Workspaces.FocusedWorkspace.SwapFocusAndPreviousWindow(), "swap focus and previous window");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.Left,
        () => context.Workspaces.FocusedWorkspace.ShrinkPrimaryArea(), "shrink primary area");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.Right,
        () => context.Workspaces.FocusedWorkspace.ExpandPrimaryArea(), "expand primary area");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LAlt, Keys.OemCloseBrackets,
        () => context.Workspaces.FocusedWorkspace.IncrementNumberOfPrimaryWindows(), "increment # primary windows");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LAlt, Keys.OemOpenBrackets,
        () => context.Workspaces.FocusedWorkspace.DecrementNumberOfPrimaryWindows(), "decrement # primary windows");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.F,
        () => context.Windows.ToggleFocusedWindowTiling(), "toggle tiling for focused window");

    context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LAlt, Keys.Q, context.Quit, "quit workspacer");

    context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LAlt, Keys.R, context.Restart, "restart workspacer");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D1,
        () => context.Workspaces.SwitchToWorkspace(0), "switch to workspace 1");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D2,
        () => context.Workspaces.SwitchToWorkspace(1), "switch to workspace 2");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D3,
        () => context.Workspaces.SwitchToWorkspace(2), "switch to workspace 3");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D4,
        () => context.Workspaces.SwitchToWorkspace(3), "switch to workspace 4");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D5,
        () => context.Workspaces.SwitchToWorkspace(4), "switch to workspace 5");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D6,
        () => context.Workspaces.SwitchToWorkspace(5), "switch to workspace 6");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D7,
        () => context.Workspaces.SwitchToWorkspace(6), "switch to workspace 7");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D8,
        () => context.Workspaces.SwitchToWorkspace(7), "switch to workspace 8");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.D9,
        () => context.Workspaces.SwitchToWorkspace(8), "switch to workpsace 9");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.Left,
        () => context.Workspaces.SwitchToPreviousWorkspace(), "switch to previous workspace");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.Right,
        () => context.Workspaces.SwitchToNextWorkspace(), "switch to next workspace");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.NumPad4,
        () => context.Workspaces.SwitchFocusedMonitor(3), "focus monitor 1");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.NumPad5,
        () => context.Workspaces.SwitchFocusedMonitor(0), "focus monitor 2");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.NumPad6,
        () => context.Workspaces.SwitchFocusedMonitor(1), "☻focus monitor 3");

    context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.NumPad8,
        () => context.Workspaces.SwitchFocusedMonitor(2), "focus monitor 4");

    context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LAlt, Keys.NumPad4,
        () => context.Workspaces.MoveFocusedWindowToMonitor(3), "move focused window to monitor 1");

    context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LAlt, Keys.NumPad5,
        () => context.Workspaces.MoveFocusedWindowToMonitor(0), "move focused window to monitor 2");

    context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LAlt, Keys.NumPad6,
        () => context.Workspaces.MoveFocusedWindowToMonitor(1), "move focused window to monitor 3");

    context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LAlt, Keys.NumPad8,
        () => context.Workspaces.MoveFocusedWindowToMonitor(2), "move focused window to monitor 4");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D1,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(0), "switch focused window to workspace 1");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D2,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(1), "switch focused window to workspace 2");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D3,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(2), "switch focused window to workspace 3");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D4,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(3), "switch focused window to workspace 4");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D5,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(4), "switch focused window to workspace 5");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D6,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(5), "switch focused window to workspace 6");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D7,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(6), "switch focused window to workspace 7");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D8,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(7), "switch focused window to workspace 8");

    context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.D9,
        () => context.Workspaces.MoveFocusedWindowToWorkspace(8), "switch focused window to workspace 9");

    // context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LShift, Keys.I,
    //     () => context.Windows.DumpWindowDebugOutput(), "dump debug info to console for all windows");

    // context.Keybinds.Subscribe(KeyModifiers.LAlt, Keys.I,
    //     () => context.Windows.DumpWindowUnderCursorDebugOutput(), "dump debug info to console for window under cursor");

    // context.Keybinds.Subscribe(KeyModifiers.LAlt | KeyModifiers.LWin | KeyModifiers.LShift, Keys.I,
    //     () => context.ToggleConsoleWindow(), "toggle debug console");

    // context.Keybinds.Subscribe(KeyModifiers.LWin | KeyModifiers.LShift, Keys.Oem2,
    //     () => context.Keybinds.ShowKeybindDialog(), "open keybind window");

    context.Keybinds.Subscribe(KeyModifiers.LWin, Keys.Space,
        () => Taskbar.Toggle(), "hide/show win taskbar");

};
return doConfig;
